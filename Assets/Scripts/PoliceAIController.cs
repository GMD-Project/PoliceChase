// File: Assets/Scripts/PoliceAIController.cs
using UnityEngine;

public class PoliceAIController : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Forward Chase")]
    public float forwardSpeed = 20f;
    public float turnSpeed = 220f;
    public float predictionTime = 0.9f;
    public float nearTargetRadius = 7f;

    [Header("Reverse Recovery")]
    public float reverseSpeed = 12f;
    public float reverseTurnSpeedMultiplier = 1.4f;
    public float reverseDuration = 1.4f;
    public float turnInPlaceDuration = 0.5f;

    [Header("Stuck Detection")]
    public float stuckSpeedThreshold = 1.2f;
    public float stuckTime = 0.7f;

    [Header("Obstacle Avoidance")]
    public float sensorLength = 10f;
    public float sideSensorAngle = 30f;
    public float sensorSideOffset = 1.2f;
    public float avoidanceWeight = 1.5f;
    public LayerMask obstacleMask = ~0;

    private Rigidbody rb;
    private Transform carTransform;
    private Rigidbody targetRb;

    private float stuckTimer;
    private float recoveryTimer;
    private float recoveryTurnDirection;

    private enum AiState
    {
        Chase,
        ReverseRecover,
        TurnInPlaceRecover
    }

    private AiState state = AiState.Chase;

    void Start()
    {
        rb = GetComponentInParent<Rigidbody>();

        if (rb == null)
        {
            Debug.LogError("PoliceAIController needs a Rigidbody on this object or its parent.");
            enabled = false;
            return;
        }

        carTransform = rb.transform;

        rb.constraints = RigidbodyConstraints.FreezePositionY
                       | RigidbodyConstraints.FreezeRotationX
                       | RigidbodyConstraints.FreezeRotationZ;

        if (target != null)
            targetRb = target.GetComponentInParent<Rigidbody>();
    }

    void FixedUpdate()
    {
        if (rb == null || target == null) return;

        if (targetRb == null)
            targetRb = target.GetComponentInParent<Rigidbody>();

        switch (state)
        {
            case AiState.Chase:
                HandleChase();
                break;

            case AiState.ReverseRecover:
                HandleReverseRecover();
                break;

            case AiState.TurnInPlaceRecover:
                HandleTurnInPlaceRecover();
                break;
        }
    }

    void HandleChase()
    {
        Vector3 chasePoint = GetChasePoint();
        float pursuitSteer = GetPursuitSteer(chasePoint);
        float avoidSteer = GetAvoidanceSteer();
        float finalSteer = Mathf.Clamp(pursuitSteer + avoidSteer, -1f, 1f);

        rb.MoveRotation(
            rb.rotation * Quaternion.Euler(0f, finalSteer * turnSpeed * Time.fixedDeltaTime, 0f)
        );

        rb.linearVelocity = carTransform.forward * forwardSpeed;

        CheckIfStuck();
    }

    void HandleReverseRecover()
    {
        recoveryTimer -= Time.fixedDeltaTime;

        rb.MoveRotation(
            rb.rotation * Quaternion.Euler(
                0f,
                recoveryTurnDirection * turnSpeed * reverseTurnSpeedMultiplier * Time.fixedDeltaTime,
                0f
            )
        );

        rb.linearVelocity = -carTransform.forward * reverseSpeed;

        if (recoveryTimer <= 0f)
        {
            state = AiState.TurnInPlaceRecover;
            recoveryTimer = turnInPlaceDuration;
        }
    }

    void HandleTurnInPlaceRecover()
    {
        recoveryTimer -= Time.fixedDeltaTime;

        rb.MoveRotation(
            rb.rotation * Quaternion.Euler(
                0f,
                recoveryTurnDirection * turnSpeed * reverseTurnSpeedMultiplier * Time.fixedDeltaTime,
                0f
            )
        );

        rb.linearVelocity = Vector3.zero;

        if (recoveryTimer <= 0f)
        {
            state = AiState.Chase;
            stuckTimer = 0f;
            rb.angularVelocity = Vector3.zero;
        }
    }

    void CheckIfStuck()
    {
        Vector3 flatVelocity = rb.linearVelocity;
        flatVelocity.y = 0f;

        if (flatVelocity.magnitude < stuckSpeedThreshold)
        {
            stuckTimer += Time.fixedDeltaTime;

            if (stuckTimer >= stuckTime)
                StartRecovery();
        }
        else
        {
            stuckTimer = 0f;
        }
    }

    void StartRecovery()
    {
        state = AiState.ReverseRecover;
        recoveryTimer = reverseDuration;
        stuckTimer = 0f;
        recoveryTurnDirection = Random.value < 0.5f ? -1f : 1f;
    }

    Vector3 GetChasePoint()
    {
        Vector3 targetPosition = target.position;
        Vector3 targetVelocity = Vector3.zero;

        if (targetRb != null)
        {
            targetVelocity = targetRb.linearVelocity;
            targetVelocity.y = 0f;
        }

        Vector3 predicted = targetPosition + targetVelocity * predictionTime;
        predicted.y = carTransform.position.y;

        float distance = Vector3.Distance(
            new Vector3(carTransform.position.x, 0f, carTransform.position.z),
            new Vector3(targetPosition.x, 0f, targetPosition.z)
        );

        if (distance < nearTargetRadius)
        {
            predicted = targetPosition + target.forward * 4f;
            predicted.y = carTransform.position.y;
        }

        return predicted;
    }

    float GetPursuitSteer(Vector3 chasePoint)
    {
        Vector3 toPoint = chasePoint - carTransform.position;
        toPoint.y = 0f;

        if (toPoint.sqrMagnitude < 0.01f) return 0f;

        Vector3 desiredDirection = toPoint.normalized;
        float angle = Vector3.SignedAngle(carTransform.forward, desiredDirection, Vector3.up);
        return Mathf.Clamp(angle / 45f, -1f, 1f);
    }

    float GetAvoidanceSteer()
    {
        Vector3 originCenter = carTransform.position + carTransform.forward * 1.5f + Vector3.up * 0.5f;
        Vector3 originLeft = originCenter - carTransform.right * sensorSideOffset;
        Vector3 originRight = originCenter + carTransform.right * sensorSideOffset;

        float steer = 0f;

        if (Physics.Raycast(originCenter, carTransform.forward, out RaycastHit centerHit, sensorLength, obstacleMask, QueryTriggerInteraction.Ignore))
        {
            steer += Vector3.Dot(centerHit.normal, carTransform.right) >= 0f ? -1f : 1f;
        }

        Vector3 leftDir = Quaternion.AngleAxis(-sideSensorAngle, Vector3.up) * carTransform.forward;
        if (Physics.Raycast(originLeft, leftDir, out _, sensorLength * 0.9f, obstacleMask, QueryTriggerInteraction.Ignore))
        {
            steer += 1f;
        }

        Vector3 rightDir = Quaternion.AngleAxis(sideSensorAngle, Vector3.up) * carTransform.forward;
        if (Physics.Raycast(originRight, rightDir, out _, sensorLength * 0.9f, obstacleMask, QueryTriggerInteraction.Ignore))
        {
            steer -= 1f;
        }

        return Mathf.Clamp(steer * avoidanceWeight, -1f, 1f);
    }
}
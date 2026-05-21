
using UnityEngine;
using UnityEngine.AI;

public class PoliceAIController : MonoBehaviour
{
    public enum State { Chasing, Recovering, Caught }

    [Header("Target")]
    public Transform target;

    [Header("Chase")]
    public float forwardSpeed = 15f;
    public float turnSpeed = 220f;
    public float minSpeedFactor = 0.4f;
    public float pathLookahead = 15f;
    public float reverseSpeed = 8f;

    [Header("Stuck Recovery")]
    public float stuckSpeedThreshold = 0.5f;
    public float stuckTime = 1.5f;
    public float recoveryDuration = 0.8f;

    private Rigidbody rb;
    private Transform carTransform;
    private NavMeshAgent agent;
    private State currentState;
    private float stuckTimer;
    private float recoveryTimer;

    void Start()
    {
        rb = GetComponentInParent<Rigidbody>();

        if (rb == null) { enabled = false; return; }

        carTransform = rb.transform;

        rb.constraints = RigidbodyConstraints.FreezePositionY
                       | RigidbodyConstraints.FreezeRotationX
                       | RigidbodyConstraints.FreezeRotationZ;

        agent = GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.updatePosition = false;
            agent.updateRotation = false;
        }   
        EnterState(State.Chasing);
        }

    void FixedUpdate()
    {
        if (rb == null || target == null|| agent == null) return;

       agent.SetDestination(target.position);
        agent.nextPosition = carTransform.position;

       switch (currentState)
        {
            case State.Chasing:    HandleChase();    break;
            case State.Recovering: HandleRecovery(); break;
            case State.Caught:                       break;
        }

    }

    void HandleChase()
    {
        Vector3 chasePoint = GetChasePoint();
        float steer = GetSteer(chasePoint);

        rb.MoveRotation(
            rb.rotation * Quaternion.Euler(0f, steer * turnSpeed * Time.fixedDeltaTime, 0f)
        );
        float speedFactor = Mathf.Lerp(1f, minSpeedFactor, Mathf.Clamp01((Mathf.Abs(steer) - 0.3f) / 0.7f));
        rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, carTransform.forward * forwardSpeed * speedFactor, 12f * Time.fixedDeltaTime);  
        Vector3 flatVelocity = rb.linearVelocity;
        flatVelocity.y = 0f;
        if (flatVelocity.magnitude < stuckSpeedThreshold)
        {
            stuckTimer += Time.fixedDeltaTime;
            if (stuckTimer >= stuckTime)
                EnterState(State.Recovering);
        }
        else
        {
            stuckTimer = 0f;
        }
    }

    void HandleRecovery()
    {
        recoveryTimer -= Time.fixedDeltaTime;
        rb.linearVelocity = -carTransform.forward * reverseSpeed;

        if (recoveryTimer <= 0f)
        EnterState(State.Chasing);
    }

    Vector3 GetChasePoint()
    {
        if (!agent.hasPath)
        {
            Vector3 fallback = target.position;
            fallback.y = carTransform.position.y;
            return fallback;
        }

        Vector3[] corners = agent.path.corners;
        float remaining = pathLookahead;
        Vector3 current = carTransform.position;

        for (int i = 0; i < corners.Length; i++)
        {
            float segLen = Vector3.Distance(current, corners[i]);
            if (segLen >= remaining)
                return current + (corners[i] - current).normalized * remaining;

            remaining -= segLen;
            current = corners[i];
        }

        Vector3 last = corners[corners.Length - 1];
        last.y = carTransform.position.y;
        return last;
    }

    float GetSteer(Vector3 chasePoint)
    {
        Vector3 toPoint = chasePoint - carTransform.position;
        toPoint.y = 0f;
        if (toPoint.sqrMagnitude < 0.01f) return 0f;
        float angle = Vector3.SignedAngle(carTransform.forward, toPoint.normalized, Vector3.up);
        return Mathf.Clamp(angle / 60f, -1f, 1f);
    }
    void OnCollisionEnter(Collision collision)
    {
        if (target == null || currentState == State.Caught) return;
        Transform hit = collision.transform;
        if (hit == target || hit.IsChildOf(target))
        {
            EnterState(State.Caught);
            GameMenuManager.RaisePlayerCaught();
        }
    }
    void EnterState(State newState)
    {
        currentState = newState;

        switch (newState)
        {
            case State.Chasing:
                stuckTimer = 0f;
                agent.ResetPath();
                break;
            case State.Recovering:
                recoveryTimer = recoveryDuration;
                break;
            case State.Caught:
                rb.linearVelocity = Vector3.zero;
                break;
        }
    }
}
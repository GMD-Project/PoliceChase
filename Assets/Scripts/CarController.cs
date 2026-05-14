
using UnityEngine;
using UnityEngine.InputSystem;

public class CarController : MonoBehaviour
{
    public float speed = 15f;
    public float turnSpeed = 150f;
    public bool isPlayer2 = false;

    Rigidbody rb;
    private PlayerInputActions input;
    private Vector2 moveInputP1;
    private Vector2 moveInputP2;


    void Awake()
    {
        input = new PlayerInputActions();
    }

    void OnEnable()
    {
        if (isPlayer2) input.Player2.Enable();
        else input.Player.Enable();
    }

    void OnDisable()
    {
        if (isPlayer2) input.Player2.Disable();
        else input.Player.Disable();
    }
    void Start()
    {
        rb = GetComponentInParent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezePositionY
                   | RigidbodyConstraints.FreezeRotationX
                   | RigidbodyConstraints.FreezeRotationZ;
    }

    void Update()

    {
        if (isPlayer2)
        {
            moveInputP2 = input.Player2.Move.ReadValue<Vector2>();
        }
        else
        {
            moveInputP1 = input.Player.Move.ReadValue<Vector2>();
        }

    }

    void FixedUpdate()
    {
        Vector2 moveInput = isPlayer2 ? moveInputP2 : moveInputP1;
        float move = moveInput.y;
        float turn = moveInput.x;

        rb.linearVelocity = transform.forward * move * speed;

        if (Mathf.Abs(move) > 0.01f)
            rb.MoveRotation(rb.rotation * Quaternion.Euler(0, turn * turnSpeed * Time.fixedDeltaTime * move, 0));
        else
            rb.angularVelocity = Vector3.zero;
    }
}

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
        if (isPlayer2)
    {
        if (Gamepad.all.Count > 1)
            input.Player2.Get().devices = new[] { Gamepad.all[1] };
        input.Player2.Enable();
    }
    else
    {
        if (Gamepad.all.Count > 0)
            input.Player.Get().devices = new[] { Gamepad.all[0] };
        input.Player.Enable();
    }
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

        if (Mathf.Abs(turn) > 0.01f)
        {
            rb.MoveRotation(
                rb.rotation * Quaternion.Euler(0f, turn * turnSpeed * Time.fixedDeltaTime, 0f)
            );
        }
        else
        {
            rb.angularVelocity = Vector3.zero;
        }
    }
}
using UnityEngine;
using UnityEngine.InputSystem;

public class CarController : MonoBehaviour
{
    public float speed = 15f;
    public float turnSpeed = 150f;

    Rigidbody rb;
    private PlayerInputActions input;
    private Vector2 moveInput;


     void Awake()
    {
        input = new PlayerInputActions();
    }

    void OnEnable() => input.Player.Enable();
    void OnDisable() => input.Player.Disable();

    void Start()
    {
        rb = GetComponentInParent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezePositionY 
                   | RigidbodyConstraints.FreezeRotationX 
                   | RigidbodyConstraints.FreezeRotationZ;
    }

    void Update()
    {
        moveInput = input.Player.Move.ReadValue<Vector2>();
    }

    void FixedUpdate()
    {
        float move = moveInput.y;
        float turn = moveInput.x;

        rb.linearVelocity = transform.forward * move * speed;

        if (Mathf.Abs(move) > 0.01f)
            rb.MoveRotation(rb.rotation * Quaternion.Euler(0, turn * turnSpeed * Time.fixedDeltaTime * move, 0));
        else
            rb.angularVelocity = Vector3.zero; 
    }
}
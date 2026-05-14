using UnityEngine;
using UnityEngine.InputSystem;

public class CarController : MonoBehaviour
{
    public float speed = 15f;
    public float turnSpeed = 150f;
    public bool isPlayer2 = false;

    Rigidbody rb;
    private PlayerInputActions input;
    private Vector2 moveInput;


    void Awake()
    {
        if (!isPlayer2)
            input = new PlayerInputActions();
    }

    void OnEnable() {
       if (!isPlayer2) 
             input.Player.Enable();}
    void OnDisable()
    {
        if (!isPlayer2)
            input.Player.Disable();

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
            var kb = Keyboard.current;
            float x = (kb.dKey.isPressed ? 1f : 0f) - (kb.aKey.isPressed ? 1f : 0f);
            float y = (kb.wKey.isPressed ? 1f : 0f) - (kb.sKey.isPressed ? 1f : 0f);
            moveInput = new Vector2(x, y);
        }
        else
        {
            moveInput = input.Player.Move.ReadValue<Vector2>();
        }
      
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
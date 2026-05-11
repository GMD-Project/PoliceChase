using UnityEngine;

public class CarController : MonoBehaviour
{
    public float speed = 15f;
    public float turnSpeed = 90f;

    Rigidbody rb;

    void Start()
    {
        rb = GetComponentInParent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezePositionY 
                   | RigidbodyConstraints.FreezeRotationX 
                   | RigidbodyConstraints.FreezeRotationZ;
    }

    void FixedUpdate()
    {
        float move = Input.GetAxis("Vertical");
        float turn = Input.GetAxis("Horizontal");

        rb.MovePosition(rb.position + transform.forward * move * speed * Time.fixedDeltaTime);
        rb.MoveRotation(rb.rotation * Quaternion.Euler(0, turn * turnSpeed * Time.fixedDeltaTime * move, 0));
    }
}
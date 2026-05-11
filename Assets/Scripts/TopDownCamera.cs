using UnityEngine;

public class TopDownCamera : MonoBehaviour
{
    public Transform target;
    public float height = 50f;
    public float smoothSpeed = 5f;

    void Start()
    {
        CarController car = FindObjectOfType<CarController>();
        if (car != null) target = car.transform;
    }

    void LateUpdate()
    {
        if (target == null) return;
        Vector3 desired = new Vector3(target.position.x, target.position.y + height, target.position.z);
        transform.position = Vector3.Lerp(transform.position, desired, smoothSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Euler(90f, 0f, 0f);
    }
    
}


using UnityEngine;

public class MultiplayerCatchDetector : MonoBehaviour
{
    void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.root.name == "PlayerCar")
            GameMenuManager.RaisePlayerCaught();
    }
}
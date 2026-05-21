using UnityEngine;

public class ExitTrigger : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.transform.root.name != "PlayerCar") return;
            GameMenuManager.RaisePlayerEscaped();
        gameObject.SetActive(false);
    }
}
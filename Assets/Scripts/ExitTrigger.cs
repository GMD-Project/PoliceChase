using UnityEngine;

public class ExitTrigger : MonoBehaviour
{
    private GameMenuManager gameMenuManager;

    void Start()
    {
        gameMenuManager = FindObjectOfType<GameMenuManager>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.transform.root.name != "PlayerCar") return;
        gameMenuManager?.TriggerEscaped();
        gameObject.SetActive(false);
    }
}
using UnityEngine;

public class MultiplayerCatchDetector : MonoBehaviour
{
    private GameMenuManager gameMenuManager;

    void Start()
    {
        gameMenuManager = FindObjectOfType<GameMenuManager>();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (gameMenuManager == null) return;
        if (collision.transform.root.name == "PlayerCar")
            gameMenuManager.TriggerCaught();
    }
}
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameMenuManager : MonoBehaviour
{
    [Header("References")]
    public CityGenerator cityGenerator;

    [Header("Panels")]
    public GameObject mainMenuPanel;
    public GameObject inGameMenuPanel;
    public GameObject confirmPanel;

    [Header("Mode Buttons")]
    public GameObject singlePlayerButtonObject;
    public GameObject multiplayerButtonObject;

    [Header("Main Menu Buttons")]
    public GameObject startButtonObject;
    public GameObject mainExitButtonObject;

    [Header("Optional")]
    public string menuSceneName = "";

    private bool modeSelected = false;
    private string selectedMode = "";

    void Start()
    {
        Time.timeScale = 1f;

        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        if (inGameMenuPanel != null) inGameMenuPanel.SetActive(false);
        if (confirmPanel != null) confirmPanel.SetActive(false);

        if (singlePlayerButtonObject != null) singlePlayerButtonObject.SetActive(true);
        if (multiplayerButtonObject != null) multiplayerButtonObject.SetActive(true);

        if (startButtonObject != null) startButtonObject.SetActive(false);
        if (mainExitButtonObject != null) mainExitButtonObject.SetActive(false);
    }

    public void SelectSinglePlayer()
    {
        selectedMode = "Single Player";
        modeSelected = true;
        ShowStartMenuButtons();
        Debug.Log("Selected mode: " + selectedMode);
    }

    public void SelectMultiplayer()
    {
        selectedMode = "Multiplayer";
        modeSelected = true;
        ShowStartMenuButtons();
        Debug.Log("Selected mode: " + selectedMode);
    }

    public void StartGame()
    {
        Debug.Log("Menu StartGame pressed");

        if (!modeSelected)
        {
            Debug.LogWarning("Select a mode first.");
            return;
        }

        if (cityGenerator == null)
        {
            Debug.LogError("CityGenerator is not assigned.");
            return;
        }

        cityGenerator.StartGame();

        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (confirmPanel != null) confirmPanel.SetActive(false);
        if (inGameMenuPanel != null) inGameMenuPanel.SetActive(true);

        Time.timeScale = 1f;
    }

    public void PauseGame()
    {
        Time.timeScale = 0f;
        if (confirmPanel != null) confirmPanel.SetActive(true);
    }

    public void OpenExitConfirm()
    {
        Time.timeScale = 0f;
        if (confirmPanel != null) confirmPanel.SetActive(true);
    }

    public void ContinueGame()
    {
        Time.timeScale = 1f;
        if (confirmPanel != null) confirmPanel.SetActive(false);
    }

    public void ExitGame()
    {
        Time.timeScale = 1f;

        if (!string.IsNullOrEmpty(menuSceneName))
        {
            SceneManager.LoadScene(menuSceneName);
            return;
        }

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void ShowStartMenuButtons()
    {
        if (singlePlayerButtonObject != null) singlePlayerButtonObject.SetActive(false);
        if (multiplayerButtonObject != null) multiplayerButtonObject.SetActive(false);

        if (startButtonObject != null) startButtonObject.SetActive(true);
        if (mainExitButtonObject != null) mainExitButtonObject.SetActive(true);
    }
}
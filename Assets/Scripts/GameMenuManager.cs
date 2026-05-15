using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

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
    public GameObject backButtonObject;

    [Header("Optional")]
    public string menuSceneName = "";

    [Header("Navigation")]
    public Color highlightColor = Color.yellow;
    public Color normalColor = Color.white;

    private int currentIndex = 0;
    private bool onSecondScreen = false;
    private PlayerInputActions input;

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
        if (mainExitButtonObject != null) mainExitButtonObject.SetActive(true);

        currentIndex = 0;   
        UpdateHighlight(CurrentButtons());
    }
    private void OnNavigate(InputAction.CallbackContext ctx)
    {
        Vector2 value = ctx.ReadValue<Vector2>();
        GameObject[] buttons = CurrentButtons();

        if (value.y < -0.5f)
            currentIndex = (currentIndex + 1) % buttons.Length;
        else if (value.y > 0.5f)
            currentIndex = (currentIndex - 1 + buttons.Length) % buttons.Length;

        UpdateHighlight(buttons);
    }
    private void OnSelect(InputAction.CallbackContext ctx)
    {
        CurrentButtons()[currentIndex].GetComponent<UnityEngine.UI.Button>().onClick.Invoke();
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

        cityGenerator.gameMode = selectedMode;
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
        if (mainExitButtonObject != null) mainExitButtonObject.SetActive(false);
        if (startButtonObject != null) startButtonObject.SetActive(true);
        if (backButtonObject != null) backButtonObject.SetActive(true);

        onSecondScreen = true;
        currentIndex = 0;
        UpdateHighlight(CurrentButtons());
    }

    public void GoBack()
    {
        modeSelected = false;
        selectedMode = "";

        if (singlePlayerButtonObject != null) singlePlayerButtonObject.SetActive(true);
        if (multiplayerButtonObject != null) multiplayerButtonObject.SetActive(true);
        if (mainExitButtonObject != null) mainExitButtonObject.SetActive(true);

        if (startButtonObject != null) startButtonObject.SetActive(false);
        if (backButtonObject != null) backButtonObject.SetActive(false);
    
        onSecondScreen = false;
        currentIndex = 0;
        UpdateHighlight(CurrentButtons());
    }


    private GameObject[] CurrentButtons()
    {
        if (onSecondScreen)
            return new GameObject[] { startButtonObject, backButtonObject };
        else
            return new GameObject[] { singlePlayerButtonObject, multiplayerButtonObject, mainExitButtonObject };
    }

    private void UpdateHighlight(GameObject[] buttons)
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i] == null) continue;
            var img = buttons[i].GetComponent<UnityEngine.UI.Image>();
            if (img != null) img.color = (i == currentIndex) ? highlightColor : normalColor;
        }
    }

    void Awake()
    {
        input = new PlayerInputActions();
    }

    void OnEnable()
    {
        input.Menu.Enable();
        input.Menu.Navigate.performed += OnNavigate;
        input.Menu.Select.performed += OnSelect;
    }

    void OnDisable()
    {
        input.Menu.Navigate.performed -= OnNavigate;
        input.Menu.Select.performed -= OnSelect;
        input.Menu.Disable();
    }
}
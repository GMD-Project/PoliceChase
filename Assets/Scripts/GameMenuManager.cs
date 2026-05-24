using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using System.Collections;
using System;

public class GameMenuManager : MonoBehaviour
{
    public static event Action OnPlayerCaught;
    public static event Action OnPlayerEscaped;
    public static void RaisePlayerCaught()  => OnPlayerCaught?.Invoke();
    public static void RaisePlayerEscaped() => OnPlayerEscaped?.Invoke();

    [Header("References")]
    public CityGenerator cityGenerator;

    [Header("Panels")]
    public GameObject mainMenuPanel;
    public GameObject confirmPanel;

    [Header("Mode Buttons")]
    public GameObject singlePlayerButtonObject;
    public GameObject multiplayerButtonObject;

    [Header("Main Menu Buttons")]
    public GameObject startButtonObject;
    public GameObject mainExitButtonObject;
    public GameObject backButtonObject;

    [Header("Pause Menu Buttons")]
    public GameObject resumeButtonObject;
    public GameObject exitToMenuButtonObject;

    [Header("End Game Panels")]
    public GameObject caughtPanel;
    public GameObject escapedPanel;

    [Header("End Game Buttons")]
    public GameObject tryAgainButtonObject;
    public GameObject playAgainButtonObject;
    public GameObject caughtExitButtonObject;
    public GameObject escapedExitButtonObject;

    [Header("End Game Animations")]
    public RectTransform bustedText;

    [Header("Audio")]
    public AudioClip menuMusic;
    public AudioClip gameMusic;
    public AudioClip escapeMusic;
    public AudioClip caughtMusic;

    [Header("Optional")]
    public string menuSceneName = "";

    [Header("Navigation")]
    public Color highlightColor = Color.yellow;
    public Color normalColor = Color.white;

    private int currentIndex = 0;
    private bool onSecondScreen = false;
    private PlayerInputActions input;
    private AudioSource audioSource;
    private bool modeSelected = false;
    private string selectedMode = "";
    private bool gameStarted = false;
    private bool isPaused = false;
    private bool gameEnded = false;
    private Color _originalSunColor = Color.white;
    private float _originalSunIntensity = 1f;


    void Start()
    {
        Time.timeScale = 1f;
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.loop = true;
        audioSource.volume = 1f;
        PlayMusic(menuMusic);

        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        if (confirmPanel != null) confirmPanel.SetActive(false);

        if (singlePlayerButtonObject != null) singlePlayerButtonObject.SetActive(true);
        if (multiplayerButtonObject != null) multiplayerButtonObject.SetActive(true);

        if (startButtonObject != null) startButtonObject.SetActive(false);
        if (mainExitButtonObject != null) mainExitButtonObject.SetActive(true);

        currentIndex = 0;
        UpdateHighlight(CurrentButtons());
        if (caughtPanel != null) caughtPanel.SetActive(false);
        if (escapedPanel != null) escapedPanel.SetActive(false);

        Light sun = RenderSettings.sun;
        if (sun != null)
        {
            _originalSunColor = sun.color;
            _originalSunIntensity = sun.intensity;
        }
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
        if (!mainMenuPanel.activeInHierarchy && !confirmPanel.activeInHierarchy && !caughtPanel.activeInHierarchy && !escapedPanel.activeInHierarchy)
            return;
        GameObject[] buttons = CurrentButtons();
        if (buttons.Length == 0) return;
        if (buttons[currentIndex] == null) return;

        buttons[currentIndex].GetComponent<UnityEngine.UI.Button>().onClick.Invoke();
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
        gameStarted = true;
        isPaused = false;

        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (confirmPanel != null) confirmPanel.SetActive(false);

        Time.timeScale = 1f;
        PlayMusic(gameMusic);
        Light sun = RenderSettings.sun;
        if (sun != null)
        {
            sun.color = _originalSunColor;
            sun.intensity = _originalSunIntensity;
        }
    }

    public void PauseGame()
    {
        if (!gameStarted) return;

        isPaused = true;
        Time.timeScale = 0f;
        PlayMusic(menuMusic);

        if (confirmPanel != null)
            confirmPanel.SetActive(true);

        currentIndex = 0;
        UpdateHighlight(CurrentButtons());
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        PlayMusic(gameMusic);

        if (confirmPanel != null)
            confirmPanel.SetActive(false);

        currentIndex = 0;
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


    private void ShowInitialMenu()
    {
        modeSelected = false;
        selectedMode = "";
        gameStarted = false;
        gameEnded = false;
        isPaused = false;
        onSecondScreen = false;
        currentIndex = 0;

        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        if (confirmPanel != null) confirmPanel.SetActive(false);
        if (caughtPanel != null) caughtPanel.SetActive(false);
        if (escapedPanel != null) escapedPanel.SetActive(false);

        if (singlePlayerButtonObject != null) singlePlayerButtonObject.SetActive(true);
        if (multiplayerButtonObject != null) multiplayerButtonObject.SetActive(true);
        if (mainExitButtonObject != null) mainExitButtonObject.SetActive(true);

        if (startButtonObject != null) startButtonObject.SetActive(false);
        if (backButtonObject != null) backButtonObject.SetActive(false);

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
        if (caughtPanel != null && caughtPanel.activeSelf)
            return new GameObject[] { tryAgainButtonObject, caughtExitButtonObject };

        if (escapedPanel != null && escapedPanel.activeSelf)
            return new GameObject[] { playAgainButtonObject, escapedExitButtonObject };

        if (isPaused)
            return new GameObject[] { resumeButtonObject, exitToMenuButtonObject };

        if (onSecondScreen)
            return new GameObject[] { startButtonObject, backButtonObject };

        return new GameObject[] { singlePlayerButtonObject, multiplayerButtonObject, mainExitButtonObject };

    }

    private void UpdateHighlight(GameObject[] buttons)
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i] == null) continue;
            var img = buttons[i].GetComponentInChildren<UnityEngine.UI.Image>();
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
        input.InGameAction.Enable();
        input.InGameAction.Pause.performed += OnPausePressed;
        OnPlayerCaught += TriggerCaught;
        OnPlayerEscaped += TriggerEscaped;
    }

    void OnDisable()
    {
        input.Menu.Navigate.performed -= OnNavigate;
        input.Menu.Select.performed -= OnSelect;
        input.Menu.Disable();
        input.InGameAction.Pause.performed -= OnPausePressed;
        input.InGameAction.Disable();
        OnPlayerCaught -= TriggerCaught;
        OnPlayerEscaped -= TriggerEscaped;
        
    }
    private void OnPausePressed(InputAction.CallbackContext ctx)
    {
        if (!gameStarted) return;

        if (isPaused)
            ResumeGame();
        else
            PauseGame();
    }
    public void ExitToMainMenu()
    {
        Time.timeScale = 1f;

        if (cityGenerator != null)
            cityGenerator.ClearCity();
        PlayMusic(menuMusic);

        ShowInitialMenu();
    }

    public void TriggerCaught()
    {
        if (gameEnded || !gameStarted) return;
        gameEnded = true;
        gameStarted = false;
        StartCoroutine(CaughtSequence());
    }

    public void TriggerEscaped()
    {
        if (gameEnded || !gameStarted) return;
        gameEnded = true;
        gameStarted = false;
        StartCoroutine(EscapedSequence());
    }

    IEnumerator CaughtSequence()
    {
        PlayMusic(caughtMusic);
        GameObject playerCar = GameObject.Find("PlayerCar");
        if (playerCar != null)
        {
            CarController cc = playerCar.GetComponentInChildren<CarController>();
            if (cc != null) cc.enabled = false;
            Rigidbody rb = playerCar.GetComponent<Rigidbody>();
            if (rb != null) rb.linearVelocity = Vector3.zero;
        }

        GameObject policeCar = GameObject.Find("PoliceCar");
        if (policeCar != null)
        {
            PoliceAIController ai = policeCar.GetComponent<PoliceAIController>();
            if (ai != null) ai.enabled = false;
        }

        UnityEngine.UI.Image panelImage = caughtPanel != null
            ? caughtPanel.GetComponent<UnityEngine.UI.Image>()
            : null;

        Color flashRed = new Color(0.8f, 0f, 0f, 0.6f);
        Color flashBlue = new Color(0f, 0.2f, 0.8f, 0.6f);

        if (caughtPanel != null) caughtPanel.SetActive(true);

        float elapsed = 0f;
        float flashTimer = 0f;
        bool isRed = true;

        while (elapsed < 1.5f)
        {
            elapsed += Time.deltaTime;
            flashTimer += Time.deltaTime;

            if (flashTimer >= 0.15f)
            {
                flashTimer = 0f;
                isRed = !isRed;
                if (panelImage != null) panelImage.color = isRed ? flashRed : flashBlue;
            }

            yield return null;
        }

        if (panelImage != null) panelImage.color = new Color(0.7f, 0f, 0f, 0.6f);

        currentIndex = 0;
        UpdateHighlight(CurrentButtons());
    }

    IEnumerator EscapedSequence()
    {
        PlayMusic(escapeMusic);
        GameObject playerCar = GameObject.Find("PlayerCar");
        Rigidbody rb = null;

        if (playerCar != null)
        {
            CarController cc = playerCar.GetComponentInChildren<CarController>();
            if (cc != null) cc.enabled = false;
            rb = playerCar.GetComponent<Rigidbody>();
            if (rb != null) rb.constraints = RigidbodyConstraints.FreezePositionY
                                        | RigidbodyConstraints.FreezeRotationX
                                        | RigidbodyConstraints.FreezeRotationZ;
        }
        if (playerCar != null && cityGenerator != null && cityGenerator.exitWorldDirection != Vector3.zero)
        {
            playerCar.transform.rotation = Quaternion.LookRotation(cityGenerator.exitWorldDirection, Vector3.up);
        }

        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            TopDownCamera topCam = mainCam.GetComponent<TopDownCamera>();
            if (topCam != null) topCam.enabled = false;
        }

        Light sun = RenderSettings.sun;
        Color sunsetColor = new Color(1f, 0.45f, 0.1f);
        Color originalColor = sun != null ? sun.color : Color.white;
        float originalIntensity = sun != null ? sun.intensity : 1f;

        float cinematicDuration = 4.5f;
        float elapsed = 0f;
        float driveSpeed = 15f;

        while (elapsed < cinematicDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / cinematicDuration;

            if (playerCar != null && rb != null)
                rb.linearVelocity = playerCar.transform.forward * driveSpeed;

            if (sun != null)
            {
                sun.color = Color.Lerp(originalColor, sunsetColor, t);
                sun.intensity = Mathf.Lerp(originalIntensity, 0.6f, t);
            }

            if (mainCam != null && playerCar != null)
            {
                Vector3 target = playerCar.transform.position
                            - playerCar.transform.forward * 8f
                            + Vector3.up * 3.5f;
                mainCam.transform.position = Vector3.Lerp(mainCam.transform.position, target, Time.deltaTime * 4f);
                mainCam.transform.LookAt(playerCar.transform.position + playerCar.transform.forward * 4f);
            }

            yield return null;
        }

        if (escapedPanel != null) escapedPanel.SetActive(true);
        currentIndex = 0;
        UpdateHighlight(CurrentButtons());
    }

    public void TryAgain()
    {
        gameEnded = false;
        if (caughtPanel != null) caughtPanel.SetActive(false);
        StartGame();
    }

    public void PlayAgain()
    {
        gameEnded = false;
        if (escapedPanel != null) escapedPanel.SetActive(false);
        StartGame();
    }

    void PlayMusic(AudioClip clip)
    {
        if (clip == null || audioSource == null) return;
        if (audioSource.clip == clip) return;
        audioSource.clip = clip;
        audioSource.Play();
    }
}
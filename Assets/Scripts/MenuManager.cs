using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Manages all menu screens including main menu, pause menu, and game over screen.
/// </summary>
public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance { get; private set; }
    
    [Header("Menu Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject settingsPanel;
    
    [Header("Main Menu Elements")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private Text titleText;
    [SerializeField] private Text highScoreText;
    
    [Header("Pause Menu Elements")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button pauseSettingsButton;
    [SerializeField] private Button mainMenuButton;
    
    [Header("Game Over Elements")]
    [SerializeField] private Text finalScoreText;
    [SerializeField] private Text finalHighScoreText;
    [SerializeField] private Text newHighScoreText;
    [SerializeField] private Button retryButton;
    [SerializeField] private Button gameOverMainMenuButton;
    
    [Header("Settings")]
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private Toggle sfxToggle;
    [SerializeField] private Button closeSettingsButton;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        SetupButtons();
    }
    
    private void Start()
    {
        ShowMainMenu();
    }
    
    private void SetupButtons()
    {
        // Main Menu
        if (playButton != null)
            playButton.onClick.AddListener(OnPlayClicked);
        if (settingsButton != null)
            settingsButton.onClick.AddListener(OnSettingsClicked);
        if (quitButton != null)
            quitButton.onClick.AddListener(OnQuitClicked);
        
        // Pause Menu
        if (resumeButton != null)
            resumeButton.onClick.AddListener(OnResumeClicked);
        if (pauseSettingsButton != null)
            pauseSettingsButton.onClick.AddListener(OnSettingsClicked);
        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(OnMainMenuClicked);
        
        // Game Over
        if (retryButton != null)
            retryButton.onClick.AddListener(OnRetryClicked);
        if (gameOverMainMenuButton != null)
            gameOverMainMenuButton.onClick.AddListener(OnMainMenuClicked);
        
        // Settings
        if (closeSettingsButton != null)
            closeSettingsButton.onClick.AddListener(OnCloseSettingsClicked);
        if (volumeSlider != null)
            volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        if (sfxToggle != null)
            sfxToggle.onValueChanged.AddListener(OnSFXToggled);
    }
    
    /// <summary>
    /// Show main menu
    /// </summary>
    public void ShowMainMenu()
    {
        HideAllPanels();
        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(true);
        }
        
        // Update high score display
        if (highScoreText != null && GameManager.Instance != null)
        {
            highScoreText.text = $"HIGH SCORE: {GameManager.Instance.HighScore:N0}";
        }
        
        Time.timeScale = 1f;
    }
    
    /// <summary>
    /// Hide main menu
    /// </summary>
    public void HideMainMenu()
    {
        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(false);
        }
    }
    
    /// <summary>
    /// Show pause menu
    /// </summary>
    public void ShowPauseMenu()
    {
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(true);
        }
    }
    
    /// <summary>
    /// Hide pause menu
    /// </summary>
    public void HidePauseMenu()
    {
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
    }
    
    /// <summary>
    /// Show game over screen with score
    /// </summary>
    public void ShowGameOverMenu(int score, int highScore)
    {
        HideAllPanels();
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
        
        if (finalScoreText != null)
        {
            finalScoreText.text = $"SCORE: {score:N0}";
        }
        
        if (finalHighScoreText != null)
        {
            finalHighScoreText.text = $"HIGH SCORE: {highScore:N0}";
        }
        
        if (newHighScoreText != null)
        {
            newHighScoreText.gameObject.SetActive(score >= highScore && score > 0);
        }
    }
    
    /// <summary>
    /// Show settings panel
    /// </summary>
    public void ShowSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
            
            // Load current settings
            if (volumeSlider != null)
            {
                volumeSlider.value = PlayerPrefs.GetFloat("Volume", 1f);
            }
            if (sfxToggle != null)
            {
                sfxToggle.isOn = PlayerPrefs.GetInt("SFX", 1) == 1;
            }
        }
    }
    
    private void HideAllPanels()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
    }
    
    // Button Callbacks
    private void OnPlayClicked()
    {
        AudioManager.Instance?.PlaySound("ButtonClick");
        HideMainMenu();
        GameManager.Instance?.StartGame();
    }
    
    private void OnSettingsClicked()
    {
        AudioManager.Instance?.PlaySound("ButtonClick");
        ShowSettings();
    }
    
    private void OnQuitClicked()
    {
        AudioManager.Instance?.PlaySound("ButtonClick");
        GameManager.Instance?.QuitGame();
    }
    
    private void OnResumeClicked()
    {
        AudioManager.Instance?.PlaySound("ButtonClick");
        GameManager.Instance?.ResumeGame();
    }
    
    private void OnMainMenuClicked()
    {
        AudioManager.Instance?.PlaySound("ButtonClick");
        GameManager.Instance?.ReturnToMenu();
    }
    
    private void OnRetryClicked()
    {
        AudioManager.Instance?.PlaySound("ButtonClick");
        HideAllPanels();
        GameManager.Instance?.StartGame();
    }
    
    private void OnCloseSettingsClicked()
    {
        AudioManager.Instance?.PlaySound("ButtonClick");
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
    }
    
    private void OnVolumeChanged(float value)
    {
        AudioManager.Instance?.SetMasterVolume(value);
        PlayerPrefs.SetFloat("Volume", value);
    }
    
    private void OnSFXToggled(bool isOn)
    {
        AudioManager.Instance?.SetSFXEnabled(isOn);
        PlayerPrefs.SetInt("SFX", isOn ? 1 : 0);
    }
    
    /// <summary>
    /// Create menu UI elements programmatically
    /// </summary>
    public void CreateMenuUI()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null) return;
        
        // Create Main Menu Panel
        mainMenuPanel = CreateMenuPanel("MainMenuPanel", canvas.transform);
        titleText = CreateMenuText("TitleText", mainMenuPanel.transform, new Vector2(0, 150), "SPACE SHOOTER", 48);
        playButton = CreateMenuButton("PlayButton", mainMenuPanel.transform, new Vector2(0, 50), "PLAY");
        settingsButton = CreateMenuButton("SettingsButton", mainMenuPanel.transform, new Vector2(0, -20), "SETTINGS");
        quitButton = CreateMenuButton("QuitButton", mainMenuPanel.transform, new Vector2(0, -90), "QUIT");
        highScoreText = CreateMenuText("HighScoreText", mainMenuPanel.transform, new Vector2(0, -180), "HIGH SCORE: 0", 24);
        
        // Create Pause Menu Panel
        pauseMenuPanel = CreateMenuPanel("PauseMenuPanel", canvas.transform);
        CreateMenuText("PauseTitleText", pauseMenuPanel.transform, new Vector2(0, 100), "PAUSED", 48);
        resumeButton = CreateMenuButton("ResumeButton", pauseMenuPanel.transform, new Vector2(0, 20), "RESUME");
        mainMenuButton = CreateMenuButton("MainMenuButton", pauseMenuPanel.transform, new Vector2(0, -50), "MAIN MENU");
        pauseMenuPanel.SetActive(false);
        
        // Create Game Over Panel
        gameOverPanel = CreateMenuPanel("GameOverPanel", canvas.transform);
        CreateMenuText("GameOverTitleText", gameOverPanel.transform, new Vector2(0, 150), "GAME OVER", 48);
        finalScoreText = CreateMenuText("FinalScoreText", gameOverPanel.transform, new Vector2(0, 80), "SCORE: 0", 32);
        finalHighScoreText = CreateMenuText("FinalHighScoreText", gameOverPanel.transform, new Vector2(0, 40), "HIGH SCORE: 0", 24);
        newHighScoreText = CreateMenuText("NewHighScoreText", gameOverPanel.transform, new Vector2(0, 0), "NEW HIGH SCORE!", 28);
        newHighScoreText.color = Color.yellow;
        retryButton = CreateMenuButton("RetryButton", gameOverPanel.transform, new Vector2(0, -60), "RETRY");
        gameOverMainMenuButton = CreateMenuButton("GameOverMainMenuButton", gameOverPanel.transform, new Vector2(0, -130), "MAIN MENU");
        gameOverPanel.SetActive(false);
        
        SetupButtons();
    }
    
    private GameObject CreateMenuPanel(string name, Transform parent)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent, false);
        
        RectTransform rt = panel.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;
        
        Image image = panel.AddComponent<Image>();
        image.color = new Color(0, 0, 0, 0.8f);
        
        return panel;
    }
    
    private Text CreateMenuText(string name, Transform parent, Vector2 position, string content, int fontSize)
    {
        GameObject textObj = new GameObject(name);
        textObj.transform.SetParent(parent, false);
        
        RectTransform rt = textObj.AddComponent<RectTransform>();
        rt.anchoredPosition = position;
        rt.sizeDelta = new Vector2(400, 60);
        
        Text text = textObj.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = fontSize;
        text.color = Color.white;
        text.alignment = TextAnchor.MiddleCenter;
        text.text = content;
        
        return text;
    }
    
    private Button CreateMenuButton(string name, Transform parent, Vector2 position, string label)
    {
        GameObject buttonObj = new GameObject(name);
        buttonObj.transform.SetParent(parent, false);
        
        RectTransform rt = buttonObj.AddComponent<RectTransform>();
        rt.anchoredPosition = position;
        rt.sizeDelta = new Vector2(200, 50);
        
        Image image = buttonObj.AddComponent<Image>();
        image.color = new Color(0.2f, 0.4f, 0.8f);
        
        Button button = buttonObj.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.highlightedColor = new Color(0.3f, 0.5f, 1f);
        colors.pressedColor = new Color(0.15f, 0.3f, 0.6f);
        button.colors = colors;
        
        // Add text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);
        
        RectTransform textRt = textObj.AddComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.sizeDelta = Vector2.zero;
        
        Text text = textObj.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 24;
        text.color = Color.white;
        text.alignment = TextAnchor.MiddleCenter;
        text.text = label;
        
        return button;
    }
}

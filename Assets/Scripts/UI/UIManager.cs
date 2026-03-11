using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Manages all UI elements and updates.
/// Attach this script to a Canvas GameObject.
/// </summary>
public class UIManager : MonoBehaviour
{
    [Header("HUD Elements")]
    [Tooltip("Text component for displaying score")]
    [SerializeField] private Text scoreText;
    
    [Tooltip("Text component for displaying high score")]
    [SerializeField] private Text highScoreText;
    
    [Tooltip("Text component for displaying health")]
    [SerializeField] private Text healthText;
    
    [Tooltip("Slider for health bar")]
    [SerializeField] private Slider healthBar;
    
    [Tooltip("Text component for displaying wave number")]
    [SerializeField] private Text waveText;
    
    [Tooltip("Text component for displaying lives")]
    [SerializeField] private Text livesText;
    
    [Header("Menu Panels")]
    [Tooltip("Main menu panel GameObject")]
    [SerializeField] private GameObject mainMenuPanel;
    
    [Tooltip("HUD panel GameObject")]
    [SerializeField] private GameObject hudPanel;
    
    [Tooltip("Pause menu panel GameObject")]
    [SerializeField] private GameObject pauseMenuPanel;
    
    [Tooltip("Game over panel GameObject")]
    [SerializeField] private GameObject gameOverPanel;
    
    [Header("Game Over Elements")]
    [Tooltip("Final score text on game over screen")]
    [SerializeField] private Text finalScoreText;
    
    [Tooltip("New high score indicator")]
    [SerializeField] private GameObject newHighScoreIndicator;
    
    // Singleton instance
    private static UIManager instance;
    public static UIManager Instance => instance;
    
    // Cached player reference
    private PlayerHealth playerHealth;
    
    /// <summary>
    /// Initialize singleton.
    /// </summary>
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        // Create UI elements if not assigned
        if (mainMenuPanel == null || hudPanel == null)
        {
            CreateDefaultUI();
        }
    }
    
    /// <summary>
    /// Subscribe to events on start.
    /// </summary>
    private void Start()
    {
        // Subscribe to score changes
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnScoreChanged += UpdateScoreDisplay;
            ScoreManager.Instance.OnHighScoreChanged += UpdateHighScoreDisplay;
            UpdateScoreDisplay(ScoreManager.Instance.CurrentScore);
            UpdateHighScoreDisplay(ScoreManager.Instance.HighScore);
        }
        
        // Subscribe to game state changes
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStateChanged += OnGameStateChanged;
            GameManager.Instance.OnLivesChanged += UpdateLivesDisplay;
        }
        
        // Start with main menu
        ShowMainMenu();
    }
    
    /// <summary>
    /// Clean up event subscriptions.
    /// </summary>
    private void OnDestroy()
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnScoreChanged -= UpdateScoreDisplay;
            ScoreManager.Instance.OnHighScoreChanged -= UpdateHighScoreDisplay;
        }
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStateChanged -= OnGameStateChanged;
            GameManager.Instance.OnLivesChanged -= UpdateLivesDisplay;
        }
    }
    
    /// <summary>
    /// Create default UI elements when none are assigned.
    /// </summary>
    private void CreateDefaultUI()
    {
        // Ensure we have a Canvas
        Canvas canvas = GetComponent<Canvas>();
        if (canvas == null)
        {
            canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            gameObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            gameObject.AddComponent<GraphicRaycaster>();
        }
        
        // Create Main Menu Panel
        mainMenuPanel = CreatePanel("MainMenuPanel");
        CreateMainMenuContent(mainMenuPanel);
        
        // Create HUD Panel
        hudPanel = CreatePanel("HUDPanel");
        CreateHUDContent(hudPanel);
        
        // Create Pause Menu Panel
        pauseMenuPanel = CreatePanel("PauseMenuPanel");
        CreatePauseMenuContent(pauseMenuPanel);
        
        // Create Game Over Panel
        gameOverPanel = CreatePanel("GameOverPanel");
        CreateGameOverContent(gameOverPanel);
    }
    
    /// <summary>
    /// Create a UI panel.
    /// </summary>
    private GameObject CreatePanel(string name)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(transform, false);
        
        RectTransform rectTransform = panel.AddComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.sizeDelta = Vector2.zero;
        rectTransform.anchoredPosition = Vector2.zero;
        
        return panel;
    }
    
    /// <summary>
    /// Create main menu UI content.
    /// </summary>
    private void CreateMainMenuContent(GameObject panel)
    {
        // Background
        Image bg = panel.AddComponent<Image>();
        bg.color = new Color(0, 0, 0, 0.8f);
        
        // Title
        GameObject titleObj = CreateText(panel.transform, "TitleText", "SPACE SHOOTER", 48, TextAnchor.MiddleCenter);
        SetRectTransform(titleObj.GetComponent<RectTransform>(), new Vector2(0.5f, 0.75f), new Vector2(400, 80));
        
        // Play Button
        GameObject playBtn = CreateButton(panel.transform, "PlayButton", "PLAY", () => {
            if (GameManager.Instance != null) GameManager.Instance.StartGame();
        });
        SetRectTransform(playBtn.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(200, 50));
        
        // Quit Button
        GameObject quitBtn = CreateButton(panel.transform, "QuitButton", "QUIT", () => {
            if (GameManager.Instance != null) GameManager.Instance.QuitGame();
        });
        SetRectTransform(quitBtn.GetComponent<RectTransform>(), new Vector2(0.5f, 0.35f), new Vector2(200, 50));
        
        // Instructions
        GameObject instructionsObj = CreateText(panel.transform, "InstructionsText", 
            "Controls:\nWASD or Arrow Keys - Move\nSpace or Left Click - Shoot\nESC or P - Pause", 18, TextAnchor.MiddleCenter);
        SetRectTransform(instructionsObj.GetComponent<RectTransform>(), new Vector2(0.5f, 0.15f), new Vector2(400, 100));
    }
    
    /// <summary>
    /// Create HUD UI content.
    /// </summary>
    private void CreateHUDContent(GameObject panel)
    {
        // Score text (top left)
        GameObject scoreObj = CreateText(panel.transform, "ScoreText", "Score: 0", 24, TextAnchor.UpperLeft);
        SetRectTransform(scoreObj.GetComponent<RectTransform>(), new Vector2(0, 1), new Vector2(0, -10), new Vector2(200, 40), new Vector2(10, 0));
        scoreText = scoreObj.GetComponent<Text>();
        
        // High score text (top center)
        GameObject highScoreObj = CreateText(panel.transform, "HighScoreText", "High Score: 0", 20, TextAnchor.UpperCenter);
        SetRectTransform(highScoreObj.GetComponent<RectTransform>(), new Vector2(0.5f, 1), new Vector2(200, 30));
        highScoreObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -10);
        highScoreText = highScoreObj.GetComponent<Text>();
        
        // Lives text (top right)
        GameObject livesObj = CreateText(panel.transform, "LivesText", "Lives: 3", 24, TextAnchor.UpperRight);
        SetRectTransform(livesObj.GetComponent<RectTransform>(), new Vector2(1, 1), new Vector2(-10, -10), new Vector2(150, 40), new Vector2(0, 0));
        livesText = livesObj.GetComponent<Text>();
        
        // Health bar (bottom left)
        GameObject healthBarObj = CreateHealthBar(panel.transform);
        healthBar = healthBarObj.GetComponent<Slider>();
        
        // Health text
        GameObject healthObj = CreateText(panel.transform, "HealthText", "HP: 100", 18, TextAnchor.LowerLeft);
        SetRectTransform(healthObj.GetComponent<RectTransform>(), new Vector2(0, 0), new Vector2(10, 40), new Vector2(100, 30), new Vector2(0, 0));
        healthText = healthObj.GetComponent<Text>();
        
        // Wave text (bottom center)
        GameObject waveObj = CreateText(panel.transform, "WaveText", "", 32, TextAnchor.LowerCenter);
        SetRectTransform(waveObj.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(300, 50));
        waveText = waveObj.GetComponent<Text>();
        waveText.color = Color.yellow;
    }
    
    /// <summary>
    /// Create pause menu content.
    /// </summary>
    private void CreatePauseMenuContent(GameObject panel)
    {
        // Background
        Image bg = panel.AddComponent<Image>();
        bg.color = new Color(0, 0, 0, 0.7f);
        
        // Title
        GameObject titleObj = CreateText(panel.transform, "PausedText", "PAUSED", 48, TextAnchor.MiddleCenter);
        SetRectTransform(titleObj.GetComponent<RectTransform>(), new Vector2(0.5f, 0.65f), new Vector2(300, 80));
        
        // Resume Button
        GameObject resumeBtn = CreateButton(panel.transform, "ResumeButton", "RESUME", () => {
            if (GameManager.Instance != null) GameManager.Instance.ResumeGame();
        });
        SetRectTransform(resumeBtn.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(200, 50));
        
        // Main Menu Button
        GameObject menuBtn = CreateButton(panel.transform, "MainMenuButton", "MAIN MENU", () => {
            if (GameManager.Instance != null) GameManager.Instance.ReturnToMainMenu();
        });
        SetRectTransform(menuBtn.GetComponent<RectTransform>(), new Vector2(0.5f, 0.35f), new Vector2(200, 50));
    }
    
    /// <summary>
    /// Create game over content.
    /// </summary>
    private void CreateGameOverContent(GameObject panel)
    {
        // Background
        Image bg = panel.AddComponent<Image>();
        bg.color = new Color(0, 0, 0, 0.85f);
        
        // Title
        GameObject titleObj = CreateText(panel.transform, "GameOverText", "GAME OVER", 56, TextAnchor.MiddleCenter);
        SetRectTransform(titleObj.GetComponent<RectTransform>(), new Vector2(0.5f, 0.7f), new Vector2(400, 80));
        titleObj.GetComponent<Text>().color = Color.red;
        
        // Final Score
        GameObject scoreObj = CreateText(panel.transform, "FinalScoreText", "Final Score: 0", 32, TextAnchor.MiddleCenter);
        SetRectTransform(scoreObj.GetComponent<RectTransform>(), new Vector2(0.5f, 0.55f), new Vector2(300, 50));
        finalScoreText = scoreObj.GetComponent<Text>();
        
        // New High Score indicator
        GameObject highScoreIndObj = CreateText(panel.transform, "NewHighScoreText", "NEW HIGH SCORE!", 28, TextAnchor.MiddleCenter);
        SetRectTransform(highScoreIndObj.GetComponent<RectTransform>(), new Vector2(0.5f, 0.45f), new Vector2(300, 40));
        highScoreIndObj.GetComponent<Text>().color = Color.yellow;
        newHighScoreIndicator = highScoreIndObj;
        
        // Restart Button
        GameObject restartBtn = CreateButton(panel.transform, "RestartButton", "PLAY AGAIN", () => {
            if (GameManager.Instance != null) GameManager.Instance.RestartGame();
        });
        SetRectTransform(restartBtn.GetComponent<RectTransform>(), new Vector2(0.5f, 0.3f), new Vector2(200, 50));
        
        // Main Menu Button
        GameObject menuBtn = CreateButton(panel.transform, "MainMenuButton2", "MAIN MENU", () => {
            if (GameManager.Instance != null) GameManager.Instance.ReturnToMainMenu();
        });
        SetRectTransform(menuBtn.GetComponent<RectTransform>(), new Vector2(0.5f, 0.18f), new Vector2(200, 50));
    }
    
    /// <summary>
    /// Create a text element.
    /// </summary>
    private GameObject CreateText(Transform parent, string name, string content, int fontSize, TextAnchor alignment)
    {
        GameObject textObj = new GameObject(name);
        textObj.transform.SetParent(parent, false);
        
        RectTransform rectTransform = textObj.AddComponent<RectTransform>();
        
        Text text = textObj.AddComponent<Text>();
        text.text = content;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (text.font == null) text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = Color.white;
        
        return textObj;
    }
    
    /// <summary>
    /// Create a button element.
    /// </summary>
    private GameObject CreateButton(Transform parent, string name, string text, UnityEngine.Events.UnityAction onClick)
    {
        GameObject buttonObj = new GameObject(name);
        buttonObj.transform.SetParent(parent, false);
        
        RectTransform rectTransform = buttonObj.AddComponent<RectTransform>();
        
        Image image = buttonObj.AddComponent<Image>();
        image.color = new Color(0.2f, 0.2f, 0.2f, 1f);
        
        Button button = buttonObj.AddComponent<Button>();
        button.targetGraphic = image;
        
        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.2f, 0.2f, 0.2f, 1f);
        colors.highlightedColor = new Color(0.3f, 0.3f, 0.3f, 1f);
        colors.pressedColor = new Color(0.1f, 0.1f, 0.1f, 1f);
        button.colors = colors;
        
        button.onClick.AddListener(onClick);
        
        // Add text child
        GameObject textObj = CreateText(buttonObj.transform, "Text", text, 24, TextAnchor.MiddleCenter);
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        
        return buttonObj;
    }
    
    /// <summary>
    /// Create a health bar slider.
    /// </summary>
    private GameObject CreateHealthBar(Transform parent)
    {
        GameObject sliderObj = new GameObject("HealthBar");
        sliderObj.transform.SetParent(parent, false);
        
        RectTransform rectTransform = sliderObj.AddComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(0, 0);
        rectTransform.pivot = new Vector2(0, 0);
        rectTransform.anchoredPosition = new Vector2(10, 10);
        rectTransform.sizeDelta = new Vector2(200, 20);
        
        Slider slider = sliderObj.AddComponent<Slider>();
        slider.minValue = 0;
        slider.maxValue = 100;
        slider.value = 100;
        slider.interactable = false;
        
        // Background
        GameObject background = new GameObject("Background");
        background.transform.SetParent(sliderObj.transform, false);
        RectTransform bgRect = background.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;
        Image bgImage = background.AddComponent<Image>();
        bgImage.color = new Color(0.3f, 0.3f, 0.3f, 1f);
        
        // Fill Area
        GameObject fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(sliderObj.transform, false);
        RectTransform fillAreaRect = fillArea.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = Vector2.zero;
        fillAreaRect.anchorMax = Vector2.one;
        fillAreaRect.sizeDelta = new Vector2(-5, 0);
        fillAreaRect.anchoredPosition = new Vector2(-2.5f, 0);
        
        // Fill
        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(fillArea.transform, false);
        RectTransform fillRect = fill.AddComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.sizeDelta = Vector2.zero;
        Image fillImage = fill.AddComponent<Image>();
        fillImage.color = Color.green;
        
        slider.fillRect = fillRect;
        
        return sliderObj;
    }
    
    /// <summary>
    /// Helper to set RectTransform properties.
    /// </summary>
    private void SetRectTransform(RectTransform rect, Vector2 anchorPivot, Vector2 sizeDelta)
    {
        rect.anchorMin = anchorPivot;
        rect.anchorMax = anchorPivot;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = sizeDelta;
        rect.anchoredPosition = Vector2.zero;
    }
    
    /// <summary>
    /// Helper to set RectTransform with position.
    /// </summary>
    private void SetRectTransform(RectTransform rect, Vector2 anchorPivot, Vector2 anchoredPos, Vector2 sizeDelta, Vector2 pivot)
    {
        rect.anchorMin = anchorPivot;
        rect.anchorMax = anchorPivot;
        rect.pivot = pivot;
        rect.sizeDelta = sizeDelta;
        rect.anchoredPosition = anchoredPos;
    }
    
    /// <summary>
    /// Handle game state changes.
    /// </summary>
    private void OnGameStateChanged(GameManager.GameState newState)
    {
        switch (newState)
        {
            case GameManager.GameState.MainMenu:
                ShowMainMenu();
                break;
            case GameManager.GameState.Playing:
                ShowHUD();
                break;
            case GameManager.GameState.Paused:
                ShowPauseMenu();
                break;
            case GameManager.GameState.GameOver:
                ShowGameOverScreen();
                break;
        }
    }
    
    /// <summary>
    /// Show the main menu.
    /// </summary>
    public void ShowMainMenu()
    {
        SetPanelActive(mainMenuPanel, true);
        SetPanelActive(hudPanel, false);
        SetPanelActive(pauseMenuPanel, false);
        SetPanelActive(gameOverPanel, false);
    }
    
    /// <summary>
    /// Show the HUD (gameplay UI).
    /// </summary>
    public void ShowHUD()
    {
        SetPanelActive(mainMenuPanel, false);
        SetPanelActive(hudPanel, true);
        SetPanelActive(pauseMenuPanel, false);
        SetPanelActive(gameOverPanel, false);
        
        // Subscribe to player health if available
        SubscribeToPlayerHealth();
    }
    
    /// <summary>
    /// Show the pause menu.
    /// </summary>
    public void ShowPauseMenu()
    {
        SetPanelActive(pauseMenuPanel, true);
    }
    
    /// <summary>
    /// Hide the pause menu.
    /// </summary>
    public void HidePauseMenu()
    {
        SetPanelActive(pauseMenuPanel, false);
    }
    
    /// <summary>
    /// Show the game over screen.
    /// </summary>
    public void ShowGameOverScreen()
    {
        SetPanelActive(hudPanel, true);
        SetPanelActive(gameOverPanel, true);
        SetPanelActive(pauseMenuPanel, false);
        
        // Update final score
        if (finalScoreText != null && ScoreManager.Instance != null)
        {
            finalScoreText.text = $"Final Score: {ScoreManager.Instance.CurrentScore}";
        }
        
        // Show/hide new high score indicator
        if (newHighScoreIndicator != null && ScoreManager.Instance != null)
        {
            newHighScoreIndicator.SetActive(ScoreManager.Instance.CheckHighScore());
        }
    }
    
    /// <summary>
    /// Safely set panel active state.
    /// </summary>
    private void SetPanelActive(GameObject panel, bool active)
    {
        if (panel != null)
        {
            panel.SetActive(active);
        }
    }
    
    /// <summary>
    /// Update the score display.
    /// </summary>
    private void UpdateScoreDisplay(int score)
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {score}";
        }
    }
    
    /// <summary>
    /// Update the high score display.
    /// </summary>
    private void UpdateHighScoreDisplay(int highScore)
    {
        if (highScoreText != null)
        {
            highScoreText.text = $"High Score: {highScore}";
        }
    }
    
    /// <summary>
    /// Update the health display.
    /// </summary>
    public void UpdateHealthDisplay(int currentHealth, int maxHealth)
    {
        if (healthText != null)
        {
            healthText.text = $"HP: {currentHealth}";
        }
        
        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = currentHealth;
            
            // Change color based on health percentage
            float healthPercent = (float)currentHealth / maxHealth;
            Image fillImage = healthBar.fillRect?.GetComponent<Image>();
            if (fillImage != null)
            {
                if (healthPercent > 0.5f)
                    fillImage.color = Color.green;
                else if (healthPercent > 0.25f)
                    fillImage.color = Color.yellow;
                else
                    fillImage.color = Color.red;
            }
        }
    }
    
    /// <summary>
    /// Update lives display.
    /// </summary>
    private void UpdateLivesDisplay(int lives)
    {
        if (livesText != null)
        {
            livesText.text = $"Lives: {lives}";
        }
    }
    
    /// <summary>
    /// Show wave announcement.
    /// </summary>
    public void ShowWaveText(int waveNumber)
    {
        if (waveText != null)
        {
            StartCoroutine(ShowWaveTextCoroutine(waveNumber));
        }
    }
    
    /// <summary>
    /// Coroutine to show and hide wave text.
    /// </summary>
    private IEnumerator ShowWaveTextCoroutine(int waveNumber)
    {
        waveText.text = $"WAVE {waveNumber}";
        waveText.gameObject.SetActive(true);
        
        yield return new WaitForSeconds(2f);
        
        waveText.gameObject.SetActive(false);
    }
    
    /// <summary>
    /// Subscribe to player health events.
    /// </summary>
    private void SubscribeToPlayerHealth()
    {
        // Unsubscribe from previous player
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged -= UpdateHealthDisplay;
        }
        
        // Find and subscribe to new player
        StartCoroutine(FindAndSubscribeToPlayer());
    }
    
    /// <summary>
    /// Coroutine to find player after spawn.
    /// </summary>
    private IEnumerator FindAndSubscribeToPlayer()
    {
        yield return null; // Wait one frame for player to spawn
        
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.OnHealthChanged += UpdateHealthDisplay;
                UpdateHealthDisplay(playerHealth.CurrentHealth, playerHealth.MaxHealth);
            }
        }
    }
}

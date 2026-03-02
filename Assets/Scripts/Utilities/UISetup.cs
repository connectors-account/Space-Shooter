using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UISetup creates the UI structure at runtime.
/// Creates all necessary UI panels and elements.
/// </summary>
public class UISetup : MonoBehaviour
{
    [Header("Colors")]
    public Color panelBackgroundColor = new Color(0, 0, 0, 0.8f);
    public Color buttonColor = new Color(0.2f, 0.6f, 0.9f, 1f);
    public Color textColor = Color.white;

    private Canvas mainCanvas;
    private UIManager uiManager;

    void Awake()
    {
        CreateUI();
    }

    /// <summary>
    /// Create all UI elements
    /// </summary>
    void CreateUI()
    {
        // Create canvas
        GameObject canvasObj = new GameObject("GameCanvas");
        mainCanvas = canvasObj.AddComponent<Canvas>();
        mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();

        // Add UIManager
        uiManager = canvasObj.AddComponent<UIManager>();

        // Create panels
        uiManager.mainMenuPanel = CreateMainMenuPanel(canvasObj.transform);
        uiManager.gameUIPanel = CreateGameUIPanel(canvasObj.transform);
        uiManager.pauseMenuPanel = CreatePauseMenuPanel(canvasObj.transform);
        uiManager.gameOverPanel = CreateGameOverPanel(canvasObj.transform);

        // Create wave announcement
        uiManager.waveAnnouncementText = CreateWaveAnnouncementText(canvasObj.transform);
    }

    /// <summary>
    /// Create main menu panel
    /// </summary>
    GameObject CreateMainMenuPanel(Transform parent)
    {
        GameObject panel = CreatePanel("MainMenuPanel", parent);
        RectTransform rt = panel.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        // Title
        CreateText("TitleText", panel.transform, "SPACE SHOOTER", 48, new Vector2(0, 100));
        
        // Subtitle
        CreateText("SubtitleText", panel.transform, "Defend the Galaxy!", 24, new Vector2(0, 40));

        // Play button
        CreateButton("PlayButton", panel.transform, "PLAY", new Vector2(0, -40), () =>
        {
            if (UIManager.Instance != null) UIManager.Instance.OnPlayButtonPressed();
        });

        // Quit button
        CreateButton("QuitButton", panel.transform, "QUIT", new Vector2(0, -110), () =>
        {
            if (UIManager.Instance != null) UIManager.Instance.OnQuitButtonPressed();
        });

        // Controls info
        CreateText("ControlsText", panel.transform, "Controls: WASD/Arrows to move, SPACE to shoot, ESC to pause", 16, new Vector2(0, -200));

        return panel;
    }

    /// <summary>
    /// Create game UI panel (HUD)
    /// </summary>
    GameObject CreateGameUIPanel(Transform parent)
    {
        GameObject panel = CreatePanel("GameUIPanel", parent, false);
        Image panelImage = panel.GetComponent<Image>();
        panelImage.color = Color.clear; // Transparent background
        
        RectTransform rt = panel.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        // Score (top left)
        uiManager.scoreText = CreateText("ScoreText", panel.transform, "Score: 0", 24, 
            Vector2.zero, TextAnchor.UpperLeft, new Vector2(0, 1), new Vector2(0, 1), new Vector2(20, -20));

        // High Score (top left, below score)
        uiManager.highScoreText = CreateText("HighScoreText", panel.transform, "High Score: 0", 18,
            Vector2.zero, TextAnchor.UpperLeft, new Vector2(0, 1), new Vector2(0, 1), new Vector2(20, -50));

        // Wave (top center)
        uiManager.waveText = CreateText("WaveText", panel.transform, "Wave: 1", 24,
            Vector2.zero, TextAnchor.UpperCenter, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -20));

        // Health (top right)
        CreateText("HealthLabel", panel.transform, "Health:", 18,
            Vector2.zero, TextAnchor.UpperRight, new Vector2(1, 1), new Vector2(1, 1), new Vector2(-180, -20));

        // Health bar
        uiManager.healthBar = CreateHealthBar(panel.transform);

        return panel;
    }

    /// <summary>
    /// Create pause menu panel
    /// </summary>
    GameObject CreatePauseMenuPanel(Transform parent)
    {
        GameObject panel = CreatePanel("PauseMenuPanel", parent, false);
        RectTransform rt = panel.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        // Pause title
        CreateText("PauseTitle", panel.transform, "PAUSED", 48, new Vector2(0, 80));

        // Resume button
        CreateButton("ResumeButton", panel.transform, "RESUME", new Vector2(0, 0), () =>
        {
            if (UIManager.Instance != null) UIManager.Instance.OnResumeButtonPressed();
        });

        // Main menu button
        CreateButton("MainMenuButton", panel.transform, "MAIN MENU", new Vector2(0, -70), () =>
        {
            if (UIManager.Instance != null) UIManager.Instance.OnMainMenuButtonPressed();
        });

        return panel;
    }

    /// <summary>
    /// Create game over panel
    /// </summary>
    GameObject CreateGameOverPanel(Transform parent)
    {
        GameObject panel = CreatePanel("GameOverPanel", parent, false);
        RectTransform rt = panel.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        // Game over title
        CreateText("GameOverTitle", panel.transform, "GAME OVER", 48, new Vector2(0, 120), TextAnchor.MiddleCenter);

        // Final score
        uiManager.finalScoreText = CreateText("FinalScoreText", panel.transform, "Score: 0", 32, new Vector2(0, 50));

        // High score
        uiManager.gameOverHighScoreText = CreateText("GameOverHighScore", panel.transform, "High Score: 0", 24, new Vector2(0, 0));

        // Restart button
        CreateButton("RestartButton", panel.transform, "PLAY AGAIN", new Vector2(0, -70), () =>
        {
            if (UIManager.Instance != null) UIManager.Instance.OnRestartButtonPressed();
        });

        // Main menu button
        CreateButton("GameOverMainMenuButton", panel.transform, "MAIN MENU", new Vector2(0, -140), () =>
        {
            if (UIManager.Instance != null) UIManager.Instance.OnMainMenuButtonPressed();
        });

        // Press R hint
        CreateText("RestartHint", panel.transform, "Press R to restart", 16, new Vector2(0, -200));

        return panel;
    }

    /// <summary>
    /// Create wave announcement text
    /// </summary>
    Text CreateWaveAnnouncementText(Transform parent)
    {
        Text text = CreateText("WaveAnnouncement", parent, "WAVE 1", 64, Vector2.zero);
        text.gameObject.SetActive(false);
        return text;
    }

    /// <summary>
    /// Helper: Create a panel
    /// </summary>
    GameObject CreatePanel(string name, Transform parent, bool showBackground = true)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent, false);
        
        RectTransform rt = panel.AddComponent<RectTransform>();
        
        Image image = panel.AddComponent<Image>();
        image.color = showBackground ? panelBackgroundColor : Color.clear;
        
        return panel;
    }

    /// <summary>
    /// Helper: Create text
    /// </summary>
    Text CreateText(string name, Transform parent, string content, int fontSize, Vector2 position,
        TextAnchor alignment = TextAnchor.MiddleCenter, 
        Vector2? anchorMin = null, Vector2? anchorMax = null, Vector2? anchoredPos = null)
    {
        GameObject textObj = new GameObject(name);
        textObj.transform.SetParent(parent, false);
        
        RectTransform rt = textObj.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(400, 60);
        
        if (anchorMin.HasValue && anchorMax.HasValue)
        {
            rt.anchorMin = anchorMin.Value;
            rt.anchorMax = anchorMax.Value;
            rt.anchoredPosition = anchoredPos ?? Vector2.zero;
        }
        else
        {
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = position;
        }
        
        Text text = textObj.AddComponent<Text>();
        text.text = content;
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = textColor;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (text.font == null)
        {
            text.font = Font.CreateDynamicFontFromOSFont("Arial", fontSize);
        }
        
        return text;
    }

    /// <summary>
    /// Helper: Create button
    /// </summary>
    void CreateButton(string name, Transform parent, string label, Vector2 position, UnityEngine.Events.UnityAction onClick)
    {
        GameObject buttonObj = new GameObject(name);
        buttonObj.transform.SetParent(parent, false);
        
        RectTransform rt = buttonObj.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(200, 50);
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = position;
        
        Image image = buttonObj.AddComponent<Image>();
        image.color = buttonColor;
        
        Button button = buttonObj.AddComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(onClick);
        
        // Button text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);
        
        RectTransform textRt = textObj.AddComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.offsetMin = Vector2.zero;
        textRt.offsetMax = Vector2.zero;
        
        Text text = textObj.AddComponent<Text>();
        text.text = label;
        text.fontSize = 24;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (text.font == null)
        {
            text.font = Font.CreateDynamicFontFromOSFont("Arial", 24);
        }
    }

    /// <summary>
    /// Helper: Create health bar
    /// </summary>
    Slider CreateHealthBar(Transform parent)
    {
        GameObject sliderObj = new GameObject("HealthBar");
        sliderObj.transform.SetParent(parent, false);
        
        RectTransform rt = sliderObj.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(150, 20);
        rt.anchorMin = new Vector2(1, 1);
        rt.anchorMax = new Vector2(1, 1);
        rt.anchoredPosition = new Vector2(-95, -20);
        
        Slider slider = sliderObj.AddComponent<Slider>();
        slider.minValue = 0;
        slider.maxValue = 100;
        slider.value = 100;
        slider.interactable = false;
        
        // Background
        GameObject background = new GameObject("Background");
        background.transform.SetParent(sliderObj.transform, false);
        RectTransform bgRt = background.AddComponent<RectTransform>();
        bgRt.anchorMin = Vector2.zero;
        bgRt.anchorMax = Vector2.one;
        bgRt.offsetMin = Vector2.zero;
        bgRt.offsetMax = Vector2.zero;
        Image bgImage = background.AddComponent<Image>();
        bgImage.color = new Color(0.2f, 0.2f, 0.2f, 1f);
        
        // Fill area
        GameObject fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(sliderObj.transform, false);
        RectTransform fillAreaRt = fillArea.AddComponent<RectTransform>();
        fillAreaRt.anchorMin = Vector2.zero;
        fillAreaRt.anchorMax = Vector2.one;
        fillAreaRt.offsetMin = new Vector2(5, 5);
        fillAreaRt.offsetMax = new Vector2(-5, -5);
        
        // Fill
        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(fillArea.transform, false);
        RectTransform fillRt = fill.AddComponent<RectTransform>();
        fillRt.anchorMin = Vector2.zero;
        fillRt.anchorMax = new Vector2(0, 1);
        fillRt.offsetMin = Vector2.zero;
        fillRt.offsetMax = Vector2.zero;
        Image fillImage = fill.AddComponent<Image>();
        fillImage.color = Color.green;
        
        slider.fillRect = fillRt;
        
        return slider;
    }
}

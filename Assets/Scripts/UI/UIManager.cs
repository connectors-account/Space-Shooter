using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages all UI panels: Main Menu, Game HUD, Pause Menu, Game Over screen.
/// Builds UI elements programmatically (no prefab dependencies).
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    // UI Panels
    private GameObject mainMenuPanel;
    private GameObject gameHUDPanel;
    private GameObject pauseMenuPanel;
    private GameObject gameOverPanel;

    // HUD elements
    private Text scoreText;
    private Text comboText;
    private Text healthText;
    private Text waveText;
    private GameObject healthBar;
    private Image healthBarFill;

    // Game Over elements
    private Text finalScoreText;
    private Text finalWaveText;
    private Text highScoreText;

    // Announcement
    private Text waveAnnouncementText;
    private Text powerUpText;

    private Canvas canvas;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        CreateUI();
    }

    void CreateUI()
    {
        // Create Canvas
        GameObject canvasObj = new GameObject("UICanvas");
        canvasObj.transform.SetParent(transform);
        canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        canvasObj.AddComponent<GraphicRaycaster>();

        // Create all panels
        CreateMainMenu(canvasObj.transform);
        CreateGameHUD(canvasObj.transform);
        CreatePauseMenu(canvasObj.transform);
        CreateGameOverPanel(canvasObj.transform);

        // Ensure EventSystem exists
        if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<UnityEngine.EventSystems.EventSystem>();
            es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }
    }

    // ======================== MAIN MENU ========================
    void CreateMainMenu(Transform parent)
    {
        mainMenuPanel = CreatePanel(parent, "MainMenuPanel", new Color(0, 0, 0, 0.85f));

        // Title
        CreateText(mainMenuPanel.transform, "TitleText", "SPACE SHOOTER",
            new Vector2(0, 200), 72, Color.cyan, FontStyle.Bold);

        // Subtitle
        CreateText(mainMenuPanel.transform, "SubtitleText", "Defend the Galaxy",
            new Vector2(0, 120), 28, new Color(0.7f, 0.7f, 0.8f), FontStyle.Italic);

        // Start Button
        CreateButton(mainMenuPanel.transform, "StartButton", "START GAME",
            new Vector2(0, -20), new Vector2(300, 60), Color.green, () =>
            {
                if (AudioManager.Instance != null) AudioManager.Instance.PlayMenuSelect();
                if (GameManager.Instance != null) GameManager.Instance.StartGame();
            });

        // Quit Button
        CreateButton(mainMenuPanel.transform, "QuitButton", "QUIT",
            new Vector2(0, -100), new Vector2(300, 60), Color.red, () =>
            {
                if (AudioManager.Instance != null) AudioManager.Instance.PlayMenuSelect();
                if (GameManager.Instance != null) GameManager.Instance.QuitGame();
            });

        // Controls info
        CreateText(mainMenuPanel.transform, "ControlsText",
            "Controls: WASD/Arrows = Move | Space = Shoot | Esc = Pause",
            new Vector2(0, -250), 20, new Color(0.6f, 0.6f, 0.6f), FontStyle.Normal);

        // High Score
        int hs = PlayerPrefs.GetInt("HighScore", 0);
        CreateText(mainMenuPanel.transform, "HighScoreMenuText",
            $"High Score: {hs}",
            new Vector2(0, -300), 24, Color.yellow, FontStyle.Normal);
    }

    // ======================== GAME HUD ========================
    void CreateGameHUD(Transform parent)
    {
        gameHUDPanel = CreatePanel(parent, "GameHUDPanel", Color.clear);

        // Score (top-left)
        scoreText = CreateText(gameHUDPanel.transform, "ScoreText", "Score: 0",
            new Vector2(-750, 480), 32, Color.white, FontStyle.Bold).GetComponent<Text>();
        scoreText.alignment = TextAnchor.UpperLeft;

        // Combo (below score)
        comboText = CreateText(gameHUDPanel.transform, "ComboText", "",
            new Vector2(-750, 440), 24, Color.yellow, FontStyle.Bold).GetComponent<Text>();
        comboText.alignment = TextAnchor.UpperLeft;

        // Wave (top-center)
        waveText = CreateText(gameHUDPanel.transform, "WaveText", "Wave: 1",
            new Vector2(0, 480), 32, Color.cyan, FontStyle.Bold).GetComponent<Text>();

        // Health bar (top-right)
        healthText = CreateText(gameHUDPanel.transform, "HealthText", "HP",
            new Vector2(580, 490), 24, Color.white, FontStyle.Bold).GetComponent<Text>();

        // Health bar background
        GameObject healthBg = CreateUIImage(gameHUDPanel.transform, "HealthBarBG",
            new Vector2(750, 490), new Vector2(250, 25), new Color(0.3f, 0, 0));

        // Health bar fill
        healthBar = CreateUIImage(gameHUDPanel.transform, "HealthBarFill",
            new Vector2(750, 490), new Vector2(250, 25), Color.red);
        healthBarFill = healthBar.GetComponent<Image>();

        // Wave announcement (center, large)
        waveAnnouncementText = CreateText(gameHUDPanel.transform, "WaveAnnouncement", "",
            new Vector2(0, 100), 56, Color.white, FontStyle.Bold).GetComponent<Text>();

        // Power-up pickup text
        powerUpText = CreateText(gameHUDPanel.transform, "PowerUpText", "",
            new Vector2(0, -200), 30, Color.yellow, FontStyle.Bold).GetComponent<Text>();

        gameHUDPanel.SetActive(false);
    }

    // ======================== PAUSE MENU ========================
    void CreatePauseMenu(Transform parent)
    {
        pauseMenuPanel = CreatePanel(parent, "PauseMenuPanel", new Color(0, 0, 0, 0.7f));

        CreateText(pauseMenuPanel.transform, "PausedText", "PAUSED",
            new Vector2(0, 150), 64, Color.white, FontStyle.Bold);

        CreateButton(pauseMenuPanel.transform, "ResumeButton", "RESUME",
            new Vector2(0, 20), new Vector2(300, 60), Color.green, () =>
            {
                if (AudioManager.Instance != null) AudioManager.Instance.PlayMenuSelect();
                if (GameManager.Instance != null) GameManager.Instance.ResumeGame();
            });

        CreateButton(pauseMenuPanel.transform, "MenuButton", "MAIN MENU",
            new Vector2(0, -60), new Vector2(300, 60), Color.yellow, () =>
            {
                if (AudioManager.Instance != null) AudioManager.Instance.PlayMenuSelect();
                if (GameManager.Instance != null) GameManager.Instance.ReturnToMenu();
            });

        CreateButton(pauseMenuPanel.transform, "QuitPauseButton", "QUIT GAME",
            new Vector2(0, -140), new Vector2(300, 60), Color.red, () =>
            {
                if (AudioManager.Instance != null) AudioManager.Instance.PlayMenuSelect();
                if (GameManager.Instance != null) GameManager.Instance.QuitGame();
            });

        pauseMenuPanel.SetActive(false);
    }

    // ======================== GAME OVER ========================
    void CreateGameOverPanel(Transform parent)
    {
        gameOverPanel = CreatePanel(parent, "GameOverPanel", new Color(0, 0, 0, 0.85f));

        CreateText(gameOverPanel.transform, "GameOverTitle", "GAME OVER",
            new Vector2(0, 200), 72, Color.red, FontStyle.Bold);

        finalScoreText = CreateText(gameOverPanel.transform, "FinalScore", "Score: 0",
            new Vector2(0, 100), 40, Color.white, FontStyle.Normal).GetComponent<Text>();

        finalWaveText = CreateText(gameOverPanel.transform, "FinalWave", "Wave: 0",
            new Vector2(0, 50), 32, Color.cyan, FontStyle.Normal).GetComponent<Text>();

        highScoreText = CreateText(gameOverPanel.transform, "HighScore", "High Score: 0",
            new Vector2(0, -10), 28, Color.yellow, FontStyle.Normal).GetComponent<Text>();

        CreateButton(gameOverPanel.transform, "RestartButton", "PLAY AGAIN",
            new Vector2(0, -100), new Vector2(300, 60), Color.green, () =>
            {
                if (AudioManager.Instance != null) AudioManager.Instance.PlayMenuSelect();
                if (GameManager.Instance != null) GameManager.Instance.StartGame();
            });

        CreateButton(gameOverPanel.transform, "MenuGameOverButton", "MAIN MENU",
            new Vector2(0, -180), new Vector2(300, 60), Color.yellow, () =>
            {
                if (AudioManager.Instance != null) AudioManager.Instance.PlayMenuSelect();
                if (GameManager.Instance != null) GameManager.Instance.ReturnToMenu();
            });

        gameOverPanel.SetActive(false);
    }

    // ======================== PUBLIC METHODS ========================
    public void ShowMainMenu()
    {
        mainMenuPanel.SetActive(true);
        gameHUDPanel.SetActive(false);
        pauseMenuPanel.SetActive(false);
        gameOverPanel.SetActive(false);
    }

    public void ShowGameHUD()
    {
        mainMenuPanel.SetActive(false);
        gameHUDPanel.SetActive(true);
        pauseMenuPanel.SetActive(false);
        gameOverPanel.SetActive(false);
    }

    public void ShowPauseMenu()
    {
        pauseMenuPanel.SetActive(true);
    }

    public void ShowGameOver(int score, int wave)
    {
        gameHUDPanel.SetActive(false);
        gameOverPanel.SetActive(true);

        if (finalScoreText != null) finalScoreText.text = $"Score: {score}";
        if (finalWaveText != null) finalWaveText.text = $"Reached Wave: {wave}";
        int hs = ScoreManager.Instance != null ? ScoreManager.Instance.GetHighScore() : 0;
        if (highScoreText != null) highScoreText.text = $"High Score: {hs}";
    }

    public void UpdateScore(int score, int combo)
    {
        if (scoreText != null) scoreText.text = $"Score: {score}";
        if (comboText != null)
        {
            comboText.text = combo > 1 ? $"Combo x{combo}!" : "";
            comboText.color = combo >= 4 ? Color.red : Color.yellow;
        }
    }

    public void UpdateHealth(int current, int max)
    {
        if (healthText != null) healthText.text = $"HP: {current}/{max}";
        if (healthBarFill != null)
        {
            float pct = (float)current / max;
            healthBarFill.fillAmount = pct;
            healthBarFill.color = pct > 0.5f ? Color.green : (pct > 0.25f ? Color.yellow : Color.red);
        }
    }

    public void UpdateWave(int wave)
    {
        if (waveText != null) waveText.text = $"Wave: {wave}";
    }

    public void ShowWaveAnnouncement(int wave)
    {
        if (waveAnnouncementText != null)
        {
            bool isBoss = wave % 5 == 0;
            waveAnnouncementText.text = isBoss ? $"!! BOSS WAVE {wave} !!" : $"Wave {wave}";
            waveAnnouncementText.color = isBoss ? Color.red : Color.white;
            CancelInvoke(nameof(HideWaveAnnouncement));
            Invoke(nameof(HideWaveAnnouncement), 2f);
        }
    }

    void HideWaveAnnouncement()
    {
        if (waveAnnouncementText != null)
            waveAnnouncementText.text = "";
    }

    public void ShowPowerUpText(string text)
    {
        if (powerUpText != null)
        {
            powerUpText.text = $"+{text}!";
            CancelInvoke(nameof(HidePowerUpText));
            Invoke(nameof(HidePowerUpText), 1.5f);
        }
    }

    void HidePowerUpText()
    {
        if (powerUpText != null)
            powerUpText.text = "";
    }

    // ======================== UI HELPERS ========================
    GameObject CreatePanel(Transform parent, string name, Color bgColor)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent, false);
        RectTransform rt = panel.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        Image img = panel.AddComponent<Image>();
        img.color = bgColor;
        img.raycastTarget = (bgColor.a > 0);

        return panel;
    }

    GameObject CreateText(Transform parent, string name, string content,
        Vector2 position, int fontSize, Color color, FontStyle style)
    {
        GameObject textObj = new GameObject(name);
        textObj.transform.SetParent(parent, false);
        RectTransform rt = textObj.AddComponent<RectTransform>();
        rt.anchoredPosition = position;
        rt.sizeDelta = new Vector2(800, 100);

        Text text = textObj.AddComponent<Text>();
        text.text = content;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (text.font == null)
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.fontSize = fontSize;
        text.color = color;
        text.fontStyle = style;
        text.alignment = TextAnchor.MiddleCenter;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;

        return textObj;
    }

    void CreateButton(Transform parent, string name, string label,
        Vector2 position, Vector2 size, Color textColor, UnityEngine.Events.UnityAction onClick)
    {
        GameObject btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent, false);
        RectTransform rt = btnObj.AddComponent<RectTransform>();
        rt.anchoredPosition = position;
        rt.sizeDelta = size;

        Image img = btnObj.AddComponent<Image>();
        img.color = new Color(0.15f, 0.15f, 0.2f, 0.9f);

        Button btn = btnObj.AddComponent<Button>();
        ColorBlock cb = btn.colors;
        cb.highlightedColor = new Color(0.3f, 0.3f, 0.4f);
        cb.pressedColor = new Color(0.1f, 0.1f, 0.15f);
        btn.colors = cb;
        btn.onClick.AddListener(onClick);

        // Button text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(btnObj.transform, false);
        RectTransform trt = textObj.AddComponent<RectTransform>();
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.offsetMin = Vector2.zero;
        trt.offsetMax = Vector2.zero;

        Text text = textObj.AddComponent<Text>();
        text.text = label;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (text.font == null)
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.fontSize = 28;
        text.color = textColor;
        text.fontStyle = FontStyle.Bold;
        text.alignment = TextAnchor.MiddleCenter;
    }

    GameObject CreateUIImage(Transform parent, string name,
        Vector2 position, Vector2 size, Color color)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        RectTransform rt = obj.AddComponent<RectTransform>();
        rt.anchoredPosition = position;
        rt.sizeDelta = size;

        Image img = obj.AddComponent<Image>();
        img.color = color;
        img.type = Image.Type.Filled;
        img.fillMethod = Image.FillMethod.Horizontal;

        return obj;
    }
}

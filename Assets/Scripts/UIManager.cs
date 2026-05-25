using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Manages all UI elements: Main Menu, Game HUD, Game Over screen, and notifications.
/// Creates all UI elements programmatically — no prefab dependencies.
/// </summary>
public class UIManager : MonoBehaviour
{
    // UI Panels
    private GameObject mainMenuPanel;
    private GameObject gameHUDPanel;
    private GameObject gameOverPanel;

    // HUD elements
    private Text scoreText;
    private Text waveText;
    private Text healthText;
    private Text powerUpNotification;

    // Main Menu elements
    private Text titleText;
    private Text highScoreMenuText;

    // Game Over elements
    private Text gameOverTitle;
    private Text finalScoreText;
    private Text highScoreText;

    // Canvas
    private Canvas canvas;

    private void Awake()
    {
        CreateUI();
    }

    /// <summary>
    /// Builds the entire UI hierarchy in code.
    /// </summary>
    private void CreateUI()
    {
        // --- Canvas ---
        GameObject canvasObj = new GameObject("GameCanvas");
        canvasObj.transform.SetParent(transform);
        canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObj.AddComponent<GraphicRaycaster>();

        // Event System (needed for button clicks)
        if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        // --- Main Menu Panel ---
        mainMenuPanel = CreatePanel(canvasObj.transform, "MainMenuPanel", new Color(0.05f, 0.05f, 0.15f, 0.95f));
        titleText = CreateText(mainMenuPanel.transform, "TitleText", "SPACE SHOOTER", 72,
            new Vector2(0, 150), new Vector2(800, 100), TextAnchor.MiddleCenter, Color.cyan);
        highScoreMenuText = CreateText(mainMenuPanel.transform, "HighScoreMenuText", "HIGH SCORE: 0", 28,
            new Vector2(0, 50), new Vector2(600, 50), TextAnchor.MiddleCenter, Color.yellow);
        CreateButton(mainMenuPanel.transform, "PlayButton", "PLAY", new Vector2(0, -40),
            new Vector2(300, 60), Color.green, () => GameManager.Instance?.StartGame());
        CreateButton(mainMenuPanel.transform, "QuitButton", "QUIT", new Vector2(0, -120),
            new Vector2(300, 60), Color.red, () => GameManager.Instance?.QuitGame());
        CreateText(mainMenuPanel.transform, "ControlsText",
            "CONTROLS: Arrow Keys / WASD to Move  |  SPACE to Shoot", 20,
            new Vector2(0, -220), new Vector2(800, 40), TextAnchor.MiddleCenter, new Color(0.7f, 0.7f, 0.7f));

        // --- Game HUD Panel ---
        gameHUDPanel = CreatePanel(canvasObj.transform, "GameHUDPanel", Color.clear);
        scoreText = CreateText(gameHUDPanel.transform, "ScoreText", "SCORE: 0", 32,
            new Vector2(-750, 490), new Vector2(400, 50), TextAnchor.UpperLeft, Color.white);
        waveText = CreateText(gameHUDPanel.transform, "WaveText", "WAVE: 1", 28,
            new Vector2(0, 490), new Vector2(300, 50), TextAnchor.UpperCenter, Color.yellow);
        healthText = CreateText(gameHUDPanel.transform, "HealthText", "HEALTH: ♥♥♥♥♥", 28,
            new Vector2(750, 490), new Vector2(400, 50), TextAnchor.UpperRight, Color.red);
        powerUpNotification = CreateText(gameHUDPanel.transform, "PowerUpText", "", 36,
            new Vector2(0, 200), new Vector2(600, 60), TextAnchor.MiddleCenter, Color.yellow);

        // --- Game Over Panel ---
        gameOverPanel = CreatePanel(canvasObj.transform, "GameOverPanel", new Color(0.1f, 0.02f, 0.02f, 0.92f));
        gameOverTitle = CreateText(gameOverPanel.transform, "GameOverTitle", "GAME OVER", 80,
            new Vector2(0, 180), new Vector2(800, 100), TextAnchor.MiddleCenter, Color.red);
        finalScoreText = CreateText(gameOverPanel.transform, "FinalScoreText", "SCORE: 0", 40,
            new Vector2(0, 70), new Vector2(600, 60), TextAnchor.MiddleCenter, Color.white);
        highScoreText = CreateText(gameOverPanel.transform, "HighScoreText", "HIGH SCORE: 0", 30,
            new Vector2(0, 10), new Vector2(600, 50), TextAnchor.MiddleCenter, Color.yellow);
        CreateButton(gameOverPanel.transform, "RestartButton", "PLAY AGAIN", new Vector2(0, -80),
            new Vector2(300, 60), Color.green, () => GameManager.Instance?.RestartGame());
        CreateButton(gameOverPanel.transform, "MenuButton", "MAIN MENU", new Vector2(0, -160),
            new Vector2(300, 60), new Color(0.3f, 0.5f, 1f), () => GameManager.Instance?.ReturnToMainMenu());

        // Hide all panels initially
        mainMenuPanel.SetActive(false);
        gameHUDPanel.SetActive(false);
        gameOverPanel.SetActive(false);
    }

    // ==================== Public API ====================

    public void ShowMainMenu(int highScore)
    {
        mainMenuPanel.SetActive(true);
        gameHUDPanel.SetActive(false);
        gameOverPanel.SetActive(false);
        highScoreMenuText.text = $"HIGH SCORE: {highScore}";
    }

    public void ShowGameHUD()
    {
        mainMenuPanel.SetActive(false);
        gameHUDPanel.SetActive(true);
        gameOverPanel.SetActive(false);
    }

    public void ShowGameOver(int score, int highScore)
    {
        mainMenuPanel.SetActive(false);
        gameHUDPanel.SetActive(false);
        gameOverPanel.SetActive(true);
        finalScoreText.text = $"SCORE: {score}";
        highScoreText.text = $"HIGH SCORE: {highScore}";
    }

    public void UpdateScore(int score)
    {
        if (scoreText != null) scoreText.text = $"SCORE: {score}";
    }

    public void UpdateWave(int wave)
    {
        if (waveText != null) waveText.text = $"WAVE: {wave}";
    }

    public void UpdateHealth(int current, int max)
    {
        if (healthText != null)
        {
            string hearts = "";
            for (int i = 0; i < max; i++)
            {
                hearts += i < current ? "♥ " : "♡ ";
            }
            healthText.text = hearts.Trim();
        }
    }

    public void ShowPowerUpText(string text)
    {
        if (powerUpNotification != null)
        {
            StopCoroutine("FadePowerUpText");
            StartCoroutine(FadePowerUpText(text));
        }
    }

    private IEnumerator FadePowerUpText(string text)
    {
        powerUpNotification.text = text;
        Color c = Color.yellow;
        powerUpNotification.color = c;

        float duration = 2f;
        float timer = 0;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            c.a = Mathf.Lerp(1f, 0f, timer / duration);
            powerUpNotification.color = c;
            yield return null;
        }

        powerUpNotification.text = "";
    }

    // ==================== UI Factory Helpers ====================

    private GameObject CreatePanel(Transform parent, string name, Color bgColor)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent, false);

        RectTransform rt = panel.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        if (bgColor.a > 0)
        {
            Image img = panel.AddComponent<Image>();
            img.color = bgColor;
        }

        return panel;
    }

    private Text CreateText(Transform parent, string name, string content, int fontSize,
        Vector2 anchoredPos, Vector2 size, TextAnchor alignment, Color color)
    {
        GameObject textObj = new GameObject(name);
        textObj.transform.SetParent(parent, false);

        RectTransform rt = textObj.AddComponent<RectTransform>();
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;

        Text text = textObj.AddComponent<Text>();
        text.text = content;
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = color;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (text.font == null)
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Overflow;

        // Add outline for readability
        Outline outline = textObj.AddComponent<Outline>();
        outline.effectColor = Color.black;
        outline.effectDistance = new Vector2(2, -2);

        return text;
    }

    private void CreateButton(Transform parent, string name, string label,
        Vector2 anchoredPos, Vector2 size, Color btnColor, UnityEngine.Events.UnityAction onClick)
    {
        GameObject buttonObj = new GameObject(name);
        buttonObj.transform.SetParent(parent, false);

        RectTransform rt = buttonObj.AddComponent<RectTransform>();
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;

        Image img = buttonObj.AddComponent<Image>();
        img.color = new Color(btnColor.r * 0.3f, btnColor.g * 0.3f, btnColor.b * 0.3f, 0.8f);

        Button btn = buttonObj.AddComponent<Button>();
        ColorBlock colors = btn.colors;
        colors.highlightedColor = new Color(btnColor.r * 0.5f, btnColor.g * 0.5f, btnColor.b * 0.5f, 1f);
        colors.pressedColor = btnColor;
        btn.colors = colors;
        btn.onClick.AddListener(onClick);

        // Button text
        Text btnText = CreateText(buttonObj.transform, name + "Text", label, 28,
            Vector2.zero, size, TextAnchor.MiddleCenter, Color.white);
    }
}

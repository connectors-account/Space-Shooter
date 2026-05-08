using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Game Over screen with final score, high score, and restart/menu options.
/// </summary>
public class GameOverUI : MonoBehaviour
{
    private Canvas canvas;
    private GameObject panel;

    private void Start()
    {
        BuildUI();

        if (GameManager.Instance != null)
            GameManager.Instance.OnGameStateChanged += OnGameStateChanged;

        panel.SetActive(false);
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnGameStateChanged -= OnGameStateChanged;
    }

    private void OnGameStateChanged(GameState state)
    {
        if (state == GameState.GameOver)
            Show();
        else
            Hide();
    }

    private void Show()
    {
        panel.SetActive(true);

        // Update score displays
        Text finalScore = panel.transform.Find("FinalScore")?.GetComponent<Text>();
        if (finalScore != null && GameManager.Instance != null)
            finalScore.text = $"Final Score: {GameManager.Instance.Score:N0}";

        Text highScore = panel.transform.Find("HighScore")?.GetComponent<Text>();
        if (highScore != null && GameManager.Instance != null)
        {
            highScore.text = $"High Score: {GameManager.Instance.HighScore:N0}";
            if (GameManager.Instance.Score >= GameManager.Instance.HighScore)
                highScore.color = new Color(1f, 0.9f, 0.3f);
        }

        Text waveReached = panel.transform.Find("WaveReached")?.GetComponent<Text>();
        if (waveReached != null && GameManager.Instance != null)
            waveReached.text = $"Wave Reached: {GameManager.Instance.CurrentWave}";
    }

    private void Hide()
    {
        panel.SetActive(false);
    }

    private void BuildUI()
    {
        // Canvas
        GameObject canvasObj = new GameObject("GameOverCanvas");
        canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 90;
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasObj.AddComponent<GraphicRaycaster>();

        // Dark overlay panel
        panel = CreateUIElement("Panel", canvasObj.transform);
        Image panelImg = panel.AddComponent<Image>();
        panelImg.color = new Color(0f, 0f, 0f, 0.85f);
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.sizeDelta = Vector2.zero;

        // Game Over title
        CreateText("GameOverTitle", panel.transform, "GAME OVER",
            new Vector2(0, 180), new Vector2(600, 80), 64, new Color(1f, 0.2f, 0.2f));

        // Score displays
        CreateText("FinalScore", panel.transform, "Final Score: 0",
            new Vector2(0, 80), new Vector2(500, 50), 36, Color.white);

        CreateText("WaveReached", panel.transform, "Wave Reached: 1",
            new Vector2(0, 30), new Vector2(500, 40), 28, new Color(0.8f, 0.8f, 1f));

        CreateText("HighScore", panel.transform, "High Score: 0",
            new Vector2(0, -20), new Vector2(500, 40), 28, new Color(0.6f, 0.6f, 0.6f));

        // Buttons
        CreateButton("RestartBtn", "RESTART", panel.transform, new Vector2(0, -100), OnRestartClicked);
        CreateButton("MenuBtn", "MAIN MENU", panel.transform, new Vector2(0, -175), OnMenuClicked);
        CreateButton("QuitBtn", "QUIT", panel.transform, new Vector2(0, -250), OnQuitClicked);
    }

    private void OnRestartClicked()
    {
        GameManager.Instance?.RestartGame();
    }

    private void OnMenuClicked()
    {
        GameManager.Instance?.LoadMainMenu();
    }

    private void OnQuitClicked()
    {
        GameManager.Instance?.QuitGame();
    }

    // --- Helpers ---

    private void CreateText(string name, Transform parent, string content,
                           Vector2 pos, Vector2 size, int fontSize, Color color)
    {
        GameObject obj = CreateUIElement(name, parent);
        Text text = obj.AddComponent<Text>();
        text.text = content;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = fontSize;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = color;
        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.anchoredPosition = pos;
        rect.sizeDelta = size;
    }

    private void CreateButton(string name, string text, Transform parent, Vector2 position,
                              UnityEngine.Events.UnityAction onClick)
    {
        GameObject btnObj = CreateUIElement(name, parent);
        Image btnImg = btnObj.AddComponent<Image>();
        btnImg.color = new Color(0.1f, 0.15f, 0.3f, 0.9f);
        Button btn = btnObj.AddComponent<Button>();
        RectTransform btnRect = btnObj.GetComponent<RectTransform>();
        btnRect.anchoredPosition = position;
        btnRect.sizeDelta = new Vector2(300, 55);

        GameObject textObj = CreateUIElement("Text", btnObj.transform);
        Text btnText = textObj.AddComponent<Text>();
        btnText.text = text;
        btnText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        btnText.fontSize = 28;
        btnText.alignment = TextAnchor.MiddleCenter;
        btnText.color = Color.white;
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;

        ColorBlock colors = btn.colors;
        colors.normalColor = new Color(0.1f, 0.15f, 0.3f, 0.9f);
        colors.highlightedColor = new Color(0.15f, 0.25f, 0.5f, 1f);
        colors.pressedColor = new Color(0.05f, 0.1f, 0.2f, 1f);
        btn.colors = colors;
        btn.onClick.AddListener(onClick);
    }

    private GameObject CreateUIElement(string name, Transform parent)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform));
        obj.transform.SetParent(parent, false);
        return obj;
    }
}

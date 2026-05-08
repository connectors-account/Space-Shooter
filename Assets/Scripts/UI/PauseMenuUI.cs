using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Pause menu overlay. Toggled with ESC key.
/// </summary>
public class PauseMenuUI : MonoBehaviour
{
    private Canvas canvas;
    private GameObject panel;

    private void Start()
    {
        BuildUI();
        panel.SetActive(false);

        if (GameManager.Instance != null)
            GameManager.Instance.OnGameStateChanged += OnGameStateChanged;
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnGameStateChanged -= OnGameStateChanged;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && GameManager.Instance != null)
        {
            if (GameManager.Instance.CurrentState == GameState.Playing ||
                GameManager.Instance.CurrentState == GameState.Paused)
            {
                GameManager.Instance.TogglePause();
            }
        }
    }

    private void OnGameStateChanged(GameState state)
    {
        panel.SetActive(state == GameState.Paused);
    }

    private void BuildUI()
    {
        GameObject canvasObj = new GameObject("PauseCanvas");
        canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 80;
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasObj.AddComponent<GraphicRaycaster>();

        // Dark overlay
        panel = CreateUIElement("Panel", canvasObj.transform);
        Image panelImg = panel.AddComponent<Image>();
        panelImg.color = new Color(0f, 0f, 0.05f, 0.8f);
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.sizeDelta = Vector2.zero;

        // Pause title
        CreateText("PauseTitle", panel.transform, "PAUSED",
            new Vector2(0, 120), new Vector2(400, 70), 56, new Color(0.8f, 0.8f, 1f));

        // Buttons
        CreateButton("ResumeBtn", "RESUME", panel.transform, new Vector2(0, 20), OnResumeClicked);
        CreateButton("RestartBtn", "RESTART", panel.transform, new Vector2(0, -55), OnRestartClicked);
        CreateButton("MenuBtn", "MAIN MENU", panel.transform, new Vector2(0, -130), OnMenuClicked);

        // Hint
        CreateText("Hint", panel.transform, "Press ESC to resume",
            new Vector2(0, -220), new Vector2(400, 30), 20, new Color(0.5f, 0.5f, 0.6f));
    }

    private void OnResumeClicked()
    {
        GameManager.Instance?.ResumeGame();
    }

    private void OnRestartClicked()
    {
        GameManager.Instance?.RestartGame();
    }

    private void OnMenuClicked()
    {
        GameManager.Instance?.LoadMainMenu();
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

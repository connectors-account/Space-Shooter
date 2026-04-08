using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Builds the Game Over scene at runtime.
/// </summary>
public class SceneSetup_GameOver : MonoBehaviour
{
    void Start()
    {
        Camera.main.backgroundColor = new Color(0.05f, 0.02f, 0.02f);
        Camera.main.orthographic = true;
        Camera.main.orthographicSize = 5.5f;

        // Canvas
        GameObject canvasGO = new GameObject("Canvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGO.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasGO.GetComponent<CanvasScaler>().referenceResolution = new Vector2(800, 600);
        canvasGO.AddComponent<GraphicRaycaster>();

        if (UnityEngine.EventSystems.EventSystem.current == null)
        {
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<UnityEngine.EventSystems.EventSystem>();
            es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        UIManager ui = canvasGO.AddComponent<UIManager>();

        // Game Over title
        CreateText(canvasGO.transform, "GameOverTitle", "GAME OVER",
            new Vector2(0, 120), 52, Color.red, FontStyle.Bold);

        // Final score
        int score = GameManager.Instance != null ? GameManager.Instance.GetScore() : 0;
        Text scoreText = CreateText(canvasGO.transform, "FinalScore", "Score: " + score,
            new Vector2(0, 40), 32, Color.white, FontStyle.Normal);
        ui.finalScoreText = scoreText;

        // High score
        int hs = GameManager.Instance != null ? GameManager.Instance.GetHighScore() : 0;
        Text hsText = CreateText(canvasGO.transform, "HighScore", "High Score: " + hs,
            new Vector2(0, -10), 24, Color.yellow, FontStyle.Normal);
        ui.gameOverHighScoreText = hsText;

        // Restart button
        CreateButton(canvasGO.transform, "RestartBtn", "PLAY AGAIN",
            new Vector2(0, -80), new Vector2(220, 55), () => ui.OnRestartButton());

        // Main menu button
        CreateButton(canvasGO.transform, "MenuBtn", "MAIN MENU",
            new Vector2(0, -150), new Vector2(220, 55), () => ui.OnMainMenuButton());
    }

    Text CreateText(Transform parent, string name, string content,
        Vector2 anchoredPos, int fontSize, Color color, FontStyle style)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = new Vector2(700, 70);

        Text text = go.AddComponent<Text>();
        text.text = content;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (text.font == null)
            text.font = Font.CreateDynamicFontFromOSFont("Arial", fontSize);
        text.fontSize = fontSize;
        text.fontStyle = style;
        text.color = color;
        text.alignment = TextAnchor.MiddleCenter;

        return text;
    }

    void CreateButton(Transform parent, string name, string label,
        Vector2 anchoredPos, Vector2 size, UnityEngine.Events.UnityAction onClick)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;

        Image img = go.AddComponent<Image>();
        img.color = new Color(0.2f, 0.3f, 0.6f, 0.9f);

        Button btn = go.AddComponent<Button>();
        btn.targetGraphic = img;
        btn.onClick.AddListener(onClick);

        GameObject textGO = new GameObject("Label");
        textGO.transform.SetParent(go.transform, false);
        RectTransform trt = textGO.AddComponent<RectTransform>();
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.offsetMin = Vector2.zero;
        trt.offsetMax = Vector2.zero;

        Text text = textGO.AddComponent<Text>();
        text.text = label;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (text.font == null)
            text.font = Font.CreateDynamicFontFromOSFont("Arial", 26);
        text.fontSize = 26;
        text.fontStyle = FontStyle.Bold;
        text.color = Color.white;
        text.alignment = TextAnchor.MiddleCenter;
    }
}

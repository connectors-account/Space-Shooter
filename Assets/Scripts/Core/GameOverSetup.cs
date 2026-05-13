// ============================================================================
// GameOverSetup.cs - Runtime bootstrapper for the Game Over scene
// Creates all Game Over UI elements programmatically.
// ============================================================================
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Attach to an empty GameObject in the GameOver scene.
/// Creates the full game-over UI from code on Awake.
/// </summary>
public class GameOverSetup : MonoBehaviour
{
    private void Awake()
    {
        // Ensure singletons.
        if (GameManager.Instance == null)
        {
            GameObject gm = new GameObject("GameManager");
            gm.AddComponent<GameManager>();
        }
        if (AudioManager.Instance == null)
        {
            GameObject am = new GameObject("AudioManager");
            am.AddComponent<AudioManager>();
        }

        SetupCamera();
        CreateUI();
    }

    private void SetupCamera()
    {
        Camera cam = Camera.main;
        if (cam != null)
        {
            cam.orthographic = true;
            cam.orthographicSize = 6f;
            cam.backgroundColor = new Color(0.05f, 0.02f, 0.02f);
        }
    }

    private void CreateUI()
    {
        // Canvas.
        GameObject canvasObj = new GameObject("GameOverCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasObj.AddComponent<GraphicRaycaster>();

        // GameOverUI component.
        GameOverUI goUI = canvasObj.AddComponent<GameOverUI>();

        // Title.
        Text title = CreateText(canvasObj.transform, "Title", "GAME OVER",
            new Vector2(0, 250), new Vector2(800, 100), 72, Color.red, TextAnchor.MiddleCenter);
        CenterRT(title.GetComponent<RectTransform>());
        SetField(goUI, "gameOverTitle", title);

        // New high score (hidden by default).
        Text newHS = CreateText(canvasObj.transform, "NewHighScore", "★ NEW HIGH SCORE! ★",
            new Vector2(0, 160), new Vector2(600, 50), 32, Color.yellow, TextAnchor.MiddleCenter);
        CenterRT(newHS.GetComponent<RectTransform>());
        newHS.enabled = false;
        SetField(goUI, "newHighScoreText", newHS);

        // Final score.
        Text finalScore = CreateText(canvasObj.transform, "FinalScore", "Final Score: 0",
            new Vector2(0, 90), new Vector2(500, 50), 36, Color.white, TextAnchor.MiddleCenter);
        CenterRT(finalScore.GetComponent<RectTransform>());
        SetField(goUI, "finalScoreText", finalScore);

        // High score.
        Text highScore = CreateText(canvasObj.transform, "HighScore", "High Score: 0",
            new Vector2(0, 40), new Vector2(500, 40), 28, Color.gray, TextAnchor.MiddleCenter);
        CenterRT(highScore.GetComponent<RectTransform>());
        SetField(goUI, "highScoreText", highScore);

        // Wave reached.
        Text waveReached = CreateText(canvasObj.transform, "WaveReached", "Wave Reached: 1",
            new Vector2(0, -10), new Vector2(500, 40), 28, Color.gray, TextAnchor.MiddleCenter);
        CenterRT(waveReached.GetComponent<RectTransform>());
        SetField(goUI, "waveReachedText", waveReached);

        // Retry button.
        Button retryBtn = CreateButton(canvasObj.transform, "RetryButton", "PLAY AGAIN",
            new Vector2(0, -100), new Vector2(300, 60), 28);
        SetField(goUI, "retryButton", retryBtn);

        // Main Menu button.
        Button menuBtn = CreateButton(canvasObj.transform, "MainMenuButton", "MAIN MENU",
            new Vector2(0, -180), new Vector2(300, 60), 28);
        SetField(goUI, "mainMenuButton", menuBtn);
    }

    // ---- UI Helpers ----

    private Text CreateText(Transform parent, string name, string content, Vector2 pos, Vector2 size, int fontSize, Color color, TextAnchor anchor)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        RectTransform rt = obj.AddComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        Text t = obj.AddComponent<Text>();
        t.text = content;
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (t.font == null) t.font = Font.CreateDynamicFontFromOSFont("Arial", fontSize);
        t.fontSize = fontSize;
        t.color = color;
        t.alignment = anchor;
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        return t;
    }

    private Button CreateButton(Transform parent, string name, string label, Vector2 pos, Vector2 size, int fontSize)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        RectTransform rt = obj.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;

        Image img = obj.AddComponent<Image>();
        img.color = new Color(0.4f, 0.1f, 0.1f);

        Button btn = obj.AddComponent<Button>();
        ColorBlock cb = btn.colors;
        cb.normalColor = new Color(0.4f, 0.1f, 0.1f);
        cb.highlightedColor = new Color(0.6f, 0.2f, 0.2f);
        cb.pressedColor = new Color(0.3f, 0.05f, 0.05f);
        btn.colors = cb;

        Text txt = CreateText(obj.transform, "Label", label, Vector2.zero, size, fontSize, Color.white, TextAnchor.MiddleCenter);
        RectTransform txtRT = txt.GetComponent<RectTransform>();
        txtRT.anchorMin = Vector2.zero;
        txtRT.anchorMax = Vector2.one;
        txtRT.offsetMin = Vector2.zero;
        txtRT.offsetMax = Vector2.zero;

        return btn;
    }

    private void CenterRT(RectTransform rt)
    {
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
    }

    private void SetField(object target, string fieldName, object value)
    {
        var field = target.GetType().GetField(fieldName,
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field != null) field.SetValue(target, value);
    }
}

// ============================================================================
// MainMenuSetup.cs - Runtime bootstrapper for the Main Menu scene
// Creates all menu UI elements programmatically.
// ============================================================================
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Attach to an empty GameObject in the MainMenu scene.
/// Creates the full main menu UI from code on Awake.
/// </summary>
public class MainMenuSetup : MonoBehaviour
{
    private void Awake()
    {
        // Ensure singletons exist.
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
        CreateBackground();
        CreateMenuUI();
    }

    private void SetupCamera()
    {
        Camera cam = Camera.main;
        if (cam != null)
        {
            cam.orthographic = true;
            cam.orthographicSize = 6f;
            cam.backgroundColor = new Color(0.02f, 0.02f, 0.08f);
        }
    }

    private void CreateBackground()
    {
        GameObject bg = new GameObject("MenuBackground");
        bg.AddComponent<BackgroundSetup>();
    }

    private void CreateMenuUI()
    {
        // Canvas.
        GameObject canvasObj = new GameObject("MenuCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasObj.AddComponent<GraphicRaycaster>();

        // Add MainMenuUI component.
        MainMenuUI menuUI = canvasObj.AddComponent<MainMenuUI>();

        // Title.
        Text title = CreateText(canvasObj.transform, "Title", "SPACE SHOOTER",
            new Vector2(0, 200), new Vector2(800, 100), 72, Color.cyan, TextAnchor.MiddleCenter);
        CenterRT(title.GetComponent<RectTransform>());
        SetField(menuUI, "titleText", title);

        // Subtitle / instructions.
        Text instructions = CreateText(canvasObj.transform, "Instructions",
            "WASD / Arrow Keys - Move\nSpace / Left Click - Fire\nP / Esc - Pause",
            new Vector2(0, 50), new Vector2(600, 100), 22, new Color(0.7f, 0.7f, 0.7f), TextAnchor.MiddleCenter);
        CenterRT(instructions.GetComponent<RectTransform>());
        SetField(menuUI, "instructionsText", instructions);

        // High score display.
        Text highScore = CreateText(canvasObj.transform, "HighScore", "High Score: 0",
            new Vector2(0, -30), new Vector2(400, 40), 26, Color.yellow, TextAnchor.MiddleCenter);
        CenterRT(highScore.GetComponent<RectTransform>());
        SetField(menuUI, "highScoreText", highScore);

        // Start button.
        Button startBtn = CreateButton(canvasObj.transform, "StartButton", "START GAME",
            new Vector2(0, -110), new Vector2(300, 60), 30);
        SetField(menuUI, "startButton", startBtn);

        // Quit button.
        Button quitBtn = CreateButton(canvasObj.transform, "QuitButton", "QUIT",
            new Vector2(0, -190), new Vector2(300, 60), 30);
        SetField(menuUI, "quitButton", quitBtn);

        // Version text.
        CreateText(canvasObj.transform, "Version", "v1.0.0",
            new Vector2(10, 10), new Vector2(200, 30), 16, new Color(0.4f, 0.4f, 0.4f), TextAnchor.LowerLeft);
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
        img.color = new Color(0.1f, 0.3f, 0.6f);

        Button btn = obj.AddComponent<Button>();
        ColorBlock cb = btn.colors;
        cb.normalColor = new Color(0.1f, 0.3f, 0.6f);
        cb.highlightedColor = new Color(0.2f, 0.5f, 0.8f);
        cb.pressedColor = new Color(0.05f, 0.2f, 0.4f);
        btn.colors = cb;

        // Label.
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

using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Automatically builds the Main Menu scene at runtime.
/// Attach to a single empty GameObject in the MainMenu scene.
/// </summary>
public class MainMenuSetup : MonoBehaviour
{
    private void Awake()
    {
        SetupCamera();
        CreateBackground();
        CreateUI();
        EnsureManagers();
    }

    private void SetupCamera()
    {
        Camera cam = Camera.main;
        if (cam != null)
        {
            cam.backgroundColor = new Color(0.01f, 0.01f, 0.06f);
            cam.orthographic = true;
            cam.orthographicSize = 5.5f;
        }
    }

    private void CreateBackground()
    {
        GameObject bg = new GameObject("MenuBackground");
        bg.AddComponent<ParallaxBackground>();
    }

    private void CreateUI()
    {
        // Canvas
        GameObject canvasObj = new GameObject("MenuCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        canvasObj.AddComponent<GraphicRaycaster>();

        // Main Menu Controller
        MainMenuController menuCtrl = canvasObj.AddComponent<MainMenuController>();

        // ─── Title ───
        Text titleText = CreateText(canvasObj.transform, "Title",
            "STELLAR VANGUARD", TextAnchor.MiddleCenter,
            new Vector2(0, 180), new Vector2(800, 100), 64,
            new Color(0.3f, 0.8f, 1f));

        // Subtitle
        CreateText(canvasObj.transform, "Subtitle",
            "SPACE SHOOTER", TextAnchor.MiddleCenter,
            new Vector2(0, 120), new Vector2(600, 50), 28,
            new Color(0.6f, 0.6f, 0.8f));

        // ─── High Score ───
        Text highScoreText = CreateText(canvasObj.transform, "HighScore",
            "", TextAnchor.MiddleCenter,
            new Vector2(0, 60), new Vector2(400, 40), 24, Color.yellow);

        // ─── Play Button ───
        Button playButton = CreateMenuButton(canvasObj.transform, "PlayButton",
            "▶  PLAY", new Vector2(0, -20), new Vector2(300, 60),
            new Color(0.1f, 0.5f, 0.2f, 0.9f));

        // ─── Quit Button ───
        Button quitButton = CreateMenuButton(canvasObj.transform, "QuitButton",
            "✕  QUIT", new Vector2(0, -100), new Vector2(300, 60),
            new Color(0.5f, 0.1f, 0.1f, 0.9f));

        // ─── Controls Info ───
        Text controlsText = CreateText(canvasObj.transform, "Controls",
            "WASD / Arrow Keys - Move\nSpace - Shoot\nESC - Pause",
            TextAnchor.MiddleCenter, new Vector2(0, -210),
            new Vector2(500, 80), 18, new Color(0.5f, 0.5f, 0.6f));

        // ─── Version ───
        Text versionText = CreateText(canvasObj.transform, "Version",
            "v1.0", TextAnchor.LowerRight, new Vector2(-15, 10),
            new Vector2(100, 25), 14, new Color(0.3f, 0.3f, 0.4f));

        // ─── Wire references ───
        SetField(menuCtrl, "titleText", titleText);
        SetField(menuCtrl, "highScoreText", highScoreText);
        SetField(menuCtrl, "playButton", playButton);
        SetField(menuCtrl, "quitButton", quitButton);
        SetField(menuCtrl, "controlsText", controlsText);
        SetField(menuCtrl, "versionText", versionText);
    }

    private void EnsureManagers()
    {
        if (GameManager.Instance == null)
        {
            GameObject gm = new GameObject("GameManager");
            gm.AddComponent<GameManager>();
        }
        if (ScoreManager.Instance == null)
        {
            GameObject sm = new GameObject("ScoreManager");
            sm.AddComponent<ScoreManager>();
        }
        if (AudioManager.Instance == null)
        {
            GameObject am = new GameObject("AudioManager");
            am.AddComponent<AudioManager>();
        }
    }

    // ────────── UI HELPERS ──────────

    private Text CreateText(Transform parent, string name, string content,
        TextAnchor alignment, Vector2 position, Vector2 size, int fontSize, Color color)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);

        RectTransform rt = obj.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = position;
        rt.sizeDelta = size;

        // Handle corner-anchored text
        if (alignment == TextAnchor.LowerRight)
        {
            rt.anchorMin = new Vector2(1, 0);
            rt.anchorMax = new Vector2(1, 0);
            rt.pivot = new Vector2(1, 0);
        }

        Text text = obj.AddComponent<Text>();
        text.text = content;
        text.fontSize = fontSize;
        text.color = color;
        text.alignment = alignment;
        text.font = Font.CreateDynamicFontFromOSFont("Arial", fontSize);
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Overflow;

        Outline outline = obj.AddComponent<Outline>();
        outline.effectColor = new Color(0, 0, 0, 0.8f);
        outline.effectDistance = new Vector2(2, -2);

        return text;
    }

    private Button CreateMenuButton(Transform parent, string name, string label,
        Vector2 position, Vector2 size, Color bgColor)
    {
        GameObject btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent, false);

        RectTransform rt = btnObj.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = position;
        rt.sizeDelta = size;

        Image img = btnObj.AddComponent<Image>();
        img.color = bgColor;

        Button btn = btnObj.AddComponent<Button>();
        ColorBlock colors = btn.colors;
        colors.normalColor = bgColor;
        colors.highlightedColor = bgColor * 1.3f;
        colors.pressedColor = bgColor * 0.7f;
        colors.highlightedColor = new Color(
            Mathf.Min(bgColor.r * 1.3f, 1f),
            Mathf.Min(bgColor.g * 1.3f, 1f),
            Mathf.Min(bgColor.b * 1.3f, 1f),
            1f);
        btn.colors = colors;

        // Label
        Text text = CreateText(btnObj.transform, "Label", label,
            TextAnchor.MiddleCenter, Vector2.zero, size, 28, Color.white);
        RectTransform textRT = text.GetComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.offsetMin = Vector2.zero;
        textRT.offsetMax = Vector2.zero;

        return btn;
    }

    private void SetField(object target, string fieldName, object value)
    {
        var field = target.GetType().GetField(fieldName,
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Instance);
        if (field != null) field.SetValue(target, value);
    }
}

using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Bootstraps the MainMenu scene by creating all UI elements at runtime.
/// </summary>
public class MainMenuBootstrap : MonoBehaviour
{
    void Awake()
    {
        SetupCamera();
        CreateGameManager();
        CreateBackground();
        CreateUI();
    }

    void SetupCamera()
    {
        Camera.main.orthographic = true;
        Camera.main.orthographicSize = 5f;
        Camera.main.backgroundColor = new Color(0.02f, 0.02f, 0.08f);
        Camera.main.transform.position = new Vector3(0, 0, -10);
    }

    void CreateGameManager()
    {
        if (GameManager.Instance == null)
        {
            GameObject gmObj = new GameObject("GameManager");
            gmObj.AddComponent<GameManager>();
        }
    }

    void CreateBackground()
    {
        GameObject starfield = new GameObject("MenuStarfield");
        StarfieldGenerator sfg = starfield.AddComponent<StarfieldGenerator>();
        sfg.starCount = 150;
        sfg.scrollSpeed = 0.3f;
    }

    void CreateUI()
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

        MainMenuUI menuUI = canvasObj.AddComponent<MainMenuUI>();

        // Title
        menuUI.titleText = CreateText(canvasObj.transform, "TitleText", "SPACE SHOOTER",
            new Vector2(0.5f, 0.75f), 72, Color.cyan);
        
        // Subtitle
        CreateText(canvasObj.transform, "SubtitleText", "Defend the Galaxy",
            new Vector2(0.5f, 0.65f), 28, new Color(0.7f, 0.7f, 1f));

        // High Score
        menuUI.highScoreText = CreateText(canvasObj.transform, "HighScoreText",
            "HIGH SCORE: " + PlayerPrefs.GetInt("HighScore", 0),
            new Vector2(0.5f, 0.55f), 24, Color.yellow);

        // Play Button
        menuUI.playButton = CreateButton(canvasObj.transform, "PlayButton", "PLAY",
            new Vector2(0.5f, 0.4f), new Vector2(300, 60), new Color(0.1f, 0.4f, 0.1f));

        // Quit Button
        menuUI.quitButton = CreateButton(canvasObj.transform, "QuitButton", "QUIT",
            new Vector2(0.5f, 0.28f), new Vector2(300, 60), new Color(0.4f, 0.1f, 0.1f));

        // Controls info
        CreateText(canvasObj.transform, "ControlsText",
            "WASD / Arrow Keys - Move    |    SPACE - Shoot    |    ESC - Pause",
            new Vector2(0.5f, 0.1f), 18, new Color(0.6f, 0.6f, 0.6f));
    }

    Text CreateText(Transform parent, string name, string content, Vector2 anchor, int fontSize, Color color)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);

        RectTransform rt = obj.AddComponent<RectTransform>();
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(800, 80);

        Text text = obj.AddComponent<Text>();
        text.text = content;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (text.font == null)
            text.font = Font.CreateDynamicFontFromOSFont("Arial", fontSize);
        text.fontSize = fontSize;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = color;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;

        Outline outline = obj.AddComponent<Outline>();
        outline.effectColor = Color.black;
        outline.effectDistance = new Vector2(2, -2);

        return text;
    }

    Button CreateButton(Transform parent, string name, string label, Vector2 anchor, Vector2 size, Color bgColor)
    {
        GameObject btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent, false);

        RectTransform rt = btnObj.AddComponent<RectTransform>();
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = size;

        Image img = btnObj.AddComponent<Image>();
        img.color = bgColor;

        Button btn = btnObj.AddComponent<Button>();
        var colors = btn.colors;
        colors.highlightedColor = bgColor * 1.3f;
        colors.pressedColor = bgColor * 0.7f;
        btn.colors = colors;

        // Label
        GameObject labelObj = new GameObject("Label");
        labelObj.transform.SetParent(btnObj.transform, false);

        RectTransform labelRt = labelObj.AddComponent<RectTransform>();
        labelRt.anchorMin = Vector2.zero;
        labelRt.anchorMax = Vector2.one;
        labelRt.sizeDelta = Vector2.zero;

        Text labelText = labelObj.AddComponent<Text>();
        labelText.text = label;
        labelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (labelText.font == null)
            labelText.font = Font.CreateDynamicFontFromOSFont("Arial", 30);
        labelText.fontSize = 30;
        labelText.alignment = TextAnchor.MiddleCenter;
        labelText.color = Color.white;
        labelText.fontStyle = FontStyle.Bold;

        return btn;
    }
}

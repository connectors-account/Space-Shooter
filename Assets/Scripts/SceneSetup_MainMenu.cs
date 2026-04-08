using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Builds the MainMenu scene entirely in code at runtime.
/// Attach to an empty GameObject in the MainMenu scene.
/// </summary>
public class SceneSetup_MainMenu : MonoBehaviour
{
    void Start()
    {
        // Ensure managers exist
        EnsureManagers();

        // Camera background
        Camera.main.backgroundColor = new Color(0.02f, 0.02f, 0.08f);
        Camera.main.orthographic = true;
        Camera.main.orthographicSize = 5.5f;

        // Starfield
        GameObject stars = new GameObject("Starfield");
        stars.AddComponent<BackgroundStarfield>();

        // Canvas + UI
        GameObject canvasGO = CreateCanvas();
        UIManager ui = canvasGO.AddComponent<UIManager>();

        // Title
        CreateText(canvasGO.transform, "TitleText", "SPACE SHOOTER",
            new Vector2(0, 140), 48, Color.white, FontStyle.Bold);

        // High Score
        Text hsText = CreateText(canvasGO.transform, "HighScoreText", "High Score: 0",
            new Vector2(0, 60), 24, Color.yellow, FontStyle.Normal);
        ui.highScoreText = hsText;

        // Play Button
        CreateButton(canvasGO.transform, "PlayButton", "PLAY",
            new Vector2(0, -30), new Vector2(200, 60), () => ui.OnPlayButton());

        // Quit Button
        CreateButton(canvasGO.transform, "QuitButton", "QUIT",
            new Vector2(0, -110), new Vector2(200, 60), () => ui.OnQuitButton());

        // Controls info
        CreateText(canvasGO.transform, "ControlsText",
            "Controls: WASD/Arrows = Move | Space = Shoot | Esc = Pause",
            new Vector2(0, -200), 16, new Color(0.7f, 0.7f, 0.7f), FontStyle.Italic);
    }

    void EnsureManagers()
    {
        if (GameManager.Instance == null)
        {
            GameObject go = new GameObject("GameManager");
            go.AddComponent<GameManager>();
        }
        if (AudioManager.Instance == null)
        {
            GameObject go = new GameObject("AudioManager");
            go.AddComponent<AudioManager>();
        }
        if (SpriteGenerator.Instance == null)
        {
            GameObject go = new GameObject("SpriteGenerator");
            go.AddComponent<SpriteGenerator>();
        }
    }

    // --- UI Helpers ---

    GameObject CreateCanvas()
    {
        GameObject go = new GameObject("Canvas");
        Canvas canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        go.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        go.GetComponent<CanvasScaler>().referenceResolution = new Vector2(800, 600);
        go.AddComponent<GraphicRaycaster>();

        // EventSystem
        if (UnityEngine.EventSystems.EventSystem.current == null)
        {
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<UnityEngine.EventSystems.EventSystem>();
            es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        return go;
    }

    Text CreateText(Transform parent, string name, string content,
        Vector2 anchoredPos, int fontSize, Color color, FontStyle style)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = new Vector2(700, 60);

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
        ColorBlock cb = btn.colors;
        cb.highlightedColor = new Color(0.3f, 0.5f, 0.9f);
        cb.pressedColor = new Color(0.1f, 0.2f, 0.4f);
        btn.colors = cb;
        btn.onClick.AddListener(onClick);

        // Button label
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
            text.font = Font.CreateDynamicFontFromOSFont("Arial", 28);
        text.fontSize = 28;
        text.fontStyle = FontStyle.Bold;
        text.color = Color.white;
        text.alignment = TextAnchor.MiddleCenter;
    }
}

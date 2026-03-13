using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Sets up the Main Menu scene at runtime with all UI elements.
/// Also creates the GameManager and AudioManager singletons if they don't exist.
/// </summary>
public class MainMenuSetup : MonoBehaviour
{
    private void Awake()
    {
        SetupCamera();
        EnsureManagers();
        CreateBackground();
        CreateUI();
    }

    private void SetupCamera()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            GameObject camObj = new GameObject("MainCamera");
            cam = camObj.AddComponent<Camera>();
            camObj.AddComponent<AudioListener>();
        }
        cam.orthographic = true;
        cam.orthographicSize = 5f;
        cam.backgroundColor = new Color(0.02f, 0.02f, 0.08f);
        cam.transform.position = new Vector3(0, 0, -10);
    }

    private void EnsureManagers()
    {
        if (GameManager.Instance == null)
        {
            GameObject gmObj = new GameObject("GameManager");
            gmObj.AddComponent<GameManager>();
        }

        if (AudioManager.Instance == null)
        {
            GameObject amObj = new GameObject("AudioManager");
            amObj.AddComponent<AudioManager>();
        }
    }

    private void CreateBackground()
    {
        GameObject bgObj = new GameObject("MenuStarfield");
        bgObj.AddComponent<StarfieldGenerator>();
    }

    private void CreateUI()
    {
        // Canvas
        GameObject canvasObj = new GameObject("MenuCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        canvasObj.AddComponent<GraphicRaycaster>();

        // Main Menu UI component
        MainMenuUI menuUI = canvasObj.AddComponent<MainMenuUI>();

        // Title
        menuUI.titleText = CreateText(canvasObj.transform, "Title", "SPACE SHOOTER",
            new Vector2(0, 200), new Vector2(800, 100), TextAnchor.MiddleCenter, 72,
            new Color(0.3f, 0.7f, 1f));
        CenterAnchors(menuUI.titleText.GetComponent<RectTransform>());

        // Subtitle
        CreateText(canvasObj.transform, "Subtitle", "DEFEND THE GALAXY",
            new Vector2(0, 130), new Vector2(600, 40), TextAnchor.MiddleCenter, 24,
            new Color(0.5f, 0.7f, 0.9f));

        // High Score
        menuUI.highScoreText = CreateText(canvasObj.transform, "HighScore", "HIGH SCORE: 0",
            new Vector2(0, 50), new Vector2(400, 40), TextAnchor.MiddleCenter, 28,
            new Color(1f, 0.8f, 0.2f));
        CenterAnchors(menuUI.highScoreText.GetComponent<RectTransform>());

        // Start Button
        menuUI.startButton = CreateButton(canvasObj.transform, "StartButton", "START GAME",
            new Vector2(0, -40), new Vector2(300, 60), new Color(0.1f, 0.5f, 0.1f));
        CenterAnchors(menuUI.startButton.GetComponent<RectTransform>());

        // Quit Button
        menuUI.quitButton = CreateButton(canvasObj.transform, "QuitButton", "QUIT",
            new Vector2(0, -120), new Vector2(300, 60), new Color(0.5f, 0.1f, 0.1f));
        CenterAnchors(menuUI.quitButton.GetComponent<RectTransform>());

        // Controls text
        CreateText(canvasObj.transform, "Controls", "WASD / Arrow Keys: Move  |  SPACE: Shoot  |  ESC: Pause",
            new Vector2(0, -230), new Vector2(800, 30), TextAnchor.MiddleCenter, 18,
            new Color(0.5f, 0.5f, 0.5f));

        // Version
        menuUI.versionText = CreateText(canvasObj.transform, "Version", "v1.0.0",
            new Vector2(10, 10), new Vector2(100, 25), TextAnchor.LowerLeft, 14,
            new Color(0.3f, 0.3f, 0.3f));
        RectTransform vRT = menuUI.versionText.GetComponent<RectTransform>();
        vRT.anchorMin = Vector2.zero;
        vRT.anchorMax = Vector2.zero;
        vRT.pivot = Vector2.zero;
    }

    private Text CreateText(Transform parent, string name, string text,
        Vector2 position, Vector2 size, TextAnchor alignment, int fontSize, Color color)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);

        RectTransform rt = obj.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = position;
        rt.sizeDelta = size;

        Text txt = obj.AddComponent<Text>();
        txt.text = text;
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (txt.font == null)
            txt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        txt.fontSize = fontSize;
        txt.color = color;
        txt.alignment = alignment;
        txt.horizontalOverflow = HorizontalWrapMode.Overflow;

        Shadow shadow = obj.AddComponent<Shadow>();
        shadow.effectColor = new Color(0, 0, 0, 0.5f);
        shadow.effectDistance = new Vector2(2, -2);

        return txt;
    }

    private Button CreateButton(Transform parent, string name, string text,
        Vector2 position, Vector2 size, Color color)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);

        RectTransform rt = obj.AddComponent<RectTransform>();
        rt.anchoredPosition = position;
        rt.sizeDelta = size;

        Image img = obj.AddComponent<Image>();
        img.color = color;

        Button btn = obj.AddComponent<Button>();
        ColorBlock colors = btn.colors;
        colors.normalColor = color;
        colors.highlightedColor = color * 1.3f;
        colors.pressedColor = color * 0.7f;
        colors.selectedColor = color * 1.1f;
        btn.colors = colors;

        // Button text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(obj.transform, false);
        RectTransform textRT = textObj.AddComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.offsetMin = Vector2.zero;
        textRT.offsetMax = Vector2.zero;

        Text txt = textObj.AddComponent<Text>();
        txt.text = text;
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (txt.font == null)
            txt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        txt.fontSize = 26;
        txt.color = Color.white;
        txt.alignment = TextAnchor.MiddleCenter;

        return btn;
    }

    private void CenterAnchors(RectTransform rt)
    {
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
    }
}

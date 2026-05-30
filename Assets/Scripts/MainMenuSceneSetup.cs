using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// MainMenuSceneSetup – Programmatically creates the entire Main Menu UI at runtime.
/// Attach this to a single empty GameObject in the MainMenu scene.
/// Creates: Camera, Canvas, Title, Start/Quit buttons, Starfield background.
/// </summary>
public class MainMenuSceneSetup : MonoBehaviour
{
    void Awake()
    {
        // ── Camera ──
        Camera cam = Camera.main;
        if (cam == null)
        {
            GameObject camObj = new GameObject("Main Camera");
            cam = camObj.AddComponent<Camera>();
            camObj.tag = "MainCamera";
        }
        cam.orthographic = true;
        cam.orthographicSize = 6f;
        cam.backgroundColor = new Color(0.02f, 0.02f, 0.08f);
        cam.transform.position = new Vector3(0, 0, -10);

        // ── Starfield ──
        GameObject starfield = new GameObject("Starfield");
        StarfieldBackground sf = starfield.AddComponent<StarfieldBackground>();
        sf.starCount = 80;
        sf.scrollSpeed = 0.8f;

        // ── Canvas ──
        GameObject canvasObj = new GameObject("MenuCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(800, 600);
        canvasObj.AddComponent<GraphicRaycaster>();

        // ── MainMenuController ──
        MainMenuController menu = canvasObj.AddComponent<MainMenuController>();

        // ── Title Text ──
        CreateText(canvasObj.transform, "Title", "SPACE SHOOTER",
            new Vector2(0, 120), new Vector2(600, 80), 52,
            new Color(0.3f, 1f, 0.4f), TextAnchor.MiddleCenter);

        // ── Subtitle ──
        CreateText(canvasObj.transform, "Subtitle", "Defend the Galaxy!",
            new Vector2(0, 60), new Vector2(400, 40), 22,
            new Color(0.7f, 0.7f, 0.9f), TextAnchor.MiddleCenter);

        // ── Start Button ──
        GameObject startBtn = CreateButton(canvasObj.transform, "StartButton",
            "START GAME", new Vector2(0, -20), new Vector2(220, 55),
            new Color(0.15f, 0.5f, 0.2f));
        menu.startButton = startBtn.GetComponent<Button>();

        // ── Quit Button ──
        GameObject quitBtn = CreateButton(canvasObj.transform, "QuitButton",
            "QUIT", new Vector2(0, -90), new Vector2(220, 55),
            new Color(0.5f, 0.15f, 0.15f));
        menu.quitButton = quitBtn.GetComponent<Button>();

        // ── Controls Info ──
        CreateText(canvasObj.transform, "Controls", "WASD / Arrow Keys = Move   |   Space = Shoot",
            new Vector2(0, -170), new Vector2(600, 30), 16,
            new Color(0.5f, 0.5f, 0.6f), TextAnchor.MiddleCenter);

        // ── EventSystem ──
        if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<UnityEngine.EventSystems.EventSystem>();
            es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }
    }

    // ── Helpers ──

    void CreateText(Transform parent, string name, string content,
        Vector2 position, Vector2 size, int fontSize, Color color, TextAnchor alignment)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);

        RectTransform rt = obj.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = position;
        rt.sizeDelta = size;

        Text text = obj.AddComponent<Text>();
        text.text = content;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (text.font == null)
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.fontSize = fontSize;
        text.color = color;
        text.alignment = alignment;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;

        // Add outline for better readability
        Outline outline = obj.AddComponent<Outline>();
        outline.effectColor = new Color(0, 0, 0, 0.8f);
        outline.effectDistance = new Vector2(2, -2);
    }

    GameObject CreateButton(Transform parent, string name, string label,
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
        colors.selectedColor = bgColor;
        btn.colors = colors;

        // Label
        GameObject labelObj = new GameObject("Label");
        labelObj.transform.SetParent(btnObj.transform, false);
        RectTransform lrt = labelObj.AddComponent<RectTransform>();
        lrt.anchorMin = Vector2.zero;
        lrt.anchorMax = Vector2.one;
        lrt.offsetMin = Vector2.zero;
        lrt.offsetMax = Vector2.zero;

        Text text = labelObj.AddComponent<Text>();
        text.text = label;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (text.font == null)
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.fontSize = 24;
        text.color = Color.white;
        text.alignment = TextAnchor.MiddleCenter;
        text.fontStyle = FontStyle.Bold;

        return btnObj;
    }
}

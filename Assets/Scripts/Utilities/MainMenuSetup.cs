// ============================================================================
// MainMenuSetup.cs - Auto-configures the Main Menu scene at runtime
// Creates all UI elements programmatically.
// Attach to a single empty GameObject in the MainMenu scene.
// ============================================================================
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Creates the entire Main Menu UI programmatically.
/// Includes title, start/quit buttons, high score display, and background.
/// </summary>
public class MainMenuSetup : MonoBehaviour
{
    private void Awake()
    {
        // Ensure managers exist
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

        CreateBackground();
        CreateMenuUI();
    }

    private void CreateBackground()
    {
        // Starfield background
        GameObject bg = new GameObject("MenuBackground");
        SpriteRenderer sr = bg.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteGenerator.CreateStarfield(256, 256, 300);
        sr.sortingOrder = -10;
        bg.transform.localScale = new Vector3(3f, 3f, 1f);
        bg.transform.position = new Vector3(0, 0, 10);

        // Slow scrolling
        BackgroundScroller bs = bg.AddComponent<BackgroundScroller>();
        bs.scrollSpeed = 0.3f;
        bs.tileHeight = 30f;
    }

    private void CreateMenuUI()
    {
        // ---- Canvas ----
        GameObject canvasObj = new GameObject("MenuCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        canvasObj.AddComponent<GraphicRaycaster>();

        // ---- Event System ----
        if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<UnityEngine.EventSystems.EventSystem>();
            es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        // ---- MainMenuUI component ----
        MainMenuUI menuUI = canvasObj.AddComponent<MainMenuUI>();

        // ---- Title ----
        menuUI.titleText = CreateText(canvasObj.transform, "Title",
            "SPACE SHOOTER", 64, Color.cyan,
            new Vector2(0, 200), new Vector2(800, 100));

        // Add glow outline to title
        Outline titleOutline = menuUI.titleText.GetComponent<Outline>();
        if (titleOutline != null)
        {
            titleOutline.effectColor = new Color(0, 0.5f, 1f, 0.5f);
            titleOutline.effectDistance = new Vector2(2, -2);
        }

        // ---- Subtitle ----
        CreateText(canvasObj.transform, "Subtitle",
            "DEFEND THE GALAXY", 24, new Color(0.7f, 0.7f, 0.7f),
            new Vector2(0, 130), new Vector2(600, 40));

        // ---- High Score ----
        menuUI.highScoreText = CreateText(canvasObj.transform, "HighScore",
            "", 28, Color.yellow,
            new Vector2(0, 60), new Vector2(400, 40));

        // ---- Start Button ----
        menuUI.startButton = CreateButton(canvasObj.transform, "StartBtn",
            "START GAME", new Vector2(0, -40), new Color(0.1f, 0.6f, 0.1f));

        // ---- Quit Button ----
        menuUI.quitButton = CreateButton(canvasObj.transform, "QuitBtn",
            "QUIT", new Vector2(0, -120), new Color(0.6f, 0.1f, 0.1f));

        // ---- Instructions ----
        menuUI.instructionsText = CreateText(canvasObj.transform, "Instructions",
            "", 20, new Color(0.6f, 0.6f, 0.6f),
            new Vector2(0, -250), new Vector2(500, 120));

        // ---- Version ----
        CreateText(canvasObj.transform, "Version",
            "v1.0", 16, new Color(0.4f, 0.4f, 0.4f),
            new Vector2(0, -350), new Vector2(200, 30));
    }

    // ========================================================================
    // UI Helpers
    // ========================================================================

    private Text CreateText(Transform parent, string name, string content,
        int fontSize, Color color, Vector2 position, Vector2 size)
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
        text.fontSize = fontSize;
        text.color = color;
        text.alignment = TextAnchor.MiddleCenter;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;

        Outline outline = obj.AddComponent<Outline>();
        outline.effectColor = Color.black;
        outline.effectDistance = new Vector2(1, -1);

        return text;
    }

    private Button CreateButton(Transform parent, string name, string label,
        Vector2 position, Color bgColor)
    {
        GameObject btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent, false);

        RectTransform rt = btnObj.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = position;
        rt.sizeDelta = new Vector2(300, 60);

        Image img = btnObj.AddComponent<Image>();
        img.color = bgColor;

        Button btn = btnObj.AddComponent<Button>();
        ColorBlock colors = btn.colors;
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

        Text text = labelObj.AddComponent<Text>();
        text.text = label;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 28;
        text.color = Color.white;
        text.alignment = TextAnchor.MiddleCenter;

        return btn;
    }
}

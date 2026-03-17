using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Runtime main menu builder - creates the main menu UI at runtime.
/// Attach to a GameObject in the MainMenu scene.
/// </summary>
public class MainMenuSetup : MonoBehaviour
{
    void Awake()
    {
        Camera.main.backgroundColor = new Color(0.02f, 0.01f, 0.1f);

        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        // Create Canvas
        GameObject canvasObj = new GameObject("MainMenuCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasObj.AddComponent<GraphicRaycaster>();

        // EventSystem
        if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<UnityEngine.EventSystems.EventSystem>();
            es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        MainMenuUI menuUI = canvasObj.AddComponent<MainMenuUI>();

        // Title
        menuUI.titleText = CreateText(canvasObj.transform, "Title", "SPACE SHOOTER",
            new Vector2(0, 150), new Vector2(600, 80), 64, Color.cyan, font);

        // Subtitle
        CreateText(canvasObj.transform, "Subtitle", "Defend the Galaxy!",
            new Vector2(0, 80), new Vector2(400, 40), 24, Color.white, font);

        // Start Button
        menuUI.startButton = CreateMenuButton(canvasObj.transform, "StartBtn", "START GAME",
            new Vector2(0, -30), new Vector2(300, 60), font);

        // Quit Button
        menuUI.quitButton = CreateMenuButton(canvasObj.transform, "QuitBtn", "QUIT",
            new Vector2(0, -110), new Vector2(300, 60), font);

        // Controls info
        CreateText(canvasObj.transform, "Controls",
            "Controls: WASD/Arrows = Move | Space = Shoot | ESC = Pause",
            new Vector2(0, -250), new Vector2(700, 30), 18, new Color(0.6f, 0.6f, 0.6f), font);
    }

    Text CreateText(Transform parent, string name, string content,
        Vector2 pos, Vector2 size, int fontSize, Color color, Font font)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        RectTransform rt = obj.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;

        Text text = obj.AddComponent<Text>();
        text.text = content;
        text.font = font;
        text.fontSize = fontSize;
        text.color = color;
        text.alignment = TextAnchor.MiddleCenter;

        Outline outline = obj.AddComponent<Outline>();
        outline.effectColor = Color.black;
        outline.effectDistance = new Vector2(2, -2);

        return text;
    }

    Button CreateMenuButton(Transform parent, string name, string label,
        Vector2 pos, Vector2 size, Font font)
    {
        GameObject btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent, false);
        RectTransform rt = btnObj.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;

        Image img = btnObj.AddComponent<Image>();
        img.color = new Color(0.1f, 0.1f, 0.4f, 0.9f);

        Button btn = btnObj.AddComponent<Button>();
        ColorBlock cb = btn.colors;
        cb.highlightedColor = new Color(0.2f, 0.2f, 0.7f, 1f);
        cb.pressedColor = new Color(0.05f, 0.05f, 0.2f, 1f);
        btn.colors = cb;

        // Label
        Text text = CreateText(btnObj.transform, $"{name}_Label", label,
            Vector2.zero, size, 28, Color.white, font);
        text.raycastTarget = false;

        return btn;
    }
}

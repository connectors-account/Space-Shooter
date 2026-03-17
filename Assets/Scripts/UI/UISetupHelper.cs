using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Runtime UI builder that creates all game UI if not already set up in the editor.
/// This is a convenience script - in production you'd set up UI in the Unity Editor.
/// Attach to a GameObject in the GameScene.
/// </summary>
public class UISetupHelper : MonoBehaviour
{
    public Font uiFont;

    void Awake()
    {
        if (uiFont == null)
            uiFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        SetupHUD();
        SetupGameOverUI();
        SetupPauseMenu();
    }

    void SetupHUD()
    {
        if (HUDManager.Instance != null) return;

        // Create Canvas
        GameObject canvasObj = CreateCanvas("HUDCanvas");
        HUDManager hud = canvasObj.AddComponent<HUDManager>();

        // Score text - top left
        hud.scoreText = CreateText(canvasObj.transform, "ScoreText", "SCORE: 0",
            TextAnchor.UpperLeft, new Vector2(20, -20), new Vector2(300, 40));

        // Health text - top right
        hud.healthText = CreateText(canvasObj.transform, "HealthText", "HP: 5/5",
            TextAnchor.UpperRight, new Vector2(-20, -20), new Vector2(200, 40));

        // Wave text - top center
        hud.waveText = CreateText(canvasObj.transform, "WaveText", "WAVE 1",
            TextAnchor.UpperCenter, new Vector2(0, -20), new Vector2(200, 40));

        // Health bar - under health text
        hud.healthBar = CreateHealthBar(canvasObj.transform);
    }

    void SetupGameOverUI()
    {
        if (GameOverUI.Instance != null) return;

        // Create panel on existing canvas or new one
        Canvas existingCanvas = FindObjectOfType<Canvas>();
        Transform parent = existingCanvas != null ? existingCanvas.transform : CreateCanvas("GameOverCanvas").transform;

        // Create dark overlay panel
        GameObject panel = CreatePanel(parent, "GameOverPanel", new Color(0, 0, 0, 0.8f));
        GameOverUI goUI = panel.AddComponent<GameOverUI>();

        goUI.gameOverText = CreateText(panel.transform, "GameOverTitle", "GAME OVER",
            TextAnchor.MiddleCenter, new Vector2(0, 80), new Vector2(400, 60), 48);

        goUI.finalScoreText = CreateText(panel.transform, "FinalScore", "Final Score: 0",
            TextAnchor.MiddleCenter, new Vector2(0, 20), new Vector2(400, 40), 28);

        goUI.finalWaveText = CreateText(panel.transform, "FinalWave", "Wave Reached: 1",
            TextAnchor.MiddleCenter, new Vector2(0, -20), new Vector2(400, 40), 24);

        goUI.restartButton = CreateButton(panel.transform, "RestartBtn", "RESTART",
            new Vector2(-100, -80), new Vector2(160, 50));

        goUI.mainMenuButton = CreateButton(panel.transform, "MenuBtn", "MAIN MENU",
            new Vector2(100, -80), new Vector2(160, 50));

        panel.SetActive(false);
    }

    void SetupPauseMenu()
    {
        if (PauseMenuUI.Instance != null) return;

        Canvas existingCanvas = FindObjectOfType<Canvas>();
        Transform parent = existingCanvas != null ? existingCanvas.transform : CreateCanvas("PauseCanvas").transform;

        GameObject panel = CreatePanel(parent, "PausePanel", new Color(0, 0, 0, 0.7f));
        PauseMenuUI pauseUI = panel.AddComponent<PauseMenuUI>();

        pauseUI.pauseText = CreateText(panel.transform, "PauseTitle", "PAUSED",
            TextAnchor.MiddleCenter, new Vector2(0, 60), new Vector2(400, 60), 48);

        pauseUI.resumeButton = CreateButton(panel.transform, "ResumeBtn", "RESUME",
            new Vector2(0, -10), new Vector2(200, 50));

        pauseUI.mainMenuButton = CreateButton(panel.transform, "PauseMenuBtn", "MAIN MENU",
            new Vector2(0, -70), new Vector2(200, 50));

        panel.SetActive(false);
    }

    // === Helper Methods ===

    GameObject CreateCanvas(string name)
    {
        GameObject canvasObj = new GameObject(name);
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        canvasObj.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasObj.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1920, 1080);
        canvasObj.AddComponent<GraphicRaycaster>();

        // Ensure EventSystem exists
        if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<UnityEngine.EventSystems.EventSystem>();
            es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        return canvasObj;
    }

    Text CreateText(Transform parent, string name, string content,
        TextAnchor anchor, Vector2 position, Vector2 size, int fontSize = 24)
    {
        GameObject textObj = new GameObject(name);
        textObj.transform.SetParent(parent, false);

        RectTransform rt = textObj.AddComponent<RectTransform>();
        rt.anchoredPosition = position;
        rt.sizeDelta = size;

        // Set anchors based on alignment
        switch (anchor)
        {
            case TextAnchor.UpperLeft:
                rt.anchorMin = new Vector2(0, 1);
                rt.anchorMax = new Vector2(0, 1);
                rt.pivot = new Vector2(0, 1);
                break;
            case TextAnchor.UpperRight:
                rt.anchorMin = new Vector2(1, 1);
                rt.anchorMax = new Vector2(1, 1);
                rt.pivot = new Vector2(1, 1);
                break;
            case TextAnchor.UpperCenter:
                rt.anchorMin = new Vector2(0.5f, 1);
                rt.anchorMax = new Vector2(0.5f, 1);
                rt.pivot = new Vector2(0.5f, 1);
                break;
            default:
                rt.anchorMin = new Vector2(0.5f, 0.5f);
                rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                break;
        }

        Text text = textObj.AddComponent<Text>();
        text.text = content;
        text.font = uiFont;
        text.fontSize = fontSize;
        text.color = Color.white;
        text.alignment = anchor;

        // Add outline for readability
        Outline outline = textObj.AddComponent<Outline>();
        outline.effectColor = Color.black;
        outline.effectDistance = new Vector2(1, -1);

        return text;
    }

    Button CreateButton(Transform parent, string name, string label,
        Vector2 position, Vector2 size)
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
        img.color = new Color(0.2f, 0.2f, 0.6f, 0.9f);

        Button btn = btnObj.AddComponent<Button>();
        ColorBlock cb = btn.colors;
        cb.highlightedColor = new Color(0.3f, 0.3f, 0.8f, 1f);
        cb.pressedColor = new Color(0.15f, 0.15f, 0.4f, 1f);
        btn.colors = cb;

        // Button label
        Text text = CreateText(btnObj.transform, $"{name}_Label", label,
            TextAnchor.MiddleCenter, Vector2.zero, size, 22);
        text.raycastTarget = false;

        return btn;
    }

    GameObject CreatePanel(Transform parent, string name, Color color)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent, false);

        RectTransform rt = panel.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;

        Image img = panel.AddComponent<Image>();
        img.color = color;

        return panel;
    }

    Slider CreateHealthBar(Transform parent)
    {
        GameObject sliderObj = new GameObject("HealthBar");
        sliderObj.transform.SetParent(parent, false);

        RectTransform rt = sliderObj.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(1, 1);
        rt.anchorMax = new Vector2(1, 1);
        rt.pivot = new Vector2(1, 1);
        rt.anchoredPosition = new Vector2(-20, -55);
        rt.sizeDelta = new Vector2(200, 20);

        Slider slider = sliderObj.AddComponent<Slider>();
        slider.minValue = 0;
        slider.maxValue = 5;
        slider.value = 5;
        slider.interactable = false;

        // Background
        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(sliderObj.transform, false);
        RectTransform bgRt = bgObj.AddComponent<RectTransform>();
        bgRt.anchorMin = Vector2.zero;
        bgRt.anchorMax = Vector2.one;
        bgRt.sizeDelta = Vector2.zero;
        Image bgImg = bgObj.AddComponent<Image>();
        bgImg.color = new Color(0.2f, 0, 0, 0.8f);

        // Fill area
        GameObject fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(sliderObj.transform, false);
        RectTransform faRt = fillArea.AddComponent<RectTransform>();
        faRt.anchorMin = Vector2.zero;
        faRt.anchorMax = Vector2.one;
        faRt.sizeDelta = new Vector2(-10, 0);
        faRt.anchoredPosition = Vector2.zero;

        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(fillArea.transform, false);
        RectTransform fillRt = fill.AddComponent<RectTransform>();
        fillRt.anchorMin = Vector2.zero;
        fillRt.anchorMax = new Vector2(0, 1);
        fillRt.sizeDelta = new Vector2(10, 0);
        Image fillImg = fill.AddComponent<Image>();
        fillImg.color = new Color(0, 0.8f, 0, 1f);

        slider.fillRect = fillRt;

        return slider;
    }
}

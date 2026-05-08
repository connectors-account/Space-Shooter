using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Main menu screen with Play, Options, and Quit buttons.
/// Creates its own UI programmatically at runtime.
/// </summary>
public class MainMenuUI : MonoBehaviour
{
    private Canvas canvas;
    private GameObject optionsPanel;
    private Slider volumeSlider;
    private Toggle mouseControlToggle;

    private void Start()
    {
        BuildUI();

        if (GameManager.Instance != null)
            GameManager.Instance.ChangeState(GameState.MainMenu);
    }

    private void BuildUI()
    {
        // Create Canvas
        GameObject canvasObj = new GameObject("MainMenuCanvas");
        canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        canvasObj.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasObj.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1920, 1080);
        canvasObj.AddComponent<GraphicRaycaster>();

        // Background
        GameObject bg = CreateUIElement("Background", canvasObj.transform);
        Image bgImg = bg.AddComponent<Image>();
        bgImg.color = new Color(0.02f, 0.02f, 0.08f, 1f);
        RectTransform bgRect = bg.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;

        // Title
        GameObject titleObj = CreateUIElement("Title", canvasObj.transform);
        Text titleText = titleObj.AddComponent<Text>();
        titleText.text = "SPACE SHOOTER";
        titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        titleText.fontSize = 72;
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.color = new Color(0f, 0.8f, 1f);
        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchoredPosition = new Vector2(0, 200);
        titleRect.sizeDelta = new Vector2(800, 100);

        // Subtitle
        GameObject subObj = CreateUIElement("Subtitle", canvasObj.transform);
        Text subText = subObj.AddComponent<Text>();
        subText.text = "Defend the Galaxy";
        subText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        subText.fontSize = 28;
        subText.alignment = TextAnchor.MiddleCenter;
        subText.color = new Color(0.6f, 0.6f, 0.8f);
        RectTransform subRect = subObj.GetComponent<RectTransform>();
        subRect.anchoredPosition = new Vector2(0, 130);
        subRect.sizeDelta = new Vector2(600, 50);

        // High Score
        int highScore = PlayerPrefs.GetInt("HighScore", 0);
        if (highScore > 0)
        {
            GameObject hsObj = CreateUIElement("HighScore", canvasObj.transform);
            Text hsText = hsObj.AddComponent<Text>();
            hsText.text = $"High Score: {highScore:N0}";
            hsText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            hsText.fontSize = 24;
            hsText.alignment = TextAnchor.MiddleCenter;
            hsText.color = new Color(1f, 0.9f, 0.3f);
            RectTransform hsRect = hsObj.GetComponent<RectTransform>();
            hsRect.anchoredPosition = new Vector2(0, 80);
            hsRect.sizeDelta = new Vector2(400, 40);
        }

        // Buttons
        CreateButton("PlayButton", "PLAY", canvasObj.transform, new Vector2(0, -20), OnPlayClicked);
        CreateButton("OptionsButton", "OPTIONS", canvasObj.transform, new Vector2(0, -100), OnOptionsClicked);
        CreateButton("QuitButton", "QUIT", canvasObj.transform, new Vector2(0, -180), OnQuitClicked);

        // Controls hint
        GameObject hintObj = CreateUIElement("Hint", canvasObj.transform);
        Text hintText = hintObj.AddComponent<Text>();
        hintText.text = "WASD/Arrows to move | Space/Click to shoot | ESC to pause";
        hintText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        hintText.fontSize = 18;
        hintText.alignment = TextAnchor.MiddleCenter;
        hintText.color = new Color(0.5f, 0.5f, 0.6f);
        RectTransform hintRect = hintObj.GetComponent<RectTransform>();
        hintRect.anchoredPosition = new Vector2(0, -300);
        hintRect.sizeDelta = new Vector2(800, 30);

        // Build options panel (initially hidden)
        BuildOptionsPanel(canvasObj.transform);
    }

    private void BuildOptionsPanel(Transform parent)
    {
        optionsPanel = CreateUIElement("OptionsPanel", parent);
        Image panelImg = optionsPanel.AddComponent<Image>();
        panelImg.color = new Color(0.05f, 0.05f, 0.15f, 0.95f);
        RectTransform panelRect = optionsPanel.GetComponent<RectTransform>();
        panelRect.sizeDelta = new Vector2(500, 400);
        panelRect.anchoredPosition = Vector2.zero;

        // Options Title
        GameObject optTitle = CreateUIElement("OptTitle", optionsPanel.transform);
        Text optTitleText = optTitle.AddComponent<Text>();
        optTitleText.text = "OPTIONS";
        optTitleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        optTitleText.fontSize = 40;
        optTitleText.alignment = TextAnchor.MiddleCenter;
        optTitleText.color = Color.white;
        RectTransform optTitleRect = optTitle.GetComponent<RectTransform>();
        optTitleRect.anchoredPosition = new Vector2(0, 150);
        optTitleRect.sizeDelta = new Vector2(400, 50);

        // Volume label
        GameObject volLabel = CreateUIElement("VolLabel", optionsPanel.transform);
        Text volText = volLabel.AddComponent<Text>();
        volText.text = "Volume";
        volText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        volText.fontSize = 24;
        volText.alignment = TextAnchor.MiddleCenter;
        volText.color = Color.white;
        RectTransform volRect = volLabel.GetComponent<RectTransform>();
        volRect.anchoredPosition = new Vector2(0, 70);
        volRect.sizeDelta = new Vector2(200, 30);

        // Volume slider
        GameObject sliderObj = CreateSlider("VolumeSlider", optionsPanel.transform, new Vector2(0, 30));
        volumeSlider = sliderObj.GetComponent<Slider>();
        volumeSlider.value = PlayerPrefs.GetFloat("Volume", 0.7f);
        volumeSlider.onValueChanged.AddListener(OnVolumeChanged);

        // Mouse control toggle
        GameObject toggleLabel = CreateUIElement("ToggleLabel", optionsPanel.transform);
        Text tText = toggleLabel.AddComponent<Text>();
        tText.text = "Mouse Control";
        tText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        tText.fontSize = 24;
        tText.alignment = TextAnchor.MiddleCenter;
        tText.color = Color.white;
        RectTransform tRect = toggleLabel.GetComponent<RectTransform>();
        tRect.anchoredPosition = new Vector2(0, -40);
        tRect.sizeDelta = new Vector2(200, 30);

        // Simple toggle using a button
        bool mouseOn = PlayerPrefs.GetInt("MouseControl", 0) == 1;
        CreateButton("MouseToggle", mouseOn ? "[X] Enabled" : "[ ] Disabled",
            optionsPanel.transform, new Vector2(0, -80), OnMouseToggleClicked);

        // Back button
        CreateButton("BackButton", "BACK", optionsPanel.transform, new Vector2(0, -150), OnBackClicked);

        optionsPanel.SetActive(false);
    }

    // --- Button Callbacks ---

    private void OnPlayClicked()
    {
        SceneManager.LoadScene("GameScene");
    }

    private void OnOptionsClicked()
    {
        optionsPanel.SetActive(true);
    }

    private void OnQuitClicked()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.QuitGame();
        else
            Application.Quit();
    }

    private void OnVolumeChanged(float value)
    {
        PlayerPrefs.SetFloat("Volume", value);
        AudioListener.volume = value;
    }

    private void OnMouseToggleClicked()
    {
        int current = PlayerPrefs.GetInt("MouseControl", 0);
        int newVal = current == 0 ? 1 : 0;
        PlayerPrefs.SetInt("MouseControl", newVal);

        // Update button text
        Text btnText = optionsPanel.transform.Find("MouseToggle")?.GetComponentInChildren<Text>();
        if (btnText != null)
            btnText.text = newVal == 1 ? "[X] Enabled" : "[ ] Disabled";
    }

    private void OnBackClicked()
    {
        optionsPanel.SetActive(false);
    }

    // --- UI Helpers ---

    private GameObject CreateUIElement(string name, Transform parent)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform));
        obj.transform.SetParent(parent, false);
        return obj;
    }

    private void CreateButton(string name, string text, Transform parent, Vector2 position,
                              UnityEngine.Events.UnityAction onClick)
    {
        GameObject btnObj = CreateUIElement(name, parent);
        Image btnImg = btnObj.AddComponent<Image>();
        btnImg.color = new Color(0.1f, 0.15f, 0.3f, 0.9f);
        Button btn = btnObj.AddComponent<Button>();

        RectTransform btnRect = btnObj.GetComponent<RectTransform>();
        btnRect.anchoredPosition = position;
        btnRect.sizeDelta = new Vector2(300, 55);

        // Button text
        GameObject textObj = CreateUIElement("Text", btnObj.transform);
        Text btnText = textObj.AddComponent<Text>();
        btnText.text = text;
        btnText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        btnText.fontSize = 28;
        btnText.alignment = TextAnchor.MiddleCenter;
        btnText.color = Color.white;
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;

        // Hover colors
        ColorBlock colors = btn.colors;
        colors.normalColor = new Color(0.1f, 0.15f, 0.3f, 0.9f);
        colors.highlightedColor = new Color(0.15f, 0.25f, 0.5f, 1f);
        colors.pressedColor = new Color(0.05f, 0.1f, 0.2f, 1f);
        btn.colors = colors;

        btn.onClick.AddListener(onClick);
    }

    private GameObject CreateSlider(string name, Transform parent, Vector2 position)
    {
        // Create slider with background and fill
        GameObject sliderObj = CreateUIElement(name, parent);
        RectTransform sliderRect = sliderObj.GetComponent<RectTransform>();
        sliderRect.anchoredPosition = position;
        sliderRect.sizeDelta = new Vector2(300, 20);

        // Background
        GameObject bgObj = CreateUIElement("Background", sliderObj.transform);
        Image bgImg = bgObj.AddComponent<Image>();
        bgImg.color = new Color(0.2f, 0.2f, 0.3f);
        RectTransform bgRect = bgObj.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;

        // Fill Area
        GameObject fillArea = CreateUIElement("Fill Area", sliderObj.transform);
        RectTransform fillAreaRect = fillArea.GetComponent<RectTransform>();
        fillAreaRect.anchorMin = Vector2.zero;
        fillAreaRect.anchorMax = Vector2.one;
        fillAreaRect.sizeDelta = new Vector2(-20, 0);
        fillAreaRect.anchoredPosition = new Vector2(-5, 0);

        // Fill
        GameObject fillObj = CreateUIElement("Fill", fillArea.transform);
        Image fillImg = fillObj.AddComponent<Image>();
        fillImg.color = new Color(0f, 0.8f, 1f);
        RectTransform fillRect = fillObj.GetComponent<RectTransform>();
        fillRect.sizeDelta = new Vector2(10, 0);

        // Handle area
        GameObject handleArea = CreateUIElement("Handle Slide Area", sliderObj.transform);
        RectTransform handleRect = handleArea.GetComponent<RectTransform>();
        handleRect.anchorMin = Vector2.zero;
        handleRect.anchorMax = Vector2.one;
        handleRect.sizeDelta = new Vector2(-20, 0);
        handleRect.anchoredPosition = Vector2.zero;

        // Handle
        GameObject handleObj = CreateUIElement("Handle", handleArea.transform);
        Image handleImg = handleObj.AddComponent<Image>();
        handleImg.color = Color.white;
        RectTransform hRect = handleObj.GetComponent<RectTransform>();
        hRect.sizeDelta = new Vector2(20, 20);

        Slider slider = sliderObj.AddComponent<Slider>();
        slider.fillRect = fillRect;
        slider.handleRect = hRect;
        slider.targetGraphic = handleImg;
        slider.minValue = 0f;
        slider.maxValue = 1f;

        return sliderObj;
    }
}

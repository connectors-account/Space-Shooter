using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages all UI panels: Main Menu, HUD, Pause Menu, Game Over.
/// Attach to a Canvas object. Wire up buttons in Inspector or let AutoSetup handle it.
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Panels (assign in Inspector or auto-created)")]
    public GameObject mainMenuPanel;
    public GameObject hudPanel;
    public GameObject pausePanel;
    public GameObject gameOverPanel;

    [Header("HUD Elements")]
    public Text scoreText;
    public Text livesText;
    public Text healthText;
    public Text waveText;

    [Header("Game Over Elements")]
    public Text finalScoreText;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // Build UI programmatically if panels aren't assigned
        if (mainMenuPanel == null) BuildUI();

        SubscribeEvents();
        ShowMainMenu();
    }

    void SubscribeEvents()
    {
        var gm = GameManager.Instance;
        if (gm == null) return;
        gm.OnScoreChanged += s => { if (scoreText) scoreText.text = "Score: " + s; };
        gm.OnLivesChanged += l => { if (livesText) livesText.text = "Lives: " + l; };
        gm.OnWaveChanged  += w => { if (waveText)  waveText.text  = "Wave " + w; };
        gm.OnGameOver     += ShowGameOver;
        gm.OnGameStarted  += ShowHUD;
    }

    void Update()
    {
        // Update health bar from player
        if (healthText != null && PlayerController.Instance != null && GameManager.Instance != null && GameManager.Instance.IsPlaying)
        {
            healthText.text = "HP: " + PlayerController.Instance.CurrentHealth;
        }

        // Pause toggle
        if (Input.GetKeyDown(KeyCode.Escape) && GameManager.Instance != null && GameManager.Instance.IsPlaying)
        {
            GameManager.Instance.TogglePause();
            if (pausePanel != null) pausePanel.SetActive(GameManager.Instance.IsPaused);
        }
    }

    void ShowMainMenu()
    {
        SetPanels(main: true, hud: false, pause: false, over: false);
    }

    void ShowHUD()
    {
        SetPanels(main: false, hud: true, pause: false, over: false);
    }

    void ShowGameOver()
    {
        SetPanels(main: false, hud: true, pause: false, over: true);
        if (finalScoreText != null)
            finalScoreText.text = "Final Score: " + GameManager.Instance.Score;
    }

    void SetPanels(bool main, bool hud, bool pause, bool over)
    {
        if (mainMenuPanel)  mainMenuPanel.SetActive(main);
        if (hudPanel)       hudPanel.SetActive(hud);
        if (pausePanel)     pausePanel.SetActive(pause);
        if (gameOverPanel)  gameOverPanel.SetActive(over);
    }

    // ── Button callbacks ──
    public void OnPlayButton()    { GameManager.Instance?.StartGame(); }
    public void OnResumeButton()  { GameManager.Instance?.TogglePause(); pausePanel?.SetActive(false); }
    public void OnRestartButton() { GameManager.Instance?.RestartGame(); }
    public void OnQuitButton()    { GameManager.Instance?.QuitGame(); }

    // ═══════════════════════════════════════════════════════════
    //  Programmatic UI Builder — creates full UI if not wired
    // ═══════════════════════════════════════════════════════════
    void BuildUI()
    {
        Canvas canvas = GetComponent<Canvas>();
        if (canvas == null)
        {
            canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            gameObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            gameObject.AddComponent<GraphicRaycaster>();
        }

        // ── Main Menu ──
        mainMenuPanel = CreatePanel("MainMenu");
        CreateText(mainMenuPanel.transform, "SPACE SHOOTER", 48, new Vector2(0, 100));
        CreateText(mainMenuPanel.transform, "Arrow Keys / WASD to Move\nSpace to Shoot\nESC to Pause", 20, new Vector2(0, -20));
        CreateButton(mainMenuPanel.transform, "PLAY", new Vector2(0, -120), OnPlayButton);

        // ── HUD ──
        hudPanel = CreatePanel("HUD");
        hudPanel.GetComponent<Image>().color = Color.clear;
        scoreText  = CreateText(hudPanel.transform, "Score: 0",  24, new Vector2(-300, 340)).GetComponent<Text>();
        livesText  = CreateText(hudPanel.transform, "Lives: 3",  24, new Vector2(0, 340)).GetComponent<Text>();
        healthText = CreateText(hudPanel.transform, "HP: 100",   24, new Vector2(300, 340)).GetComponent<Text>();
        waveText   = CreateText(hudPanel.transform, "Wave 1",    28, new Vector2(0, 300)).GetComponent<Text>();

        // ── Pause Menu ──
        pausePanel = CreatePanel("PauseMenu");
        CreateText(pausePanel.transform, "PAUSED", 48, new Vector2(0, 60));
        CreateButton(pausePanel.transform, "RESUME", new Vector2(0, -20), OnResumeButton);
        CreateButton(pausePanel.transform, "QUIT", new Vector2(0, -80), OnQuitButton);

        // ── Game Over ──
        gameOverPanel = CreatePanel("GameOver");
        CreateText(gameOverPanel.transform, "GAME OVER", 48, new Vector2(0, 80));
        finalScoreText = CreateText(gameOverPanel.transform, "Final Score: 0", 30, new Vector2(0, 10)).GetComponent<Text>();
        CreateButton(gameOverPanel.transform, "RESTART", new Vector2(0, -60), OnRestartButton);
        CreateButton(gameOverPanel.transform, "QUIT", new Vector2(0, -120), OnQuitButton);
    }

    GameObject CreatePanel(string name)
    {
        GameObject panel = new GameObject(name, typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(transform, false);
        RectTransform rt = panel.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        panel.GetComponent<Image>().color = new Color(0, 0, 0, 0.75f);
        return panel;
    }

    GameObject CreateText(Transform parent, string content, int fontSize, Vector2 pos)
    {
        GameObject go = new GameObject("Text", typeof(RectTransform), typeof(Text));
        go.transform.SetParent(parent, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(600, 80);
        Text t = go.GetComponent<Text>();
        t.text = content;
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.fontSize = fontSize;
        t.color = Color.white;
        t.alignment = TextAnchor.MiddleCenter;
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        return go;
    }

    void CreateButton(Transform parent, string label, Vector2 pos, UnityEngine.Events.UnityAction action)
    {
        GameObject go = new GameObject("Btn_" + label, typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(200, 50);
        go.GetComponent<Image>().color = new Color(0.2f, 0.4f, 0.8f, 1f);
        go.GetComponent<Button>().onClick.AddListener(action);

        // Button label
        GameObject txt = new GameObject("Label", typeof(RectTransform), typeof(Text));
        txt.transform.SetParent(go.transform, false);
        RectTransform trt = txt.GetComponent<RectTransform>();
        trt.anchorMin = Vector2.zero; trt.anchorMax = Vector2.one;
        trt.offsetMin = Vector2.zero; trt.offsetMax = Vector2.zero;
        Text t = txt.GetComponent<Text>();
        t.text = label;
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.fontSize = 24;
        t.color = Color.white;
        t.alignment = TextAnchor.MiddleCenter;
    }
}

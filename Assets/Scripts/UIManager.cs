using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages all UI: HUD (score, health, wave), main menu, and game over screen.
/// Wire the panels/texts in the inspector, or leave them null to have a basic
/// UI generated automatically at runtime.
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("HUD")]
    public Text scoreText;
    public Text healthText;
    public Text waveText;
    public Slider healthBar;
    public Text waveBanner;

    [Header("Panels")]
    public GameObject mainMenuPanel;
    public GameObject hudPanel;
    public GameObject gameOverPanel;

    [Header("Game Over")]
    public Text finalScoreText;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // If the scene has no UI wired up, build a functional one at runtime.
        if (mainMenuPanel == null || hudPanel == null || gameOverPanel == null)
        {
            RuntimeUIBuilder.Build(this);
        }
    }

    public void OnGameStateChanged(GameManager.GameState state)
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(state == GameManager.GameState.MainMenu);
        if (hudPanel != null) hudPanel.SetActive(state == GameManager.GameState.Playing);
        if (gameOverPanel != null) gameOverPanel.SetActive(state == GameManager.GameState.GameOver);
    }

    public void UpdateScore(int score)
    {
        if (scoreText != null) scoreText.text = "Score: " + score;
    }

    public void UpdateWave(int wave)
    {
        if (waveText != null) waveText.text = "Wave: " + wave;
    }

    public void UpdateHealth(int current, int max)
    {
        if (healthText != null) healthText.text = "HP: " + current + " / " + max;
        if (healthBar != null)
        {
            healthBar.maxValue = max;
            healthBar.value = current;
        }
    }

    public void ShowWaveBanner(int wave)
    {
        if (waveBanner != null)
        {
            StopCoroutine(nameof(WaveBannerRoutine));
            StartCoroutine(WaveBannerRoutine(wave));
        }
    }

    private IEnumerator WaveBannerRoutine(int wave)
    {
        waveBanner.text = "WAVE " + wave;
        waveBanner.gameObject.SetActive(true);
        yield return new WaitForSeconds(1.5f);
        waveBanner.gameObject.SetActive(false);
    }

    public void ShowGameOver(int score, int wave)
    {
        if (finalScoreText != null)
        {
            finalScoreText.text = "Final Score: " + score + "\nWaves Survived: " + wave;
        }
    }

    // Button hooks -----------------------------------------------------------

    public void OnStartButton()
    {
        if (GameManager.Instance != null) GameManager.Instance.StartGame();
    }

    public void OnRestartButton()
    {
        if (GameManager.Instance != null) GameManager.Instance.RestartGame();
    }

    public void OnQuitButton()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}

/// <summary>
/// Builds a complete Canvas-based UI at runtime (main menu, HUD, game over)
/// so the project is playable without manually laying out the UI in the editor.
/// </summary>
public static class RuntimeUIBuilder
{
    public static void Build(UIManager ui)
    {
        // Root canvas.
        GameObject canvasGO = new GameObject("Canvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasGO.AddComponent<GraphicRaycaster>();

        EnsureEventSystem();

        // ---------- HUD ----------
        GameObject hud = CreatePanel("HUDPanel", canvasGO.transform, Color.clear);
        ui.hudPanel = hud;
        ui.scoreText = CreateText("ScoreText", hud.transform, "Score: 0", 36,
            TextAnchor.UpperLeft, new Vector2(0, 1), new Vector2(20, -20), new Vector2(400, 50));
        ui.waveText = CreateText("WaveText", hud.transform, "Wave: 0", 36,
            TextAnchor.UpperRight, new Vector2(1, 1), new Vector2(-20, -20), new Vector2(400, 50));
        ui.healthText = CreateText("HealthText", hud.transform, "HP: 100 / 100", 30,
            TextAnchor.LowerLeft, new Vector2(0, 0), new Vector2(20, 20), new Vector2(400, 50));
        ui.waveBanner = CreateText("WaveBanner", hud.transform, "WAVE 1", 80,
            TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(800, 120));
        ui.waveBanner.color = Color.yellow;
        ui.waveBanner.gameObject.SetActive(false);

        // ---------- Main Menu ----------
        GameObject menu = CreatePanel("MainMenuPanel", canvasGO.transform, new Color(0f, 0f, 0.1f, 0.85f));
        ui.mainMenuPanel = menu;
        CreateText("Title", menu.transform, "SPACE SHOOTER", 90,
            TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0, 200), new Vector2(1200, 150));
        CreateText("Hint", menu.transform, "Move: WASD / Arrows    Shoot: Space",
            32, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0, 80), new Vector2(1200, 60));
        CreateButton("StartButton", menu.transform, "START", new Vector2(0, -40), ui.OnStartButton);
        CreateButton("QuitButton", menu.transform, "QUIT", new Vector2(0, -180), ui.OnQuitButton);

        // ---------- Game Over ----------
        GameObject over = CreatePanel("GameOverPanel", canvasGO.transform, new Color(0.1f, 0f, 0f, 0.85f));
        ui.gameOverPanel = over;
        CreateText("GameOverTitle", over.transform, "GAME OVER", 90,
            TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0, 200), new Vector2(1200, 150));
        ui.finalScoreText = CreateText("FinalScore", over.transform, "Final Score: 0",
            40, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0, 60), new Vector2(1200, 120));
        CreateButton("RestartButton", over.transform, "RESTART", new Vector2(0, -60), ui.OnRestartButton);
        CreateButton("QuitButton2", over.transform, "QUIT", new Vector2(0, -200), ui.OnQuitButton);

        // Start with only the menu visible.
        hud.SetActive(false);
        over.SetActive(false);
        menu.SetActive(true);
    }

    private static void EnsureEventSystem()
    {
        if (Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<UnityEngine.EventSystems.EventSystem>();
            es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }
    }

    private static GameObject CreatePanel(string name, Transform parent, Color color)
    {
        GameObject panel = new GameObject(name, typeof(RectTransform));
        panel.transform.SetParent(parent, false);
        Image img = panel.AddComponent<Image>();
        img.color = color;
        RectTransform rt = panel.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        return panel;
    }

    private static Text CreateText(string name, Transform parent, string content, int fontSize,
        TextAnchor anchor, Vector2 pivotAnchor, Vector2 anchoredPos, Vector2 size)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        Text txt = go.AddComponent<Text>();
        txt.text = content;
        txt.fontSize = fontSize;
        txt.alignment = anchor;
        txt.color = Color.white;
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (txt.font == null)
        {
            txt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        }

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = pivotAnchor;
        rt.anchorMax = pivotAnchor;
        rt.pivot = pivotAnchor;
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;
        return txt;
    }

    private static void CreateButton(string name, Transform parent, string label,
        Vector2 anchoredPos, UnityEngine.Events.UnityAction onClick)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        Image img = go.AddComponent<Image>();
        img.color = new Color(0.2f, 0.5f, 0.9f, 1f);

        Button btn = go.AddComponent<Button>();
        btn.onClick.AddListener(onClick);

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = new Vector2(360, 100);

        Text txt = CreateText(name + "Label", go.transform, label, 44,
            TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(360, 100));
        txt.color = Color.white;
    }
}

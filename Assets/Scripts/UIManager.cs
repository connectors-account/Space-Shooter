using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages all UI elements: HUD (score, health, wave, lives),
/// main menu panel, pause panel, and game-over panel.
/// Listens to GameManager events to show/hide panels.
/// </summary>
public class UIManager : MonoBehaviour
{
    // ── HUD ─────────────────────────────────────────────────────────────
    [Header("HUD Elements")]
    [SerializeField] private Text scoreText;
    [SerializeField] private Text highScoreText;
    [SerializeField] private Text waveText;
    [SerializeField] private Text livesText;
    [SerializeField] private Slider healthBar;
    [SerializeField] private Text healthText;

    // ── Panels ──────────────────────────────────────────────────────────
    [Header("Panels")]
    [SerializeField] private GameObject hudPanel;
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject gameOverPanel;

    [Header("Game Over")]
    [SerializeField] private Text finalScoreText;
    [SerializeField] private Text finalHighScoreText;

    // ── Buttons (assign in Inspector) ───────────────────────────────────
    [Header("Buttons")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button quitButton;

    // ────────────────────────────────────────────────────────────────────
    // Unity Lifecycle
    // ────────────────────────────────────────────────────────────────────
    private void Awake()
    {
        // Wire up button callbacks
        if (startButton != null) startButton.onClick.AddListener(OnStartClicked);
        if (resumeButton != null) resumeButton.onClick.AddListener(OnResumeClicked);
        if (restartButton != null) restartButton.onClick.AddListener(OnRestartClicked);
        if (mainMenuButton != null) mainMenuButton.onClick.AddListener(OnMainMenuClicked);
        if (quitButton != null) quitButton.onClick.AddListener(OnQuitClicked);
    }

    private void Start()
    {
        // Subscribe to GameManager events
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnScoreChanged += UpdateScore;
            GameManager.Instance.OnGameStateChanged += OnGameStateChanged;
            GameManager.Instance.OnWaveChanged += UpdateWave;
        }

        // Show main menu initially
        ShowMainMenu();
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnScoreChanged -= UpdateScore;
            GameManager.Instance.OnGameStateChanged -= OnGameStateChanged;
            GameManager.Instance.OnWaveChanged -= UpdateWave;
        }
    }

    // ────────────────────────────────────────────────────────────────────
    // Panel Management
    // ────────────────────────────────────────────────────────────────────
    private void SetPanels(bool hud, bool menu, bool pause, bool gameOver)
    {
        if (hudPanel != null) hudPanel.SetActive(hud);
        if (mainMenuPanel != null) mainMenuPanel.SetActive(menu);
        if (pausePanel != null) pausePanel.SetActive(pause);
        if (gameOverPanel != null) gameOverPanel.SetActive(gameOver);
    }

    private void ShowMainMenu()
    {
        SetPanels(false, true, false, false);
    }

    private void ShowHUD()
    {
        SetPanels(true, false, false, false);
    }

    private void ShowPause()
    {
        SetPanels(true, false, true, false);
    }

    private void ShowGameOver()
    {
        SetPanels(true, false, false, true);

        if (finalScoreText != null && GameManager.Instance != null)
            finalScoreText.text = $"Score: {GameManager.Instance.Score}";
        if (finalHighScoreText != null && GameManager.Instance != null)
            finalHighScoreText.text = $"Best: {GameManager.Instance.HighScore}";
    }

    // ────────────────────────────────────────────────────────────────────
    // Event Handlers
    // ────────────────────────────────────────────────────────────────────
    private void OnGameStateChanged(GameManager.GameState state)
    {
        switch (state)
        {
            case GameManager.GameState.MainMenu:
                ShowMainMenu();
                break;
            case GameManager.GameState.Playing:
                ShowHUD();
                break;
            case GameManager.GameState.Paused:
                ShowPause();
                break;
            case GameManager.GameState.GameOver:
                ShowGameOver();
                break;
        }
    }

    private void UpdateScore(int score)
    {
        if (scoreText != null)
            scoreText.text = $"Score: {score}";
        if (highScoreText != null && GameManager.Instance != null)
            highScoreText.text = $"Best: {GameManager.Instance.HighScore}";
    }

    private void UpdateWave(int wave)
    {
        if (waveText != null)
            waveText.text = $"Wave {wave}";
    }

    /// <summary>Call from PlayerController's HealthSystem.OnHealthChanged.</summary>
    public void UpdateHealth(int current, int max)
    {
        if (healthBar != null)
        {
            healthBar.maxValue = max;
            healthBar.value = current;
        }
        if (healthText != null)
            healthText.text = $"{current}/{max}";
    }

    /// <summary>Update lives display.</summary>
    public void UpdateLives(int lives)
    {
        if (livesText != null)
            livesText.text = $"Lives: {lives}";
    }

    // ────────────────────────────────────────────────────────────────────
    // Button Callbacks
    // ────────────────────────────────────────────────────────────────────
    private void OnStartClicked()
    {
        GameManager.Instance?.StartGame();
    }

    private void OnResumeClicked()
    {
        GameManager.Instance?.TogglePause();
    }

    private void OnRestartClicked()
    {
        GameManager.Instance?.RestartGame();
    }

    private void OnMainMenuClicked()
    {
        GameManager.Instance?.ReturnToMainMenu();
    }

    private void OnQuitClicked()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}

// ============================================================================
// UIManager.cs - All UI management: HUD, menus, game over screen
// ============================================================================
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages all in-game UI: the HUD (score, health, wave), pause menu,
/// and game-over screen. Subscribes to GameManager events.
/// Attach to a Canvas GameObject in the GameScene.
/// </summary>
public class UIManager : MonoBehaviour
{
    // ---- HUD Elements ----
    [Header("HUD")]
    public Text scoreText;
    public Text waveText;
    public Text healthText;
    public Slider healthBar;
    public Text highScoreText;

    // ---- Pause Menu ----
    [Header("Pause Menu")]
    public GameObject pausePanel;
    public Button resumeButton;
    public Button pauseMainMenuButton;

    // ---- Game Over Screen ----
    [Header("Game Over")]
    public GameObject gameOverPanel;
    public Text finalScoreText;
    public Text gameOverHighScoreText;
    public Button restartButton;
    public Button gameOverMainMenuButton;

    // ---- Wave Announcement ----
    [Header("Wave Announcement")]
    public Text waveAnnouncementText;
    private float _waveAnnouncementTimer;

    // ---- Player reference (for health display) ----
    private HealthSystem _playerHealth;

    // ========================================================================
    // Unity Lifecycle
    // ========================================================================
    private void Start()
    {
        // Find player health
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            _playerHealth = player.GetComponent<HealthSystem>();

        // Subscribe to GameManager events
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnScoreChanged += UpdateScoreDisplay;
            GameManager.Instance.OnWaveChanged += ShowWaveAnnouncement;
            GameManager.Instance.OnGameStateChanged += HandleStateChange;
        }

        // Subscribe to player health events
        if (_playerHealth != null)
        {
            _playerHealth.OnDamaged += (dmg, curr) => UpdateHealthDisplay();
            _playerHealth.OnHealed += (heal, curr) => UpdateHealthDisplay();
        }

        // Wire up buttons
        if (resumeButton != null)
            resumeButton.onClick.AddListener(OnResumeClicked);
        if (pauseMainMenuButton != null)
            pauseMainMenuButton.onClick.AddListener(OnMainMenuClicked);
        if (restartButton != null)
            restartButton.onClick.AddListener(OnRestartClicked);
        if (gameOverMainMenuButton != null)
            gameOverMainMenuButton.onClick.AddListener(OnMainMenuClicked);

        // Initial UI state
        if (pausePanel != null) pausePanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (waveAnnouncementText != null) waveAnnouncementText.gameObject.SetActive(false);

        UpdateScoreDisplay(0);
        UpdateHealthDisplay();
    }

    private void Update()
    {
        // Handle pause input (Escape key is handled in PlayerController,
        // but we also listen here for resume)
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (GameManager.Instance != null &&
                GameManager.Instance.CurrentState == GameManager.GameState.Paused)
            {
                OnResumeClicked();
            }
        }

        // Wave announcement fade-out timer
        if (_waveAnnouncementTimer > 0)
        {
            _waveAnnouncementTimer -= Time.unscaledDeltaTime;
            if (_waveAnnouncementTimer <= 0 && waveAnnouncementText != null)
            {
                waveAnnouncementText.gameObject.SetActive(false);
            }
        }
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnScoreChanged -= UpdateScoreDisplay;
            GameManager.Instance.OnWaveChanged -= ShowWaveAnnouncement;
            GameManager.Instance.OnGameStateChanged -= HandleStateChange;
        }
    }

    // ========================================================================
    // Display Updates
    // ========================================================================

    private void UpdateScoreDisplay(int score)
    {
        if (scoreText != null)
            scoreText.text = $"SCORE: {score}";
        if (highScoreText != null && GameManager.Instance != null)
            highScoreText.text = $"HIGH: {GameManager.Instance.HighScore}";
    }

    private void UpdateHealthDisplay()
    {
        if (_playerHealth == null) return;

        if (healthText != null)
            healthText.text = $"HP: {_playerHealth.CurrentHealth}/{_playerHealth.maxHealth}";

        if (healthBar != null)
        {
            healthBar.maxValue = _playerHealth.maxHealth;
            healthBar.value = _playerHealth.CurrentHealth;
        }
    }

    private void ShowWaveAnnouncement(int wave)
    {
        if (waveText != null)
            waveText.text = $"WAVE: {wave}";

        if (waveAnnouncementText != null)
        {
            waveAnnouncementText.text = $"WAVE {wave}";
            waveAnnouncementText.gameObject.SetActive(true);
            _waveAnnouncementTimer = 2.5f; // Show for 2.5 seconds
        }
    }

    // ========================================================================
    // State Change Handler
    // ========================================================================

    private void HandleStateChange(GameManager.GameState newState)
    {
        switch (newState)
        {
            case GameManager.GameState.Playing:
                if (pausePanel != null) pausePanel.SetActive(false);
                if (gameOverPanel != null) gameOverPanel.SetActive(false);
                break;

            case GameManager.GameState.Paused:
                if (pausePanel != null) pausePanel.SetActive(true);
                break;

            case GameManager.GameState.GameOver:
                ShowGameOverScreen();
                break;
        }
    }

    private void ShowGameOverScreen()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        if (finalScoreText != null && GameManager.Instance != null)
            finalScoreText.text = $"FINAL SCORE: {GameManager.Instance.Score}";

        if (gameOverHighScoreText != null && GameManager.Instance != null)
            gameOverHighScoreText.text = $"HIGH SCORE: {GameManager.Instance.HighScore}";
    }

    // ========================================================================
    // Button Handlers
    // ========================================================================

    public void OnResumeClicked()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.ResumeGame();
    }

    public void OnRestartClicked()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.StartGame();
    }

    public void OnMainMenuClicked()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.GoToMainMenu();
    }
}

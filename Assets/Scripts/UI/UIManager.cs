using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UIManager handles all UI elements and updates.
/// Central controller for HUD, menus, and UI events.
/// </summary>
public class UIManager : MonoBehaviour
{
    // Singleton instance
    public static UIManager Instance { get; private set; }

    [Header("HUD Elements")]
    [SerializeField] private GameObject hudPanel;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI highScoreText;
    [SerializeField] private TextMeshProUGUI waveText;
    [SerializeField] private TextMeshProUGUI comboText;
    [SerializeField] private HealthDisplay healthDisplay;

    [Header("Menu Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject waveAnnouncementPanel;

    [Header("Game Over Elements")]
    [SerializeField] private TextMeshProUGUI finalScoreText;
    [SerializeField] private TextMeshProUGUI newHighScoreText;
    [SerializeField] private TextMeshProUGUI gameOverWaveText;

    [Header("Wave Announcement")]
    [SerializeField] private TextMeshProUGUI waveAnnouncementText;
    [SerializeField] private float waveAnnouncementDuration = 2f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void OnEnable()
    {
        // Subscribe to events
        ScoreManager.OnScoreChanged += UpdateScoreDisplay;
        ScoreManager.OnHighScoreChanged += UpdateHighScoreDisplay;
        ScoreManager.OnComboChanged += UpdateComboDisplay;
        PlayerHealth.OnHealthChanged += UpdateHealthDisplay;
        WaveManager.OnWaveStart += ShowWaveAnnouncement;
        WaveManager.OnBossWaveStart += ShowBossWarning;
        GameManager.OnGameStart += OnGameStart;
        GameManager.OnGamePause += ShowPauseMenu;
        GameManager.OnGameResume += HidePauseMenu;
        GameManager.OnGameOver += ShowGameOverScreen;
    }

    private void OnDisable()
    {
        // Unsubscribe from events
        ScoreManager.OnScoreChanged -= UpdateScoreDisplay;
        ScoreManager.OnHighScoreChanged -= UpdateHighScoreDisplay;
        ScoreManager.OnComboChanged -= UpdateComboDisplay;
        PlayerHealth.OnHealthChanged -= UpdateHealthDisplay;
        WaveManager.OnWaveStart -= ShowWaveAnnouncement;
        WaveManager.OnBossWaveStart -= ShowBossWarning;
        GameManager.OnGameStart -= OnGameStart;
        GameManager.OnGamePause -= ShowPauseMenu;
        GameManager.OnGameResume -= HidePauseMenu;
        GameManager.OnGameOver -= ShowGameOverScreen;
    }

    private void Start()
    {
        // Initialize UI state
        InitializeUI();
    }

    /// <summary>
    /// Initialize UI to default state
    /// </summary>
    private void InitializeUI()
    {
        // Hide all panels initially
        if (hudPanel != null) hudPanel.SetActive(false);
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (waveAnnouncementPanel != null) waveAnnouncementPanel.SetActive(false);
        if (comboText != null) comboText.gameObject.SetActive(false);

        // Set initial text values
        UpdateScoreDisplay(0);
        UpdateHighScoreDisplay(ScoreManager.Instance != null ? ScoreManager.Instance.HighScore : 0);
        UpdateWaveDisplay(0);
    }

    /// <summary>
    /// Called when game starts
    /// </summary>
    private void OnGameStart()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (hudPanel != null) hudPanel.SetActive(true);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        UpdateWaveDisplay(0);
    }

    /// <summary>
    /// Update score display
    /// </summary>
    private void UpdateScoreDisplay(int score)
    {
        if (scoreText != null)
        {
            scoreText.text = $"SCORE: {score:N0}";
        }
    }

    /// <summary>
    /// Update high score display
    /// </summary>
    private void UpdateHighScoreDisplay(int highScore)
    {
        if (highScoreText != null)
        {
            highScoreText.text = $"HIGH: {highScore:N0}";
        }
    }

    /// <summary>
    /// Update wave display
    /// </summary>
    public void UpdateWaveDisplay(int wave)
    {
        if (waveText != null)
        {
            waveText.text = wave > 0 ? $"WAVE {wave}" : "";
        }
    }

    /// <summary>
    /// Update combo display
    /// </summary>
    private void UpdateComboDisplay(int combo)
    {
        if (comboText != null)
        {
            if (combo > 1)
            {
                comboText.gameObject.SetActive(true);
                comboText.text = $"COMBO x{combo}";
            }
            else
            {
                comboText.gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Update health display
    /// </summary>
    private void UpdateHealthDisplay(int current, int max)
    {
        if (healthDisplay != null)
        {
            healthDisplay.UpdateHealth(current, max);
        }
    }

    /// <summary>
    /// Show wave announcement
    /// </summary>
    private void ShowWaveAnnouncement(int waveNumber)
    {
        UpdateWaveDisplay(waveNumber);
        
        if (waveAnnouncementPanel != null && waveAnnouncementText != null)
        {
            waveAnnouncementText.text = $"WAVE {waveNumber}";
            waveAnnouncementPanel.SetActive(true);
            StartCoroutine(HideWaveAnnouncementAfterDelay());
        }
    }

    /// <summary>
    /// Show boss warning
    /// </summary>
    private void ShowBossWarning()
    {
        if (waveAnnouncementPanel != null && waveAnnouncementText != null)
        {
            waveAnnouncementText.text = "⚠ BOSS INCOMING ⚠";
            waveAnnouncementText.color = Color.red;
            waveAnnouncementPanel.SetActive(true);
            StartCoroutine(HideWaveAnnouncementAfterDelay());
        }
    }

    /// <summary>
    /// Hide wave announcement after delay
    /// </summary>
    private System.Collections.IEnumerator HideWaveAnnouncementAfterDelay()
    {
        yield return new WaitForSeconds(waveAnnouncementDuration);
        
        if (waveAnnouncementPanel != null)
        {
            waveAnnouncementPanel.SetActive(false);
        }
        
        if (waveAnnouncementText != null)
        {
            waveAnnouncementText.color = Color.white;
        }
    }

    /// <summary>
    /// Show pause menu
    /// </summary>
    private void ShowPauseMenu()
    {
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(true);
        }
    }

    /// <summary>
    /// Hide pause menu
    /// </summary>
    private void HidePauseMenu()
    {
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }
    }

    /// <summary>
    /// Show game over screen
    /// </summary>
    private void ShowGameOverScreen()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
        
        if (hudPanel != null)
        {
            hudPanel.SetActive(false);
        }

        // Update final score
        if (ScoreManager.Instance != null)
        {
            if (finalScoreText != null)
            {
                finalScoreText.text = $"FINAL SCORE: {ScoreManager.Instance.GetFinalScore():N0}";
            }

            // Show new high score message if applicable
            if (newHighScoreText != null)
            {
                newHighScoreText.gameObject.SetActive(ScoreManager.Instance.IsNewHighScore());
            }
        }

        // Show wave reached
        if (WaveManager.Instance != null && gameOverWaveText != null)
        {
            gameOverWaveText.text = $"WAVE REACHED: {WaveManager.Instance.CurrentWave}";
        }
    }

    // UI Button Callbacks

    /// <summary>
    /// Called when Play button is pressed
    /// </summary>
    public void OnPlayButtonPressed()
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySound("ButtonClick");
        }
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartGame();
        }
    }

    /// <summary>
    /// Called when Resume button is pressed
    /// </summary>
    public void OnResumeButtonPressed()
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySound("ButtonClick");
        }
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResumeGame();
        }
    }

    /// <summary>
    /// Called when Restart button is pressed
    /// </summary>
    public void OnRestartButtonPressed()
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySound("ButtonClick");
        }
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RestartGame();
        }
    }

    /// <summary>
    /// Called when Main Menu button is pressed
    /// </summary>
    public void OnMainMenuButtonPressed()
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySound("ButtonClick");
        }
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ReturnToMainMenu();
        }
    }

    /// <summary>
    /// Called when Quit button is pressed
    /// </summary>
    public void OnQuitButtonPressed()
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySound("ButtonClick");
        }
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.QuitGame();
        }
    }
}

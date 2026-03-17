using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Manages all UI elements: HUD (score, health, wave), pause menu, game over screen.
/// Attach to a Canvas GameObject in the GamePlay scene.
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("HUD Elements")]
    [SerializeField] private Text scoreText;
    [SerializeField] private Text waveText;
    [SerializeField] private Slider healthBar;
    [SerializeField] private Image healthBarFill;

    [Header("Wave Announcement")]
    [SerializeField] private GameObject waveAnnouncementPanel;
    [SerializeField] private Text waveAnnouncementText;
    [SerializeField] private float announcementDuration = 2f;

    [Header("Pause Menu")]
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button pauseMainMenuButton;
    [SerializeField] private Button pauseQuitButton;

    [Header("Game Over Screen")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private Text finalScoreText;
    [SerializeField] private Text highScoreText;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button gameOverMainMenuButton;

    private void Awake()
    {
        // Singleton (scene-level, does not persist)
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Hide overlay panels
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (waveAnnouncementPanel != null) waveAnnouncementPanel.SetActive(false);
    }

    private void Start()
    {
        SetupButtons();
    }

    /// <summary>
    /// Wire up button click listeners.
    /// </summary>
    private void SetupButtons()
    {
        if (resumeButton != null)
            resumeButton.onClick.AddListener(() => {
                if (GameManager.Instance != null) GameManager.Instance.ResumeGame();
            });

        if (pauseMainMenuButton != null)
            pauseMainMenuButton.onClick.AddListener(() => {
                if (GameManager.Instance != null) GameManager.Instance.GoToMainMenu();
            });

        if (pauseQuitButton != null)
            pauseQuitButton.onClick.AddListener(() => {
                if (GameManager.Instance != null) GameManager.Instance.QuitGame();
            });

        if (restartButton != null)
            restartButton.onClick.AddListener(() => {
                if (GameManager.Instance != null) GameManager.Instance.RestartGame();
            });

        if (gameOverMainMenuButton != null)
            gameOverMainMenuButton.onClick.AddListener(() => {
                if (GameManager.Instance != null) GameManager.Instance.GoToMainMenu();
            });
    }

    /// <summary>
    /// Update the score display.
    /// </summary>
    public void UpdateScoreText(int score)
    {
        if (scoreText != null)
            scoreText.text = "Score: " + score.ToString("N0");
    }

    /// <summary>
    /// Update the wave number display.
    /// </summary>
    public void UpdateWaveText(int wave)
    {
        if (waveText != null)
            waveText.text = "Wave: " + wave;
    }

    /// <summary>
    /// Update the health bar slider and color.
    /// </summary>
    public void UpdateHealthBar(int currentHealth, int maxHealth)
    {
        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = currentHealth;
        }

        // Change color based on health percentage
        if (healthBarFill != null)
        {
            float healthPercent = (float)currentHealth / maxHealth;
            if (healthPercent > 0.6f)
                healthBarFill.color = Color.green;
            else if (healthPercent > 0.3f)
                healthBarFill.color = Color.yellow;
            else
                healthBarFill.color = Color.red;
        }
    }

    /// <summary>
    /// Display a wave announcement that fades out.
    /// </summary>
    public void ShowWaveAnnouncement(int waveNumber)
    {
        if (waveAnnouncementPanel == null || waveAnnouncementText == null) return;

        waveAnnouncementText.text = "WAVE " + waveNumber;
        waveAnnouncementPanel.SetActive(true);
        StartCoroutine(HideAnnouncementRoutine());
    }

    private IEnumerator HideAnnouncementRoutine()
    {
        yield return new WaitForSeconds(announcementDuration);
        if (waveAnnouncementPanel != null)
            waveAnnouncementPanel.SetActive(false);
    }

    /// <summary>
    /// Show or hide the pause menu.
    /// </summary>
    public void ShowPauseMenu(bool show)
    {
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(show);
    }

    /// <summary>
    /// Show the game over screen with final and high scores.
    /// </summary>
    public void ShowGameOverScreen(int finalScore, int highScore)
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);

            if (finalScoreText != null)
                finalScoreText.text = "Score: " + finalScore.ToString("N0");

            if (highScoreText != null)
                highScoreText.text = "High Score: " + highScore.ToString("N0");
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}

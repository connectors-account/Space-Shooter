using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Manages all UI elements: HUD, pause menu, game over screen.
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("HUD Elements")]
    [SerializeField] private Text scoreText;
    [SerializeField] private Text waveText;
    [SerializeField] private Text livesText;
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Text healthText;

    [Header("Wave Announcement")]
    [SerializeField] private GameObject waveAnnouncement;
    [SerializeField] private Text waveAnnouncementText;
    [SerializeField] private float announcementDuration = 2f;

    [Header("Pause Menu")]
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button pauseMainMenuButton;
    [SerializeField] private Button pauseQuitButton;

    [Header("Game Over Screen")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private Text gameOverScoreText;
    [SerializeField] private Text gameOverHighScoreText;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button gameOverMainMenuButton;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        // Setup button listeners
        if (resumeButton != null)
            resumeButton.onClick.AddListener(() => GameManager.Instance?.ResumeGame());
        if (pauseMainMenuButton != null)
            pauseMainMenuButton.onClick.AddListener(() => GameManager.Instance?.ReturnToMainMenu());
        if (pauseQuitButton != null)
            pauseQuitButton.onClick.AddListener(() => GameManager.Instance?.QuitGame());
        if (restartButton != null)
            restartButton.onClick.AddListener(() => GameManager.Instance?.RestartGame());
        if (gameOverMainMenuButton != null)
            gameOverMainMenuButton.onClick.AddListener(() => GameManager.Instance?.ReturnToMainMenu());

        // Hide panels
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (waveAnnouncement != null) waveAnnouncement.SetActive(false);

        // Initialize HUD
        UpdateScore(0);
        UpdateWave(1);
        UpdateLives(GameManager.Instance != null ? GameManager.Instance.Lives : 3);
    }

    public void UpdateScore(int score)
    {
        if (scoreText != null)
            scoreText.text = "SCORE: " + score.ToString("N0");
    }

    public void UpdateWave(int wave)
    {
        if (waveText != null)
            waveText.text = "WAVE: " + wave;
    }

    public void UpdateHealth(int current, int max)
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = max;
            healthSlider.value = current;
        }
        if (healthText != null)
            healthText.text = current + " / " + max;
    }

    public void UpdateLives(int lives)
    {
        if (livesText != null)
            livesText.text = "LIVES: " + lives;
    }

    public void ShowWaveAnnouncement(int wave)
    {
        if (waveAnnouncement != null && waveAnnouncementText != null)
        {
            waveAnnouncementText.text = "WAVE " + wave;
            waveAnnouncement.SetActive(true);
            StartCoroutine(HideAnnouncementAfterDelay());
        }
    }

    private IEnumerator HideAnnouncementAfterDelay()
    {
        yield return new WaitForSecondsRealtime(announcementDuration);
        if (waveAnnouncement != null)
            waveAnnouncement.SetActive(false);
    }

    public void ShowPauseMenu(bool show)
    {
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(show);
    }

    public void ShowGameOver(int finalScore, int highScore)
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            if (gameOverScoreText != null)
                gameOverScoreText.text = "SCORE: " + finalScore.ToString("N0");
            if (gameOverHighScoreText != null)
                gameOverHighScoreText.text = "HIGH SCORE: " + highScore.ToString("N0");
        }
    }

    public void HideAllPanels()
    {
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (waveAnnouncement != null) waveAnnouncement.SetActive(false);
    }
}

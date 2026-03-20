using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages all HUD elements: score, health, wave display.
/// Attach to a Canvas GameObject. Wire up references in Inspector.
/// </summary>
public class UIManager : MonoBehaviour
{
    [Header("HUD Panel")]
    [SerializeField] private GameObject hudPanel;
    [SerializeField] private Text scoreText;
    [SerializeField] private Text waveText;
    [SerializeField] private Text healthText;
    [SerializeField] private Slider healthSlider;

    [Header("Pause Menu")]
    [SerializeField] private GameObject pauseMenuPanel;

    [Header("Game Over Panel")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private Text finalScoreText;
    [SerializeField] private Text highScoreText;

    [Header("Wave Announcement")]
    [SerializeField] private Text waveAnnouncementText;
    [SerializeField] private float announcementDuration = 2f;

    private float announcementTimer = 0f;

    private void Start()
    {
        // Initially hide overlays
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (waveAnnouncementText != null) waveAnnouncementText.gameObject.SetActive(false);
    }

    private void Update()
    {
        // Fade out wave announcement
        if (waveAnnouncementText != null && waveAnnouncementText.gameObject.activeSelf)
        {
            announcementTimer -= Time.deltaTime;
            if (announcementTimer <= 0f)
            {
                waveAnnouncementText.gameObject.SetActive(false);
            }
            else
            {
                // Fade out effect
                Color c = waveAnnouncementText.color;
                c.a = Mathf.Clamp01(announcementTimer / (announcementDuration * 0.5f));
                waveAnnouncementText.color = c;
            }
        }
    }

    /// <summary>
    /// Shows the in-game HUD.
    /// </summary>
    public void ShowHUD()
    {
        if (hudPanel != null) hudPanel.SetActive(true);
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
    }

    /// <summary>
    /// Updates the score display.
    /// </summary>
    public void UpdateScore(int score)
    {
        if (scoreText != null)
            scoreText.text = "Score: " + score.ToString("N0");
    }

    /// <summary>
    /// Updates the wave display and shows an announcement.
    /// </summary>
    public void UpdateWave(int wave)
    {
        if (waveText != null)
            waveText.text = "Wave: " + wave;

        // Show wave announcement
        if (waveAnnouncementText != null)
        {
            waveAnnouncementText.text = "WAVE " + wave;
            waveAnnouncementText.color = new Color(1f, 1f, 0f, 1f); // Yellow
            waveAnnouncementText.gameObject.SetActive(true);
            announcementTimer = announcementDuration;
        }
    }

    /// <summary>
    /// Updates the health bar and text.
    /// </summary>
    public void UpdateHealthDisplay(int current, int max)
    {
        if (healthText != null)
            healthText.text = "HP: " + current + "/" + max;

        if (healthSlider != null)
        {
            healthSlider.maxValue = max;
            healthSlider.value = current;
        }
    }

    /// <summary>
    /// Shows the pause menu.
    /// </summary>
    public void ShowPauseMenu()
    {
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(true);
    }

    /// <summary>
    /// Hides the pause menu.
    /// </summary>
    public void HidePauseMenu()
    {
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
    }

    /// <summary>
    /// Shows the game over screen with final and high scores.
    /// </summary>
    public void ShowGameOver(int finalScore, int highScore)
    {
        if (hudPanel != null) hudPanel.SetActive(false);
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            if (finalScoreText != null)
                finalScoreText.text = "Final Score: " + finalScore.ToString("N0");
            if (highScoreText != null)
                highScoreText.text = "High Score: " + highScore.ToString("N0");
        }
    }

    // --- Button Callbacks ---

    public void OnResumeButton()
    {
        GameManager.Instance?.ResumeGame();
    }

    public void OnRestartButton()
    {
        GameManager.Instance?.RestartGame();
    }

    public void OnMainMenuButton()
    {
        GameManager.Instance?.GoToMainMenu();
    }

    public void OnQuitButton()
    {
        GameManager.Instance?.QuitGame();
    }
}

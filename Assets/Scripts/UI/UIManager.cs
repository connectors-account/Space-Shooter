using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Manages all in-game UI: HUD, pause menu, game over screen, wave announcements.
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("HUD")]
    public Text scoreText;
    public Text highScoreText;
    public Text healthText;
    public Text waveText;
    public Text comboText;

    [Header("Wave Announcement")]
    public GameObject waveAnnouncementPanel;
    public Text waveAnnouncementText;

    [Header("Game Over")]
    public GameObject gameOverPanel;
    public Text finalScoreText;
    public Text finalHighScoreText;
    public Button restartButton;
    public Button mainMenuButton;

    [Header("Pause Menu")]
    public GameObject pausePanel;
    public Button resumeButton;
    public Button pauseMainMenuButton;
    public Button quitButton;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        // Hide overlay panels
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);
        if (waveAnnouncementPanel != null) waveAnnouncementPanel.SetActive(false);
        if (comboText != null) comboText.gameObject.SetActive(false);

        // Wire up buttons
        if (restartButton != null)
            restartButton.onClick.AddListener(() => GameManager.Instance?.RestartGame());
        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(() => GameManager.Instance?.ReturnToMainMenu());
        if (resumeButton != null)
            resumeButton.onClick.AddListener(() => GameManager.Instance?.ResumeGame());
        if (pauseMainMenuButton != null)
            pauseMainMenuButton.onClick.AddListener(() => GameManager.Instance?.ReturnToMainMenu());
        if (quitButton != null)
            quitButton.onClick.AddListener(() => GameManager.Instance?.QuitGame());

        UpdateScoreDisplay(0);
        if (highScoreText != null)
            highScoreText.text = "HI: " + PlayerPrefs.GetInt("HighScore", 0);
    }

    public void UpdateScoreDisplay(int score)
    {
        if (scoreText != null)
            scoreText.text = "SCORE: " + score;
    }

    public void UpdateHealthDisplay(int current, int max)
    {
        if (healthText != null)
        {
            string hearts = "";
            for (int i = 0; i < max; i++)
                hearts += i < current ? "♥ " : "♡ ";
            healthText.text = hearts.Trim();
        }
    }

    public void ShowWaveAnnouncement(int wave)
    {
        if (waveText != null)
            waveText.text = "WAVE " + wave;

        if (waveAnnouncementPanel != null)
        {
            waveAnnouncementText.text = "WAVE " + wave;
            StartCoroutine(ShowAnnouncementCoroutine());
        }
    }

    IEnumerator ShowAnnouncementCoroutine()
    {
        waveAnnouncementPanel.SetActive(true);
        yield return new WaitForSeconds(1.5f);
        if (waveAnnouncementPanel != null)
            waveAnnouncementPanel.SetActive(false);
    }

    public void ShowWaveClear()
    {
        if (waveAnnouncementPanel != null && waveAnnouncementText != null)
        {
            waveAnnouncementText.text = "WAVE CLEAR!";
            StartCoroutine(ShowAnnouncementCoroutine());
        }
    }

    public void ShowCombo(int combo)
    {
        if (comboText != null)
        {
            comboText.gameObject.SetActive(true);
            comboText.text = combo + "x COMBO!";
            StartCoroutine(HideComboAfterDelay());
        }
    }

    IEnumerator HideComboAfterDelay()
    {
        yield return new WaitForSeconds(1f);
        if (comboText != null)
            comboText.gameObject.SetActive(false);
    }

    public void ShowGameOver()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            if (finalScoreText != null && ScoreManager.Instance != null)
                finalScoreText.text = "SCORE: " + ScoreManager.Instance.CurrentScore;
            if (finalHighScoreText != null && ScoreManager.Instance != null)
                finalHighScoreText.text = "HIGH SCORE: " + ScoreManager.Instance.HighScore;
        }
    }

    public void ShowPauseMenu(bool show)
    {
        if (pausePanel != null)
            pausePanel.SetActive(show);
    }
}

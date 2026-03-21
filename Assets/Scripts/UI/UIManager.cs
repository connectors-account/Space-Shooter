using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Manages all in-game UI elements: HUD, wave announcements, combo display.
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("HUD Elements")]
    public Text scoreText;
    public Text waveText;
    public Text livesText;
    public Slider healthBar;
    public Image healthBarFill;

    [Header("Wave Announcement")]
    public GameObject waveAnnouncement;
    public Text waveAnnouncementText;
    public float announcementDuration = 2f;

    [Header("Combo Display")]
    public Text comboText;
    public float comboDisplayDuration = 1f;

    [Header("Game Over Panel")]
    public GameObject gameOverPanel;
    public Text gameOverScoreText;
    public Text gameOverHighScoreText;
    public Button restartButton;
    public Button mainMenuButton;

    [Header("Pause Panel")]
    public GameObject pausePanel;
    public Button resumeButton;
    public Button pauseMainMenuButton;

    [Header("Colors")]
    public Color healthHighColor = Color.green;
    public Color healthMidColor = Color.yellow;
    public Color healthLowColor = Color.red;

    private Coroutine comboCoroutine;
    private Coroutine waveCoroutine;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // Hide panels at start
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);
        if (waveAnnouncement != null) waveAnnouncement.SetActive(false);
        if (comboText != null) comboText.gameObject.SetActive(false);

        // Wire up buttons
        if (restartButton != null)
            restartButton.onClick.AddListener(() => GameManager.Instance?.RestartGame());
        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(() => GameManager.Instance?.GoToMainMenu());
        if (resumeButton != null)
            resumeButton.onClick.AddListener(() => GameManager.Instance?.TogglePause());
        if (pauseMainMenuButton != null)
            pauseMainMenuButton.onClick.AddListener(() => GameManager.Instance?.GoToMainMenu());

        // Initial UI state
        UpdateScore(0);
        UpdateWave(0);
    }

    private void Update()
    {
        // Handle pause input
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (GameManager.Instance.CurrentState == GameManager.GameState.Playing ||
                GameManager.Instance.CurrentState == GameManager.GameState.Paused)
            {
                GameManager.Instance.TogglePause();
                TogglePausePanel();
            }
        }
    }

    public void UpdateScore(int score)
    {
        if (scoreText != null)
            scoreText.text = $"SCORE: {score:N0}";
    }

    public void UpdateWave(int wave)
    {
        if (waveText != null)
            waveText.text = $"WAVE: {wave}";
    }

    public void UpdateLives(int lives)
    {
        if (livesText != null)
            livesText.text = $"LIVES: {lives}";
    }

    public void UpdateHealth(float percent)
    {
        if (healthBar != null)
            healthBar.value = percent;

        if (healthBarFill != null)
        {
            if (percent > 0.6f)
                healthBarFill.color = healthHighColor;
            else if (percent > 0.3f)
                healthBarFill.color = healthMidColor;
            else
                healthBarFill.color = healthLowColor;
        }
    }

    public void ShowWaveAnnouncement(int wave)
    {
        if (waveAnnouncement == null || waveAnnouncementText == null) return;

        if (waveCoroutine != null) StopCoroutine(waveCoroutine);
        waveCoroutine = StartCoroutine(ShowWaveAnnouncementRoutine(wave));
    }

    private IEnumerator ShowWaveAnnouncementRoutine(int wave)
    {
        waveAnnouncementText.text = $"WAVE {wave}";
        waveAnnouncement.SetActive(true);
        yield return new WaitForSeconds(announcementDuration);
        waveAnnouncement.SetActive(false);
    }

    public void ShowCombo(int multiplier)
    {
        if (comboText == null) return;

        if (comboCoroutine != null) StopCoroutine(comboCoroutine);
        comboCoroutine = StartCoroutine(ShowComboRoutine(multiplier));
    }

    private IEnumerator ShowComboRoutine(int multiplier)
    {
        comboText.text = $"x{multiplier} COMBO!";
        comboText.gameObject.SetActive(true);
        yield return new WaitForSeconds(comboDisplayDuration);
        comboText.gameObject.SetActive(false);
    }

    public void ShowGameOver(int score, int highScore)
    {
        if (gameOverPanel == null) return;

        if (gameOverScoreText != null)
            gameOverScoreText.text = $"SCORE: {score:N0}";
        if (gameOverHighScoreText != null)
            gameOverHighScoreText.text = $"HIGH SCORE: {highScore:N0}";

        gameOverPanel.SetActive(true);
    }

    private void TogglePausePanel()
    {
        if (pausePanel == null) return;

        bool isPaused = GameManager.Instance.CurrentState == GameManager.GameState.Paused;
        pausePanel.SetActive(isPaused);
    }
}

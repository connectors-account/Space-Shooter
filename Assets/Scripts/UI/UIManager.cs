using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Manages all UI elements: HUD, main menu, pause menu, game over screen.
/// Singleton pattern.
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("HUD")]
    [SerializeField] private GameObject hudPanel;
    [SerializeField] private Text scoreText;
    [SerializeField] private Text livesText;
    [SerializeField] private Text waveText;
    [SerializeField] private Image healthBarFill;
    [SerializeField] private GameObject shieldIcon;
    [SerializeField] private Text waveAnnouncementText;

    [Header("Main Menu")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private Text titleText;
    [SerializeField] private Text highScoreMenuText;

    [Header("Pause Menu")]
    [SerializeField] private GameObject pauseMenuPanel;

    [Header("Game Over")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private Text finalScoreText;
    [SerializeField] private Text highScoreText;

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
        ShowMainMenu();
    }

    // --- Main Menu ---
    public void ShowMainMenu()
    {
        SetAllPanels(false);
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        if (highScoreMenuText != null)
        {
            int hs = PlayerPrefs.GetInt("HighScore", 0);
            highScoreMenuText.text = "HIGH SCORE: " + hs;
        }
    }

    public void OnStartButtonClicked()
    {
        SetAllPanels(false);
        GameManager.Instance?.StartGame();
    }

    public void OnQuitButtonClicked()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    // --- HUD ---
    public void ShowHUD()
    {
        SetAllPanels(false);
        if (hudPanel != null) hudPanel.SetActive(true);
    }

    public void UpdateScore(int score)
    {
        if (scoreText != null) scoreText.text = "SCORE: " + score;
    }

    public void UpdateLives(int lives)
    {
        if (livesText != null) livesText.text = "LIVES: " + lives;
    }

    public void UpdateWave(int wave)
    {
        if (waveText != null) waveText.text = "WAVE " + wave;
    }

    public void UpdateHealthBar(float normalizedHealth)
    {
        if (healthBarFill != null)
        {
            healthBarFill.fillAmount = normalizedHealth;
            // Color gradient: green > yellow > red
            healthBarFill.color = Color.Lerp(Color.red, Color.green, normalizedHealth);
        }
    }

    public void UpdateShieldIndicator(bool active)
    {
        if (shieldIcon != null) shieldIcon.SetActive(active);
    }

    public void ShowWaveAnnouncement(int wave)
    {
        if (waveAnnouncementText != null)
        {
            StartCoroutine(WaveAnnouncementCoroutine(wave));
        }
    }

    private IEnumerator WaveAnnouncementCoroutine(int wave)
    {
        waveAnnouncementText.gameObject.SetActive(true);
        waveAnnouncementText.text = "WAVE " + wave;

        // Fade in
        float duration = 0.5f;
        float elapsed = 0f;
        Color c = waveAnnouncementText.color;

        while (elapsed < duration)
        {
            c.a = elapsed / duration;
            waveAnnouncementText.color = c;
            elapsed += Time.deltaTime;
            yield return null;
        }

        c.a = 1f;
        waveAnnouncementText.color = c;

        yield return new WaitForSeconds(1.5f);

        // Fade out
        elapsed = 0f;
        while (elapsed < duration)
        {
            c.a = 1f - (elapsed / duration);
            waveAnnouncementText.color = c;
            elapsed += Time.deltaTime;
            yield return null;
        }

        waveAnnouncementText.gameObject.SetActive(false);
    }

    // --- Pause Menu ---
    public void ShowPauseMenu()
    {
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(true);
    }

    public void HidePauseMenu()
    {
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
    }

    public void OnResumeButtonClicked()
    {
        GameManager.Instance?.ResumeGame();
    }

    public void OnMainMenuButtonClicked()
    {
        GameManager.Instance?.GoToMainMenu();
    }

    // --- Game Over ---
    public void ShowGameOverScreen(int finalScore, int highScore)
    {
        SetAllPanels(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        if (finalScoreText != null) finalScoreText.text = "SCORE: " + finalScore;
        if (highScoreText != null) highScoreText.text = "HIGH SCORE: " + highScore;
    }

    public void OnRestartButtonClicked()
    {
        SetAllPanels(false);
        GameManager.Instance?.RestartGame();
    }

    // --- Utility ---
    private void SetAllPanels(bool active)
    {
        if (hudPanel != null) hudPanel.SetActive(active);
        if (mainMenuPanel != null) mainMenuPanel.SetActive(active);
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(active);
        if (gameOverPanel != null) gameOverPanel.SetActive(active);
    }
}

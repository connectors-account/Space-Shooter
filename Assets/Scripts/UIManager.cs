using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Manages all UI elements: score, health, wave info, game over screen.
/// Singleton pattern for easy access from other scripts.
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Game HUD")]
    public Text scoreText;
    public Text healthText;
    public Text waveText;
    public GameObject healthBar; // Optional: visual health bar

    [Header("Wave Announcement")]
    public Text waveAnnouncementText;
    public float announcementDuration = 2f;

    [Header("Menu Panel")]
    public GameObject menuPanel;
    public Text titleText;
    public Text highScoreMenuText;
    public Button startButton;
    public Button quitButton;

    [Header("Game Over Panel")]
    public GameObject gameOverPanel;
    public Text gameOverText;
    public Text finalScoreText;
    public Text highScoreText;
    public Button restartButton;
    public Button menuQuitButton;

    [Header("Game HUD Panel")]
    public GameObject gameHUDPanel;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        ShowMenu();
    }

    // === Menu ===
    public void ShowMenu()
    {
        if (menuPanel != null) menuPanel.SetActive(true);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (gameHUDPanel != null) gameHUDPanel.SetActive(false);

        if (highScoreMenuText != null)
        {
            int hs = PlayerPrefs.GetInt("HighScore", 0);
            highScoreMenuText.text = "HIGH SCORE: " + hs;
        }

        // Wire up buttons
        if (startButton != null)
        {
            startButton.onClick.RemoveAllListeners();
            startButton.onClick.AddListener(OnStartClicked);
        }
        if (quitButton != null)
        {
            quitButton.onClick.RemoveAllListeners();
            quitButton.onClick.AddListener(OnQuitClicked);
        }
    }

    void OnStartClicked()
    {
        if (menuPanel != null) menuPanel.SetActive(false);
        ShowGameUI();
        if (GameManager.Instance != null)
            GameManager.Instance.StartGame();
    }

    void OnQuitClicked()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.QuitGame();
    }

    // === Game HUD ===
    public void ShowGameUI()
    {
        if (gameHUDPanel != null) gameHUDPanel.SetActive(true);
        if (menuPanel != null) menuPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
    }

    public void UpdateScore(int score)
    {
        if (scoreText != null)
            scoreText.text = "SCORE: " + score;
    }

    public void UpdateHealth(int current, int max)
    {
        if (healthText != null)
            healthText.text = "HP: " + current + " / " + max;

        // Update health bar if present
        if (healthBar != null)
        {
            Image barImage = healthBar.GetComponent<Image>();
            if (barImage != null)
            {
                barImage.fillAmount = (float)current / max;
                // Color changes based on health percentage
                float pct = (float)current / max;
                if (pct > 0.6f) barImage.color = Color.green;
                else if (pct > 0.3f) barImage.color = Color.yellow;
                else barImage.color = Color.red;
            }
        }
    }

    public void UpdateWave(int wave)
    {
        if (waveText != null)
            waveText.text = "WAVE: " + wave;
    }

    public void ShowWaveAnnouncement(int wave)
    {
        if (waveAnnouncementText != null)
        {
            StartCoroutine(WaveAnnouncementCoroutine(wave));
        }
    }

    IEnumerator WaveAnnouncementCoroutine(int wave)
    {
        waveAnnouncementText.text = "WAVE " + wave;
        waveAnnouncementText.gameObject.SetActive(true);

        // Fade in
        float elapsed = 0f;
        Color c = waveAnnouncementText.color;
        while (elapsed < 0.3f)
        {
            c.a = elapsed / 0.3f;
            waveAnnouncementText.color = c;
            elapsed += Time.deltaTime;
            yield return null;
        }
        c.a = 1f;
        waveAnnouncementText.color = c;

        yield return new WaitForSeconds(announcementDuration - 0.6f);

        // Fade out
        elapsed = 0f;
        while (elapsed < 0.3f)
        {
            c.a = 1f - (elapsed / 0.3f);
            waveAnnouncementText.color = c;
            elapsed += Time.deltaTime;
            yield return null;
        }

        waveAnnouncementText.gameObject.SetActive(false);
    }

    // === Game Over ===
    public void ShowGameOver(int finalScore, int highScore)
    {
        if (gameHUDPanel != null) gameHUDPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(true);

        if (finalScoreText != null)
            finalScoreText.text = "SCORE: " + finalScore;
        if (highScoreText != null)
            highScoreText.text = "HIGH SCORE: " + highScore;

        if (restartButton != null)
        {
            restartButton.onClick.RemoveAllListeners();
            restartButton.onClick.AddListener(OnRestartClicked);
        }
        if (menuQuitButton != null)
        {
            menuQuitButton.onClick.RemoveAllListeners();
            menuQuitButton.onClick.AddListener(OnQuitClicked);
        }
    }

    void OnRestartClicked()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.RestartGame();
    }
}

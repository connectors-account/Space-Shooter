using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// UIManager - Singleton that manages all in-game UI: score, health, wave banners, game over screen.
/// Attach to a Canvas GameObject. Set up child UI elements and assign them in the Inspector.
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("HUD Elements")]
    public Text scoreText;
    public Text waveText;
    public Text healthText;
    public Image[] healthIcons;

    [Header("Wave Banner")]
    public GameObject waveBannerPanel;
    public Text waveBannerText;
    public float waveBannerDuration = 2f;

    [Header("Game Over Panel")]
    public GameObject gameOverPanel;
    public Text gameOverScoreText;
    public Text gameOverHighScoreText;
    public Button restartButton;
    public Button mainMenuButton;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
        if (waveBannerPanel != null)
            waveBannerPanel.SetActive(false);
    }

    private void Start()
    {
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(OnRestartClicked);
        }
        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(OnMainMenuClicked);
        }
    }

    /// <summary>
    /// Update the score display.
    /// </summary>
    public void UpdateScore(int score)
    {
        if (scoreText != null)
        {
            scoreText.text = "SCORE: " + score.ToString("D6");
        }
    }

    /// <summary>
    /// Update the health display using text and optional heart icons.
    /// </summary>
    public void UpdateHealth(int currentHealth, int maxHealth)
    {
        if (healthText != null)
        {
            healthText.text = "HP: " + currentHealth + " / " + maxHealth;
        }

        if (healthIcons != null)
        {
            for (int i = 0; i < healthIcons.Length; i++)
            {
                if (healthIcons[i] != null)
                {
                    healthIcons[i].enabled = (i < currentHealth);
                }
            }
        }
    }

    /// <summary>
    /// Show the wave number banner briefly.
    /// </summary>
    public void ShowWaveBanner(int waveNumber)
    {
        if (waveBannerPanel != null && waveBannerText != null)
        {
            waveBannerText.text = "WAVE " + waveNumber;
            waveBannerPanel.SetActive(true);
            StartCoroutine(HideWaveBannerAfterDelay());
        }

        if (waveText != null)
        {
            waveText.text = "WAVE: " + waveNumber;
        }
    }

    private IEnumerator HideWaveBannerAfterDelay()
    {
        yield return new WaitForSeconds(waveBannerDuration);
        if (waveBannerPanel != null)
        {
            waveBannerPanel.SetActive(false);
        }
    }

    /// <summary>
    /// Show the game over screen with final score and high score.
    /// </summary>
    public void ShowGameOver(int score, int highScore)
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
        if (gameOverScoreText != null)
        {
            gameOverScoreText.text = "SCORE: " + score.ToString("D6");
        }
        if (gameOverHighScoreText != null)
        {
            gameOverHighScoreText.text = "HIGH SCORE: " + highScore.ToString("D6");
        }
    }

    /// <summary>
    /// Hide the game over screen (used on restart).
    /// </summary>
    public void HideGameOver()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
    }

    private void OnRestartClicked()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RestartGame();
        }
    }

    private void OnMainMenuClicked()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ReturnToMainMenu();
        }
    }
}

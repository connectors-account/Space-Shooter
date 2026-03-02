using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("HUD Elements")]
    public Slider healthBar;
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI highScoreText;
    public TextMeshProUGUI waveText;

    [Header("Panels")]
    public GameObject pausePanel;
    public GameObject gameOverPanel;
    public GameObject waveCompletePanel;

    [Header("Game Over UI")]
    public TextMeshProUGUI finalScoreText;
    public TextMeshProUGUI finalHighScoreText;

    [Header("Wave Complete UI")]
    public TextMeshProUGUI waveCompleteText;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        // Hide all panels initially
        if (pausePanel != null) pausePanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (waveCompletePanel != null) waveCompletePanel.SetActive(false);

        // Subscribe to events
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnScoreChanged.AddListener(UpdateScore);
            ScoreManager.Instance.OnHighScoreChanged.AddListener(UpdateHighScore);
        }
    }

    public void UpdateHealthBar(int currentHealth, int maxHealth)
    {
        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = currentHealth;
        }

        if (healthText != null)
        {
            healthText.text = $"{currentHealth}/{maxHealth}";
        }
    }

    public void UpdateScore(int score)
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {score:N0}";
        }
    }

    public void UpdateHighScore(int highScore)
    {
        if (highScoreText != null)
        {
            highScoreText.text = $"High Score: {highScore:N0}";
        }
    }

    public void UpdateWaveCounter(int wave)
    {
        if (waveText != null)
        {
            waveText.text = $"Wave: {wave}";
        }
    }

    public void ShowPauseMenu(bool show)
    {
        if (pausePanel != null)
        {
            pausePanel.SetActive(show);
        }
    }

    public void ShowGameOverScreen()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);

            if (finalScoreText != null && ScoreManager.Instance != null)
            {
                finalScoreText.text = $"Final Score: {ScoreManager.Instance.GetScore():N0}";
            }

            if (finalHighScoreText != null && ScoreManager.Instance != null)
            {
                finalHighScoreText.text = $"High Score: {ScoreManager.Instance.GetHighScore():N0}";
            }
        }
    }

    public void ShowWaveComplete(int wave)
    {
        if (waveCompletePanel != null)
        {
            waveCompletePanel.SetActive(true);

            if (waveCompleteText != null)
            {
                waveCompleteText.text = $"Wave {wave} Complete!";
            }

            // Hide after delay
            StartCoroutine(HideWaveComplete());
        }
    }

    System.Collections.IEnumerator HideWaveComplete()
    {
        yield return new WaitForSeconds(2f);
        if (waveCompletePanel != null)
        {
            waveCompletePanel.SetActive(false);
        }
    }

    // Button callbacks
    public void OnResumeButtonClicked()
    {
        GameManager.Instance?.ResumeGame();
    }

    public void OnRestartButtonClicked()
    {
        GameManager.Instance?.RestartGame();
    }

    public void OnMainMenuButtonClicked()
    {
        GameManager.Instance?.LoadMainMenu();
    }

    public void OnQuitButtonClicked()
    {
        GameManager.Instance?.QuitGame();
    }
}

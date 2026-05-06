using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("HUD")]
    [SerializeField] private Text scoreText;
    [SerializeField] private Text healthText;
    [SerializeField] private Text waveText;
    [SerializeField] private Text rapidFireIndicatorText;
    [SerializeField] private Text shieldIndicatorText;
    [SerializeField] private GameObject hudRoot;

    [Header("Menus")]
    [SerializeField] private GameObject pauseMenuRoot;
    [SerializeField] private GameObject gameOverRoot;
    [SerializeField] private Text gameOverSummaryText;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void ShowGameplayHUD(bool isVisible)
    {
        if (hudRoot != null)
        {
            hudRoot.SetActive(isVisible);
        }
    }

    public void UpdateScore(int score)
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {score}";
        }
    }

    public void UpdateHealth(int currentHealth, int maxHealth)
    {
        if (healthText != null)
        {
            healthText.text = $"Lives: {currentHealth}/{maxHealth}";
        }
    }

    public void UpdateWave(int waveNumber)
    {
        if (waveText != null)
        {
            waveText.text = $"Wave: {waveNumber}";
        }
    }

    public void SetRapidFireIndicator(bool active)
    {
        if (rapidFireIndicatorText != null)
        {
            rapidFireIndicatorText.enabled = active;
            rapidFireIndicatorText.text = "RAPID FIRE";
        }
    }

    public void SetShieldIndicator(bool active)
    {
        if (shieldIndicatorText != null)
        {
            shieldIndicatorText.enabled = active;
            shieldIndicatorText.text = "SHIELD";
        }
    }

    public void ShowPauseMenu(bool isVisible)
    {
        if (pauseMenuRoot != null)
        {
            pauseMenuRoot.SetActive(isVisible);
        }
    }

    public void ShowGameOver(int score, int wave)
    {
        if (gameOverRoot != null)
        {
            gameOverRoot.SetActive(true);
        }

        if (gameOverSummaryText != null)
        {
            gameOverSummaryText.text = $"Game Over\nScore: {score}\nWave Reached: {wave}";
        }
    }

    public void HideGameOver()
    {
        if (gameOverRoot != null)
        {
            gameOverRoot.SetActive(false);
        }
    }
}

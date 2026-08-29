using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles score/health display and game-over UI.
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("HUD")]
    [SerializeField] private Text scoreText;
    [SerializeField] private Text healthText;

    [Header("Game Over")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private Text gameOverScoreText;
    [SerializeField] private Text highScoreText;

    [Header("Pause")]
    [SerializeField] private GameObject pauseMenuPanel;

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
        HideGameOver();
        HidePauseMenu();
    }

    public void UpdateScore(int score)
    {
        if (scoreText != null)
            scoreText.text = $"Score: {score}";
    }

    public void UpdateHealth(int current, int max)
    {
        if (healthText != null)
            healthText.text = $"Health: {current}/{max}";
    }

    public void ShowGameOver(int finalScore, int bestScore)
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        if (gameOverScoreText != null)
            gameOverScoreText.text = $"Final Score: {finalScore}";

        if (highScoreText != null)
            highScoreText.text = $"High Score: {bestScore}";
    }

    public void HideGameOver()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
    }

    public void ShowPauseMenu()
    {
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(true);
    }

    public void HidePauseMenu()
    {
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);
    }
}

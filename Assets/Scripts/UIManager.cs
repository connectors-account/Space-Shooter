using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages all in-game UI elements: score, health, game over screen.
/// Singleton pattern. Attach this to a "UIManager" GameObject in the scene.
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("HUD Elements")]
    public Text scoreText;
    public Text healthText;

    [Header("Game Over Panel")]
    public GameObject gameOverPanel;
    public Text finalScoreText;
    public Button restartButton;
    public Button mainMenuButton;

    void Awake()
    {
        // Singleton setup
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        // Hide game over panel at start
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        // Set up button listeners
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(OnRestartClicked);
        }
        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(OnMainMenuClicked);
        }

        // Initialize displays
        UpdateScore(0);
    }

    /// <summary>
    /// Updates the score display.
    /// </summary>
    public void UpdateScore(int score)
    {
        if (scoreText != null)
        {
            scoreText.text = "SCORE: " + score.ToString();
        }
    }

    /// <summary>
    /// Updates the health display.
    /// </summary>
    public void UpdateHealth(int current, int max)
    {
        if (healthText != null)
        {
            healthText.text = "HP: " + current + " / " + max;
        }
    }

    /// <summary>
    /// Shows the game over screen with the final score.
    /// </summary>
    public void ShowGameOverScreen(int finalScore)
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
        if (finalScoreText != null)
        {
            finalScoreText.text = "FINAL SCORE: " + finalScore.ToString();
        }
    }

    void OnRestartClicked()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RestartGame();
        }
        else
        {
            SceneManager.LoadScene("GameScene");
        }
    }

    void OnMainMenuClicked()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.GoToMainMenu();
        }
        else
        {
            SceneManager.LoadScene("MainMenu");
        }
    }
}

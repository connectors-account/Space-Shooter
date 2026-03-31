using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UIManager updates the HUD (score & health) and shows the Game Over panel.
/// Attach this script to a "UIManager" GameObject that is a child of the Canvas.
/// </summary>
public class UIManager : MonoBehaviour
{
    [Header("HUD Elements")]
    [Tooltip("Text component that displays the player's score")]
    public Text scoreText;

    [Tooltip("Text component that displays the player's health")]
    public Text healthText;

    [Header("Game Over Elements")]
    [Tooltip("Panel shown when the game is over (disabled by default)")]
    public GameObject gameOverPanel;

    [Tooltip("Text on the game-over screen showing final score")]
    public Text finalScoreText;

    [Tooltip("Restart button on the game-over panel")]
    public Button restartButton;

    void Start()
    {
        // Hide Game Over panel at start
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        // Subscribe to GameManager events
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnScoreChanged += UpdateScore;
            GameManager.Instance.OnHealthChanged += UpdateHealth;
            GameManager.Instance.OnGameOver += ShowGameOver;

            // Set initial UI values
            UpdateScore(GameManager.Instance.score);
            UpdateHealth(GameManager.Instance.currentHealth);
        }

        // Wire up the restart button
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(OnRestartClicked);
        }
    }

    void OnDestroy()
    {
        // Unsubscribe to avoid null-reference errors on scene reload
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnScoreChanged -= UpdateScore;
            GameManager.Instance.OnHealthChanged -= UpdateHealth;
            GameManager.Instance.OnGameOver -= ShowGameOver;
        }
    }

    /// <summary>
    /// Updates the score display.
    /// </summary>
    private void UpdateScore(int newScore)
    {
        if (scoreText != null)
            scoreText.text = "Score: " + newScore;
    }

    /// <summary>
    /// Updates the health display.
    /// </summary>
    private void UpdateHealth(int newHealth)
    {
        if (healthText != null)
            healthText.text = "Health: " + newHealth;
    }

    /// <summary>
    /// Shows the Game Over panel with the final score.
    /// </summary>
    private void ShowGameOver()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);

            if (finalScoreText != null)
                finalScoreText.text = "Final Score: " + GameManager.Instance.score;
        }
    }

    /// <summary>
    /// Called by the Restart button.
    /// </summary>
    private void OnRestartClicked()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.RestartGame();
    }
}

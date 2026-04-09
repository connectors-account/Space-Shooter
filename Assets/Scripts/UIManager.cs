using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages all in-game UI: score display, health bar, and game-over screen.
/// Uses Unity's built-in UI system (Canvas + Text + Image).
/// Attach to the Canvas GameObject.
/// </summary>
public class UIManager : MonoBehaviour
{
    // --- Singleton ---
    public static UIManager Instance { get; private set; }

    [Header("HUD Elements")]
    [Tooltip("Text component showing the current score.")]
    public Text scoreText;

    [Tooltip("Text component showing health as text (e.g. 'HP: 80/100').")]
    public Text healthText;

    [Tooltip("Image used as a health bar fill (set Image Type to Filled).")]
    public Image healthBarFill;

    [Header("Game Over Panel")]
    [Tooltip("The parent panel shown when the game ends.")]
    public GameObject gameOverPanel;

    [Tooltip("Text showing the final score on the game-over screen.")]
    public Text finalScoreText;

    [Tooltip("Text showing restart instructions.")]
    public Text restartText;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Hide game-over panel at start
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        // Initialize displays
        UpdateScore(0);
        UpdateHealth(100, 100);
    }

    /// <summary>
    /// Update the score display.
    /// </summary>
    public void UpdateScore(int score)
    {
        if (scoreText != null)
            scoreText.text = "SCORE: " + score.ToString();
    }

    /// <summary>
    /// Update the health display (both text and bar).
    /// </summary>
    public void UpdateHealth(int current, int max)
    {
        if (healthText != null)
            healthText.text = "HP: " + current + " / " + max;

        if (healthBarFill != null)
            healthBarFill.fillAmount = (float)current / max;
    }

    /// <summary>
    /// Show the game-over screen with the final score.
    /// </summary>
    public void ShowGameOver(int finalScore)
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        if (finalScoreText != null)
            finalScoreText.text = "FINAL SCORE: " + finalScore.ToString();

        if (restartText != null)
            restartText.text = "Press R to Restart";
    }
}

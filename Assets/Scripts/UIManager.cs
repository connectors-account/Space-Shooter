using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages all in-game UI: score display, health bar, and Game Over panel.
/// Uses a singleton so other scripts can call UIManager.Instance.UpdateScore() etc.
/// Attach this to a Canvas GameObject in the scene.
/// </summary>
public class UIManager : MonoBehaviour
{
    // ---- Singleton ----
    public static UIManager Instance { get; private set; }

    [Header("HUD Elements")]
    [Tooltip("Text component that shows the current score.")]
    public Text scoreText;

    [Tooltip("Text component that shows current health.")]
    public Text healthText;

    [Tooltip("Optional: Image used as a health bar fill (Image Type = Filled).")]
    public Image healthBarFill;

    [Header("Game Over Panel")]
    [Tooltip("Panel that is shown when the game ends.")]
    public GameObject gameOverPanel;

    [Tooltip("Text inside the Game Over panel showing the final score.")]
    public Text finalScoreText;

    [Tooltip("Text showing restart instructions.")]
    public Text restartText;

    // =========================================================================
    // Unity Lifecycle
    // =========================================================================

    private void Awake()
    {
        // Singleton enforcement
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Make sure the Game Over panel starts hidden
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
    }

    private void Start()
    {
        UpdateScore(0);
    }

    // =========================================================================
    // Public Methods
    // =========================================================================

    /// <summary>
    /// Updates the score display.
    /// </summary>
    public void UpdateScore(int score)
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + score.ToString();
        }
    }

    /// <summary>
    /// Updates the health display (text and optional fill bar).
    /// </summary>
    public void UpdateHealth(int currentHealth, int maxHealth)
    {
        if (healthText != null)
        {
            healthText.text = "Health: " + currentHealth + " / " + maxHealth;
        }

        if (healthBarFill != null)
        {
            healthBarFill.fillAmount = (float)currentHealth / maxHealth;
        }
    }

    /// <summary>
    /// Shows the Game Over panel with the final score.
    /// </summary>
    public void ShowGameOver(int finalScore)
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        if (finalScoreText != null)
        {
            finalScoreText.text = "Final Score: " + finalScore.ToString();
        }

        if (restartText != null)
        {
            restartText.text = "Press R to Restart\nPress ESC to Quit";
        }
    }
}

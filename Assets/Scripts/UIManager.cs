using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages all in-game UI elements:
///   - Score text
///   - Health text and health bar (a UI Slider or Image fill)
///   - Wave text
///   - The Game Over / Win panel and its result text
/// Implemented as a singleton so the GameManager can push updates easily.
///
/// Uses the legacy UnityEngine.UI.Text component for maximum compatibility
/// across Unity versions (no extra TextMeshPro package required).
/// </summary>
public class UIManager : MonoBehaviour
{
    // Singleton instance.
    public static UIManager Instance { get; private set; }

    [Header("HUD Texts")]
    [Tooltip("Text element that displays the current score.")]
    public Text scoreText;

    [Tooltip("Text element that displays the current health.")]
    public Text healthText;

    [Tooltip("Text element that displays the current wave number (optional).")]
    public Text waveText;

    [Header("Health Bar (optional)")]
    [Tooltip("A UI Slider used as a health bar. Optional.")]
    public Slider healthBar;

    [Header("Game Over / Win Panel")]
    [Tooltip("Parent panel shown when the game ends.")]
    public GameObject gameOverPanel;

    [Tooltip("Text element on the game over panel showing the result.")]
    public Text gameOverText;

    [Tooltip("Text element on the game over panel showing the final score.")]
    public Text finalScoreText;

    /// <summary>
    /// Awake sets up the singleton.
    /// </summary>
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    /// <summary>
    /// Start hides the game over panel at launch.
    /// </summary>
    private void Start()
    {
        HideGameOver();
    }

    /// <summary>
    /// Updates the score display.
    /// </summary>
    /// <param name="score">Current score value.</param>
    public void UpdateScore(int score)
    {
        if (scoreText != null)
            scoreText.text = "Score: " + score;
    }

    /// <summary>
    /// Updates the health text and (optionally) the health bar slider.
    /// </summary>
    /// <param name="current">Current health.</param>
    /// <param name="max">Maximum health.</param>
    public void UpdateHealth(int current, int max)
    {
        if (healthText != null)
            healthText.text = "Health: " + current + " / " + max;

        if (healthBar != null)
        {
            healthBar.maxValue = max;
            healthBar.value = current;
        }
    }

    /// <summary>
    /// Updates the wave display.
    /// </summary>
    /// <param name="wave">Current wave number.</param>
    public void UpdateWave(int wave)
    {
        if (waveText != null)
            waveText.text = "Wave: " + wave;
    }

    /// <summary>
    /// Shows the end-of-game panel with either a win or lose message.
    /// </summary>
    /// <param name="finalScore">Score to display.</param>
    /// <param name="won">True if the player won, false if they lost.</param>
    public void ShowGameOver(int finalScore, bool won)
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        if (gameOverText != null)
            gameOverText.text = won ? "YOU WIN!" : "GAME OVER";

        if (finalScoreText != null)
            finalScoreText.text = "Final Score: " + finalScore;
    }

    /// <summary>
    /// Hides the end-of-game panel.
    /// </summary>
    public void HideGameOver()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
    }

    // ---- Button callback helpers (hook these up in the Inspector) ----

    /// <summary>Restart button callback.</summary>
    public void OnRestartButton()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.RestartGame();
    }

    /// <summary>Main menu button callback.</summary>
    public void OnMainMenuButton()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.GoToMainMenu();
    }
}

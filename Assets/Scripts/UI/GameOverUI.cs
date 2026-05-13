// ============================================================================
// GameOverUI.cs - Game Over screen controller
// Shows final score, high score, and provides Retry / Main Menu options.
// ============================================================================
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Drives the Game Over screen. Attach to the Canvas in the GameOver scene.
/// </summary>
public class GameOverUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Text gameOverTitle;
    [SerializeField] private Text finalScoreText;
    [SerializeField] private Text highScoreText;
    [SerializeField] private Text waveReachedText;
    [SerializeField] private Text newHighScoreText;
    [SerializeField] private Button retryButton;
    [SerializeField] private Button mainMenuButton;

    // ========================================================================
    // Unity Lifecycle
    // ========================================================================

    private void Start()
    {
        // Wire buttons.
        if (retryButton != null) retryButton.onClick.AddListener(OnRetryClicked);
        if (mainMenuButton != null) mainMenuButton.onClick.AddListener(OnMainMenuClicked);

        // Populate score information from GameManager.
        int score = GameManager.Instance != null ? GameManager.Instance.Score : 0;
        int highScore = GameManager.Instance != null ? GameManager.Instance.HighScore : PlayerPrefs.GetInt("HighScore", 0);
        int wave = GameManager.Instance != null ? GameManager.Instance.CurrentWave : 1;

        if (gameOverTitle != null) gameOverTitle.text = "GAME OVER";
        if (finalScoreText != null) finalScoreText.text = $"Final Score: {score:N0}";
        if (highScoreText != null) highScoreText.text = $"High Score: {highScore:N0}";
        if (waveReachedText != null) waveReachedText.text = $"Wave Reached: {wave}";

        // Show a "NEW HIGH SCORE!" message if applicable.
        bool isNewHighScore = score >= highScore && score > 0;
        if (newHighScoreText != null)
        {
            newHighScoreText.text = "★ NEW HIGH SCORE! ★";
            newHighScoreText.enabled = isNewHighScore;
        }

        // Play game-over music/sting.
        AudioManager.Instance?.PlaySFX(AudioManager.SFX.GameOverSting);
    }

    // ========================================================================
    // Button Callbacks
    // ========================================================================

    private void OnRetryClicked()
    {
        AudioManager.Instance?.PlaySFX(AudioManager.SFX.UIClick);
        GameManager.Instance?.StartGame();
    }

    private void OnMainMenuClicked()
    {
        AudioManager.Instance?.PlaySFX(AudioManager.SFX.UIClick);
        GameManager.Instance?.ReturnToMainMenu();
    }
}

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// GameOverController manages the Game Over and Victory screens.
/// It displays the final score and provides Restart / Main Menu / Quit buttons.
/// Attach this to a GameOver Canvas (initially disabled) in the GamePlay scene.
/// </summary>
public class GameOverController : MonoBehaviour
{
    // ============================================================
    // UI REFERENCES
    // ============================================================
    [Header("UI Elements")]
    [Tooltip("Panel containing all game-over UI (enable on game over)")]
    public GameObject gameOverPanel;

    [Tooltip("Title text - shows 'GAME OVER' or 'VICTORY'")]
    public Text titleText;

    [Tooltip("Displays the final score")]
    public Text finalScoreText;

    [Tooltip("Displays the high score")]
    public Text highScoreText;

    [Tooltip("New high score celebration text (hidden unless beaten)")]
    public Text newHighScoreText;

    [Header("Buttons")]
    public Button restartButton;
    public Button mainMenuButton;
    public Button quitButton;

    // ============================================================
    // UNITY LIFECYCLE
    // ============================================================

    void Start()
    {
        // Initially hide the game over panel
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (newHighScoreText != null)
            newHighScoreText.gameObject.SetActive(false);

        // Hook up button listeners
        if (restartButton != null)
            restartButton.onClick.AddListener(OnRestartClicked);

        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(OnMainMenuClicked);

        if (quitButton != null)
            quitButton.onClick.AddListener(OnQuitClicked);

        // Subscribe to GameManager events
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameOver += ShowGameOver;
            GameManager.Instance.OnVictory += ShowVictory;
        }
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameOver -= ShowGameOver;
            GameManager.Instance.OnVictory -= ShowVictory;
        }
    }

    // ============================================================
    // SHOW SCREENS
    // ============================================================

    /// <summary>
    /// Display the Game Over screen with final score.
    /// </summary>
    public void ShowGameOver()
    {
        ShowEndScreen("GAME OVER");
    }

    /// <summary>
    /// Display the Victory screen with final score.
    /// </summary>
    public void ShowVictory()
    {
        ShowEndScreen("VICTORY!");
    }

    /// <summary>
    /// Common logic for both game over and victory screens.
    /// </summary>
    void ShowEndScreen(string title)
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        if (titleText != null)
            titleText.text = title;

        // Get scores from GameManager
        int finalScore = 0;
        int highScore = 0;

        if (GameManager.Instance != null)
        {
            finalScore = GameManager.Instance.Score;
            highScore = GameManager.Instance.HighScore;
        }

        if (finalScoreText != null)
            finalScoreText.text = "SCORE: " + finalScore.ToString();

        if (highScoreText != null)
            highScoreText.text = "HIGH SCORE: " + highScore.ToString();

        // Show celebration if new high score
        if (newHighScoreText != null)
        {
            bool isNewHigh = (finalScore >= highScore && finalScore > 0);
            newHighScoreText.gameObject.SetActive(isNewHigh);
            if (isNewHigh)
                newHighScoreText.text = "\u2605 NEW HIGH SCORE! \u2605";
        }

        // Pause the game so nothing moves in the background
        Time.timeScale = 0f;
    }

    // ============================================================
    // BUTTON HANDLERS
    // ============================================================

    /// <summary>Restart the gameplay scene.</summary>
    public void OnRestartClicked()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("GamePlay");
    }

    /// <summary>Return to the main menu.</summary>
    public void OnMainMenuClicked()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    /// <summary>Quit the application.</summary>
    public void OnQuitClicked()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}

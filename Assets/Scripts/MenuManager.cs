// =============================================================================
// MenuManager.cs
// Handles the Main Menu and Game Over screen logic.
// Attach this to a Canvas or empty GameObject in the MainMenu / GameOver scenes.
// Wire the UI buttons to the public methods via the Unity Inspector.
// =============================================================================
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Main Menu UI References
    // -------------------------------------------------------------------------
    [Header("Main Menu UI")]
    [Tooltip("The title text of the game.")]
    public Text titleText;

    [Tooltip("High score display on the main menu.")]
    public Text highScoreText;

    // -------------------------------------------------------------------------
    // Game Over UI References
    // -------------------------------------------------------------------------
    [Header("Game Over UI")]
    [Tooltip("Text showing 'GAME OVER'.")]
    public Text gameOverText;

    [Tooltip("Text showing the final score.")]
    public Text finalScoreText;

    [Tooltip("Text showing the high score on the game over screen.")]
    public Text gameOverHighScoreText;

    [Tooltip("Text showing a 'NEW HIGH SCORE!' message.")]
    public Text newHighScoreText;

    // -------------------------------------------------------------------------
    // Unity Lifecycle
    // -------------------------------------------------------------------------

    /// <summary>
    /// Initialize menu display based on current scene.
    /// </summary>
    void Start()
    {
        Time.timeScale = 1f; // Ensure time is running normally

        string sceneName = SceneManager.GetActiveScene().name;

        if (sceneName == "MainMenu")
        {
            SetupMainMenu();
        }
        else if (sceneName == "GameOver")
        {
            SetupGameOverScreen();
        }
    }

    // -------------------------------------------------------------------------
    // Main Menu Setup
    // -------------------------------------------------------------------------

    /// <summary>
    /// Configures the main menu display with title and high score.
    /// </summary>
    private void SetupMainMenu()
    {
        if (titleText != null)
        {
            titleText.text = "SPACE SHOOTER";
        }

        // Display high score
        int highScore = PlayerPrefs.GetInt("HighScore", 0);
        if (highScoreText != null)
        {
            highScoreText.text = "HIGH SCORE: " + highScore.ToString("N0");
        }
    }

    // -------------------------------------------------------------------------
    // Game Over Setup
    // -------------------------------------------------------------------------

    /// <summary>
    /// Configures the game over screen with final score and high score.
    /// </summary>
    private void SetupGameOverScreen()
    {
        int finalScore = 0;
        int highScore = PlayerPrefs.GetInt("HighScore", 0);

        // Get score from GameManager if it exists
        if (GameManager.Instance != null)
        {
            finalScore = GameManager.Instance.GetScore();
        }

        if (gameOverText != null)
        {
            gameOverText.text = "GAME OVER";
        }

        if (finalScoreText != null)
        {
            finalScoreText.text = "SCORE: " + finalScore.ToString("N0");
        }

        if (gameOverHighScoreText != null)
        {
            gameOverHighScoreText.text = "HIGH SCORE: " + highScore.ToString("N0");
        }

        // Show "NEW HIGH SCORE!" if the player beat the record
        if (newHighScoreText != null)
        {
            newHighScoreText.gameObject.SetActive(finalScore >= highScore && finalScore > 0);
        }
    }

    // -------------------------------------------------------------------------
    // Button Callbacks
    // -------------------------------------------------------------------------

    /// <summary>
    /// Starts a new game by loading the GamePlay scene.
    /// Wire this to the "Play" or "Start Game" button.
    /// </summary>
    public void OnPlayButton()
    {
        SceneManager.LoadScene("GamePlay");
    }

    /// <summary>
    /// Restarts the game from the Game Over screen.
    /// Wire this to the "Play Again" button.
    /// </summary>
    public void OnRestartButton()
    {
        SceneManager.LoadScene("GamePlay");
    }

    /// <summary>
    /// Returns to the main menu from the Game Over screen.
    /// Wire this to the "Main Menu" button.
    /// </summary>
    public void OnMainMenuButton()
    {
        // If GameManager exists, use its method; otherwise load directly
        if (GameManager.Instance != null)
        {
            GameManager.Instance.GoToMainMenu();
        }
        else
        {
            SceneManager.LoadScene("MainMenu");
        }
    }

    /// <summary>
    /// Quits the application.
    /// Wire this to the "Quit" button.
    /// </summary>
    public void OnQuitButton()
    {
        PlayerPrefs.Save();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    /// <summary>
    /// Resets the high score to zero (optional utility button).
    /// </summary>
    public void OnResetHighScoreButton()
    {
        PlayerPrefs.SetInt("HighScore", 0);
        PlayerPrefs.Save();

        if (highScoreText != null)
        {
            highScoreText.text = "HIGH SCORE: 0";
        }
    }
}

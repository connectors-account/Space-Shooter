using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// MainMenuController manages the main menu screen.
/// It provides buttons to start the game and quit the application.
/// Attach this to a Canvas in the MainMenu scene.
/// </summary>
public class MainMenuController : MonoBehaviour
{
    // ============================================================
    // UI REFERENCES
    // ============================================================
    [Header("UI Elements")]
    [Tooltip("The 'Start Game' button")]
    public Button startButton;

    [Tooltip("The 'Quit' button")]
    public Button quitButton;

    [Tooltip("Text displaying the high score")]
    public Text highScoreText;

    [Tooltip("Game title text")]
    public Text titleText;

    // ============================================================
    // UNITY LIFECYCLE
    // ============================================================

    void Start()
    {
        // Ensure time is running (could be paused from a previous session)
        Time.timeScale = 1f;

        // Hook up button listeners
        if (startButton != null)
            startButton.onClick.AddListener(OnStartClicked);

        if (quitButton != null)
            quitButton.onClick.AddListener(OnQuitClicked);

        // Display high score
        UpdateHighScoreDisplay();
    }

    // ============================================================
    // BUTTON HANDLERS
    // ============================================================

    /// <summary>
    /// Load the gameplay scene when the player clicks Start.
    /// </summary>
    public void OnStartClicked()
    {
        SceneManager.LoadScene("GamePlay");
    }

    /// <summary>
    /// Quit the application when the player clicks Quit.
    /// </summary>
    public void OnQuitClicked()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }

    // ============================================================
    // HIGH SCORE DISPLAY
    // ============================================================

    /// <summary>
    /// Read the persisted high score and display it.
    /// </summary>
    void UpdateHighScoreDisplay()
    {
        if (highScoreText != null)
        {
            int highScore = PlayerPrefs.GetInt("HighScore", 0);
            highScoreText.text = "HIGH SCORE: " + highScore.ToString();
        }
    }
}

// ============================================================================
// MainMenuUI.cs - Main menu screen logic
// ============================================================================
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controls the Main Menu scene UI.
/// Provides Start Game and Quit buttons.
/// Attach to the Canvas in the MainMenu scene.
/// </summary>
public class MainMenuUI : MonoBehaviour
{
    // ---- UI References ----
    [Header("Buttons")]
    public Button startButton;
    public Button quitButton;

    [Header("Display")]
    public Text titleText;
    public Text highScoreText;
    public Text instructionsText;

    // ========================================================================
    // Unity Lifecycle
    // ========================================================================
    private void Start()
    {
        // Wire up buttons
        if (startButton != null)
            startButton.onClick.AddListener(OnStartClicked);
        if (quitButton != null)
            quitButton.onClick.AddListener(OnQuitClicked);

        // Set title
        if (titleText != null)
            titleText.text = "SPACE SHOOTER";

        // Show high score
        if (highScoreText != null)
        {
            int highScore = PlayerPrefs.GetInt("HighScore", 0);
            highScoreText.text = highScore > 0 ? $"HIGH SCORE: {highScore}" : "";
        }

        // Show instructions
        if (instructionsText != null)
        {
            instructionsText.text =
                "CONTROLS:\n" +
                "WASD / Arrow Keys - Move\n" +
                "SPACE - Shoot\n" +
                "ESC - Pause";
        }

        // Ensure time is running (might be paused from previous game)
        Time.timeScale = 1f;
    }

    // ========================================================================
    // Button Handlers
    // ========================================================================

    public void OnStartClicked()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartGame();
        }
        else
        {
            // Fallback: load GameScene directly
            UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");
        }
    }

    public void OnQuitClicked()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.QuitGame();
        }
        else
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}

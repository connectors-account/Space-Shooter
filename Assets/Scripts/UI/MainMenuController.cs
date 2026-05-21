using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// MainMenuController - Handles the main menu scene UI: play, quit, and high score display.
/// Attach to a Canvas in the MainMenu scene.
/// </summary>
public class MainMenuController : MonoBehaviour
{
    [Header("UI Elements")]
    public Text titleText;
    public Text highScoreText;
    public Button playButton;
    public Button quitButton;
    public Text controlsText;

    private void Start()
    {
        // Display high score
        int highScore = PlayerPrefs.GetInt("HighScore", 0);
        if (highScoreText != null)
        {
            highScoreText.text = "HIGH SCORE: " + highScore.ToString("D6");
        }

        // Display controls info
        if (controlsText != null)
        {
            controlsText.text =
                "CONTROLS\n" +
                "-------------------\n" +
                "WASD / Arrow Keys : Move\n" +
                "Space : Shoot\n" +
                "Escape : Pause (in game)";
        }

        // Wire up buttons
        if (playButton != null)
        {
            playButton.onClick.AddListener(OnPlayClicked);
        }
        if (quitButton != null)
        {
            quitButton.onClick.AddListener(OnQuitClicked);
        }
    }

    /// <summary>
    /// Load the main game scene.
    /// </summary>
    private void OnPlayClicked()
    {
        SceneManager.LoadScene("Game");
    }

    /// <summary>
    /// Quit the application (only works in a built executable, not in the editor).
    /// </summary>
    private void OnQuitClicked()
    {
        Debug.Log("Quit game.");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}

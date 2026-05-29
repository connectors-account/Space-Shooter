using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Main menu controller. Attach to a Canvas in the MainMenu scene.
/// Handles Start Game, Quit, and displays high score.
/// </summary>
public class MainMenu : MonoBehaviour
{
    [Header("UI Elements")]
    public Text titleText;
    public Text highScoreText;
    public Button startButton;
    public Button quitButton;
    public Text instructionsText;

    void Start()
    {
        Time.timeScale = 1f;

        if (titleText != null)
            titleText.text = "SPACE SHOOTER";

        if (highScoreText != null)
        {
            int highScore = PlayerPrefs.GetInt("HighScore", 0);
            highScoreText.text = "High Score: " + highScore.ToString("N0");
        }

        if (instructionsText != null)
        {
            instructionsText.text =
                "Controls:\n" +
                "WASD / Arrow Keys - Move\n" +
                "Space - Shoot\n" +
                "Escape - Pause";
        }

        if (startButton != null)
            startButton.onClick.AddListener(OnStartClicked);

        if (quitButton != null)
            quitButton.onClick.AddListener(OnQuitClicked);
    }

    void OnStartClicked()
    {
        SceneManager.LoadScene("Game");
    }

    void OnQuitClicked()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}

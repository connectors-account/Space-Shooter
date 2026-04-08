using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles Main Menu and Game Over screen UI and interactions.
/// Attach to a UI manager object in MainMenu and GameOver scenes.
/// </summary>
public class MenuManager : MonoBehaviour
{
    [Header("Main Menu Elements")]
    public Text titleText;
    public Text highScoreText;
    public Button playButton;
    public Button quitButton;

    [Header("Game Over Elements")]
    public Text gameOverText;
    public Text finalScoreText;
    public Text finalHighScoreText;
    public Button restartButton;
    public Button menuButton;

    void Start()
    {
        Time.timeScale = 1f;

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

    /// <summary>
    /// Sets up the main menu UI.
    /// </summary>
    private void SetupMainMenu()
    {
        if (titleText != null)
        {
            titleText.text = "SPACE SHOOTER";
        }

        if (highScoreText != null)
        {
            int highScore = PlayerPrefs.GetInt("HighScore", 0);
            highScoreText.text = "HIGH SCORE: " + highScore.ToString();
        }

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
    /// Sets up the game over screen UI.
    /// </summary>
    private void SetupGameOverScreen()
    {
        if (gameOverText != null)
        {
            gameOverText.text = "GAME OVER";
        }

        if (finalScoreText != null && GameManager.Instance != null)
        {
            finalScoreText.text = "SCORE: " + GameManager.Instance.score.ToString();
        }

        if (finalHighScoreText != null && GameManager.Instance != null)
        {
            finalHighScoreText.text = "HIGH SCORE: " + GameManager.Instance.highScore.ToString();
        }

        if (restartButton != null)
        {
            restartButton.onClick.AddListener(OnRestartClicked);
        }

        if (menuButton != null)
        {
            menuButton.onClick.AddListener(OnMenuClicked);
        }
    }

    /// <summary>
    /// Play button: starts a new game.
    /// </summary>
    public void OnPlayClicked()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartGame();
        }
        else
        {
            SceneManager.LoadScene("GamePlay");
        }
    }

    /// <summary>
    /// Restart button: starts a new game from Game Over screen.
    /// </summary>
    public void OnRestartClicked()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartGame();
        }
        else
        {
            SceneManager.LoadScene("GamePlay");
        }
    }

    /// <summary>
    /// Return to main menu.
    /// </summary>
    public void OnMenuClicked()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ReturnToMenu();
        }
        else
        {
            SceneManager.LoadScene("MainMenu");
        }
    }

    /// <summary>
    /// Quit the game.
    /// </summary>
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

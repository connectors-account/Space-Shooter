using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages main menu, pause menu, and game over screen UI panels.
/// In-game panels (Pause, GameOver) are children of the Game scene Canvas.
/// Main menu is a separate scene with its own Canvas.
/// </summary>
public class MenuManager : MonoBehaviour
{
    [Header("Main Menu (MainMenuScene only)")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private Button playButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private Text titleText;
    [SerializeField] private Text mainMenuHighScoreText;

    [Header("Pause Menu (GameScene only)")]
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button pauseQuitButton;

    [Header("Game Over Screen (GameScene only)")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private Text gameOverScoreText;
    [SerializeField] private Text gameOverHighScoreText;
    [SerializeField] private Text gameOverWaveText;
    [SerializeField] private Text newHighScoreText;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button menuButton;

    private void Start()
    {
        SetupMainMenu();
        SetupPauseMenu();
        SetupGameOverScreen();
    }

    private void SetupMainMenu()
    {
        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(true);
        }

        if (playButton != null)
        {
            playButton.onClick.AddListener(OnPlayClicked);
        }

        if (quitButton != null)
        {
            quitButton.onClick.AddListener(OnQuitClicked);
        }

        if (mainMenuHighScoreText != null && GameManager.Instance != null)
        {
            int hs = GameManager.Instance.HighScore;
            mainMenuHighScoreText.text = hs > 0 ? "HIGH SCORE: " + hs.ToString("N0") : "";
        }
    }

    private void SetupPauseMenu()
    {
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }

        if (resumeButton != null)
        {
            resumeButton.onClick.AddListener(OnResumeClicked);
        }

        if (pauseQuitButton != null)
        {
            pauseQuitButton.onClick.AddListener(OnPauseQuitClicked);
        }
    }

    private void SetupGameOverScreen()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
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

    // --- Main Menu Actions ---

    private void OnPlayClicked()
    {
        AudioManager.Instance?.PlaySFX("UIClick");
        GameManager.Instance?.StartGame();
    }

    private void OnQuitClicked()
    {
        AudioManager.Instance?.PlaySFX("UIClick");
        GameManager.Instance?.QuitGame();
    }

    // --- Pause Menu Actions ---

    public void ShowPauseMenu()
    {
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(true);
        }
    }

    public void HidePauseMenu()
    {
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }
    }

    private void OnResumeClicked()
    {
        AudioManager.Instance?.PlaySFX("UIClick");
        GameManager.Instance?.ResumeGame();
    }

    private void OnPauseQuitClicked()
    {
        AudioManager.Instance?.PlaySFX("UIClick");
        GameManager.Instance?.ReturnToMainMenu();
    }

    // --- Game Over Actions ---

    public void ShowGameOverScreen(int finalScore, int highScore, int wavesReached)
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        if (gameOverScoreText != null)
        {
            gameOverScoreText.text = "SCORE: " + finalScore.ToString("N0");
        }

        if (gameOverHighScoreText != null)
        {
            gameOverHighScoreText.text = "HIGH SCORE: " + highScore.ToString("N0");
        }

        if (gameOverWaveText != null)
        {
            gameOverWaveText.text = "WAVES SURVIVED: " + wavesReached;
        }

        if (newHighScoreText != null)
        {
            newHighScoreText.gameObject.SetActive(finalScore >= highScore && finalScore > 0);
        }
    }

    private void OnRestartClicked()
    {
        AudioManager.Instance?.PlaySFX("UIClick");
        GameManager.Instance?.RestartGame();
    }

    private void OnMenuClicked()
    {
        AudioManager.Instance?.PlaySFX("UIClick");
        GameManager.Instance?.ReturnToMainMenu();
    }
}

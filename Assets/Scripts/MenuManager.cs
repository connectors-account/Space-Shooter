using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// MenuManager controls the Main Menu, Pause Menu, and Game Over overlay.
/// Uses Unity UI Canvas panels toggled on/off.
/// </summary>
public class MenuManager : MonoBehaviour
{
    // ── Singleton ────────────────────────────────────────────
    public static MenuManager Instance { get; private set; }

    // ── UI Panels ────────────────────────────────────────────
    [Header("Menu Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private GameObject gameOverPanel;

    // ── Main Menu Elements ───────────────────────────────────
    [Header("Main Menu")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private Text titleText;
    [SerializeField] private Text highScoreText;

    // ── Pause Menu Elements ──────────────────────────────────
    [Header("Pause Menu")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button pauseQuitButton;

    // ── Game Over Elements ───────────────────────────────────
    [Header("Game Over")]
    [SerializeField] private Text finalScoreText;
    [SerializeField] private Text gameOverHighScoreText;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button menuButton;

    // ──────────────────────────────────────────────────────────
    // Unity Lifecycle
    // ──────────────────────────────────────────────────────────

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        // Wire up button listeners
        if (startButton != null) startButton.onClick.AddListener(OnStartClicked);
        if (quitButton != null) quitButton.onClick.AddListener(OnQuitClicked);
        if (resumeButton != null) resumeButton.onClick.AddListener(OnResumeClicked);
        if (pauseQuitButton != null) pauseQuitButton.onClick.AddListener(OnPauseQuitClicked);
        if (restartButton != null) restartButton.onClick.AddListener(OnRestartClicked);
        if (menuButton != null) menuButton.onClick.AddListener(OnMenuClicked);

        // Initial visibility
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);

        // If we are on the MainMenu scene show the main menu
        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(true);
            UpdateMainMenuHighScore();
        }
    }

    // ──────────────────────────────────────────────────────────
    // Main Menu
    // ──────────────────────────────────────────────────────────

    private void UpdateMainMenuHighScore()
    {
        int hs = PlayerPrefs.GetInt("HighScore", 0);
        if (highScoreText != null)
            highScoreText.text = "HIGH SCORE: " + hs.ToString("N0");
    }

    private void OnStartClicked()
    {
        PlayButtonClick();
        if (GameManager.Instance != null)
            GameManager.Instance.StartGame();
    }

    private void OnQuitClicked()
    {
        PlayButtonClick();
        if (GameManager.Instance != null)
            GameManager.Instance.QuitGame();
    }

    // ──────────────────────────────────────────────────────────
    // Pause Menu
    // ──────────────────────────────────────────────────────────

    public void ShowPauseMenu()
    {
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(true);
    }

    public void HidePauseMenu()
    {
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
    }

    private void OnResumeClicked()
    {
        PlayButtonClick();
        if (GameManager.Instance != null)
            GameManager.Instance.ResumeGame();
    }

    private void OnPauseQuitClicked()
    {
        PlayButtonClick();
        if (GameManager.Instance != null)
            GameManager.Instance.ReturnToMainMenu();
    }

    // ──────────────────────────────────────────────────────────
    // Game Over Screen
    // ──────────────────────────────────────────────────────────

    public void ShowGameOverScreen(int finalScore, int highScore)
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);

            if (finalScoreText != null)
                finalScoreText.text = "SCORE: " + finalScore.ToString("N0");

            if (gameOverHighScoreText != null)
                gameOverHighScoreText.text = "HIGH SCORE: " + highScore.ToString("N0");
        }
    }

    private void OnRestartClicked()
    {
        PlayButtonClick();
        if (GameManager.Instance != null)
            GameManager.Instance.RestartGame();
    }

    private void OnMenuClicked()
    {
        PlayButtonClick();
        if (GameManager.Instance != null)
            GameManager.Instance.ReturnToMainMenu();
    }

    // ──────────────────────────────────────────────────────────
    // Helpers
    // ──────────────────────────────────────────────────────────

    private void PlayButtonClick()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX("ButtonClick");
    }
}

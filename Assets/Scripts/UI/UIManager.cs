using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages all in-game UI elements: health bar, score, wave counter,
/// game over screen, pause menu, and victory screen.
/// Singleton accessible via UIManager.Instance.
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("HUD Elements")]
    public Text scoreText;
    public Text waveText;
    public Text highScoreText;

    [Header("Health Display")]
    public Image[] healthIcons;
    public Color healthActiveColor = Color.green;
    public Color healthInactiveColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);

    [Header("Game Over Panel")]
    public GameObject gameOverPanel;
    public Text gameOverTitleText;
    public Text finalScoreText;
    public Text finalHighScoreText;
    public Button restartButton;
    public Button mainMenuButton;

    [Header("Pause Panel")]
    public GameObject pausePanel;
    public Button resumeButton;
    public Button pauseMainMenuButton;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        // Hide overlays at start
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);

        // Wire up buttons
        if (restartButton != null)
            restartButton.onClick.AddListener(OnRestartClicked);
        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(OnMainMenuClicked);
        if (resumeButton != null)
            resumeButton.onClick.AddListener(OnResumeClicked);
        if (pauseMainMenuButton != null)
            pauseMainMenuButton.onClick.AddListener(OnMainMenuClicked);
    }

    public void UpdateScore(int score)
    {
        if (scoreText != null)
            scoreText.text = "Score: " + score.ToString("N0");
    }

    public void UpdateWave(int wave)
    {
        if (waveText != null)
            waveText.text = "Wave: " + wave;
    }

    public void UpdateHealth(int current, int max)
    {
        if (healthIcons == null) return;

        for (int i = 0; i < healthIcons.Length; i++)
        {
            if (healthIcons[i] != null)
            {
                healthIcons[i].color = (i < current) ? healthActiveColor : healthInactiveColor;
            }
        }
    }

    public void ShowGameOver(int score, int highScore)
    {
        if (gameOverPanel == null) return;

        gameOverPanel.SetActive(true);

        if (gameOverTitleText != null)
            gameOverTitleText.text = "GAME OVER";

        if (finalScoreText != null)
            finalScoreText.text = "Score: " + score.ToString("N0");

        if (finalHighScoreText != null)
            finalHighScoreText.text = "High Score: " + highScore.ToString("N0");
    }

    public void ShowVictory(int score, int highScore)
    {
        if (gameOverPanel == null) return;

        gameOverPanel.SetActive(true);

        if (gameOverTitleText != null)
            gameOverTitleText.text = "VICTORY!";

        if (finalScoreText != null)
            finalScoreText.text = "Score: " + score.ToString("N0");

        if (finalHighScoreText != null)
            finalHighScoreText.text = "High Score: " + highScore.ToString("N0");
    }

    public void HideGameOver()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
    }

    public void ShowPauseMenu(bool show)
    {
        if (pausePanel != null)
            pausePanel.SetActive(show);
    }

    void OnRestartClicked()
    {
        GameManager.Instance?.RestartGame();
    }

    void OnMainMenuClicked()
    {
        GameManager.Instance?.GoToMainMenu();
    }

    void OnResumeClicked()
    {
        GameManager.Instance?.TogglePause();
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}

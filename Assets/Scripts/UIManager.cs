using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Handles HUD updates and visibility for all menus.
/// </summary>
public class UIManager : MonoBehaviour
{
    [Header("HUD")]
    [SerializeField] private GameObject hudPanel;
    [SerializeField] private Text scoreText;
    [SerializeField] private Text livesText;
    [SerializeField] private Text waveText;

    [Header("Menus")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private Text gameOverTitleText;
    [SerializeField] private Text gameOverScoreText;

    private void Start()
    {
        ShowMainMenu();
    }

    public void UpdateScore(int score)
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {score}";
        }
    }

    public void UpdateLives(int lives)
    {
        if (livesText != null)
        {
            livesText.text = $"Lives: {lives}";
        }
    }

    public void UpdateWave(int wave, int total)
    {
        if (waveText != null)
        {
            waveText.text = $"Wave: {wave}/{total}";
        }
    }

    public void ShowMainMenu()
    {
        SetMenuState(hud: false, main: true, pause: false, gameOver: false);
    }

    public void ShowGameplayHUD()
    {
        SetMenuState(hud: true, main: false, pause: false, gameOver: false);
    }

    public void ShowPause(bool show)
    {
        if (pausePanel != null)
        {
            pausePanel.SetActive(show);
        }
    }

    public void ShowGameOver(bool won, int finalScore)
    {
        SetMenuState(hud: false, main: false, pause: false, gameOver: true);

        if (gameOverTitleText != null)
        {
            gameOverTitleText.text = won ? "Victory!" : "Game Over";
        }

        if (gameOverScoreText != null)
        {
            gameOverScoreText.text = $"Final Score: {finalScore}";
        }
    }

    private void SetMenuState(bool hud, bool main, bool pause, bool gameOver)
    {
        if (hudPanel != null) hudPanel.SetActive(hud);
        if (mainMenuPanel != null) mainMenuPanel.SetActive(main);
        if (pausePanel != null) pausePanel.SetActive(pause);
        if (gameOverPanel != null) gameOverPanel.SetActive(gameOver);
    }

    // --- UI Button Events ---
    public void OnStartButton()
    {
        GameManager.Instance.StartNewRun();
    }

    public void OnQuitButton()
    {
        Application.Quit();
    }

    public void OnResumeButton()
    {
        GameManager.Instance.SetPause(false);
    }

    public void OnRestartButton()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}

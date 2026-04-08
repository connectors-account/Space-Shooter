using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Handles all UI updates: HUD (score, health), wave banners, pause menu.
/// Singleton, does NOT persist across scenes (each scene has its own Canvas).
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("HUD Elements")]
    public Text scoreText;
    public Text healthText;
    public Text waveBannerText;

    [Header("Pause Menu")]
    public GameObject pausePanel;

    [Header("Main Menu Elements")]
    public Text highScoreText;

    [Header("Game Over Elements")]
    public Text finalScoreText;
    public Text gameOverHighScoreText;

    void Awake()
    {
        Instance = this;
        if (pausePanel != null) pausePanel.SetActive(false);
        if (waveBannerText != null) waveBannerText.gameObject.SetActive(false);
    }

    void Start()
    {
        // Populate main menu or game over UI if applicable
        if (highScoreText != null && GameManager.Instance != null)
            highScoreText.text = "High Score: " + GameManager.Instance.GetHighScore();

        if (finalScoreText != null && GameManager.Instance != null)
            finalScoreText.text = "Score: " + GameManager.Instance.GetScore();

        if (gameOverHighScoreText != null && GameManager.Instance != null)
            gameOverHighScoreText.text = "High Score: " + GameManager.Instance.GetHighScore();
    }

    void Update()
    {
        // Listen for pause key during gameplay
        if (Input.GetKeyDown(KeyCode.Escape) && GameManager.Instance != null)
        {
            if (GameManager.Instance.CurrentState == GameManager.GameState.Playing ||
                GameManager.Instance.CurrentState == GameManager.GameState.Paused)
            {
                GameManager.Instance.TogglePause();
            }
        }
    }

    public void UpdateScore(int score)
    {
        if (scoreText != null)
            scoreText.text = "Score: " + score;
    }

    public void UpdateHealth(int current, int max)
    {
        if (healthText != null)
            healthText.text = "HP: " + current + " / " + max;
    }

    public void ShowWaveBanner(int waveNumber)
    {
        if (waveBannerText != null)
            StartCoroutine(ShowBannerCoroutine("Wave " + waveNumber));
    }

    IEnumerator ShowBannerCoroutine(string text)
    {
        waveBannerText.text = text;
        waveBannerText.gameObject.SetActive(true);
        yield return new WaitForSeconds(2f);
        if (waveBannerText != null)
            waveBannerText.gameObject.SetActive(false);
    }

    public void ShowPauseMenu(bool show)
    {
        if (pausePanel != null)
            pausePanel.SetActive(show);
    }

    // --- Button handlers (wire these in the Unity Inspector) ---

    public void OnPlayButton()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.StartGame();
    }

    public void OnResumeButton()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.TogglePause();
    }

    public void OnMainMenuButton()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.ReturnToMainMenu();
    }

    public void OnQuitButton()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.QuitGame();
    }

    public void OnRestartButton()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.StartGame();
    }
}

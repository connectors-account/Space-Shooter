using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages all in-game UI: HUD (score, health, wave), pause panel,
/// game-over panel.  Subscribes to GameManager events.
/// </summary>
public class UIManager : MonoBehaviour
{
    [Header("HUD")]
    [SerializeField] private Text scoreText;
    [SerializeField] private Text waveText;
    [SerializeField] private Text healthText;

    [Header("Panels")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private Text       finalScoreText;

    private void OnEnable()
    {
        if (GameManager.Instance == null) return;
        GameManager.Instance.OnScoreChanged  += UpdateScore;
        GameManager.Instance.OnHealthChanged += UpdateHealth;
        GameManager.Instance.OnWaveChanged   += UpdateWave;
        GameManager.Instance.OnGameOver      += ShowGameOver;
        GameManager.Instance.OnGamePaused    += ShowPause;
        GameManager.Instance.OnGameResumed   += HidePause;
    }

    private void OnDisable()
    {
        if (GameManager.Instance == null) return;
        GameManager.Instance.OnScoreChanged  -= UpdateScore;
        GameManager.Instance.OnHealthChanged -= UpdateHealth;
        GameManager.Instance.OnWaveChanged   -= UpdateWave;
        GameManager.Instance.OnGameOver      -= ShowGameOver;
        GameManager.Instance.OnGamePaused    -= ShowPause;
        GameManager.Instance.OnGameResumed   -= HidePause;
    }

    private void Start()
    {
        if (pausePanel != null) pausePanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);

        // Initialize display
        if (GameManager.Instance != null)
        {
            UpdateScore(GameManager.Instance.Score);
            UpdateHealth(GameManager.Instance.PlayerHealth);
            UpdateWave(GameManager.Instance.CurrentWave);
        }
    }

    // ── HUD updates ──────────────────────────────────────────────────
    private void UpdateScore(int score)
    {
        if (scoreText != null) scoreText.text = "SCORE: " + score.ToString("N0");
    }

    private void UpdateHealth(int hp)
    {
        if (healthText != null)
        {
            string hearts = "";
            int maxHp = GameManager.Instance != null ? GameManager.Instance.PlayerMaxHealth : 5;
            for (int i = 0; i < maxHp; i++)
                hearts += i < hp ? "♥ " : "♡ ";
            healthText.text = hearts.Trim();
        }
    }

    private void UpdateWave(int wave)
    {
        if (waveText != null) waveText.text = "WAVE " + wave;
    }

    // ── Panels ───────────────────────────────────────────────────────
    private void ShowPause()
    {
        if (pausePanel != null) pausePanel.SetActive(true);
    }

    private void HidePause()
    {
        if (pausePanel != null) pausePanel.SetActive(false);
    }

    private void ShowGameOver()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            if (finalScoreText != null && GameManager.Instance != null)
                finalScoreText.text = "FINAL SCORE\n" + GameManager.Instance.Score.ToString("N0");
        }
    }

    // ── Button callbacks (wire these in the Inspector) ───────────────
    public void OnResumeButton()
    {
        GameManager.Instance?.ResumeGame();
    }

    public void OnRestartButton()
    {
        GameManager.Instance?.RestartGame();
    }

    public void OnMainMenuButton()
    {
        GameManager.Instance?.LoadMainMenu();
    }

    public void OnQuitButton()
    {
        GameManager.Instance?.QuitGame();
    }
}

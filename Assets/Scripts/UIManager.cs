using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages all on-screen UI: the main menu, the in-game HUD (score, health,
/// wave), the game-over screen, and the temporary "Wave N" banner.
///
/// It listens to events from GameManager, ScoreManager, and the player's
/// HealthSystem so the UI always reflects the current game state without
/// polling every frame.
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Panels")]
    [Tooltip("Main menu panel (shown on Menu state).")]
    public GameObject menuPanel;

    [Tooltip("In-game HUD panel (shown on Playing state).")]
    public GameObject hudPanel;

    [Tooltip("Game-over panel (shown on GameOver state).")]
    public GameObject gameOverPanel;

    [Header("HUD Elements")]
    public Text scoreText;
    public Text waveText;
    public Slider healthBar;
    public Text waveBannerText;     // Large centered "WAVE N" text, flashed briefly.

    [Header("Game Over Elements")]
    public Text finalScoreText;
    public Text highScoreText;

    [Header("Menu Elements")]
    public Text menuHighScoreText;

    // Reference to the player's health system so we can show the health bar.
    private HealthSystem playerHealth;

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
        // Wire up GameManager state changes.
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnStateChanged += HandleStateChanged;

            // Cache the player's health system and subscribe to its changes.
            if (GameManager.Instance.player != null)
            {
                playerHealth = GameManager.Instance.player.GetComponent<HealthSystem>();
                if (playerHealth != null)
                    playerHealth.OnHealthChanged += UpdateHealthBar;
            }
        }

        // Wire up score updates.
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnScoreChanged += UpdateScore;
            ScoreManager.Instance.OnHighScoreChanged += UpdateHighScore;
            UpdateHighScore(ScoreManager.Instance.HighScore);
        }

        // Initialize to the menu look.
        HandleStateChanged(GameManager.Instance != null ? GameManager.Instance.CurrentState : GameManager.GameState.Menu);
    }

    private void OnDestroy()
    {
        // Always unsubscribe to avoid dangling event handlers.
        if (GameManager.Instance != null)
            GameManager.Instance.OnStateChanged -= HandleStateChanged;
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnScoreChanged -= UpdateScore;
            ScoreManager.Instance.OnHighScoreChanged -= UpdateHighScore;
        }
        if (playerHealth != null)
            playerHealth.OnHealthChanged -= UpdateHealthBar;
    }

    private void Update()
    {
        // Continuously refresh the wave label while playing.
        if (GameManager.Instance != null
            && GameManager.Instance.CurrentState == GameManager.GameState.Playing
            && GameManager.Instance.enemySpawner != null
            && waveText != null)
        {
            waveText.text = "Wave: " + GameManager.Instance.enemySpawner.CurrentWave;
        }
    }

    /// <summary>Show/hide panels to match the current game state.</summary>
    private void HandleStateChanged(GameManager.GameState state)
    {
        if (menuPanel != null) menuPanel.SetActive(state == GameManager.GameState.Menu);
        if (hudPanel != null) hudPanel.SetActive(state == GameManager.GameState.Playing);
        if (gameOverPanel != null) gameOverPanel.SetActive(state == GameManager.GameState.GameOver);

        if (state == GameManager.GameState.GameOver)
            ShowGameOverStats();

        if (state == GameManager.GameState.Menu && menuHighScoreText != null && ScoreManager.Instance != null)
            menuHighScoreText.text = "High Score: " + ScoreManager.Instance.HighScore;
    }

    private void UpdateScore(int score)
    {
        if (scoreText != null)
            scoreText.text = "Score: " + score;
    }

    private void UpdateHighScore(int highScore)
    {
        if (menuHighScoreText != null)
            menuHighScoreText.text = "High Score: " + highScore;
    }

    private void UpdateHealthBar(int current, int max)
    {
        if (healthBar != null)
        {
            healthBar.maxValue = max;
            healthBar.value = current;
        }
    }

    private void ShowGameOverStats()
    {
        if (ScoreManager.Instance == null)
            return;

        if (finalScoreText != null)
            finalScoreText.text = "Score: " + ScoreManager.Instance.Score;
        if (highScoreText != null)
            highScoreText.text = "High Score: " + ScoreManager.Instance.HighScore;
    }

    /// <summary>Flash a large "WAVE N" banner for a couple of seconds.</summary>
    public void ShowWaveBanner(int wave)
    {
        if (waveBannerText == null)
            return;

        StopAllCoroutines();
        StartCoroutine(WaveBannerRoutine(wave));
    }

    private IEnumerator WaveBannerRoutine(int wave)
    {
        waveBannerText.gameObject.SetActive(true);
        waveBannerText.text = "WAVE " + wave;
        yield return new WaitForSeconds(2f);
        waveBannerText.gameObject.SetActive(false);
    }

    // -------- Button hooks (assign these in the Inspector's OnClick) --------

    /// <summary>Hooked to the menu "Play" button.</summary>
    public void OnPlayButton()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.StartGame();
    }

    /// <summary>Hooked to the game-over "Restart" button.</summary>
    public void OnRestartButton()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.StartGame();
    }

    /// <summary>Hooked to the game-over "Menu" button.</summary>
    public void OnMenuButton()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.ReturnToMenu();
    }

    /// <summary>Hooked to any "Quit" button.</summary>
    public void OnQuitButton()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.QuitGame();
    }
}

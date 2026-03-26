using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages all in-game HUD elements: score, health hearts, wave number,
/// power-up timer, Game Over screen, and wave announcements.
/// Attach to a Canvas GameObject in GameScene.
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("HUD Elements")]
    public Text scoreText;
    public Text waveText;
    public Text healthText;         // e.g. "♥ ♥ ♥" or use Image hearts
    public Text powerUpTimerText;

    [Header("Wave Announcement")]
    public Text waveAnnouncementText;
    public float announcementDuration = 2f;

    [Header("Game Over Panel")]
    public GameObject gameOverPanel;
    public Text finalScoreText;
    public Button restartButton;
    public Button menuButton;

    [Header("Pause Panel")]
    public GameObject pausePanel;

    private float powerUpTimer;
    private bool  showingPowerUpTimer;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        // Subscribe to GameManager events
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnScoreChanged  += UpdateScore;
            GameManager.Instance.OnHealthChanged += UpdateHealth;
            GameManager.Instance.OnWaveChanged   += UpdateWave;
            GameManager.Instance.OnStateChanged  += OnStateChanged;
        }

        // Button listeners
        if (restartButton != null) restartButton.onClick.AddListener(OnRestart);
        if (menuButton    != null) menuButton.onClick.AddListener(OnMenu);

        // Initial state
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (pausePanel    != null) pausePanel.SetActive(false);
        if (powerUpTimerText != null) powerUpTimerText.gameObject.SetActive(false);
        if (waveAnnouncementText != null) waveAnnouncementText.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnScoreChanged  -= UpdateScore;
            GameManager.Instance.OnHealthChanged -= UpdateHealth;
            GameManager.Instance.OnWaveChanged   -= UpdateWave;
            GameManager.Instance.OnStateChanged  -= OnStateChanged;
        }
    }

    private void Update()
    {
        // Power-up countdown
        if (showingPowerUpTimer)
        {
            powerUpTimer -= Time.deltaTime;
            if (powerUpTimer <= 0f)
            {
                showingPowerUpTimer = false;
                if (powerUpTimerText != null) powerUpTimerText.gameObject.SetActive(false);
            }
            else if (powerUpTimerText != null)
            {
                powerUpTimerText.text = $"Power-Up: {powerUpTimer:F1}s";
            }
        }
    }

    // ── Event handlers ────────────────────────────────────────────────
    private void UpdateScore(int score)
    {
        if (scoreText != null) scoreText.text = $"Score: {score}";
    }

    private void UpdateHealth(int hp)
    {
        if (healthText != null)
        {
            string hearts = "";
            int maxHp = GameManager.Instance != null ? GameManager.Instance.MaxHealth : 3;
            for (int i = 0; i < maxHp; i++)
                hearts += i < hp ? "♥ " : "♡ ";
            healthText.text = hearts.Trim();
        }
    }

    private void UpdateWave(int wave)
    {
        if (waveText != null) waveText.text = $"Wave {wave}";
        StartCoroutine(ShowWaveAnnouncement(wave));
    }

    private System.Collections.IEnumerator ShowWaveAnnouncement(int wave)
    {
        if (waveAnnouncementText == null) yield break;

        bool isBoss = wave % 5 == 0;
        waveAnnouncementText.text = isBoss ? $"!! BOSS WAVE {wave} !!" : $"Wave {wave}";
        waveAnnouncementText.color = isBoss ? Color.red : Color.white;
        waveAnnouncementText.gameObject.SetActive(true);

        yield return new WaitForSeconds(announcementDuration);

        waveAnnouncementText.gameObject.SetActive(false);
    }

    private void OnStateChanged(GameManager.State state)
    {
        switch (state)
        {
            case GameManager.State.GameOver:
                if (gameOverPanel != null)
                {
                    gameOverPanel.SetActive(true);
                    if (finalScoreText != null && GameManager.Instance != null)
                        finalScoreText.text = $"Final Score: {GameManager.Instance.Score}";
                }
                break;

            case GameManager.State.Paused:
                if (pausePanel != null) pausePanel.SetActive(true);
                break;

            case GameManager.State.Playing:
                if (pausePanel    != null) pausePanel.SetActive(false);
                if (gameOverPanel != null) gameOverPanel.SetActive(false);
                break;
        }
    }

    // ── Public API ────────────────────────────────────────────────────
    public void ShowPowerUpTimer(string name, float duration)
    {
        powerUpTimer        = duration;
        showingPowerUpTimer = true;
        if (powerUpTimerText != null)
        {
            powerUpTimerText.text = $"{name}: {duration:F1}s";
            powerUpTimerText.gameObject.SetActive(true);
        }
    }

    // ── Button callbacks ─────────────────────────────────────────────
    private void OnRestart()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayMenuClick();
        if (GameManager.Instance  != null) GameManager.Instance.RestartGame();
    }

    private void OnMenu()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayMenuClick();
        if (GameManager.Instance  != null) GameManager.Instance.ReturnToMenu();
    }
}

using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UIManager controls the in-game HUD: score display, health bar,
/// wave number, and wave announcement text.
/// </summary>
public class UIManager : MonoBehaviour
{
    // ── Singleton ────────────────────────────────────────────
    public static UIManager Instance { get; private set; }

    // ── HUD Elements ─────────────────────────────────────────
    [Header("HUD Text")]
    [SerializeField] private Text scoreText;
    [SerializeField] private Text waveText;
    [SerializeField] private Text healthText;
    [SerializeField] private Text waveAnnouncementText;

    // ── Health Bar (optional Slider) ─────────────────────────
    [Header("Health Bar")]
    [SerializeField] private Slider healthSlider;

    // ── Wave Announcement ────────────────────────────────────
    [Header("Wave Announcement")]
    [SerializeField] private float announcementDuration = 2f;
    private float announcementTimer = 0f;

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
        // Hide announcement text initially
        if (waveAnnouncementText != null)
            waveAnnouncementText.gameObject.SetActive(false);
    }

    private void Update()
    {
        // Countdown for wave announcement visibility
        if (announcementTimer > 0f)
        {
            announcementTimer -= Time.deltaTime;
            if (announcementTimer <= 0f && waveAnnouncementText != null)
            {
                waveAnnouncementText.gameObject.SetActive(false);
            }
        }
    }

    // ──────────────────────────────────────────────────────────
    // Public API
    // ──────────────────────────────────────────────────────────

    /// <summary>
    /// Update the score text on the HUD.
    /// </summary>
    public void UpdateScoreDisplay(int score)
    {
        if (scoreText != null)
            scoreText.text = "SCORE: " + score.ToString("N0");
    }

    /// <summary>
    /// Update the wave number text on the HUD.
    /// </summary>
    public void UpdateWaveDisplay(int wave)
    {
        if (waveText != null)
            waveText.text = "WAVE " + wave;
    }

    /// <summary>
    /// Update the health display (both text and slider).
    /// </summary>
    public void UpdateHealthDisplay(int current, int max)
    {
        if (healthText != null)
            healthText.text = "HP: " + current + " / " + max;

        if (healthSlider != null)
        {
            healthSlider.maxValue = max;
            healthSlider.value = current;
        }
    }

    /// <summary>
    /// Show a large "WAVE X" announcement briefly in the centre of the screen.
    /// </summary>
    public void ShowWaveAnnouncement(int waveNumber)
    {
        if (waveAnnouncementText != null)
        {
            bool isBossWave = (waveNumber % 5 == 0);
            waveAnnouncementText.text = isBossWave
                ? "!! BOSS WAVE !!"
                : "WAVE " + waveNumber;

            waveAnnouncementText.color = isBossWave ? Color.red : Color.white;
            waveAnnouncementText.gameObject.SetActive(true);
            announcementTimer = announcementDuration;
        }

        // Play wave start SFX
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX("WaveStart");
    }
}

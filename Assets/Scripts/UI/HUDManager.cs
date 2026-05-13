// ============================================================================
// HUDManager.cs - In-game heads-up display controller
// Displays health bar, shield bar, score, combo, and wave information.
// ============================================================================
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Updates all HUD elements each frame based on game state.
/// Attach to the HUD Canvas root in the Game scene.
/// </summary>
public class HUDManager : MonoBehaviour
{
    [Header("Health & Shield Bars")]
    [Tooltip("Slider or Image (filled) for the health bar.")]
    [SerializeField] private Slider healthBar;
    [Tooltip("Slider or Image (filled) for the shield bar.")]
    [SerializeField] private Slider shieldBar;
    [Tooltip("Text display for health numbers.")]
    [SerializeField] private Text healthText;

    [Header("Score Display")]
    [SerializeField] private Text scoreText;
    [SerializeField] private Text highScoreText;
    [SerializeField] private Text comboText;

    [Header("Wave Display")]
    [SerializeField] private Text waveText;
    [SerializeField] private Text waveAnnouncementText;
    [Tooltip("Seconds the wave announcement stays visible.")]
    [SerializeField] private float waveAnnounceDuration = 2f;

    [Header("Weapon Level")]
    [SerializeField] private Text weaponLevelText;

    private float waveAnnounceTimer;
    private PlayerHealth playerHealth;
    private PlayerShooting playerShooting;

    // ========================================================================
    // Unity Lifecycle
    // ========================================================================

    private void Start()
    {
        // Find player references.
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerHealth = player.GetComponent<PlayerHealth>();
            playerShooting = player.GetComponent<PlayerShooting>();

            // Subscribe to health events.
            if (playerHealth != null)
            {
                playerHealth.OnHealthChanged += UpdateHealthBar;
                playerHealth.OnShieldChanged += UpdateShieldBar;
            }
        }

        // Subscribe to game events.
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnScoreChanged += UpdateScore;
            GameManager.Instance.OnComboChanged += UpdateCombo;
            GameManager.Instance.OnWaveStarted += AnnounceWave;
        }

        // Initialize displays.
        UpdateScore(GameManager.Instance != null ? GameManager.Instance.Score : 0);
        UpdateCombo(1);
        if (waveAnnouncementText != null) waveAnnouncementText.enabled = false;

        AnnounceWave(GameManager.Instance != null ? GameManager.Instance.CurrentWave : 1);
    }

    private void Update()
    {
        // Update weapon level display.
        if (weaponLevelText != null && playerShooting != null)
        {
            weaponLevelText.text = $"Weapon Lv.{playerShooting.WeaponLevel}";
        }

        // Fade out wave announcement.
        if (waveAnnounceTimer > 0f)
        {
            waveAnnounceTimer -= Time.deltaTime;
            if (waveAnnounceTimer <= 0f && waveAnnouncementText != null)
            {
                waveAnnouncementText.enabled = false;
            }
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe to prevent memory leaks.
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged -= UpdateHealthBar;
            playerHealth.OnShieldChanged -= UpdateShieldBar;
        }
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnScoreChanged -= UpdateScore;
            GameManager.Instance.OnComboChanged -= UpdateCombo;
            GameManager.Instance.OnWaveStarted -= AnnounceWave;
        }
    }

    // ========================================================================
    // UI Update Methods
    // ========================================================================

    private void UpdateHealthBar(int current, int max)
    {
        if (healthBar != null)
        {
            healthBar.maxValue = max;
            healthBar.value = current;
        }
        if (healthText != null)
        {
            healthText.text = $"{current}/{max}";
        }
    }

    private void UpdateShieldBar(int current, int max)
    {
        if (shieldBar != null)
        {
            shieldBar.maxValue = max;
            shieldBar.value = current;
            shieldBar.gameObject.SetActive(max > 0);
        }
    }

    private void UpdateScore(int score)
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {score:N0}";
        }
        if (highScoreText != null && GameManager.Instance != null)
        {
            highScoreText.text = $"Best: {GameManager.Instance.HighScore:N0}";
        }
    }

    private void UpdateCombo(int multiplier)
    {
        if (comboText != null)
        {
            comboText.text = multiplier > 1 ? $"x{multiplier} Combo!" : "";
            comboText.enabled = multiplier > 1;
        }
    }

    private void AnnounceWave(int wave)
    {
        if (waveText != null)
        {
            waveText.text = $"Wave {wave}";
        }
        if (waveAnnouncementText != null)
        {
            waveAnnouncementText.text = $"-- WAVE {wave} --";
            waveAnnouncementText.enabled = true;
            waveAnnounceTimer = waveAnnounceDuration;
        }
    }
}

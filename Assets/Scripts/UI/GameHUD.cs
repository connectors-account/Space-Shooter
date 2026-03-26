// ============================================================================
// GameHUD.cs — In-game heads-up display
// ============================================================================
using UnityEngine;
using UnityEngine.UI;

public class GameHUD : MonoBehaviour
{
    [Header("Score")]
    [SerializeField] private Text scoreText;
    [SerializeField] private Text highScoreText;

    [Header("Health")]
    [SerializeField] private Image[] healthIcons;     // array of heart/ship icons
    [SerializeField] private Text healthText;          // fallback text display
    [SerializeField] private Image healthBar;          // filled health bar

    [Header("Shield")]
    [SerializeField] private Image shieldBar;
    [SerializeField] private Text shieldText;

    [Header("Wave")]
    [SerializeField] private Text waveText;
    [SerializeField] private Text waveAnnouncementText; // large center text
    [SerializeField] private float waveAnnounceDuration = 2f;

    [Header("Lives")]
    [SerializeField] private Text livesText;

    [Header("Weapon")]
    [SerializeField] private Text weaponText;

    // Runtime
    private float waveAnnounceTimer;

    // =========================================================================
    private void OnEnable()
    {
        GameManager.OnScoreChanged += UpdateScore;
        GameManager.OnLivesChanged += UpdateLives;
        GameManager.OnWaveChanged += UpdateWave;
        PlayerHealth.OnHealthChanged += UpdateHealth;
        PlayerHealth.OnShieldChanged += UpdateShield;
    }

    private void OnDisable()
    {
        GameManager.OnScoreChanged -= UpdateScore;
        GameManager.OnLivesChanged -= UpdateLives;
        GameManager.OnWaveChanged -= UpdateWave;
        PlayerHealth.OnHealthChanged -= UpdateHealth;
        PlayerHealth.OnShieldChanged -= UpdateShield;
    }

    private void Start()
    {
        if (waveAnnouncementText != null)
            waveAnnouncementText.gameObject.SetActive(false);

        // Initialize displays
        UpdateScore(0);
        UpdateLives(GameManager.Instance != null ? GameManager.Instance.Lives : 3);
    }

    private void Update()
    {
        // Wave announcement fade
        if (waveAnnounceTimer > 0)
        {
            waveAnnounceTimer -= Time.deltaTime;
            if (waveAnnounceTimer <= 0 && waveAnnouncementText != null)
            {
                waveAnnouncementText.gameObject.SetActive(false);
            }
        }

        // Update weapon display
        UpdateWeaponDisplay();
    }

    // =========================================================================
    // Score
    // =========================================================================
    private void UpdateScore(int score)
    {
        if (scoreText != null)
            scoreText.text = $"SCORE: {score:N0}";

        int hs = GameManager.Instance != null ? GameManager.Instance.HighScore : 0;
        if (highScoreText != null)
            highScoreText.text = $"HI: {Mathf.Max(score, hs):N0}";
    }

    // =========================================================================
    // Health & Lives
    // =========================================================================
    private void UpdateHealth(int current, int max)
    {
        if (healthText != null)
            healthText.text = $"HP: {current}/{max}";

        if (healthBar != null)
            healthBar.fillAmount = max > 0 ? (float)current / max : 0;

        // Icon-based health display
        if (healthIcons != null)
        {
            for (int i = 0; i < healthIcons.Length; i++)
            {
                if (healthIcons[i] != null)
                    healthIcons[i].enabled = i < current;
            }
        }
    }

    private void UpdateLives(int lives)
    {
        if (livesText != null)
            livesText.text = $"LIVES: {lives}";
    }

    // =========================================================================
    // Shield
    // =========================================================================
    private void UpdateShield(int shield)
    {
        if (shieldText != null)
            shieldText.text = shield > 0 ? $"SHIELD: {shield}" : "";

        if (shieldBar != null)
            shieldBar.fillAmount = shield / 3f;
    }

    // =========================================================================
    // Wave
    // =========================================================================
    private void UpdateWave(int wave)
    {
        if (waveText != null)
        {
            int total = GameManager.Instance != null ? GameManager.Instance.TotalWaves : 10;
            waveText.text = $"WAVE {wave}/{total}";
        }

        // Big announcement in center
        if (waveAnnouncementText != null)
        {
            waveAnnouncementText.gameObject.SetActive(true);
            waveAnnouncementText.text = $"~ WAVE {wave} ~";
            waveAnnounceTimer = waveAnnounceDuration;
        }
    }

    // =========================================================================
    // Weapon
    // =========================================================================
    private void UpdateWeaponDisplay()
    {
        if (weaponText == null) return;

        GameObject player = GameManager.Instance?.PlayerShip;
        if (player == null) return;

        PlayerShooting shooting = player.GetComponent<PlayerShooting>();
        if (shooting != null)
        {
            weaponText.text = shooting.CurrentWeapon.ToString().ToUpper();
        }
    }
}

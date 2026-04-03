using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// HUDController updates the in-game heads-up display.
/// It shows the player's score, current health, active wave number,
/// and any power-up indicators.
/// Attach this to the HUD Canvas in the GamePlay scene.
/// </summary>
public class HUDController : MonoBehaviour
{
    // ============================================================
    // UI REFERENCES
    // ============================================================
    [Header("UI Text Elements")]
    [Tooltip("Text element displaying the current score")]
    public Text scoreText;

    [Tooltip("Text element displaying the current wave number")]
    public Text waveText;

    [Tooltip("Text element displaying current health")]
    public Text healthText;

    [Tooltip("Text element for wave announcement (briefly shown)")]
    public Text waveAnnounceText;

    [Header("Health Bar (Optional)")]
    [Tooltip("Slider for visual health bar")]
    public Slider healthBar;

    [Header("Power-Up Indicators")]
    [Tooltip("Text or image shown when rapid fire is active")]
    public GameObject rapidFireIndicator;

    [Tooltip("Text or image shown when shield is active")]
    public GameObject shieldIndicator;

    // ============================================================
    // INTERNAL
    // ============================================================
    private HealthSystem playerHealth;
    private PlayerController playerController;
    private float waveAnnounceTimer = 0f;

    // ============================================================
    // UNITY LIFECYCLE
    // ============================================================

    void Start()
    {
        // Subscribe to GameManager events
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnScoreChanged += UpdateScore;
            GameManager.Instance.OnWaveChanged += UpdateWave;
            GameManager.Instance.OnStateChanged += OnGameStateChanged;
        }

        // Find the player and subscribe to health changes
        FindPlayer();

        // Initialize display
        UpdateScore(0);
        UpdateWave(0);

        // Hide wave announcement and power-up indicators
        if (waveAnnounceText != null)
            waveAnnounceText.gameObject.SetActive(false);
        if (rapidFireIndicator != null)
            rapidFireIndicator.SetActive(false);
        if (shieldIndicator != null)
            shieldIndicator.SetActive(false);
    }

    void Update()
    {
        // Try to find player if we haven't yet
        if (playerController == null)
        {
            FindPlayer();
        }

        // Update power-up indicators based on player state
        UpdatePowerUpIndicators();

        // Hide wave announcement after a delay
        if (waveAnnounceTimer > 0f)
        {
            waveAnnounceTimer -= Time.deltaTime;
            if (waveAnnounceTimer <= 0f && waveAnnounceText != null)
            {
                waveAnnounceText.gameObject.SetActive(false);
            }
        }
    }

    void OnDestroy()
    {
        // Unsubscribe from events
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnScoreChanged -= UpdateScore;
            GameManager.Instance.OnWaveChanged -= UpdateWave;
            GameManager.Instance.OnStateChanged -= OnGameStateChanged;
        }

        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged -= UpdateHealth;
        }
    }

    // ============================================================
    // FIND PLAYER
    // ============================================================

    /// <summary>
    /// Locate the player object and subscribe to its health events.
    /// </summary>
    void FindPlayer()
    {
        // Try GameManager reference first
        GameObject playerObj = GameManager.Instance != null
            ? GameManager.Instance.Player
            : GameObject.FindGameObjectWithTag("Player");

        if (playerObj == null)
            playerObj = GameObject.FindGameObjectWithTag("Player");

        if (playerObj != null)
        {
            playerController = playerObj.GetComponent<PlayerController>();
            playerHealth = playerObj.GetComponent<HealthSystem>();

            if (playerHealth != null)
            {
                playerHealth.OnHealthChanged += UpdateHealth;
                // Initialize health display
                UpdateHealth(playerHealth.CurrentHealth, playerHealth.maxHealth);
            }
        }
    }

    // ============================================================
    // UI UPDATE METHODS
    // ============================================================

    /// <summary>Update the score display.</summary>
    void UpdateScore(int newScore)
    {
        if (scoreText != null)
            scoreText.text = "SCORE: " + newScore.ToString();
    }

    /// <summary>Update the wave number display and show announcement.</summary>
    void UpdateWave(int newWave)
    {
        if (waveText != null)
            waveText.text = "WAVE: " + newWave.ToString();

        // Show big wave announcement briefly
        if (waveAnnounceText != null && newWave > 0)
        {
            waveAnnounceText.text = "WAVE " + newWave;
            waveAnnounceText.gameObject.SetActive(true);
            waveAnnounceTimer = 2f; // show for 2 seconds
        }
    }

    /// <summary>Update the health display (text and optional slider).</summary>
    void UpdateHealth(int current, int max)
    {
        if (healthText != null)
            healthText.text = "HP: " + current + " / " + max;

        if (healthBar != null)
        {
            healthBar.maxValue = max;
            healthBar.value = current;
        }
    }

    /// <summary>Show/hide power-up status indicators.</summary>
    void UpdatePowerUpIndicators()
    {
        if (playerController == null) return;

        if (rapidFireIndicator != null)
            rapidFireIndicator.SetActive(playerController.hasRapidFire);

        if (shieldIndicator != null)
            shieldIndicator.SetActive(playerController.hasShield);
    }

    /// <summary>React to game state changes (e.g., hide HUD on game over).</summary>
    void OnGameStateChanged(GameManager.GameState state)
    {
        // HUD stays visible in most states; could be hidden if desired
    }
}

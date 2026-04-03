using UnityEngine;

/// <summary>
/// PowerUpController defines the behavior for collectible power-ups.
/// When the player touches a power-up, it applies its effect.
///
/// Power-up types:
///   - Health:    Restores 1 HP
///   - RapidFire: Doubles fire rate for 8 seconds
///   - Shield:    Absorbs one hit for 10 seconds
/// </summary>
public class PowerUpController : MonoBehaviour
{
    // ============================================================
    // CONFIGURATION
    // ============================================================

    /// <summary>The three power-up types available in the game.</summary>
    public enum PowerUpType { Health, RapidFire, Shield }

    [Header("Power-Up Settings")]
    public PowerUpType type = PowerUpType.Health;

    [Tooltip("How fast the power-up drifts downward")]
    public float fallSpeed = 2f;

    [Tooltip("Duration for timed power-ups (RapidFire, Shield)")]
    public float effectDuration = 8f;

    [Tooltip("Health restored by the Health power-up")]
    public int healthAmount = 1;

    [Tooltip("Seconds before the power-up despawns if not collected")]
    public float despawnTime = 10f;

    // ============================================================
    // VISUAL
    // ============================================================
    [Header("Visual")]
    [Tooltip("Bob amplitude (gentle up-down float)")]
    public float bobAmplitude = 0.2f;
    public float bobFrequency = 2f;

    // ============================================================
    // INTERNAL
    // ============================================================
    private float spawnTime;
    private float baseY;

    // ============================================================
    // UNITY LIFECYCLE
    // ============================================================

    void Start()
    {
        spawnTime = Time.time;
        baseY = transform.position.y;
        gameObject.tag = "PowerUp";
    }

    void Update()
    {
        // Drift downward
        baseY -= fallSpeed * Time.deltaTime;

        // Gentle bobbing motion to catch the player's eye
        float bob = Mathf.Sin(Time.time * bobFrequency) * bobAmplitude;
        transform.position = new Vector3(transform.position.x, baseY + bob, 0f);

        // Despawn if uncollected for too long
        if (Time.time - spawnTime >= despawnTime)
        {
            Destroy(gameObject);
            return;
        }

        // Destroy if off screen
        if (transform.position.y < -7f)
        {
            Destroy(gameObject);
        }
    }

    // ============================================================
    // COLLISION WITH PLAYER
    // ============================================================

    /// <summary>
    /// When the player collides with this power-up, apply the effect.
    /// Uses trigger collisions (both must have colliders, one trigger).
    /// </summary>
    void OnTriggerEnter2D(Collider2D other)
    {
        // Only react to the player
        if (!other.CompareTag("Player")) return;

        PlayerController player = other.GetComponent<PlayerController>();
        if (player == null) return;

        // Apply the power-up effect
        ApplyEffect(player);

        // Destroy the power-up after collection
        Destroy(gameObject);
    }

    /// <summary>
    /// Apply the appropriate effect based on the power-up type.
    /// </summary>
    void ApplyEffect(PlayerController player)
    {
        switch (type)
        {
            case PowerUpType.Health:
                // Restore health
                player.Heal(healthAmount);
                Debug.Log("Power-up collected: Health +" + healthAmount);
                break;

            case PowerUpType.RapidFire:
                // Enable rapid fire for a duration
                player.ActivateRapidFire(effectDuration);
                Debug.Log("Power-up collected: Rapid Fire for " + effectDuration + "s");
                break;

            case PowerUpType.Shield:
                // Enable damage-absorbing shield
                player.ActivateShield(effectDuration);
                Debug.Log("Power-up collected: Shield for " + effectDuration + "s");
                break;
        }
    }
}

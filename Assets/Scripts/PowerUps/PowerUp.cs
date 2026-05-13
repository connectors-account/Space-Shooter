// ============================================================================
// PowerUp.cs - Collectible power-up items dropped by enemies
// Supports Health, Shield, and Weapon Upgrade types.
// ============================================================================
using UnityEngine;

/// <summary>
/// A collectible power-up that drifts downward. When the player touches it,
/// the appropriate effect is applied and the pickup disappears.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class PowerUp : MonoBehaviour
{
    /// <summary>Available power-up effect types.</summary>
    public enum PowerUpType
    {
        Health,
        Shield,
        WeaponUpgrade
    }

    [Header("Power-Up Configuration")]
    [Tooltip("Which effect this power-up grants.")]
    [SerializeField] private PowerUpType type = PowerUpType.Health;
    [Tooltip("Magnitude of the effect (HP healed, shield points, etc.).")]
    [SerializeField] private int effectAmount = 25;

    [Header("Movement")]
    [Tooltip("Downward drift speed.")]
    [SerializeField] private float fallSpeed = 2f;
    [Tooltip("Horizontal bob amplitude.")]
    [SerializeField] private float bobAmplitude = 0.5f;
    [Tooltip("Horizontal bob frequency.")]
    [SerializeField] private float bobFrequency = 2f;

    [Header("Visual")]
    [Tooltip("Rotation speed in degrees per second for visual flair.")]
    [SerializeField] private float rotationSpeed = 90f;

    private float spawnTime;
    private float startX;

    // ========================================================================
    // Unity Lifecycle
    // ========================================================================

    private void Start()
    {
        spawnTime = Time.time;
        startX = transform.position.x;

        // Set tag for identification.
        gameObject.tag = "PowerUp";

        // Ensure trigger collider.
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    private void Update()
    {
        // Drift downward with a gentle horizontal bob.
        float elapsed = Time.time - spawnTime;
        float xOffset = Mathf.Sin(elapsed * bobFrequency) * bobAmplitude;
        float newX = startX + xOffset;
        float newY = transform.position.y - fallSpeed * Time.deltaTime;
        transform.position = new Vector3(newX, newY, transform.position.z);

        // Spin for visual feedback.
        transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);

        // Despawn if off-screen.
        if (GameBounds.Instance != null && GameBounds.Instance.IsOutOfBounds(transform.position))
        {
            Destroy(gameObject);
        }
    }

    // ========================================================================
    // Collision
    // ========================================================================

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        ApplyEffect(other.gameObject);

        // Play pickup SFX.
        AudioManager.Instance?.PlaySFX(AudioManager.SFX.PowerUp);

        // Destroy this power-up.
        Destroy(gameObject);
    }

    // ========================================================================
    // Effect Application
    // ========================================================================

    /// <summary>
    /// Applies this power-up's effect to the player.
    /// </summary>
    private void ApplyEffect(GameObject player)
    {
        switch (type)
        {
            case PowerUpType.Health:
                PlayerHealth health = player.GetComponent<PlayerHealth>();
                if (health != null)
                {
                    health.Heal(effectAmount);
                }
                break;

            case PowerUpType.Shield:
                PlayerHealth shieldHealth = player.GetComponent<PlayerHealth>();
                if (shieldHealth != null)
                {
                    shieldHealth.AddShield(effectAmount);
                }
                break;

            case PowerUpType.WeaponUpgrade:
                PlayerShooting shooting = player.GetComponent<PlayerShooting>();
                if (shooting != null)
                {
                    shooting.UpgradeWeapon();
                }
                break;
        }
    }
}

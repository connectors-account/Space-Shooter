// =============================================================================
// PowerUp.cs
// Handles power-up item behavior: drifts downward, collected on contact with
// the player, and applies the configured effect (shield, rapid fire, or heal).
// Attach this script to each power-up prefab.
// =============================================================================
using UnityEngine;

/// <summary>
/// Types of power-ups available in the game.
/// </summary>
public enum PowerUpType
{
    /// <summary>Grants a shield that absorbs one hit.</summary>
    Shield,
    /// <summary>Increases fire rate for a limited duration.</summary>
    RapidFire,
    /// <summary>Restores health points.</summary>
    Health
}

public class PowerUp : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Power-Up Settings
    // -------------------------------------------------------------------------
    [Header("Power-Up Configuration")]
    [Tooltip("The type of power-up this item grants.")]
    public PowerUpType powerUpType = PowerUpType.Shield;

    [Tooltip("Speed at which the power-up drifts downward.")]
    public float fallSpeed = 2f;

    [Tooltip("Health restored by the Health power-up.")]
    public int healAmount = 2;

    [Tooltip("Duration of the Rapid Fire power-up in seconds.")]
    public float rapidFireDuration = 5f;

    [Tooltip("Time in seconds before the power-up auto-destroys if not collected.")]
    public float lifetime = 10f;

    // -------------------------------------------------------------------------
    // Visual Effects
    // -------------------------------------------------------------------------
    [Header("Visual")]
    [Tooltip("Rotation speed for visual flair (degrees per second).")]
    public float rotateSpeed = 90f;

    [Tooltip("Whether the power-up bobs up and down while falling.")]
    public bool bobEffect = true;

    [Tooltip("Amplitude of the bobbing effect.")]
    public float bobAmplitude = 0.3f;

    [Tooltip("Speed of the bobbing effect.")]
    public float bobFrequency = 3f;

    // -------------------------------------------------------------------------
    // Internal
    // -------------------------------------------------------------------------
    private float aliveTime = 0f;
    private float startY;
    private float lifetimeTimer;

    // -------------------------------------------------------------------------
    // Unity Lifecycle
    // -------------------------------------------------------------------------

    /// <summary>
    /// Initialize timers and cache starting position.
    /// </summary>
    void Start()
    {
        startY = transform.position.y;
        lifetimeTimer = lifetime;
    }

    /// <summary>
    /// Move the power-up downward with optional bob and rotation effects.
    /// </summary>
    void Update()
    {
        aliveTime += Time.deltaTime;

        // Move downward
        float newY = transform.position.y - fallSpeed * Time.deltaTime;

        // Optional bobbing effect (oscillates relative to the fall position)
        if (bobEffect)
        {
            float bobOffset = Mathf.Sin(aliveTime * bobFrequency) * bobAmplitude * Time.deltaTime;
            newY += bobOffset;
        }

        transform.position = new Vector3(transform.position.x, newY, 0f);

        // Rotate for visual effect
        transform.Rotate(Vector3.forward, rotateSpeed * Time.deltaTime);

        // Lifetime countdown
        lifetimeTimer -= Time.deltaTime;

        // Flash before expiring (last 2 seconds)
        if (lifetimeTimer < 2f)
        {
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                // Blink on/off
                sr.enabled = (Mathf.FloorToInt(lifetimeTimer * 5f) % 2 == 0);
            }
        }

        // Destroy if lifetime expired or off-screen
        if (lifetimeTimer <= 0f || transform.position.y < -7f)
        {
            Destroy(gameObject);
        }
    }

    // -------------------------------------------------------------------------
    // Collection (Collision)
    // -------------------------------------------------------------------------

    /// <summary>
    /// When the player collects this power-up, apply the effect and destroy.
    /// </summary>
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                ApplyPowerUp(player);
            }

            AudioManager.Instance?.PlaySFX("PowerUp");

            // Destroy the power-up item
            Destroy(gameObject);
        }
    }

    // -------------------------------------------------------------------------
    // Power-Up Application
    // -------------------------------------------------------------------------

    /// <summary>
    /// Applies the power-up effect to the player based on the configured type.
    /// </summary>
    /// <param name="player">The PlayerController to apply the effect to.</param>
    private void ApplyPowerUp(PlayerController player)
    {
        switch (powerUpType)
        {
            case PowerUpType.Shield:
                player.ActivateShield();
                Debug.Log("Power-Up: Shield activated!");
                break;

            case PowerUpType.RapidFire:
                player.ActivateRapidFire(rapidFireDuration);
                Debug.Log("Power-Up: Rapid Fire activated for " + rapidFireDuration + " seconds!");
                break;

            case PowerUpType.Health:
                player.Heal(healAmount);
                Debug.Log("Power-Up: Healed " + healAmount + " HP!");
                break;
        }
    }
}

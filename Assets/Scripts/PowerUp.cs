using UnityEngine;

/// <summary>
/// A collectible power-up that drifts downward. When the player touches it,
/// it applies one of three effects (health, rapid fire, or shield) and then
/// destroys itself.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class PowerUp : MonoBehaviour
{
    /// <summary>The three kinds of power-up available.</summary>
    public enum PowerUpType
    {
        Health,
        RapidFire,
        Shield
    }

    [Header("Power-up Settings")]
    [Tooltip("Which effect this power-up grants.")]
    public PowerUpType type = PowerUpType.Health;

    [Tooltip("Downward drift speed in units/second.")]
    public float fallSpeed = 2f;

    [Tooltip("Y below which the power-up despawns if uncollected.")]
    public float despawnY = -6f;

    [Header("Effect Amounts")]
    [Tooltip("HP restored by a Health power-up.")]
    public int healthAmount = 30;

    [Tooltip("Duration (seconds) of RapidFire and Shield power-ups.")]
    public float effectDuration = 6f;

    private void Update()
    {
        // Drift downward.
        transform.position += Vector3.down * fallSpeed * Time.deltaTime;

        // Clean up if it leaves the screen.
        if (transform.position.y < despawnY)
            Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Only the player can collect power-ups.
        if (!other.CompareTag("Player"))
            return;

        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null)
            ApplyEffect(player);

        Destroy(gameObject);
    }

    /// <summary>Apply this power-up's effect to the player.</summary>
    private void ApplyEffect(PlayerController player)
    {
        switch (type)
        {
            case PowerUpType.Health:
                player.ApplyHealth(healthAmount);
                break;
            case PowerUpType.RapidFire:
                player.ApplyRapidFire(effectDuration);
                break;
            case PowerUpType.Shield:
                player.ApplyShield(effectDuration);
                break;
        }
    }
}

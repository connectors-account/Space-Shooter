// ============================================================================
// PowerUpController.cs - Power-up behavior and application
// ============================================================================
using UnityEngine;

/// <summary>
/// Handles power-up movement (drifts downward) and applies its effect
/// to the player on pickup. Supports: Health, WeaponUpgrade, Shield, SpeedBoost.
/// </summary>
public class PowerUpController : MonoBehaviour
{
    // ---- Power-Up Types ----
    public enum PowerUpType
    {
        Health,         // Restores player health
        WeaponUpgrade,  // Upgrades weapon level
        Shield,         // Temporary invincibility
        SpeedBoost      // Temporary speed increase
    }

    // ---- Configuration ----
    [Header("Power-Up Settings")]
    public PowerUpType type = PowerUpType.Health;

    [Tooltip("How fast the power-up drifts downward")]
    public float driftSpeed = 2f;

    [Tooltip("Health restored (Health type only)")]
    public int healthAmount = 30;

    [Tooltip("Shield duration in seconds (Shield type only)")]
    public float shieldDuration = 5f;

    [Tooltip("Speed multiplier (SpeedBoost type only)")]
    public float speedMultiplier = 1.5f;

    [Tooltip("Speed boost duration (SpeedBoost type only)")]
    public float speedDuration = 5f;

    [Tooltip("Seconds before the power-up despawns if not collected")]
    public float lifetime = 10f;

    // ---- Visual ----
    [Header("Visual")]
    public float bobAmplitude = 0.2f;
    public float bobFrequency = 3f;

    // ---- Internal ----
    private float _spawnTime;
    private Vector3 _startPos;

    // ========================================================================
    // Unity Lifecycle
    // ========================================================================
    private void Start()
    {
        _spawnTime = Time.time;
        _startPos = transform.position;
        gameObject.tag = "PowerUp";

        // Tint the sprite based on type for visual distinction
        ApplyColor();
    }

    private void Update()
    {
        // Drift downward
        transform.Translate(Vector3.down * driftSpeed * Time.deltaTime, Space.World);

        // Bob up and down for visual flair
        float bob = Mathf.Sin((Time.time - _spawnTime) * bobFrequency) * bobAmplitude;
        Vector3 pos = transform.position;
        pos.x += bob * Time.deltaTime;
        transform.position = pos;

        // Despawn after lifetime
        if (Time.time - _spawnTime > lifetime)
        {
            Destroy(gameObject);
        }

        // Destroy if off-screen
        if (Camera.main != null)
        {
            Vector3 vp = Camera.main.WorldToViewportPoint(transform.position);
            if (vp.y < -0.1f) Destroy(gameObject);
        }
    }

    // ========================================================================
    // Apply Power-Up to Player
    // ========================================================================

    /// <summary>
    /// Called by CollisionHandler when the player touches this power-up.
    /// </summary>
    public void ApplyPowerUp(GameObject player)
    {
        PlayerController pc = player.GetComponent<PlayerController>();
        HealthSystem hs = player.GetComponent<HealthSystem>();

        switch (type)
        {
            case PowerUpType.Health:
                if (hs != null)
                    hs.Heal(healthAmount);
                break;

            case PowerUpType.WeaponUpgrade:
                if (pc != null)
                    pc.UpgradeWeapon();
                break;

            case PowerUpType.Shield:
                if (pc != null)
                    pc.ActivateShield(shieldDuration);
                break;

            case PowerUpType.SpeedBoost:
                if (pc != null)
                    pc.ActivateSpeedBoost(speedMultiplier, speedDuration);
                break;
        }

        // Play pickup sound
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX("PowerUp");

        // Destroy the power-up object
        Destroy(gameObject);
    }

    // ========================================================================
    // Visual Helper
    // ========================================================================
    private void ApplyColor()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr == null) return;

        switch (type)
        {
            case PowerUpType.Health:
                sr.color = Color.green;
                break;
            case PowerUpType.WeaponUpgrade:
                sr.color = new Color(1f, 0.5f, 0f); // Orange
                break;
            case PowerUpType.Shield:
                sr.color = new Color(0.3f, 0.7f, 1f); // Light blue
                break;
            case PowerUpType.SpeedBoost:
                sr.color = Color.yellow;
                break;
        }
    }
}

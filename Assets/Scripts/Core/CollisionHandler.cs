// ============================================================================
// CollisionHandler.cs - Centralized collision detection logic
// Uses Unity’s 2D trigger system. Attach to objects that need collision.
// Layers/Tags used: "Player", "Enemy", "PlayerBullet", "EnemyBullet", "PowerUp"
// ============================================================================
using UnityEngine;

/// <summary>
/// Handles all trigger-based 2D collisions.
/// Attach this to Player, Enemies, Bullets, and PowerUps.
/// It checks tags and delegates to the appropriate systems.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class CollisionHandler : MonoBehaviour
{
    // ---- Configuration ----
    [Header("Collision Settings")]
    [Tooltip("Damage dealt when this object collides (bullets, enemies ramming)")]
    public int contactDamage = 10;

    [Tooltip("Should this object be destroyed on collision?")]
    public bool destroyOnContact = false;

    [Tooltip("Points awarded when this object is destroyed (enemies only)")]
    public int scoreValue = 0;

    // Cached references
    private HealthSystem _health;

    private void Awake()
    {
        _health = GetComponent<HealthSystem>();
    }

    // ========================================================================
    // Trigger Callbacks (2D)
    // ========================================================================
    private void OnTriggerEnter2D(Collider2D other)
    {
        // --- Player bullet hits Enemy ---
        if (gameObject.CompareTag("Enemy") && other.CompareTag("PlayerBullet"))
        {
            HandleBulletHit(other.gameObject);
            return;
        }

        // --- Enemy bullet hits Player ---
        if (gameObject.CompareTag("Player") && other.CompareTag("EnemyBullet"))
        {
            HandleBulletHit(other.gameObject);
            return;
        }

        // --- Enemy rams Player (body collision) ---
        if (gameObject.CompareTag("Player") && other.CompareTag("Enemy"))
        {
            // Both take damage
            CollisionHandler otherCH = other.GetComponent<CollisionHandler>();
            if (_health != null && otherCH != null)
            {
                _health.TakeDamage(otherCH.contactDamage);
            }
            // Enemy also takes damage from ramming
            HealthSystem otherHealth = other.GetComponent<HealthSystem>();
            if (otherHealth != null)
            {
                otherHealth.TakeDamage(contactDamage);
            }
            return;
        }

        // --- Player picks up PowerUp ---
        if (gameObject.CompareTag("Player") && other.CompareTag("PowerUp"))
        {
            PowerUpController pu = other.GetComponent<PowerUpController>();
            if (pu != null)
            {
                pu.ApplyPowerUp(gameObject);
            }
            return;
        }
    }

    /// <summary>Handle a bullet hitting this object.</summary>
    private void HandleBulletHit(GameObject bullet)
    {
        // Get bullet damage
        BulletController bc = bullet.GetComponent<BulletController>();
        int dmg = bc != null ? bc.damage : 10;

        // Apply damage to this object
        if (_health != null)
        {
            _health.TakeDamage(dmg);
        }

        // Destroy the bullet
        Destroy(bullet);
    }
}

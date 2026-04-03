using UnityEngine;

/// <summary>
/// CollisionHandler sits on the Player and on Enemies.
/// It detects trigger collisions between bullets and ships,
/// applying damage through the HealthSystem.
///
/// Collision rules:
///   - PlayerBullet  hits Enemy  → enemy takes damage
///   - EnemyBullet   hits Player → player takes damage (unless shielded)
///   - Enemy body     hits Player → both take damage
/// </summary>
public class CollisionHandler : MonoBehaviour
{
    [Tooltip("Is this the player? Determines collision logic.")]
    public bool isPlayer = false;

    private HealthSystem healthSystem;
    private PlayerController playerController;

    void Start()
    {
        healthSystem = GetComponent<HealthSystem>();

        if (isPlayer)
        {
            playerController = GetComponent<PlayerController>();

            // Subscribe to death event to trigger game over
            if (healthSystem != null)
            {
                healthSystem.OnDeath += OnPlayerDeath;
            }
        }
    }

    void OnDestroy()
    {
        if (isPlayer && healthSystem != null)
        {
            healthSystem.OnDeath -= OnPlayerDeath;
        }
    }

    /// <summary>
    /// Handle trigger collisions. Both objects need Collider2D (one set to trigger).
    /// </summary>
    void OnTriggerEnter2D(Collider2D other)
    {
        if (isPlayer)
        {
            HandlePlayerCollision(other);
        }
        else
        {
            HandleEnemyCollision(other);
        }
    }

    // ============================================================
    // PLAYER COLLISION LOGIC
    // ============================================================

    /// <summary>
    /// The player was hit by something. Check what it was.
    /// </summary>
    void HandlePlayerCollision(Collider2D other)
    {
        // Hit by enemy bullet
        if (other.CompareTag("EnemyBullet"))
        {
            BulletController bullet = other.GetComponent<BulletController>();
            int damage = bullet != null ? bullet.damage : 1;

            // Check if shield absorbs the hit
            if (playerController != null && playerController.TryAbsorbDamage())
            {
                // Shield absorbed – destroy the bullet, no damage taken
                Destroy(other.gameObject);
                return;
            }

            // Apply damage to player
            if (healthSystem != null)
            {
                healthSystem.TakeDamage(damage);
            }

            // Destroy the bullet
            Destroy(other.gameObject);
        }
        // Collided with enemy body (ramming)
        else if (other.CompareTag("Enemy"))
        {
            // Check shield first
            if (playerController != null && playerController.TryAbsorbDamage())
            {
                // Destroy the ramming enemy but player is safe
                HealthSystem enemyHealth = other.GetComponent<HealthSystem>();
                if (enemyHealth != null)
                    enemyHealth.TakeDamage(999); // instant kill
                return;
            }

            // Both take damage
            if (healthSystem != null)
                healthSystem.TakeDamage(1);

            HealthSystem enemyHp = other.GetComponent<HealthSystem>();
            if (enemyHp != null)
                enemyHp.TakeDamage(999); // enemy dies on ram
        }
    }

    // ============================================================
    // ENEMY COLLISION LOGIC
    // ============================================================

    /// <summary>
    /// An enemy was hit by something. Check what it was.
    /// </summary>
    void HandleEnemyCollision(Collider2D other)
    {
        // Hit by player bullet
        if (other.CompareTag("PlayerBullet"))
        {
            BulletController bullet = other.GetComponent<BulletController>();
            int damage = bullet != null ? bullet.damage : 1;

            // Apply damage to this enemy
            if (healthSystem != null)
            {
                healthSystem.TakeDamage(damage);
            }

            // Destroy the bullet on impact
            Destroy(other.gameObject);
        }
    }

    // ============================================================
    // PLAYER DEATH
    // ============================================================

    /// <summary>
    /// Called when the player's health reaches zero.
    /// Notifies the GameManager to end the game.
    /// </summary>
    void OnPlayerDeath()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.TriggerGameOver();
        }

        // Disable player visuals and controls
        // (don't destroy yet so GameOver screen can reference it)
        gameObject.SetActive(false);
    }
}

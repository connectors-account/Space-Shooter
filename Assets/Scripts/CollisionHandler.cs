using UnityEngine;

/// <summary>
/// Centralized collision handling for the game.
/// Attach to any GameObject with a Collider2D that needs collision response.
/// Uses Unity's trigger system for all game collisions.
/// </summary>
public class CollisionHandler : MonoBehaviour
{
    public enum ColliderOwner
    {
        Player,
        Enemy,
        PlayerBullet,
        EnemyBullet,
        PowerUp
    }

    [SerializeField] private ColliderOwner ownerType = ColliderOwner.Player;

    private void OnTriggerEnter2D(Collider2D other)
    {
        switch (ownerType)
        {
            case ColliderOwner.Player:
                HandlePlayerCollision(other);
                break;
            case ColliderOwner.Enemy:
                HandleEnemyCollision(other);
                break;
            case ColliderOwner.PlayerBullet:
                HandlePlayerBulletCollision(other);
                break;
            case ColliderOwner.EnemyBullet:
                HandleEnemyBulletCollision(other);
                break;
            case ColliderOwner.PowerUp:
                HandlePowerUpCollision(other);
                break;
        }
    }

    private void HandlePlayerCollision(Collider2D other)
    {
        // Player hit by enemy bullet
        BulletController bullet = other.GetComponent<BulletController>();
        if (bullet != null && !bullet.IsPlayerBullet)
        {
            HealthSystem hs = GetComponent<HealthSystem>();
            if (hs != null)
            {
                hs.TakeDamage(bullet.Damage);
            }
            Destroy(other.gameObject);
            return;
        }

        // Player collides with enemy (contact damage)
        EnemyController enemy = other.GetComponent<EnemyController>();
        if (enemy != null)
        {
            HealthSystem playerHs = GetComponent<HealthSystem>();
            if (playerHs != null)
            {
                playerHs.TakeDamage(30); // Contact damage
            }

            // Destroy the enemy on contact
            HealthSystem enemyHs = other.GetComponent<HealthSystem>();
            if (enemyHs != null)
            {
                enemyHs.TakeDamage(9999);
            }
            return;
        }

        // Player collects power-up
        PowerUpController powerUp = other.GetComponent<PowerUpController>();
        if (powerUp != null)
        {
            PlayerController pc = GetComponent<PlayerController>();
            if (pc != null)
            {
                powerUp.ApplyEffect(pc);
            }
        }
    }

    private void HandleEnemyCollision(Collider2D other)
    {
        // Enemy hit by player bullet
        BulletController bullet = other.GetComponent<BulletController>();
        if (bullet != null && bullet.IsPlayerBullet)
        {
            HealthSystem hs = GetComponent<HealthSystem>();
            if (hs != null)
            {
                hs.TakeDamage(bullet.Damage);
            }
            Destroy(other.gameObject);
        }
    }

    private void HandlePlayerBulletCollision(Collider2D other)
    {
        // Player bullet hits enemy
        EnemyController enemy = other.GetComponent<EnemyController>();
        if (enemy != null)
        {
            HealthSystem hs = other.GetComponent<HealthSystem>();
            BulletController bc = GetComponent<BulletController>();

            if (hs != null && bc != null)
            {
                hs.TakeDamage(bc.Damage);
            }
            Destroy(gameObject);
        }
    }

    private void HandleEnemyBulletCollision(Collider2D other)
    {
        // Enemy bullet hits player
        if (other.CompareTag("Player"))
        {
            HealthSystem hs = other.GetComponent<HealthSystem>();
            BulletController bc = GetComponent<BulletController>();

            if (hs != null && bc != null)
            {
                hs.TakeDamage(bc.Damage);
            }
            Destroy(gameObject);
        }
    }

    private void HandlePowerUpCollision(Collider2D other)
    {
        // Power-up collected by player
        if (other.CompareTag("Player"))
        {
            PowerUpController pc = GetComponent<PowerUpController>();
            PlayerController player = other.GetComponent<PlayerController>();

            if (pc != null && player != null)
            {
                pc.ApplyEffect(player);
            }
        }
    }
}

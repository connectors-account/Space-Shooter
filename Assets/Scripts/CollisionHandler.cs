using UnityEngine;

/// <summary>
/// Centralized trigger/collision resolution for bullets, ships, and pickups.
/// Attach this script to Player, Enemy, Bullet, and PowerUp prefabs.
/// </summary>
public class CollisionHandler : MonoBehaviour
{
    private PlayerController player;
    private EnemyController enemy;
    private BulletController bullet;
    private PowerUpController powerUp;

    private void Awake()
    {
        player = GetComponent<PlayerController>();
        enemy = GetComponent<EnemyController>();
        bullet = GetComponent<BulletController>();
        powerUp = GetComponent<PowerUpController>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
        {
            return;
        }

        // Bullet -> Target logic
        if (bullet != null)
        {
            HandleBulletHit(other);
            return;
        }

        // Player direct collisions
        if (player != null)
        {
            HandlePlayerHit(other);
            return;
        }

        // Enemy direct collisions
        if (enemy != null)
        {
            HandleEnemyHit(other);
            return;
        }

        // Pickup collection
        if (powerUp != null)
        {
            HandlePowerUpCollected(other);
        }
    }

    private void HandleBulletHit(Collider2D other)
    {
        if (bullet.IsPlayerBullet)
        {
            EnemyController targetEnemy = other.GetComponent<EnemyController>();
            if (targetEnemy != null)
            {
                targetEnemy.TakeDamage(bullet.Damage);
                Destroy(gameObject);
            }
        }
        else
        {
            PlayerController targetPlayer = other.GetComponent<PlayerController>();
            if (targetPlayer != null)
            {
                targetPlayer.TakeDamage(bullet.Damage);
                Destroy(gameObject);
            }
        }
    }

    private void HandlePlayerHit(Collider2D other)
    {
        EnemyController hitEnemy = other.GetComponent<EnemyController>();
        if (hitEnemy != null)
        {
            player.TakeDamage(hitEnemy.ContactDamage);
            hitEnemy.TakeDamage(9999); // crash damage destroys enemy
            return;
        }

        PowerUpController pickedPowerUp = other.GetComponent<PowerUpController>();
        if (pickedPowerUp != null)
        {
            pickedPowerUp.ApplyTo(player);
            Destroy(pickedPowerUp.gameObject);
        }
    }

    private void HandleEnemyHit(Collider2D other)
    {
        PlayerController hitPlayer = other.GetComponent<PlayerController>();
        if (hitPlayer != null)
        {
            hitPlayer.TakeDamage(enemy.ContactDamage);
            enemy.TakeDamage(9999);
        }
    }

    private void HandlePowerUpCollected(Collider2D other)
    {
        PlayerController targetPlayer = other.GetComponent<PlayerController>();
        if (targetPlayer != null)
        {
            powerUp.ApplyTo(targetPlayer);
            Destroy(gameObject);
        }
    }
}

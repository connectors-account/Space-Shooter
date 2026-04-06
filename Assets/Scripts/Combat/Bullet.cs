using UnityEngine;

/// <summary>
/// Bullet behavior for both player and enemy projectiles.
/// Moves in a given direction, deals damage on hit, and returns to pool when off-screen.
/// </summary>
public class Bullet : MonoBehaviour, IPoolable
{
    [Header("Bullet Settings")]
    public string poolTag = "PlayerBullet";

    private Vector2 direction;
    private float speed;
    private int damage;
    private bool isPlayerBullet;

    /// <summary>
    /// Initialize bullet with direction, speed, damage, and ownership.
    /// Called by the shooter after spawning from pool.
    /// </summary>
    public void Initialize(Vector2 dir, float spd, int dmg, bool playerBullet)
    {
        direction = dir.normalized;
        speed = spd;
        damage = dmg;
        isPlayerBullet = playerBullet;

        // Set tag based on who shot it
        poolTag = playerBullet ? "PlayerBullet" : "EnemyBullet";

        // Rotate bullet to face movement direction
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    public void OnSpawnFromPool()
    {
        // Reset is handled by Initialize
    }

    private void Update()
    {
        transform.Translate(direction * speed * Time.deltaTime, Space.World);

        // Return to pool if off-screen
        if (GameBounds.Instance != null && GameBounds.Instance.IsOutOfBounds(transform.position, 2f))
        {
            ReturnToPool();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isPlayerBullet)
        {
            // Player bullet hits enemy
            EnemyBase enemy = other.GetComponent<EnemyBase>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
                ReturnToPool();
            }
        }
        else
        {
            // Enemy bullet hits player
            if (other.CompareTag("Player"))
            {
                PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(damage);
                }
                ReturnToPool();
            }
        }
    }

    private void ReturnToPool()
    {
        if (ObjectPool.Instance != null)
        {
            ObjectPool.Instance.ReturnToPool(poolTag, gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}

using UnityEngine;

/// <summary>
/// Generic bullet behavior for both player and enemy bullets.
/// Handles movement, damage, and auto-destruction when off screen.
/// </summary>
public class Bullet : MonoBehaviour
{
    [Header("Bullet Settings")]
    public float speed = 12f;
    public int damage = 10;
    public float lifetime = 5f;

    private Vector2 direction;
    private bool isPlayerBullet;
    private float lifeTimer;

    public void Initialize(Vector2 dir, bool playerBullet)
    {
        direction = dir.normalized;
        isPlayerBullet = playerBullet;
        lifeTimer = lifetime;

        // Set tag for collision detection
        gameObject.tag = playerBullet ? "PlayerBullet" : "EnemyBullet";

        // Set layer
        gameObject.layer = LayerMask.NameToLayer(playerBullet ? "PlayerBullet" : "EnemyBullet");

        // Rotate to face movement direction
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    private void Update()
    {
        transform.Translate(direction * speed * Time.deltaTime, Space.World);

        lifeTimer -= Time.deltaTime;
        if (lifeTimer <= 0f)
        {
            Destroy(gameObject);
            return;
        }

        // Destroy if out of bounds
        if (GameBounds.Instance != null && GameBounds.Instance.IsOutOfBounds(transform.position, 3f))
        {
            Destroy(gameObject);
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
                EffectsManager.Instance?.SpawnHitEffect(transform.position);
                Destroy(gameObject);
            }
        }
        else
        {
            // Enemy bullet hits player
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.TakeDamage(damage);
                Destroy(gameObject);
            }
        }
    }
}

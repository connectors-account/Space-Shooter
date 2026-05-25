using UnityEngine;

/// <summary>
/// Controls bullet movement and collision. Used for both player and enemy bullets.
/// Initialized by the shooter with direction, speed, and ownership.
/// </summary>
public class BulletController : MonoBehaviour
{
    [SerializeField] private int damage = 1;
    [SerializeField] private float lifetime = 5f;

    private Vector2 direction;
    private float speed;
    private bool isPlayerBullet;
    private float spawnTime;

    /// <summary>
    /// Called by the shooter to set bullet parameters.
    /// </summary>
    public void Initialize(Vector2 dir, float spd, bool playerBullet)
    {
        direction = dir.normalized;
        speed = spd;
        isPlayerBullet = playerBullet;
        spawnTime = Time.time;

        // Set tag for collision filtering
        gameObject.tag = playerBullet ? "PlayerBullet" : "EnemyBullet";

        // Rotate to face movement direction
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        // Set color based on ownership
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = playerBullet ? new Color(0.3f, 1f, 0.3f) : new Color(1f, 0.3f, 0.3f);
        }
    }

    private void Update()
    {
        // Move bullet
        transform.Translate(direction * speed * Time.deltaTime, Space.World);

        // Destroy after lifetime
        if (Time.time - spawnTime > lifetime)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isPlayerBullet)
        {
            // Player bullet hits enemy
            if (other.CompareTag("Enemy"))
            {
                EnemyController enemy = other.GetComponent<EnemyController>();
                if (enemy != null)
                {
                    enemy.TakeDamage(damage);
                }
                Destroy(gameObject);
            }
        }
        else
        {
            // Enemy bullet hits player
            if (other.CompareTag("Player"))
            {
                PlayerController player = other.GetComponent<PlayerController>();
                if (player != null)
                {
                    player.TakeDamage(damage);
                }
                Destroy(gameObject);
            }
        }
    }
}

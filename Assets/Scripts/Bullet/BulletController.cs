using UnityEngine;

/// <summary>
/// Controls bullet movement and collision. Used for both player and enemy bullets.
/// Attach to Bullet prefab GameObjects.
/// </summary>
public class BulletController : MonoBehaviour
{
    [SerializeField] private float defaultSpeed = 10f;
    [SerializeField] private int defaultDamage = 1;
    [SerializeField] private float lifetime = 5f;

    private Vector2 direction;
    private float speed;
    private int damage;
    private bool isPlayerBullet;
    private bool initialized = false;

    /// <summary>
    /// Initialize the bullet with direction, speed, damage, and owner.
    /// </summary>
    public void Initialize(Vector2 dir, float spd, int dmg, bool fromPlayer)
    {
        direction = dir.normalized;
        speed = spd;
        damage = dmg;
        isPlayerBullet = fromPlayer;
        initialized = true;

        // Set tag for collision detection
        gameObject.tag = fromPlayer ? "PlayerBullet" : "EnemyBullet";

        // Rotate bullet to face movement direction
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        // Auto-destroy after lifetime
        Destroy(gameObject, lifetime);
    }

    private void Start()
    {
        if (!initialized)
        {
            // Default: move upward (player bullet)
            direction = Vector2.up;
            speed = defaultSpeed;
            damage = defaultDamage;
            isPlayerBullet = true;
            Destroy(gameObject, lifetime);
        }
    }

    private void Update()
    {
        transform.position += (Vector3)(direction * speed * Time.deltaTime);
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

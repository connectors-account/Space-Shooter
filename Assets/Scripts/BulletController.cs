// =============================================================================
// BulletController.cs
// Controls bullet movement, lifetime, and damage dealing.
// Used for both player and enemy bullets. The `isPlayerBullet` flag
// determines which objects this bullet can damage.
// Attach this script to bullet prefabs.
// =============================================================================
using UnityEngine;

public class BulletController : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Bullet Settings
    // -------------------------------------------------------------------------
    [Header("Bullet Properties")]
    [Tooltip("Speed of the bullet in units per second.")]
    public float speed = 12f;

    [Tooltip("Damage dealt by this bullet on impact.")]
    public int damage = 1;

    [Tooltip("If true, this bullet damages enemies. If false, it damages the player.")]
    public bool isPlayerBullet = true;

    [Tooltip("Time in seconds before the bullet auto-destroys (prevents leaks).")]
    public float lifetime = 5f;

    // -------------------------------------------------------------------------
    // Internal State
    // -------------------------------------------------------------------------
    private Vector2 direction = Vector2.up; // Default: player bullets go up
    private float lifetimeTimer;

    // -------------------------------------------------------------------------
    // Unity Lifecycle
    // -------------------------------------------------------------------------

    /// <summary>
    /// Initialize lifetime countdown.
    /// </summary>
    void Start()
    {
        lifetimeTimer = lifetime;

        // If this is an enemy bullet and direction hasn't been set, default to down
        if (!isPlayerBullet && direction == Vector2.up)
        {
            direction = Vector2.down;
        }
    }

    /// <summary>
    /// Move the bullet and handle lifetime expiration.
    /// </summary>
    void Update()
    {
        // Move in the assigned direction
        transform.position += (Vector3)direction * speed * Time.deltaTime;

        // Countdown lifetime and destroy when expired
        lifetimeTimer -= Time.deltaTime;
        if (lifetimeTimer <= 0f)
        {
            Destroy(gameObject);
        }
    }

    // -------------------------------------------------------------------------
    // Configuration
    // -------------------------------------------------------------------------

    /// <summary>
    /// Set the movement direction of this bullet.
    /// Call this right after instantiation if needed.
    /// </summary>
    /// <param name="newDirection">Normalized direction vector.</param>
    public void SetDirection(Vector2 newDirection)
    {
        direction = newDirection.normalized;

        // Optionally rotate the bullet sprite to face the movement direction
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle - 90f);
    }

    // -------------------------------------------------------------------------
    // Collision Handling
    // -------------------------------------------------------------------------

    /// <summary>
    /// Handle collision with other game objects.
    /// Player bullets damage enemies; enemy bullets damage the player.
    /// </summary>
    void OnTriggerEnter2D(Collider2D other)
    {
        if (isPlayerBullet)
        {
            // Player bullet hit an enemy
            if (other.CompareTag("Enemy"))
            {
                EnemyController enemy = other.GetComponent<EnemyController>();
                if (enemy != null)
                {
                    enemy.TakeDamage(damage);
                }
                Destroy(gameObject); // Destroy the bullet on impact
            }
        }
        else
        {
            // Enemy bullet hit the player
            if (other.CompareTag("Player"))
            {
                PlayerController player = other.GetComponent<PlayerController>();
                if (player != null)
                {
                    player.TakeDamage(damage);
                }
                Destroy(gameObject); // Destroy the bullet on impact
            }
        }
    }
}

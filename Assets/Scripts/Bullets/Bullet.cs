// ============================================================================
// Bullet.cs — Universal projectile for player and enemy bullets
// ============================================================================
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Bullet : MonoBehaviour
{
    [Header("Defaults (overridden by Initialize)")]
    [SerializeField] private float defaultSpeed = 10f;
    [SerializeField] private int damage = 1;
    [SerializeField] private bool isPlayerBullet = true;
    [SerializeField] private float lifetime = 5f;

    [Header("Visual")]
    [SerializeField] private TrailRenderer trail;

    private Rigidbody2D rb;
    private Vector2 direction;
    private float speed;
    private bool initialized;

    // Properties
    public int Damage => damage;
    public bool IsPlayerBullet => isPlayerBullet;

    // =========================================================================
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    private void Start()
    {
        if (!initialized)
        {
            // Use defaults if Initialize wasn't called
            rb.linearVelocity = Vector2.up * defaultSpeed;
        }

        // Auto-destroy after lifetime
        Destroy(gameObject, lifetime);

        // Set tag based on ownership
        gameObject.tag = isPlayerBullet ? "PlayerBullet" : "EnemyBullet";
    }

    // =========================================================================
    // Initialization (called by shooter scripts)
    // =========================================================================
    public void Initialize(Vector2 dir, float spd, bool playerOwned, int dmg = 1)
    {
        direction = dir.normalized;
        speed = spd;
        isPlayerBullet = playerOwned;
        damage = dmg;
        initialized = true;

        if (rb == null) rb = GetComponent<Rigidbody2D>();
        rb.linearVelocity = direction * speed;

        // Rotate to face movement direction
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        gameObject.tag = isPlayerBullet ? "PlayerBullet" : "EnemyBullet";
    }

    // =========================================================================
    // Screen Bounds Check
    // =========================================================================
    private void Update()
    {
        // Destroy if off-screen
        Vector3 pos = transform.position;
        if (pos.y > 7f || pos.y < -7f || pos.x > 6f || pos.x < -6f)
        {
            Destroy(gameObject);
        }
    }

    // =========================================================================
    // Collision
    // =========================================================================
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isPlayerBullet)
        {
            if (other.CompareTag("Enemy"))
            {
                EnemyBase enemy = other.GetComponent<EnemyBase>();
                if (enemy != null)
                {
                    enemy.TakeDamage(damage);
                }
                Destroy(gameObject);
            }
        }
        else // Enemy bullet
        {
            if (other.CompareTag("Player"))
            {
                // Damage handled by PlayerHealth.OnTriggerEnter2D
                Destroy(gameObject);
            }
        }
    }
}

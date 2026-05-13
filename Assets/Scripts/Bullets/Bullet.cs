// ============================================================================
// Bullet.cs - Universal bullet behaviour for both player and enemy projectiles
// ============================================================================
using UnityEngine;

/// <summary>
/// A bullet that travels in a given direction at a set speed.
/// Tagged as either "PlayerBullet" or "EnemyBullet" on initialization.
/// Handles its own out-of-bounds cleanup and pool return.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class Bullet : MonoBehaviour
{
    [Header("Bullet Properties")]
    [Tooltip("Travel speed in world units per second.")]
    [SerializeField] private float speed = 12f;
    [Tooltip("Damage dealt on hit.")]
    [SerializeField] private int damage = 10;
    [Tooltip("Lifetime in seconds before auto-despawn.")]
    [SerializeField] private float lifetime = 5f;

    // ---- Runtime ----
    private Vector2 direction;
    private bool isPlayerBullet;
    private float aliveTimer;
    private Rigidbody2D rb;

    /// <summary>Damage this bullet deals to whatever it hits.</summary>
    public int Damage => damage;
    /// <summary>Whether this is a player-fired bullet.</summary>
    public bool IsPlayerBullet => isPlayerBullet;

    // ========================================================================
    // Unity Lifecycle
    // ========================================================================

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        // Ensure trigger collider.
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    private void OnEnable()
    {
        aliveTimer = lifetime;
    }

    private void Update()
    {
        aliveTimer -= Time.deltaTime;
        if (aliveTimer <= 0f)
        {
            ReturnToPool();
            return;
        }

        // Out-of-bounds check.
        if (GameBounds.Instance != null && GameBounds.Instance.IsOutOfBounds(transform.position))
        {
            ReturnToPool();
        }
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = direction * speed;
    }

    // ========================================================================
    // Initialization
    // ========================================================================

    /// <summary>
    /// Sets the bullet's travel direction and ownership. Must be called right
    /// after spawning (by PlayerShooting or EnemyBase.Shoot).
    /// </summary>
    /// <param name="dir">Normalized movement direction.</param>
    /// <param name="fromPlayer">True if fired by the player.</param>
    public void Initialize(Vector2 dir, bool fromPlayer)
    {
        direction = dir.normalized;
        isPlayerBullet = fromPlayer;

        // Set tag for collision filtering.
        gameObject.tag = fromPlayer ? "PlayerBullet" : "EnemyBullet";

        // Set layer.
        gameObject.layer = LayerMask.NameToLayer(fromPlayer ? "PlayerBullet" : "EnemyBullet");

        // Rotate sprite to face the direction of travel.
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle - 90f);

        aliveTimer = lifetime;
    }

    /// <summary>
    /// Overload that also sets custom speed and damage.
    /// </summary>
    public void Initialize(Vector2 dir, bool fromPlayer, float customSpeed, int customDamage)
    {
        speed = customSpeed;
        damage = customDamage;
        Initialize(dir, fromPlayer);
    }

    // ========================================================================
    // Collision
    // ========================================================================

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Player bullets hitting enemies are handled by EnemyBase.OnTriggerEnter2D.
        // Enemy bullets hitting the player:
        if (!isPlayerBullet && other.CompareTag("Player"))
        {
            PlayerHealth ph = other.GetComponent<PlayerHealth>();
            if (ph != null)
            {
                ph.TakeDamage(damage);
            }
            ReturnToPool();
        }
    }

    // ========================================================================
    // Pool Return
    // ========================================================================

    /// <summary>
    /// Returns this bullet to the appropriate object pool or destroys it.
    /// </summary>
    public void ReturnToPool()
    {
        rb.linearVelocity = Vector2.zero;

        string poolName = isPlayerBullet ? "PlayerBulletPool" : "EnemyBulletPool";
        if (PoolManager.Instance != null)
        {
            PoolManager.Instance.Return(poolName, gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}

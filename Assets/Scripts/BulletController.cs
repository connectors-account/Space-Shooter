using UnityEngine;

/// <summary>
/// Controls projectile physics and lifetime.
/// Works for both player and enemy bullets — call Initialize(isPlayerBullet)
/// after instantiation.
/// Requires: Rigidbody2D (set to Kinematic), Collider2D (trigger).
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class BulletController : MonoBehaviour
{
    [Header("Bullet Settings")]
    [SerializeField] private float speed = 15f;
    [SerializeField] private float lifetime = 4f;
    [SerializeField] private int damage = 25;

    [Header("Visual")]
    [SerializeField] private Color playerBulletColor = Color.cyan;
    [SerializeField] private Color enemyBulletColor = Color.red;

    private bool _isPlayerBullet;
    private Rigidbody2D _rb;

    // ────────────────────────────────────────────────────────────────────
    // Initialization (called by the spawner after Instantiate)
    // ────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Must be called immediately after Instantiate.
    /// Sets direction, tag, and color.
    /// </summary>
    public void Initialize(bool isPlayerBullet, float speedOverride = -1f)
    {
        _isPlayerBullet = isPlayerBullet;

        if (speedOverride > 0f)
            speed = speedOverride;

        // Tag so collision handlers can differentiate
        gameObject.tag = _isPlayerBullet ? "PlayerBullet" : "EnemyBullet";

        // Tint
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.color = _isPlayerBullet ? playerBulletColor : enemyBulletColor;

        // Set layer for physics filtering
        gameObject.layer = LayerMask.NameToLayer(
            _isPlayerBullet ? "PlayerBullet" : "EnemyBullet");
    }

    // ────────────────────────────────────────────────────────────────────
    // Unity Lifecycle
    // ────────────────────────────────────────────────────────────────────
    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _rb.gravityScale = 0f;
    }

    private void Start()
    {
        // Direction is "up" relative to the bullet's rotation
        _rb.linearVelocity = transform.up * speed;

        // Auto-destroy after lifetime
        Destroy(gameObject, lifetime);
    }

    // ────────────────────────────────────────────────────────────────────
    // Collision
    // ────────────────────────────────────────────────────────────────────
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_isPlayerBullet)
        {
            // Player bullet hits enemy
            if (other.CompareTag("Enemy"))
            {
                HealthSystem enemyHealth = other.GetComponent<HealthSystem>();
                if (enemyHealth != null)
                    enemyHealth.TakeDamage(damage);
                Destroy(gameObject);
            }
        }
        else
        {
            // Enemy bullet hits player — handled in PlayerController
            // Just destroy self on contact with player
            if (other.CompareTag("Player"))
            {
                Destroy(gameObject);
            }
        }
    }

    // ────────────────────────────────────────────────────────────────────
    // Properties
    // ────────────────────────────────────────────────────────────────────
    public int Damage => damage;
    public bool IsPlayerBullet => _isPlayerBullet;
}

using UnityEngine;

/// <summary>
/// BulletController - Moves the bullet in a given direction and handles collision.
/// Attach to bullet prefabs with Rigidbody2D and CircleCollider2D (trigger).
/// Tag player bullets as "PlayerBullet" and enemy bullets as "EnemyBullet".
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
public class BulletController : MonoBehaviour
{
    [Header("Bullet Settings")]
    public float speed = 10f;
    public int damage = 1;
    public bool isPlayerBullet = true;
    public float lifetime = 5f;

    private Rigidbody2D rb;
    private Vector2 direction;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        CircleCollider2D col = GetComponent<CircleCollider2D>();
        col.isTrigger = true;
    }

    /// <summary>
    /// Call this immediately after Instantiate to set direction, speed, and ownership.
    /// </summary>
    public void Initialize(Vector2 dir, float spd, bool playerBullet, int dmg = 1)
    {
        direction = dir.normalized;
        speed = spd;
        isPlayerBullet = playerBullet;
        damage = dmg;

        // Set tag based on ownership
        gameObject.tag = isPlayerBullet ? "PlayerBullet" : "EnemyBullet";

        // Rotate bullet to face movement direction
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    private void Start()
    {
        // Self-destruct after lifetime to prevent orphan bullets
        Destroy(gameObject, lifetime);
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = direction * speed;
    }

    /// <summary>
    /// Destroy bullet if it goes far off-screen.
    /// </summary>
    private void Update()
    {
        if (Mathf.Abs(transform.position.x) > 20f || Mathf.Abs(transform.position.y) > 15f)
        {
            Destroy(gameObject);
        }
    }
}

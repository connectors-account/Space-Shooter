using UnityEngine;

/// <summary>
/// Controls bullet movement and auto-destruction.
/// Bullets are initialized with direction and speed by their spawner.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class BulletController : MonoBehaviour
{
    [Header("Bullet Settings")]
    [SerializeField] private int damage = 25;
    [SerializeField] private float lifetime = 5f;

    private Rigidbody2D rb;
    private bool isPlayerBullet = true;
    private float timer;

    public int Damage => damage;
    public bool IsPlayerBullet => isPlayerBullet;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    /// <summary>
    /// Initialize the bullet with a direction, speed, and ownership flag.
    /// </summary>
    public void Initialize(Vector2 direction, float speed, bool playerBullet)
    {
        isPlayerBullet = playerBullet;
        rb.linearVelocity = direction.normalized * speed;
        timer = lifetime;

        // Rotate bullet to face movement direction
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        // Set layer for collision filtering
        gameObject.layer = LayerMask.NameToLayer(playerBullet ? "PlayerBullet" : "EnemyBullet");
    }

    /// <summary>
    /// Overload for setting custom damage.
    /// </summary>
    public void Initialize(Vector2 direction, float speed, bool playerBullet, int customDamage)
    {
        Initialize(direction, speed, playerBullet);
        damage = customDamage;
    }

    private void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            Destroy(gameObject);
            return;
        }

        // Destroy if out of screen bounds (with margin)
        Vector3 pos = transform.position;
        if (Mathf.Abs(pos.x) > 12f || Mathf.Abs(pos.y) > 8f)
        {
            Destroy(gameObject);
        }
    }
}

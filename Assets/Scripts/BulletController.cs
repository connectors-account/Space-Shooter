using UnityEngine;

/// <summary>
/// BulletController handles bullet movement, lifetime, and damage.
/// Used by both player and enemy bullets.
/// Tag the bullet's GameObject as "PlayerBullet" or "EnemyBullet" at runtime.
/// </summary>
public class BulletController : MonoBehaviour
{
    // ── Configuration ────────────────────────────────────────
    [Header("Bullet Settings")]
    [SerializeField] private int damage = 1;
    [SerializeField] private float lifetime = 5f;

    // ── Internal ─────────────────────────────────────────────
    private Vector2 direction;
    private float speed;
    private bool isPlayerBullet;
    private float spawnTime;

    // ── Public Properties ────────────────────────────────────
    public int Damage => damage;
    public bool IsPlayerBullet => isPlayerBullet;

    // ──────────────────────────────────────────────────────────
    // Initialization (called by PlayerController / EnemyController)
    // ──────────────────────────────────────────────────────────

    /// <summary>
    /// Set up the bullet's direction, speed, and ownership.
    /// Also sets the tag so collision handlers can distinguish bullets.
    /// </summary>
    public void Initialize(Vector2 dir, float spd, bool playerBullet)
    {
        direction = dir.normalized;
        speed = spd;
        isPlayerBullet = playerBullet;
        spawnTime = Time.time;

        // Set appropriate tag
        gameObject.tag = playerBullet ? "PlayerBullet" : "EnemyBullet";

        // Set the layer for collision filtering (optional, layers set in project)
        gameObject.layer = playerBullet
            ? LayerMask.NameToLayer("PlayerBullet")
            : LayerMask.NameToLayer("EnemyBullet");

        // Rotate sprite to face the direction of travel
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        // Tint: green for player, red for enemy
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = playerBullet ? Color.green : Color.red;
        }
    }

    // ──────────────────────────────────────────────────────────
    // Update – move each frame
    // ──────────────────────────────────────────────────────────

    private void Update()
    {
        transform.Translate(direction * speed * Time.deltaTime, Space.World);

        // Destroy after lifetime expires
        if (Time.time - spawnTime > lifetime)
        {
            Destroy(gameObject);
        }

        // Also destroy if way off-screen
        if (Mathf.Abs(transform.position.x) > 12f || Mathf.Abs(transform.position.y) > 10f)
        {
            Destroy(gameObject);
        }
    }
}

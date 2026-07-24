// ============================================================
//  Bullet.cs  –  Handles both player & enemy bullets
//  Call Init() after Instantiate; call Despawn() to recycle.
// ============================================================
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class Bullet : MonoBehaviour
{
    [Header("Defaults (overridden by Init)")]
    public int   damage = 1;
    public float speed  = 14f;
    public float maxLifetime = 5f;

    Rigidbody2D _rb;
    float       _lifeTimer;
    bool        _isPlayer;

    // ── Unity lifecycle ──────────────────────────────────────

    void Awake() => _rb = GetComponent<Rigidbody2D>();

    void OnEnable()
    {
        _lifeTimer = 0f;
        if (_rb) _rb.linearVelocity = Vector2.zero;
    }

    void Update()
    {
        _lifeTimer += Time.deltaTime;
        if (_lifeTimer >= maxLifetime)
            Despawn();
    }

    // ── Public API ───────────────────────────────────────────

    /// <summary>
    /// Initialise the bullet direction, speed, damage, and owner.
    /// Must be called right after Instantiate.
    /// </summary>
    public void Init(Vector2 direction, float spd, int dmg, bool isPlayer)
    {
        _isPlayer  = isPlayer;
        damage     = dmg;
        speed      = spd;
        _lifeTimer = 0f;

        // Tag so colliders know who owns this bullet
        gameObject.tag = isPlayer ? "PlayerBullet" : "EnemyBullet";

        // Tint
        var sr = GetComponent<SpriteRenderer>();
        if (sr) sr.color = isPlayer ? new Color(0.4f, 0.8f, 1f) : new Color(1f, 0.35f, 0.2f);

        if (_rb)
        {
            _rb.gravityScale = 0f;
            _rb.linearVelocity = direction.normalized * speed;
        }

        // Rotate sprite to face direction
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    /// <summary>Return to pool (or destroy if not pooled).</summary>
    public void Despawn()
    {
        // Try pool first
        string tag = gameObject.tag;
        if (ObjectPool.Instance != null)
            ObjectPool.Instance.Despawn(tag, gameObject);
        else
            Destroy(gameObject);
    }

    // ── Collision guard ──────────────────────────────────────
    // Primary collision logic lives in PlayerHealth / EnemyBase.
    // This handles off-screen cleanup only.

    void OnBecameInvisible() => Despawn();
}

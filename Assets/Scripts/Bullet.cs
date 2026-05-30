using UnityEngine;

/// <summary>
/// Generic bullet — works for both player and enemy projectiles.
/// Attach to bullet prefab (small Sprite + Rigidbody2D kinematic + BoxCollider2D isTrigger).
/// Tag: "PlayerBullet" or "EnemyBullet" — set by Init().
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class Bullet : MonoBehaviour
{
    [Header("Defaults (overridden by Init)")]
    public int damage = 10;
    public float lifetime = 5f;

    Vector2 direction;
    float speed;

    void Awake()
    {
        var rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic;
    }

    /// <summary>
    /// Called right after Instantiate to configure the bullet.
    /// </summary>
    public void Init(Vector2 dir, float spd, bool isPlayer)
    {
        direction = dir.normalized;
        speed = spd;
        gameObject.tag = isPlayer ? "PlayerBullet" : "EnemyBullet";

        // Color hint
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.color = isPlayer ? Color.cyan : Color.red;

        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        transform.Translate(direction * speed * Time.deltaTime, Space.World);
    }
}

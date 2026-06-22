using UnityEngine;

/// <summary>
/// Projectile fired by the player or enemies. Moves in a straight line and
/// deals damage on contact. Bullets are recycled through BulletPool rather
/// than being destroyed, so OnDisable/ReturnToPool is used instead of Destroy.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class Bullet : MonoBehaviour
{
    [Header("Bullet Settings")]
    [Tooltip("Travel speed in units/second.")]
    public float speed = 12f;

    [Tooltip("Damage dealt to whatever it hits.")]
    public int damage = 25;

    [Tooltip("Seconds before the bullet auto-returns to the pool if it hits nothing.")]
    public float lifeTime = 3f;

    [Tooltip("Direction of travel. Up (0,1) for player, down (0,-1) for enemies.")]
    public Vector2 direction = Vector2.up;

    // Tag of objects this bullet is allowed to damage ("Enemy" or "Player").
    private string targetTag;
    // Reference back to the pool so we can recycle ourselves.
    private BulletPool ownerPool;
    private float spawnTime;

    /// <summary>
    /// Configure a freshly spawned bullet. Called by whoever fires it.
    /// </summary>
    public void Initialize(Vector2 dir, string target, int dmg, BulletPool pool)
    {
        direction = dir.normalized;
        targetTag = target;
        damage = dmg;
        ownerPool = pool;
        spawnTime = Time.time;
    }

    private void Update()
    {
        // Move along the configured direction every frame.
        transform.position += (Vector3)(direction * speed * Time.deltaTime);

        // Recycle after its lifetime expires (covers bullets that miss).
        if (Time.time - spawnTime >= lifeTime)
            ReturnToPool();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Only damage the intended target type.
        if (!other.CompareTag(targetTag))
            return;

        var health = other.GetComponent<HealthSystem>();
        if (health != null)
            health.TakeDamage(damage);

        // Bullet is consumed on hit.
        ReturnToPool();
    }

    /// <summary>Return this bullet to its pool, or destroy it if no pool is set.</summary>
    private void ReturnToPool()
    {
        if (ownerPool != null)
            ownerPool.ReturnBullet(this);
        else
            gameObject.SetActive(false);
    }
}

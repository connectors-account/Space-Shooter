using UnityEngine;

/// <summary>
/// Bullet behavior for both player and enemy bullets.
/// Uses object pooling for performance. Handles movement, damage, and auto-despawn.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class Bullet : MonoBehaviour, IPoolable
{
    [Header("Bullet Settings")]
    [SerializeField] private float speed = 12f;
    [SerializeField] private int damage = 1;
    [SerializeField] private float lifetime = 5f;

    private Rigidbody2D rb;
    private Vector2 direction;
    private bool isPlayerBullet;
    private float spawnTime;
    private string myPoolTag;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
    }

    /// <summary>
    /// Initialize bullet direction and ownership after spawning from pool.
    /// </summary>
    public void Initialize(Vector2 dir, bool playerBullet, float customSpeed = 0f, int customDamage = 0)
    {
        direction = dir.normalized;
        isPlayerBullet = playerBullet;
        myPoolTag = playerBullet ? Tags.PlayerBullet : Tags.EnemyBullet;

        if (customSpeed > 0f) speed = customSpeed;
        if (customDamage > 0) damage = customDamage;

        // Set rotation to face direction
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        rb.velocity = direction * speed;
    }

    public void OnSpawnFromPool()
    {
        spawnTime = Time.time;
    }

    public void OnReturnToPool()
    {
        rb.velocity = Vector2.zero;
    }

    private void Update()
    {
        // Auto-despawn after lifetime
        if (Time.time - spawnTime > lifetime)
        {
            ReturnToPool();
            return;
        }

        // Check bounds
        if (GameManager.Instance != null && !GameManager.Instance.IsInBounds(transform.position, 1f))
        {
            ReturnToPool();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isPlayerBullet)
        {
            // Player bullet hits enemy
            EnemyBase enemy = other.GetComponent<EnemyBase>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
                WaveManager.Instance?.OnEnemyDestroyed();
                SpawnHitEffect();
                ReturnToPool();
            }
        }
        else
        {
            // Enemy bullet hits player
            if (other.CompareTag(Tags.Player))
            {
                PlayerHealth ph = other.GetComponent<PlayerHealth>();
                if (ph != null)
                {
                    ph.TakeDamage(damage);
                    SpawnHitEffect();
                    ReturnToPool();
                }
            }
        }
    }

    private void SpawnHitEffect()
    {
        // Small explosion/hit effect
        if (ObjectPool.Instance != null)
        {
            ObjectPool.Instance.Spawn(Tags.Explosion, transform.position, Quaternion.identity);
        }
    }

    private void ReturnToPool()
    {
        if (ObjectPool.Instance != null)
        {
            ObjectPool.Instance.Despawn(myPoolTag, gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}

using UnityEngine;

public class Bullet : MonoBehaviour, IPooledObject
{
    [Header("Bullet Settings")]
    public float speed = 10f;
    public int damage = 1;
    public bool isPlayerBullet = true;
    public float lifetime = 5f;

    [Header("Visual")]
    public TrailRenderer trailRenderer;

    private Vector2 direction = Vector2.up;
    private Rigidbody2D rb;
    private float spawnTime;
    private bool isActive = true;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void OnObjectSpawn()
    {
        isActive = true;
        spawnTime = Time.time;
        direction = isPlayerBullet ? Vector2.up : Vector2.down;

        if (trailRenderer != null)
        {
            trailRenderer.Clear();
        }

        if (rb != null)
        {
            rb.velocity = direction * speed;
        }
    }

    private void Start()
    {
        if (spawnTime == 0)
        {
            OnObjectSpawn();
        }
    }

    private void Update()
    {
        if (!isActive) return;

        // Move bullet if no rigidbody
        if (rb == null)
        {
            transform.Translate(direction * speed * Time.deltaTime, Space.World);
        }

        // Check lifetime
        if (Time.time - spawnTime > lifetime)
        {
            Deactivate();
        }

        // Check bounds
        if (IsOutOfBounds())
        {
            Deactivate();
        }
    }

    public void SetDirection(Vector2 newDirection, float newSpeed = -1f)
    {
        direction = newDirection.normalized;
        if (newSpeed > 0)
        {
            speed = newSpeed;
        }

        if (rb != null)
        {
            rb.velocity = direction * speed;
        }

        // Rotate bullet to face direction
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        HandleCollision(other.gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandleCollision(collision.gameObject);
    }

    private void HandleCollision(GameObject other)
    {
        if (!isActive) return;

        if (isPlayerBullet)
        {
            // Player bullet hits enemy
            if (other.CompareTag("Enemy"))
            {
                HealthSystem enemyHealth = other.GetComponent<HealthSystem>();
                if (enemyHealth != null)
                {
                    enemyHealth.TakeDamage(damage);
                }
                Deactivate();
            }
        }
        else
        {
            // Enemy bullet hits player
            if (other.CompareTag("Player"))
            {
                PlayerController player = other.GetComponent<PlayerController>();
                if (player != null && !player.IsInvincible())
                {
                    HealthSystem playerHealth = other.GetComponent<HealthSystem>();
                    if (playerHealth != null)
                    {
                        playerHealth.TakeDamage(damage);
                    }
                }
                Deactivate();
            }
        }
    }

    private bool IsOutOfBounds()
    {
        return transform.position.y > 7f ||
               transform.position.y < -7f ||
               transform.position.x > 10f ||
               transform.position.x < -10f;
    }

    private void Deactivate()
    {
        isActive = false;
        string poolTag = isPlayerBullet ? "PlayerBullet" : "EnemyBullet";

        if (ObjectPooler.Instance != null)
        {
            ObjectPooler.Instance.ReturnToPool(poolTag, gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}

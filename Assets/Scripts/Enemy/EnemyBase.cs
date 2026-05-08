using UnityEngine;

/// <summary>
/// Base class for all enemy types. Handles health, scoring, movement patterns,
/// and dropping power-ups on death.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class EnemyBase : MonoBehaviour, IPoolable
{
    [Header("Enemy Stats")]
    [SerializeField] protected int maxHealth = 1;
    [SerializeField] protected float moveSpeed = 2f;
    [SerializeField] protected int scoreValue = 100;
    [SerializeField] protected float powerUpDropChance = 0.1f;

    [Header("Shooting")]
    [SerializeField] protected float fireRate = 1f;
    [SerializeField] protected bool canShoot = true;

    [Header("Pool")]
    [SerializeField] private string poolTag = Tags.EnemyBasic;

    protected int currentHealth;
    protected float fireTimer;
    protected Rigidbody2D rb;
    protected Transform playerTransform;
    protected EnemyMovementPattern movementPattern = EnemyMovementPattern.StraightDown;

    // Movement pattern parameters
    protected float sinAmplitude = 2f;
    protected float sinFrequency = 2f;
    protected float spawnTime;
    protected Vector2 startPosition;

    public string PoolTag => poolTag;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
    }

    public virtual void OnSpawnFromPool()
    {
        currentHealth = maxHealth;
        fireTimer = Random.Range(0.5f, fireRate); // Stagger initial shots
        spawnTime = Time.time;
        startPosition = transform.position;

        // Try to find player
        GameObject player = GameObject.FindGameObjectWithTag(Tags.Player);
        playerTransform = player != null ? player.transform : null;
    }

    public virtual void OnReturnToPool()
    {
        rb.velocity = Vector2.zero;
    }

    protected virtual void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Playing) return;

        UpdateMovement();
        UpdateShooting();
        CheckOutOfBounds();
    }

    protected virtual void UpdateMovement()
    {
        float elapsed = Time.time - spawnTime;

        switch (movementPattern)
        {
            case EnemyMovementPattern.StraightDown:
                rb.velocity = Vector2.down * moveSpeed;
                break;

            case EnemyMovementPattern.Zigzag:
                float xVel = Mathf.Sin(elapsed * sinFrequency) * sinAmplitude;
                rb.velocity = new Vector2(xVel, -moveSpeed);
                break;

            case EnemyMovementPattern.Sine:
                float sineX = Mathf.Sin(elapsed * sinFrequency) * sinAmplitude;
                Vector2 targetPos = new Vector2(startPosition.x + sineX, transform.position.y - moveSpeed * Time.deltaTime);
                rb.MovePosition(targetPos);
                break;

            case EnemyMovementPattern.DiagonalLeft:
                rb.velocity = new Vector2(-moveSpeed * 0.5f, -moveSpeed);
                break;

            case EnemyMovementPattern.DiagonalRight:
                rb.velocity = new Vector2(moveSpeed * 0.5f, -moveSpeed);
                break;

            case EnemyMovementPattern.TrackPlayer:
                if (playerTransform != null && playerTransform.gameObject.activeSelf)
                {
                    float dir = Mathf.Sign(playerTransform.position.x - transform.position.x);
                    rb.velocity = new Vector2(dir * moveSpeed * 0.3f, -moveSpeed * 0.5f);
                }
                else
                {
                    rb.velocity = Vector2.down * moveSpeed;
                }
                break;

            case EnemyMovementPattern.Hover:
                // Move to a Y position then hover left/right
                if (transform.position.y > 2f)
                {
                    rb.velocity = Vector2.down * moveSpeed;
                }
                else
                {
                    float hoverX = Mathf.Sin(elapsed * sinFrequency) * sinAmplitude;
                    rb.velocity = new Vector2(hoverX, 0f);
                }
                break;
        }
    }

    protected virtual void UpdateShooting()
    {
        if (!canShoot) return;

        fireTimer -= Time.deltaTime;
        if (fireTimer <= 0f)
        {
            Shoot();
            fireTimer = fireRate;
        }
    }

    protected virtual void Shoot()
    {
        if (ObjectPool.Instance == null) return;

        GameObject bullet = ObjectPool.Instance.Spawn(Tags.EnemyBullet, transform.position + Vector3.down * 0.5f, Quaternion.identity);
        if (bullet != null)
        {
            Bullet b = bullet.GetComponent<Bullet>();
            if (b != null)
            {
                b.Initialize(Vector2.down, false);
            }
        }
    }

    public virtual void TakeDamage(int damage)
    {
        currentHealth -= damage;

        // Flash white on hit
        StartCoroutine(FlashWhite());

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        // Add score
        GameManager.Instance?.AddScore(scoreValue);

        // Spawn explosion
        if (ObjectPool.Instance != null)
        {
            ObjectPool.Instance.Spawn(Tags.Explosion, transform.position, Quaternion.identity);
        }

        // Chance to drop power-up
        if (Random.value < powerUpDropChance)
        {
            PowerUpSpawner.Instance?.SpawnRandomPowerUp(transform.position);
        }

        AudioManager.Instance?.PlaySFX("EnemyDeath");

        // Return to pool
        if (ObjectPool.Instance != null)
        {
            ObjectPool.Instance.Despawn(poolTag, gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    protected void CheckOutOfBounds()
    {
        if (GameManager.Instance != null && !GameManager.Instance.IsInBounds(transform.position, 2f))
        {
            if (ObjectPool.Instance != null)
                ObjectPool.Instance.Despawn(poolTag, gameObject);
            else
                gameObject.SetActive(false);
        }
    }

    private System.Collections.IEnumerator FlashWhite()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr == null) sr = GetComponentInChildren<SpriteRenderer>();
        if (sr != null)
        {
            Color original = sr.color;
            sr.color = Color.white;
            yield return new WaitForSeconds(0.05f);
            sr.color = original;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Collision with player
        if (other.CompareTag(Tags.Player))
        {
            PlayerHealth ph = other.GetComponent<PlayerHealth>();
            if (ph != null)
            {
                ph.TakeDamage(1);
            }
            TakeDamage(maxHealth); // Enemy dies on contact
        }
    }

    /// <summary>
    /// Configure enemy stats (called by WaveManager).
    /// </summary>
    public void Configure(int health, float speed, int score, float dropChance,
                          EnemyMovementPattern pattern, float shootRate = 1f, bool shoots = true)
    {
        maxHealth = health;
        currentHealth = health;
        moveSpeed = speed;
        scoreValue = score;
        powerUpDropChance = dropChance;
        movementPattern = pattern;
        fireRate = shootRate;
        canShoot = shoots;
    }

    public void SetPoolTag(string tag) { poolTag = tag; }
}

/// <summary>
/// Available enemy movement patterns.
/// </summary>
public enum EnemyMovementPattern
{
    StraightDown,
    Zigzag,
    Sine,
    DiagonalLeft,
    DiagonalRight,
    TrackPlayer,
    Hover
}

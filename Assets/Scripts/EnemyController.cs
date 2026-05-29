using UnityEngine;

/// <summary>
/// Controls enemy behavior including movement patterns, shooting, and scoring.
/// Different enemy types use different movement and attack patterns.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(HealthSystem))]
public class EnemyController : MonoBehaviour
{
    public enum EnemyType
    {
        Basic,      // Moves straight down, occasional shots
        Zigzag,     // Moves down in a zigzag pattern
        Tank,       // Slow, high health, frequent shots
        Fast        // Quick movement, no shooting
    }

    [Header("Enemy Config")]
    [SerializeField] private EnemyType enemyType = EnemyType.Basic;
    [SerializeField] private int scoreValue = 100;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float zigzagAmplitude = 3f;
    [SerializeField] private float zigzagFrequency = 2f;

    [Header("Shooting")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float fireRate = 2f;
    [SerializeField] private float bulletSpeed = 6f;

    [Header("Boundaries")]
    [SerializeField] private float destroyY = -7f;

    private Rigidbody2D rb;
    private HealthSystem healthSystem;
    private float nextFireTime;
    private float spawnX;
    private float timeAlive;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        healthSystem = GetComponent<HealthSystem>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        rb.gravityScale = 0f;
        rb.freezeRotation = true;
    }

    private void Start()
    {
        spawnX = transform.position.x;
        timeAlive = 0f;
        nextFireTime = Time.time + Random.Range(0.5f, fireRate);

        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;

        healthSystem.OnDeath += HandleDeath;
        healthSystem.OnDamageTaken += HandleDamageTaken;

        ApplyTypeSettings();
    }

    /// <summary>
    /// Configure this enemy with a specific type and difficulty multiplier.
    /// </summary>
    public void Configure(EnemyType type, float difficultyMultiplier)
    {
        enemyType = type;
        ApplyTypeSettings();

        // Scale stats with difficulty
        moveSpeed *= (1f + difficultyMultiplier * 0.15f);
        fireRate = Mathf.Max(0.5f, fireRate / (1f + difficultyMultiplier * 0.1f));
        scoreValue = Mathf.RoundToInt(scoreValue * (1f + difficultyMultiplier * 0.2f));
    }

    private void ApplyTypeSettings()
    {
        switch (enemyType)
        {
            case EnemyType.Basic:
                moveSpeed = 3f;
                fireRate = 2.5f;
                scoreValue = 100;
                break;
            case EnemyType.Zigzag:
                moveSpeed = 3.5f;
                fireRate = 3f;
                scoreValue = 150;
                break;
            case EnemyType.Tank:
                moveSpeed = 1.5f;
                fireRate = 1.5f;
                scoreValue = 300;
                healthSystem.SetMaxHealth(150);
                healthSystem.FullHeal();
                break;
            case EnemyType.Fast:
                moveSpeed = 7f;
                fireRate = 999f; // Effectively never shoots
                scoreValue = 200;
                break;
        }
    }

    private void Update()
    {
        timeAlive += Time.deltaTime;
        HandleMovement();
        HandleShooting();
        CheckBounds();
    }

    private void HandleMovement()
    {
        Vector2 velocity = Vector2.zero;

        switch (enemyType)
        {
            case EnemyType.Basic:
                velocity = Vector2.down * moveSpeed;
                break;

            case EnemyType.Zigzag:
                float xOffset = Mathf.Sin(timeAlive * zigzagFrequency) * zigzagAmplitude;
                float targetX = spawnX + xOffset;
                float xDiff = targetX - transform.position.x;
                velocity = new Vector2(xDiff * 2f, -moveSpeed);
                break;

            case EnemyType.Tank:
                velocity = Vector2.down * moveSpeed;
                break;

            case EnemyType.Fast:
                float fastXOffset = Mathf.Sin(timeAlive * 3f) * 2f;
                velocity = new Vector2(fastXOffset, -moveSpeed);
                break;
        }

        rb.linearVelocity = velocity;
    }

    private void HandleShooting()
    {
        if (bulletPrefab == null) return;
        if (enemyType == EnemyType.Fast) return;

        if (Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + fireRate + Random.Range(-0.3f, 0.3f);
            FireBullet();
        }
    }

    private void FireBullet()
    {
        Vector3 spawnPos = transform.position + Vector3.down * 0.5f;
        GameObject bullet = Instantiate(bulletPrefab, spawnPos, Quaternion.identity);

        BulletController bc = bullet.GetComponent<BulletController>();
        if (bc != null)
        {
            Vector2 shootDir = Vector2.down;

            // Tank enemies aim at player
            if (enemyType == EnemyType.Tank)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    shootDir = (player.transform.position - transform.position).normalized;
                }
            }

            bc.Initialize(shootDir, bulletSpeed, false, 20);
        }

        AudioManager.Instance?.PlaySFX("EnemyShoot");
    }

    private void CheckBounds()
    {
        if (transform.position.y < destroyY)
        {
            Destroy(gameObject);
        }
    }

    private void HandleDamageTaken()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.white;
            Invoke(nameof(ResetColor), 0.1f);
        }
    }

    private void ResetColor()
    {
        if (spriteRenderer != null)
            spriteRenderer.color = originalColor;
    }

    private void HandleDeath()
    {
        GameManager.Instance?.AddScore(scoreValue);
        AudioManager.Instance?.PlaySFX("EnemyDeath");

        // Chance to drop power-up (15%)
        if (Random.value < 0.15f)
        {
            GameManager.Instance?.SpawnPowerUp(transform.position);
        }
    }

    private void OnDestroy()
    {
        if (healthSystem != null)
        {
            healthSystem.OnDeath -= HandleDeath;
            healthSystem.OnDamageTaken -= HandleDamageTaken;
        }
    }
}

using UnityEngine;
using System;

public enum EnemyType
{
    Basic,      // Moves straight down
    Zigzag,     // Zigzag movement pattern
    Circular,   // Circular/sine wave movement
    Charger,    // Charges at player
    Boss        // Boss enemy with special patterns
}

public class EnemyBase : MonoBehaviour, IPooledObject
{
    [Header("Enemy Settings")]
    public EnemyType enemyType = EnemyType.Basic;
    public float moveSpeed = 3f;
    public int scoreValue = 100;
    public int damage = 1;

    [Header("Shooting Settings")]
    public bool canShoot = true;
    public GameObject bulletPrefab;
    public float fireRate = 2f;
    public float bulletSpeed = 5f;

    [Header("Movement Pattern Settings")]
    public float zigzagAmplitude = 2f;
    public float zigzagFrequency = 2f;
    public float circularRadius = 1f;
    public float circularSpeed = 2f;

    [Header("Audio")]
    public AudioClip shootSound;
    public AudioClip deathSound;

    // State
    protected float nextFireTime;
    protected Vector3 startPosition;
    protected float timeSinceSpawn;
    protected bool isActive = true;

    // Components
    protected Rigidbody2D rb;
    protected HealthSystem healthSystem;
    protected SpriteRenderer spriteRenderer;
    protected AudioSource audioSource;

    // Events
    public static event Action<int> OnEnemyKilled;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        healthSystem = GetComponent<HealthSystem>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    protected virtual void Start()
    {
        startPosition = transform.position;

        if (healthSystem != null)
        {
            healthSystem.OnDeath += HandleDeath;
        }
    }

    public virtual void OnObjectSpawn()
    {
        isActive = true;
        timeSinceSpawn = 0f;
        startPosition = transform.position;
        nextFireTime = Time.time + Random.Range(0.5f, fireRate);

        if (healthSystem != null)
        {
            healthSystem.ResetHealth();
        }
    }

    protected virtual void Update()
    {
        if (!isActive || GameManager.Instance?.CurrentState != GameManager.GameState.Playing)
            return;

        timeSinceSpawn += Time.deltaTime;
        HandleMovement();
        HandleShooting();
        CheckBounds();
    }

    protected virtual void HandleMovement()
    {
        switch (enemyType)
        {
            case EnemyType.Basic:
                BasicMovement();
                break;
            case EnemyType.Zigzag:
                ZigzagMovement();
                break;
            case EnemyType.Circular:
                CircularMovement();
                break;
            case EnemyType.Charger:
                ChargerMovement();
                break;
            case EnemyType.Boss:
                BossMovement();
                break;
        }
    }

    protected void BasicMovement()
    {
        transform.Translate(Vector3.down * moveSpeed * Time.deltaTime);
    }

    protected void ZigzagMovement()
    {
        float xOffset = Mathf.Sin(timeSinceSpawn * zigzagFrequency) * zigzagAmplitude;
        Vector3 movement = new Vector3(xOffset * Time.deltaTime, -moveSpeed * Time.deltaTime, 0);
        transform.Translate(movement);
    }

    protected void CircularMovement()
    {
        float xOffset = Mathf.Sin(timeSinceSpawn * circularSpeed) * circularRadius;
        transform.position = new Vector3(
            startPosition.x + xOffset,
            transform.position.y - moveSpeed * Time.deltaTime,
            0
        );
    }

    protected void ChargerMovement()
    {
        // Move towards player if player exists
        if (PlayerController.Instance != null)
        {
            Vector3 direction = (PlayerController.Instance.transform.position - transform.position).normalized;
            // Bias towards moving down but also towards player
            direction = (direction + Vector3.down * 0.5f).normalized;
            transform.Translate(direction * moveSpeed * Time.deltaTime);
        }
        else
        {
            BasicMovement();
        }
    }

    protected virtual void BossMovement()
    {
        // Move to position at top, then move side to side
        if (transform.position.y > 3f)
        {
            transform.Translate(Vector3.down * moveSpeed * 0.5f * Time.deltaTime);
        }
        else
        {
            float xOffset = Mathf.Sin(timeSinceSpawn * circularSpeed) * 3f;
            transform.position = new Vector3(xOffset, transform.position.y, 0);
        }
    }

    protected virtual void HandleShooting()
    {
        if (!canShoot || bulletPrefab == null)
            return;

        if (Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + fireRate;
        }
    }

    protected virtual void Shoot()
    {
        Vector3 spawnPos = transform.position + Vector3.down * 0.5f;

        GameObject bullet;
        if (ObjectPooler.Instance != null)
        {
            bullet = ObjectPooler.Instance.SpawnFromPool("EnemyBullet", spawnPos, Quaternion.identity);
        }
        else
        {
            bullet = Instantiate(bulletPrefab, spawnPos, Quaternion.identity);
        }

        if (bullet != null)
        {
            Bullet bulletComponent = bullet.GetComponent<Bullet>();
            if (bulletComponent != null)
            {
                bulletComponent.SetDirection(Vector2.down, bulletSpeed);
            }
        }

        if (shootSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(shootSound);
        }
        AudioManager.Instance?.PlaySFX("EnemyShoot");
    }

    protected void CheckBounds()
    {
        if (transform.position.y < -6f || 
            transform.position.x < -10f || 
            transform.position.x > 10f)
        {
            Deactivate();
        }
    }

    protected virtual void HandleDeath()
    {
        OnEnemyKilled?.Invoke(scoreValue);
        ScoreManager.Instance?.AddScore(scoreValue);

        if (deathSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(deathSound);
        }
        AudioManager.Instance?.PlaySFX("EnemyDeath");

        // Spawn power-up chance
        PowerUpSpawner.Instance?.TrySpawnPowerUp(transform.position);

        Deactivate();
    }

    protected void Deactivate()
    {
        isActive = false;
        if (ObjectPooler.Instance != null)
        {
            ObjectPooler.Instance.ReturnToPool(GetPoolTag(), gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    protected string GetPoolTag()
    {
        return enemyType.ToString() + "Enemy";
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandleCollision(collision.gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        HandleCollision(other.gameObject);
    }

    protected virtual void HandleCollision(GameObject other)
    {
        if (other.CompareTag("Player"))
        {
            HealthSystem playerHealth = other.GetComponent<HealthSystem>();
            PlayerController player = other.GetComponent<PlayerController>();

            if (player != null && !player.IsInvincible())
            {
                playerHealth?.TakeDamage(damage);
            }
        }
    }

    private void OnDestroy()
    {
        if (healthSystem != null)
        {
            healthSystem.OnDeath -= HandleDeath;
        }
    }
}

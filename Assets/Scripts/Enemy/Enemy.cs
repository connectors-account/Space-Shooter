using UnityEngine;
using System;

/// <summary>
/// Base Enemy class that handles health, damage, and basic behavior.
/// Can be extended for different enemy types.
/// </summary>
public class Enemy : MonoBehaviour
{
    [Header("Enemy Settings")]
    [SerializeField] protected int maxHealth = 1;
    [SerializeField] protected float moveSpeed = 3f;
    [SerializeField] protected int scoreValue = 100;
    [SerializeField] protected int contactDamage = 1;

    [Header("Shooting Settings")]
    [SerializeField] protected bool canShoot = false;
    [SerializeField] protected float fireRate = 2f;
    [SerializeField] protected string bulletPoolTag = "EnemyBullet";

    [Header("Movement Pattern")]
    [SerializeField] protected EnemyMovementPattern movementPattern = EnemyMovementPattern.Straight;
    [SerializeField] protected float sineAmplitude = 2f;
    [SerializeField] protected float sineFrequency = 2f;

    [Header("Audio")]
    [SerializeField] protected string deathSoundName = "EnemyDeath";
    [SerializeField] protected string shootSoundName = "EnemyShoot";

    [Header("Drop Settings")]
    [SerializeField] protected float powerUpDropChance = 0.1f;

    // Events
    public static event Action<int> OnEnemyKilled; // score value

    // Protected variables
    protected int currentHealth;
    protected float nextFireTime;
    protected float startX;
    protected float moveTimer;
    protected bool isInitialized;

    public enum EnemyMovementPattern
    {
        Straight,       // Move straight down
        Sine,           // Sine wave movement
        Diagonal,       // Move diagonally
        ZigZag,         // ZigZag pattern
        Tracking        // Track player position
    }

    protected virtual void OnEnable()
    {
        Initialize();
    }

    /// <summary>
    /// Initialize enemy state (called when spawned from pool)
    /// </summary>
    public virtual void Initialize()
    {
        currentHealth = maxHealth;
        startX = transform.position.x;
        moveTimer = 0f;
        nextFireTime = Time.time + Random.Range(0.5f, fireRate);
        isInitialized = true;
    }

    /// <summary>
    /// Set enemy stats (called by spawner for wave scaling)
    /// </summary>
    public virtual void SetStats(int health, float speed, int score)
    {
        maxHealth = health;
        currentHealth = health;
        moveSpeed = speed;
        scoreValue = score;
    }

    protected virtual void Update()
    {
        if (!isInitialized) return;

        // Don't update if game is paused
        if (GameManager.Instance != null && GameManager.Instance.IsPaused)
            return;

        HandleMovement();
        HandleShooting();
        CheckBounds();
    }

    /// <summary>
    /// Handle enemy movement based on pattern
    /// </summary>
    protected virtual void HandleMovement()
    {
        moveTimer += Time.deltaTime;
        Vector3 movement = Vector3.zero;

        switch (movementPattern)
        {
            case EnemyMovementPattern.Straight:
                movement = Vector3.down * moveSpeed * Time.deltaTime;
                break;

            case EnemyMovementPattern.Sine:
                float sineOffset = Mathf.Sin(moveTimer * sineFrequency) * sineAmplitude;
                float newX = startX + sineOffset;
                movement = new Vector3(newX - transform.position.x, -moveSpeed * Time.deltaTime, 0);
                break;

            case EnemyMovementPattern.Diagonal:
                movement = new Vector3(sineAmplitude * Time.deltaTime, -moveSpeed * Time.deltaTime, 0);
                break;

            case EnemyMovementPattern.ZigZag:
                float zigzag = Mathf.PingPong(moveTimer * sineFrequency, 1f) * 2f - 1f;
                movement = new Vector3(zigzag * sineAmplitude * Time.deltaTime, -moveSpeed * Time.deltaTime, 0);
                break;

            case EnemyMovementPattern.Tracking:
                // Find player and move towards their x position
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    float targetX = player.transform.position.x;
                    float xDiff = targetX - transform.position.x;
                    float xMove = Mathf.Clamp(xDiff, -1f, 1f) * sineAmplitude * Time.deltaTime;
                    movement = new Vector3(xMove, -moveSpeed * Time.deltaTime, 0);
                }
                else
                {
                    movement = Vector3.down * moveSpeed * Time.deltaTime;
                }
                break;
        }

        transform.position += movement;
    }

    /// <summary>
    /// Handle enemy shooting
    /// </summary>
    protected virtual void HandleShooting()
    {
        if (!canShoot) return;

        if (Time.time >= nextFireTime)
        {
            Fire();
            nextFireTime = Time.time + fireRate;
        }
    }

    /// <summary>
    /// Fire a bullet
    /// </summary>
    protected virtual void Fire()
    {
        if (ObjectPooler.Instance == null) return;

        // Play shoot sound
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySound(shootSoundName);
        }

        // Spawn bullet from pool
        GameObject bullet = ObjectPooler.Instance.SpawnFromPool(bulletPoolTag, transform.position, Quaternion.Euler(0, 0, 180));
        if (bullet != null)
        {
            Bullet bulletScript = bullet.GetComponent<Bullet>();
            if (bulletScript != null)
            {
                bulletScript.SetDirection(Vector2.down);
            }
        }
    }

    /// <summary>
    /// Check if enemy is out of bounds
    /// </summary>
    protected virtual void CheckBounds()
    {
        // Deactivate if below screen
        if (transform.position.y < -7f)
        {
            gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Apply damage to enemy
    /// </summary>
    public virtual void TakeDamage(int damage)
    {
        currentHealth -= damage;

        // Visual feedback (flash white)
        StartCoroutine(DamageFlash());

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// Flash sprite when taking damage
    /// </summary>
    protected System.Collections.IEnumerator DamageFlash()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            Color originalColor = sr.color;
            sr.color = Color.red;
            yield return new WaitForSeconds(0.05f);
            sr.color = originalColor;
        }
    }

    /// <summary>
    /// Handle enemy death
    /// </summary>
    protected virtual void Die()
    {
        // Play death sound
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySound(deathSoundName);
        }

        // Spawn explosion effect
        SpawnExplosion();

        // Award score
        OnEnemyKilled?.Invoke(scoreValue);

        // Chance to drop power-up
        TryDropPowerUp();

        // Return to pool
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Spawn explosion effect at death position
    /// </summary>
    protected virtual void SpawnExplosion()
    {
        if (ObjectPooler.Instance != null)
        {
            ObjectPooler.Instance.SpawnFromPool("Explosion", transform.position, Quaternion.identity);
        }
    }

    /// <summary>
    /// Attempt to drop a power-up
    /// </summary>
    protected virtual void TryDropPowerUp()
    {
        if (Random.value < powerUpDropChance && PowerUpSpawner.Instance != null)
        {
            PowerUpSpawner.Instance.SpawnRandomPowerUp(transform.position);
        }
    }

    /// <summary>
    /// Handle collision with player bullets
    /// </summary>
    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("PlayerBullet"))
        {
            // Get bullet damage
            Bullet bullet = other.GetComponent<Bullet>();
            int damage = bullet != null ? bullet.Damage : 1;

            // Take damage
            TakeDamage(damage);

            // Return bullet to pool
            other.gameObject.SetActive(false);
        }
    }
}

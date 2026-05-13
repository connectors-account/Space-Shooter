// ============================================================================
// EnemyBase.cs - Abstract base class for all enemy types
// Provides health, scoring, shooting, and despawning logic.
// ============================================================================
using UnityEngine;

/// <summary>
/// Base class for every enemy ship. Concrete types override MovementPattern()
/// to define unique flight paths. Handles health, damage, shooting, and death.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public abstract class EnemyBase : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] protected int maxHealth = 30;
    [SerializeField] protected int contactDamage = 20;
    [SerializeField] protected int scoreValue = 100;
    [SerializeField] protected float moveSpeed = 3f;

    [Header("Shooting")]
    [SerializeField] protected bool canShoot = true;
    [SerializeField] protected float fireRate = 1f;
    [SerializeField] protected GameObject bulletPrefab;

    [Header("Power-Up Drop")]
    [Tooltip("Chance (0-1) to drop a power-up on death.")]
    [SerializeField] protected float powerUpDropChance = 0.15f;
    [SerializeField] protected GameObject[] powerUpPrefabs;

    // ---- Runtime State ----
    protected int currentHealth;
    protected Rigidbody2D rb;
    protected float fireTimer;
    protected float spawnTime;
    protected Transform playerTransform;

    // ========================================================================
    // Unity Lifecycle
    // ========================================================================

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
    }

    protected virtual void OnEnable()
    {
        currentHealth = maxHealth;
        spawnTime = Time.time;
        fireTimer = Random.Range(0.5f, 1.5f); // Stagger initial shot timing.

        // Cache player reference.
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        playerTransform = player != null ? player.transform : null;
    }

    protected virtual void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameManager.GameState.Playing)
            return;

        // Execute the subclass's movement pattern.
        MovementPattern();

        // Shooting timer.
        if (canShoot)
        {
            fireTimer -= Time.deltaTime;
            if (fireTimer <= 0f)
            {
                Shoot();
                fireTimer = 1f / fireRate;
            }
        }

        // Despawn if out of bounds.
        if (GameBounds.Instance != null && GameBounds.Instance.IsOutOfBounds(transform.position))
        {
            ReturnToPool();
        }
    }

    // ========================================================================
    // Abstract / Virtual Methods
    // ========================================================================

    /// <summary>
    /// Override in subclasses to define unique movement behaviour each frame.
    /// </summary>
    protected abstract void MovementPattern();

    /// <summary>
    /// Default shooting: fires a bullet straight down toward the player.
    /// Override for custom bullet patterns.
    /// </summary>
    protected virtual void Shoot()
    {
        if (bulletPrefab == null) return;

        Vector3 spawnPos = transform.position + Vector3.down * 0.6f;
        GameObject bulletObj = null;

        if (PoolManager.Instance != null)
        {
            bulletObj = PoolManager.Instance.Get("EnemyBulletPool", spawnPos, Quaternion.identity);
        }

        if (bulletObj == null)
        {
            bulletObj = Instantiate(bulletPrefab, spawnPos, Quaternion.identity);
        }

        Bullet bullet = bulletObj.GetComponent<Bullet>();
        if (bullet != null)
        {
            Vector2 direction = Vector2.down;
            // Optionally aim toward the player for harder enemies.
            if (playerTransform != null && Random.value > 0.5f)
            {
                direction = ((Vector2)(playerTransform.position - transform.position)).normalized;
            }
            bullet.Initialize(direction, false);
        }

        AudioManager.Instance?.PlaySFX(AudioManager.SFX.EnemyShoot);
    }

    // ========================================================================
    // Damage & Death
    // ========================================================================

    /// <summary>
    /// Inflicts damage on this enemy. Destroys it if health reaches zero.
    /// </summary>
    public void TakeDamage(int amount)
    {
        currentHealth -= amount;

        // Flash white on hit.
        StartCoroutine(FlashWhite());

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// Handles enemy death: awards score, drops power-ups, plays effects.
    /// </summary>
    protected virtual void Die()
    {
        // Award score.
        GameManager.Instance?.AddScore(scoreValue);

        // Play explosion SFX.
        AudioManager.Instance?.PlaySFX(AudioManager.SFX.Explosion);

        // Attempt power-up drop.
        TryDropPowerUp();

        // Return to pool.
        ReturnToPool();
    }

    /// <summary>
    /// Rolls for a power-up drop and spawns a random one from the list.
    /// </summary>
    protected void TryDropPowerUp()
    {
        if (powerUpPrefabs == null || powerUpPrefabs.Length == 0) return;
        if (Random.value > powerUpDropChance) return;

        int index = Random.Range(0, powerUpPrefabs.Length);
        if (powerUpPrefabs[index] != null)
        {
            Instantiate(powerUpPrefabs[index], transform.position, Quaternion.identity);
        }
    }

    /// <summary>
    /// Returns this enemy to the object pool or destroys it.
    /// </summary>
    protected void ReturnToPool()
    {
        if (PoolManager.Instance != null)
        {
            PoolManager.Instance.Return("EnemyPool", gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // ========================================================================
    // Collision
    // ========================================================================

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Collide with player bullets.
        if (other.CompareTag("PlayerBullet"))
        {
            Bullet bullet = other.GetComponent<Bullet>();
            int damage = bullet != null ? bullet.Damage : 10;
            TakeDamage(damage);

            // Deactivate the bullet.
            if (bullet != null)
            {
                bullet.ReturnToPool();
            }
            else
            {
                Destroy(other.gameObject);
            }
        }

        // Collide with player (contact damage).
        if (other.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(contactDamage);
            }
            Die(); // Enemy also dies on contact.
        }
    }

    // ========================================================================
    // Visual Feedback
    // ========================================================================

    /// <summary>Briefly flashes the sprite white when taking damage.</summary>
    private System.Collections.IEnumerator FlashWhite()
    {
        SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
        if (sr == null) yield break;

        Color original = sr.color;
        sr.color = Color.white;
        yield return new WaitForSeconds(0.08f);
        if (sr != null) sr.color = original;
    }

    // ========================================================================
    // Difficulty Scaling
    // ========================================================================

    /// <summary>
    /// Called by the spawner to scale stats based on the current wave difficulty.
    /// </summary>
    public virtual void ApplyDifficultyScaling(float multiplier)
    {
        maxHealth = Mathf.RoundToInt(maxHealth * multiplier);
        currentHealth = maxHealth;
        moveSpeed *= (1f + (multiplier - 1f) * 0.5f);
        scoreValue = Mathf.RoundToInt(scoreValue * multiplier);
        if (canShoot)
        {
            fireRate *= (1f + (multiplier - 1f) * 0.3f);
        }
    }
}

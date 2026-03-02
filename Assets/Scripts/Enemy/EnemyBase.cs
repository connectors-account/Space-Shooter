using UnityEngine;

/// <summary>
/// Base class for all enemy types.
/// Provides common functionality like health, movement, and shooting.
/// </summary>
public class EnemyBase : MonoBehaviour
{
    [Header("Enemy Stats")]
    [Tooltip("Enemy health points")]
    public int health = 20;
    
    [Tooltip("Points awarded when destroyed")]
    public int scoreValue = 100;
    
    [Tooltip("Damage dealt to player on collision")]
    public int collisionDamage = 20;

    [Header("Movement")]
    [Tooltip("Base movement speed")]
    public float moveSpeed = 3f;

    [Header("Shooting")]
    [Tooltip("Can this enemy shoot?")]
    public bool canShoot = false;
    
    [Tooltip("Bullet prefab for enemy shots")]
    public GameObject bulletPrefab;
    
    [Tooltip("Time between shots")]
    public float fireRate = 2f;
    
    [Tooltip("Bullet speed")]
    public float bulletSpeed = 8f;
    
    protected float nextFireTime = 0f;

    [Header("Boundaries")]
    [Tooltip("Y position at which enemy is destroyed")]
    public float destroyY = -6f;

    [Header("Audio")]
    public AudioClip shootSound;
    public AudioClip deathSound;
    protected AudioSource audioSource;

    [Header("Visual Effects")]
    [Tooltip("Effect spawned on death")]
    public GameObject deathEffect;

    protected SpriteRenderer spriteRenderer;

    protected virtual void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
        
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Randomize first fire time slightly
        nextFireTime = Time.time + Random.Range(0.5f, fireRate);
    }

    protected virtual void Update()
    {
        // Handle movement - override in subclasses for different patterns
        Move();
        
        // Handle shooting if enabled
        if (canShoot)
        {
            HandleShooting();
        }
        
        // Destroy if off-screen
        if (transform.position.y < destroyY)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Override this method to implement different movement patterns
    /// </summary>
    protected virtual void Move()
    {
        // Default: move straight down
        transform.position += Vector3.down * moveSpeed * Time.deltaTime;
    }

    /// <summary>
    /// Handle enemy shooting logic
    /// </summary>
    protected virtual void HandleShooting()
    {
        if (Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + fireRate;
        }
    }

    /// <summary>
    /// Fire a bullet
    /// </summary>
    protected virtual void Shoot()
    {
        if (bulletPrefab != null)
        {
            GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
            Bullet bulletScript = bullet.GetComponent<Bullet>();
            
            if (bulletScript != null)
            {
                bulletScript.SetDirection(Vector2.down);
                bulletScript.SetSpeed(bulletSpeed);
                bulletScript.SetIsPlayerBullet(false);
            }
            
            if (shootSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(shootSound);
            }
        }
    }

    /// <summary>
    /// Apply damage to this enemy
    /// </summary>
    public virtual void TakeDamage(int damage)
    {
        health -= damage;
        
        // Flash effect on hit
        StartCoroutine(FlashOnHit());
        
        if (health <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// Visual feedback when hit
    /// </summary>
    System.Collections.IEnumerator FlashOnHit()
    {
        if (spriteRenderer != null)
        {
            Color originalColor = spriteRenderer.color;
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            if (spriteRenderer != null)
            {
                spriteRenderer.color = originalColor;
            }
        }
    }

    /// <summary>
    /// Handle enemy death
    /// </summary>
    protected virtual void Die()
    {
        // Add score
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddScore(scoreValue);
        }
        
        // Play death sound
        if (deathSound != null)
        {
            AudioSource.PlayClipAtPoint(deathSound, transform.position);
        }
        
        // Spawn death effect
        if (deathEffect != null)
        {
            Instantiate(deathEffect, transform.position, Quaternion.identity);
        }
        
        // Notify spawner
        if (EnemySpawner.Instance != null)
        {
            EnemySpawner.Instance.OnEnemyDestroyed();
        }
        
        Destroy(gameObject);
    }

    /// <summary>
    /// Handle collision with player
    /// </summary>
    void OnTriggerEnter2D(Collider2D other)
    {
        // Check for player collision
        if (other.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(collisionDamage);
            }
            
            // Enemy also takes damage from collision
            TakeDamage(health); // Destroy self
        }
    }
}

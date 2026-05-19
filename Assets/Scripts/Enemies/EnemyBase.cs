using UnityEngine;

/// <summary>
/// Base class for all enemy types. Provides health, scoring, power-up drops,
/// and shooting functionality. Subclasses override movement patterns.
/// </summary>
public class EnemyBase : MonoBehaviour
{
    [Header("Enemy Stats")]
    [SerializeField] protected int maxHealth = 3;
    [SerializeField] protected int scoreValue = 100;
    [SerializeField] protected float moveSpeed = 3f;

    [Header("Shooting")]
    [SerializeField] protected GameObject bulletPrefab;
    [SerializeField] protected float fireRate = 2f;
    [SerializeField] protected float fireRateVariance = 0.5f;
    [SerializeField] protected AudioClip shootSound;

    [Header("Power-Up Drop")]
    [SerializeField] [Range(0f, 1f)] protected float powerUpDropChance = 0.2f;

    [Header("Audio")]
    [SerializeField] protected AudioClip deathSound;

    protected int currentHealth;
    protected float nextFireTime;
    protected AudioSource audioSource;
    protected SpriteRenderer spriteRenderer;
    protected bool isDead;

    protected virtual void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    protected virtual void Start()
    {
        currentHealth = maxHealth;
        nextFireTime = Time.time + Random.Range(0.5f, fireRate);
    }

    protected virtual void Update()
    {
        if (isDead || GameManager.Instance.IsGamePaused) return;

        Move();
        HandleShooting();
        CheckBounds();
    }

    /// <summary>
    /// Override in subclasses to define unique movement patterns.
    /// </summary>
    protected virtual void Move() { }

    /// <summary>
    /// Fires bullets at the defined fire rate with some randomness.
    /// </summary>
    protected virtual void HandleShooting()
    {
        if (Time.time >= nextFireTime && bulletPrefab != null)
        {
            Shoot();
            nextFireTime = Time.time + fireRate + Random.Range(-fireRateVariance, fireRateVariance);
        }
    }

    /// <summary>
    /// Spawns a bullet aimed downward. Override for custom patterns.
    /// </summary>
    protected virtual void Shoot()
    {
        GameObject bulletObj = Instantiate(bulletPrefab, transform.position + Vector3.down * 0.5f, Quaternion.identity);
        Bullet bullet = bulletObj.GetComponent<Bullet>();
        if (bullet != null)
        {
            bullet.SetDirection(Vector2.down);
        }

        if (shootSound != null && audioSource != null)
            audioSource.PlayOneShot(shootSound, 0.4f);
    }

    /// <summary>
    /// Destroys the enemy if it moves too far off screen.
    /// </summary>
    protected virtual void CheckBounds()
    {
        if (transform.position.y < -7f || transform.position.y > 10f ||
            Mathf.Abs(transform.position.x) > 12f)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Applies damage to this enemy. Triggers death if health reaches zero.
    /// </summary>
    public virtual void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;

        // Flash white when hit
        StartCoroutine(FlashWhite());

        if (currentHealth <= 0)
            Die();
    }

    /// <summary>
    /// Brief white flash effect when hit.
    /// </summary>
    private System.Collections.IEnumerator FlashWhite()
    {
        if (spriteRenderer != null)
        {
            Color originalColor = spriteRenderer.color;
            spriteRenderer.color = Color.white;
            yield return new WaitForSeconds(0.05f);
            if (spriteRenderer != null)
                spriteRenderer.color = originalColor;
        }
    }

    /// <summary>
    /// Handles enemy death: score, effects, power-up drops.
    /// </summary>
    protected virtual void Die()
    {
        isDead = true;

        GameManager.Instance.AddScore(scoreValue);
        ExplosionEffect.SpawnExplosion(transform.position, 1f);

        // Chance to drop a power-up
        if (Random.value <= powerUpDropChance)
            PowerUpSpawner.Instance.SpawnRandomPowerUp(transform.position);

        if (deathSound != null && audioSource != null)
            AudioSource.PlayClipAtPoint(deathSound, transform.position, 0.6f);

        EnemySpawner.Instance.OnEnemyDestroyed();
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isDead) return;

        // Handle collision with player bullets
        if (other.CompareTag("PlayerBullet"))
        {
            Bullet bullet = other.GetComponent<Bullet>();
            int damage = bullet != null ? bullet.Damage : 1;
            TakeDamage(damage);
            Destroy(other.gameObject);
        }
    }
}

using UnityEngine;

/// <summary>
/// Base class for all enemy types. Handles health, damage, scoring, and drops.
/// </summary>
public class EnemyBase : MonoBehaviour
{
    [Header("Enemy Stats")]
    [SerializeField] protected int maxHealth = 30;
    [SerializeField] protected int contactDamage = 20;
    [SerializeField] protected int scoreValue = 100;
    [SerializeField] protected float moveSpeed = 3f;

    [Header("Shooting")]
    [SerializeField] protected bool canShoot = true;
    [SerializeField] protected GameObject bulletPrefab;
    [SerializeField] protected float fireRate = 2f;
    [SerializeField] protected float bulletSpeed = 6f;
    [SerializeField] protected int bulletDamage = 15;

    [Header("Drops")]
    [SerializeField] protected float powerUpDropChance = 0.15f;

    [Header("Effects")]
    [SerializeField] protected GameObject explosionPrefab;

    protected int currentHealth;
    protected float nextFireTime;
    protected bool isDead;
    protected SpriteRenderer spriteRenderer;

    public bool IsDead => isDead;

    protected virtual void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    protected virtual void Start()
    {
        currentHealth = maxHealth;
        nextFireTime = Time.time + Random.Range(0.5f, fireRate);
    }

    protected virtual void Update()
    {
        if (isDead || GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Playing)
            return;

        Move();

        if (canShoot)
            HandleShooting();

        // Destroy if off screen
        if (transform.position.y < -7f || transform.position.y > 8f ||
            Mathf.Abs(transform.position.x) > 12f)
        {
            Destroy(gameObject);
        }
    }

    protected virtual void Move()
    {
        // Default: move downward
        transform.Translate(Vector3.down * moveSpeed * Time.deltaTime, Space.World);
    }

    protected virtual void HandleShooting()
    {
        if (Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + fireRate + Random.Range(-0.3f, 0.3f);
            Shoot();
        }
    }

    protected virtual void Shoot()
    {
        if (bulletPrefab == null) return;

        Vector3 spawnPos = transform.position + Vector3.down * 0.5f;
        GameObject bullet = Instantiate(bulletPrefab, spawnPos, Quaternion.identity);
        Bullet bulletComp = bullet.GetComponent<Bullet>();
        if (bulletComp != null)
        {
            bulletComp.Initialize(Vector2.down, bulletSpeed, false, bulletDamage);
        }

        AudioManager.Instance?.PlaySFX("EnemyShoot");
    }

    public virtual void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;

        // Flash white on hit
        StartCoroutine(HitFlash());

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        isDead = true;

        // Score
        ScoreManager.Instance?.AddScore(scoreValue);

        // Explosion effect
        if (explosionPrefab != null)
        {
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        }
        else
        {
            CreateDefaultExplosion();
        }

        AudioManager.Instance?.PlaySFX("Explosion");

        // Power-up drop
        if (Random.value < powerUpDropChance)
        {
            PowerUpSpawner.Instance?.SpawnRandomPowerUp(transform.position);
        }

        // Notify wave manager
        EnemySpawner.Instance?.OnEnemyDestroyed();

        Destroy(gameObject);
    }

    private void CreateDefaultExplosion()
    {
        GameObject explosion = new GameObject("Explosion");
        explosion.transform.position = transform.position;
        explosion.AddComponent<ExplosionEffect>();
    }

    protected System.Collections.IEnumerator HitFlash()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(0.08f);
            if (spriteRenderer != null)
                spriteRenderer.color = Color.white;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null && !isDead)
        {
            player.TakeDamage(contactDamage);
            TakeDamage(maxHealth); // Destroy on contact
        }
    }
}

using UnityEngine;

/// <summary>
/// Base class for all enemy types. Handles health, damage, scoring, and destruction.
/// Derived classes implement specific movement and shooting patterns.
/// </summary>
public class EnemyBase : MonoBehaviour
{
    [Header("Enemy Stats")]
    public int maxHealth = 30;
    public int scoreValue = 100;
    public float moveSpeed = 3f;

    [Header("Shooting")]
    public bool canShoot = true;
    public float fireRate = 2f;
    public GameObject bulletPrefab;
    public Transform firePoint;

    [Header("Collision Damage")]
    public int collisionDamage = 20;

    protected int currentHealth;
    protected float nextFireTime;
    protected SpriteRenderer spriteRenderer;

    protected virtual void Start()
    {
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();
        nextFireTime = Time.time + Random.Range(0.5f, fireRate);
    }

    protected virtual void Update()
    {
        if (GameManager.Instance != null && (!GameManager.Instance.isGameActive || GameManager.Instance.isPaused))
            return;

        Move();

        if (canShoot && Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + fireRate + Random.Range(-0.3f, 0.3f);
        }

        // Destroy if off screen (below)
        if (GameBounds.Instance != null && transform.position.y < GameBounds.Instance.minY - 2f)
        {
            Destroy(gameObject);
        }
    }

    protected virtual void Move()
    {
        // Default: move straight down
        transform.Translate(Vector3.down * moveSpeed * Time.deltaTime, Space.World);
    }

    protected virtual void Shoot()
    {
        if (bulletPrefab == null) return;

        Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position;
        GameObject bullet = Instantiate(bulletPrefab, spawnPos, Quaternion.identity);
        Bullet bulletScript = bullet.GetComponent<Bullet>();
        if (bulletScript != null)
        {
            bulletScript.Initialize(Vector2.down, false);
        }
        AudioManager.Instance?.PlaySFX("EnemyShoot");
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        // Flash white on hit
        if (spriteRenderer != null)
        {
            StartCoroutine(FlashWhite());
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

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

    protected virtual void Die()
    {
        // Add score
        GameManager.Instance?.AddScore(scoreValue);

        // Spawn explosion
        EffectsManager.Instance?.SpawnExplosion(transform.position, 1f);
        AudioManager.Instance?.PlaySFX("EnemyExplosion");

        // Chance to drop power-up
        PowerUpSpawner spawner = FindObjectOfType<PowerUpSpawner>();
        if (spawner != null)
        {
            spawner.TrySpawnPowerUp(transform.position);
        }

        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            player.TakeDamage(collisionDamage);
            TakeDamage(maxHealth); // Enemy dies on collision with player
        }
    }
}

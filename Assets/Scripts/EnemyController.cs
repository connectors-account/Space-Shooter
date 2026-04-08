using UnityEngine;

/// <summary>
/// Enemy types with different behaviors.
/// </summary>
public enum EnemyType
{
    Straight,   // Moves straight down
    Zigzag,     // Zigzag pattern while moving down
    Swooper,    // Swoops in an arc
    Tank        // Slow but tough, fires more often
}

/// <summary>
/// Controls enemy behavior: movement patterns, shooting, health, and drops.
/// </summary>
public class EnemyController : MonoBehaviour
{
    [Header("Enemy Settings")]
    public EnemyType enemyType = EnemyType.Straight;
    public float moveSpeed = 3f;
    public int maxHealth = 20;
    public int currentHealth;
    public int scoreValue = 100;
    public int contactDamage = 20;

    [Header("Shooting")]
    public GameObject bulletPrefab;
    public float fireRate = 2f;
    public float fireRateVariance = 0.5f;
    private float nextFireTime;

    [Header("Movement")]
    public float zigzagAmplitude = 3f;
    public float zigzagFrequency = 2f;
    public float swoopRadius = 4f;

    [Header("Power-Up Drop")]
    public GameObject[] powerUpPrefabs;
    [Range(0f, 1f)]
    public float dropChance = 0.15f;

    // Internal state
    private float spawnX;
    private float aliveTime = 0f;
    private float difficultyMult = 1f;
    private SpriteRenderer spriteRenderer;
    private float destroyBoundary = -7f;

    void Start()
    {
        currentHealth = maxHealth;
        spawnX = transform.position.x;
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Randomize first shot timing
        nextFireTime = Time.time + Random.Range(0.5f, fireRate);

        // Apply type-specific settings
        switch (enemyType)
        {
            case EnemyType.Straight:
                moveSpeed = 3f * difficultyMult;
                maxHealth = 20;
                scoreValue = 100;
                break;
            case EnemyType.Zigzag:
                moveSpeed = 2.5f * difficultyMult;
                maxHealth = 20;
                scoreValue = 150;
                break;
            case EnemyType.Swooper:
                moveSpeed = 4f * difficultyMult;
                maxHealth = 15;
                scoreValue = 200;
                break;
            case EnemyType.Tank:
                moveSpeed = 1.5f * difficultyMult;
                maxHealth = 60;
                scoreValue = 300;
                fireRate = 1.5f;
                break;
        }

        currentHealth = maxHealth;
    }

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.isPaused)
            return;

        aliveTime += Time.deltaTime;
        HandleMovement();
        HandleShooting();

        // Destroy if off screen
        if (transform.position.y < destroyBoundary)
        {
            // Don't count as "killed" for score, but remove from wave count
            if (GameManager.Instance != null)
            {
                GameManager.Instance.enemiesRemainingInWave--;
            }
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Sets the difficulty multiplier for this enemy.
    /// </summary>
    public void SetDifficulty(float mult)
    {
        difficultyMult = mult;
        moveSpeed *= mult;
        fireRate = Mathf.Max(0.5f, fireRate / mult);
    }

    /// <summary>
    /// Handles movement based on enemy type.
    /// </summary>
    private void HandleMovement()
    {
        switch (enemyType)
        {
            case EnemyType.Straight:
                transform.position += Vector3.down * moveSpeed * Time.deltaTime;
                break;

            case EnemyType.Zigzag:
                float offsetX = Mathf.Sin(aliveTime * zigzagFrequency) * zigzagAmplitude;
                float newX = spawnX + offsetX * Time.deltaTime * zigzagFrequency;
                transform.position += Vector3.down * moveSpeed * Time.deltaTime;
                transform.position = new Vector3(
                    spawnX + Mathf.Sin(aliveTime * zigzagFrequency) * zigzagAmplitude * 0.5f,
                    transform.position.y,
                    0f);
                break;

            case EnemyType.Swooper:
                float swoopX = Mathf.Sin(aliveTime * 1.5f) * swoopRadius;
                float swoopY = -moveSpeed;
                transform.position += new Vector3(swoopX * Time.deltaTime, swoopY * Time.deltaTime, 0f);
                break;

            case EnemyType.Tank:
                transform.position += Vector3.down * moveSpeed * Time.deltaTime;
                break;
        }
    }

    /// <summary>
    /// Handles enemy shooting at timed intervals.
    /// </summary>
    private void HandleShooting()
    {
        if (bulletPrefab == null) return;

        if (Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + fireRate + Random.Range(-fireRateVariance, fireRateVariance);
            Shoot();
        }
    }

    /// <summary>
    /// Fires a bullet downward.
    /// </summary>
    private void Shoot()
    {
        Vector3 spawnPos = transform.position + Vector3.down * 0.5f;
        GameObject bullet = Instantiate(bulletPrefab, spawnPos, Quaternion.identity);

        BulletController bc = bullet.GetComponent<BulletController>();
        if (bc != null)
        {
            bc.isPlayerBullet = false;
            bc.direction = Vector2.down;
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySound("EnemyShoot");
        }
    }

    /// <summary>
    /// Applies damage to the enemy.
    /// </summary>
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        // Flash red when hit
        if (spriteRenderer != null)
        {
            StartCoroutine(FlashRed());
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// Brief red flash coroutine on hit.
    /// </summary>
    private System.Collections.IEnumerator FlashRed()
    {
        if (spriteRenderer != null)
        {
            Color original = spriteRenderer.color;
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            if (spriteRenderer != null)
                spriteRenderer.color = original;
        }
    }

    /// <summary>
    /// Handles enemy death: scoring, drops, and cleanup.
    /// </summary>
    private void Die()
    {
        // Report to GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnEnemyKilled(scoreValue);
        }

        // Chance to drop a power-up
        if (powerUpPrefabs != null && powerUpPrefabs.Length > 0 && Random.value <= dropChance)
        {
            int index = Random.Range(0, powerUpPrefabs.Length);
            Instantiate(powerUpPrefabs[index], transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("PlayerBullet"))
        {
            BulletController bullet = other.GetComponent<BulletController>();
            int damage = bullet != null ? bullet.damage : 10;
            TakeDamage(damage);
            Destroy(other.gameObject);
        }
    }
}

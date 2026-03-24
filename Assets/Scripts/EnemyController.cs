using UnityEngine;

/// <summary>
/// EnemyController manages enemy ship behaviour including movement patterns,
/// shooting, health, and scoring. Attach to each enemy prefab.
/// </summary>
public class EnemyController : MonoBehaviour
{
    // ── Enemy Types ──────────────────────────────────────────
    public enum EnemyType
    {
        Straight,   // Moves straight down
        Zigzag,     // Moves in a zigzag pattern
        Sine,       // Moves in a sine-wave pattern
        Charger,    // Aims toward the player then charges
        Boss        // Large enemy with more health and shooting
    }

    [Header("Type")]
    [SerializeField] private EnemyType enemyType = EnemyType.Straight;

    // ── Movement ─────────────────────────────────────────────
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float zigzagAmplitude = 3f;
    [SerializeField] private float zigzagFrequency = 2f;

    // ── Shooting ─────────────────────────────────────────────
    [Header("Shooting")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float fireRate = 1.5f;
    [SerializeField] private float bulletSpeed = 7f;
    [SerializeField] private bool canShoot = true;

    // ── Health & Scoring ─────────────────────────────────────
    [Header("Stats")]
    [SerializeField] private int maxHealth = 2;
    [SerializeField] private int scoreValue = 100;

    // ── Power-Up Drop ────────────────────────────────────────
    [Header("Drops")]
    [SerializeField] [Range(0f, 1f)] private float powerUpDropChance = 0.15f;

    // ── Internal ─────────────────────────────────────────────
    private int currentHealth;
    private float nextFireTime;
    private float spawnX;
    private float aliveTime = 0f;
    private Transform playerTransform;
    private SpriteRenderer spriteRenderer;
    private bool isDead = false;

    // ── Boundary for auto-destroy ────────────────────────────
    private float destroyBoundaryY = -7f;

    // ──────────────────────────────────────────────────────────
    // Unity Lifecycle
    // ──────────────────────────────────────────────────────────

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        currentHealth = maxHealth;
        spawnX = transform.position.x;
        nextFireTime = Time.time + Random.Range(0.5f, fireRate);

        // Cache player reference
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            playerTransform = player.transform;
    }

    private void Update()
    {
        if (isDead) return;

        aliveTime += Time.deltaTime;
        HandleMovement();
        HandleShooting();
        CheckBounds();
    }

    // ──────────────────────────────────────────────────────────
    // Public Setup (called by EnemySpawner)
    // ──────────────────────────────────────────────────────────

    /// <summary>
    /// Configure this enemy after instantiation.
    /// </summary>
    public void Setup(EnemyType type, int health, int score, float speed, bool shoots)
    {
        enemyType = type;
        maxHealth = health;
        currentHealth = health;
        scoreValue = score;
        moveSpeed = speed;
        canShoot = shoots;
    }

    // ──────────────────────────────────────────────────────────
    // Movement Patterns
    // ──────────────────────────────────────────────────────────

    private void HandleMovement()
    {
        switch (enemyType)
        {
            case EnemyType.Straight:
                MoveStraight();
                break;
            case EnemyType.Zigzag:
                MoveZigzag();
                break;
            case EnemyType.Sine:
                MoveSine();
                break;
            case EnemyType.Charger:
                MoveCharger();
                break;
            case EnemyType.Boss:
                MoveBoss();
                break;
        }
    }

    private void MoveStraight()
    {
        transform.Translate(Vector3.down * moveSpeed * Time.deltaTime, Space.World);
    }

    private void MoveZigzag()
    {
        float xOffset = Mathf.PingPong(aliveTime * zigzagFrequency, zigzagAmplitude * 2f) - zigzagAmplitude;
        float newX = spawnX + xOffset;
        float newY = transform.position.y - moveSpeed * Time.deltaTime;
        transform.position = new Vector3(newX, newY, 0f);
    }

    private void MoveSine()
    {
        float xOffset = Mathf.Sin(aliveTime * zigzagFrequency) * zigzagAmplitude;
        float newX = spawnX + xOffset;
        float newY = transform.position.y - moveSpeed * Time.deltaTime;
        transform.position = new Vector3(newX, newY, 0f);
    }

    private void MoveCharger()
    {
        // Move down initially, then charge toward player position
        if (aliveTime < 1.0f)
        {
            transform.Translate(Vector3.down * moveSpeed * 0.5f * Time.deltaTime, Space.World);
        }
        else
        {
            if (playerTransform != null)
            {
                Vector3 direction = (playerTransform.position - transform.position).normalized;
                transform.Translate(direction * moveSpeed * 1.5f * Time.deltaTime, Space.World);
            }
            else
            {
                MoveStraight();
            }
        }
    }

    private void MoveBoss()
    {
        // Boss moves to a position near the top, then patrols horizontally
        float targetY = 3.5f;
        if (transform.position.y > targetY)
        {
            transform.Translate(Vector3.down * moveSpeed * 0.5f * Time.deltaTime, Space.World);
        }
        else
        {
            float xOffset = Mathf.Sin(aliveTime * 0.8f) * 5f;
            transform.position = new Vector3(xOffset, targetY, 0f);
        }
    }

    // ──────────────────────────────────────────────────────────
    // Shooting
    // ──────────────────────────────────────────────────────────

    private void HandleShooting()
    {
        if (!canShoot) return;
        if (bulletPrefab == null) return;
        if (Time.time < nextFireTime) return;

        nextFireTime = Time.time + fireRate;

        switch (enemyType)
        {
            case EnemyType.Boss:
                // Boss fires a spread of 3 bullets
                FireBullet(Vector2.down);
                FireBullet(new Vector2(-0.3f, -1f).normalized);
                FireBullet(new Vector2(0.3f, -1f).normalized);
                break;

            case EnemyType.Charger:
                // Charger fires aimed bullet toward player
                if (playerTransform != null)
                {
                    Vector2 aimDir = ((Vector2)(playerTransform.position - transform.position)).normalized;
                    FireBullet(aimDir);
                }
                else
                {
                    FireBullet(Vector2.down);
                }
                break;

            default:
                FireBullet(Vector2.down);
                break;
        }
    }

    private void FireBullet(Vector2 direction)
    {
        Vector3 spawnPos = transform.position + Vector3.down * 0.5f;
        GameObject bullet = Instantiate(bulletPrefab, spawnPos, Quaternion.identity);
        BulletController bc = bullet.GetComponent<BulletController>();
        if (bc != null)
        {
            bc.Initialize(direction, bulletSpeed, false); // false = enemy bullet
        }
    }

    // ──────────────────────────────────────────────────────────
    // Damage & Death
    // ──────────────────────────────────────────────────────────

    public void TakeDamage(int amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        StartCoroutine(FlashDamage());

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        isDead = true;

        // Award score
        if (GameManager.Instance != null)
            GameManager.Instance.AddScore(scoreValue);

        // Play explosion SFX
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX("Explosion");

        // Chance to drop a power-up
        if (Random.value <= powerUpDropChance)
        {
            if (PowerUpSpawner.Instance != null)
                PowerUpSpawner.Instance.SpawnPowerUpAt(transform.position);
        }

        // Notify spawner that this enemy was destroyed
        if (EnemySpawner.Instance != null)
            EnemySpawner.Instance.OnEnemyDestroyed();

        Destroy(gameObject);
    }

    /// <summary>
    /// Brief color flash on damage.
    /// </summary>
    private System.Collections.IEnumerator FlashDamage()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(0.08f);
            if (spriteRenderer != null)
                spriteRenderer.color = Color.white;
        }
    }

    // ──────────────────────────────────────────────────────────
    // Collision – player bullets hurt the enemy
    // ──────────────────────────────────────────────────────────

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isDead) return;

        if (other.CompareTag("PlayerBullet"))
        {
            BulletController bc = other.GetComponent<BulletController>();
            int dmg = (bc != null) ? bc.Damage : 1;
            TakeDamage(dmg);
            Destroy(other.gameObject);
        }
    }

    // ──────────────────────────────────────────────────────────
    // Auto-destroy when out of view
    // ──────────────────────────────────────────────────────────

    private void CheckBounds()
    {
        if (transform.position.y < destroyBoundaryY ||
            Mathf.Abs(transform.position.x) > 12f ||
            transform.position.y > 10f)
        {
            if (EnemySpawner.Instance != null)
                EnemySpawner.Instance.OnEnemyDestroyed();

            Destroy(gameObject);
        }
    }
}

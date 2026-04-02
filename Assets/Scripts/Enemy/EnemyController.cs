using UnityEngine;

/// <summary>
/// Controls enemy behavior: movement patterns, shooting, health, and death.
/// Attach to Enemy prefabs with Rigidbody2D and Collider2D.
/// </summary>
public class EnemyController : MonoBehaviour
{
    public enum EnemyType { Basic, Zigzag, Bomber, Elite }
    public enum MovementPattern { StraightDown, Zigzag, SineWave, Dive, CircleEntry }

    [Header("Enemy Config")]
    [SerializeField] private EnemyType enemyType = EnemyType.Basic;
    [SerializeField] private MovementPattern movementPattern = MovementPattern.StraightDown;

    [Header("Stats")]
    [SerializeField] private int maxHealth = 1;
    [SerializeField] private int scoreValue = 100;
    [SerializeField] private float moveSpeed = 3f;

    [Header("Shooting")]
    [SerializeField] private bool canShoot = true;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float fireRate = 2f;
    [SerializeField] private float bulletSpeed = 6f;

    [Header("Movement Params")]
    [SerializeField] private float zigzagAmplitude = 3f;
    [SerializeField] private float zigzagFrequency = 2f;
    [SerializeField] private float sineAmplitude = 2f;
    [SerializeField] private float sineFrequency = 1.5f;

    [Header("Drops")]
    [SerializeField] private GameObject[] powerUpPrefabs;
    [SerializeField] [Range(0f, 1f)] private float dropChance = 0.1f;

    [Header("Effects")]
    [SerializeField] private GameObject explosionPrefab;

    private int currentHealth;
    private float nextFireTime;
    private float spawnTime;
    private float startX;
    private bool isDead;

    private void Start()
    {
        currentHealth = maxHealth;
        spawnTime = Time.time;
        startX = transform.position.x;
        nextFireTime = Time.time + Random.Range(0.5f, fireRate);
    }

    private void Update()
    {
        if (isDead) return;
        if (GameManager.Instance != null && !GameManager.Instance.IsPlaying) return;

        HandleMovement();
        HandleShooting();
        CheckBounds();
    }

    private void HandleMovement()
    {
        float elapsed = Time.time - spawnTime;

        switch (movementPattern)
        {
            case MovementPattern.StraightDown:
                transform.Translate(Vector3.down * moveSpeed * Time.deltaTime, Space.World);
                break;

            case MovementPattern.Zigzag:
                float zigzagX = Mathf.Sin(elapsed * zigzagFrequency) * zigzagAmplitude;
                float newX = startX + zigzagX;
                Vector3 zigPos = transform.position;
                zigPos.x = Mathf.Lerp(zigPos.x, newX, Time.deltaTime * 5f);
                zigPos.y -= moveSpeed * Time.deltaTime;
                transform.position = zigPos;
                break;

            case MovementPattern.SineWave:
                float sineX = Mathf.Sin(elapsed * sineFrequency) * sineAmplitude;
                Vector3 sinePos = transform.position;
                sinePos.x = startX + sineX;
                sinePos.y -= moveSpeed * Time.deltaTime;
                transform.position = sinePos;
                break;

            case MovementPattern.Dive:
                float diveSpeed = moveSpeed * (1f + elapsed * 0.5f);
                transform.Translate(Vector3.down * diveSpeed * Time.deltaTime, Space.World);
                break;

            case MovementPattern.CircleEntry:
                float circleRadius = 1.5f;
                float circleSpeed = 3f;
                float cx = Mathf.Cos(elapsed * circleSpeed) * circleRadius;
                float cy = Mathf.Sin(elapsed * circleSpeed) * circleRadius;
                Vector3 circleTarget = new Vector3(startX + cx, transform.position.y + cy * Time.deltaTime - moveSpeed * Time.deltaTime, 0f);
                transform.position = Vector3.Lerp(transform.position, circleTarget, Time.deltaTime * 3f);
                // Still move down overall
                transform.Translate(Vector3.down * moveSpeed * 0.5f * Time.deltaTime, Space.World);
                break;
        }
    }

    private void HandleShooting()
    {
        if (!canShoot || bulletPrefab == null) return;

        if (Time.time >= nextFireTime)
        {
            Fire();
            nextFireTime = Time.time + fireRate + Random.Range(-0.3f, 0.3f);
        }
    }

    private void Fire()
    {
        AudioManager.Instance?.PlaySFX("EnemyShoot");

        switch (enemyType)
        {
            case EnemyType.Basic:
                SpawnBullet(Vector2.down);
                break;

            case EnemyType.Zigzag:
                SpawnBullet(Vector2.down);
                break;

            case EnemyType.Bomber:
                // Spread shot
                SpawnBullet(Vector2.down);
                SpawnBullet(new Vector2(-0.3f, -1f).normalized);
                SpawnBullet(new Vector2(0.3f, -1f).normalized);
                break;

            case EnemyType.Elite:
                // Aimed shot toward player
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null && player.activeInHierarchy)
                {
                    Vector2 dir = (player.transform.position - transform.position).normalized;
                    SpawnBullet(dir);
                }
                else
                {
                    SpawnBullet(Vector2.down);
                }
                break;
        }
    }

    private void SpawnBullet(Vector2 direction)
    {
        Vector3 spawnPos = transform.position + Vector3.down * 0.5f;
        GameObject bullet = Instantiate(bulletPrefab, spawnPos, Quaternion.identity);
        BulletController bc = bullet.GetComponent<BulletController>();
        if (bc != null)
        {
            bc.Initialize(direction, bulletSpeed, 1, false);
        }
    }

    private void CheckBounds()
    {
        if (transform.position.y < -7f || transform.position.y > 8f ||
            Mathf.Abs(transform.position.x) > 12f)
        {
            Destroy(gameObject);
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;

        // Flash white on hit
        StartCoroutine(FlashWhite());

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private System.Collections.IEnumerator FlashWhite()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            Color original = sr.color;
            sr.color = Color.white;
            yield return new WaitForSeconds(0.05f);
            if (sr != null)
                sr.color = original;
        }
    }

    private void Die()
    {
        isDead = true;

        // Spawn explosion
        if (explosionPrefab != null)
        {
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        }

        AudioManager.Instance?.PlaySFX("EnemyExplosion");
        GameManager.Instance?.AddScore(scoreValue);

        // Chance to drop power-up
        if (powerUpPrefabs != null && powerUpPrefabs.Length > 0 && Random.value <= dropChance)
        {
            int idx = Random.Range(0, powerUpPrefabs.Length);
            if (powerUpPrefabs[idx] != null)
            {
                Instantiate(powerUpPrefabs[idx], transform.position, Quaternion.identity);
            }
        }

        SpawnManager.Instance?.OnEnemyDestroyed();
        Destroy(gameObject);
    }

    /// <summary>
    /// Configure this enemy at runtime (used by SpawnManager).
    /// </summary>
    public void Configure(EnemyType type, MovementPattern pattern, int health, float speed, int score, float shootRate)
    {
        enemyType = type;
        movementPattern = pattern;
        maxHealth = health;
        currentHealth = health;
        moveSpeed = speed;
        scoreValue = score;
        fireRate = shootRate;
    }
}

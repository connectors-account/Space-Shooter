using UnityEngine;

/// <summary>
/// Base enemy class. Handles health, damage, scoring, and drop-on-death.
/// Subclass or configure for different enemy behaviors.
/// </summary>
public class EnemyBase : MonoBehaviour
{
    [Header("Stats")]
    public int maxHealth = 3;
    public int scoreValue = 100;
    public int contactDamage = 1;

    [Header("Movement")]
    public float moveSpeed = 2f;
    public EnemyMovePattern movePattern = EnemyMovePattern.StraightDown;
    public float sineAmplitude = 2f;
    public float sineFrequency = 2f;

    [Header("Shooting")]
    public bool canShoot = true;
    public GameObject enemyBulletPrefab;
    public float shootInterval = 2f;
    public float shootIntervalVariance = 0.5f;

    [Header("Drops")]
    public GameObject healthPickupPrefab;
    [Range(0f, 1f)]
    public float healthDropChance = 0.1f;

    private int currentHealth;
    private float nextShootTime;
    private float startX;
    private float aliveTime;

    public enum EnemyMovePattern
    {
        StraightDown,
        SineWave,
        DiagonalLeft,
        DiagonalRight,
        ZigZag
    }

    void Start()
    {
        currentHealth = maxHealth;
        startX = transform.position.x;
        nextShootTime = Time.time + shootInterval + Random.Range(-shootIntervalVariance, shootIntervalVariance);
    }

    void Update()
    {
        if (GameManager.Instance != null && !GameManager.Instance.IsGameActive)
            return;

        aliveTime += Time.deltaTime;
        HandleMovement();
        HandleShooting();
        CheckBounds();
    }

    void HandleMovement()
    {
        Vector3 pos = transform.position;

        switch (movePattern)
        {
            case EnemyMovePattern.StraightDown:
                pos.y -= moveSpeed * Time.deltaTime;
                break;

            case EnemyMovePattern.SineWave:
                pos.y -= moveSpeed * Time.deltaTime;
                pos.x = startX + Mathf.Sin(aliveTime * sineFrequency) * sineAmplitude;
                break;

            case EnemyMovePattern.DiagonalLeft:
                pos.y -= moveSpeed * Time.deltaTime;
                pos.x -= moveSpeed * 0.5f * Time.deltaTime;
                break;

            case EnemyMovePattern.DiagonalRight:
                pos.y -= moveSpeed * Time.deltaTime;
                pos.x += moveSpeed * 0.5f * Time.deltaTime;
                break;

            case EnemyMovePattern.ZigZag:
                pos.y -= moveSpeed * Time.deltaTime;
                float zigzagPhase = Mathf.PingPong(aliveTime * sineFrequency, 1f);
                pos.x = startX + (zigzagPhase * 2f - 1f) * sineAmplitude;
                break;
        }

        transform.position = pos;
    }

    void HandleShooting()
    {
        if (!canShoot || enemyBulletPrefab == null) return;
        if (Time.time < nextShootTime) return;

        Vector3 bulletSpawn = transform.position + Vector3.down * 0.5f;
        GameObject bullet = Instantiate(enemyBulletPrefab, bulletSpawn, Quaternion.identity);
        Bullet b = bullet.GetComponent<Bullet>();
        if (b != null)
        {
            b.direction = Vector2.down;
            b.speed = 6f;
        }

        nextShootTime = Time.time + shootInterval + Random.Range(-shootIntervalVariance, shootIntervalVariance);
    }

    void CheckBounds()
    {
        if (transform.position.y < -7f || Mathf.Abs(transform.position.x) > 12f)
        {
            Destroy(gameObject);
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        // Flash red briefly
        StartCoroutine(FlashRed());

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    System.Collections.IEnumerator FlashRed()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            Color original = sr.color;
            sr.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            if (sr != null)
                sr.color = original;
        }
    }

    void Die()
    {
        // Award score
        if (ScoreManager.Instance != null)
            ScoreManager.Instance.AddScore(scoreValue);

        // Chance to drop health pickup
        if (healthPickupPrefab != null && Random.value < healthDropChance)
        {
            Instantiate(healthPickupPrefab, transform.position, Quaternion.identity);
        }

        // Notify wave spawner
        if (WaveSpawner.Instance != null)
            WaveSpawner.Instance.OnEnemyDestroyed();

        Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("PlayerBullet"))
        {
            Bullet bullet = other.GetComponent<Bullet>();
            int dmg = bullet != null ? bullet.damage : 1;
            TakeDamage(dmg);
            Destroy(other.gameObject);
        }
    }
}

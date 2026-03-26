using UnityEngine;

/// <summary>
/// Enemy behaviour supporting four archetypes.
/// Attach to every enemy prefab variant.
/// </summary>
[RequireComponent(typeof(Collider2D), typeof(SpriteRenderer))]
public class Enemy : MonoBehaviour
{
    // ── Enums ─────────────────────────────────────────────────────────
    public enum EnemyType { Basic, Zigzag, Tank, Boss }

    [Header("Identity")]
    public EnemyType enemyType = EnemyType.Basic;

    [Header("Stats")]
    public int   maxHealth   = 1;
    public int   scoreValue  = 100;
    public float moveSpeed   = 3f;

    [Header("Shooting")]
    public float shootInterval  = 2f;   // seconds between shots
    public float bulletSpeed    = 6f;
    public int   bulletDamage   = 1;

    [Header("Zigzag")]
    public float zigzagAmplitude = 2f;
    public float zigzagFrequency = 2f;

    [Header("Boss")]
    public float bossShootInterval = 1f;
    public int   bossBurstCount    = 12;

    [Header("Drops")]
    public GameObject[] powerUpPrefabs;  // assign in Inspector
    [Range(0f, 1f)] public float dropChance = 0.15f;

    // ── Runtime ───────────────────────────────────────────────────────
    private int   currentHealth;
    private float shootTimer;
    private float spawnX;
    private float aliveTime;

    private void Start()
    {
        currentHealth = maxHealth;
        shootTimer    = shootInterval;  // first shot after full interval
        spawnX        = transform.position.x;
    }

    private void Update()
    {
        if (GameManager.Instance != null &&
            GameManager.Instance.CurrentState != GameManager.State.Playing) return;

        Move();
        Shoot();
        CheckOffScreen();
    }

    // ── Movement patterns ─────────────────────────────────────────────
    private void Move()
    {
        aliveTime += Time.deltaTime;

        switch (enemyType)
        {
            case EnemyType.Basic:
                transform.Translate(Vector3.down * moveSpeed * Time.deltaTime, Space.World);
                break;

            case EnemyType.Zigzag:
                float xOffset = Mathf.Sin(aliveTime * zigzagFrequency) * zigzagAmplitude;
                float newX = spawnX + xOffset;
                float yMove = -moveSpeed * Time.deltaTime;
                transform.position = new Vector3(newX, transform.position.y + yMove, 0f);
                break;

            case EnemyType.Tank:
                transform.Translate(Vector3.down * (moveSpeed * 0.5f) * Time.deltaTime, Space.World);
                break;

            case EnemyType.Boss:
                // Boss slowly drifts side-to-side near the top
                float bossDrift = Mathf.Sin(aliveTime * 0.8f) * 3f;
                float bossTargetY = ScreenBounds.Instance != null
                    ? ScreenBounds.Instance.Top - 2f
                    : 3.5f;
                float bossY = Mathf.MoveTowards(transform.position.y, bossTargetY,
                                                 moveSpeed * Time.deltaTime);
                transform.position = new Vector3(bossDrift, bossY, 0f);
                break;
        }
    }

    // ── Shooting ──────────────────────────────────────────────────────
    private void Shoot()
    {
        float interval = enemyType == EnemyType.Boss ? bossShootInterval : shootInterval;
        shootTimer -= Time.deltaTime;
        if (shootTimer <= 0f)
        {
            shootTimer = interval;
            Vector3 spawnPos = transform.position + Vector3.down * 0.5f;

            switch (enemyType)
            {
                case EnemyType.Basic:
                case EnemyType.Zigzag:
                    BulletSpawner.SingleShot(spawnPos, Vector2.down, bulletSpeed,
                                             bulletDamage, Bullet.Owner.Enemy);
                    break;
                case EnemyType.Tank:
                    BulletSpawner.SpreadShot3(spawnPos, Vector2.down, bulletSpeed,
                                              bulletDamage, Bullet.Owner.Enemy);
                    break;
                case EnemyType.Boss:
                    BulletSpawner.BurstPattern(spawnPos, bossBurstCount, bulletSpeed,
                                               bulletDamage, Bullet.Owner.Enemy);
                    break;
            }
        }
    }

    // ── Damage ────────────────────────────────────────────────────────
    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        // Flash white briefly
        StartCoroutine(FlashWhite());

        if (currentHealth <= 0)
            Die();
    }

    private System.Collections.IEnumerator FlashWhite()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        Color original = sr.color;
        sr.color = Color.white;
        yield return new WaitForSeconds(0.05f);
        sr.color = original;
    }

    private void Die()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.AddScore(scoreValue);

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayExplosion();

        TryDropPowerUp();
        Destroy(gameObject);
    }

    private void TryDropPowerUp()
    {
        if (powerUpPrefabs == null || powerUpPrefabs.Length == 0) return;
        if (Random.value > dropChance) return;

        int idx = Random.Range(0, powerUpPrefabs.Length);
        if (powerUpPrefabs[idx] != null)
            Instantiate(powerUpPrefabs[idx], transform.position, Quaternion.identity);
    }

    // ── Off-screen cleanup ────────────────────────────────────────────
    private void CheckOffScreen()
    {
        if (ScreenBounds.Instance != null && ScreenBounds.Instance.IsOffScreen(transform.position))
        {
            // Don't award score for enemies that just flew past
            Destroy(gameObject);
        }
    }
}

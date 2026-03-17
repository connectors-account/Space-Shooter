using UnityEngine;

/// <summary>
/// Controls a single enemy: movement pattern, health, shooting, and scoring
/// on death. Enemies move downward with optional sine-wave or zigzag patterns.
/// </summary>
[RequireComponent(typeof(SpriteRenderer), typeof(BoxCollider2D))]
public class EnemyController : MonoBehaviour
{
    // ── Movement patterns ────────────────────────────────────────────
    public enum MovePattern { Straight, SineWave, Zigzag, Dive }

    [Header("Movement")]
    [SerializeField] private MovePattern pattern    = MovePattern.Straight;
    [SerializeField] private float moveSpeed        = 3f;
    [SerializeField] private float sineAmplitude    = 2f;
    [SerializeField] private float sineFrequency    = 2f;

    [Header("Combat")]
    [SerializeField] private int   health          = 2;
    [SerializeField] private int   scoreValue      = 100;
    [SerializeField] private int   contactDamage   = 1;

    [Header("Shooting")]
    [SerializeField] private bool       canShoot       = true;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float      fireRate       = 1.5f;
    [SerializeField] private float      bulletSpeed    = 6f;

    [Header("Drops")]
    [SerializeField] private GameObject[] powerUpPrefabs;
    [SerializeField, Range(0f, 1f)] private float dropChance = 0.15f;

    // ── Runtime ──────────────────────────────────────────────────────
    private float startX;
    private float aliveTime;
    private float nextFireTime;
    private bool  isDead;

    // Allow SpawnManager to override pattern at spawn
    public void Setup(MovePattern p, float speed, int hp, int score, bool shoots)
    {
        pattern    = p;
        moveSpeed  = speed;
        health     = hp;
        scoreValue = score;
        canShoot   = shoots;
    }

    private void Start()
    {
        startX = transform.position.x;
        nextFireTime = Time.time + Random.Range(0.5f, fireRate);
    }

    private void Update()
    {
        if (isDead) return;
        if (GameManager.Instance != null && (GameManager.Instance.IsGameOver || GameManager.Instance.IsPaused))
            return;

        Move();
        TryShoot();
        DestroyIfOffScreen();
    }

    // ── Movement ─────────────────────────────────────────────────────
    private void Move()
    {
        aliveTime += Time.deltaTime;
        float yMove = -moveSpeed * Time.deltaTime;

        switch (pattern)
        {
            case MovePattern.Straight:
                transform.position += new Vector3(0, yMove, 0);
                break;

            case MovePattern.SineWave:
                float xOffset = Mathf.Sin(aliveTime * sineFrequency) * sineAmplitude * Time.deltaTime;
                transform.position += new Vector3(xOffset, yMove, 0);
                break;

            case MovePattern.Zigzag:
                float zigzag = Mathf.PingPong(aliveTime * sineFrequency, 1f) * 2f - 1f;
                transform.position += new Vector3(zigzag * sineAmplitude * Time.deltaTime, yMove, 0);
                break;

            case MovePattern.Dive:
                float diveSpeed = moveSpeed + aliveTime * 0.5f; // accelerates
                transform.position += new Vector3(0, -diveSpeed * Time.deltaTime, 0);
                break;
        }
    }

    // ── Shooting ─────────────────────────────────────────────────────
    private void TryShoot()
    {
        if (!canShoot || bulletPrefab == null) return;
        if (Time.time < nextFireTime) return;

        nextFireTime = Time.time + fireRate + Random.Range(-0.2f, 0.3f);

        GameObject bullet = Instantiate(bulletPrefab, transform.position + Vector3.down * 0.5f, Quaternion.identity);
        BulletController bc = bullet.GetComponent<BulletController>();
        if (bc != null) bc.Init(Vector3.down, bulletSpeed, false);

        AudioManager.Instance?.PlaySFX("EnemyShoot");
    }

    // ── Damage ───────────────────────────────────────────────────────
    public void TakeDamage(int amount)
    {
        if (isDead) return;
        health -= amount;
        StartCoroutine(FlashWhite());
        if (health <= 0) Die();
    }

    private System.Collections.IEnumerator FlashWhite()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr == null) yield break;
        Color orig = sr.color;
        sr.color = Color.white;
        yield return new WaitForSeconds(0.08f);
        if (sr != null) sr.color = orig;
    }

    private void Die()
    {
        isDead = true;
        GameManager.Instance?.AddScore(scoreValue);
        AudioManager.Instance?.PlaySFX("EnemyExplode");
        TryDropPowerUp();
        SpawnManager.Instance?.OnEnemyDestroyed();
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

    // ── Collision (enemies damage player on contact) ─────────────────
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isDead) return;

        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            player.TakeDamage(contactDamage);
            Die();
        }
    }

    // ── Cleanup ──────────────────────────────────────────────────────
    private void DestroyIfOffScreen()
    {
        if (transform.position.y < -7f)
        {
            SpawnManager.Instance?.OnEnemyDestroyed();
            Destroy(gameObject);
        }
    }
}

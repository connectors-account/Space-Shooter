using UnityEngine;

/// <summary>
/// Three enemy behaviours selectable in the Inspector:
///   Straight  – falls straight down.
///   Zigzag    – side-to-side sine wave while descending.
///   Shooter   – falls straight and periodically fires at the player.
///
/// Tag the enemy prefabs "Enemy". Attach a PolygonCollider2D (isTrigger = true).
/// </summary>
public enum EnemyType { Straight, Zigzag, Shooter }

public class Enemy : MonoBehaviour
{
    // ── Inspector ──────────────────────────────────────────────────────────────
    [Header("Type & Stats")]
    public EnemyType enemyType  = EnemyType.Straight;
    public int       health     = 2;
    public int       scoreValue = 100;
    public float     moveSpeed  = 3f;

    [Header("Zigzag")]
    public float zigzagAmplitude  = 2.5f;
    public float zigzagFrequency  = 2.0f;

    [Header("Shooter")]
    public GameObject enemyBulletPrefab;
    public float      shootInterval = 2f;

    // ── Private ────────────────────────────────────────────────────────────────
    Vector3 spawnPosition;
    float   spawnTime;
    float   shootTimer;

    // ── Unity ──────────────────────────────────────────────────────────────────
    void Start()
    {
        spawnPosition = transform.position;
        spawnTime     = Time.time;
        // Randomise first shot delay so enemies don't all fire simultaneously
        shootTimer = Random.Range(0.3f, shootInterval);
    }

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver) return;

        Move();

        if (enemyType == EnemyType.Shooter)
            HandleShooting();

        // Cull when off the bottom of the screen
        if (transform.position.y < -6.5f)
            Destroy(gameObject);
    }

    // ── Movement ───────────────────────────────────────────────────────────────
    void Move()
    {
        float elapsed = Time.time - spawnTime;

        if (enemyType == EnemyType.Zigzag)
        {
            float xOffset = Mathf.Sin(elapsed * zigzagFrequency) * zigzagAmplitude;
            Vector3 p = spawnPosition;
            p.x = Mathf.Clamp(spawnPosition.x + xOffset, -8.5f, 8.5f);
            p.y = spawnPosition.y - elapsed * moveSpeed;
            transform.position = p;
        }
        else
        {
            transform.position += Vector3.down * moveSpeed * Time.deltaTime;
        }
    }

    // ── Shooting (Shooter type only) ───────────────────────────────────────────
    void HandleShooting()
    {
        if (enemyBulletPrefab == null) return;

        shootTimer -= Time.deltaTime;
        if (shootTimer <= 0f)
        {
            Instantiate(enemyBulletPrefab, transform.position, Quaternion.identity);
            shootTimer = shootInterval;
        }
    }

    // ── Public API ─────────────────────────────────────────────────────────────
    public void TakeDamage(int amount)
    {
        health -= amount;
        if (health <= 0) Die();
    }

    // ── Private ────────────────────────────────────────────────────────────────
    void Die()
    {
        GameManager.Instance?.AddScore(scoreValue);
        Destroy(gameObject);
    }

    // Ramming damage: enemy also dies when it hits the player
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager.Instance?.TakeDamage(1);
            Die();
        }
    }
}

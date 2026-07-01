using UnityEngine;

/// <summary>
/// Controls an enemy ship. Supports multiple movement patterns, periodic
/// shooting toward the player, health, and score rewards. Falls back to a
/// programmatic bullet if no prefab is assigned.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class EnemyController : MonoBehaviour
{
    public enum MovePattern { Straight, Sine, Diagonal }

    [Header("Movement")]
    [Tooltip("Downward speed in world units per second.")]
    public float moveSpeed = 2.5f;
    [Tooltip("Horizontal amplitude for the Sine pattern.")]
    public float sineAmplitude = 2f;
    [Tooltip("Horizontal frequency for the Sine pattern.")]
    public float sineFrequency = 2f;
    [Tooltip("Y position below which the enemy despawns.")]
    public float despawnY = -6f;

    [Header("Shooting")]
    [Tooltip("Enemy bullet prefab. If null, one is created programmatically.")]
    public GameObject bulletPrefab;
    [Tooltip("Chance (0-1) per shooting window that the enemy fires.")]
    [Range(0f, 1f)] public float shootChance = 0.7f;
    [Tooltip("Minimum seconds between shots.")]
    public float minShootInterval = 1.2f;
    [Tooltip("Maximum seconds between shots.")]
    public float maxShootInterval = 3f;
    public float bulletSpeed = 6f;
    public int bulletDamage = 20;

    [Header("Stats")]
    public int maxHealth = 30;
    [Tooltip("Score awarded to the player when destroyed.")]
    public int scoreValue = 100;

    private MovePattern pattern;
    private int currentHealth;
    private float startX;
    private float spawnTime;
    private float nextShootTime;
    private float diagonalDir = 1f;
    private bool counted = true; // whether GameManager is tracking this enemy

    private void Start()
    {
        currentHealth = maxHealth;
        startX = transform.position.x;
        spawnTime = Time.time;

        // Pick a random movement pattern for variety.
        pattern = (MovePattern)Random.Range(0, System.Enum.GetValues(typeof(MovePattern)).Length);
        diagonalDir = Random.value < 0.5f ? -1f : 1f;

        ScheduleNextShot();
    }

    private void Update()
    {
        Move();
        TryShoot();

        if (transform.position.y < despawnY)
        {
            DestroyEnemy(false, despawned: true);
        }
    }

    private void Move()
    {
        Vector3 pos = transform.position;
        float t = Time.time - spawnTime;

        switch (pattern)
        {
            case MovePattern.Straight:
                pos.y -= moveSpeed * Time.deltaTime;
                break;

            case MovePattern.Sine:
                pos.y -= moveSpeed * Time.deltaTime;
                pos.x = startX + Mathf.Sin(t * sineFrequency) * sineAmplitude;
                break;

            case MovePattern.Diagonal:
                pos.y -= moveSpeed * Time.deltaTime;
                pos.x += diagonalDir * moveSpeed * 0.5f * Time.deltaTime;
                // Bounce off horizontal edges.
                if (Mathf.Abs(pos.x) > 8.5f) diagonalDir *= -1f;
                break;
        }

        transform.position = pos;
    }

    private void TryShoot()
    {
        if (GameManager.Instance != null && GameManager.Instance.State != GameManager.GameState.Playing)
        {
            return;
        }

        if (Time.time >= nextShootTime)
        {
            if (Random.value <= shootChance)
            {
                Shoot();
            }
            ScheduleNextShot();
        }
    }

    private void ScheduleNextShot()
    {
        nextShootTime = Time.time + Random.Range(minShootInterval, maxShootInterval);
    }

    private void Shoot()
    {
        Vector3 spawnPos = transform.position + Vector3.down * 0.6f;

        GameObject bullet;
        if (bulletPrefab != null)
        {
            bullet = Instantiate(bulletPrefab, spawnPos, Quaternion.identity);
        }
        else
        {
            bullet = BulletFactory.CreateBullet(spawnPos, new Color(1f, 0.4f, 0.3f));
        }

        BulletController bc = bullet.GetComponent<BulletController>();
        if (bc == null) bc = bullet.AddComponent<BulletController>();
        bc.Initialize(Vector2.down, bulletSpeed, false, bulletDamage);
    }

    /// <summary>Apply damage; destroy and award score if health depletes.</summary>
    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        if (currentHealth <= 0)
        {
            DestroyEnemy(true);
        }
    }

    /// <summary>
    /// Remove the enemy. When killedByPlayer is true, award score.
    /// The despawned flag distinguishes "left the screen" from "rammed player".
    /// </summary>
    public void DestroyEnemy(bool killedByPlayer, bool despawned = false)
    {
        if (counted && GameManager.Instance != null)
        {
            counted = false;
            if (killedByPlayer)
            {
                GameManager.Instance.OnEnemyKilled(scoreValue);
            }
            else
            {
                GameManager.Instance.OnEnemyDespawned();
            }
        }
        Destroy(gameObject);
    }
}

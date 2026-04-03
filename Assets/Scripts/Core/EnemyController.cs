using UnityEngine;

/// <summary>
/// EnemyController defines the behaviour for all enemy types.
/// Each enemy has a type that determines its movement pattern,
/// health, speed, score value, and shooting behaviour.
/// </summary>
[RequireComponent(typeof(HealthSystem))]
public class EnemyController : MonoBehaviour
{
    // ============================================================
    // ENEMY TYPES
    // ============================================================

    /// <summary>
    /// Three distinct enemy types with different behaviours:
    /// - Basic: moves straight down, shoots occasionally
    /// - Fast: zigzags quickly, fragile, shoots rarely
    /// - Tank: slow, high HP, shoots frequently
    /// </summary>
    public enum EnemyType { Basic, Fast, Tank }

    [Header("Enemy Configuration")]
    public EnemyType enemyType = EnemyType.Basic;

    // ============================================================
    // MOVEMENT
    // ============================================================
    [Header("Movement")]
    [Tooltip("Downward speed in units/sec")]
    public float moveSpeed = 3f;

    [Tooltip("Horizontal zigzag amplitude (Fast type only)")]
    public float zigzagAmplitude = 3f;

    [Tooltip("Zigzag frequency (Fast type only)")]
    public float zigzagFrequency = 2f;

    // ============================================================
    // SHOOTING
    // ============================================================
    [Header("Shooting")]
    [Tooltip("Enemy bullet prefab")]
    public GameObject bulletPrefab;

    [Tooltip("Seconds between shots")]
    public float fireRate = 2f;

    [Tooltip("Speed of enemy bullets")]
    public float bulletSpeed = 6f;

    // ============================================================
    // SCORE
    // ============================================================
    [Header("Score")]
    public int scoreValue = 100;

    // ============================================================
    // POWER-UP DROPS
    // ============================================================
    [Header("Power-Up Drops")]
    [Tooltip("Chance (0-1) to drop a power-up on death")]
    public float powerUpDropChance = 0.15f;

    [Tooltip("Array of power-up prefabs that can be dropped")]
    public GameObject[] powerUpPrefabs;

    // ============================================================
    // INTERNAL STATE
    // ============================================================
    private HealthSystem healthSystem;
    private float nextFireTime;
    private float spawnX; // remember start X for zigzag calculation
    private float aliveTime = 0f;

    // ============================================================
    // UNITY LIFECYCLE
    // ============================================================

    void Start()
    {
        healthSystem = GetComponent<HealthSystem>();
        healthSystem.OnDeath += HandleDeath;

        spawnX = transform.position.x;

        // Configure stats based on enemy type
        ConfigureByType();

        // Randomize first shot timing so enemies don't all fire at once
        nextFireTime = Time.time + Random.Range(0.5f, fireRate);
    }

    void Update()
    {
        // Don't act if game isn't in play
        if (GameManager.Instance != null &&
            GameManager.Instance.CurrentState != GameManager.GameState.Playing)
            return;

        HandleMovement();
        HandleShooting();
        CheckBounds();

        aliveTime += Time.deltaTime;
    }

    void OnDestroy()
    {
        // Unsubscribe to prevent memory leaks
        if (healthSystem != null)
            healthSystem.OnDeath -= HandleDeath;
    }

    // ============================================================
    // TYPE CONFIGURATION
    // ============================================================

    /// <summary>
    /// Set stats based on the enemy type enum. This lets the spawner
    /// just set the type and everything else auto-configures.
    /// </summary>
    void ConfigureByType()
    {
        switch (enemyType)
        {
            case EnemyType.Basic:
                // Balanced stats – straight movement, moderate shooting
                moveSpeed = 2.5f;
                fireRate = 2.0f;
                healthSystem.maxHealth = 2;
                healthSystem.ResetHealth();
                scoreValue = GameManager.Instance != null
                    ? GameManager.Instance.baseEnemyScore : 100;
                break;

            case EnemyType.Fast:
                // Quick zigzag movement, low HP, rarely shoots
                moveSpeed = 4f;
                fireRate = 3.5f;
                healthSystem.maxHealth = 1;
                healthSystem.ResetHealth();
                scoreValue = GameManager.Instance != null
                    ? GameManager.Instance.fastEnemyScore : 150;
                zigzagAmplitude = 3f;
                zigzagFrequency = 2.5f;
                break;

            case EnemyType.Tank:
                // Slow and tanky, shoots frequently
                moveSpeed = 1.5f;
                fireRate = 1.2f;
                healthSystem.maxHealth = 5;
                healthSystem.ResetHealth();
                scoreValue = GameManager.Instance != null
                    ? GameManager.Instance.tankEnemyScore : 250;
                break;
        }
    }

    // ============================================================
    // MOVEMENT PATTERNS
    // ============================================================

    /// <summary>
    /// Move the enemy based on its type.
    /// Basic/Tank: straight down.
    /// Fast: sinusoidal zigzag while moving down.
    /// </summary>
    void HandleMovement()
    {
        switch (enemyType)
        {
            case EnemyType.Basic:
            case EnemyType.Tank:
                // Straight downward movement
                transform.position += Vector3.down * moveSpeed * Time.deltaTime;
                break;

            case EnemyType.Fast:
                // Zigzag: sinusoidal horizontal movement + downward drift
                float newX = spawnX + Mathf.Sin(aliveTime * zigzagFrequency) * zigzagAmplitude;
                float newY = transform.position.y - moveSpeed * Time.deltaTime;
                transform.position = new Vector3(newX, newY, 0f);
                break;
        }
    }

    // ============================================================
    // SHOOTING
    // ============================================================

    /// <summary>
    /// Fire a bullet downward at the configured fire rate.
    /// </summary>
    void HandleShooting()
    {
        if (bulletPrefab == null) return;
        if (Time.time < nextFireTime) return;

        // Create bullet going downward
        GameObject bullet = Instantiate(bulletPrefab, transform.position + Vector3.down * 0.5f, Quaternion.identity);
        bullet.tag = "EnemyBullet";

        BulletController bc = bullet.GetComponent<BulletController>();
        if (bc != null)
        {
            bc.direction = Vector2.down;
            bc.speed = bulletSpeed;
            bc.damage = 1;
        }

        nextFireTime = Time.time + fireRate;
    }

    // ============================================================
    // BOUNDS CHECK
    // ============================================================

    /// <summary>
    /// Destroy the enemy if it moves too far off screen
    /// (missed by the player – no score awarded).
    /// </summary>
    void CheckBounds()
    {
        if (transform.position.y < -7f)
        {
            // Notify GameManager that this enemy is gone (but no score)
            if (GameManager.Instance != null)
            {
                GameManager.Instance.EnemyDestroyed();
            }
            Destroy(gameObject);
        }
    }

    // ============================================================
    // DEATH HANDLING
    // ============================================================

    /// <summary>
    /// Called when HealthSystem fires OnDeath.
    /// Awards score, possibly drops a power-up, notifies GameManager.
    /// </summary>
    void HandleDeath()
    {
        // Award score to player
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddScore(scoreValue);
            GameManager.Instance.EnemyDestroyed();
        }

        // Roll for power-up drop
        TryDropPowerUp();

        // Destroy this enemy object
        Destroy(gameObject);
    }

    /// <summary>
    /// Randomly drop a power-up at the enemy's position.
    /// </summary>
    void TryDropPowerUp()
    {
        if (powerUpPrefabs == null || powerUpPrefabs.Length == 0) return;
        if (Random.value > powerUpDropChance) return;

        // Pick a random power-up from the array
        int index = Random.Range(0, powerUpPrefabs.Length);
        if (powerUpPrefabs[index] != null)
        {
            Instantiate(powerUpPrefabs[index], transform.position, Quaternion.identity);
        }
    }
}

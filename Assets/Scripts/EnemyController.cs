using UnityEngine;

/// <summary>
/// Defines enemy types with different behavior patterns.
/// </summary>
public enum EnemyType
{
    /// <summary>Moves straight down at constant speed.</summary>
    Basic,
    /// <summary>Moves in a sine-wave pattern while descending.</summary>
    Zigzag,
    /// <summary>Tougher enemy that shoots at the player.</summary>
    Heavy
}

/// <summary>
/// Controls enemy behavior: movement, shooting, health, and scoring.
/// Attach to enemy prefabs with Rigidbody2D, BoxCollider2D, and SpriteRenderer.
/// </summary>
public class EnemyController : MonoBehaviour
{
    [Header("Enemy Settings")]
    [SerializeField] private EnemyType enemyType = EnemyType.Basic;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private int health = 20;
    [SerializeField] private int scoreValue = 100;
    [SerializeField] private int contactDamage = 20;

    [Header("Shooting (Heavy type)")]
    [SerializeField] private GameObject enemyBulletPrefab;
    [SerializeField] private float fireRate = 1.5f;
    [SerializeField] private float bulletSpeed = 6f;

    [Header("Zigzag Settings")]
    [SerializeField] private float zigzagAmplitude = 2f;
    [SerializeField] private float zigzagFrequency = 2f;

    [Header("Power-up Drop")]
    [SerializeField] private GameObject[] powerUpPrefabs;
    [SerializeField] [Range(0f, 1f)] private float powerUpDropChance = 0.15f;

    [Header("Audio")]
    [SerializeField] private string explosionSFX = "EnemyExplosion";

    // Internal state
    private float nextFireTime;
    private float zigzagStartX;
    private float timeAlive;
    private Camera mainCamera;
    private float screenBottom;

    public EnemyType Type => enemyType;
    public int ScoreValue => scoreValue;

    private void Start()
    {
        mainCamera = Camera.main;
        zigzagStartX = transform.position.x;
        nextFireTime = Time.time + Random.Range(0.5f, fireRate);

        // Calculate screen bottom for cleanup
        Vector3 bottomLeft = mainCamera.ViewportToWorldPoint(new Vector3(0, 0, 0));
        screenBottom = bottomLeft.y - 1f;
    }

    private void Update()
    {
        if (GameManager.Instance != null && !GameManager.Instance.IsGameActive)
            return;

        timeAlive += Time.deltaTime;

        switch (enemyType)
        {
            case EnemyType.Basic:
                MoveBasic();
                break;
            case EnemyType.Zigzag:
                MoveZigzag();
                break;
            case EnemyType.Heavy:
                MoveBasic();
                HandleShooting();
                break;
        }

        // Destroy if off-screen
        if (transform.position.y < screenBottom)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Straight downward movement.
    /// </summary>
    private void MoveBasic()
    {
        transform.Translate(Vector3.down * moveSpeed * Time.deltaTime);
    }

    /// <summary>
    /// Sine-wave horizontal movement while descending.
    /// </summary>
    private void MoveZigzag()
    {
        transform.Translate(Vector3.down * moveSpeed * Time.deltaTime);
        float newX = zigzagStartX + Mathf.Sin(timeAlive * zigzagFrequency) * zigzagAmplitude;
        transform.position = new Vector3(newX, transform.position.y, 0);
    }

    /// <summary>
    /// Heavy enemies periodically fire bullets downward.
    /// </summary>
    private void HandleShooting()
    {
        if (Time.time >= nextFireTime && enemyBulletPrefab != null)
        {
            nextFireTime = Time.time + fireRate;
            FireBullet();
        }
    }

    /// <summary>
    /// Spawn an enemy bullet aimed downward.
    /// </summary>
    private void FireBullet()
    {
        Vector3 spawnPos = transform.position + Vector3.down * 0.5f;
        GameObject bullet = Instantiate(enemyBulletPrefab, spawnPos, Quaternion.identity);
        BulletController bc = bullet.GetComponent<BulletController>();
        if (bc != null)
        {
            bc.Initialize(Vector2.down, bulletSpeed, false);
        }
    }

    /// <summary>
    /// Apply damage to this enemy. Destroys enemy and awards score if health reaches 0.
    /// </summary>
    public void TakeDamage(int damage)
    {
        health -= damage;

        if (health <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// Handle enemy death: award score, play sound, possibly drop power-up.
    /// </summary>
    private void Die()
    {
        // Award score
        if (GameManager.Instance != null)
            GameManager.Instance.AddScore(scoreValue);

        // Play explosion sound
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(explosionSFX);

        // Chance to drop a power-up
        TryDropPowerUp();

        Destroy(gameObject);
    }

    /// <summary>
    /// Roll for power-up drop and instantiate one if successful.
    /// </summary>
    private void TryDropPowerUp()
    {
        if (powerUpPrefabs == null || powerUpPrefabs.Length == 0) return;

        if (Random.value <= powerUpDropChance)
        {
            int index = Random.Range(0, powerUpPrefabs.Length);
            if (powerUpPrefabs[index] != null)
            {
                Instantiate(powerUpPrefabs[index], transform.position, Quaternion.identity);
            }
        }
    }

    /// <summary>
    /// Configure enemy stats based on wave number for difficulty scaling.
    /// </summary>
    public void SetDifficulty(int waveNumber)
    {
        float multiplier = 1f + (waveNumber - 1) * 0.1f;
        health = Mathf.RoundToInt(health * multiplier);
        moveSpeed *= (1f + (waveNumber - 1) * 0.05f);
        scoreValue = Mathf.RoundToInt(scoreValue * multiplier);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Handle collision with player bullets
        if (other.CompareTag("PlayerBullet"))
        {
            BulletController bullet = other.GetComponent<BulletController>();
            if (bullet != null)
            {
                TakeDamage(bullet.Damage);
                Destroy(other.gameObject);
            }
        }
    }
}

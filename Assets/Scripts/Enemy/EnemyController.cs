using UnityEngine;

/// <summary>
/// Controls enemy behavior: movement patterns, health, and shooting.
/// Attach to Enemy prefab GameObjects.
/// </summary>
public class EnemyController : MonoBehaviour
{
    public enum MovementPattern
    {
        StraightDown,
        Zigzag,
        Sine,
        Dive
    }

    [Header("Movement")]
    [SerializeField] private MovementPattern pattern = MovementPattern.StraightDown;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float zigzagAmplitude = 2f;
    [SerializeField] private float zigzagFrequency = 2f;

    [Header("Combat")]
    [SerializeField] private int maxHealth = 1;
    [SerializeField] private int scoreValue = 100;
    [SerializeField] private int contactDamage = 1;

    [Header("Shooting")]
    [SerializeField] private bool canShoot = false;
    [SerializeField] private GameObject enemyBulletPrefab;
    [SerializeField] private float shootInterval = 2f;
    [SerializeField] private float bulletSpeed = 6f;

    [Header("Drops")]
    [SerializeField] private GameObject[] powerUpPrefabs;
    [SerializeField] [Range(0f, 1f)] private float dropChance = 0.15f;

    // State
    private int currentHealth;
    private float spawnX;
    private float timeSinceSpawn;
    private float nextShootTime;
    private bool isDead = false;

    private void Start()
    {
        currentHealth = maxHealth;
        spawnX = transform.position.x;
        timeSinceSpawn = 0f;
        nextShootTime = Time.time + Random.Range(0.5f, shootInterval);
    }

    private void Update()
    {
        if (isDead) return;

        timeSinceSpawn += Time.deltaTime;
        MoveByPattern();
        HandleShooting();
        DestroyIfOffScreen();
    }

    /// <summary>
    /// Moves the enemy based on its assigned movement pattern.
    /// </summary>
    private void MoveByPattern()
    {
        Vector3 pos = transform.position;

        switch (pattern)
        {
            case MovementPattern.StraightDown:
                pos.y -= moveSpeed * Time.deltaTime;
                break;

            case MovementPattern.Zigzag:
                pos.y -= moveSpeed * Time.deltaTime;
                pos.x = spawnX + Mathf.Sin(timeSinceSpawn * zigzagFrequency) * zigzagAmplitude;
                break;

            case MovementPattern.Sine:
                pos.y -= moveSpeed * 0.7f * Time.deltaTime;
                pos.x = spawnX + Mathf.Sin(timeSinceSpawn * zigzagFrequency * 0.5f) * zigzagAmplitude * 1.5f;
                break;

            case MovementPattern.Dive:
                // Moves down slowly, then dives fast toward player
                if (timeSinceSpawn < 1.5f)
                {
                    pos.y -= moveSpeed * 0.5f * Time.deltaTime;
                }
                else
                {
                    pos.y -= moveSpeed * 2.5f * Time.deltaTime;
                }
                break;
        }

        transform.position = pos;
    }

    /// <summary>
    /// Fires bullets at intervals toward the player.
    /// </summary>
    private void HandleShooting()
    {
        if (!canShoot || enemyBulletPrefab == null) return;
        if (Time.time < nextShootTime) return;

        nextShootTime = Time.time + shootInterval;

        // Shoot downward
        GameObject bullet = Instantiate(enemyBulletPrefab, transform.position, Quaternion.identity);
        BulletController bc = bullet.GetComponent<BulletController>();
        if (bc != null)
        {
            // Aim at player if visible, otherwise shoot straight down
            Vector2 direction = Vector2.down;
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null && player.activeInHierarchy)
            {
                direction = (player.transform.position - transform.position).normalized;
            }
            bc.Initialize(direction, bulletSpeed, 1, false);
        }

        AudioManager.Instance?.PlaySFX("EnemyShoot");
    }

    /// <summary>
    /// Called when enemy takes damage from player bullets.
    /// </summary>
    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;

        // Flash white briefly
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
            if (sr != null) sr.color = original;
        }
    }

    private void Die()
    {
        isDead = true;

        // Add score
        GameManager.Instance?.AddScore(scoreValue);

        // Notify spawner
        GameManager.Instance?.EnemyDestroyed();

        AudioManager.Instance?.PlaySFX("EnemyDeath");

        // Chance to drop power-up
        TryDropPowerUp();

        Destroy(gameObject);
    }

    private void TryDropPowerUp()
    {
        if (powerUpPrefabs == null || powerUpPrefabs.Length == 0) return;
        if (Random.value <= dropChance)
        {
            int index = Random.Range(0, powerUpPrefabs.Length);
            if (powerUpPrefabs[index] != null)
            {
                Instantiate(powerUpPrefabs[index], transform.position, Quaternion.identity);
            }
        }
    }

    private void DestroyIfOffScreen()
    {
        if (transform.position.y < -7f || transform.position.y > 10f ||
            Mathf.Abs(transform.position.x) > 12f)
        {
            GameManager.Instance?.EnemyDestroyed();
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Configure this enemy with specific parameters (called by spawner).
    /// </summary>
    public void Setup(MovementPattern movePattern, float speed, int health, int score,
                      bool shoots, float shootRate, GameObject bulletPrefab,
                      GameObject[] powerUps, float dropRate)
    {
        pattern = movePattern;
        moveSpeed = speed;
        maxHealth = health;
        currentHealth = health;
        scoreValue = score;
        canShoot = shoots;
        shootInterval = shootRate;
        enemyBulletPrefab = bulletPrefab;
        powerUpPrefabs = powerUps;
        dropChance = dropRate;
    }
}

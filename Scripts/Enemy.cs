using UnityEngine;

/// <summary>
/// Enemy behavior including movement patterns, health, and scoring
/// </summary>
public class Enemy : MonoBehaviour
{
    [Header("Enemy Stats")]
    [SerializeField] private int maxHealth = 1;
    [SerializeField] private int scoreValue = 100;
    [SerializeField] private float moveSpeed = 3f;

    [Header("Movement Pattern")]
    [SerializeField] private MovementPattern movementPattern = MovementPattern.Straight;
    [SerializeField] private float zigzagAmplitude = 2f;
    [SerializeField] private float zigzagFrequency = 2f;

    [Header("Shooting (Optional)")]
    [SerializeField] private bool canShoot = false;
    [SerializeField] private GameObject enemyBulletPrefab;
    [SerializeField] private float shootInterval = 2f;

    [Header("Visual Effects")]
    [SerializeField] private GameObject explosionPrefab;

    public enum MovementPattern
    {
        Straight,
        Zigzag,
        Diagonal,
        Sine
    }

    private int currentHealth;
    private float startX;
    private float timeAlive = 0f;
    private float nextShootTime;
    private float waveMultiplier = 1f;
    private float destroyBoundary = -7f;

    private GameManager gameManager;
    private EnemySpawner enemySpawner;

    private void Start()
    {
        currentHealth = Mathf.CeilToInt(maxHealth * waveMultiplier);
        startX = transform.position.x;
        nextShootTime = Time.time + Random.Range(0.5f, shootInterval);

        gameManager = FindObjectOfType<GameManager>();
        enemySpawner = FindObjectOfType<EnemySpawner>();
    }

    private void Update()
    {
        // Don't update if game is over
        if (gameManager != null && gameManager.IsGameOver())
            return;

        timeAlive += Time.deltaTime;
        
        HandleMovement();
        HandleShooting();
        CheckBounds();
    }

    private void HandleMovement()
    {
        Vector3 movement = Vector3.zero;

        switch (movementPattern)
        {
            case MovementPattern.Straight:
                // Move straight down
                movement = Vector3.down * moveSpeed * Time.deltaTime;
                break;

            case MovementPattern.Zigzag:
                // Move down with zigzag pattern
                float zigzagX = Mathf.Sin(timeAlive * zigzagFrequency) * zigzagAmplitude;
                movement = new Vector3(
                    Mathf.Cos(timeAlive * zigzagFrequency) * zigzagAmplitude * Time.deltaTime,
                    -moveSpeed * Time.deltaTime,
                    0f
                );
                break;

            case MovementPattern.Diagonal:
                // Move diagonally (direction based on starting position)
                float diagonalDir = startX > 0 ? -1f : 1f;
                movement = new Vector3(
                    diagonalDir * moveSpeed * 0.5f * Time.deltaTime,
                    -moveSpeed * Time.deltaTime,
                    0f
                );
                break;

            case MovementPattern.Sine:
                // Sine wave movement
                float sineX = Mathf.Sin(timeAlive * zigzagFrequency) * zigzagAmplitude * Time.deltaTime;
                movement = new Vector3(sineX, -moveSpeed * Time.deltaTime, 0f);
                break;
        }

        transform.position += movement;
    }

    private void HandleShooting()
    {
        if (!canShoot || enemyBulletPrefab == null)
            return;

        if (Time.time >= nextShootTime)
        {
            Shoot();
            nextShootTime = Time.time + shootInterval;
        }
    }

    private void Shoot()
    {
        Vector3 bulletSpawnPos = transform.position + Vector3.down * 0.5f;
        GameObject bullet = Instantiate(enemyBulletPrefab, bulletSpawnPos, Quaternion.identity);
        
        // Set bullet to move downward (toward player)
        Bullet bulletScript = bullet.GetComponent<Bullet>();
        if (bulletScript != null)
        {
            bulletScript.SetDirection(Vector2.down);
        }
    }

    private void CheckBounds()
    {
        // Destroy enemy if it goes off screen
        if (transform.position.y < destroyBoundary)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if hit by player bullet
        if (other.CompareTag("PlayerBullet"))
        {
            TakeDamage(1);
            Destroy(other.gameObject); // Destroy the bullet
        }
        // Check if collided with player
        else if (other.CompareTag("Player"))
        {
            // Enemy dies on contact with player
            Die(false); // Don't give score for collision
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        // Visual feedback - flash red
        StartCoroutine(FlashRed());

        if (currentHealth <= 0)
        {
            Die(true); // Give score when killed by bullet
        }
    }

    private System.Collections.IEnumerator FlashRed()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            Color originalColor = sr.color;
            sr.color = Color.red;
            yield return new WaitForSeconds(0.05f);
            sr.color = originalColor;
        }
    }

    private void Die(bool giveScore)
    {
        // Spawn explosion effect if available
        if (explosionPrefab != null)
        {
            GameObject explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            Destroy(explosion, 1f); // Clean up explosion after 1 second
        }

        // Add score if killed by player
        if (giveScore && gameManager != null)
        {
            int finalScore = Mathf.CeilToInt(scoreValue * waveMultiplier);
            gameManager.AddScore(finalScore);
        }

        // Notify spawner
        if (enemySpawner != null)
        {
            enemySpawner.OnEnemyDestroyed();
        }

        // Destroy this enemy
        Destroy(gameObject);
    }

    // Called by spawner to scale enemy based on wave
    public void SetWaveMultiplier(float multiplier)
    {
        waveMultiplier = multiplier;
        moveSpeed *= (1f + (multiplier - 1f) * 0.3f); // Slight speed increase per wave
    }

    // Get score value (for UI or other systems)
    public int GetScoreValue()
    {
        return Mathf.CeilToInt(scoreValue * waveMultiplier);
    }
}

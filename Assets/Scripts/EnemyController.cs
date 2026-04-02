// =============================================================================
// EnemyController.cs
// Controls enemy ship behavior: movement patterns, shooting, health, and
// loot drops. Supports multiple enemy types via the EnemyType enum.
// Attach this script to each enemy ship prefab.
// =============================================================================
using UnityEngine;

/// <summary>
/// Defines the different types of enemies in the game.
/// Each type has unique movement and attack patterns.
/// </summary>
public enum EnemyType
{
    /// <summary>Moves straight down. Simple and slow.</summary>
    Basic,
    /// <summary>Moves in a sinusoidal (wave) pattern.</summary>
    Zigzag,
    /// <summary>Charges directly toward the player's position.</summary>
    Charger
}

public class EnemyController : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Enemy Configuration
    // -------------------------------------------------------------------------
    [Header("Enemy Type")]
    [Tooltip("Determines the movement and behavior pattern of this enemy.")]
    public EnemyType enemyType = EnemyType.Basic;

    // -------------------------------------------------------------------------
    // Movement Settings
    // -------------------------------------------------------------------------
    [Header("Movement")]
    [Tooltip("Base downward movement speed in units per second.")]
    public float moveSpeed = 3f;

    [Tooltip("Amplitude of the zigzag pattern (Zigzag type only).")]
    public float zigzagAmplitude = 3f;

    [Tooltip("Frequency of the zigzag pattern (Zigzag type only).")]
    public float zigzagFrequency = 2f;

    [Tooltip("Speed multiplier when charging at the player (Charger type only).")]
    public float chargeSpeed = 6f;

    // -------------------------------------------------------------------------
    // Combat Settings
    // -------------------------------------------------------------------------
    [Header("Combat")]
    [Tooltip("Health points of this enemy.")]
    public int health = 1;

    [Tooltip("Damage dealt to the player on collision.")]
    public int contactDamage = 1;

    [Tooltip("Points awarded to the player for destroying this enemy.")]
    public int scoreValue = 100;

    [Tooltip("Whether this enemy can shoot at the player.")]
    public bool canShoot = false;

    [Tooltip("Reference to the enemy bullet prefab.")]
    public GameObject enemyBulletPrefab;

    [Tooltip("Time between shots in seconds.")]
    public float fireRate = 2f;

    [Tooltip("Point from which bullets are spawned.")]
    public Transform firePoint;

    // -------------------------------------------------------------------------
    // Drops
    // -------------------------------------------------------------------------
    [Header("Drops")]
    [Tooltip("Possible power-up prefabs this enemy can drop on death.")]
    public GameObject[] powerUpDrops;

    [Tooltip("Chance (0-1) of dropping a power-up on death.")]
    [Range(0f, 1f)]
    public float dropChance = 0.15f;

    // -------------------------------------------------------------------------
    // Effects
    // -------------------------------------------------------------------------
    [Header("Effects")]
    [Tooltip("Explosion effect prefab spawned on death.")]
    public GameObject explosionPrefab;

    // -------------------------------------------------------------------------
    // Internal State
    // -------------------------------------------------------------------------
    private float nextFireTime = 0f;
    private float spawnX;           // Starting X position (for zigzag offset)
    private float aliveTime = 0f;   // Time since spawn (for sine calculation)
    private Transform playerTransform;
    private bool hasChargeTarget = false;
    private Vector3 chargeDirection;

    // Lower bound: destroy enemy if it goes off screen
    private float destroyY = -7f;

    // -------------------------------------------------------------------------
    // Unity Lifecycle
    // -------------------------------------------------------------------------

    /// <summary>
    /// Cache initial position and find the player reference.
    /// </summary>
    void Start()
    {
        spawnX = transform.position.x;

        // Try to find the player ship in the scene
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }

        // If no fire point assigned, create one below the ship
        if (firePoint == null && canShoot)
        {
            GameObject fp = new GameObject("EnemyFirePoint");
            fp.transform.SetParent(transform);
            fp.transform.localPosition = new Vector3(0f, -0.6f, 0f);
            firePoint = fp.transform;
        }

        // Randomize first shot timing so enemies don't all fire at once
        nextFireTime = Time.time + Random.Range(0.5f, fireRate);
    }

    /// <summary>
    /// Update movement pattern and handle shooting every frame.
    /// </summary>
    void Update()
    {
        aliveTime += Time.deltaTime;

        // Execute movement based on enemy type
        switch (enemyType)
        {
            case EnemyType.Basic:
                MoveBasic();
                break;
            case EnemyType.Zigzag:
                MoveZigzag();
                break;
            case EnemyType.Charger:
                MoveCharger();
                break;
        }

        // Handle shooting if this enemy can shoot
        if (canShoot)
        {
            HandleShooting();
        }

        // Destroy if off-screen (below the camera view)
        if (transform.position.y < destroyY)
        {
            Destroy(gameObject);
        }
    }

    // -------------------------------------------------------------------------
    // Movement Patterns
    // -------------------------------------------------------------------------

    /// <summary>
    /// Basic movement: straight downward at constant speed.
    /// </summary>
    private void MoveBasic()
    {
        transform.position += Vector3.down * moveSpeed * Time.deltaTime;
    }

    /// <summary>
    /// Zigzag movement: moves down while oscillating left and right.
    /// Uses a sine wave based on time since spawn.
    /// </summary>
    private void MoveZigzag()
    {
        // Move downward
        float newY = transform.position.y - moveSpeed * Time.deltaTime;
        // Oscillate horizontally using sine wave
        float newX = spawnX + Mathf.Sin(aliveTime * zigzagFrequency) * zigzagAmplitude;
        transform.position = new Vector3(newX, newY, 0f);
    }

    /// <summary>
    /// Charger movement: locks onto the player's position at spawn, then
    /// charges in that direction at high speed.
    /// </summary>
    private void MoveCharger()
    {
        // On first frame, calculate charge direction toward the player
        if (!hasChargeTarget)
        {
            if (playerTransform != null)
            {
                chargeDirection = (playerTransform.position - transform.position).normalized;
            }
            else
            {
                chargeDirection = Vector3.down; // Default to straight down if no player
            }
            hasChargeTarget = true;
        }

        transform.position += chargeDirection * chargeSpeed * Time.deltaTime;

        // Also destroy if too far off screen in any direction
        if (Mathf.Abs(transform.position.x) > 12f || Mathf.Abs(transform.position.y) > 8f)
        {
            Destroy(gameObject);
        }
    }

    // -------------------------------------------------------------------------
    // Shooting
    // -------------------------------------------------------------------------

    /// <summary>
    /// Fires bullets at the player at the configured fire rate.
    /// </summary>
    private void HandleShooting()
    {
        if (Time.time >= nextFireTime && enemyBulletPrefab != null)
        {
            Vector3 spawnPos = (firePoint != null) ? firePoint.position : transform.position;
            GameObject bullet = Instantiate(enemyBulletPrefab, spawnPos, Quaternion.identity);

            // Set the bullet to move downward (enemy bullets go down)
            BulletController bc = bullet.GetComponent<BulletController>();
            if (bc != null)
            {
                bc.SetDirection(Vector2.down);
                bc.isPlayerBullet = false;
            }

            AudioManager.Instance?.PlaySFX("EnemyShoot");
            nextFireTime = Time.time + fireRate;
        }
    }

    // -------------------------------------------------------------------------
    // Damage & Death
    // -------------------------------------------------------------------------

    /// <summary>
    /// Apply damage to this enemy. Destroys the enemy if health drops to zero.
    /// </summary>
    /// <param name="damage">Amount of damage to apply.</param>
    public void TakeDamage(int damage)
    {
        health -= damage;

        if (health <= 0)
        {
            Die();
        }
        else
        {
            // Flash the enemy sprite briefly to indicate damage
            StartCoroutine(FlashDamage());
        }
    }

    /// <summary>
    /// Brief visual flash when taking damage (but not dying).
    /// </summary>
    private System.Collections.IEnumerator FlashDamage()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            Color originalColor = sr.color;
            sr.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            sr.color = originalColor;
        }
    }

    /// <summary>
    /// Handles enemy death: awards score, spawns effects, drops power-ups.
    /// </summary>
    private void Die()
    {
        // Award score to the player
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddScore(scoreValue);
        }

        // Notify spawner that this enemy was destroyed
        if (EnemySpawner.Instance != null)
        {
            EnemySpawner.Instance.OnEnemyDestroyed();
        }

        // Spawn explosion effect
        if (explosionPrefab != null)
        {
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        }

        // Roll for power-up drop
        TryDropPowerUp();

        AudioManager.Instance?.PlaySFX("Explosion");

        // Destroy the enemy GameObject
        Destroy(gameObject);
    }

    /// <summary>
    /// Randomly drops a power-up based on the configured drop chance.
    /// </summary>
    private void TryDropPowerUp()
    {
        if (powerUpDrops != null && powerUpDrops.Length > 0 && Random.value <= dropChance)
        {
            int randomIndex = Random.Range(0, powerUpDrops.Length);
            if (powerUpDrops[randomIndex] != null)
            {
                Instantiate(powerUpDrops[randomIndex], transform.position, Quaternion.identity);
            }
        }
    }

    // -------------------------------------------------------------------------
    // Collision Handling
    // -------------------------------------------------------------------------

    /// <summary>
    /// When this enemy collides with the player, deal contact damage.
    /// </summary>
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.TakeDamage(contactDamage);
            }
            // Destroy self on contact with player
            Die();
        }
    }
}

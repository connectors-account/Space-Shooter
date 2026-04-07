using UnityEngine;

/// <summary>
/// EnemyController - Handles enemy movement patterns, shooting, health, and scoring.
/// Attach to each enemy prefab with Rigidbody2D, BoxCollider2D (trigger), SpriteRenderer.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
public class EnemyController : MonoBehaviour
{
    public enum EnemyType
    {
        /// <summary>Moves straight down. Simple cannon fodder.</summary>
        Straight,
        /// <summary>Moves in a sine-wave pattern.</summary>
        Zigzag,
        /// <summary>Moves toward the player, faster and tougher.</summary>
        Chaser
    }

    [Header("Type & Stats")]
    public EnemyType enemyType = EnemyType.Straight;
    public int health = 1;
    public int scoreValue = 100;
    public float moveSpeed = 3f;

    [Header("Shooting")]
    public bool canShoot = true;
    public GameObject bulletPrefab;
    public float fireRate = 2f;
    public float bulletSpeed = 6f;
    private float nextFireTime;

    [Header("Zigzag Settings")]
    public float zigzagAmplitude = 3f;
    public float zigzagFrequency = 2f;

    [Header("Chaser Settings")]
    public float chaseSpeed = 4f;

    [Header("Power-Up Drop")]
    public GameObject[] powerUpPrefabs;
    [Range(0f, 1f)]
    public float powerUpDropChance = 0.15f;

    [Header("Audio")]
    public AudioSource hitAudioSource;

    private Rigidbody2D rb;
    private float spawnX;
    private float aliveTime;
    private Transform playerTransform;

    // Screen bounds for cleanup
    private float screenBottom;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        BoxCollider2D col = GetComponent<BoxCollider2D>();
        col.isTrigger = true;

        gameObject.tag = "Enemy";
    }

    private void Start()
    {
        spawnX = transform.position.x;
        aliveTime = 0f;

        Camera cam = Camera.main;
        if (cam != null)
            screenBottom = -cam.orthographicSize - 2f;

        // Find the player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            playerTransform = player.transform;

        // Randomize first shot time
        nextFireTime = Time.time + Random.Range(0.5f, fireRate);
    }

    private void Update()
    {
        if (GameManager.Instance != null && (GameManager.Instance.IsGameOver || GameManager.Instance.IsPaused))
            return;

        aliveTime += Time.deltaTime;
        HandleShooting();

        // Destroy if off screen bottom
        if (transform.position.y < screenBottom)
        {
            Destroy(gameObject);
        }
    }

    private void FixedUpdate()
    {
        if (GameManager.Instance != null && (GameManager.Instance.IsGameOver || GameManager.Instance.IsPaused))
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        switch (enemyType)
        {
            case EnemyType.Straight:
                MoveStraight();
                break;
            case EnemyType.Zigzag:
                MoveZigzag();
                break;
            case EnemyType.Chaser:
                MoveChaser();
                break;
        }
    }

    private void MoveStraight()
    {
        rb.linearVelocity = Vector2.down * moveSpeed;
    }

    private void MoveZigzag()
    {
        float xOffset = Mathf.Sin(aliveTime * zigzagFrequency) * zigzagAmplitude;
        float targetX = spawnX + xOffset;
        float xVel = (targetX - transform.position.x) * zigzagFrequency;
        rb.linearVelocity = new Vector2(xVel, -moveSpeed);
    }

    private void MoveChaser()
    {
        if (playerTransform != null)
        {
            Vector2 direction = ((Vector2)playerTransform.position - (Vector2)transform.position).normalized;
            // Bias downward so chaser doesn't endlessly orbit
            direction = (direction + Vector2.down * 0.3f).normalized;
            rb.linearVelocity = direction * chaseSpeed;
        }
        else
        {
            rb.linearVelocity = Vector2.down * moveSpeed;
        }
    }

    private void HandleShooting()
    {
        if (!canShoot || bulletPrefab == null) return;
        if (Time.time < nextFireTime) return;

        nextFireTime = Time.time + fireRate;
        FireBullet();
    }

    private void FireBullet()
    {
        Vector2 direction = Vector2.down;

        // Chasers aim toward the player
        if (enemyType == EnemyType.Chaser && playerTransform != null)
        {
            direction = ((Vector2)playerTransform.position - (Vector2)transform.position).normalized;
        }

        GameObject bullet = Instantiate(bulletPrefab, transform.position + Vector3.down * 0.5f, Quaternion.identity);
        BulletController bc = bullet.GetComponent<BulletController>();
        if (bc != null)
        {
            bc.Initialize(direction, bulletSpeed, false, 1);
        }
    }

    /// <summary>
    /// Apply damage to this enemy. Destroy if health <= 0.
    /// </summary>
    public void TakeDamage(int damage)
    {
        health -= damage;

        if (hitAudioSource != null)
            hitAudioSource.Play();

        if (health <= 0)
        {
            Die();
        }
        else
        {
            // Flash red briefly
            StartCoroutine(FlashRed());
        }
    }

    private System.Collections.IEnumerator FlashRed()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            Color original = sr.color;
            sr.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            sr.color = original;
        }
    }

    private void Die()
    {
        // Add score
        if (GameManager.Instance != null)
            GameManager.Instance.AddScore(scoreValue);

        // Chance to drop power-up
        if (powerUpPrefabs != null && powerUpPrefabs.Length > 0 && Random.value <= powerUpDropChance)
        {
            int index = Random.Range(0, powerUpPrefabs.Length);
            if (powerUpPrefabs[index] != null)
            {
                Instantiate(powerUpPrefabs[index], transform.position, Quaternion.identity);
            }
        }

        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Hit by player bullet
        if (other.CompareTag("PlayerBullet"))
        {
            BulletController bc = other.GetComponent<BulletController>();
            int dmg = bc != null ? bc.damage : 1;
            TakeDamage(dmg);
            Destroy(other.gameObject);
        }
        // Collided with player
        else if (other.CompareTag("Player"))
        {
            TakeDamage(health); // Self-destruct on player collision
        }
    }
}

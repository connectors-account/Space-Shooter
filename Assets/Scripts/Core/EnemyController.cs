using UnityEngine;

/// <summary>
/// Controls enemy behavior: movement patterns, shooting, health, and scoring.
/// Attach to enemy prefabs. Set enemyType to define behavior.
/// Tag enemy GameObjects as "Enemy".
/// </summary>
public enum EnemyType
{
    StraightDown,   // Moves straight down at constant speed
    Zigzag,         // Moves downward in a zigzag pattern
    Tracker         // Moves toward the player
}

public class EnemyController : MonoBehaviour
{
    [Header("General")]
    public EnemyType enemyType = EnemyType.StraightDown;
    public float moveSpeed = 3f;
    public int health = 1;
    public int scoreValue = 100;

    [Header("Shooting")]
    public bool canShoot = true;
    public GameObject bulletPrefab;
    public float fireRate = 2f;
    public float bulletSpeed = 6f;

    [Header("Zigzag Settings")]
    public float zigzagAmplitude = 2f;
    public float zigzagFrequency = 2f;

    [Header("Tracker Settings")]
    public float trackingStrength = 3f;

    [Header("Visual")]
    public GameObject explosionPrefab;

    [Header("Drops")]
    [Range(0f, 1f)]
    public float powerUpDropChance = 0.15f;

    private float nextFireTime;
    private float spawnX;
    private float aliveTime;
    private Transform playerTransform;

    void Start()
    {
        spawnX = transform.position.x;
        aliveTime = 0f;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            playerTransform = player.transform;

        // Randomize initial fire delay
        nextFireTime = Time.time + Random.Range(0.5f, fireRate);
    }

    void Update()
    {
        aliveTime += Time.deltaTime;
        HandleMovement();
        HandleShooting();
        CheckBounds();
    }

    void HandleMovement()
    {
        switch (enemyType)
        {
            case EnemyType.StraightDown:
                transform.Translate(Vector3.down * moveSpeed * Time.deltaTime, Space.World);
                break;

            case EnemyType.Zigzag:
                float xOffset = Mathf.Sin(aliveTime * zigzagFrequency) * zigzagAmplitude;
                Vector3 zigzagPos = new Vector3(
                    spawnX + xOffset,
                    transform.position.y - moveSpeed * Time.deltaTime,
                    0f
                );
                transform.position = zigzagPos;
                break;

            case EnemyType.Tracker:
                if (playerTransform != null && playerTransform.gameObject.activeInHierarchy)
                {
                    Vector3 direction = (playerTransform.position - transform.position).normalized;
                    // Move mostly downward but track player horizontally
                    Vector3 movement = new Vector3(
                        direction.x * trackingStrength,
                        -moveSpeed,
                        0f
                    );
                    transform.Translate(movement * Time.deltaTime, Space.World);
                }
                else
                {
                    transform.Translate(Vector3.down * moveSpeed * Time.deltaTime, Space.World);
                }
                break;
        }
    }

    void HandleShooting()
    {
        if (!canShoot || bulletPrefab == null) return;

        if (Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + fireRate;
            FireBullet();
        }
    }

    void FireBullet()
    {
        Vector3 spawnPos = transform.position + Vector3.down * 0.5f;
        Vector2 direction = Vector2.down;

        // Tracker enemies aim at player
        if (enemyType == EnemyType.Tracker && playerTransform != null && playerTransform.gameObject.activeInHierarchy)
        {
            direction = (playerTransform.position - transform.position).normalized;
        }

        GameObject bullet = Instantiate(bulletPrefab, spawnPos, Quaternion.identity);
        BulletController bc = bullet.GetComponent<BulletController>();
        if (bc != null)
        {
            bc.Initialize(direction, bulletSpeed, false);
        }
    }

    void CheckBounds()
    {
        if (transform.position.y < -7f || transform.position.y > 7f ||
            Mathf.Abs(transform.position.x) > 12f)
        {
            Destroy(gameObject);
        }
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        if (health <= 0)
        {
            Die();
        }
        else
        {
            // Flash white briefly on hit
            StartCoroutine(FlashWhite());
        }
    }

    System.Collections.IEnumerator FlashWhite()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            Color original = sr.color;
            sr.color = Color.white;
            yield return new WaitForSeconds(0.05f);
            sr.color = original;
        }
    }

    void Die()
    {
        // Spawn explosion
        if (explosionPrefab != null)
        {
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        }

        // Award score
        GameManager.Instance?.AddScore(scoreValue);

        // Chance to drop power-up
        if (Random.value < powerUpDropChance)
        {
            SpawnManager.Instance?.SpawnRandomPowerUp(transform.position);
        }

        AudioManager.Instance?.PlaySFX("Explosion");

        Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            TakeDamage(health); // Die on contact with player
        }
    }
}

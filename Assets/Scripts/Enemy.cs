using UnityEngine;

public class Enemy : MonoBehaviour
{
    public enum EnemyType { Basic, Fast, Tank, Boss }
    public enum MovementPattern { Straight, Zigzag, Circular, Homing }
    public enum ShootingPattern { None, Single, Spread, Burst }

    [Header("Enemy Settings")]
    public EnemyType enemyType = EnemyType.Basic;
    public MovementPattern movementPattern = MovementPattern.Straight;
    public ShootingPattern shootingPattern = ShootingPattern.Single;

    [Header("Stats")]
    public int health = 1;
    public int scoreValue = 100;
    public float moveSpeed = 3f;
    public int contactDamage = 1;

    [Header("Shooting")]
    public GameObject bulletPrefab;
    public float fireRate = 1f;
    public float bulletSpeed = 8f;

    [Header("Audio")]
    public AudioClip shootSound;
    public AudioClip hitSound;
    public AudioClip deathSound;

    [Header("Power-up Drop")]
    public GameObject[] powerUpPrefabs;
    [Range(0f, 1f)]
    public float powerUpDropChance = 0.1f;

    private float nextFireTime = 0f;
    private float zigzagTimer = 0f;
    private float zigzagDirection = 1f;
    private Transform playerTransform;
    private AudioSource audioSource;
    private float circularAngle = 0f;
    private Vector3 startPosition;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }

        startPosition = transform.position;
        SetupEnemyType();
    }

    void SetupEnemyType()
    {
        switch (enemyType)
        {
            case EnemyType.Basic:
                health = 1;
                scoreValue = 100;
                moveSpeed = 3f;
                break;
            case EnemyType.Fast:
                health = 1;
                scoreValue = 150;
                moveSpeed = 6f;
                break;
            case EnemyType.Tank:
                health = 5;
                scoreValue = 300;
                moveSpeed = 1.5f;
                break;
            case EnemyType.Boss:
                health = 20;
                scoreValue = 1000;
                moveSpeed = 1f;
                break;
        }
    }

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGamePaused())
            return;

        HandleMovement();
        HandleShooting();
        CheckBounds();
    }

    void HandleMovement()
    {
        switch (movementPattern)
        {
            case MovementPattern.Straight:
                transform.position += Vector3.down * moveSpeed * Time.deltaTime;
                break;

            case MovementPattern.Zigzag:
                zigzagTimer += Time.deltaTime;
                if (zigzagTimer >= 0.5f)
                {
                    zigzagDirection *= -1f;
                    zigzagTimer = 0f;
                }
                Vector3 zigzagMove = new Vector3(zigzagDirection * moveSpeed * 0.5f, -moveSpeed, 0f);
                transform.position += zigzagMove * Time.deltaTime;
                break;

            case MovementPattern.Circular:
                circularAngle += moveSpeed * Time.deltaTime;
                float x = startPosition.x + Mathf.Sin(circularAngle) * 2f;
                float y = startPosition.y - Time.deltaTime * moveSpeed;
                startPosition.y = y + Time.deltaTime * moveSpeed;
                transform.position = new Vector3(x, transform.position.y - moveSpeed * Time.deltaTime * 0.5f, 0f);
                break;

            case MovementPattern.Homing:
                if (playerTransform != null)
                {
                    Vector3 direction = (playerTransform.position - transform.position).normalized;
                    transform.position += direction * moveSpeed * Time.deltaTime * 0.5f;
                    transform.position += Vector3.down * moveSpeed * Time.deltaTime * 0.3f;
                }
                else
                {
                    transform.position += Vector3.down * moveSpeed * Time.deltaTime;
                }
                break;
        }
    }

    void HandleShooting()
    {
        if (shootingPattern == ShootingPattern.None || bulletPrefab == null)
            return;

        if (Time.time >= nextFireTime)
        {
            Fire();
            nextFireTime = Time.time + fireRate;
        }
    }

    void Fire()
    {
        PlaySound(shootSound);

        switch (shootingPattern)
        {
            case ShootingPattern.Single:
                SpawnBullet(Vector2.down);
                break;

            case ShootingPattern.Spread:
                SpawnBullet(new Vector2(-0.3f, -1f).normalized);
                SpawnBullet(Vector2.down);
                SpawnBullet(new Vector2(0.3f, -1f).normalized);
                break;

            case ShootingPattern.Burst:
                for (int i = 0; i < 5; i++)
                {
                    float angle = -30f + (i * 15f);
                    Vector2 direction = Quaternion.Euler(0, 0, angle) * Vector2.down;
                    SpawnBullet(direction);
                }
                break;
        }
    }

    void SpawnBullet(Vector2 direction)
    {
        GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
        Bullet bulletScript = bullet.GetComponent<Bullet>();
        if (bulletScript != null)
        {
            bulletScript.SetDirection(direction);
            bulletScript.SetSpeed(bulletSpeed);
            bulletScript.isPlayerBullet = false;
        }
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        PlaySound(hitSound);

        // Flash effect
        StartCoroutine(FlashEffect());

        if (health <= 0)
        {
            Die();
        }
    }

    System.Collections.IEnumerator FlashEffect()
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

    void Die()
    {
        PlaySound(deathSound);
        
        ScoreManager.Instance?.AddScore(scoreValue);
        WaveManager.Instance?.OnEnemyKilled();

        // Chance to drop power-up
        if (powerUpPrefabs != null && powerUpPrefabs.Length > 0)
        {
            if (Random.value <= powerUpDropChance)
            {
                int randomIndex = Random.Range(0, powerUpPrefabs.Length);
                if (powerUpPrefabs[randomIndex] != null)
                {
                    Instantiate(powerUpPrefabs[randomIndex], transform.position, Quaternion.identity);
                }
            }
        }

        Destroy(gameObject);
    }

    void CheckBounds()
    {
        if (transform.position.y < -7f)
        {
            Destroy(gameObject);
        }
    }

    void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.TakeDamage(contactDamage);
            }
            Die();
        }
    }
}

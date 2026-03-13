using UnityEngine;

public enum EnemyType
{
    Basic,
    Zigzag,
    Tank
}

public class EnemyController : MonoBehaviour
{
    [Header("Enemy Settings")]
    public EnemyType enemyType = EnemyType.Basic;
    public float moveSpeed = 3f;
    public int health = 1;
    public int scoreValue = 100;
    public int damageToPlayer = 1;

    [Header("Shooting Settings")]
    public bool canShoot = false;
    public GameObject bulletPrefab;
    public float fireRate = 2f;
    public float bulletSpeed = 8f;

    [Header("Zigzag Settings")]
    public float zigzagAmplitude = 2f;
    public float zigzagFrequency = 2f;

    [Header("Boundaries")]
    public float destroyYPosition = -6f;

    private float nextFireTime = 0f;
    private float startX;
    private float elapsedTime = 0f;
    private GameManager gameManager;
    private AudioManager audioManager;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        audioManager = FindObjectOfType<AudioManager>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        startX = transform.position.x;

        ApplyEnemyTypeSettings();
    }

    void ApplyEnemyTypeSettings()
    {
        switch (enemyType)
        {
            case EnemyType.Basic:
                health = 1;
                scoreValue = 100;
                moveSpeed = 3f;
                if (spriteRenderer != null)
                    spriteRenderer.color = Color.red;
                break;

            case EnemyType.Zigzag:
                health = 1;
                scoreValue = 150;
                moveSpeed = 4f;
                if (spriteRenderer != null)
                    spriteRenderer.color = Color.yellow;
                break;

            case EnemyType.Tank:
                health = 3;
                scoreValue = 300;
                moveSpeed = 1.5f;
                canShoot = true;
                if (spriteRenderer != null)
                    spriteRenderer.color = new Color(0.5f, 0f, 0.5f);
                break;
        }
    }

    void Update()
    {
        if (gameManager != null && !gameManager.IsGameActive())
            return;

        HandleMovement();
        HandleShooting();
        CheckBoundary();
    }

    void HandleMovement()
    {
        elapsedTime += Time.deltaTime;

        switch (enemyType)
        {
            case EnemyType.Basic:
                transform.position += Vector3.down * moveSpeed * Time.deltaTime;
                break;

            case EnemyType.Zigzag:
                float xOffset = Mathf.Sin(elapsedTime * zigzagFrequency) * zigzagAmplitude;
                transform.position = new Vector3(startX + xOffset, transform.position.y - moveSpeed * Time.deltaTime, transform.position.z);
                break;

            case EnemyType.Tank:
                transform.position += Vector3.down * moveSpeed * Time.deltaTime;
                break;
        }
    }

    void HandleShooting()
    {
        if (canShoot && bulletPrefab != null && Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + fireRate;
        }
    }

    void Shoot()
    {
        if (bulletPrefab != null)
        {
            GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
            BulletController bulletController = bullet.GetComponent<BulletController>();
            if (bulletController != null)
            {
                bulletController.Initialize(Vector2.down, bulletSpeed, false);
            }
        }
    }

    void CheckBoundary()
    {
        if (transform.position.y < destroyYPosition)
        {
            Destroy(gameObject);
        }
    }

    public void TakeDamage(int damage)
    {
        health -= damage;

        if (spriteRenderer != null)
        {
            StartCoroutine(FlashWhite());
        }

        if (health <= 0)
        {
            Die();
        }
    }

    System.Collections.IEnumerator FlashWhite()
    {
        Color originalColor = spriteRenderer.color;
        spriteRenderer.color = Color.white;
        yield return new WaitForSeconds(0.05f);
        if (spriteRenderer != null)
            spriteRenderer.color = originalColor;
    }

    void Die()
    {
        if (audioManager != null)
            audioManager.PlayExplosionSound();

        if (gameManager != null)
            gameManager.AddScore(scoreValue);

        SpawnManager spawnManager = FindObjectOfType<SpawnManager>();
        if (spawnManager != null)
            spawnManager.OnEnemyDestroyed();

        Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.TakeDamage(damageToPlayer);
            }
            Die();
        }
    }

    public void SetEnemyType(EnemyType type)
    {
        enemyType = type;
        ApplyEnemyTypeSettings();
    }
}

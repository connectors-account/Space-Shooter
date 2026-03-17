using UnityEngine;

/// <summary>
/// Base class for all enemy types. Handles health, scoring, and destruction.
/// </summary>
public class EnemyBase : MonoBehaviour
{
    [Header("Stats")]
    public int maxHealth = 1;
    public int scoreValue = 100;
    public float moveSpeed = 3f;

    [Header("Shooting")]
    public GameObject bulletPrefab;
    public float fireRate = 2f;
    public float bulletSpeed = 6f;
    public bool canShoot = true;

    [Header("Effects")]
    public GameObject explosionPrefab;
    public GameObject[] possibleDrops; // power-up prefabs
    [Range(0f, 1f)]
    public float dropChance = 0.15f;

    protected int currentHealth;
    protected float nextFireTime;
    protected Transform playerTransform;

    protected virtual void Start()
    {
        currentHealth = maxHealth;
        nextFireTime = Time.time + Random.Range(1f, fireRate);

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            playerTransform = player.transform;
    }

    protected virtual void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver) return;
        if (GameManager.Instance != null && GameManager.Instance.IsPaused) return;

        Move();
        TryShoot();
        CheckOffScreen();
    }

    protected virtual void Move()
    {
        // Default: move downward
        transform.Translate(Vector3.down * moveSpeed * Time.deltaTime, Space.World);
    }

    protected virtual void TryShoot()
    {
        if (!canShoot || bulletPrefab == null) return;
        if (Time.time < nextFireTime) return;

        Shoot();
        nextFireTime = Time.time + fireRate;
    }

    protected virtual void Shoot()
    {
        GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
        Bullet b = bullet.GetComponent<Bullet>();
        if (b != null)
        {
            b.isPlayerBullet = false;

            // Aim at player if visible, else shoot downward
            Vector2 dir = Vector2.down;
            if (playerTransform != null && playerTransform.gameObject.activeInHierarchy)
            {
                dir = (playerTransform.position - transform.position).normalized;
            }
            b.SetDirection(dir, bulletSpeed);
        }

        AudioManager.Instance?.PlaySFX("EnemyShoot");
    }

    public virtual void TakeDamage(int damage)
    {
        currentHealth -= damage;

        // Flash white briefly
        StartCoroutine(FlashWhite());

        if (currentHealth <= 0)
        {
            Die();
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
            if (sr != null) sr.color = original;
        }
    }

    protected virtual void Die()
    {
        // Spawn explosion
        if (explosionPrefab != null)
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);

        // Add score
        GameManager.Instance?.AddScore(scoreValue);

        // Chance to drop power-up
        TryDropPowerUp();

        // Notify spawner
        EnemySpawner.Instance?.OnEnemyDestroyed();

        AudioManager.Instance?.PlaySFX("EnemyExplosion");

        Destroy(gameObject);
    }

    void TryDropPowerUp()
    {
        if (possibleDrops == null || possibleDrops.Length == 0) return;
        if (Random.value > dropChance) return;

        // Filter out null entries
        GameObject drop = null;
        int attempts = 0;
        while (drop == null && attempts < possibleDrops.Length)
        {
            int idx = Random.Range(0, possibleDrops.Length);
            drop = possibleDrops[idx];
            attempts++;
        }

        if (drop != null)
        {
            Instantiate(drop, transform.position, Quaternion.identity);
        }
    }

    void CheckOffScreen()
    {
        Vector3 viewPos = Camera.main.WorldToViewportPoint(transform.position);
        // Destroy if too far below screen
        if (viewPos.y < -0.2f)
        {
            EnemySpawner.Instance?.OnEnemyDestroyed();
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.TakeDamage(1);
            }
            Die();
        }
    }
}

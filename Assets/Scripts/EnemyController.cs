using UnityEngine;

/// <summary>
/// Controls enemy ship behavior including movement, shooting, and health.
/// </summary>
public class EnemyController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float horizontalMovement = 0f;

    [Header("Shooting Settings")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float fireRate = 2f;
    [SerializeField] private bool canShoot = true;

    [Header("Health & Score")]
    [SerializeField] private int maxHealth = 1;
    [SerializeField] private int scoreValue = 100;

    [Header("Boundaries")]
    [SerializeField] private float destroyY = -6f;

    private int currentHealth;
    private float nextFireTime;

    public int MaxHealth => maxHealth;
    public int ScoreValue => scoreValue;

    private void Start()
    {
        currentHealth = maxHealth;
        nextFireTime = Time.time + Random.Range(0.5f, fireRate);
        
        // Add slight random horizontal movement variation
        horizontalMovement = Random.Range(-0.5f, 0.5f);
    }

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
            return;

        Move();
        HandleShooting();
        CheckBounds();
    }

    private void Move()
    {
        Vector3 movement = new Vector3(horizontalMovement, -1f, 0f).normalized * moveSpeed * Time.deltaTime;
        transform.Translate(movement, Space.World);
    }

    private void HandleShooting()
    {
        if (!canShoot || bulletPrefab == null)
            return;

        if (Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + fireRate + Random.Range(-0.5f, 0.5f);
        }
    }

    private void Shoot()
    {
        Vector3 spawnPosition = transform.position + Vector3.down * 0.5f;
        GameObject bullet = Instantiate(bulletPrefab, spawnPosition, Quaternion.identity);
        
        BulletController bulletController = bullet.GetComponent<BulletController>();
        if (bulletController != null)
        {
            bulletController.Initialize(false, Vector3.down);
        }
    }

    private void CheckBounds()
    {
        if (transform.position.y < destroyY)
        {
            Destroy(gameObject);
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        // Add score
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddScore(scoreValue);
        }

        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Handle collision with player bullet
        if (other.CompareTag("PlayerBullet"))
        {
            TakeDamage(1);
            Destroy(other.gameObject);
        }
    }

    /// <summary>
    /// Configure enemy properties (called by spawner)
    /// </summary>
    public void Configure(float speed, int health, int score, bool shooting)
    {
        moveSpeed = speed;
        maxHealth = health;
        currentHealth = health;
        scoreValue = score;
        canShoot = shooting;
    }
}

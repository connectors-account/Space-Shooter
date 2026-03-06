using UnityEngine;

/// <summary>
/// Controls enemy behavior including movement patterns, shooting, and death.
/// Supports multiple enemy types with different behaviors.
/// </summary>
public class EnemyController : MonoBehaviour
{
    public enum EnemyType
    {
        Basic,      // Moves straight down
        Zigzag,     // Moves in zigzag pattern
        Shooter     // Moves and shoots at player
    }
    
    [Header("Enemy Settings")]
    [SerializeField] private EnemyType enemyType = EnemyType.Basic;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private int scoreValue = 100;
    [SerializeField] private int collisionDamage = 20;
    
    [Header("Zigzag Settings")]
    [SerializeField] private float zigzagAmplitude = 2f;
    [SerializeField] private float zigzagFrequency = 2f;
    
    [Header("Shooter Settings")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float fireRate = 2f;
    [SerializeField] private float fireDelay = 1f;
    
    [Header("Boundaries")]
    [SerializeField] private float destroyY = -6f;
    
    // Components
    private HealthSystem healthSystem;
    private SpriteRenderer spriteRenderer;
    
    // State
    private float startX;
    private float timeAlive;
    private float nextFireTime;
    private bool canShoot;
    
    private void Awake()
    {
        healthSystem = GetComponent<HealthSystem>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        if (healthSystem != null)
        {
            healthSystem.OnDeath += OnEnemyDeath;
        }
    }
    
    private void Start()
    {
        startX = transform.position.x;
        nextFireTime = Time.time + fireDelay;
        
        // Set color based on type
        if (spriteRenderer != null)
        {
            switch (enemyType)
            {
                case EnemyType.Basic:
                    spriteRenderer.color = new Color(1f, 0.3f, 0.3f);
                    break;
                case EnemyType.Zigzag:
                    spriteRenderer.color = new Color(1f, 0.6f, 0.2f);
                    break;
                case EnemyType.Shooter:
                    spriteRenderer.color = new Color(0.8f, 0.2f, 0.8f);
                    break;
            }
        }
    }
    
    private void OnDestroy()
    {
        if (healthSystem != null)
        {
            healthSystem.OnDeath -= OnEnemyDeath;
        }
    }
    
    private void Update()
    {
        if (GameManager.Instance != null && !GameManager.Instance.IsPlaying) return;
        
        timeAlive += Time.deltaTime;
        
        switch (enemyType)
        {
            case EnemyType.Basic:
                MoveBasic();
                break;
            case EnemyType.Zigzag:
                MoveZigzag();
                break;
            case EnemyType.Shooter:
                MoveBasic();
                HandleShooting();
                break;
        }
        
        // Destroy if off screen
        if (transform.position.y < destroyY)
        {
            Destroy(gameObject);
        }
    }
    
    private void MoveBasic()
    {
        transform.position += Vector3.down * moveSpeed * Time.deltaTime;
    }
    
    private void MoveZigzag()
    {
        float newX = startX + Mathf.Sin(timeAlive * zigzagFrequency) * zigzagAmplitude;
        Vector3 newPos = transform.position;
        newPos.x = newX;
        newPos.y -= moveSpeed * Time.deltaTime;
        transform.position = newPos;
    }
    
    private void HandleShooting()
    {
        if (Time.time >= nextFireTime && PlayerController.Instance != null)
        {
            Fire();
            nextFireTime = Time.time + fireRate;
        }
    }
    
    private void Fire()
    {
        if (bulletPrefab == null) return;
        
        Vector3 spawnPos = transform.position + Vector3.down * 0.5f;
        GameObject bullet = Instantiate(bulletPrefab, spawnPos, Quaternion.Euler(0, 0, 180));
        
        BulletController bulletController = bullet.GetComponent<BulletController>();
        if (bulletController != null)
        {
            // Aim at player
            Vector2 direction = Vector2.down;
            if (PlayerController.Instance != null)
            {
                direction = (PlayerController.Instance.transform.position - transform.position).normalized;
            }
            bulletController.Initialize(direction, false);
        }
        
        AudioManager.Instance?.PlaySound("EnemyShoot");
    }
    
    private void OnEnemyDeath()
    {
        GameManager.Instance?.AddScore(scoreValue);
        GameManager.Instance?.EnemyDestroyed();
        
        // Chance to drop power-up
        float dropChance = Random.Range(0f, 1f);
        if (dropChance < 0.15f) // 15% chance
        {
            GameManager.Instance?.SpawnPowerUp(transform.position);
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Collision with player
        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            player.Health?.TakeDamage(collisionDamage);
            healthSystem?.TakeDamage(healthSystem.MaxHealth); // Destroy enemy on collision
        }
    }
    
    /// <summary>
    /// Initialize enemy with specific settings
    /// </summary>
    public void Initialize(EnemyType type, float speed, int health)
    {
        enemyType = type;
        moveSpeed = speed;
        
        if (healthSystem != null)
        {
            healthSystem.SetMaxHealth(health);
            healthSystem.SetFullHealth();
        }
    }
}

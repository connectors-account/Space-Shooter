using UnityEngine;

/// <summary>
/// Controls the player ship movement, shooting, and health management.
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float horizontalBoundary = 8f;
    [SerializeField] private float verticalBoundaryTop = 4f;
    [SerializeField] private float verticalBoundaryBottom = -4f;

    [Header("Shooting Settings")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireRate = 0.25f;

    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 3;

    private int currentHealth;
    private float nextFireTime = 0f;
    private bool canControl = true;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;

    private void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthUI();
    }

    private void Update()
    {
        if (!canControl || GameManager.Instance == null || GameManager.Instance.IsGameOver)
            return;

        HandleMovement();
        HandleShooting();
    }

    private void HandleMovement()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        // WASD and Arrow keys are both mapped to Horizontal/Vertical by default in Unity
        Vector3 movement = new Vector3(horizontalInput, verticalInput, 0f) * moveSpeed * Time.deltaTime;
        transform.Translate(movement, Space.World);

        // Clamp position to screen boundaries
        float clampedX = Mathf.Clamp(transform.position.x, -horizontalBoundary, horizontalBoundary);
        float clampedY = Mathf.Clamp(transform.position.y, verticalBoundaryBottom, verticalBoundaryTop);
        transform.position = new Vector3(clampedX, clampedY, transform.position.z);
    }

    private void HandleShooting()
    {
        if (Input.GetKey(KeyCode.Space) && Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + fireRate;
        }
    }

    private void Shoot()
    {
        if (bulletPrefab == null)
        {
            Debug.LogWarning("Bullet prefab not assigned to PlayerController!");
            return;
        }

        Vector3 spawnPosition = firePoint != null ? firePoint.position : transform.position + Vector3.up * 0.5f;
        GameObject bullet = Instantiate(bulletPrefab, spawnPosition, Quaternion.identity);
        
        BulletController bulletController = bullet.GetComponent<BulletController>();
        if (bulletController != null)
        {
            bulletController.Initialize(true, Vector3.up);
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);
        
        UpdateHealthUI();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void UpdateHealthUI()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateHealth(currentHealth, maxHealth);
        }
    }

    private void Die()
    {
        canControl = false;
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.GameOver();
        }

        // Visual feedback - disable renderer instead of destroying
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = false;
        }

        // Disable collider
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.enabled = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Handle collision with enemy
        if (other.CompareTag("Enemy"))
        {
            TakeDamage(1);
            
            EnemyController enemy = other.GetComponent<EnemyController>();
            if (enemy != null)
            {
                enemy.TakeDamage(enemy.MaxHealth); // Destroy enemy on collision
            }
        }
        // Handle collision with enemy bullet
        else if (other.CompareTag("EnemyBullet"))
        {
            TakeDamage(1);
            Destroy(other.gameObject);
        }
    }

    public void ResetPlayer()
    {
        currentHealth = maxHealth;
        canControl = true;
        transform.position = new Vector3(0f, -3f, 0f);
        
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
        }

        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.enabled = true;
        }

        UpdateHealthUI();
    }
}

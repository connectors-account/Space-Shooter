using UnityEngine;

/// <summary>
/// Controls the player ship: movement, shooting, health, and power-up effects.
/// Attach to the Player GameObject with a Rigidbody2D, BoxCollider2D, and SpriteRenderer.
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float padding = 0.5f;

    [Header("Shooting")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireRate = 0.2f;
    [SerializeField] private float bulletSpeed = 12f;

    [Header("Health")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private float invincibilityDuration = 1.5f;

    [Header("Audio")]
    [SerializeField] private string shootSFX = "PlayerShoot";
    [SerializeField] private string hurtSFX = "PlayerHurt";
    [SerializeField] private string deathSFX = "PlayerDeath";
    [SerializeField] private string powerUpSFX = "PowerUp";

    // Internal state
    private int currentHealth;
    private float nextFireTime;
    private bool isInvincible;
    private float invincibilityTimer;
    private int weaponLevel = 1; // 1 = single, 2 = double, 3 = triple
    private bool hasShield;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    private Camera mainCamera;
    private float minX, maxX, minY, maxY;

    // Public accessors
    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public bool HasShield => hasShield;
    public int WeaponLevel => weaponLevel;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        mainCamera = Camera.main;
        currentHealth = maxHealth;

        CalculateBounds();
    }

    private void Update()
    {
        if (GameManager.Instance != null && !GameManager.Instance.IsGameActive)
            return;

        HandleMovement();
        HandleShooting();
        HandleInvincibility();
    }

    /// <summary>
    /// Calculate screen boundaries for clamping player position.
    /// </summary>
    private void CalculateBounds()
    {
        Vector3 bottomLeft = mainCamera.ViewportToWorldPoint(new Vector3(0, 0, 0));
        Vector3 topRight = mainCamera.ViewportToWorldPoint(new Vector3(1, 1, 0));
        minX = bottomLeft.x + padding;
        maxX = topRight.x - padding;
        minY = bottomLeft.y + padding;
        maxY = topRight.y - padding;
    }

    /// <summary>
    /// Process WASD / Arrow key input and move the player within screen bounds.
    /// </summary>
    private void HandleMovement()
    {
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");

        Vector2 movement = new Vector2(horizontalInput, verticalInput).normalized;
        Vector3 newPosition = transform.position + (Vector3)(movement * moveSpeed * Time.deltaTime);

        // Clamp to screen bounds
        newPosition.x = Mathf.Clamp(newPosition.x, minX, maxX);
        newPosition.y = Mathf.Clamp(newPosition.y, minY, maxY);

        transform.position = newPosition;
    }

    /// <summary>
    /// Fire bullets when Space is pressed, respecting fire rate and weapon level.
    /// </summary>
    private void HandleShooting()
    {
        if (Input.GetKey(KeyCode.Space) && Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + fireRate;
            FireBullets();

            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX(shootSFX);
        }
    }

    /// <summary>
    /// Spawn bullets based on current weapon level.
    /// </summary>
    private void FireBullets()
    {
        if (bulletPrefab == null) return;

        Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position + Vector3.up * 0.5f;

        switch (weaponLevel)
        {
            case 1: // Single shot
                SpawnBullet(spawnPos, Vector2.up);
                break;

            case 2: // Double shot
                SpawnBullet(spawnPos + Vector3.left * 0.25f, Vector2.up);
                SpawnBullet(spawnPos + Vector3.right * 0.25f, Vector2.up);
                break;

            case 3: // Triple shot (spread)
                SpawnBullet(spawnPos, Vector2.up);
                SpawnBullet(spawnPos + Vector3.left * 0.3f, new Vector2(-0.15f, 1f).normalized);
                SpawnBullet(spawnPos + Vector3.right * 0.3f, new Vector2(0.15f, 1f).normalized);
                break;
        }
    }

    /// <summary>
    /// Instantiate a single bullet with the given direction.
    /// </summary>
    private void SpawnBullet(Vector3 position, Vector2 direction)
    {
        GameObject bullet = Instantiate(bulletPrefab, position, Quaternion.identity);
        BulletController bc = bullet.GetComponent<BulletController>();
        if (bc != null)
        {
            bc.Initialize(direction, bulletSpeed, true);
        }
    }

    /// <summary>
    /// Handle invincibility flashing effect.
    /// </summary>
    private void HandleInvincibility()
    {
        if (!isInvincible) return;

        invincibilityTimer -= Time.deltaTime;

        // Flash effect
        float alpha = Mathf.PingPong(Time.time * 10f, 1f) > 0.5f ? 1f : 0.3f;
        Color c = spriteRenderer.color;
        c.a = alpha;
        spriteRenderer.color = c;

        if (invincibilityTimer <= 0f)
        {
            isInvincible = false;
            Color color = spriteRenderer.color;
            color.a = 1f;
            spriteRenderer.color = color;
        }
    }

    /// <summary>
    /// Apply damage to the player. Shield absorbs one hit.
    /// </summary>
    public void TakeDamage(int damage)
    {
        if (isInvincible) return;

        if (hasShield)
        {
            hasShield = false;
            ActivateInvincibility();
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX(hurtSFX);
            return;
        }

        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(hurtSFX);

        if (UIManager.Instance != null)
            UIManager.Instance.UpdateHealthBar(currentHealth, maxHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            ActivateInvincibility();
        }
    }

    /// <summary>
    /// Start invincibility period after taking damage.
    /// </summary>
    private void ActivateInvincibility()
    {
        isInvincible = true;
        invincibilityTimer = invincibilityDuration;
    }

    /// <summary>
    /// Handle player death.
    /// </summary>
    private void Die()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(deathSFX);

        if (GameManager.Instance != null)
            GameManager.Instance.GameOver();

        gameObject.SetActive(false);
    }

    /// <summary>
    /// Apply a power-up effect to the player.
    /// </summary>
    public void ApplyPowerUp(PowerUpType type)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(powerUpSFX);

        switch (type)
        {
            case PowerUpType.WeaponUpgrade:
                weaponLevel = Mathf.Min(weaponLevel + 1, 3);
                break;

            case PowerUpType.Shield:
                hasShield = true;
                break;

            case PowerUpType.HealthRestore:
                currentHealth = Mathf.Min(currentHealth + 30, maxHealth);
                if (UIManager.Instance != null)
                    UIManager.Instance.UpdateHealthBar(currentHealth, maxHealth);
                break;
        }
    }

    /// <summary>
    /// Reset player to default state (for new game).
    /// </summary>
    public void ResetPlayer()
    {
        currentHealth = maxHealth;
        weaponLevel = 1;
        hasShield = false;
        isInvincible = false;
        gameObject.SetActive(true);
        transform.position = new Vector3(0, -3f, 0);

        Color c = spriteRenderer.color;
        c.a = 1f;
        spriteRenderer.color = c;

        if (UIManager.Instance != null)
            UIManager.Instance.UpdateHealthBar(currentHealth, maxHealth);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Handle collision with enemy bullets
        if (other.CompareTag("EnemyBullet"))
        {
            BulletController bullet = other.GetComponent<BulletController>();
            if (bullet != null)
            {
                TakeDamage(bullet.Damage);
                Destroy(other.gameObject);
            }
        }

        // Handle collision with enemies (contact damage)
        if (other.CompareTag("Enemy"))
        {
            TakeDamage(20);
        }

        // Handle collision with power-ups
        if (other.CompareTag("PowerUp"))
        {
            PowerUpController powerUp = other.GetComponent<PowerUpController>();
            if (powerUp != null)
            {
                ApplyPowerUp(powerUp.Type);
                Destroy(other.gameObject);
            }
        }
    }
}

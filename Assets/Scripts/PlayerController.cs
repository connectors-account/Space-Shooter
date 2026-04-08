using UnityEngine;

/// <summary>
/// Controls the player ship: movement, shooting, health, and power-up handling.
/// </summary>
public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }

    [Header("Movement")]
    public float moveSpeed = 8f;
    public float boundaryX = 8.5f;
    public float boundaryY = 4.5f;

    [Header("Shooting")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float fireRate = 0.25f;
    private float nextFireTime = 0f;

    [Header("Health")]
    public int maxHealth = 100;
    public int currentHealth;

    [Header("Power-Ups")]
    public bool hasShield = false;
    public bool hasRapidFire = false;
    public float rapidFireRate = 0.1f;
    public float powerUpDuration = 5f;
    private float shieldTimer = 0f;
    private float rapidFireTimer = 0f;

    [Header("Visual Feedback")]
    public GameObject shieldVisual;
    private SpriteRenderer spriteRenderer;
    private float flashTimer = 0f;
    private bool isFlashing = false;
    private float invincibilityTime = 0.5f;
    private float invincibilityTimer = 0f;
    private bool isInvincible = false;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (shieldVisual != null)
            shieldVisual.SetActive(false);

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateHealth(currentHealth, maxHealth);
        }
    }

    void Update()
    {
        if (GameManager.Instance != null && (!GameManager.Instance.isGameActive || GameManager.Instance.isPaused))
            return;

        HandleMovement();
        HandleShooting();
        UpdatePowerUps();
        UpdateInvincibility();
    }

    /// <summary>
    /// Handles player movement via WASD or Arrow keys.
    /// </summary>
    private void HandleMovement()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");

        Vector3 movement = new Vector3(moveX, moveY, 0f).normalized;
        transform.position += movement * moveSpeed * Time.deltaTime;

        // Clamp position within screen boundaries
        float clampedX = Mathf.Clamp(transform.position.x, -boundaryX, boundaryX);
        float clampedY = Mathf.Clamp(transform.position.y, -boundaryY, boundaryY);
        transform.position = new Vector3(clampedX, clampedY, 0f);
    }

    /// <summary>
    /// Handles shooting when Space is pressed.
    /// </summary>
    private void HandleShooting()
    {
        float currentFireRate = hasRapidFire ? rapidFireRate : fireRate;

        if (Input.GetKey(KeyCode.Space) && Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + currentFireRate;
            Shoot();
        }
    }

    /// <summary>
    /// Spawns a bullet from the fire point.
    /// </summary>
    private void Shoot()
    {
        if (bulletPrefab == null) return;

        Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position + Vector3.up * 0.5f;
        GameObject bullet = Instantiate(bulletPrefab, spawnPos, Quaternion.identity);

        BulletController bc = bullet.GetComponent<BulletController>();
        if (bc != null)
        {
            bc.isPlayerBullet = true;
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySound("PlayerShoot");
        }
    }

    /// <summary>
    /// Applies damage to the player.
    /// </summary>
    public void TakeDamage(int damage)
    {
        if (isInvincible) return;

        if (hasShield)
        {
            hasShield = false;
            shieldTimer = 0f;
            if (shieldVisual != null)
                shieldVisual.SetActive(false);

            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySound("ShieldBreak");

            StartInvincibility();
            return;
        }

        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateHealth(currentHealth, maxHealth);
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySound("PlayerHit");
        }

        StartInvincibility();
        StartFlash();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// Heals the player by the specified amount.
    /// </summary>
    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateHealth(currentHealth, maxHealth);
        }
    }

    /// <summary>
    /// Activates a power-up effect.
    /// </summary>
    public void ActivatePowerUp(PowerUpType type)
    {
        switch (type)
        {
            case PowerUpType.Health:
                Heal(30);
                break;

            case PowerUpType.RapidFire:
                hasRapidFire = true;
                rapidFireTimer = powerUpDuration;
                break;

            case PowerUpType.Shield:
                hasShield = true;
                shieldTimer = powerUpDuration;
                if (shieldVisual != null)
                    shieldVisual.SetActive(true);
                break;
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySound("PowerUp");
        }
    }

    /// <summary>
    /// Updates active power-up timers.
    /// </summary>
    private void UpdatePowerUps()
    {
        if (hasRapidFire)
        {
            rapidFireTimer -= Time.deltaTime;
            if (rapidFireTimer <= 0f)
            {
                hasRapidFire = false;
            }
        }

        if (hasShield)
        {
            shieldTimer -= Time.deltaTime;
            if (shieldTimer <= 0f)
            {
                hasShield = false;
                if (shieldVisual != null)
                    shieldVisual.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Makes the player temporarily invincible after taking a hit.
    /// </summary>
    private void StartInvincibility()
    {
        isInvincible = true;
        invincibilityTimer = invincibilityTime;
    }

    private void UpdateInvincibility()
    {
        if (!isInvincible) return;

        invincibilityTimer -= Time.deltaTime;

        // Flash effect during invincibility
        if (isFlashing && spriteRenderer != null)
        {
            flashTimer -= Time.deltaTime;
            float alpha = Mathf.PingPong(Time.time * 10f, 1f) > 0.5f ? 1f : 0.3f;
            Color c = spriteRenderer.color;
            spriteRenderer.color = new Color(c.r, c.g, c.b, alpha);
        }

        if (invincibilityTimer <= 0f)
        {
            isInvincible = false;
            isFlashing = false;
            if (spriteRenderer != null)
            {
                Color c = spriteRenderer.color;
                spriteRenderer.color = new Color(c.r, c.g, c.b, 1f);
            }
        }
    }

    private void StartFlash()
    {
        isFlashing = true;
        flashTimer = invincibilityTime;
    }

    /// <summary>
    /// Handles player death.
    /// </summary>
    private void Die()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySound("PlayerDeath");
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.GameOver();
        }

        Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("EnemyBullet"))
        {
            TakeDamage(10);
            Destroy(other.gameObject);
        }
        else if (other.CompareTag("Enemy"))
        {
            TakeDamage(20);
        }
        else if (other.CompareTag("PowerUp"))
        {
            PowerUpController powerUp = other.GetComponent<PowerUpController>();
            if (powerUp != null)
            {
                ActivatePowerUp(powerUp.powerUpType);
            }
            Destroy(other.gameObject);
        }
    }
}

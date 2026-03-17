using UnityEngine;

/// <summary>
/// Handles player ship movement, shooting, health, and power-up states.
/// Attach to the Player GameObject.
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 8f;
    public float padding = 0.5f; // screen edge padding

    [Header("Shooting")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float fireRate = 0.25f;
    public float bulletSpeed = 12f;

    [Header("Health")]
    public int maxHealth = 5;
    public float invincibilityDuration = 1.5f;

    [Header("Power-ups")]
    public float rapidFireRate = 0.1f;
    public float powerUpDuration = 8f;

    [Header("Effects")]
    public GameObject explosionPrefab;
    public GameObject shieldVisualPrefab;

    // Internal state
    private int currentHealth;
    private float nextFireTime;
    private float currentFireRate;
    private bool isInvincible;
    private float invincibilityTimer;
    private bool hasShield;
    private bool hasRapidFire;
    private float rapidFireTimer;
    private GameObject shieldVisual;
    private SpriteRenderer spriteRenderer;
    private Camera mainCamera;
    private Vector2 screenBounds;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public bool HasShield => hasShield;

    void Start()
    {
        currentHealth = maxHealth;
        currentFireRate = fireRate;
        spriteRenderer = GetComponent<SpriteRenderer>();
        mainCamera = Camera.main;

        // Calculate screen bounds in world coordinates
        screenBounds = mainCamera.ScreenToWorldPoint(
            new Vector3(Screen.width, Screen.height, mainCamera.transform.position.z));

        // Create fire point if not assigned
        if (firePoint == null)
        {
            GameObject fp = new GameObject("FirePoint");
            fp.transform.SetParent(transform);
            fp.transform.localPosition = new Vector3(0, 0.6f, 0);
            firePoint = fp.transform;
        }
    }

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver) return;
        if (GameManager.Instance != null && GameManager.Instance.IsPaused) return;

        HandleMovement();
        HandleShooting();
        HandleInvincibility();
        HandlePowerUpTimers();
    }

    void HandleMovement()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 movement = new Vector3(h, v, 0).normalized * moveSpeed * Time.deltaTime;
        transform.position += movement;

        // Clamp position to screen bounds
        float clampX = Mathf.Clamp(transform.position.x, -Mathf.Abs(screenBounds.x) + padding,
                                     Mathf.Abs(screenBounds.x) - padding);
        float clampY = Mathf.Clamp(transform.position.y, -Mathf.Abs(screenBounds.y) + padding,
                                     Mathf.Abs(screenBounds.y) - padding);
        transform.position = new Vector3(clampX, clampY, 0);
    }

    void HandleShooting()
    {
        if (Input.GetKey(KeyCode.Space) && Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + currentFireRate;
        }
    }

    void Shoot()
    {
        if (bulletPrefab == null) return;

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        Bullet bulletScript = bullet.GetComponent<Bullet>();
        if (bulletScript != null)
        {
            bulletScript.SetDirection(Vector2.up, bulletSpeed);
            bulletScript.isPlayerBullet = true;
        }

        AudioManager.Instance?.PlaySFX("PlayerShoot");
    }

    void HandleInvincibility()
    {
        if (!isInvincible) return;

        invincibilityTimer -= Time.deltaTime;

        // Flash effect
        if (spriteRenderer != null)
        {
            float alpha = Mathf.PingPong(Time.time * 10f, 1f) > 0.5f ? 1f : 0.3f;
            Color c = spriteRenderer.color;
            c.a = alpha;
            spriteRenderer.color = c;
        }

        if (invincibilityTimer <= 0f)
        {
            isInvincible = false;
            if (spriteRenderer != null)
            {
                Color c = spriteRenderer.color;
                c.a = 1f;
                spriteRenderer.color = c;
            }
        }
    }

    void HandlePowerUpTimers()
    {
        if (hasRapidFire)
        {
            rapidFireTimer -= Time.deltaTime;
            if (rapidFireTimer <= 0f)
            {
                hasRapidFire = false;
                currentFireRate = fireRate;
            }
        }
    }

    public void TakeDamage(int damage)
    {
        if (isInvincible || hasShield)
        {
            if (hasShield)
            {
                hasShield = false;
                if (shieldVisual != null) Destroy(shieldVisual);
                AudioManager.Instance?.PlaySFX("ShieldBreak");
            }
            return;
        }

        currentHealth -= damage;
        AudioManager.Instance?.PlaySFX("PlayerHit");

        // Trigger invincibility frames
        isInvincible = true;
        invincibilityTimer = invincibilityDuration;

        // Update HUD
        if (HUDManager.Instance != null)
            HUDManager.Instance.UpdateHealth(currentHealth, maxHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        if (explosionPrefab != null)
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);

        AudioManager.Instance?.PlaySFX("PlayerExplosion");
        GameManager.Instance?.GameOver();
        gameObject.SetActive(false);
    }

    // === Power-up Methods ===

    public void ActivateRapidFire()
    {
        hasRapidFire = true;
        currentFireRate = rapidFireRate;
        rapidFireTimer = powerUpDuration;
        AudioManager.Instance?.PlaySFX("PowerUp");
    }

    public void ActivateShield()
    {
        hasShield = true;
        AudioManager.Instance?.PlaySFX("PowerUp");

        if (shieldVisual != null) Destroy(shieldVisual);

        if (shieldVisualPrefab != null)
        {
            shieldVisual = Instantiate(shieldVisualPrefab, transform);
            shieldVisual.transform.localPosition = Vector3.zero;
        }
    }

    public void RestoreHealth(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        AudioManager.Instance?.PlaySFX("PowerUp");

        if (HUDManager.Instance != null)
            HUDManager.Instance.UpdateHealth(currentHealth, maxHealth);
    }
}

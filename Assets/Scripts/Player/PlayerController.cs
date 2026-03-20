using UnityEngine;

/// <summary>
/// Controls the player ship: movement, shooting, health, shields, and weapon upgrades.
/// Attach to the Player GameObject.
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float padding = 0.5f;

    [Header("Shooting")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireRate = 0.25f;
    [SerializeField] private float bulletSpeed = 12f;

    [Header("Health")]
    [SerializeField] private int maxHealth = 5;
    [SerializeField] private float invincibilityDuration = 1.5f;

    [Header("Visual Feedback")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color shieldColor = new Color(0.3f, 0.8f, 1f, 1f);

    // State
    private int currentHealth;
    private int weaponLevel = 1;
    private bool hasShield = false;
    private bool isInvincible = false;
    private float nextFireTime = 0f;
    private float invincibilityTimer = 0f;
    private Color originalColor;

    // Screen bounds
    private float minX, maxX, minY, maxY;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public int WeaponLevel => weaponLevel;
    public bool HasShield => hasShield;

    private void Start()
    {
        currentHealth = maxHealth;

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer != null ? spriteRenderer.color : Color.white;

        CalculateScreenBounds();

        if (firePoint == null)
        {
            // Create a fire point above the player if not assigned
            GameObject fp = new GameObject("FirePoint");
            fp.transform.SetParent(transform);
            fp.transform.localPosition = new Vector3(0f, 0.6f, 0f);
            firePoint = fp.transform;
        }
    }

    private void Update()
    {
        HandleMovement();
        HandleShooting();
        HandleInvincibility();
    }

    /// <summary>
    /// Handles WASD and Arrow key movement, clamped to screen bounds.
    /// </summary>
    private void HandleMovement()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 movement = new Vector3(horizontal, vertical, 0f).normalized;
        transform.position += movement * moveSpeed * Time.deltaTime;

        // Clamp to screen
        float clampedX = Mathf.Clamp(transform.position.x, minX, maxX);
        float clampedY = Mathf.Clamp(transform.position.y, minY, maxY);
        transform.position = new Vector3(clampedX, clampedY, 0f);
    }

    /// <summary>
    /// Fires bullets when Space is held, respecting fire rate and weapon level.
    /// </summary>
    private void HandleShooting()
    {
        if (Input.GetKey(KeyCode.Space) && Time.time >= nextFireTime)
        {
            Fire();
            nextFireTime = Time.time + fireRate;
        }
    }

    /// <summary>
    /// Spawns bullets based on current weapon level.
    /// Level 1: single shot. Level 2: dual shot. Level 3: triple spread.
    /// </summary>
    private void Fire()
    {
        if (bulletPrefab == null) return;

        AudioManager.Instance?.PlaySFX("PlayerShoot");

        switch (weaponLevel)
        {
            case 1:
                SpawnBullet(firePoint.position, Vector2.up);
                break;
            case 2:
                SpawnBullet(firePoint.position + Vector3.left * 0.2f, Vector2.up);
                SpawnBullet(firePoint.position + Vector3.right * 0.2f, Vector2.up);
                break;
            default: // Level 3+
                SpawnBullet(firePoint.position, Vector2.up);
                SpawnBullet(firePoint.position + Vector3.left * 0.25f, new Vector2(-0.15f, 1f).normalized);
                SpawnBullet(firePoint.position + Vector3.right * 0.25f, new Vector2(0.15f, 1f).normalized);
                break;
        }
    }

    private void SpawnBullet(Vector3 position, Vector2 direction)
    {
        GameObject bullet = Instantiate(bulletPrefab, position, Quaternion.identity);
        BulletController bc = bullet.GetComponent<BulletController>();
        if (bc != null)
        {
            bc.Initialize(direction, bulletSpeed, 1, true);
        }
    }

    /// <summary>
    /// Flashes the player sprite during invincibility.
    /// </summary>
    private void HandleInvincibility()
    {
        if (!isInvincible) return;

        invincibilityTimer -= Time.deltaTime;
        if (invincibilityTimer <= 0f)
        {
            isInvincible = false;
            if (spriteRenderer != null)
                spriteRenderer.color = hasShield ? shieldColor : originalColor;
        }
        else
        {
            // Flash effect
            if (spriteRenderer != null)
            {
                float alpha = Mathf.PingPong(Time.time * 10f, 1f) > 0.5f ? 1f : 0.3f;
                Color c = spriteRenderer.color;
                c.a = alpha;
                spriteRenderer.color = c;
            }
        }
    }

    /// <summary>
    /// Called when the player takes damage. Shield absorbs one hit.
    /// </summary>
    public void TakeDamage(int damage)
    {
        if (isInvincible) return;

        if (hasShield)
        {
            hasShield = false;
            if (spriteRenderer != null)
                spriteRenderer.color = originalColor;
            AudioManager.Instance?.PlaySFX("ShieldBreak");
            StartInvincibility();
            return;
        }

        currentHealth -= damage;
        AudioManager.Instance?.PlaySFX("PlayerHit");

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
        else
        {
            StartInvincibility();
        }

        // Update UI
        UIManager uiManager = FindObjectOfType<UIManager>();
        if (uiManager != null) uiManager.UpdateHealthDisplay(currentHealth, maxHealth);
    }

    private void StartInvincibility()
    {
        isInvincible = true;
        invincibilityTimer = invincibilityDuration;
    }

    private void Die()
    {
        AudioManager.Instance?.PlaySFX("PlayerDeath");
        GameManager.Instance?.GameOver();
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Power-up: Restore health.
    /// </summary>
    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        AudioManager.Instance?.PlaySFX("PowerUp");
        UIManager uiManager = FindObjectOfType<UIManager>();
        if (uiManager != null) uiManager.UpdateHealthDisplay(currentHealth, maxHealth);
    }

    /// <summary>
    /// Power-up: Upgrade weapon (max level 3).
    /// </summary>
    public void UpgradeWeapon()
    {
        weaponLevel = Mathf.Min(weaponLevel + 1, 3);
        fireRate = Mathf.Max(0.1f, fireRate - 0.05f);
        AudioManager.Instance?.PlaySFX("PowerUp");
    }

    /// <summary>
    /// Power-up: Activate shield.
    /// </summary>
    public void ActivateShield()
    {
        hasShield = true;
        if (spriteRenderer != null)
            spriteRenderer.color = shieldColor;
        AudioManager.Instance?.PlaySFX("PowerUp");
    }

    /// <summary>
    /// Resets player state for a new game.
    /// </summary>
    public void ResetPlayer()
    {
        currentHealth = maxHealth;
        weaponLevel = 1;
        hasShield = false;
        isInvincible = false;
        fireRate = 0.25f;
        transform.position = new Vector3(0f, -3.5f, 0f);
        if (spriteRenderer != null)
            spriteRenderer.color = originalColor;
        gameObject.SetActive(true);
    }

    private void CalculateScreenBounds()
    {
        Camera cam = Camera.main;
        if (cam == null) return;
        float camHeight = cam.orthographicSize;
        float camWidth = camHeight * cam.aspect;
        minX = -camWidth + padding;
        maxX = camWidth - padding;
        minY = -camHeight + padding;
        maxY = camHeight - padding;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Collision with enemy
        if (other.CompareTag("Enemy"))
        {
            TakeDamage(1);
            EnemyController enemy = other.GetComponent<EnemyController>();
            if (enemy != null) enemy.TakeDamage(999);
        }

        // Collision with enemy bullet
        if (other.CompareTag("EnemyBullet"))
        {
            TakeDamage(1);
            Destroy(other.gameObject);
        }
    }
}

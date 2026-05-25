using UnityEngine;

/// <summary>
/// Controls the player ship: movement, shooting, health, and power-up effects.
/// Attach to the Player GameObject with a Rigidbody2D and Collider2D.
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float screenPadding = 0.5f;

    [Header("Shooting")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireRate = 0.25f;
    [SerializeField] private float bulletSpeed = 12f;

    [Header("Health")]
    [SerializeField] private int maxHealth = 5;
    private int currentHealth;

    [Header("Visual Feedback")]
    [SerializeField] private float invincibilityDuration = 1.5f;
    private float invincibilityTimer;
    private bool isInvincible;
    private SpriteRenderer spriteRenderer;

    // Power-up states
    private bool hasShield;
    private float rapidFireTimer;
    private float multiShotTimer;
    private bool isRapidFire;
    private bool isMultiShot;
    private float rapidFireRate = 0.1f;

    // Internal state
    private float nextFireTime;
    private Rigidbody2D rb;
    private Camera mainCamera;
    private Vector2 screenBounds;

    // Shield visual
    private GameObject shieldVisual;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        mainCamera = Camera.main;
    }

    private void Start()
    {
        currentHealth = maxHealth;

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

        // Create shield visual
        CreateShieldVisual();

        // Notify GameManager of max health
        if (GameManager.Instance != null)
            GameManager.Instance.SetPlayerMaxHealth(maxHealth);
    }

    private void Update()
    {
        HandleMovement();
        HandleShooting();
        HandleInvincibility();
        HandlePowerUpTimers();
    }

    /// <summary>
    /// Handles WASD/Arrow key movement with screen clamping.
    /// </summary>
    private void HandleMovement()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector2 movement = new Vector2(horizontal, vertical).normalized;
        rb.velocity = movement * moveSpeed;

        // Clamp position to screen bounds
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, -screenBounds.x + screenPadding, screenBounds.x - screenPadding);
        pos.y = Mathf.Clamp(pos.y, -screenBounds.y + screenPadding, screenBounds.y - screenPadding);
        transform.position = pos;
    }

    /// <summary>
    /// Fires bullets when Space is pressed, respecting fire rate and power-ups.
    /// </summary>
    private void HandleShooting()
    {
        if (Input.GetKey(KeyCode.Space) && Time.time >= nextFireTime)
        {
            float currentFireRate = isRapidFire ? rapidFireRate : fireRate;
            nextFireTime = Time.time + currentFireRate;

            if (isMultiShot)
            {
                FireMultiShot();
            }
            else
            {
                FireBullet(firePoint.position, Vector2.up);
            }

            AudioManager.Instance?.PlaySFX("PlayerShoot");
        }
    }

    /// <summary>
    /// Fires a single bullet in the given direction.
    /// </summary>
    private void FireBullet(Vector3 position, Vector2 direction)
    {
        if (bulletPrefab == null) return;

        GameObject bullet = Instantiate(bulletPrefab, position, Quaternion.identity);
        BulletController bc = bullet.GetComponent<BulletController>();
        if (bc != null)
        {
            bc.Initialize(direction, bulletSpeed, true);
        }
    }

    /// <summary>
    /// Fires three bullets in a spread pattern.
    /// </summary>
    private void FireMultiShot()
    {
        FireBullet(firePoint.position, Vector2.up);
        FireBullet(firePoint.position, new Vector2(-0.25f, 1f).normalized);
        FireBullet(firePoint.position, new Vector2(0.25f, 1f).normalized);
    }

    /// <summary>
    /// Handles invincibility flashing effect after taking damage.
    /// </summary>
    private void HandleInvincibility()
    {
        if (!isInvincible) return;

        invincibilityTimer -= Time.deltaTime;
        if (invincibilityTimer <= 0)
        {
            isInvincible = false;
            if (spriteRenderer != null)
            {
                Color c = spriteRenderer.color;
                c.a = 1f;
                spriteRenderer.color = c;
            }
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
    /// Decrements power-up timers and deactivates expired power-ups.
    /// </summary>
    private void HandlePowerUpTimers()
    {
        if (isRapidFire)
        {
            rapidFireTimer -= Time.deltaTime;
            if (rapidFireTimer <= 0) isRapidFire = false;
        }
        if (isMultiShot)
        {
            multiShotTimer -= Time.deltaTime;
            if (multiShotTimer <= 0) isMultiShot = false;
        }
    }

    /// <summary>
    /// Called when the player takes damage. Returns remaining health.
    /// </summary>
    public void TakeDamage(int damage)
    {
        if (isInvincible) return;

        // Shield absorbs one hit
        if (hasShield)
        {
            hasShield = false;
            if (shieldVisual != null) shieldVisual.SetActive(false);
            AudioManager.Instance?.PlaySFX("ShieldBreak");
            StartInvincibility();
            return;
        }

        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);

        AudioManager.Instance?.PlaySFX("PlayerHit");

        if (GameManager.Instance != null)
            GameManager.Instance.UpdatePlayerHealth(currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            StartInvincibility();
        }
    }

    /// <summary>
    /// Activates invincibility frames after taking damage.
    /// </summary>
    private void StartInvincibility()
    {
        isInvincible = true;
        invincibilityTimer = invincibilityDuration;
    }

    /// <summary>
    /// Handles player death: notifies GameManager and disables the player.
    /// </summary>
    private void Die()
    {
        AudioManager.Instance?.PlaySFX("PlayerDeath");
        if (GameManager.Instance != null)
            GameManager.Instance.OnPlayerDeath();
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Applies a power-up effect to the player.
    /// </summary>
    public void ApplyPowerUp(PowerUpType type, float duration)
    {
        switch (type)
        {
            case PowerUpType.Shield:
                hasShield = true;
                if (shieldVisual != null) shieldVisual.SetActive(true);
                break;

            case PowerUpType.RapidFire:
                isRapidFire = true;
                rapidFireTimer = duration;
                break;

            case PowerUpType.MultiShot:
                isMultiShot = true;
                multiShotTimer = duration;
                break;

            case PowerUpType.Health:
                currentHealth = Mathf.Min(currentHealth + 1, maxHealth);
                if (GameManager.Instance != null)
                    GameManager.Instance.UpdatePlayerHealth(currentHealth);
                break;
        }
    }

    /// <summary>
    /// Creates a child object to visually represent the shield power-up.
    /// </summary>
    private void CreateShieldVisual()
    {
        shieldVisual = new GameObject("ShieldVisual");
        shieldVisual.transform.SetParent(transform);
        shieldVisual.transform.localPosition = Vector3.zero;
        SpriteRenderer sr = shieldVisual.AddComponent<SpriteRenderer>();
        sr.sprite = CreateCircleSprite();
        sr.color = new Color(0.3f, 0.7f, 1f, 0.35f);
        sr.sortingOrder = 5;
        shieldVisual.transform.localScale = Vector3.one * 2.5f;
        shieldVisual.SetActive(false);
    }

    /// <summary>
    /// Programmatically creates a circular sprite for the shield.
    /// </summary>
    private Sprite CreateCircleSprite()
    {
        int size = 64;
        Texture2D tex = new Texture2D(size, size);
        Color[] colors = new Color[size * size];
        float center = size / 2f;
        float radius = size / 2f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                colors[y * size + x] = dist <= radius ? Color.white : Color.clear;
            }
        }

        tex.SetPixels(colors);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 64f);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Collision with enemy
        if (other.CompareTag("Enemy"))
        {
            TakeDamage(1);
        }
    }

    public int GetCurrentHealth() => currentHealth;
    public int GetMaxHealth() => maxHealth;
    public bool HasShield() => hasShield;
}

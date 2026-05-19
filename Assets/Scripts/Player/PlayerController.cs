using UnityEngine;

/// <summary>
/// Controls the player spaceship movement, shooting, and health.
/// Attach to the Player GameObject with a Rigidbody2D and Collider2D.
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float padding = 0.5f;

    [Header("Shooting Settings")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireRate = 0.3f;
    [SerializeField] private float rapidFireRate = 0.1f;
    [SerializeField] private AudioClip shootSound;

    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 5;
    [SerializeField] private float invincibilityDuration = 2f;
    [SerializeField] private AudioClip hitSound;
    [SerializeField] private AudioClip deathSound;

    [Header("Shield Settings")]
    [SerializeField] private GameObject shieldVisual;
    [SerializeField] private float shieldDuration = 8f;

    [Header("Rapid Fire Settings")]
    [SerializeField] private float rapidFireDuration = 6f;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private AudioSource audioSource;

    private int currentHealth;
    private float nextFireTime;
    private bool isInvincible;
    private float invincibilityTimer;
    private bool hasShield;
    private float shieldTimer;
    private bool hasRapidFire;
    private float rapidFireTimer;

    private float minX, maxX, minY, maxY;
    private bool isDead;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public bool HasShield => hasShield;
    public bool HasRapidFire => hasRapidFire;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    private void Start()
    {
        currentHealth = maxHealth;
        isDead = false;
        CalculateBounds();

        if (shieldVisual != null)
            shieldVisual.SetActive(false);

        GameManager.Instance.UpdateHealthUI(currentHealth, maxHealth);
    }

    private void Update()
    {
        if (isDead || GameManager.Instance.IsGamePaused) return;

        HandleMovement();
        HandleShooting();
        HandlePowerUpTimers();
        HandleInvincibility();
    }

    /// <summary>
    /// Calculates screen boundaries to keep the player within the visible area.
    /// </summary>
    private void CalculateBounds()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        minX = cam.ViewportToWorldPoint(Vector3.zero).x + padding;
        maxX = cam.ViewportToWorldPoint(Vector3.right).x - padding;
        minY = cam.ViewportToWorldPoint(Vector3.zero).y + padding;
        maxY = cam.ViewportToWorldPoint(Vector3.up).y - padding;
    }

    /// <summary>
    /// Handles WASD / Arrow key movement with screen clamping.
    /// </summary>
    private void HandleMovement()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector2 movement = new Vector2(horizontal, vertical).normalized * moveSpeed;
        rb.linearVelocity = movement;

        // Clamp position to screen bounds
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.y = Mathf.Clamp(pos.y, minY, maxY);
        transform.position = pos;
    }

    /// <summary>
    /// Handles shooting when Space is pressed, respecting fire rate.
    /// </summary>
    private void HandleShooting()
    {
        if (Input.GetKey(KeyCode.Space) && Time.time >= nextFireTime)
        {
            float currentFireRate = hasRapidFire ? rapidFireRate : fireRate;
            nextFireTime = Time.time + currentFireRate;
            Shoot();
        }
    }

    /// <summary>
    /// Instantiates a bullet at the fire point.
    /// </summary>
    private void Shoot()
    {
        if (bulletPrefab == null) return;

        Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position + Vector3.up * 0.6f;
        Instantiate(bulletPrefab, spawnPos, Quaternion.identity);

        if (shootSound != null && audioSource != null)
            audioSource.PlayOneShot(shootSound, 0.5f);
    }

    /// <summary>
    /// Manages power-up duration timers.
    /// </summary>
    private void HandlePowerUpTimers()
    {
        if (hasShield)
        {
            shieldTimer -= Time.deltaTime;
            if (shieldTimer <= 0)
                DeactivateShield();
        }

        if (hasRapidFire)
        {
            rapidFireTimer -= Time.deltaTime;
            if (rapidFireTimer <= 0)
                hasRapidFire = false;
        }
    }

    /// <summary>
    /// Handles the invincibility blink effect after taking damage.
    /// </summary>
    private void HandleInvincibility()
    {
        if (!isInvincible) return;

        invincibilityTimer -= Time.deltaTime;

        // Blink effect during invincibility
        float alpha = Mathf.PingPong(Time.time * 10f, 1f) > 0.5f ? 1f : 0.3f;
        Color c = spriteRenderer.color;
        c.a = alpha;
        spriteRenderer.color = c;

        if (invincibilityTimer <= 0)
        {
            isInvincible = false;
            Color color = spriteRenderer.color;
            color.a = 1f;
            spriteRenderer.color = color;
        }
    }

    /// <summary>
    /// Applies damage to the player. Returns true if player died.
    /// </summary>
    public bool TakeDamage(int damage)
    {
        if (isDead || isInvincible) return false;

        if (hasShield)
        {
            DeactivateShield();
            StartInvincibility();
            return false;
        }

        currentHealth -= damage;
        GameManager.Instance.UpdateHealthUI(currentHealth, maxHealth);

        if (hitSound != null && audioSource != null)
            audioSource.PlayOneShot(hitSound, 0.7f);

        if (currentHealth <= 0)
        {
            Die();
            return true;
        }

        StartInvincibility();
        return false;
    }

    /// <summary>
    /// Makes the player temporarily invincible after taking damage.
    /// </summary>
    private void StartInvincibility()
    {
        isInvincible = true;
        invincibilityTimer = invincibilityDuration;
    }

    /// <summary>
    /// Handles player death: plays effects and triggers game over.
    /// </summary>
    private void Die()
    {
        isDead = true;

        if (deathSound != null && audioSource != null)
            audioSource.PlayOneShot(deathSound);

        ExplosionEffect.SpawnExplosion(transform.position, 1.5f);
        spriteRenderer.enabled = false;
        rb.linearVelocity = Vector2.zero;
        GetComponent<Collider2D>().enabled = false;

        GameManager.Instance.OnPlayerDeath();
    }

    /// <summary>
    /// Activates the shield power-up.
    /// </summary>
    public void ActivateShield()
    {
        hasShield = true;
        shieldTimer = shieldDuration;
        if (shieldVisual != null)
            shieldVisual.SetActive(true);
    }

    /// <summary>
    /// Deactivates the shield.
    /// </summary>
    private void DeactivateShield()
    {
        hasShield = false;
        if (shieldVisual != null)
            shieldVisual.SetActive(false);
    }

    /// <summary>
    /// Activates the rapid fire power-up.
    /// </summary>
    public void ActivateRapidFire()
    {
        hasRapidFire = true;
        rapidFireTimer = rapidFireDuration;
    }

    /// <summary>
    /// Restores health by the given amount.
    /// </summary>
    public void RestoreHealth(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        GameManager.Instance.UpdateHealthUI(currentHealth, maxHealth);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isDead) return;

        // Handle collision with enemy bullets
        if (other.CompareTag("EnemyBullet"))
        {
            Bullet bullet = other.GetComponent<Bullet>();
            int damage = bullet != null ? bullet.Damage : 1;
            TakeDamage(damage);
            Destroy(other.gameObject);
        }

        // Handle collision with enemies directly
        if (other.CompareTag("Enemy"))
        {
            TakeDamage(1);
        }
    }
}

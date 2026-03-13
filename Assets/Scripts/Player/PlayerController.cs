using UnityEngine;

/// <summary>
/// Handles player ship movement, shooting, health, respawn, and power-up states.
/// Attached to the Player ship GameObject.
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 8f;
    public float smoothing = 0.05f;

    [Header("Health")]
    public int maxHealth = 100;
    private int currentHealth;

    [Header("Shooting")]
    public Transform firePoint;
    public Transform firePointLeft;
    public Transform firePointRight;
    public float fireRate = 0.2f;
    private float nextFireTime = 0f;

    [Header("Weapon Levels")]
    private int weaponLevel = 1; // 1 = single, 2 = double, 3 = triple spread
    private float weaponLevelTimer = 0f;
    private float weaponLevelDuration = 10f;

    [Header("Shield")]
    public GameObject shieldVisual;
    private bool hasShield = false;
    private float shieldTimer = 0f;
    private float shieldDuration = 8f;

    [Header("Invincibility")]
    private bool isInvincible = false;
    private float invincibilityTimer = 0f;
    private SpriteRenderer spriteRenderer;
    private float blinkTimer = 0f;

    [Header("References")]
    public GameObject bulletPrefab;

    // Events
    public System.Action<int, int> OnHealthChanged; // current, max
    public System.Action OnPlayerDeath;

    private Vector2 velocity = Vector2.zero;
    private bool isDead = false;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public bool HasShield => hasShield;

    private void Start()
    {
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (shieldVisual != null)
            shieldVisual.SetActive(false);

        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    private void Update()
    {
        if (isDead || GameManager.Instance == null || !GameManager.Instance.isGameActive) return;
        if (GameManager.Instance.isPaused) return;

        HandleMovement();
        HandleShooting();
        UpdateTimers();
    }

    private void HandleMovement()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector2 targetVelocity = new Vector2(horizontal, vertical).normalized * moveSpeed;
        Vector2 currentVelocity = Vector2.SmoothDamp(
            new Vector2(transform.position.x, transform.position.y),
            new Vector2(transform.position.x, transform.position.y) + targetVelocity * Time.deltaTime,
            ref velocity,
            smoothing
        );

        transform.position = new Vector3(currentVelocity.x, currentVelocity.y, 0f);

        // Clamp to screen bounds
        if (GameBounds.Instance != null)
        {
            float clampedX = Mathf.Clamp(transform.position.x, GameBounds.Instance.minX, GameBounds.Instance.maxX);
            float clampedY = Mathf.Clamp(transform.position.y, GameBounds.Instance.minY, GameBounds.Instance.maxY);
            transform.position = new Vector3(clampedX, clampedY, 0f);
        }
    }

    private void HandleShooting()
    {
        if (Input.GetKey(KeyCode.Space) && Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + fireRate;
            Fire();
        }
    }

    private void Fire()
    {
        AudioManager.Instance?.PlaySFX("PlayerShoot");

        switch (weaponLevel)
        {
            case 1:
                SpawnBullet(firePoint.position, Vector2.up);
                break;
            case 2:
                if (firePointLeft != null && firePointRight != null)
                {
                    SpawnBullet(firePointLeft.position, Vector2.up);
                    SpawnBullet(firePointRight.position, Vector2.up);
                }
                else
                {
                    SpawnBullet(firePoint.position + Vector3.left * 0.2f, Vector2.up);
                    SpawnBullet(firePoint.position + Vector3.right * 0.2f, Vector2.up);
                }
                break;
            case 3:
                SpawnBullet(firePoint.position, Vector2.up);
                SpawnBullet(firePoint.position, new Vector2(-0.3f, 1f).normalized);
                SpawnBullet(firePoint.position, new Vector2(0.3f, 1f).normalized);
                break;
        }
    }

    private void SpawnBullet(Vector3 position, Vector2 direction)
    {
        if (bulletPrefab != null)
        {
            GameObject bullet = Instantiate(bulletPrefab, position, Quaternion.identity);
            Bullet bulletScript = bullet.GetComponent<Bullet>();
            if (bulletScript != null)
            {
                bulletScript.Initialize(direction, true);
            }
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead || isInvincible) return;

        if (hasShield)
        {
            hasShield = false;
            shieldTimer = 0f;
            if (shieldVisual != null) shieldVisual.SetActive(false);
            AudioManager.Instance?.PlaySFX("ShieldBreak");
            StartInvincibility(1f);
            return;
        }

        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        AudioManager.Instance?.PlaySFX("PlayerHit");

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            StartInvincibility(0.5f);
        }
    }

    private void Die()
    {
        isDead = true;
        AudioManager.Instance?.PlaySFX("PlayerExplosion");
        OnPlayerDeath?.Invoke();

        // Spawn explosion effect
        EffectsManager.Instance?.SpawnExplosion(transform.position, 1.5f);

        GameManager.Instance?.PlayerDied();

        // Check if game over or respawn
        if (GameManager.Instance != null && GameManager.Instance.CurrentLives > 0)
        {
            Invoke(nameof(Respawn), GameManager.Instance.respawnDelay);
            spriteRenderer.enabled = false;
            GetComponent<Collider2D>().enabled = false;
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    private void Respawn()
    {
        isDead = false;
        currentHealth = maxHealth;
        weaponLevel = 1;
        hasShield = false;
        if (shieldVisual != null) shieldVisual.SetActive(false);

        transform.position = new Vector3(0f, -3f, 0f);
        spriteRenderer.enabled = true;
        GetComponent<Collider2D>().enabled = true;

        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        StartInvincibility(GameManager.Instance.invincibilityDuration);
    }

    private void StartInvincibility(float duration)
    {
        isInvincible = true;
        invincibilityTimer = duration;
    }

    private void UpdateTimers()
    {
        // Invincibility blink
        if (isInvincible)
        {
            invincibilityTimer -= Time.deltaTime;
            blinkTimer += Time.deltaTime;

            if (blinkTimer >= 0.1f)
            {
                blinkTimer = 0f;
                if (spriteRenderer != null)
                    spriteRenderer.enabled = !spriteRenderer.enabled;
            }

            if (invincibilityTimer <= 0f)
            {
                isInvincible = false;
                if (spriteRenderer != null)
                    spriteRenderer.enabled = true;
            }
        }

        // Weapon level timer
        if (weaponLevel > 1)
        {
            weaponLevelTimer -= Time.deltaTime;
            if (weaponLevelTimer <= 0f)
            {
                weaponLevel = 1;
            }
        }

        // Shield timer
        if (hasShield)
        {
            shieldTimer -= Time.deltaTime;
            if (shieldTimer <= 0f)
            {
                hasShield = false;
                if (shieldVisual != null) shieldVisual.SetActive(false);
            }
        }
    }

    // Power-up application methods
    public void UpgradeWeapon()
    {
        weaponLevel = Mathf.Min(weaponLevel + 1, 3);
        weaponLevelTimer = weaponLevelDuration;
        AudioManager.Instance?.PlaySFX("PowerUp");
    }

    public void ActivateShield()
    {
        hasShield = true;
        shieldTimer = shieldDuration;
        if (shieldVisual != null) shieldVisual.SetActive(true);
        AudioManager.Instance?.PlaySFX("PowerUp");
    }

    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        AudioManager.Instance?.PlaySFX("PowerUp");
    }
}

using UnityEngine;

/// <summary>
/// Controls the player ship: movement, shooting, health, and power-up effects.
/// Attach to the Player prefab with a Rigidbody2D and Collider2D.
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float smoothTime = 0.05f;

    [Header("Boundaries")]
    [SerializeField] private float minX = -8.5f;
    [SerializeField] private float maxX = 8.5f;
    [SerializeField] private float minY = -4.5f;
    [SerializeField] private float maxY = 4.5f;

    [Header("Shooting")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireRate = 0.2f;
    [SerializeField] private float bulletSpeed = 12f;

    [Header("Health")]
    [SerializeField] private int maxHealth = 5;
    [SerializeField] private float invincibilityDuration = 1.5f;

    [Header("Effects")]
    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private GameObject shieldVisual;

    // Internal state
    private int currentHealth;
    private float nextFireTime;
    private Vector2 velocity;
    private bool isInvincible;
    private float invincibilityTimer;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    // Power-up state
    private int weaponLevel = 1;
    private bool hasShield;
    private float shieldTimer;
    private float weaponUpgradeTimer;

    public int MaxHealth => maxHealth;
    public int CurrentHealth => currentHealth;
    public bool HasShield => hasShield;
    public int WeaponLevel => weaponLevel;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }
    }

    private void Start()
    {
        currentHealth = maxHealth;
        weaponLevel = 1;
        hasShield = false;

        if (shieldVisual != null)
            shieldVisual.SetActive(false);

        if (firePoint == null)
        {
            GameObject fp = new GameObject("FirePoint");
            fp.transform.SetParent(transform);
            fp.transform.localPosition = new Vector3(0f, 0.7f, 0f);
            firePoint = fp.transform;
        }
    }

    private void Update()
    {
        if (GameManager.Instance != null && !GameManager.Instance.IsPlaying)
            return;

        HandleMovement();
        HandleShooting();
        HandleInvincibility();
        HandlePowerUpTimers();
    }

    private void HandleMovement()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        Vector2 input = new Vector2(horizontal, vertical).normalized;
        Vector2 targetVelocity = input * moveSpeed;

        Vector2 smoothedVelocity = Vector2.SmoothDamp(
            rb != null ? rb.linearVelocity : Vector2.zero,
            targetVelocity,
            ref velocity,
            smoothTime
        );

        if (rb != null)
        {
            rb.linearVelocity = smoothedVelocity;
        }
        else
        {
            transform.Translate(smoothedVelocity * Time.deltaTime);
        }

        // Clamp position to boundaries
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.y = Mathf.Clamp(pos.y, minY, maxY);
        transform.position = pos;
    }

    private void HandleShooting()
    {
        if (Input.GetKey(KeyCode.Space) || Input.GetMouseButton(0))
        {
            if (Time.time >= nextFireTime)
            {
                Fire();
                nextFireTime = Time.time + GetFireRate();
            }
        }
    }

    private float GetFireRate()
    {
        switch (weaponLevel)
        {
            case 1: return fireRate;
            case 2: return fireRate * 0.75f;
            case 3: return fireRate * 0.5f;
            default: return fireRate * 0.4f;
        }
    }

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
                SpawnBullet(firePoint.position + Vector3.left * 0.25f, Vector2.up);
                SpawnBullet(firePoint.position + Vector3.right * 0.25f, Vector2.up);
                break;
            case 3:
                SpawnBullet(firePoint.position, Vector2.up);
                SpawnBullet(firePoint.position + Vector3.left * 0.3f, new Vector2(-0.15f, 1f).normalized);
                SpawnBullet(firePoint.position + Vector3.right * 0.3f, new Vector2(0.15f, 1f).normalized);
                break;
            default:
                SpawnBullet(firePoint.position, Vector2.up);
                SpawnBullet(firePoint.position + Vector3.left * 0.3f, new Vector2(-0.1f, 1f).normalized);
                SpawnBullet(firePoint.position + Vector3.right * 0.3f, new Vector2(0.1f, 1f).normalized);
                SpawnBullet(firePoint.position + Vector3.left * 0.15f, Vector2.up);
                SpawnBullet(firePoint.position + Vector3.right * 0.15f, Vector2.up);
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

    private void HandleInvincibility()
    {
        if (!isInvincible) return;

        invincibilityTimer -= Time.deltaTime;
        // Blink effect
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

    private void HandlePowerUpTimers()
    {
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

        if (weaponLevel > 1)
        {
            weaponUpgradeTimer -= Time.deltaTime;
            if (weaponUpgradeTimer <= 0f)
            {
                weaponLevel = 1;
            }
        }
    }

    public void TakeDamage(int damage)
    {
        if (isInvincible) return;

        if (hasShield)
        {
            hasShield = false;
            if (shieldVisual != null)
                shieldVisual.SetActive(false);
            AudioManager.Instance?.PlaySFX("ShieldBreak");
            return;
        }

        currentHealth -= damage;
        AudioManager.Instance?.PlaySFX("PlayerHit");

        GameManager.Instance?.OnPlayerHealthChanged(currentHealth, maxHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            isInvincible = true;
            invincibilityTimer = invincibilityDuration;
        }
    }

    private void Die()
    {
        if (explosionPrefab != null)
        {
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        }
        AudioManager.Instance?.PlaySFX("PlayerExplosion");
        GameManager.Instance?.OnPlayerDeath();
        gameObject.SetActive(false);
    }

    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        GameManager.Instance?.OnPlayerHealthChanged(currentHealth, maxHealth);
        AudioManager.Instance?.PlaySFX("Heal");
    }

    public void ActivateShield(float duration)
    {
        hasShield = true;
        shieldTimer = duration;
        if (shieldVisual != null)
            shieldVisual.SetActive(true);
        AudioManager.Instance?.PlaySFX("ShieldActivate");
    }

    public void UpgradeWeapon(float duration)
    {
        weaponLevel = Mathf.Min(weaponLevel + 1, 4);
        weaponUpgradeTimer = duration;
        AudioManager.Instance?.PlaySFX("WeaponUpgrade");
    }

    public void ResetPlayer()
    {
        currentHealth = maxHealth;
        weaponLevel = 1;
        hasShield = false;
        isInvincible = false;
        transform.position = new Vector3(0f, -3.5f, 0f);
        gameObject.SetActive(true);
        if (spriteRenderer != null)
        {
            Color c = spriteRenderer.color;
            c.a = 1f;
            spriteRenderer.color = c;
        }
        if (shieldVisual != null)
            shieldVisual.SetActive(false);
        GameManager.Instance?.OnPlayerHealthChanged(currentHealth, maxHealth);
    }
}

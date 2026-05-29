using UnityEngine;

/// <summary>
/// Controls the player ship: movement, shooting, health, and power-up effects.
/// Attach to the Player GameObject with a Rigidbody2D and Collider2D.
/// Tag the player GameObject as "Player".
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 8f;
    public float screenBorderPadding = 0.5f;

    [Header("Shooting")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float fireRate = 0.25f;
    public float bulletSpeed = 12f;

    [Header("Health")]
    public int maxHealth = 3;
    public float invincibilityDuration = 1.5f;

    [Header("Power-Up Durations")]
    public float shieldDuration = 8f;
    public float rapidFireDuration = 8f;
    public float spreadShotDuration = 8f;

    [Header("Visual")]
    public GameObject shieldVisual;
    public GameObject explosionPrefab;

    // Internal state
    private int currentHealth;
    private float nextFireTime;
    private bool isInvincible;
    private float invincibilityTimer;

    // Power-up state
    private bool hasShield;
    private float shieldTimer;
    private bool hasRapidFire;
    private float rapidFireTimer;
    private bool hasSpreadShot;
    private float spreadShotTimer;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Camera mainCam;
    private Vector2 screenBounds;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        mainCam = Camera.main;

        currentHealth = maxHealth;
        CalculateScreenBounds();

        if (shieldVisual != null)
            shieldVisual.SetActive(false);

        if (UIManager.Instance != null)
            UIManager.Instance.UpdateHealth(currentHealth, maxHealth);
    }

    void Update()
    {
        HandleMovement();
        HandleShooting();
        UpdatePowerUps();
        UpdateInvincibility();
        ClampPosition();
    }

    void CalculateScreenBounds()
    {
        screenBounds = mainCam.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 0));
    }

    void HandleMovement()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector2 movement = new Vector2(horizontal, vertical).normalized;
        rb.velocity = movement * moveSpeed;
    }

    void ClampPosition()
    {
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, -screenBounds.x + screenBorderPadding, screenBounds.x - screenBorderPadding);
        pos.y = Mathf.Clamp(pos.y, -screenBounds.y + screenBorderPadding, screenBounds.y - screenBorderPadding);
        transform.position = pos;
    }

    void HandleShooting()
    {
        if (Input.GetKey(KeyCode.Space) && Time.time >= nextFireTime)
        {
            float currentFireRate = hasRapidFire ? fireRate * 0.4f : fireRate;
            nextFireTime = Time.time + currentFireRate;

            if (hasSpreadShot)
            {
                FireSpreadShot();
            }
            else
            {
                FireSingleShot(Vector2.up);
            }

            AudioManager.Instance?.PlaySFX("PlayerShoot");
        }
    }

    void FireSingleShot(Vector2 direction)
    {
        if (bulletPrefab == null) return;

        Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position + Vector3.up * 0.5f;
        GameObject bullet = Instantiate(bulletPrefab, spawnPos, Quaternion.identity);
        BulletController bc = bullet.GetComponent<BulletController>();
        if (bc != null)
        {
            bc.Initialize(direction, bulletSpeed, true);
        }
    }

    void FireSpreadShot()
    {
        if (bulletPrefab == null) return;

        Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position + Vector3.up * 0.5f;

        // Center shot
        FireBulletAtAngle(spawnPos, 0f);
        // Left shot
        FireBulletAtAngle(spawnPos, -15f);
        // Right shot
        FireBulletAtAngle(spawnPos, 15f);
    }

    void FireBulletAtAngle(Vector3 spawnPos, float angleDeg)
    {
        float rad = (90f + angleDeg) * Mathf.Deg2Rad;
        Vector2 dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)).normalized;

        GameObject bullet = Instantiate(bulletPrefab, spawnPos, Quaternion.identity);
        BulletController bc = bullet.GetComponent<BulletController>();
        if (bc != null)
        {
            bc.Initialize(dir, bulletSpeed, true);
        }
    }

    void UpdatePowerUps()
    {
        if (hasShield)
        {
            shieldTimer -= Time.deltaTime;
            if (shieldTimer <= 0f)
            {
                hasShield = false;
                if (shieldVisual != null) shieldVisual.SetActive(false);
            }
        }

        if (hasRapidFire)
        {
            rapidFireTimer -= Time.deltaTime;
            if (rapidFireTimer <= 0f) hasRapidFire = false;
        }

        if (hasSpreadShot)
        {
            spreadShotTimer -= Time.deltaTime;
            if (spreadShotTimer <= 0f) hasSpreadShot = false;
        }
    }

    void UpdateInvincibility()
    {
        if (isInvincible)
        {
            invincibilityTimer -= Time.deltaTime;
            // Blink effect
            float alpha = Mathf.PingPong(Time.time * 8f, 1f) > 0.5f ? 1f : 0.3f;
            if (spriteRenderer != null)
            {
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
    }

    public void TakeDamage(int damage)
    {
        if (isInvincible) return;

        if (hasShield)
        {
            hasShield = false;
            if (shieldVisual != null) shieldVisual.SetActive(false);
            AudioManager.Instance?.PlaySFX("ShieldHit");
            StartInvincibility();
            return;
        }

        currentHealth -= damage;
        AudioManager.Instance?.PlaySFX("PlayerHit");

        if (UIManager.Instance != null)
            UIManager.Instance.UpdateHealth(currentHealth, maxHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            StartInvincibility();
        }
    }

    void StartInvincibility()
    {
        isInvincible = true;
        invincibilityTimer = invincibilityDuration;
    }

    void Die()
    {
        if (explosionPrefab != null)
        {
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        }

        AudioManager.Instance?.PlaySFX("Explosion");
        GameManager.Instance?.PlayerDied();
        gameObject.SetActive(false);
    }

    public void ActivatePowerUp(PowerUpType type)
    {
        switch (type)
        {
            case PowerUpType.Shield:
                hasShield = true;
                shieldTimer = shieldDuration;
                if (shieldVisual != null) shieldVisual.SetActive(true);
                break;

            case PowerUpType.RapidFire:
                hasRapidFire = true;
                rapidFireTimer = rapidFireDuration;
                break;

            case PowerUpType.SpreadShot:
                hasSpreadShot = true;
                spreadShotTimer = spreadShotDuration;
                break;
        }

        AudioManager.Instance?.PlaySFX("PowerUp");
    }

    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        if (UIManager.Instance != null)
            UIManager.Instance.UpdateHealth(currentHealth, maxHealth);
    }

    public void ResetPlayer()
    {
        currentHealth = maxHealth;
        isInvincible = false;
        hasShield = false;
        hasRapidFire = false;
        hasSpreadShot = false;
        transform.position = new Vector3(0, -3.5f, 0);
        gameObject.SetActive(true);

        if (spriteRenderer != null)
        {
            Color c = spriteRenderer.color;
            c.a = 1f;
            spriteRenderer.color = c;
        }

        if (shieldVisual != null) shieldVisual.SetActive(false);

        if (UIManager.Instance != null)
            UIManager.Instance.UpdateHealth(currentHealth, maxHealth);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("EnemyBullet"))
        {
            TakeDamage(1);
            Destroy(other.gameObject);
        }
        else if (other.CompareTag("Enemy"))
        {
            TakeDamage(1);
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

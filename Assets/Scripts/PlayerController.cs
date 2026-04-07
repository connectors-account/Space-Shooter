using UnityEngine;

/// <summary>
/// PlayerController - Handles player movement, shooting, health, and power-up effects.
/// Attach to the Player ship GameObject with a Rigidbody2D, BoxCollider2D (trigger), and SpriteRenderer.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 8f;
    public float screenPadding = 0.5f;

    [Header("Shooting")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float fireRate = 0.25f;
    public float rapidFireRate = 0.1f;
    public float rapidFireDuration = 5f;
    public float bulletSpeed = 12f;

    [Header("Invincibility")]
    public float invincibilityDuration = 2f;
    public float blinkInterval = 0.15f;

    [Header("Audio")]
    public AudioSource shootAudioSource;
    public AudioSource hitAudioSource;
    public AudioSource powerUpAudioSource;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private float nextFireTime;
    private float currentFireRate;
    private float rapidFireEndTime;
    private bool isInvincible;
    private float invincibilityEndTime;
    private bool isAlive = true;

    // Screen bounds
    private float minX, maxX, minY, maxY;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        BoxCollider2D col = GetComponent<BoxCollider2D>();
        col.isTrigger = true;
    }

    private void Start()
    {
        currentFireRate = fireRate;
        CalculateScreenBounds();

        // Create fire point if not assigned
        if (firePoint == null)
        {
            GameObject fp = new GameObject("FirePoint");
            fp.transform.SetParent(transform);
            fp.transform.localPosition = new Vector3(0f, 0.6f, 0f);
            firePoint = fp.transform;
        }
    }

    private void Update()
    {
        if (!isAlive || (GameManager.Instance != null && GameManager.Instance.IsGameOver)) return;
        if (GameManager.Instance != null && GameManager.Instance.IsPaused) return;

        HandleShooting();
        HandlePowerUpTimers();
        HandleInvincibility();
    }

    private void FixedUpdate()
    {
        if (!isAlive || (GameManager.Instance != null && GameManager.Instance.IsGameOver)) return;
        if (GameManager.Instance != null && GameManager.Instance.IsPaused) return;

        HandleMovement();
    }

    /// <summary>
    /// Read WASD / Arrow key input and move the ship, clamped to screen bounds.
    /// </summary>
    private void HandleMovement()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector2 direction = new Vector2(h, v).normalized;
        rb.linearVelocity = direction * moveSpeed;

        // Clamp position to screen bounds
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.y = Mathf.Clamp(pos.y, minY, maxY);
        transform.position = pos;
    }

    /// <summary>
    /// Fire a bullet upward when Space is pressed, respecting fire rate.
    /// </summary>
    private void HandleShooting()
    {
        if (Input.GetKey(KeyCode.Space) && Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + currentFireRate;
            FireBullet();
        }
    }

    private void FireBullet()
    {
        if (bulletPrefab == null) return;

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        BulletController bc = bullet.GetComponent<BulletController>();
        if (bc != null)
        {
            bc.Initialize(Vector2.up, bulletSpeed, true, 1);
        }

        if (shootAudioSource != null)
            shootAudioSource.Play();
    }

    /// <summary>
    /// Called when the player is hit by an enemy or enemy bullet.
    /// </summary>
    public void TakeDamage()
    {
        if (isInvincible || !isAlive) return;

        if (hitAudioSource != null)
            hitAudioSource.Play();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.LoseLife();

            if (GameManager.Instance.PlayerLives > 0)
            {
                StartInvincibility();
            }
            else
            {
                Die();
            }
        }
        else
        {
            Die();
        }
    }

    private void Die()
    {
        isAlive = false;
        spriteRenderer.enabled = false;
        // Disable collider
        GetComponent<BoxCollider2D>().enabled = false;
        rb.linearVelocity = Vector2.zero;
    }

    private void StartInvincibility()
    {
        isInvincible = true;
        invincibilityEndTime = Time.time + invincibilityDuration;
    }

    private void HandleInvincibility()
    {
        if (!isInvincible) return;

        // Blink effect
        float t = Mathf.PingPong(Time.time / blinkInterval, 1f);
        spriteRenderer.enabled = t > 0.5f;

        if (Time.time >= invincibilityEndTime)
        {
            isInvincible = false;
            spriteRenderer.enabled = true;
        }
    }

    /// <summary>
    /// Activate rapid-fire power-up.
    /// </summary>
    public void ActivateRapidFire()
    {
        currentFireRate = rapidFireRate;
        rapidFireEndTime = Time.time + rapidFireDuration;

        if (powerUpAudioSource != null)
            powerUpAudioSource.Play();
    }

    /// <summary>
    /// Restore one life (health power-up).
    /// </summary>
    public void RestoreHealth()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.RestoreLife();

        if (powerUpAudioSource != null)
            powerUpAudioSource.Play();
    }

    private void HandlePowerUpTimers()
    {
        // Rapid fire expires
        if (currentFireRate < fireRate && Time.time >= rapidFireEndTime)
        {
            currentFireRate = fireRate;
        }
    }

    private void CalculateScreenBounds()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        float halfHeight = cam.orthographicSize;
        float halfWidth = halfHeight * cam.aspect;

        minX = -halfWidth + screenPadding;
        maxX = halfWidth - screenPadding;
        minY = -halfHeight + screenPadding;
        maxY = halfHeight - screenPadding;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isAlive) return;

        // Hit by enemy bullet
        if (other.CompareTag("EnemyBullet"))
        {
            TakeDamage();
            Destroy(other.gameObject);
        }
        // Collided with enemy
        else if (other.CompareTag("Enemy"))
        {
            TakeDamage();
        }
        // Picked up power-up
        else if (other.CompareTag("PowerUp"))
        {
            PowerUpController pu = other.GetComponent<PowerUpController>();
            if (pu != null)
            {
                pu.ApplyEffect(this);
            }
            Destroy(other.gameObject);
        }
    }
}

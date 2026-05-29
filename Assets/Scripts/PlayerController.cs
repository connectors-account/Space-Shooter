using UnityEngine;

/// <summary>
/// Controls player ship movement, shooting, and power-up states.
/// Attach to the Player GameObject with a Rigidbody2D and Collider2D.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(HealthSystem))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float boundaryX = 8.5f;
    [SerializeField] private float boundaryYTop = 4.5f;
    [SerializeField] private float boundaryYBottom = -4.5f;

    [Header("Shooting")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireRate = 0.25f;
    [SerializeField] private float rapidFireRate = 0.1f;
    [SerializeField] private float bulletSpeed = 12f;

    [Header("Visual Feedback")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color damageFlashColor = Color.red;
    [SerializeField] private Color shieldColor = Color.cyan;

    private Rigidbody2D rb;
    private HealthSystem healthSystem;
    private float nextFireTime = 0f;
    private bool isRapidFire = false;
    private float rapidFireTimer = 0f;
    private Color originalColor;
    private float flashTimer = 0f;
    private bool isFlashing = false;

    // Shield visual child object
    private GameObject shieldVisual;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        healthSystem = GetComponent<HealthSystem>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;
    }

    private void Start()
    {
        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        healthSystem.OnDamageTaken += HandleDamageTaken;
        healthSystem.OnDeath += HandleDeath;
        healthSystem.OnShieldBroken += HandleShieldBroken;

        // Create shield visual
        shieldVisual = new GameObject("ShieldVisual");
        shieldVisual.transform.SetParent(transform);
        shieldVisual.transform.localPosition = Vector3.zero;
        var shieldSr = shieldVisual.AddComponent<SpriteRenderer>();
        shieldSr.color = new Color(0f, 1f, 1f, 0.3f);
        shieldSr.sortingOrder = 5;
        shieldVisual.transform.localScale = Vector3.one * 1.5f;
        shieldVisual.SetActive(false);
    }

    private void Update()
    {
        HandleShooting();
        HandlePowerUpTimers();
        HandleFlash();
        UpdateShieldVisual();
    }

    private void FixedUpdate()
    {
        HandleMovement();
    }

    private void HandleMovement()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector2 movement = new Vector2(horizontal, vertical).normalized * moveSpeed;
        rb.linearVelocity = movement;

        // Clamp position within boundaries
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, -boundaryX, boundaryX);
        pos.y = Mathf.Clamp(pos.y, boundaryYBottom, boundaryYTop);
        transform.position = pos;
    }

    private void HandleShooting()
    {
        if (Input.GetKey(KeyCode.Space) && Time.time >= nextFireTime)
        {
            float currentFireRate = isRapidFire ? rapidFireRate : fireRate;
            nextFireTime = Time.time + currentFireRate;
            FireBullet();
        }
    }

    private void FireBullet()
    {
        if (bulletPrefab == null) return;

        Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position + Vector3.up * 0.6f;

        GameObject bullet = Instantiate(bulletPrefab, spawnPos, Quaternion.identity);
        BulletController bc = bullet.GetComponent<BulletController>();
        if (bc != null)
        {
            bc.Initialize(Vector2.up, bulletSpeed, true);
        }

        AudioManager.Instance?.PlaySFX("PlayerShoot");
    }

    private void HandlePowerUpTimers()
    {
        if (isRapidFire)
        {
            rapidFireTimer -= Time.deltaTime;
            if (rapidFireTimer <= 0f)
            {
                isRapidFire = false;
            }
        }
    }

    private void HandleFlash()
    {
        if (isFlashing && spriteRenderer != null)
        {
            flashTimer -= Time.deltaTime;
            if (flashTimer <= 0f)
            {
                spriteRenderer.color = originalColor;
                isFlashing = false;
            }
        }
    }

    private void UpdateShieldVisual()
    {
        if (shieldVisual != null)
        {
            shieldVisual.SetActive(healthSystem.HasShield);
        }
    }

    private void HandleDamageTaken()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = damageFlashColor;
            isFlashing = true;
            flashTimer = 0.15f;
        }
        AudioManager.Instance?.PlaySFX("PlayerHit");
    }

    private void HandleDeath()
    {
        AudioManager.Instance?.PlaySFX("PlayerDeath");
        GameManager.Instance?.GameOver();
    }

    private void HandleShieldBroken()
    {
        AudioManager.Instance?.PlaySFX("ShieldBreak");
    }

    // Power-up application methods (called by PowerUpController)
    public void ActivateRapidFire(float duration)
    {
        isRapidFire = true;
        rapidFireTimer = duration;
    }

    public void HealPlayer(int amount)
    {
        healthSystem.Heal(amount);
    }

    public void ActivateShield()
    {
        healthSystem.ActivateShield();
    }

    public HealthSystem GetHealthSystem()
    {
        return healthSystem;
    }

    private void OnDestroy()
    {
        if (healthSystem != null)
        {
            healthSystem.OnDamageTaken -= HandleDamageTaken;
            healthSystem.OnDeath -= HandleDeath;
            healthSystem.OnShieldBroken -= HandleShieldBroken;
        }
    }
}

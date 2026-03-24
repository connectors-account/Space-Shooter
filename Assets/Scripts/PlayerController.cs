using UnityEngine;

/// <summary>
/// PlayerController handles the player ship's movement, shooting, health,
/// shield status, and weapon upgrades. Attach to the Player GameObject.
/// </summary>
public class PlayerController : MonoBehaviour
{
    // ── Movement ──────────────────────────────────────────────
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float horizontalBoundary = 8.5f;
    [SerializeField] private float verticalBoundaryTop = 4.5f;
    [SerializeField] private float verticalBoundaryBottom = -4.5f;

    // ── Shooting ─────────────────────────────────────────────
    [Header("Shooting")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireRate = 0.2f;
    [SerializeField] private float bulletSpeed = 12f;

    // ── Health ───────────────────────────────────────────────
    [Header("Health")]
    [SerializeField] private int maxHealth = 5;
    private int currentHealth;

    // ── Shield ───────────────────────────────────────────────
    [Header("Shield")]
    [SerializeField] private GameObject shieldVisual;
    private bool shieldActive = false;
    private float shieldTimer = 0f;
    private float shieldDuration = 5f;

    // ── Weapon Upgrade ───────────────────────────────────────
    [Header("Weapon")]
    private int weaponLevel = 1;          // 1 = single, 2 = double, 3 = triple/spread
    private float weaponUpgradeTimer = 0f;
    private float weaponUpgradeDuration = 10f;

    // ── Internal State ───────────────────────────────────────
    private float nextFireTime = 0f;
    private bool isAlive = true;
    private SpriteRenderer spriteRenderer;

    // ── Properties for UI / GameManager ──────────────────────
    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public bool IsAlive => isAlive;

    // ──────────────────────────────────────────────────────────
    // Unity Lifecycle
    // ──────────────────────────────────────────────────────────

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        currentHealth = maxHealth;

        // Make sure shield visual is hidden at start
        if (shieldVisual != null)
            shieldVisual.SetActive(false);

        // Create a fire point if not assigned
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
        if (!isAlive) return;

        HandleMovement();
        HandleShooting();
        HandleTimers();
    }

    // ──────────────────────────────────────────────────────────
    // Movement
    // ──────────────────────────────────────────────────────────

    private void HandleMovement()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 direction = new Vector3(h, v, 0f).normalized;
        transform.Translate(direction * moveSpeed * Time.deltaTime, Space.World);

        // Clamp position to screen boundaries
        float clampedX = Mathf.Clamp(transform.position.x, -horizontalBoundary, horizontalBoundary);
        float clampedY = Mathf.Clamp(transform.position.y, verticalBoundaryBottom, verticalBoundaryTop);
        transform.position = new Vector3(clampedX, clampedY, 0f);
    }

    // ──────────────────────────────────────────────────────────
    // Shooting
    // ──────────────────────────────────────────────────────────

    private void HandleShooting()
    {
        if (Input.GetKey(KeyCode.Space) && Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + fireRate;
            FireBullets();
        }
    }

    /// <summary>
    /// Fires bullets based on the current weapon level.
    /// Level 1: single straight shot.
    /// Level 2: double parallel shots.
    /// Level 3: triple spread shots.
    /// </summary>
    private void FireBullets()
    {
        // Play shoot SFX
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX("PlayerShoot");

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
            default:
                SpawnBullet(firePoint.position, Vector2.up);
                SpawnBullet(firePoint.position + Vector3.left * 0.2f, new Vector2(-0.15f, 1f).normalized);
                SpawnBullet(firePoint.position + Vector3.right * 0.2f, new Vector2(0.15f, 1f).normalized);
                break;
        }
    }

    /// <summary>
    /// Instantiates a single bullet travelling in the given direction.
    /// </summary>
    private void SpawnBullet(Vector3 position, Vector2 direction)
    {
        if (bulletPrefab == null) return;

        GameObject bullet = Instantiate(bulletPrefab, position, Quaternion.identity);
        BulletController bc = bullet.GetComponent<BulletController>();
        if (bc != null)
        {
            bc.Initialize(direction, bulletSpeed, true); // true = player bullet
        }
    }

    // ──────────────────────────────────────────────────────────
    // Timers (shield & weapon upgrade duration)
    // ──────────────────────────────────────────────────────────

    private void HandleTimers()
    {
        // Shield countdown
        if (shieldActive)
        {
            shieldTimer -= Time.deltaTime;
            if (shieldTimer <= 0f)
            {
                DeactivateShield();
            }
        }

        // Weapon upgrade countdown
        if (weaponLevel > 1)
        {
            weaponUpgradeTimer -= Time.deltaTime;
            if (weaponUpgradeTimer <= 0f)
            {
                weaponLevel = 1;
            }
        }
    }

    // ──────────────────────────────────────────────────────────
    // Damage & Health
    // ──────────────────────────────────────────────────────────

    /// <summary>
    /// Apply damage to the player. Shield absorbs one hit then deactivates.
    /// </summary>
    public void TakeDamage(int amount)
    {
        if (!isAlive) return;

        if (shieldActive)
        {
            DeactivateShield();
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX("ShieldHit");
            return; // Shield absorbed the hit
        }

        currentHealth -= amount;

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX("PlayerHit");

        // Flash effect
        StartCoroutine(FlashDamage());

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }

        // Notify UI
        if (UIManager.Instance != null)
            UIManager.Instance.UpdateHealthDisplay(currentHealth, maxHealth);
    }

    /// <summary>
    /// Heal the player by the given amount, capped at maxHealth.
    /// </summary>
    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);

        if (UIManager.Instance != null)
            UIManager.Instance.UpdateHealthDisplay(currentHealth, maxHealth);
    }

    private void Die()
    {
        isAlive = false;

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX("Explosion");

        // Notify GameManager
        if (GameManager.Instance != null)
            GameManager.Instance.OnPlayerDeath();

        // Disable visuals and collider; keep object for reference
        if (spriteRenderer != null)
            spriteRenderer.enabled = false;

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;
    }

    /// <summary>
    /// Brief red flash when taking damage.
    /// </summary>
    private System.Collections.IEnumerator FlashDamage()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.color = Color.white;
        }
    }

    // ──────────────────────────────────────────────────────────
    // Power-Up Activation
    // ──────────────────────────────────────────────────────────

    /// <summary>
    /// Activate the shield for a set duration.
    /// </summary>
    public void ActivateShield(float duration)
    {
        shieldActive = true;
        shieldDuration = duration;
        shieldTimer = shieldDuration;

        if (shieldVisual != null)
            shieldVisual.SetActive(true);
    }

    private void DeactivateShield()
    {
        shieldActive = false;
        shieldTimer = 0f;

        if (shieldVisual != null)
            shieldVisual.SetActive(false);
    }

    /// <summary>
    /// Upgrade weapon level (max 3) for a limited duration.
    /// </summary>
    public void UpgradeWeapon()
    {
        weaponLevel = Mathf.Min(weaponLevel + 1, 3);
        weaponUpgradeTimer = weaponUpgradeDuration;
    }

    // ──────────────────────────────────────────────────────────
    // Collision – enemy bullets & enemies damage the player
    // ──────────────────────────────────────────────────────────

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isAlive) return;

        // Hit by enemy bullet
        if (other.CompareTag("EnemyBullet"))
        {
            TakeDamage(1);
            Destroy(other.gameObject);
        }
        // Collided with enemy ship
        else if (other.CompareTag("Enemy"))
        {
            TakeDamage(2);
        }
    }
}

using UnityEngine;

/// <summary>
/// PlayerController handles all player input, movement, and shooting.
/// Attach this to the Player GameObject.
///
/// Movement: WASD or Arrow keys (clamped to screen bounds).
/// Shooting: Space bar fires bullets upward.
/// </summary>
[RequireComponent(typeof(HealthSystem))]
public class PlayerController : MonoBehaviour
{
    // ============================================================
    // MOVEMENT
    // ============================================================
    [Header("Movement")]
    [Tooltip("Units per second the player moves")]
    public float moveSpeed = 8f;

    [Tooltip("Horizontal boundary (world units from center)")]
    public float horizontalBound = 8f;

    [Tooltip("Vertical boundaries (min Y, max Y)")]
    public float verticalBoundMin = -4.5f;
    public float verticalBoundMax = 4.5f;

    // ============================================================
    // SHOOTING
    // ============================================================
    [Header("Shooting")]
    [Tooltip("Bullet prefab to instantiate")]
    public GameObject bulletPrefab;

    [Tooltip("Where the bullet spawns relative to the player")]
    public Transform firePoint;

    [Tooltip("Base seconds between shots")]
    public float fireRate = 0.25f;

    [Tooltip("Fire rate when rapid-fire power-up is active")]
    public float rapidFireRate = 0.1f;

    // ============================================================
    // POWER-UP STATE
    // ============================================================
    [Header("Power-Up State")]
    public bool hasRapidFire = false;
    public float rapidFireTimer = 0f;

    public bool hasShield = false;
    public float shieldTimer = 0f;

    [Tooltip("Visual child object for the shield bubble")]
    public GameObject shieldVisual;

    // ============================================================
    // INTERNAL
    // ============================================================
    private float nextFireTime = 0f;
    private HealthSystem healthSystem;

    // ============================================================
    // UNITY LIFECYCLE
    // ============================================================

    void Start()
    {
        healthSystem = GetComponent<HealthSystem>();

        // If no fire point assigned, create one slightly above the player
        if (firePoint == null)
        {
            GameObject fp = new GameObject("FirePoint");
            fp.transform.SetParent(transform);
            fp.transform.localPosition = new Vector3(0f, 0.6f, 0f);
            firePoint = fp.transform;
        }

        // Hide shield visual initially
        if (shieldVisual != null)
            shieldVisual.SetActive(false);
    }

    void Update()
    {
        // Don't process input if game is over
        if (GameManager.Instance != null &&
            GameManager.Instance.CurrentState != GameManager.GameState.Playing &&
            GameManager.Instance.CurrentState != GameManager.GameState.WaveComplete)
            return;

        HandleMovement();
        HandleShooting();
        UpdatePowerUpTimers();
    }

    // ============================================================
    // MOVEMENT
    // ============================================================

    /// <summary>
    /// Read horizontal and vertical input and move the player.
    /// Position is clamped so the ship stays on screen.
    /// </summary>
    void HandleMovement()
    {
        // Read raw axis input (WASD + Arrow keys both work)
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        // Build movement vector and normalize to prevent diagonal speed boost
        Vector3 movement = new Vector3(h, v, 0f).normalized;

        // Apply movement
        transform.position += movement * moveSpeed * Time.deltaTime;

        // Clamp to screen bounds
        float clampedX = Mathf.Clamp(transform.position.x, -horizontalBound, horizontalBound);
        float clampedY = Mathf.Clamp(transform.position.y, verticalBoundMin, verticalBoundMax);
        transform.position = new Vector3(clampedX, clampedY, 0f);
    }

    // ============================================================
    // SHOOTING
    // ============================================================

    /// <summary>
    /// Fire bullets when Space is held, respecting the fire rate cooldown.
    /// </summary>
    void HandleShooting()
    {
        if (Input.GetKey(KeyCode.Space) && Time.time >= nextFireTime)
        {
            FireBullet();
            float currentRate = hasRapidFire ? rapidFireRate : fireRate;
            nextFireTime = Time.time + currentRate;
        }
    }

    /// <summary>
    /// Instantiate a bullet at the fire point, moving upward.
    /// </summary>
    void FireBullet()
    {
        if (bulletPrefab == null) return;

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        bullet.tag = "PlayerBullet";

        // Configure the bullet
        BulletController bc = bullet.GetComponent<BulletController>();
        if (bc != null)
        {
            bc.direction = Vector2.up;
            bc.speed = 12f;
            bc.damage = 1;
        }
    }

    // ============================================================
    // POWER-UP MANAGEMENT
    // ============================================================

    /// <summary>
    /// Tick down active power-up durations.
    /// </summary>
    void UpdatePowerUpTimers()
    {
        // Rapid Fire countdown
        if (hasRapidFire)
        {
            rapidFireTimer -= Time.deltaTime;
            if (rapidFireTimer <= 0f)
            {
                hasRapidFire = false;
                rapidFireTimer = 0f;
            }
        }

        // Shield countdown
        if (hasShield)
        {
            shieldTimer -= Time.deltaTime;
            if (shieldTimer <= 0f)
            {
                DeactivateShield();
            }
        }
    }

    /// <summary>
    /// Activate rapid fire for a duration.
    /// </summary>
    public void ActivateRapidFire(float duration)
    {
        hasRapidFire = true;
        rapidFireTimer = duration;
    }

    /// <summary>
    /// Activate shield for a duration. The shield absorbs one hit.
    /// </summary>
    public void ActivateShield(float duration)
    {
        hasShield = true;
        shieldTimer = duration;
        if (shieldVisual != null)
            shieldVisual.SetActive(true);
    }

    /// <summary>
    /// Remove the shield effect.
    /// </summary>
    public void DeactivateShield()
    {
        hasShield = false;
        shieldTimer = 0f;
        if (shieldVisual != null)
            shieldVisual.SetActive(false);
    }

    /// <summary>
    /// Heal the player by the given amount.
    /// </summary>
    public void Heal(int amount)
    {
        if (healthSystem != null)
        {
            healthSystem.Heal(amount);
        }
    }

    /// <summary>
    /// Called by CollisionHandler when the player is hit.
    /// If the shield is active, absorb the hit instead.
    /// Returns true if damage was absorbed by shield.
    /// </summary>
    public bool TryAbsorbDamage()
    {
        if (hasShield)
        {
            DeactivateShield();
            return true; // damage absorbed
        }
        return false; // take normal damage
    }
}

using UnityEngine;

/// <summary>
/// PlayerController handles player movement, shooting, and input processing.
/// Attach this to the Player GameObject.
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float horizontalBoundary = 8f;
    [SerializeField] private float verticalBoundaryTop = 4f;
    [SerializeField] private float verticalBoundaryBottom = -4f;

    [Header("Shooting Settings")]
    [SerializeField] private float fireRate = 0.2f;
    [SerializeField] private Transform[] firePoints;
    [SerializeField] private string bulletPoolTag = "PlayerBullet";

    [Header("Audio")]
    [SerializeField] private string shootSoundName = "PlayerShoot";

    // Private variables
    private float nextFireTime;
    private Vector2 moveInput;
    private PlayerHealth playerHealth;
    private bool canShoot = true;

    // Power-up states
    private int weaponLevel = 1;
    private bool hasShield = false;
    private float shieldTimer = 0f;

    public bool HasShield => hasShield;
    public int WeaponLevel => weaponLevel;

    private void Start()
    {
        playerHealth = GetComponent<PlayerHealth>();
        if (playerHealth == null)
        {
            Debug.LogWarning("PlayerHealth component not found on Player!");
        }
    }

    private void Update()
    {
        // Don't process input if game is paused
        if (GameManager.Instance != null && GameManager.Instance.IsPaused)
            return;

        HandleInput();
        HandleMovement();
        HandleShooting();
        UpdateShield();
    }

    /// <summary>
    /// Process player input for movement and shooting
    /// </summary>
    private void HandleInput()
    {
        // Get movement input (supports both WASD and Arrow keys)
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        moveInput = new Vector2(horizontal, vertical).normalized;
    }

    /// <summary>
    /// Apply movement to the player within screen boundaries
    /// </summary>
    private void HandleMovement()
    {
        // Calculate new position
        Vector3 newPosition = transform.position + (Vector3)moveInput * moveSpeed * Time.deltaTime;

        // Clamp position within boundaries
        newPosition.x = Mathf.Clamp(newPosition.x, -horizontalBoundary, horizontalBoundary);
        newPosition.y = Mathf.Clamp(newPosition.y, verticalBoundaryBottom, verticalBoundaryTop);

        transform.position = newPosition;
    }

    /// <summary>
    /// Handle shooting when fire button is pressed
    /// </summary>
    private void HandleShooting()
    {
        // Fire on Space or Left Mouse Button
        if ((Input.GetKey(KeyCode.Space) || Input.GetMouseButton(0)) && canShoot)
        {
            if (Time.time >= nextFireTime)
            {
                Fire();
                nextFireTime = Time.time + fireRate;
            }
        }
    }

    /// <summary>
    /// Fire bullets based on current weapon level
    /// </summary>
    private void Fire()
    {
        if (ObjectPooler.Instance == null)
        {
            Debug.LogError("ObjectPooler not found!");
            return;
        }

        // Play shoot sound
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySound(shootSoundName);
        }

        // Fire pattern based on weapon level
        switch (weaponLevel)
        {
            case 1:
                // Single shot from center
                FireBullet(transform.position, Vector2.up, 0f);
                break;
            case 2:
                // Double shot
                FireBullet(transform.position + Vector3.left * 0.3f, Vector2.up, 0f);
                FireBullet(transform.position + Vector3.right * 0.3f, Vector2.up, 0f);
                break;
            case 3:
                // Triple shot with spread
                FireBullet(transform.position, Vector2.up, 0f);
                FireBullet(transform.position + Vector3.left * 0.3f, new Vector2(-0.1f, 1f).normalized, -5f);
                FireBullet(transform.position + Vector3.right * 0.3f, new Vector2(0.1f, 1f).normalized, 5f);
                break;
            default:
                // Max level - five shot spread
                FireBullet(transform.position, Vector2.up, 0f);
                FireBullet(transform.position + Vector3.left * 0.2f, new Vector2(-0.1f, 1f).normalized, -10f);
                FireBullet(transform.position + Vector3.right * 0.2f, new Vector2(0.1f, 1f).normalized, 10f);
                FireBullet(transform.position + Vector3.left * 0.4f, new Vector2(-0.2f, 1f).normalized, -20f);
                FireBullet(transform.position + Vector3.right * 0.4f, new Vector2(0.2f, 1f).normalized, 20f);
                break;
        }
    }

    /// <summary>
    /// Fire a single bullet from the object pool
    /// </summary>
    private void FireBullet(Vector3 position, Vector2 direction, float angle)
    {
        GameObject bullet = ObjectPooler.Instance.SpawnFromPool(bulletPoolTag, position, Quaternion.Euler(0, 0, angle));
        if (bullet != null)
        {
            Bullet bulletScript = bullet.GetComponent<Bullet>();
            if (bulletScript != null)
            {
                bulletScript.SetDirection(direction);
            }
        }
    }

    /// <summary>
    /// Update shield duration
    /// </summary>
    private void UpdateShield()
    {
        if (hasShield)
        {
            shieldTimer -= Time.deltaTime;
            if (shieldTimer <= 0)
            {
                DeactivateShield();
            }
        }
    }

    /// <summary>
    /// Upgrade weapon level (called by power-ups)
    /// </summary>
    public void UpgradeWeapon()
    {
        weaponLevel = Mathf.Min(weaponLevel + 1, 4);
        Debug.Log($"Weapon upgraded to level {weaponLevel}");
    }

    /// <summary>
    /// Activate shield for a duration (called by power-ups)
    /// </summary>
    public void ActivateShield(float duration)
    {
        hasShield = true;
        shieldTimer = duration;
        
        // Visual feedback - enable shield child object if exists
        Transform shield = transform.Find("Shield");
        if (shield != null)
        {
            shield.gameObject.SetActive(true);
        }
        
        Debug.Log($"Shield activated for {duration} seconds");
    }

    /// <summary>
    /// Deactivate shield
    /// </summary>
    public void DeactivateShield()
    {
        hasShield = false;
        shieldTimer = 0f;
        
        // Visual feedback - disable shield child object if exists
        Transform shield = transform.Find("Shield");
        if (shield != null)
        {
            shield.gameObject.SetActive(false);
        }
        
        Debug.Log("Shield deactivated");
    }

    /// <summary>
    /// Absorb damage with shield (returns true if damage was absorbed)
    /// </summary>
    public bool AbsorbDamageWithShield()
    {
        if (hasShield)
        {
            DeactivateShield();
            return true;
        }
        return false;
    }

    /// <summary>
    /// Reset player state (called when game restarts)
    /// </summary>
    public void ResetPlayer()
    {
        weaponLevel = 1;
        hasShield = false;
        shieldTimer = 0f;
        canShoot = true;
        transform.position = new Vector3(0, -3f, 0);
    }

    /// <summary>
    /// Enable or disable shooting
    /// </summary>
    public void SetCanShoot(bool value)
    {
        canShoot = value;
    }
}

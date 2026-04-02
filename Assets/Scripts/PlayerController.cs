// =============================================================================
// PlayerController.cs
// Handles player ship movement, shooting, health, and invincibility frames.
// Attach this script to the Player ship GameObject.
// =============================================================================
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Movement Settings
    // -------------------------------------------------------------------------
    [Header("Movement")]
    [Tooltip("Speed of the player ship in units per second.")]
    public float moveSpeed = 8f;

    [Tooltip("Boundary limits for player movement (viewport-based clamping).")]
    public float minX = -8.5f;
    public float maxX = 8.5f;
    public float minY = -4.5f;
    public float maxY = 4.5f;

    // -------------------------------------------------------------------------
    // Shooting Settings
    // -------------------------------------------------------------------------
    [Header("Shooting")]
    [Tooltip("Reference to the bullet prefab the player fires.")]
    public GameObject bulletPrefab;

    [Tooltip("Point from which bullets are spawned (assign a child transform).")]
    public Transform firePoint;

    [Tooltip("Time in seconds between each shot.")]
    public float fireRate = 0.25f;

    [Tooltip("Rapid fire rate when power-up is active.")]
    public float rapidFireRate = 0.1f;

    // -------------------------------------------------------------------------
    // Health Settings
    // -------------------------------------------------------------------------
    [Header("Health")]
    [Tooltip("Maximum health of the player.")]
    public int maxHealth = 5;

    [Tooltip("Duration of invincibility after taking damage (seconds).")]
    public float invincibilityDuration = 1.5f;

    // -------------------------------------------------------------------------
    // Visual Feedback
    // -------------------------------------------------------------------------
    [Header("Effects")]
    [Tooltip("Explosion prefab instantiated when the player is destroyed.")]
    public GameObject explosionPrefab;

    [Tooltip("Shield visual GameObject (child of player, toggled on/off).")]
    public GameObject shieldVisual;

    // -------------------------------------------------------------------------
    // Internal State
    // -------------------------------------------------------------------------
    private int currentHealth;
    private float nextFireTime = 0f;
    private bool isInvincible = false;
    private float invincibilityTimer = 0f;
    private SpriteRenderer spriteRenderer;

    // Power-up states
    private bool hasShield = false;
    private bool hasRapidFire = false;
    private float rapidFireTimer = 0f;
    private float rapidFireDuration = 5f;

    // -------------------------------------------------------------------------
    // Unity Lifecycle
    // -------------------------------------------------------------------------

    /// <summary>
    /// Initialize health and cache component references.
    /// </summary>
    void Start()
    {
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();

        // If no fire point is assigned, create one slightly above the ship
        if (firePoint == null)
        {
            GameObject fp = new GameObject("FirePoint");
            fp.transform.SetParent(transform);
            fp.transform.localPosition = new Vector3(0f, 0.8f, 0f);
            firePoint = fp.transform;
        }

        // Hide shield visual at start
        if (shieldVisual != null)
        {
            shieldVisual.SetActive(false);
        }

        // Notify UI of initial health
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateHealth(currentHealth, maxHealth);
        }
    }

    /// <summary>
    /// Handle input and update timers every frame.
    /// </summary>
    void Update()
    {
        HandleMovement();
        HandleShooting();
        HandleInvincibility();
        HandlePowerUpTimers();
    }

    // -------------------------------------------------------------------------
    // Movement
    // -------------------------------------------------------------------------

    /// <summary>
    /// Reads horizontal and vertical axis input and moves the player ship.
    /// Clamps position within defined screen boundaries.
    /// </summary>
    private void HandleMovement()
    {
        // Read input axes (WASD or Arrow Keys by default)
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        // Calculate movement vector
        Vector3 movement = new Vector3(horizontal, vertical, 0f) * moveSpeed * Time.deltaTime;
        transform.position += movement;

        // Clamp position to screen boundaries
        float clampedX = Mathf.Clamp(transform.position.x, minX, maxX);
        float clampedY = Mathf.Clamp(transform.position.y, minY, maxY);
        transform.position = new Vector3(clampedX, clampedY, 0f);
    }

    // -------------------------------------------------------------------------
    // Shooting
    // -------------------------------------------------------------------------

    /// <summary>
    /// Fires bullets when the player presses the fire button (Space or left mouse).
    /// Respects fire rate cooldown.
    /// </summary>
    private void HandleShooting()
    {
        // Fire on Space key or left mouse button
        if (Input.GetKey(KeyCode.Space) || Input.GetMouseButton(0))
        {
            if (Time.time >= nextFireTime)
            {
                Fire();
                float currentFireRate = hasRapidFire ? rapidFireRate : fireRate;
                nextFireTime = Time.time + currentFireRate;
            }
        }
    }

    /// <summary>
    /// Instantiates a bullet at the fire point position.
    /// </summary>
    private void Fire()
    {
        if (bulletPrefab != null && firePoint != null)
        {
            Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
            AudioManager.Instance?.PlaySFX("PlayerShoot");
        }
    }

    // -------------------------------------------------------------------------
    // Health & Damage
    // -------------------------------------------------------------------------

    /// <summary>
    /// Apply damage to the player. Respects shield and invincibility.
    /// </summary>
    /// <param name="damage">Amount of damage to apply.</param>
    public void TakeDamage(int damage)
    {
        // Shield absorbs one hit then breaks
        if (hasShield)
        {
            hasShield = false;
            if (shieldVisual != null) shieldVisual.SetActive(false);
            AudioManager.Instance?.PlaySFX("ShieldBreak");
            return;
        }

        // Ignore damage during invincibility frames
        if (isInvincible) return;

        currentHealth -= damage;

        // Update the health UI
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateHealth(currentHealth, maxHealth);
        }

        AudioManager.Instance?.PlaySFX("PlayerHit");

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            // Start invincibility frames after taking damage
            StartInvincibility();
        }
    }

    /// <summary>
    /// Heal the player by the given amount, capped at maxHealth.
    /// </summary>
    /// <param name="amount">Health points to restore.</param>
    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateHealth(currentHealth, maxHealth);
        }
    }

    /// <summary>
    /// Handles player death: plays explosion, notifies GameManager, destroys ship.
    /// </summary>
    private void Die()
    {
        // Spawn explosion effect
        if (explosionPrefab != null)
        {
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        }

        AudioManager.Instance?.PlaySFX("Explosion");

        // Notify game manager that the player is dead
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPlayerDeath();
        }

        // Destroy the player ship
        Destroy(gameObject);
    }

    // -------------------------------------------------------------------------
    // Invincibility
    // -------------------------------------------------------------------------

    /// <summary>
    /// Activates invincibility and starts the flashing effect.
    /// </summary>
    private void StartInvincibility()
    {
        isInvincible = true;
        invincibilityTimer = invincibilityDuration;
    }

    /// <summary>
    /// Counts down invincibility timer and flashes the sprite for feedback.
    /// </summary>
    private void HandleInvincibility()
    {
        if (!isInvincible) return;

        invincibilityTimer -= Time.deltaTime;

        // Flash the sprite on/off to indicate invincibility
        if (spriteRenderer != null)
        {
            // Toggle visibility every 0.1 seconds
            spriteRenderer.enabled = (Mathf.FloorToInt(invincibilityTimer * 10f) % 2 == 0);
        }

        if (invincibilityTimer <= 0f)
        {
            isInvincible = false;
            if (spriteRenderer != null) spriteRenderer.enabled = true;
        }
    }

    // -------------------------------------------------------------------------
    // Power-Ups
    // -------------------------------------------------------------------------

    /// <summary>
    /// Activates the shield power-up (absorbs one hit).
    /// </summary>
    public void ActivateShield()
    {
        hasShield = true;
        if (shieldVisual != null) shieldVisual.SetActive(true);
    }

    /// <summary>
    /// Activates rapid fire mode for a set duration.
    /// </summary>
    /// <param name="duration">How long rapid fire lasts in seconds.</param>
    public void ActivateRapidFire(float duration)
    {
        hasRapidFire = true;
        rapidFireDuration = duration;
        rapidFireTimer = duration;
    }

    /// <summary>
    /// Counts down rapid fire timer and deactivates when expired.
    /// </summary>
    private void HandlePowerUpTimers()
    {
        if (hasRapidFire)
        {
            rapidFireTimer -= Time.deltaTime;
            if (rapidFireTimer <= 0f)
            {
                hasRapidFire = false;
            }
        }
    }

    // -------------------------------------------------------------------------
    // Public Getters
    // -------------------------------------------------------------------------

    /// <summary>Returns the player's current health.</summary>
    public int GetCurrentHealth() { return currentHealth; }

    /// <summary>Returns the player's maximum health.</summary>
    public int GetMaxHealth() { return maxHealth; }

    /// <summary>Returns true if the shield is currently active.</summary>
    public bool HasShield() { return hasShield; }

    /// <summary>Returns true if rapid fire is currently active.</summary>
    public bool HasRapidFire() { return hasRapidFire; }
}

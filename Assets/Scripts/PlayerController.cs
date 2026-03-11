using UnityEngine;

/// <summary>
/// Controls the player ship: movement, shooting, health, and collision.
/// Attach this script to the Player GameObject.
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Speed of the player ship in units per second.")]
    public float moveSpeed = 8f;

    [Header("Shooting Settings")]
    [Tooltip("Prefab for the bullet the player fires.")]
    public GameObject bulletPrefab;

    [Tooltip("Spawn point for bullets (assign an empty child object above the ship).")]
    public Transform firePoint;

    [Tooltip("Minimum seconds between consecutive shots.")]
    public float fireRate = 0.2f;

    [Header("Health Settings")]
    [Tooltip("Maximum (and starting) health of the player.")]
    public int maxHealth = 5;

    [Header("Boundaries")]
    [Tooltip("How far left/right the player can move (world units from centre).")]
    public float horizontalBound = 8f;

    [Tooltip("How far up/down the player can move (world units from centre).")]
    public float verticalBound = 4.5f;

    // ---- Runtime state ----
    private int currentHealth;
    private float nextFireTime = 0f;
    private bool isAlive = true;

    // ---- Properties for UI / GameManager ----
    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;

    // =========================================================================
    // Unity Lifecycle
    // =========================================================================

    private void Start()
    {
        currentHealth = maxHealth;

        // Tell the UI about the initial health
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateHealth(currentHealth, maxHealth);
        }
    }

    private void Update()
    {
        if (!isAlive) return;

        HandleMovement();
        HandleShooting();
    }

    // =========================================================================
    // Movement
    // =========================================================================

    /// <summary>
    /// Reads WASD / Arrow-key input and moves the ship, clamped to bounds.
    /// </summary>
    private void HandleMovement()
    {
        float h = Input.GetAxis("Horizontal"); // A/D or Left/Right
        float v = Input.GetAxis("Vertical");   // W/S or Up/Down

        Vector3 movement = new Vector3(h, v, 0f) * moveSpeed * Time.deltaTime;
        transform.position += movement;

        // Clamp so the player stays on screen
        float clampedX = Mathf.Clamp(transform.position.x, -horizontalBound, horizontalBound);
        float clampedY = Mathf.Clamp(transform.position.y, -verticalBound, verticalBound);
        transform.position = new Vector3(clampedX, clampedY, 0f);
    }

    // =========================================================================
    // Shooting
    // =========================================================================

    /// <summary>
    /// Fires a bullet when Space is pressed (respecting fire rate).
    /// </summary>
    private void HandleShooting()
    {
        if (Input.GetKey(KeyCode.Space) && Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + fireRate;
            Shoot();
        }
    }

    /// <summary>
    /// Instantiates a bullet prefab at the fire point.
    /// </summary>
    private void Shoot()
    {
        if (bulletPrefab == null || firePoint == null)
        {
            Debug.LogWarning("PlayerController: bulletPrefab or firePoint is not assigned!");
            return;
        }

        Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
    }

    // =========================================================================
    // Damage & Death
    // =========================================================================

    /// <summary>
    /// Called when something damages the player.
    /// </summary>
    public void TakeDamage(int amount)
    {
        if (!isAlive) return;

        currentHealth -= amount;
        currentHealth = Mathf.Max(currentHealth, 0);

        // Update the UI
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateHealth(currentHealth, maxHealth);
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// Handles player death: disables the ship and tells GameManager.
    /// </summary>
    private void Die()
    {
        isAlive = false;

        // Notify the GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.GameOver();
        }

        // Hide the player (you could play a death animation here instead)
        gameObject.SetActive(false);
    }

    // =========================================================================
    // Collision
    // =========================================================================

    /// <summary>
    /// When an enemy ship collides with the player, the player takes damage.
    /// Requires both objects to have Collider2D (at least one set to IsTrigger)
    /// and Rigidbody2D.
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isAlive) return;

        if (other.CompareTag("Enemy"))
        {
            TakeDamage(1);

            // Destroy the enemy that hit us
            Destroy(other.gameObject);
        }
    }
}

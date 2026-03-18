using UnityEngine;

/// <summary>
/// Controls the player ship: movement, shooting, health, and power-up effects.
/// Attach this to the Player GameObject.
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 8f;

    [Header("Shooting")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float fireRate = 0.3f;
    public float bulletSpeed = 12f;

    [Header("Health")]
    public int maxHealth = 5;
    public int currentHealth;

    [Header("Rapid Fire Power-Up")]
    public float rapidFireRate = 0.1f;
    public float rapidFireDuration = 5f;

    // Internal state
    private float nextFireTime = 0f;
    private float currentFireRate;
    private float rapidFireEndTime = 0f;
    private bool isRapidFireActive = false;

    // Screen bounds (calculated from camera)
    private float minX, maxX, minY, maxY;
    private float shipHalfWidth = 0.4f;
    private float shipHalfHeight = 0.4f;

    void Start()
    {
        currentHealth = maxHealth;
        currentFireRate = fireRate;

        // Calculate screen bounds in world coordinates
        Camera cam = Camera.main;
        if (cam != null)
        {
            float camHeight = cam.orthographicSize;
            float camWidth = camHeight * cam.aspect;
            minX = -camWidth + shipHalfWidth;
            maxX = camWidth - shipHalfWidth;
            minY = -camHeight + shipHalfHeight;
            maxY = camHeight - shipHalfHeight;
        }

        // Update UI with initial health
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateHealth(currentHealth, maxHealth);
        }
    }

    void Update()
    {
        // Don't process input if the game is over
        if (GameManager.Instance != null && GameManager.Instance.isGameOver)
            return;

        HandleMovement();
        HandleShooting();
        HandlePowerUpTimers();
    }

    /// <summary>
    /// Handles WASD / Arrow key movement, clamped to screen bounds.
    /// </summary>
    void HandleMovement()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 movement = new Vector3(h, v, 0f).normalized * moveSpeed * Time.deltaTime;
        transform.position += movement;

        // Clamp position to screen bounds
        float clampedX = Mathf.Clamp(transform.position.x, minX, maxX);
        float clampedY = Mathf.Clamp(transform.position.y, minY, maxY);
        transform.position = new Vector3(clampedX, clampedY, 0f);
    }

    /// <summary>
    /// Fires a bullet upward when Space is pressed, respecting fire rate.
    /// </summary>
    void HandleShooting()
    {
        if (Input.GetKey(KeyCode.Space) && Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + currentFireRate;
            FireBullet();
        }
    }

    /// <summary>
    /// Instantiates a bullet at the fire point and sends it upward.
    /// </summary>
    void FireBullet()
    {
        if (bulletPrefab == null || firePoint == null)
        {
            Debug.LogWarning("PlayerController: bulletPrefab or firePoint not assigned!");
            return;
        }

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        Bullet bulletScript = bullet.GetComponent<Bullet>();
        if (bulletScript != null)
        {
            bulletScript.SetDirection(Vector2.up, bulletSpeed);
            bulletScript.isPlayerBullet = true;
        }
    }

    /// <summary>
    /// Manages active power-up timers (e.g., rapid fire expiry).
    /// </summary>
    void HandlePowerUpTimers()
    {
        if (isRapidFireActive && Time.time >= rapidFireEndTime)
        {
            isRapidFireActive = false;
            currentFireRate = fireRate;
            Debug.Log("Rapid fire ended.");
        }
    }

    /// <summary>
    /// Called when the player takes damage.
    /// </summary>
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);

        // Flash the sprite red briefly
        StartCoroutine(FlashDamage());

        // Update health UI
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
    /// Brief red flash to indicate damage taken.
    /// </summary>
    System.Collections.IEnumerator FlashDamage()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            Color original = sr.color;
            sr.color = Color.red;
            yield return new WaitForSeconds(0.15f);
            sr.color = original;
        }
    }

    /// <summary>
    /// Handles player death.
    /// </summary>
    void Die()
    {
        Debug.Log("Player died!");
        if (GameManager.Instance != null)
        {
            GameManager.Instance.GameOver();
        }
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Heals the player by the given amount (capped at maxHealth).
    /// </summary>
    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateHealth(currentHealth, maxHealth);
        }
    }

    /// <summary>
    /// Activates the rapid fire power-up for a set duration.
    /// </summary>
    public void ActivateRapidFire()
    {
        isRapidFireActive = true;
        currentFireRate = rapidFireRate;
        rapidFireEndTime = Time.time + rapidFireDuration;
        Debug.Log("Rapid fire activated!");
    }

    /// <summary>
    /// Handle collisions with enemies directly (contact damage).
    /// </summary>
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            TakeDamage(1);
            // Destroy the enemy on collision
            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.Die(false); // false = no score awarded for ram-kill
            }
        }
    }
}

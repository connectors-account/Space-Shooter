using UnityEngine;

/// <summary>
/// PlayerController - Handles player ship movement, shooting, health, and power-ups.
/// Attach to the Player GameObject (a quad/sprite with a Collider2D and Rigidbody2D).
/// Tag the Player GameObject as "Player".
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 8f;
    public float horizontalBound = 8.5f;
    public float verticalBound = 4.5f;

    [Header("Shooting")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float fireRate = 0.25f;
    private float nextFireTime = 0f;

    [Header("Rapid Fire Power-Up")]
    public float rapidFireRate = 0.1f;
    public float rapidFireDuration = 5f;
    private bool isRapidFire = false;
    private float rapidFireTimer = 0f;

    [Header("Shield Power-Up")]
    public GameObject shieldVisual;
    public float shieldDuration = 8f;
    private bool hasShield = false;
    private float shieldTimer = 0f;

    [Header("Health")]
    public int maxHealth = 3;
    public int currentHealth;

    [Header("Invincibility on Hit")]
    public float invincibilityDuration = 1.5f;
    private bool isInvincible = false;
    private float invincibilityTimer = 0f;

    private SpriteRenderer spriteRenderer;

    private void Start()
    {
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (shieldVisual != null)
        {
            shieldVisual.SetActive(false);
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.player = this;
        }

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateHealth(currentHealth, maxHealth);
        }
    }

    private void Update()
    {
        if (GameManager.Instance != null && !GameManager.Instance.isGameActive) return;

        HandleMovement();
        HandleShooting();
        HandlePowerUpTimers();
        HandleInvincibility();
    }

    /// <summary>
    /// WASD / Arrow key movement, clamped to screen bounds.
    /// </summary>
    private void HandleMovement()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 direction = new Vector3(h, v, 0f).normalized;
        transform.Translate(direction * moveSpeed * Time.deltaTime, Space.World);

        float clampedX = Mathf.Clamp(transform.position.x, -horizontalBound, horizontalBound);
        float clampedY = Mathf.Clamp(transform.position.y, -verticalBound, verticalBound);
        transform.position = new Vector3(clampedX, clampedY, 0f);
    }

    /// <summary>
    /// Fire bullets with Space key. Respects fire rate and rapid-fire power-up.
    /// </summary>
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
        Instantiate(bulletPrefab, spawnPos, Quaternion.identity);
    }

    /// <summary>
    /// Tick down active power-up durations.
    /// </summary>
    private void HandlePowerUpTimers()
    {
        if (isRapidFire)
        {
            rapidFireTimer -= Time.deltaTime;
            if (rapidFireTimer <= 0f)
            {
                isRapidFire = false;
                Debug.Log("Rapid Fire ended.");
            }
        }

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
    /// Flash the sprite during invincibility frames.
    /// </summary>
    private void HandleInvincibility()
    {
        if (isInvincible)
        {
            invincibilityTimer -= Time.deltaTime;
            if (spriteRenderer != null)
            {
                float alpha = Mathf.PingPong(Time.time * 8f, 1f) > 0.5f ? 1f : 0.3f;
                Color c = spriteRenderer.color;
                spriteRenderer.color = new Color(c.r, c.g, c.b, alpha);
            }
            if (invincibilityTimer <= 0f)
            {
                isInvincible = false;
                if (spriteRenderer != null)
                {
                    Color c = spriteRenderer.color;
                    spriteRenderer.color = new Color(c.r, c.g, c.b, 1f);
                }
            }
        }
    }

    /// <summary>
    /// Apply damage to the player. Shield absorbs damage if active.
    /// </summary>
    public void TakeDamage(int damage)
    {
        if (isInvincible) return;

        if (hasShield)
        {
            DeactivateShield();
            Debug.Log("Shield absorbed damage!");
            return;
        }

        currentHealth -= damage;

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateHealth(currentHealth, maxHealth);
        }

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
        Debug.Log("Player destroyed!");
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPlayerDeath();
        }
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Activate the rapid-fire power-up.
    /// </summary>
    public void ActivateRapidFire()
    {
        isRapidFire = true;
        rapidFireTimer = rapidFireDuration;
        Debug.Log("Rapid Fire activated!");
    }

    /// <summary>
    /// Activate the shield power-up.
    /// </summary>
    public void ActivateShield()
    {
        hasShield = true;
        shieldTimer = shieldDuration;
        if (shieldVisual != null)
        {
            shieldVisual.SetActive(true);
        }
        Debug.Log("Shield activated!");
    }

    private void DeactivateShield()
    {
        hasShield = false;
        if (shieldVisual != null)
        {
            shieldVisual.SetActive(false);
        }
        Debug.Log("Shield deactivated.");
    }

    /// <summary>
    /// Collision with enemies or enemy bullets.
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            TakeDamage(1);
            EnemyController enemy = other.GetComponent<EnemyController>();
            if (enemy != null)
            {
                enemy.TakeDamage(999);
            }
        }
        else if (other.CompareTag("EnemyBullet"))
        {
            TakeDamage(1);
            Destroy(other.gameObject);
        }
    }
}

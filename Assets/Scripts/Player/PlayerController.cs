using UnityEngine;

/// <summary>
/// Controls the player ship: movement, shooting, health, and invincibility frames.
/// Attach to the Player GameObject.
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 8f;
    public float horizontalLimit = 8f;
    public float verticalLimitTop = 4.5f;
    public float verticalLimitBottom = -4.5f;

    [Header("Shooting")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float fireRate = 0.2f;
    public AudioClip shootSound;

    [Header("Health")]
    public int maxHealth = 5;
    public float invincibilityDuration = 1.5f;

    private int currentHealth;
    private float nextFireTime;
    private bool isInvincible;
    private float invincibilityTimer;
    private SpriteRenderer spriteRenderer;
    private AudioSource audioSource;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;

    void Start()
    {
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    void Update()
    {
        if (GameManager.Instance != null && !GameManager.Instance.IsGameActive)
            return;

        HandleMovement();
        HandleShooting();
        HandleInvincibility();
    }

    void HandleMovement()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(h, v, 0f) * moveSpeed * Time.deltaTime;
        transform.position += movement;

        // Clamp position to screen bounds
        float clampedX = Mathf.Clamp(transform.position.x, -horizontalLimit, horizontalLimit);
        float clampedY = Mathf.Clamp(transform.position.y, verticalLimitBottom, verticalLimitTop);
        transform.position = new Vector3(clampedX, clampedY, 0f);
    }

    void HandleShooting()
    {
        if (Input.GetKey(KeyCode.Space) && Time.time >= nextFireTime)
        {
            Fire();
            nextFireTime = Time.time + fireRate;
        }
    }

    void Fire()
    {
        if (bulletPrefab == null) return;

        Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position + Vector3.up * 0.5f;
        Instantiate(bulletPrefab, spawnPos, Quaternion.identity);

        if (shootSound != null && audioSource != null)
            audioSource.PlayOneShot(shootSound, 0.5f);
    }

    void HandleInvincibility()
    {
        if (!isInvincible) return;

        invincibilityTimer -= Time.deltaTime;

        // Blink effect
        if (spriteRenderer != null)
        {
            float alpha = Mathf.PingPong(Time.time * 10f, 1f) > 0.5f ? 1f : 0.3f;
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

    public void TakeDamage(int damage)
    {
        if (isInvincible) return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);

        if (UIManager.Instance != null)
            UIManager.Instance.UpdateHealthDisplay(currentHealth, maxHealth);

        if (currentHealth <= 0)
        {
            Die();
            return;
        }

        // Start invincibility
        isInvincible = true;
        invincibilityTimer = invincibilityDuration;
    }

    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        if (UIManager.Instance != null)
            UIManager.Instance.UpdateHealthDisplay(currentHealth, maxHealth);
    }

    void Die()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.GameOver();

        gameObject.SetActive(false);
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
            TakeDamage(2);
        }
        else if (other.CompareTag("HealthPickup"))
        {
            Heal(1);
            Destroy(other.gameObject);
        }
    }
}

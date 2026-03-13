using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 8f;
    public float horizontalBoundary = 8f;
    public float verticalBoundaryTop = 4f;
    public float verticalBoundaryBottom = -4f;

    [Header("Shooting Settings")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float fireRate = 0.2f;
    public float rapidFireRate = 0.08f;
    public float bulletSpeed = 15f;

    [Header("Health Settings")]
    public int maxHealth = 3;
    public float invincibilityDuration = 1.5f;

    [Header("Visual Feedback")]
    public SpriteRenderer spriteRenderer;

    private int currentHealth;
    private float nextFireTime = 0f;
    private bool isInvincible = false;
    private bool hasRapidFire = false;
    private bool hasShield = false;
    private float rapidFireEndTime = 0f;
    private float shieldEndTime = 0f;
    private float invincibilityEndTime = 0f;

    private GameManager gameManager;
    private AudioManager audioManager;

    void Start()
    {
        currentHealth = maxHealth;
        gameManager = FindObjectOfType<GameManager>();
        audioManager = FindObjectOfType<AudioManager>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (firePoint == null)
            firePoint = transform;
    }

    void Update()
    {
        if (gameManager != null && !gameManager.IsGameActive())
            return;

        HandleMovement();
        HandleShooting();
        HandlePowerUpTimers();
        HandleInvincibility();
    }

    void HandleMovement()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(horizontalInput, verticalInput, 0f) * moveSpeed * Time.deltaTime;
        transform.position += movement;

        float clampedX = Mathf.Clamp(transform.position.x, -horizontalBoundary, horizontalBoundary);
        float clampedY = Mathf.Clamp(transform.position.y, verticalBoundaryBottom, verticalBoundaryTop);
        transform.position = new Vector3(clampedX, clampedY, transform.position.z);
    }

    void HandleShooting()
    {
        if (Input.GetKey(KeyCode.Space) && Time.time >= nextFireTime)
        {
            Shoot();
            float currentFireRate = hasRapidFire ? rapidFireRate : fireRate;
            nextFireTime = Time.time + currentFireRate;
        }
    }

    void Shoot()
    {
        if (bulletPrefab != null)
        {
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
            BulletController bulletController = bullet.GetComponent<BulletController>();
            if (bulletController != null)
            {
                bulletController.Initialize(Vector2.up, bulletSpeed, true);
            }

            if (audioManager != null)
                audioManager.PlayShootSound();
        }
    }

    void HandlePowerUpTimers()
    {
        if (hasRapidFire && Time.time >= rapidFireEndTime)
        {
            hasRapidFire = false;
        }

        if (hasShield && Time.time >= shieldEndTime)
        {
            hasShield = false;
            UpdateShieldVisual();
        }
    }

    void HandleInvincibility()
    {
        if (isInvincible)
        {
            float flashSpeed = 10f;
            float alpha = Mathf.Abs(Mathf.Sin(Time.time * flashSpeed));
            if (spriteRenderer != null)
            {
                Color color = spriteRenderer.color;
                color.a = 0.3f + alpha * 0.7f;
                spriteRenderer.color = color;
            }

            if (Time.time >= invincibilityEndTime)
            {
                isInvincible = false;
                if (spriteRenderer != null)
                {
                    Color color = spriteRenderer.color;
                    color.a = 1f;
                    spriteRenderer.color = color;
                }
            }
        }
    }

    public void TakeDamage(int damage)
    {
        if (isInvincible || hasShield)
        {
            if (hasShield)
            {
                hasShield = false;
                UpdateShieldVisual();
                if (audioManager != null)
                    audioManager.PlayShieldBreakSound();
            }
            return;
        }

        currentHealth -= damage;

        if (audioManager != null)
            audioManager.PlayPlayerHitSound();

        if (gameManager != null)
            gameManager.UpdatePlayerHealth(currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            isInvincible = true;
            invincibilityEndTime = Time.time + invincibilityDuration;
        }
    }

    void Die()
    {
        if (audioManager != null)
            audioManager.PlayExplosionSound();

        if (gameManager != null)
            gameManager.GameOver();

        gameObject.SetActive(false);
    }

    public void ActivateRapidFire(float duration)
    {
        hasRapidFire = true;
        rapidFireEndTime = Time.time + duration;
    }

    public void ActivateShield(float duration)
    {
        hasShield = true;
        shieldEndTime = Time.time + duration;
        UpdateShieldVisual();
    }

    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        if (gameManager != null)
            gameManager.UpdatePlayerHealth(currentHealth);
    }

    void UpdateShieldVisual()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = hasShield ? new Color(0.5f, 0.5f, 1f, 1f) : Color.white;
        }
    }

    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    public int GetMaxHealth()
    {
        return maxHealth;
    }

    public bool HasShield()
    {
        return hasShield;
    }

    public bool HasRapidFire()
    {
        return hasRapidFire;
    }
}

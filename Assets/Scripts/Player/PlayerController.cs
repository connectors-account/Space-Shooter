using UnityEngine;

/// <summary>
/// Controls the player ship: movement, shooting, health, invincibility frames,
/// and weapon power-up levels.
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float smoothTime = 0.05f;

    [Header("Shooting")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireRate = 0.2f;

    [Header("Health")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private float invincibilityDuration = 1.5f;

    [Header("Bounds")]
    [SerializeField] private float boundaryPadding = 0.5f;

    // Internal state
    private int currentHealth;
    private int weaponLevel = 1;
    private bool isInvincible = false;
    private float invincibilityTimer = 0f;
    private float fireTimer = 0f;
    private bool hasShield = false;
    private Vector2 velocity;
    private Vector2 currentVelocity;
    private SpriteRenderer spriteRenderer;
    private Camera mainCam;

    // Properties
    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public int WeaponLevel => weaponLevel;
    public bool HasShield => hasShield;
    public bool IsAlive => currentHealth > 0;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        mainCam = Camera.main;
    }

    private void Start()
    {
        currentHealth = maxHealth;
    }

    private void Update()
    {
        if (!IsAlive) return;
        if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing) return;

        HandleMovement();
        HandleShooting();
        HandleInvincibility();
    }

    private void HandleMovement()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector2 targetVelocity = new Vector2(horizontal, vertical).normalized * moveSpeed;
        velocity = Vector2.SmoothDamp(velocity, targetVelocity, ref currentVelocity, smoothTime);

        transform.Translate(velocity * Time.deltaTime, Space.World);

        ClampPosition();
    }

    private void ClampPosition()
    {
        if (mainCam == null) return;

        Vector3 pos = transform.position;
        float halfHeight = mainCam.orthographicSize;
        float halfWidth = halfHeight * mainCam.aspect;

        pos.x = Mathf.Clamp(pos.x, -halfWidth + boundaryPadding, halfWidth - boundaryPadding);
        pos.y = Mathf.Clamp(pos.y, -halfHeight + boundaryPadding, halfHeight - boundaryPadding);

        transform.position = pos;
    }

    private void HandleShooting()
    {
        fireTimer -= Time.deltaTime;

        if (Input.GetKey(KeyCode.Space) && fireTimer <= 0f)
        {
            Shoot();
            fireTimer = fireRate;
        }
    }

    private void Shoot()
    {
        if (bulletPrefab == null) return;

        AudioManager.Instance?.PlaySFX("PlayerShoot");

        switch (weaponLevel)
        {
            case 1: // Single shot
                SpawnBullet(Vector3.zero, Quaternion.identity);
                break;

            case 2: // Double shot
                SpawnBullet(new Vector3(-0.2f, 0, 0), Quaternion.identity);
                SpawnBullet(new Vector3(0.2f, 0, 0), Quaternion.identity);
                break;

            case 3: // Triple spread
                SpawnBullet(Vector3.zero, Quaternion.identity);
                SpawnBullet(new Vector3(-0.15f, 0, 0), Quaternion.Euler(0, 0, 10f));
                SpawnBullet(new Vector3(0.15f, 0, 0), Quaternion.Euler(0, 0, -10f));
                break;

            default: // Level 4+: Five spread
                SpawnBullet(Vector3.zero, Quaternion.identity);
                SpawnBullet(new Vector3(-0.15f, 0, 0), Quaternion.Euler(0, 0, 10f));
                SpawnBullet(new Vector3(0.15f, 0, 0), Quaternion.Euler(0, 0, -10f));
                SpawnBullet(new Vector3(-0.3f, 0, 0), Quaternion.Euler(0, 0, 20f));
                SpawnBullet(new Vector3(0.3f, 0, 0), Quaternion.Euler(0, 0, -20f));
                break;
        }
    }

    private void SpawnBullet(Vector3 offset, Quaternion rotation)
    {
        Vector3 spawnPos = (firePoint != null ? firePoint.position : transform.position + Vector3.up * 0.5f) + offset;
        GameObject bullet = Instantiate(bulletPrefab, spawnPos, rotation);
        Bullet b = bullet.GetComponent<Bullet>();
        if (b != null)
        {
            b.Initialize(true, weaponLevel >= 3 ? 15 : 10);
        }
    }

    public void TakeDamage(int damage)
    {
        if (isInvincible || !IsAlive) return;

        if (hasShield)
        {
            hasShield = false;
            AudioManager.Instance?.PlaySFX("ShieldBreak");
            StartInvincibility();
            UIManager.Instance?.UpdateShieldIndicator(false);
            return;
        }

        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);

        AudioManager.Instance?.PlaySFX("PlayerHit");
        UIManager.Instance?.UpdateHealthBar((float)currentHealth / maxHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            StartInvincibility();
        }
    }

    private void Die()
    {
        AudioManager.Instance?.PlaySFX("PlayerExplosion");
        GameManager.Instance?.OnPlayerDeath();

        // Visual: hide sprite, we keep the GO so GameManager can respawn
        if (spriteRenderer != null) spriteRenderer.enabled = false;
    }

    public void Respawn()
    {
        currentHealth = maxHealth;
        weaponLevel = 1;
        hasShield = false;
        transform.position = new Vector3(0, -3.5f, 0);
        if (spriteRenderer != null) spriteRenderer.enabled = true;
        StartInvincibility();
        UIManager.Instance?.UpdateHealthBar(1f);
        UIManager.Instance?.UpdateShieldIndicator(false);
    }

    private void StartInvincibility()
    {
        isInvincible = true;
        invincibilityTimer = invincibilityDuration;
    }

    private void HandleInvincibility()
    {
        if (!isInvincible) return;

        invincibilityTimer -= Time.deltaTime;

        // Flash effect
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

    // --- Power-up Methods ---

    public void UpgradeWeapon()
    {
        weaponLevel = Mathf.Min(weaponLevel + 1, 5);
        AudioManager.Instance?.PlaySFX("PowerUp");
    }

    public void ActivateShield()
    {
        hasShield = true;
        AudioManager.Instance?.PlaySFX("PowerUp");
        UIManager.Instance?.UpdateShieldIndicator(true);
    }

    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        AudioManager.Instance?.PlaySFX("PowerUp");
        UIManager.Instance?.UpdateHealthBar((float)currentHealth / maxHealth);
    }
}

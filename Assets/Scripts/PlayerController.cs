using UnityEngine;

/// <summary>
/// Controls player ship movement, shooting, and health.
/// Attach to the Player GameObject (a simple triangle/arrow shape).
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 8f;
    public float padding = 0.5f;

    [Header("Shooting")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float fireRate = 0.2f;
    public int weaponLevel = 1;

    [Header("Health")]
    public int maxHealth = 5;
    public int currentHealth;

    [Header("Invincibility")]
    public float invincibilityDuration = 1.5f;
    private float invincibilityTimer = 0f;
    private bool isInvincible = false;
    private SpriteRenderer spriteRenderer;

    private float nextFireTime = 0f;
    private float minX, maxX, minY, maxY;

    void Start()
    {
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();
        CalculateBounds();

        if (UIManager.Instance != null)
            UIManager.Instance.UpdateHealth(currentHealth, maxHealth);
    }

    void CalculateBounds()
    {
        Camera cam = Camera.main;
        if (cam == null) return;
        minX = cam.ViewportToWorldPoint(Vector3.zero).x + padding;
        maxX = cam.ViewportToWorldPoint(Vector3.right).x - padding;
        minY = cam.ViewportToWorldPoint(Vector3.zero).y + padding;
        maxY = cam.ViewportToWorldPoint(Vector3.up).y - padding;
    }

    void Update()
    {
        if (GameManager.Instance != null && !GameManager.Instance.isGameActive) return;

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
        float clampedX = Mathf.Clamp(transform.position.x, minX, maxX);
        float clampedY = Mathf.Clamp(transform.position.y, minY, maxY);
        transform.position = new Vector3(clampedX, clampedY, 0f);
    }

    void HandleShooting()
    {
        if (Input.GetKey(KeyCode.Space) && Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + fireRate;
            Shoot();
        }
    }

    void Shoot()
    {
        if (bulletPrefab == null) return;

        Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position + Vector3.up * 0.6f;

        // Single shot
        GameObject bullet = Instantiate(bulletPrefab, spawnPos, Quaternion.identity);
        BulletController bc = bullet.GetComponent<BulletController>();
        if (bc != null) bc.isPlayerBullet = true;

        // Spread shots for higher weapon levels
        if (weaponLevel >= 2)
        {
            GameObject bL = Instantiate(bulletPrefab, spawnPos + Vector3.left * 0.3f, Quaternion.Euler(0, 0, 10));
            BulletController bcL = bL.GetComponent<BulletController>();
            if (bcL != null) bcL.isPlayerBullet = true;

            GameObject bR = Instantiate(bulletPrefab, spawnPos + Vector3.right * 0.3f, Quaternion.Euler(0, 0, -10));
            BulletController bcR = bR.GetComponent<BulletController>();
            if (bcR != null) bcR.isPlayerBullet = true;
        }

        if (weaponLevel >= 3)
        {
            GameObject bFL = Instantiate(bulletPrefab, spawnPos + Vector3.left * 0.5f, Quaternion.Euler(0, 0, 20));
            BulletController bcFL = bFL.GetComponent<BulletController>();
            if (bcFL != null) bcFL.isPlayerBullet = true;

            GameObject bFR = Instantiate(bulletPrefab, spawnPos + Vector3.right * 0.5f, Quaternion.Euler(0, 0, -20));
            BulletController bcFR = bFR.GetComponent<BulletController>();
            if (bcFR != null) bcFR.isPlayerBullet = true;
        }

        // Play sound if AudioManager exists
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayShoot();
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
            UIManager.Instance.UpdateHealth(currentHealth, maxHealth);

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayHit();

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
            UIManager.Instance.UpdateHealth(currentHealth, maxHealth);
    }

    public void UpgradeWeapon()
    {
        weaponLevel = Mathf.Min(weaponLevel + 1, 3);
    }

    void Die()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayExplosion();

        if (GameManager.Instance != null)
            GameManager.Instance.GameOver();

        gameObject.SetActive(false);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            TakeDamage(1);
            EnemyController enemy = other.GetComponent<EnemyController>();
            if (enemy != null) enemy.TakeDamage(999);
        }
    }
}

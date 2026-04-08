using UnityEngine;

/// <summary>
/// Controls the player ship: movement, shooting, health, and power-up state.
/// Attach to the Player GameObject.
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 8f;
    public float padding = 0.5f; // screen-edge padding

    [Header("Shooting")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float fireRate = 0.25f;
    public float bulletSpeed = 12f;

    [Header("Health")]
    public int maxHealth = 5;
    public float invincibleDuration = 1.5f;

    [Header("Audio")]
    public string shootSfx = "PlayerShoot";
    public string hitSfx   = "PlayerHit";
    public string deathSfx = "PlayerDeath";

    // Runtime state
    private int   currentHealth;
    private float nextFireTime;
    private bool  isInvincible;
    private float invincibleTimer;
    private bool  hasShield;
    private bool  hasRapidFire;
    private float rapidFireTimer;
    private float rapidFireDuration = 8f;
    private SpriteRenderer spriteRenderer;

    // Screen bounds (world units)
    private float minX, maxX, minY, maxY;

    void Start()
    {
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();
        CalculateScreenBounds();

        if (UIManager.Instance != null)
            UIManager.Instance.UpdateHealth(currentHealth, maxHealth);
    }

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameManager.GameState.Playing)
            return;

        HandleMovement();
        HandleShooting();
        HandleTimers();
    }

    void HandleMovement()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector3 dir = new Vector3(h, v, 0f).normalized;
        transform.position += dir * moveSpeed * Time.deltaTime;

        // Clamp to screen
        float cx = Mathf.Clamp(transform.position.x, minX + padding, maxX - padding);
        float cy = Mathf.Clamp(transform.position.y, minY + padding, maxY - padding);
        transform.position = new Vector3(cx, cy, 0f);
    }

    void HandleShooting()
    {
        if (Input.GetKey(KeyCode.Space) && Time.time >= nextFireTime)
        {
            float rate = hasRapidFire ? fireRate * 0.4f : fireRate;
            nextFireTime = Time.time + rate;
            Shoot();
        }
    }

    void Shoot()
    {
        if (bulletPrefab == null) return;

        Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position + Vector3.up * 0.6f;
        GameObject bullet = Instantiate(bulletPrefab, spawnPos, Quaternion.identity);
        Bullet b = bullet.GetComponent<Bullet>();
        if (b != null)
        {
            b.SetDirection(Vector2.up, bulletSpeed);
            b.isPlayerBullet = true;
        }

        // Rapid fire: also fire two diagonal bullets
        if (hasRapidFire)
        {
            GameObject bL = Instantiate(bulletPrefab, spawnPos, Quaternion.identity);
            Bullet blc = bL.GetComponent<Bullet>();
            if (blc != null) { blc.SetDirection(new Vector2(-0.2f, 1f).normalized, bulletSpeed); blc.isPlayerBullet = true; }

            GameObject bR = Instantiate(bulletPrefab, spawnPos, Quaternion.identity);
            Bullet brc = bR.GetComponent<Bullet>();
            if (brc != null) { brc.SetDirection(new Vector2(0.2f, 1f).normalized, bulletSpeed); brc.isPlayerBullet = true; }
        }

        AudioManager.PlaySfx(shootSfx);
    }

    void HandleTimers()
    {
        // Invincibility blink
        if (isInvincible)
        {
            invincibleTimer -= Time.deltaTime;
            if (spriteRenderer != null)
            {
                // Blink effect
                float alpha = Mathf.PingPong(Time.time * 10f, 1f) > 0.5f ? 1f : 0.3f;
                Color c = spriteRenderer.color;
                c.a = alpha;
                spriteRenderer.color = c;
            }
            if (invincibleTimer <= 0f)
            {
                isInvincible = false;
                if (spriteRenderer != null)
                {
                    Color c = spriteRenderer.color; c.a = 1f; spriteRenderer.color = c;
                }
            }
        }

        // Rapid fire timer
        if (hasRapidFire)
        {
            rapidFireTimer -= Time.deltaTime;
            if (rapidFireTimer <= 0f)
                hasRapidFire = false;
        }
    }

    /// <summary>Called when something damages the player.</summary>
    public void TakeDamage(int amount)
    {
        if (isInvincible || hasShield)
        {
            if (hasShield) { hasShield = false; AudioManager.PlaySfx(hitSfx); }
            return;
        }

        currentHealth -= amount;
        AudioManager.PlaySfx(hitSfx);

        if (UIManager.Instance != null)
            UIManager.Instance.UpdateHealth(currentHealth, maxHealth);

        if (currentHealth <= 0)
        {
            Die();
            return;
        }

        // Brief invincibility
        isInvincible = true;
        invincibleTimer = invincibleDuration;
    }

    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        if (UIManager.Instance != null)
            UIManager.Instance.UpdateHealth(currentHealth, maxHealth);
    }

    public void ActivateShield()
    {
        hasShield = true;
    }

    public void ActivateRapidFire(float duration)
    {
        hasRapidFire = true;
        rapidFireTimer = duration > 0 ? duration : rapidFireDuration;
    }

    void Die()
    {
        AudioManager.PlaySfx(deathSfx);
        if (GameManager.Instance != null)
            GameManager.Instance.GameOver();
        gameObject.SetActive(false);
    }

    void CalculateScreenBounds()
    {
        Camera cam = Camera.main;
        if (cam == null) return;
        Vector3 bl = cam.ViewportToWorldPoint(new Vector3(0, 0, cam.nearClipPlane));
        Vector3 tr = cam.ViewportToWorldPoint(new Vector3(1, 1, cam.nearClipPlane));
        minX = bl.x; maxX = tr.x;
        minY = bl.y; maxY = tr.y;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Enemy collision
        if (other.CompareTag("Enemy"))
        {
            TakeDamage(1);
            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy != null) enemy.TakeDamage(999);
        }

        // Enemy bullet
        if (other.CompareTag("EnemyBullet"))
        {
            TakeDamage(1);
            Destroy(other.gameObject);
        }

        // Power-up
        if (other.CompareTag("PowerUp"))
        {
            PowerUp pu = other.GetComponent<PowerUp>();
            if (pu != null) pu.Apply(this);
        }
    }
}

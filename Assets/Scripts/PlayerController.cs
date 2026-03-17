using UnityEngine;

/// <summary>
/// Handles player spaceship movement, shooting, power-up state, and
/// invincibility frames after taking damage.
/// </summary>
[RequireComponent(typeof(SpriteRenderer), typeof(BoxCollider2D))]
public class PlayerController : MonoBehaviour
{
    // ── Tunables ─────────────────────────────────────────────────────
    [Header("Movement")]
    [SerializeField] private float moveSpeed       = 8f;
    [SerializeField] private float boundaryPadding = 0.5f;

    [Header("Shooting")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float      fireRate        = 0.2f;
    [SerializeField] private float      bulletSpeed     = 12f;
    [SerializeField] private Transform  firePoint;

    [Header("Power-Up Durations")]
    [SerializeField] private float spreadShotDuration = 8f;
    [SerializeField] private float shieldDuration     = 10f;

    [Header("Invincibility")]
    [SerializeField] private float invincibilityTime  = 1.5f;

    // ── Runtime state ────────────────────────────────────────────────
    private float nextFireTime;
    private bool  isInvincible;
    private float invincibilityTimer;

    // Power-up flags
    private bool  hasSpreadShot;
    private float spreadShotTimer;
    private bool  hasShield;
    private float shieldTimer;
    private GameObject shieldVisual;

    // Screen bounds (world-space)
    private Vector2 minBounds;
    private Vector2 maxBounds;

    // ── Unity lifecycle ──────────────────────────────────────────────
    private void Start()
    {
        CalculateBounds();
        CreateShieldVisual();
    }

    private void Update()
    {
        if (GameManager.Instance != null && (GameManager.Instance.IsGameOver || GameManager.Instance.IsPaused))
            return;

        HandleMovement();
        HandleShooting();
        HandleInvincibility();
        HandlePowerUpTimers();
    }

    // ── Movement ─────────────────────────────────────────────────────
    private void HandleMovement()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector3 dir = new Vector3(h, v, 0f).normalized;
        transform.position += dir * moveSpeed * Time.deltaTime;

        // Clamp to screen
        float x = Mathf.Clamp(transform.position.x, minBounds.x + boundaryPadding, maxBounds.x - boundaryPadding);
        float y = Mathf.Clamp(transform.position.y, minBounds.y + boundaryPadding, maxBounds.y - boundaryPadding);
        transform.position = new Vector3(x, y, 0f);
    }

    // ── Shooting ─────────────────────────────────────────────────────
    private void HandleShooting()
    {
        if (Input.GetKey(KeyCode.Space) && Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + fireRate;
            Shoot();
        }
    }

    private void Shoot()
    {
        if (bulletPrefab == null) return;

        Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position + Vector3.up * 0.6f;

        if (hasSpreadShot)
        {
            // 3-way spread shot
            SpawnBullet(spawnPos, Quaternion.identity);
            SpawnBullet(spawnPos, Quaternion.Euler(0, 0, 15f));
            SpawnBullet(spawnPos, Quaternion.Euler(0, 0, -15f));
        }
        else
        {
            SpawnBullet(spawnPos, Quaternion.identity);
        }

        AudioManager.Instance?.PlaySFX("PlayerShoot");
    }

    private void SpawnBullet(Vector3 pos, Quaternion rotation)
    {
        GameObject bullet = Instantiate(bulletPrefab, pos, rotation);
        BulletController bc = bullet.GetComponent<BulletController>();
        if (bc != null)
        {
            bc.Init(rotation * Vector3.up, bulletSpeed, true);
        }
    }

    // ── Damage & Invincibility ───────────────────────────────────────
    public void TakeDamage(int amount)
    {
        if (isInvincible || hasShield) 
        {
            if (hasShield)
            {
                DeactivateShield();
            }
            return;
        }

        GameManager.Instance?.TakeDamage(amount);
        AudioManager.Instance?.PlaySFX("PlayerHit");

        isInvincible = true;
        invincibilityTimer = invincibilityTime;
    }

    private void HandleInvincibility()
    {
        if (!isInvincible) return;
        invincibilityTimer -= Time.deltaTime;

        // Blink effect
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            Color c = sr.color;
            c.a = Mathf.PingPong(Time.time * 10f, 1f) > 0.5f ? 1f : 0.3f;
            sr.color = c;
        }

        if (invincibilityTimer <= 0f)
        {
            isInvincible = false;
            if (sr != null) { Color c = sr.color; c.a = 1f; sr.color = c; }
        }
    }

    // ── Power-ups ────────────────────────────────────────────────────
    public void ActivateSpreadShot()
    {
        hasSpreadShot = true;
        spreadShotTimer = spreadShotDuration;
    }

    public void ActivateShield()
    {
        hasShield = true;
        shieldTimer = shieldDuration;
        if (shieldVisual != null) shieldVisual.SetActive(true);
    }

    private void DeactivateShield()
    {
        hasShield = false;
        shieldTimer = 0f;
        if (shieldVisual != null) shieldVisual.SetActive(false);
        AudioManager.Instance?.PlaySFX("ShieldBreak");
    }

    public void HealPlayer(int amount)
    {
        GameManager.Instance?.Heal(amount);
    }

    private void HandlePowerUpTimers()
    {
        if (hasSpreadShot)
        {
            spreadShotTimer -= Time.deltaTime;
            if (spreadShotTimer <= 0f) hasSpreadShot = false;
        }
        if (hasShield)
        {
            shieldTimer -= Time.deltaTime;
            if (shieldTimer <= 0f) DeactivateShield();
        }
    }

    // ── Helpers ──────────────────────────────────────────────────────
    private void CalculateBounds()
    {
        Camera cam = Camera.main;
        if (cam == null) return;
        minBounds = cam.ViewportToWorldPoint(new Vector3(0, 0, cam.nearClipPlane));
        maxBounds = cam.ViewportToWorldPoint(new Vector3(1, 1, cam.nearClipPlane));
    }

    private void CreateShieldVisual()
    {
        shieldVisual = new GameObject("ShieldVisual");
        shieldVisual.transform.SetParent(transform);
        shieldVisual.transform.localPosition = Vector3.zero;

        SpriteRenderer sr = shieldVisual.AddComponent<SpriteRenderer>();
        sr.sprite = CreateCircleSprite(32, new Color(0.3f, 0.7f, 1f, 0.35f));
        sr.sortingOrder = 5;
        shieldVisual.transform.localScale = Vector3.one * 2.5f;
        shieldVisual.SetActive(false);
    }

    /// <summary>Creates a simple circle sprite at runtime.</summary>
    private Sprite CreateCircleSprite(int radius, Color color)
    {
        int size = radius * 2;
        Texture2D tex = new Texture2D(size, size);
        tex.filterMode = FilterMode.Bilinear;
        Color clear = new Color(0, 0, 0, 0);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(radius, radius));
                tex.SetPixel(x, y, dist < radius ? color : clear);
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), Vector2.one * 0.5f, 100f);
    }
}

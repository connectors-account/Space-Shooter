using UnityEngine;

/// <summary>
/// PlayerController – Handles player movement, shooting, health, and power-ups.
/// Attach to the Player GameObject (a colored triangle/square sprite).
/// Requires: Rigidbody2D, BoxCollider2D (Is Trigger = true)
/// </summary>
public class PlayerController : MonoBehaviour
{
    // ── Movement ──
    [Header("Movement")]
    [Tooltip("Horizontal movement speed")]
    public float moveSpeed = 8f;

    // ── Shooting ──
    [Header("Shooting")]
    [Tooltip("Prefab for the bullet")]
    public GameObject bulletPrefab;

    [Tooltip("Normal fire rate (shots per second)")]
    public float fireRate = 4f;

    [Tooltip("Rapid-fire rate when power-up is active")]
    public float rapidFireRate = 10f;

    [Tooltip("Offset above player where bullets spawn")]
    public float bulletSpawnOffset = 0.6f;

    // ── Health ──
    [Header("Health")]
    [Tooltip("Starting number of lives")]
    public int maxHealth = 3;

    public int CurrentHealth { get; private set; }

    // ── Shield Power-Up ──
    [Header("Shield")]
    [Tooltip("Reference to child shield visual (set in Inspector or created at runtime)")]
    public GameObject shieldVisual;

    // ── Internal state ──
    private float nextFireTime = 0f;
    private bool hasRapidFire = false;
    private float rapidFireEndTime = 0f;
    private bool hasShield = false;
    private float shieldEndTime = 0f;

    // Screen boundaries (calculated from camera)
    private float screenLeft;
    private float screenRight;

    // ────────────────────────────────────────────
    void Start()
    {
        CurrentHealth = maxHealth;

        // Calculate screen edges in world coordinates
        Camera cam = Camera.main;
        float halfWidth = cam.orthographicSize * cam.aspect;
        screenLeft = -halfWidth + 0.3f;
        screenRight = halfWidth - 0.3f;

        // Create shield visual if not assigned
        if (shieldVisual == null)
        {
            shieldVisual = CreateShieldVisual();
        }
        shieldVisual.SetActive(false);
    }

    void Update()
    {
        if (GameManager.Instance != null &&
            GameManager.Instance.CurrentState != GameManager.GameState.Playing)
            return;

        HandleMovement();
        HandleShooting();
        HandlePowerUpTimers();
    }

    // ── Movement ──
    void HandleMovement()
    {
        // Support both Arrow keys and WASD via Unity's built-in Horizontal axis
        float h = Input.GetAxis("Horizontal");
        Vector3 pos = transform.position;
        pos.x += h * moveSpeed * Time.deltaTime;
        pos.x = Mathf.Clamp(pos.x, screenLeft, screenRight);
        transform.position = pos;
    }

    // ── Shooting ──
    void HandleShooting()
    {
        float currentRate = hasRapidFire ? rapidFireRate : fireRate;

        // Spacebar to shoot (also mapped to Fire1)
        if (Input.GetKey(KeyCode.Space) && Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + 1f / currentRate;
            FireBullet();
        }
    }

    void FireBullet()
    {
        if (bulletPrefab == null) return;

        Vector3 spawnPos = transform.position + Vector3.up * bulletSpawnOffset;
        Instantiate(bulletPrefab, spawnPos, Quaternion.identity);
    }

    // ── Power-Up Timers ──
    void HandlePowerUpTimers()
    {
        if (hasRapidFire && Time.time >= rapidFireEndTime)
        {
            hasRapidFire = false;
        }

        if (hasShield && Time.time >= shieldEndTime)
        {
            hasShield = false;
            if (shieldVisual != null) shieldVisual.SetActive(false);
        }
    }

    // ── Public: Apply Power-Ups ──

    /// <summary>Activate rapid fire for the given duration.</summary>
    public void ActivateRapidFire(float duration)
    {
        hasRapidFire = true;
        rapidFireEndTime = Time.time + duration;
    }

    /// <summary>Activate shield for the given duration.</summary>
    public void ActivateShield(float duration)
    {
        hasShield = true;
        shieldEndTime = Time.time + duration;
        if (shieldVisual != null) shieldVisual.SetActive(true);
    }

    // ── Collision Handling ──
    void OnTriggerEnter2D(Collider2D other)
    {
        // Hit by an enemy
        if (other.CompareTag("Enemy"))
        {
            if (hasShield)
            {
                // Shield absorbs the hit – destroy the enemy
                Destroy(other.gameObject);
                return;
            }

            TakeDamage(1);
            Destroy(other.gameObject);
        }
    }

    /// <summary>Reduce health and check for death.</summary>
    public void TakeDamage(int amount)
    {
        CurrentHealth -= amount;
        CurrentHealth = Mathf.Max(CurrentHealth, 0);

        // Update HUD
        if (HUDManager.Instance != null)
            HUDManager.Instance.UpdateHealth(CurrentHealth);

        if (CurrentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        // Notify game manager
        if (GameManager.Instance != null)
            GameManager.Instance.TriggerGameOver();

        // Disable player visuals but keep the object for reference
        GetComponent<SpriteRenderer>().enabled = false;
        GetComponent<Collider2D>().enabled = false;
        this.enabled = false;
    }

    // ── Helper: Create a simple shield circle visual ──
    GameObject CreateShieldVisual()
    {
        GameObject shield = new GameObject("ShieldVisual");
        shield.transform.SetParent(transform);
        shield.transform.localPosition = Vector3.zero;

        SpriteRenderer sr = shield.AddComponent<SpriteRenderer>();
        sr.sprite = CreateCircleSprite(32);
        sr.color = new Color(0.3f, 0.8f, 1f, 0.35f); // translucent cyan
        shield.transform.localScale = Vector3.one * 2.5f;
        sr.sortingOrder = 5;

        return shield;
    }

    /// <summary>Generates a simple filled-circle sprite at runtime.</summary>
    static Sprite CreateCircleSprite(int resolution)
    {
        Texture2D tex = new Texture2D(resolution, resolution);
        tex.filterMode = FilterMode.Bilinear;
        float center = resolution / 2f;
        float radius = center - 1;

        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                tex.SetPixel(x, y, dist <= radius ? Color.white : Color.clear);
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, resolution, resolution),
                             new Vector2(0.5f, 0.5f), resolution);
    }
}

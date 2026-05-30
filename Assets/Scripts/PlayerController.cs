using UnityEngine;

/// <summary>
/// Player ship — movement, shooting, health, power-up handling.
/// Attach to the Player prefab (Sprite + Rigidbody2D + BoxCollider2D isTrigger).
/// Tag the GameObject as "Player".
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }

    [Header("Movement")]
    public float moveSpeed = 8f;

    [Header("Boundaries (viewport 0-1)")]
    public float minX = 0.05f;
    public float maxX = 0.95f;
    public float minY = 0.05f;
    public float maxY = 0.95f;

    [Header("Shooting")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float fireRate = 0.25f;
    public float bulletSpeed = 12f;

    [Header("Health")]
    public int maxHealth = 100;

    [Header("Power-up Durations")]
    public float rapidFireDuration = 5f;
    public float shieldDuration = 6f;

    // ── Runtime ──
    public int CurrentHealth { get; private set; }
    public bool HasShield { get; private set; }
    public bool HasRapidFire { get; private set; }

    Rigidbody2D rb;
    float nextFireTime;
    float shieldTimer;
    float rapidFireTimer;
    float rapidFireRate = 0.08f;
    Camera cam;

    // Visual feedback objects (optional — created at runtime if sprites exist)
    GameObject shieldVisual;

    void Awake()
    {
        Instance = this;
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        cam = Camera.main;
        CurrentHealth = maxHealth;
    }

    void OnEnable()
    {
        // Subscribe to game start
        if (GameManager.Instance != null)
            GameManager.Instance.OnGameStarted += ResetPlayer;
    }

    void OnDisable()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnGameStarted -= ResetPlayer;
    }

    void ResetPlayer()
    {
        CurrentHealth = maxHealth;
        HasShield = false;
        HasRapidFire = false;
        transform.position = new Vector3(0f, -3.5f, 0f);
        gameObject.SetActive(true);
    }

    void Update()
    {
        if (GameManager.Instance == null || !GameManager.Instance.IsPlaying || GameManager.Instance.IsGameOver) return;

        HandleMovement();
        HandleShooting();
        HandlePowerUpTimers();
    }

    void HandleMovement()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector2 move = new Vector2(h, v).normalized * moveSpeed;
        rb.linearVelocity = move;

        // Clamp to screen bounds
        Vector3 pos = transform.position;
        Vector3 minWorld = cam.ViewportToWorldPoint(new Vector3(minX, minY, 0));
        Vector3 maxWorld = cam.ViewportToWorldPoint(new Vector3(maxX, maxY, 0));
        pos.x = Mathf.Clamp(pos.x, minWorld.x, maxWorld.x);
        pos.y = Mathf.Clamp(pos.y, minWorld.y, maxWorld.y);
        transform.position = pos;
    }

    void HandleShooting()
    {
        if (Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.Z))
        {
            float rate = HasRapidFire ? rapidFireRate : fireRate;
            if (Time.time >= nextFireTime)
            {
                nextFireTime = Time.time + rate;
                Fire();
            }
        }
    }

    void Fire()
    {
        if (bulletPrefab == null) return;
        Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position + Vector3.up * 0.6f;
        GameObject b = Instantiate(bulletPrefab, spawnPos, Quaternion.identity);
        Bullet bullet = b.GetComponent<Bullet>();
        if (bullet != null)
        {
            bullet.Init(Vector2.up, bulletSpeed, true);
        }
        SoundManager.Instance?.PlaySFX("PlayerShoot");
    }

    void HandlePowerUpTimers()
    {
        if (HasRapidFire)
        {
            rapidFireTimer -= Time.deltaTime;
            if (rapidFireTimer <= 0f) HasRapidFire = false;
        }
        if (HasShield)
        {
            shieldTimer -= Time.deltaTime;
            if (shieldTimer <= 0f)
            {
                HasShield = false;
                if (shieldVisual != null) shieldVisual.SetActive(false);
            }
        }
    }

    // ── Power-up activation ──
    public void ActivateRapidFire()
    {
        HasRapidFire = true;
        rapidFireTimer = rapidFireDuration;
    }

    public void ActivateShield()
    {
        HasShield = true;
        shieldTimer = shieldDuration;
        // Visual: create a circle around player
        if (shieldVisual == null)
        {
            shieldVisual = new GameObject("ShieldVisual");
            shieldVisual.transform.SetParent(transform);
            shieldVisual.transform.localPosition = Vector3.zero;
            var sr = shieldVisual.AddComponent<SpriteRenderer>();
            sr.sprite = CreateCircleSprite();
            sr.color = new Color(0.3f, 0.7f, 1f, 0.35f);
            sr.sortingOrder = 5;
            shieldVisual.transform.localScale = Vector3.one * 2.5f;
        }
        shieldVisual.SetActive(true);
    }

    public void Heal(int amount)
    {
        CurrentHealth = Mathf.Min(CurrentHealth + amount, maxHealth);
    }

    // ── Damage ──
    public void TakeDamage(int amount)
    {
        if (HasShield) return; // shield absorbs all damage

        CurrentHealth -= amount;
        SoundManager.Instance?.PlaySFX("PlayerHit");

        if (CurrentHealth <= 0)
        {
            CurrentHealth = 0;
            Die();
        }
    }

    void Die()
    {
        SoundManager.Instance?.PlaySFX("Explosion");
        GameManager.Instance?.LoseLife();

        if (GameManager.Instance != null && !GameManager.Instance.IsGameOver)
        {
            // Respawn after delay
            CurrentHealth = maxHealth;
            HasShield = false;
            HasRapidFire = false;
            transform.position = new Vector3(0f, -3.5f, 0f);
            // Brief invincibility via shield
            ActivateShield();
            shieldTimer = 2f; // short grace period
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("EnemyBullet"))
        {
            TakeDamage(10);
            Destroy(other.gameObject);
        }
        else if (other.CompareTag("Enemy"))
        {
            TakeDamage(25);
        }
        else if (other.CompareTag("PowerUp"))
        {
            PowerUp pu = other.GetComponent<PowerUp>();
            if (pu != null) pu.Apply(this);
            Destroy(other.gameObject);
        }
    }

    // Utility: procedural circle sprite for shield visual
    Sprite CreateCircleSprite()
    {
        int size = 64;
        Texture2D tex = new Texture2D(size, size);
        tex.filterMode = FilterMode.Bilinear;
        float center = size / 2f;
        float radius = size / 2f;
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                tex.SetPixel(x, y, dist < radius ? Color.white : Color.clear);
            }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }
}

using UnityEngine;

/// <summary>
/// Controls player movement, shooting, and power-up effects.
/// Handles keyboard input for movement and shooting.
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float smoothTime = 0.05f;
    
    [Header("Shooting Settings")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireRate = 0.2f;
    [SerializeField] private float rapidFireRate = 0.08f;
    
    [Header("Boundaries")]
    [SerializeField] private float minX = -8f;
    [SerializeField] private float maxX = 8f;
    [SerializeField] private float minY = -4f;
    [SerializeField] private float maxY = 4f;
    
    [Header("Power-up Durations")]
    [SerializeField] private float shieldDuration = 5f;
    [SerializeField] private float rapidFireDuration = 5f;
    
    // Components
    private HealthSystem healthSystem;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    
    // State
    private Vector2 velocity;
    private Vector2 targetVelocity;
    private float nextFireTime;
    private bool hasShield;
    private bool hasRapidFire;
    private float shieldTimer;
    private float rapidFireTimer;
    private GameObject shieldVisual;
    
    // Singleton for easy access
    public static PlayerController Instance { get; private set; }
    
    // Properties
    public HealthSystem Health => healthSystem;
    public bool HasShield => hasShield;
    public bool HasRapidFire => hasRapidFire;
    
    private void Awake()
    {
        Instance = this;
        healthSystem = GetComponent<HealthSystem>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        
        if (healthSystem != null)
        {
            healthSystem.OnDeath += OnPlayerDeath;
            healthSystem.OnHealthChanged += OnHealthChanged;
        }
        
        CreateShieldVisual();
    }
    
    private void OnDestroy()
    {
        if (healthSystem != null)
        {
            healthSystem.OnDeath -= OnPlayerDeath;
            healthSystem.OnHealthChanged -= OnHealthChanged;
        }
        
        if (Instance == this)
        {
            Instance = null;
        }
    }
    
    private void Update()
    {
        if (GameManager.Instance != null && !GameManager.Instance.IsPlaying) return;
        
        HandleMovement();
        HandleShooting();
        UpdatePowerUps();
    }
    
    private void HandleMovement()
    {
        float horizontal = 0f;
        float vertical = 0f;
        
        // Arrow key input
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
            horizontal = -1f;
        else if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
            horizontal = 1f;
            
        if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
            vertical = 1f;
        else if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
            vertical = -1f;
        
        targetVelocity = new Vector2(horizontal, vertical).normalized * moveSpeed;
        
        // Smooth movement
        velocity = Vector2.Lerp(velocity, targetVelocity, Time.deltaTime / smoothTime);
        
        // Apply movement
        Vector3 newPosition = transform.position + (Vector3)velocity * Time.deltaTime;
        
        // Clamp to boundaries
        newPosition.x = Mathf.Clamp(newPosition.x, minX, maxX);
        newPosition.y = Mathf.Clamp(newPosition.y, minY, maxY);
        
        transform.position = newPosition;
    }
    
    private void HandleShooting()
    {
        if (Input.GetKey(KeyCode.Space) && Time.time >= nextFireTime)
        {
            Fire();
            float currentFireRate = hasRapidFire ? rapidFireRate : fireRate;
            nextFireTime = Time.time + currentFireRate;
        }
    }
    
    private void Fire()
    {
        if (bulletPrefab == null) return;
        
        Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position + Vector3.up * 0.5f;
        
        // Normal shot
        GameObject bullet = Instantiate(bulletPrefab, spawnPos, Quaternion.identity);
        BulletController bulletController = bullet.GetComponent<BulletController>();
        if (bulletController != null)
        {
            bulletController.Initialize(Vector2.up, true);
        }
        
        // Rapid fire adds side shots
        if (hasRapidFire)
        {
            // Left angled shot
            GameObject leftBullet = Instantiate(bulletPrefab, spawnPos, Quaternion.Euler(0, 0, 15));
            BulletController leftBC = leftBullet.GetComponent<BulletController>();
            if (leftBC != null)
            {
                leftBC.Initialize(new Vector2(-0.2f, 1f).normalized, true);
            }
            
            // Right angled shot
            GameObject rightBullet = Instantiate(bulletPrefab, spawnPos, Quaternion.Euler(0, 0, -15));
            BulletController rightBC = rightBullet.GetComponent<BulletController>();
            if (rightBC != null)
            {
                rightBC.Initialize(new Vector2(0.2f, 1f).normalized, true);
            }
        }
        
        AudioManager.Instance?.PlaySound("PlayerShoot");
    }
    
    private void UpdatePowerUps()
    {
        // Update shield
        if (hasShield)
        {
            shieldTimer -= Time.deltaTime;
            if (shieldTimer <= 0)
            {
                DeactivateShield();
            }
        }
        
        // Update rapid fire
        if (hasRapidFire)
        {
            rapidFireTimer -= Time.deltaTime;
            if (rapidFireTimer <= 0)
            {
                hasRapidFire = false;
            }
        }
    }
    
    private void CreateShieldVisual()
    {
        shieldVisual = new GameObject("ShieldVisual");
        shieldVisual.transform.SetParent(transform);
        shieldVisual.transform.localPosition = Vector3.zero;
        
        SpriteRenderer sr = shieldVisual.AddComponent<SpriteRenderer>();
        sr.sprite = CreateCircleSprite();
        sr.color = new Color(0.3f, 0.7f, 1f, 0.4f);
        sr.sortingOrder = 10;
        shieldVisual.transform.localScale = Vector3.one * 2f;
        shieldVisual.SetActive(false);
    }
    
    private Sprite CreateCircleSprite()
    {
        int resolution = 64;
        Texture2D texture = new Texture2D(resolution, resolution);
        Color transparent = new Color(0, 0, 0, 0);
        
        Vector2 center = new Vector2(resolution / 2f, resolution / 2f);
        float radius = resolution / 2f - 2;
        
        for (int x = 0; x < resolution; x++)
        {
            for (int y = 0; y < resolution; y++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center);
                if (dist < radius && dist > radius - 4)
                {
                    texture.SetPixel(x, y, Color.white);
                }
                else
                {
                    texture.SetPixel(x, y, transparent);
                }
            }
        }
        
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, resolution, resolution), Vector2.one * 0.5f, resolution);
    }
    
    // Power-up activation methods
    public void ActivateShield()
    {
        hasShield = true;
        shieldTimer = shieldDuration;
        shieldVisual?.SetActive(true);
        healthSystem?.SetInvincible(shieldDuration);
    }
    
    public void DeactivateShield()
    {
        hasShield = false;
        shieldVisual?.SetActive(false);
    }
    
    public void ActivateRapidFire()
    {
        hasRapidFire = true;
        rapidFireTimer = rapidFireDuration;
    }
    
    public void RestoreHealth(int amount)
    {
        healthSystem?.Heal(amount);
    }
    
    private void OnPlayerDeath()
    {
        GameManager.Instance?.GameOver();
    }
    
    private void OnHealthChanged(int current, int max)
    {
        UIManager.Instance?.UpdateHealth(current, max);
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Handle power-up collection
        PowerUpController powerUp = other.GetComponent<PowerUpController>();
        if (powerUp != null)
        {
            powerUp.Collect(this);
        }
    }
}

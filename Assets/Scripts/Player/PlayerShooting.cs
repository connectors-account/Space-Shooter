using UnityEngine;

/// <summary>
/// Handles player shooting mechanics.
/// Attach this script to the Player GameObject.
/// </summary>
public class PlayerShooting : MonoBehaviour
{
    [Header("Bullet Settings")]
    [Tooltip("Bullet prefab to instantiate")]
    [SerializeField] private GameObject bulletPrefab;
    
    [Tooltip("Speed of the bullet")]
    [SerializeField] private float bulletSpeed = 15f;
    
    [Tooltip("Damage dealt by each bullet")]
    [SerializeField] private int bulletDamage = 10;
    
    [Header("Firing Settings")]
    [Tooltip("Time between shots in seconds")]
    [SerializeField] private float fireRate = 0.2f;
    
    [Tooltip("Offset from player position where bullets spawn")]
    [SerializeField] private Vector3 bulletSpawnOffset = new Vector3(0f, 0.5f, 0f);
    
    [Header("Power-up Settings")]
    [Tooltip("Number of bullets fired at once (for spread shot)")]
    [SerializeField] private int bulletCount = 1;
    
    [Tooltip("Angle spread for multiple bullets")]
    [SerializeField] private float spreadAngle = 15f;
    
    // Timing
    private float nextFireTime = 0f;
    
    // Audio
    private AudioSource audioSource;
    [SerializeField] private AudioClip shootSound;
    
    /// <summary>
    /// Initialize components.
    /// </summary>
    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }
    
    /// <summary>
    /// Check for fire input every frame.
    /// </summary>
    private void Update()
    {
        // Don't shoot if game is not active
        if (GameManager.Instance != null && !GameManager.Instance.IsGameActive)
        {
            return;
        }
        
        // Check for fire input (Space key or left mouse button)
        if (Input.GetKey(KeyCode.Space) || Input.GetMouseButton(0))
        {
            TryFire();
        }
    }
    
    /// <summary>
    /// Attempt to fire a bullet if enough time has passed.
    /// </summary>
    private void TryFire()
    {
        if (Time.time >= nextFireTime)
        {
            Fire();
            nextFireTime = Time.time + fireRate;
        }
    }
    
    /// <summary>
    /// Fire bullet(s) from the player's position.
    /// </summary>
    private void Fire()
    {
        // Create a bullet even if prefab is not assigned (for testing)
        if (bulletPrefab == null)
        {
            CreateDefaultBullet(Vector2.up);
        }
        else
        {
            // Single bullet
            if (bulletCount == 1)
            {
                SpawnBullet(Vector2.up);
            }
            // Multiple bullets (spread shot)
            else
            {
                float startAngle = -spreadAngle * (bulletCount - 1) / 2f;
                
                for (int i = 0; i < bulletCount; i++)
                {
                    float angle = startAngle + (spreadAngle * i);
                    Vector2 direction = Quaternion.Euler(0f, 0f, angle) * Vector2.up;
                    SpawnBullet(direction);
                }
            }
        }
        
        // Play shoot sound
        if (shootSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(shootSound);
        }
    }
    
    /// <summary>
    /// Spawn a bullet with the assigned prefab.
    /// </summary>
    /// <param name="direction">Direction the bullet should travel</param>
    private void SpawnBullet(Vector2 direction)
    {
        Vector3 spawnPosition = transform.position + bulletSpawnOffset;
        
        // Calculate rotation based on direction
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        Quaternion rotation = Quaternion.Euler(0f, 0f, angle);
        
        GameObject bulletObj = Instantiate(bulletPrefab, spawnPosition, rotation);
        
        // Configure the bullet
        Bullet bullet = bulletObj.GetComponent<Bullet>();
        if (bullet != null)
        {
            bullet.Initialize(direction, bulletSpeed, bulletDamage, true);
        }
        else
        {
            // If no Bullet component, add one
            bullet = bulletObj.AddComponent<Bullet>();
            bullet.Initialize(direction, bulletSpeed, bulletDamage, true);
        }
    }
    
    /// <summary>
    /// Create a simple bullet without a prefab (for testing).
    /// </summary>
    /// <param name="direction">Direction the bullet should travel</param>
    private void CreateDefaultBullet(Vector2 direction)
    {
        Vector3 spawnPosition = transform.position + bulletSpawnOffset;
        
        // Create a simple bullet object
        GameObject bulletObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        bulletObj.name = "PlayerBullet";
        bulletObj.transform.position = spawnPosition;
        bulletObj.transform.localScale = new Vector3(0.2f, 0.4f, 0.2f);
        bulletObj.tag = "PlayerBullet";
        
        // Remove 3D collider and add 2D components
        Destroy(bulletObj.GetComponent<BoxCollider>());
        
        // Add 2D collider
        BoxCollider2D collider = bulletObj.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        collider.size = new Vector2(0.2f, 0.4f);
        
        // Add Rigidbody2D
        Rigidbody2D rb = bulletObj.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        
        // Add and initialize Bullet component
        Bullet bullet = bulletObj.AddComponent<Bullet>();
        bullet.Initialize(direction, bulletSpeed, bulletDamage, true);
        
        // Set bullet color
        MeshRenderer renderer = bulletObj.GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            renderer.material.color = Color.cyan;
        }
    }
    
    /// <summary>
    /// Set the fire rate (useful for power-ups).
    /// </summary>
    /// <param name="newFireRate">New fire rate in seconds between shots</param>
    public void SetFireRate(float newFireRate)
    {
        fireRate = Mathf.Max(0.05f, newFireRate);
    }
    
    /// <summary>
    /// Set the number of bullets fired at once.
    /// </summary>
    /// <param name="count">Number of bullets</param>
    public void SetBulletCount(int count)
    {
        bulletCount = Mathf.Max(1, count);
    }
    
    /// <summary>
    /// Set bullet damage.
    /// </summary>
    /// <param name="damage">New damage value</param>
    public void SetBulletDamage(int damage)
    {
        bulletDamage = Mathf.Max(1, damage);
    }
    
    /// <summary>
    /// Assign a bullet prefab at runtime.
    /// </summary>
    /// <param name="prefab">The bullet prefab to use</param>
    public void SetBulletPrefab(GameObject prefab)
    {
        bulletPrefab = prefab;
    }
}

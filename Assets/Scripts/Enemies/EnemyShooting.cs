using UnityEngine;

/// <summary>
/// Handles enemy shooting mechanics.
/// Attach this script to enemy GameObjects that should shoot.
/// </summary>
public class EnemyShooting : MonoBehaviour
{
    [Header("Bullet Settings")]
    [Tooltip("Bullet prefab to instantiate")]
    [SerializeField] private GameObject bulletPrefab;
    
    [Tooltip("Speed of the bullet")]
    [SerializeField] private float bulletSpeed = 8f;
    
    [Tooltip("Damage dealt by each bullet")]
    [SerializeField] private int bulletDamage = 10;
    
    [Header("Firing Settings")]
    [Tooltip("Time between shots in seconds")]
    [SerializeField] private float fireRate = 1.5f;
    
    [Tooltip("Random variance in fire rate")]
    [SerializeField] private float fireRateVariance = 0.5f;
    
    [Tooltip("Offset from enemy position where bullets spawn")]
    [SerializeField] private Vector3 bulletSpawnOffset = new Vector3(0f, -0.5f, 0f);
    
    [Header("Targeting")]
    [Tooltip("Whether to aim at the player")]
    [SerializeField] private bool aimAtPlayer = false;
    
    // Timing
    private float nextFireTime = 0f;
    
    // Cached references
    private Transform playerTransform;
    
    /// <summary>
    /// Find the player and set initial fire time.
    /// </summary>
    private void Start()
    {
        // Add initial random delay
        nextFireTime = Time.time + Random.Range(0.5f, fireRate);
        
        // Find player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
    }
    
    /// <summary>
    /// Check for fire timing every frame.
    /// </summary>
    private void Update()
    {
        // Don't shoot if game is not active
        if (GameManager.Instance != null && !GameManager.Instance.IsGameActive)
        {
            return;
        }
        
        if (Time.time >= nextFireTime)
        {
            Fire();
            // Add variance to fire rate
            float variance = Random.Range(-fireRateVariance, fireRateVariance);
            nextFireTime = Time.time + fireRate + variance;
        }
    }
    
    /// <summary>
    /// Fire a bullet.
    /// </summary>
    private void Fire()
    {
        Vector2 direction;
        
        if (aimAtPlayer && playerTransform != null)
        {
            // Calculate direction to player
            direction = (playerTransform.position - transform.position).normalized;
        }
        else
        {
            // Fire straight down
            direction = Vector2.down;
        }
        
        if (bulletPrefab != null)
        {
            SpawnBullet(direction);
        }
        else
        {
            CreateDefaultBullet(direction);
        }
    }
    
    /// <summary>
    /// Spawn a bullet using the assigned prefab.
    /// </summary>
    /// <param name="direction">Direction to fire</param>
    private void SpawnBullet(Vector2 direction)
    {
        Vector3 spawnPosition = transform.position + bulletSpawnOffset;
        
        // Calculate rotation
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        Quaternion rotation = Quaternion.Euler(0f, 0f, angle);
        
        GameObject bulletObj = Instantiate(bulletPrefab, spawnPosition, rotation);
        
        // Configure bullet
        Bullet bullet = bulletObj.GetComponent<Bullet>();
        if (bullet != null)
        {
            bullet.Initialize(direction, bulletSpeed, bulletDamage, false);
        }
        else
        {
            bullet = bulletObj.AddComponent<Bullet>();
            bullet.Initialize(direction, bulletSpeed, bulletDamage, false);
        }
    }
    
    /// <summary>
    /// Create a default bullet without a prefab.
    /// </summary>
    /// <param name="direction">Direction to fire</param>
    private void CreateDefaultBullet(Vector2 direction)
    {
        Vector3 spawnPosition = transform.position + bulletSpawnOffset;
        
        // Create bullet from primitive
        GameObject bulletObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        bulletObj.name = "EnemyBullet";
        bulletObj.transform.position = spawnPosition;
        bulletObj.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);
        bulletObj.tag = "EnemyBullet";
        
        // Remove 3D collider
        Destroy(bulletObj.GetComponent<SphereCollider>());
        
        // Add 2D collider
        CircleCollider2D collider = bulletObj.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        collider.radius = 0.125f;
        
        // Add Rigidbody2D
        Rigidbody2D rb = bulletObj.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        
        // Add and initialize Bullet component
        Bullet bullet = bulletObj.AddComponent<Bullet>();
        bullet.Initialize(direction, bulletSpeed, bulletDamage, false);
        
        // Set bullet color
        MeshRenderer renderer = bulletObj.GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            renderer.material.color = Color.yellow;
        }
    }
    
    /// <summary>
    /// Set the fire rate.
    /// </summary>
    /// <param name="rate">New fire rate in seconds</param>
    public void SetFireRate(float rate)
    {
        fireRate = Mathf.Max(0.2f, rate);
    }
    
    /// <summary>
    /// Set whether to aim at the player.
    /// </summary>
    /// <param name="aim">Whether to aim at player</param>
    public void SetAimAtPlayer(bool aim)
    {
        aimAtPlayer = aim;
    }
    
    /// <summary>
    /// Set bullet damage.
    /// </summary>
    /// <param name="damage">New damage value</param>
    public void SetBulletDamage(int damage)
    {
        bulletDamage = Mathf.Max(1, damage);
    }
}

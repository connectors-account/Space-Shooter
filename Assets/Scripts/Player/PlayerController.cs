using UnityEngine;

/// <summary>
/// PlayerController handles player movement, shooting, and input processing.
/// Attach this script to the Player GameObject.
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Player movement speed in units per second")]
    public float moveSpeed = 8f;
    
    [Tooltip("Boundary limits for player movement")]
    public float minX = -8f;
    public float maxX = 8f;
    public float minY = -4f;
    public float maxY = 4f;

    [Header("Shooting Settings")]
    [Tooltip("Reference to the bullet prefab")]
    public GameObject bulletPrefab;
    
    [Tooltip("Point where bullets spawn")]
    public Transform firePoint;
    
    [Tooltip("Time between shots in seconds")]
    public float fireRate = 0.2f;
    
    [Tooltip("Bullet speed")]
    public float bulletSpeed = 15f;

    [Header("Audio")]
    [Tooltip("Sound played when shooting")]
    public AudioClip shootSound;
    
    private float nextFireTime = 0f;
    private AudioSource audioSource;
    private bool canControl = true;

    void Start()
    {
        // Get or add AudioSource component
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Create fire point if not assigned
        if (firePoint == null)
        {
            GameObject fp = new GameObject("FirePoint");
            fp.transform.SetParent(transform);
            fp.transform.localPosition = new Vector3(0, 0.6f, 0);
            firePoint = fp.transform;
        }
    }

    void Update()
    {
        // Don't process input if game is paused or player can't control
        if (!canControl || GameManager.Instance == null || GameManager.Instance.IsGamePaused())
            return;

        HandleMovement();
        HandleShooting();
    }

    /// <summary>
    /// Handles player movement based on input
    /// </summary>
    void HandleMovement()
    {
        // Get input from arrow keys or WASD
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");

        // Calculate movement vector
        Vector3 movement = new Vector3(horizontalInput, verticalInput, 0f).normalized;
        
        // Apply movement
        transform.position += movement * moveSpeed * Time.deltaTime;

        // Clamp position within boundaries
        float clampedX = Mathf.Clamp(transform.position.x, minX, maxX);
        float clampedY = Mathf.Clamp(transform.position.y, minY, maxY);
        transform.position = new Vector3(clampedX, clampedY, 0f);
    }

    /// <summary>
    /// Handles shooting when spacebar is pressed
    /// </summary>
    void HandleShooting()
    {
        // Check if spacebar is pressed and enough time has passed since last shot
        if (Input.GetKey(KeyCode.Space) && Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + fireRate;
        }
    }

    /// <summary>
    /// Spawns a bullet and fires it upward
    /// </summary>
    void Shoot()
    {
        if (bulletPrefab != null)
        {
            // Instantiate bullet at fire point
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
            
            // Set bullet properties
            Bullet bulletScript = bullet.GetComponent<Bullet>();
            if (bulletScript != null)
            {
                bulletScript.SetDirection(Vector2.up);
                bulletScript.SetSpeed(bulletSpeed);
                bulletScript.SetIsPlayerBullet(true);
            }
            
            // Play shoot sound
            if (shootSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(shootSound);
            }
        }
    }

    /// <summary>
    /// Enable or disable player control
    /// </summary>
    public void SetCanControl(bool value)
    {
        canControl = value;
    }

    /// <summary>
    /// Reset player to starting position
    /// </summary>
    public void ResetPlayer()
    {
        transform.position = new Vector3(0, -3f, 0);
        canControl = true;
    }
}

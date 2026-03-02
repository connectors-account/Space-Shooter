using UnityEngine;

/// <summary>
/// Controls player movement and shooting mechanics
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float boundaryX = 8f;
    [SerializeField] private float boundaryY = 4f;

    [Header("Shooting Settings")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireRate = 0.25f;

    [Header("Audio (Optional)")]
    [SerializeField] private AudioClip shootSound;
    [SerializeField] private AudioClip hitSound;

    private float nextFireTime = 0f;
    private AudioSource audioSource;
    private GameManager gameManager;

    private void Start()
    {
        // Get or add AudioSource component
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Find GameManager in scene
        gameManager = FindObjectOfType<GameManager>();

        // If no fire point is set, create one at player position
        if (firePoint == null)
        {
            GameObject fp = new GameObject("FirePoint");
            fp.transform.SetParent(transform);
            fp.transform.localPosition = new Vector3(0, 0.5f, 0);
            firePoint = fp.transform;
        }
    }

    private void Update()
    {
        // Don't process input if game is over
        if (gameManager != null && gameManager.IsGameOver())
            return;

        HandleMovement();
        HandleShooting();
    }

    private void HandleMovement()
    {
        // Get input from WASD or Arrow keys
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");

        // Calculate movement vector
        Vector3 movement = new Vector3(horizontalInput, verticalInput, 0f);
        movement = movement.normalized * moveSpeed * Time.deltaTime;

        // Apply movement
        transform.position += movement;

        // Clamp position within boundaries
        float clampedX = Mathf.Clamp(transform.position.x, -boundaryX, boundaryX);
        float clampedY = Mathf.Clamp(transform.position.y, -boundaryY, boundaryY);
        transform.position = new Vector3(clampedX, clampedY, 0f);
    }

    private void HandleShooting()
    {
        // Check for shoot input (Space or Left Mouse Button)
        if ((Input.GetKey(KeyCode.Space) || Input.GetMouseButton(0)) && Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + fireRate;
        }
    }

    private void Shoot()
    {
        if (bulletPrefab != null)
        {
            // Instantiate bullet at fire point
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
            
            // Play shoot sound if available
            if (shootSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(shootSound);
            }
        }
        else
        {
            Debug.LogWarning("PlayerController: Bullet prefab not assigned!");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if hit by enemy or enemy bullet
        if (other.CompareTag("Enemy") || other.CompareTag("EnemyBullet"))
        {
            TakeDamage();
            
            // Destroy enemy bullet if that's what hit us
            if (other.CompareTag("EnemyBullet"))
            {
                Destroy(other.gameObject);
            }
        }
    }

    private void TakeDamage()
    {
        // Play hit sound
        if (hitSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(hitSound);
        }

        // Notify GameManager of damage
        if (gameManager != null)
        {
            gameManager.PlayerTakeDamage(1);
        }

        // Brief invincibility flash effect (visual feedback)
        StartCoroutine(FlashEffect());
    }

    private System.Collections.IEnumerator FlashEffect()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            Color originalColor = sr.color;
            sr.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            sr.color = originalColor;
        }
    }

    // Public method to set movement boundaries (can be called by GameManager)
    public void SetBoundaries(float x, float y)
    {
        boundaryX = x;
        boundaryY = y;
    }
}

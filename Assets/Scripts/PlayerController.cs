using UnityEngine;

/// <summary>
/// PlayerController handles spaceship movement (WASD/Arrow keys) and shooting (Spacebar).
/// Attach this script to the Player GameObject.
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("How fast the player ship moves")]
    public float moveSpeed = 8f;

    [Header("Shooting Settings")]
    [Tooltip("Prefab for the bullet the player fires")]
    public GameObject bulletPrefab;

    [Tooltip("Point where bullets spawn (assign an empty child object)")]
    public Transform firePoint;

    [Tooltip("Minimum seconds between shots")]
    public float fireRate = 0.25f;

    [Header("Boundary Settings")]
    [Tooltip("How far left/right the player can move")]
    public float horizontalBound = 8f;

    [Tooltip("How far up/down the player can move")]
    public float verticalBound = 4.5f;

    // Internal timer that tracks when the player can fire again
    private float nextFireTime = 0f;

    void Update()
    {
        // Skip input when the game is over
        if (GameManager.Instance != null && GameManager.Instance.isGameOver)
            return;

        HandleMovement();
        HandleShooting();
    }

    /// <summary>
    /// Reads WASD / Arrow-key input and moves the player, clamped to screen bounds.
    /// </summary>
    private void HandleMovement()
    {
        float h = Input.GetAxis("Horizontal"); // A/D or Left/Right
        float v = Input.GetAxis("Vertical");   // W/S or Up/Down

        Vector3 movement = new Vector3(h, v, 0f) * moveSpeed * Time.deltaTime;
        transform.Translate(movement, Space.World);

        // Clamp position so the ship stays on screen
        float clampedX = Mathf.Clamp(transform.position.x, -horizontalBound, horizontalBound);
        float clampedY = Mathf.Clamp(transform.position.y, -verticalBound, verticalBound);
        transform.position = new Vector3(clampedX, clampedY, 0f);
    }

    /// <summary>
    /// Fires a bullet upward when the player presses Space (respecting fire rate).
    /// </summary>
    private void HandleShooting()
    {
        if (Input.GetKey(KeyCode.Space) && Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + fireRate;

            if (bulletPrefab != null && firePoint != null)
            {
                Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
            }
        }
    }

    /// <summary>
    /// When an enemy collides with the player, the player takes damage.
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            // Destroy the enemy that hit us
            Destroy(other.gameObject);

            // Tell the GameManager the player was hit
            if (GameManager.Instance != null)
            {
                GameManager.Instance.PlayerTakeDamage(1);
            }
        }
    }
}

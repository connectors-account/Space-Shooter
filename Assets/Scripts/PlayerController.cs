using UnityEngine;

/// <summary>
/// Controls player movement (WASD / Arrow keys) and shooting.
/// Attach to the Player GameObject (a stretched cube representing a ship).
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [Tooltip("Player movement speed in units per second.")]
    public float moveSpeed = 8f;

    [Header("Shooting")]
    [Tooltip("Prefab for the bullet.")]
    public GameObject bulletPrefab;

    [Tooltip("Transform marking the spawn point for bullets (front of the ship).")]
    public Transform firePoint;

    [Tooltip("Minimum seconds between shots.")]
    public float fireRate = 0.2f;

    [Header("Boundaries")]
    [Tooltip("How far left/right the player can move.")]
    public float xBound = 8f;

    [Tooltip("How far up/down the player can move.")]
    public float yBound = 4.5f;

    // Internal timer for fire-rate limiting
    private float nextFireTime = 0f;

    void Update()
    {
        // --- Movement ---
        float horizontal = Input.GetAxis("Horizontal");
        float vertical   = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(horizontal, vertical, 0f) * moveSpeed * Time.deltaTime;
        transform.Translate(movement, Space.World);

        // Clamp position so the player stays on screen
        float clampedX = Mathf.Clamp(transform.position.x, -xBound, xBound);
        float clampedY = Mathf.Clamp(transform.position.y, -yBound, yBound);
        transform.position = new Vector3(clampedX, clampedY, 0f);

        // --- Shooting ---
        if (Input.GetButton("Fire1") && Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + fireRate;
        }
    }

    /// <summary>
    /// Instantiate a bullet at the fire point, flying upward.
    /// </summary>
    void Shoot()
    {
        if (bulletPrefab == null || firePoint == null) return;

        Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
    }
}

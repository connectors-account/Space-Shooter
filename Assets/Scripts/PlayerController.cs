using UnityEngine;

/// <summary>
/// Handles player movement and shooting.
/// Uses legacy Input Manager axes/buttons.
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private Vector2 minBounds = new Vector2(-8f, -4.5f);
    [SerializeField] private Vector2 maxBounds = new Vector2(8f, 4.5f);

    [Header("Shooting")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireRate = 0.2f;

    private float nextFireTime;

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
        {
            return;
        }

        HandleMovement();
        HandleShooting();
    }

    private void HandleMovement()
    {
        // Legacy Input Manager axes. Supports WASD + arrow keys by default.
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 direction = new Vector3(horizontal, vertical, 0f).normalized;
        transform.position += direction * moveSpeed * Time.deltaTime;

        // Keep player inside camera/gameplay bounds.
        Vector3 clamped = transform.position;
        clamped.x = Mathf.Clamp(clamped.x, minBounds.x, maxBounds.x);
        clamped.y = Mathf.Clamp(clamped.y, minBounds.y, maxBounds.y);
        transform.position = clamped;
    }

    private void HandleShooting()
    {
        bool firePressed = Input.GetButton("Fire1") || Input.GetKey(KeyCode.Space);

        if (!firePressed)
        {
            return;
        }

        if (Time.time < nextFireTime)
        {
            return;
        }

        nextFireTime = Time.time + fireRate;

        if (bulletPrefab == null || firePoint == null)
        {
            Debug.LogWarning("PlayerController: bulletPrefab or firePoint is not assigned.");
            return;
        }

        Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
    }

    /// <summary>
    /// Optional fire-rate boost hook for future power-ups.
    /// </summary>
    public void SetFireRate(float newFireRate)
    {
        fireRate = Mathf.Clamp(newFireRate, 0.05f, 2f);
    }
}

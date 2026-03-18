// ============================================================================
// BulletController.cs - Controls bullet movement, lifetime, and damage
// Used for both player and enemy bullets.
// ============================================================================
using UnityEngine;

/// <summary>
/// Generic bullet component. After Initialize() is called, the bullet
/// moves in the given direction at the given speed until it leaves the
/// screen or its lifetime expires.
/// </summary>
public class BulletController : MonoBehaviour
{
    // ---- Configuration (set via Initialize or Inspector) ----
    [Header("Bullet Settings")]
    public float speed = 12f;
    public int damage = 10;
    public float lifetime = 5f;

    // ---- Internal ----
    private Vector2 _direction = Vector2.up;
    private bool _initialized = false;
    private float _spawnTime;

    // ========================================================================
    // Public API
    // ========================================================================

    /// <summary>
    /// Initialize bullet direction, speed, and damage.
    /// Called by the shooter (PlayerController or EnemyController).
    /// </summary>
    public void Initialize(Vector2 direction, float bulletSpeed, int bulletDamage)
    {
        _direction = direction.normalized;
        speed = bulletSpeed;
        damage = bulletDamage;
        _initialized = true;
        _spawnTime = Time.time;

        // Rotate sprite to face movement direction
        float angle = Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg - 90f;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    // ========================================================================
    // Unity Lifecycle
    // ========================================================================
    private void Start()
    {
        if (!_initialized)
        {
            _spawnTime = Time.time;
            _initialized = true;
        }
    }

    private void Update()
    {
        // Move the bullet
        transform.Translate(_direction * speed * Time.deltaTime, Space.World);

        // Destroy if lifetime exceeded
        if (Time.time - _spawnTime > lifetime)
        {
            Destroy(gameObject);
            return;
        }

        // Destroy if off-screen (with generous margin)
        if (IsOffScreen())
        {
            Destroy(gameObject);
        }
    }

    // ========================================================================
    // Helpers
    // ========================================================================
    private bool IsOffScreen()
    {
        Camera cam = Camera.main;
        if (cam == null) return false;

        Vector3 viewportPos = cam.WorldToViewportPoint(transform.position);
        float margin = 0.1f; // 10% margin beyond screen edges
        return viewportPos.x < -margin || viewportPos.x > 1f + margin ||
               viewportPos.y < -margin || viewportPos.y > 1f + margin;
    }
}

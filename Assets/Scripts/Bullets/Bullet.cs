using UnityEngine;

/// <summary>
/// Generic bullet behavior. Used for both player and enemy bullets.
/// Configure direction, speed, and damage in the Inspector or via code.
/// </summary>
public class Bullet : MonoBehaviour
{
    [Header("Bullet Settings")]
    [SerializeField] private float speed = 12f;
    [SerializeField] private int damage = 1;
    [SerializeField] private Vector2 direction = Vector2.up;
    [SerializeField] private float lifetime = 5f;

    private Rigidbody2D rb;

    public int Damage => damage;
    public float Speed => speed;

    /// <summary>
    /// Sets the bullet's direction (normalized).
    /// </summary>
    public void SetDirection(Vector2 dir)
    {
        direction = dir.normalized;
        ApplyVelocity();
        RotateToDirection();
    }

    /// <summary>
    /// Sets bullet speed.
    /// </summary>
    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed;
        ApplyVelocity();
    }

    /// <summary>
    /// Sets bullet damage.
    /// </summary>
    public void SetDamage(int newDamage)
    {
        damage = newDamage;
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
            rb = gameObject.AddComponent<Rigidbody2D>();

        rb.gravityScale = 0;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    private void Start()
    {
        ApplyVelocity();
        RotateToDirection();
        Destroy(gameObject, lifetime);
    }

    /// <summary>
    /// Applies velocity based on direction and speed.
    /// </summary>
    private void ApplyVelocity()
    {
        if (rb != null)
            rb.linearVelocity = direction.normalized * speed;
    }

    /// <summary>
    /// Rotates the bullet sprite to face the movement direction.
    /// </summary>
    private void RotateToDirection()
    {
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }
}

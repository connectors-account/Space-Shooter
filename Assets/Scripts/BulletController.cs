using UnityEngine;

/// <summary>
/// Controls a single bullet: moves it in a fixed direction, destroys it when
/// it leaves the screen or after a lifetime, and handles collisions. A bullet
/// fired by the player damages enemies; (optionally) enemy bullets could damage
/// the player using the same component.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class BulletController : MonoBehaviour
{
    public enum BulletOwner { Player, Enemy }

    [Header("Bullet Settings")]
    [Tooltip("Travel speed in world units per second.")]
    [SerializeField] private float speed = 14f;

    [Tooltip("Seconds before the bullet auto-destroys (safety net).")]
    [SerializeField] private float lifetime = 4f;

    [Tooltip("Damage dealt to enemies (player bullets) or player (enemy bullets).")]
    [SerializeField] private int damage = 25;

    private Vector2 direction = Vector2.up;
    private BulletOwner owner = BulletOwner.Player;
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;

        // Ensure the collider is a trigger for clean overlap detection.
        Collider2D col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    private void Start()
    {
        // Auto-destroy after its lifetime so nothing lingers off-screen.
        Destroy(gameObject, lifetime);
    }

    /// <summary>Configure the bullet's direction and owner right after spawning.</summary>
    public void Initialize(Vector2 travelDirection, BulletOwner bulletOwner)
    {
        direction = travelDirection.normalized;
        owner = bulletOwner;
    }

    private void Update()
    {
        // Move the bullet each frame.
        transform.Translate(direction * speed * Time.deltaTime, Space.World);

        // Destroy if it leaves the visible screen.
        if (IsOffScreen())
        {
            Destroy(gameObject);
        }
    }

    private bool IsOffScreen()
    {
        Camera cam = Camera.main;
        if (cam == null) return false;

        Vector3 viewPos = cam.WorldToViewportPoint(transform.position);
        return viewPos.x < -0.1f || viewPos.x > 1.1f || viewPos.y < -0.1f || viewPos.y > 1.1f;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (owner == BulletOwner.Player)
        {
            // Player bullet hits an enemy.
            if (other.CompareTag("Enemy"))
            {
                EnemyController enemy = other.GetComponent<EnemyController>();
                if (enemy != null)
                {
                    enemy.TakeDamage(damage);
                }
                Destroy(gameObject);
            }
        }
        else // Enemy bullet
        {
            if (other.CompareTag("Player"))
            {
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.DamagePlayer(damage);
                }
                Destroy(gameObject);
            }
        }
    }
}

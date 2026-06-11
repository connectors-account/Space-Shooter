using UnityEngine;

/// <summary>
/// Controls a single projectile. Moves in a straight line, applies damage on
/// contact, and self-destructs after a lifetime or when leaving the screen.
/// The same prefab is used for player and enemy bullets; the "owner" tag tells
/// it which targets it may hurt.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class BulletController : MonoBehaviour
{
    public enum Owner
    {
        Player,
        Enemy
    }

    [Header("Bullet Settings")]
    [Tooltip("Movement speed in units per second.")]
    public float speed = 12f;

    [Tooltip("Damage dealt to whatever it hits.")]
    public int damage = 25;

    [Tooltip("Seconds before the bullet auto-destroys if it hits nothing.")]
    public float lifetime = 3f;

    [Tooltip("Who fired this bullet. Determines valid targets.")]
    public Owner owner = Owner.Player;

    // Direction is set by the shooter. Default up for player shots.
    private Vector2 direction = Vector2.up;
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;

        // Ensure the collider is a trigger so we use OnTriggerEnter2D.
        Collider2D col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    private void Start()
    {
        // Auto-destroy after its lifetime regardless of collisions.
        Destroy(gameObject, lifetime);
    }

    /// <summary>
    /// Called by the shooter right after instantiation to configure the bullet.
    /// </summary>
    public void Initialize(Vector2 fireDirection, Owner bulletOwner, int bulletDamage, float bulletSpeed)
    {
        direction = fireDirection.normalized;
        owner = bulletOwner;
        damage = bulletDamage;
        speed = bulletSpeed;
    }

    private void Update()
    {
        // Move the bullet each frame.
        transform.Translate(direction * speed * Time.deltaTime, Space.World);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Player bullets damage enemies; enemy bullets damage the player.
        if (owner == Owner.Player && other.CompareTag("Enemy"))
        {
            ApplyDamage(other);
            Destroy(gameObject);
        }
        else if (owner == Owner.Enemy && other.CompareTag("Player"))
        {
            ApplyDamage(other);
            Destroy(gameObject);
        }
        else if (other.CompareTag("Boundary"))
        {
            // Despawn when crossing the screen boundary trigger.
            Destroy(gameObject);
        }
    }

    private void ApplyDamage(Collider2D target)
    {
        Health health = target.GetComponent<Health>();
        if (health != null)
        {
            health.TakeDamage(damage);
        }
    }
}

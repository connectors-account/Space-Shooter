using UnityEngine;

/// <summary>
/// Controls a single bullet.
///   - Moves in a straight line (up for player bullets, down for enemy bullets).
///   - Deals damage to the appropriate target on contact.
///   - Self-destructs after a lifetime to avoid leaking objects.
/// Requires a Rigidbody2D (kinematic) and a trigger Collider2D.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class Bullet : MonoBehaviour
{
    /// <summary>Identifies who fired the bullet, controlling what it can hit.</summary>
    public enum Owner { Player, Enemy }

    [Header("Bullet Settings")]
    [Tooltip("Travel speed in units per second.")]
    public float speed = 12f;

    [Tooltip("Damage dealt to whatever this bullet hits.")]
    public int damage = 25;

    [Tooltip("Seconds before the bullet auto-destroys if it hits nothing.")]
    public float lifetime = 3f;

    // Who owns this bullet. Defaults to Player; set via SetOwner().
    private Owner owner = Owner.Player;
    private Rigidbody2D rb;

    /// <summary>
    /// Awake caches the Rigidbody2D and ensures gravity is off.
    /// </summary>
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
    }

    /// <summary>
    /// Start sets the bullet's velocity and schedules its destruction.
    /// </summary>
    private void Start()
    {
        // Player bullets fly up (+Y); enemy bullets fly down (-Y).
        float direction = (owner == Owner.Player) ? 1f : -1f;
        rb.velocity = new Vector2(0f, direction * speed);

        // Automatically clean up after the lifetime expires.
        Destroy(gameObject, lifetime);
    }

    /// <summary>
    /// Sets the owner of the bullet and updates its tag accordingly.
    /// Must be called immediately after instantiation.
    /// </summary>
    /// <param name="newOwner">Player or Enemy.</param>
    public void SetOwner(Owner newOwner)
    {
        owner = newOwner;
        // Tagging lets other objects recognize the bullet type in collisions.
        gameObject.tag = (owner == Owner.Player) ? "PlayerBullet" : "EnemyBullet";
    }

    /// <summary>
    /// Handles collisions. Player bullets damage enemies;
    /// enemy bullets are handled by the PlayerController instead, so here we
    /// only need to deal with player bullets hitting enemies.
    /// </summary>
    /// <param name="other">The collider we hit.</param>
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (owner == Owner.Player)
        {
            // Player bullet hits an enemy.
            if (other.CompareTag("Enemy"))
            {
                Enemy enemy = other.GetComponent<Enemy>();
                if (enemy != null)
                    enemy.TakeDamage(damage);

                // The bullet is consumed on impact.
                Destroy(gameObject);
            }
        }
        // Enemy bullets hitting the player are processed in PlayerController.
    }
}

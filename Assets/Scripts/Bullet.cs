using UnityEngine;

/// <summary>
/// Generic bullet behavior. Used by both player and enemy bullets.
/// Attach this to the Bullet prefab.
/// </summary>
public class Bullet : MonoBehaviour
{
    [Header("Bullet Settings")]
    public int damage = 1;
    public bool isPlayerBullet = true;
    public float lifetime = 5f;

    private Vector2 direction;
    private float speed;

    void Start()
    {
        // Auto-destroy after lifetime to prevent orphaned bullets
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        // Move in the assigned direction
        transform.position += (Vector3)(direction * speed * Time.deltaTime);
    }

    /// <summary>
    /// Sets the bullet's travel direction and speed. Called right after instantiation.
    /// </summary>
    public void SetDirection(Vector2 dir, float spd)
    {
        direction = dir.normalized;
        speed = spd;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (isPlayerBullet)
        {
            // Player bullet hits an enemy
            if (other.CompareTag("Enemy"))
            {
                Enemy enemy = other.GetComponent<Enemy>();
                if (enemy != null)
                {
                    enemy.TakeDamage(damage);
                }
                Destroy(gameObject);
            }
        }
        else
        {
            // Enemy bullet hits the player
            if (other.CompareTag("Player"))
            {
                PlayerController player = other.GetComponent<PlayerController>();
                if (player != null)
                {
                    player.TakeDamage(damage);
                }
                Destroy(gameObject);
            }
        }
    }
}

using UnityEngine;

/// <summary>
/// Projectile fired by the player or an enemy. Moves in a fixed direction,
/// deals damage to the opposing faction and self-destructs off-screen or on hit.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class Bullet : MonoBehaviour
{
    /// <summary>Who fired the bullet, used to decide what it can hit.</summary>
    public enum Owner { Player, Enemy }

    [Tooltip("Damage dealt on impact.")]
    public int damage = 25;
    [Tooltip("Seconds before the bullet auto-destroys as a safety net.")]
    public float lifeTime = 5f;

    private Vector2 direction = Vector2.up;
    private float speed = 12f;
    private Owner owner = Owner.Player;

    /// <summary>Configure the bullet immediately after instantiation.</summary>
    public void Initialize(Vector2 dir, float spd, Owner ownerType)
    {
        direction = dir.normalized;
        speed = spd;
        owner = ownerType;

        // Destroy after lifeTime so stray bullets never accumulate.
        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        transform.position += (Vector3)(direction * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (owner == Owner.Player)
        {
            // Player bullets only damage enemies.
            if (other.CompareTag("Enemy"))
            {
                EnemyController enemy = other.GetComponent<EnemyController>();
                if (enemy != null) enemy.TakeDamage(damage);
                Destroy(gameObject);
            }
        }
        else // Enemy bullet
        {
            // Enemy bullets only damage the player.
            if (other.CompareTag("Player"))
            {
                PlayerController player = other.GetComponent<PlayerController>();
                if (player != null) player.TakeDamage(damage);
                Destroy(gameObject);
            }
        }
    }
}

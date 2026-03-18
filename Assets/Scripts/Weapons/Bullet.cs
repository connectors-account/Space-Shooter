using UnityEngine;

/// <summary>
/// Projectile that moves in a direction and deals damage on collision.
/// Used by both player and enemy bullets.
/// </summary>
public class Bullet : MonoBehaviour
{
    [SerializeField] private float speed = 12f;
    [SerializeField] private int damage = 10;
    [SerializeField] private float lifetime = 5f;
    [SerializeField] private bool isPlayerBullet = true;

    private float lifetimeTimer;

    public bool IsPlayerBullet => isPlayerBullet;
    public int Damage => damage;

    /// <summary>
    /// Initialize the bullet after spawning.
    /// </summary>
    public void Initialize(bool playerBullet, int dmg = 10, float spd = -1f)
    {
        isPlayerBullet = playerBullet;
        damage = dmg;
        if (spd > 0) speed = spd;
        lifetimeTimer = lifetime;
    }

    private void Update()
    {
        // Move in the bullet's up direction (local Y axis)
        transform.Translate(Vector3.up * speed * Time.deltaTime, Space.Self);

        lifetimeTimer -= Time.deltaTime;
        if (lifetimeTimer <= 0f)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isPlayerBullet)
        {
            // Player bullet hits enemies
            EnemyBase enemy = other.GetComponent<EnemyBase>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
                Destroy(gameObject);
            }
        }
        else
        {
            // Enemy bullet hits player
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.TakeDamage(damage);
                Destroy(gameObject);
            }
        }
    }
}

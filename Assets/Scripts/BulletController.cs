using UnityEngine;

/// <summary>
/// Generic projectile used by both player and enemies.
/// Direction, speed, and ownership are set via Init().
/// </summary>
[RequireComponent(typeof(BoxCollider2D))]
public class BulletController : MonoBehaviour
{
    [Header("Defaults (overridden by Init)")]
    [SerializeField] private float  defaultSpeed  = 10f;
    [SerializeField] private int    damage        = 1;
    [SerializeField] private float  lifetime      = 5f;

    private Vector3 direction;
    private float   speed;
    private bool    isPlayerBullet;
    private bool    initialised;

    /// <summary>
    /// Called by the shooter to configure the bullet.
    /// </summary>
    public void Init(Vector3 dir, float spd, bool playerOwned)
    {
        direction      = dir.normalized;
        speed          = spd;
        isPlayerBullet = playerOwned;
        initialised    = true;

        // Tint enemy bullets red, player bullets yellow
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.color = isPlayerBullet ? new Color(1f, 0.95f, 0.3f) : new Color(1f, 0.3f, 0.3f);

        // Set layer for selective collision
        gameObject.layer = LayerMask.NameToLayer(isPlayerBullet ? "PlayerBullet" : "EnemyBullet");

        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        if (!initialised)
        {
            // Fallback: fly upward (player default)
            direction = Vector3.up;
            speed = defaultSpeed;
            isPlayerBullet = true;
            initialised = true;
        }

        transform.position += direction * speed * Time.deltaTime;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isPlayerBullet)
        {
            EnemyController enemy = other.GetComponent<EnemyController>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
                Destroy(gameObject);
            }
        }
        else
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.TakeDamage(damage);
                Destroy(gameObject);
            }
        }
    }
}

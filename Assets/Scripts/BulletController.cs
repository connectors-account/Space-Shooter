using UnityEngine;

/// <summary>
/// Controls bullet movement, lifetime, and damage.
/// Attach to bullet prefabs with Rigidbody2D (kinematic) and CircleCollider2D (trigger).
/// </summary>
public class BulletController : MonoBehaviour
{
    [Header("Bullet Settings")]
    [SerializeField] private int damage = 10;
    [SerializeField] private float lifetime = 5f;

    private Vector2 direction;
    private float speed;
    private bool isPlayerBullet;
    private float spawnTime;

    /// <summary>Damage this bullet deals on hit.</summary>
    public int Damage => damage;

    /// <summary>Whether this bullet belongs to the player.</summary>
    public bool IsPlayerBullet => isPlayerBullet;

    /// <summary>
    /// Initialize the bullet with a direction, speed, and ownership.
    /// Called by the spawner (PlayerController or EnemyController).
    /// </summary>
    /// <param name="dir">Normalized direction vector.</param>
    /// <param name="spd">Movement speed in units/second.</param>
    /// <param name="playerBullet">True if fired by the player.</param>
    public void Initialize(Vector2 dir, float spd, bool playerBullet)
    {
        direction = dir.normalized;
        speed = spd;
        isPlayerBullet = playerBullet;
        spawnTime = Time.time;

        // Assign the correct tag for collision filtering
        gameObject.tag = playerBullet ? "PlayerBullet" : "EnemyBullet";

        // Rotate bullet to face movement direction
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    private void Update()
    {
        // Move the bullet
        transform.Translate(direction * speed * Time.deltaTime, Space.World);

        // Destroy after lifetime expires
        if (Time.time - spawnTime >= lifetime)
        {
            Destroy(gameObject);
        }
    }
}

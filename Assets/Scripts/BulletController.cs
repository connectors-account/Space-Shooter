using UnityEngine;

/// <summary>
/// Controls bullet movement, collision detection, and damage dealing.
/// Attach to Bullet prefab GameObjects.
/// </summary>
public class BulletController : MonoBehaviour
{
    [Header("Bullet Settings")]
    public float speed = 12f;
    public int damage = 1;
    public bool isPlayerBullet = true;
    public Vector3 direction = Vector3.up;
    public float lifetime = 4f;

    private float spawnTime;

    void Start()
    {
        spawnTime = Time.time;

        // If the bullet was rotated (spread shots), adjust direction accordingly
        if (transform.rotation != Quaternion.identity && isPlayerBullet)
        {
            direction = transform.up;
        }
    }

    void Update()
    {
        transform.position += direction.normalized * speed * Time.deltaTime;

        // Destroy if lifetime exceeded or out of bounds
        if (Time.time - spawnTime > lifetime || IsOutOfBounds())
        {
            Destroy(gameObject);
        }
    }

    bool IsOutOfBounds()
    {
        return Mathf.Abs(transform.position.x) > 12f ||
               Mathf.Abs(transform.position.y) > 8f;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (isPlayerBullet && other.CompareTag("Enemy"))
        {
            EnemyController enemy = other.GetComponent<EnemyController>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }
            Destroy(gameObject);
        }
        else if (!isPlayerBullet && other.CompareTag("Player"))
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

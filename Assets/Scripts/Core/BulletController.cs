using UnityEngine;

/// <summary>
/// Controls bullet movement and collision.
/// Bullets are initialized with a direction and speed.
/// Player bullets are tagged "PlayerBullet", enemy bullets "EnemyBullet".
/// </summary>
public class BulletController : MonoBehaviour
{
    public int damage = 1;
    public float lifetime = 5f;

    private Vector2 direction;
    private float speed;
    private bool isPlayerBullet;
    private float spawnTime;

    /// <summary>
    /// Call this right after instantiation to set bullet parameters.
    /// </summary>
    public void Initialize(Vector2 dir, float spd, bool playerBullet)
    {
        direction = dir.normalized;
        speed = spd;
        isPlayerBullet = playerBullet;
        spawnTime = Time.time;

        // Set tag based on owner
        gameObject.tag = isPlayerBullet ? "PlayerBullet" : "EnemyBullet";

        // Rotate bullet to face direction of travel
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        // Set color: cyan for player, red for enemy
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = isPlayerBullet ? new Color(0f, 1f, 1f, 1f) : new Color(1f, 0.3f, 0.3f, 1f);
        }
    }

    void Update()
    {
        transform.Translate(direction * speed * Time.deltaTime, Space.World);

        // Destroy if off screen or past lifetime
        if (Time.time - spawnTime > lifetime)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 pos = transform.position;
        if (pos.y > 7f || pos.y < -7f || Mathf.Abs(pos.x) > 12f)
        {
            Destroy(gameObject);
        }
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
        // Enemy bullets hitting player is handled in PlayerController
    }
}

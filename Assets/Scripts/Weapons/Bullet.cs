using UnityEngine;

/// <summary>
/// Bullet behavior - moves in a direction and destroys on impact or leaving screen.
/// Attach to Bullet prefab.
/// </summary>
public class Bullet : MonoBehaviour
{
    public int damage = 1;
    public bool isPlayerBullet = true;
    public float lifetime = 5f;

    private Vector2 direction;
    private float speed;
    private float spawnTime;

    void Start()
    {
        spawnTime = Time.time;
    }

    public void SetDirection(Vector2 dir, float spd)
    {
        direction = dir.normalized;
        speed = spd;
    }

    void Update()
    {
        transform.Translate(direction * speed * Time.deltaTime, Space.World);

        // Destroy if lifetime exceeded
        if (Time.time - spawnTime > lifetime)
        {
            Destroy(gameObject);
        }

        // Destroy if off screen
        Vector3 viewPos = Camera.main.WorldToViewportPoint(transform.position);
        if (viewPos.x < -0.1f || viewPos.x > 1.1f || viewPos.y < -0.1f || viewPos.y > 1.1f)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (isPlayerBullet && other.CompareTag("Enemy"))
        {
            EnemyBase enemy = other.GetComponent<EnemyBase>();
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

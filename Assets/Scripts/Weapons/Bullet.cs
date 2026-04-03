using UnityEngine;

/// <summary>
/// Bullet projectile behavior. Used by both player and enemy bullets.
/// Moves in a direction at a speed and destroys itself when off-screen.
/// </summary>
public class Bullet : MonoBehaviour
{
    public float speed = 12f;
    public Vector3 direction = Vector3.up;
    public bool isPlayerBullet = true;
    public int damage = 1;
    public float lifetime = 5f;

    private float timer;

    void Start()
    {
        timer = lifetime;
    }

    void Update()
    {
        transform.position += direction.normalized * speed * Time.deltaTime;

        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (isPlayerBullet)
        {
            EnemyHealth enemy = other.GetComponent<EnemyHealth>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
                Destroy(gameObject);
            }
        }
        else
        {
            PlayerHealth player = other.GetComponent<PlayerHealth>();
            if (player != null)
            {
                player.TakeDamage(damage);
                Destroy(gameObject);
            }
        }
    }
}

using UnityEngine;

/// <summary>
/// Projectile that moves in a direction and deals damage on collision.
/// Used for both player and enemy bullets.
/// </summary>
public class Bullet : MonoBehaviour
{
    [Header("Bullet Settings")]
    public float speed = 12f;
    public int damage = 25;
    public bool isPlayerBullet = true;
    public float lifetime = 5f;

    [Header("Visual")]
    public bool rotateToDirection = false;

    private Vector3 moveDirection = Vector3.up;
    private float spawnTime;

    private void Start()
    {
        spawnTime = Time.time;

        // If rotated, move in the local up direction
        if (rotateToDirection)
        {
            moveDirection = transform.up;
        }
    }

    private void Update()
    {
        transform.Translate(moveDirection * speed * Time.deltaTime, Space.World);

        // Destroy if off-screen or lifetime expired
        if (Time.time - spawnTime > lifetime || IsOffScreen())
        {
            Destroy(gameObject);
        }
    }

    /// <summary>Set custom movement direction.</summary>
    public void SetDirection(Vector3 direction)
    {
        moveDirection = direction.normalized;
        if (rotateToDirection)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isPlayerBullet)
        {
            // Player bullet hits enemy
            EnemyBase enemy = other.GetComponent<EnemyBase>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
                Destroy(gameObject);
                return;
            }
        }
        else
        {
            // Enemy bullet hits player
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.HandleHit(damage);
                Destroy(gameObject);
                return;
            }
        }
    }

    private bool IsOffScreen()
    {
        Vector3 pos = transform.position;
        return pos.y > 8f || pos.y < -8f || pos.x > 12f || pos.x < -12f;
    }
}

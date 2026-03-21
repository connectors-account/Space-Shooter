using UnityEngine;

/// <summary>
/// Enemy bullet variant. Inherits Bullet behavior with enemy-specific defaults.
/// This is a convenience component; you can also use Bullet with isPlayerBullet = false.
/// </summary>
public class EnemyBullet : MonoBehaviour
{
    [Header("Enemy Bullet Settings")]
    public float speed = 6f;
    public int damage = 20;
    public float lifetime = 6f;

    private Vector3 moveDirection = Vector3.down;
    private float spawnTime;

    private void Start()
    {
        spawnTime = Time.time;
    }

    private void Update()
    {
        transform.Translate(moveDirection * speed * Time.deltaTime, Space.World);

        if (Time.time - spawnTime > lifetime || IsOffScreen())
        {
            Destroy(gameObject);
        }
    }

    /// <summary>Set custom direction for aimed shots.</summary>
    public void SetDirection(Vector3 direction)
    {
        moveDirection = direction.normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            player.HandleHit(damage);
            Destroy(gameObject);
        }
    }

    private bool IsOffScreen()
    {
        Vector3 pos = transform.position;
        return pos.y > 8f || pos.y < -8f || pos.x > 12f || pos.x < -12f;
    }
}

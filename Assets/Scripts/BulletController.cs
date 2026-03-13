using UnityEngine;

public class BulletController : MonoBehaviour
{
    [Header("Bullet Settings")]
    public float speed = 15f;
    public int damage = 1;
    public float lifetime = 3f;
    public bool isPlayerBullet = true;

    private Vector2 direction = Vector2.up;
    private float destroyTime;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        destroyTime = Time.time + lifetime;
        spriteRenderer = GetComponent<SpriteRenderer>();
        UpdateVisual();
    }

    void Update()
    {
        transform.position += (Vector3)(direction * speed * Time.deltaTime);

        if (Time.time >= destroyTime)
        {
            Destroy(gameObject);
        }

        if (Mathf.Abs(transform.position.x) > 12f || Mathf.Abs(transform.position.y) > 8f)
        {
            Destroy(gameObject);
        }
    }

    public void Initialize(Vector2 bulletDirection, float bulletSpeed, bool playerBullet)
    {
        direction = bulletDirection.normalized;
        speed = bulletSpeed;
        isPlayerBullet = playerBullet;
        UpdateVisual();
    }

    void UpdateVisual()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = isPlayerBullet ? Color.cyan : Color.red;
        }

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (isPlayerBullet)
        {
            if (other.CompareTag("Enemy"))
            {
                EnemyController enemy = other.GetComponent<EnemyController>();
                if (enemy != null)
                {
                    enemy.TakeDamage(damage);
                }
                Destroy(gameObject);
            }
        }
        else
        {
            if (other.CompareTag("Player"))
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
}

using UnityEngine;

public class BulletController : MonoBehaviour
{
    public enum BulletOwner
    {
        Player,
        Enemy
    }

    [SerializeField] private float speed = 12f;
    [SerializeField] private float lifeTime = 4f;

    private BulletOwner owner;
    private Vector2 direction = Vector2.up;
    private int damage = 1;

    public void Initialize(BulletOwner newOwner, Vector2 newDirection, int newDamage)
    {
        owner = newOwner;
        direction = newDirection.normalized;
        damage = Mathf.Max(1, newDamage);

        gameObject.tag = owner == BulletOwner.Player ? "PlayerBullet" : "EnemyBullet";

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        transform.position += (Vector3)(direction * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (owner == BulletOwner.Player && other.CompareTag("Enemy"))
        {
            EnemyController enemy = other.GetComponent<EnemyController>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }

            Destroy(gameObject);
            return;
        }

        if (owner == BulletOwner.Enemy && other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.TakeDamage(damage);
            }

            Destroy(gameObject);
            return;
        }

        if (other.CompareTag("Bounds"))
        {
            Destroy(gameObject);
        }
    }
}

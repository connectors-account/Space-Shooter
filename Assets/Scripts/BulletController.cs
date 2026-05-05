using UnityEngine;

/// <summary>
/// Handles projectile movement and collision logic for both player and enemy bullets.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class BulletController : MonoBehaviour
{
    [SerializeField] private float defaultSpeed = 10f;
    [SerializeField] private int defaultDamage = 1;
    [SerializeField] private float lifetime = 3f;

    private Vector2 direction = Vector2.up;
    private float speed;
    private int damage;
    private bool isPlayerBullet;

    public void Initialize(Vector2 bulletDirection, float bulletSpeed, int bulletDamage, bool firedByPlayer)
    {
        direction = bulletDirection.normalized;
        speed = bulletSpeed;
        damage = bulletDamage;
        isPlayerBullet = firedByPlayer;
    }

    private void Awake()
    {
        speed = defaultSpeed;
        damage = defaultDamage;
    }

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
        {
            return;
        }

        transform.Translate(direction * speed * Time.deltaTime, Space.World);
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

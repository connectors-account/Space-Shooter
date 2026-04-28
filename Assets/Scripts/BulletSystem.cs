using UnityEngine;

public class BulletSystem : MonoBehaviour
{
    public enum BulletOwner
    {
        Player,
        Enemy
    }

    public static BulletSystem Instance { get; private set; }

    [Header("Prefabs")]
    [SerializeField] private GameObject playerBulletPrefab;
    [SerializeField] private GameObject enemyBulletPrefab;

    [Header("Bullet Settings")]
    [SerializeField] private float playerBulletSpeed = 18f;
    [SerializeField] private float enemyBulletSpeed = 10f;
    [SerializeField] private float bulletLifetime = 4f;
    [SerializeField] private int playerBulletDamage = 20;
    [SerializeField] private int enemyBulletDamage = 10;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public static void SpawnPlayerBullet(Vector3 position)
    {
        Instance?.SpawnBullet(position, Vector2.up, BulletOwner.Player);
    }

    public static void SpawnEnemyBullet(Vector3 position, Vector2 direction)
    {
        Instance?.SpawnBullet(position, direction.normalized, BulletOwner.Enemy);
    }

    private void SpawnBullet(Vector3 position, Vector2 direction, BulletOwner owner)
    {
        GameObject prefab = owner == BulletOwner.Player ? playerBulletPrefab : enemyBulletPrefab;
        if (prefab == null)
        {
            Debug.LogWarning($"{owner} bullet prefab is missing on BulletSystem.");
            return;
        }

        GameObject bulletObject = Instantiate(prefab, position, Quaternion.identity);

        BulletRuntime bulletRuntime = bulletObject.GetComponent<BulletRuntime>();
        if (bulletRuntime == null)
        {
            bulletRuntime = bulletObject.AddComponent<BulletRuntime>();
        }

        float speed = owner == BulletOwner.Player ? playerBulletSpeed : enemyBulletSpeed;
        int damage = owner == BulletOwner.Player ? playerBulletDamage : enemyBulletDamage;

        bulletRuntime.Initialize(owner, direction, speed, damage, bulletLifetime);
    }

    [RequireComponent(typeof(Collider2D))]
    public class BulletRuntime : MonoBehaviour
    {
        private BulletOwner owner;
        private Vector2 moveDirection;
        private float speed;
        private float expireTime;

        public int Damage { get; private set; }

        public void Initialize(BulletOwner bulletOwner, Vector2 direction, float bulletSpeed, int damage, float lifetime)
        {
            owner = bulletOwner;
            moveDirection = direction.normalized;
            speed = bulletSpeed;
            Damage = damage;
            expireTime = Time.time + lifetime;

            gameObject.tag = owner == BulletOwner.Player ? "PlayerBullet" : "EnemyBullet";
        }

        private void Update()
        {
            transform.position += (Vector3)(moveDirection * speed * Time.deltaTime);

            if (Time.time >= expireTime)
            {
                Destroy(gameObject);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (owner == BulletOwner.Player)
            {
                if (other.CompareTag("Enemy"))
                {
                    EnemyManager.EnemyRuntime enemy = other.GetComponent<EnemyManager.EnemyRuntime>();
                    enemy?.TakeDamage(Damage);
                    Destroy(gameObject);
                }
            }
            else
            {
                if (other.CompareTag("Player"))
                {
                    PlayerController player = other.GetComponent<PlayerController>();
                    player?.ApplyDamage(Damage);
                    Destroy(gameObject);
                }
            }
        }
    }
}

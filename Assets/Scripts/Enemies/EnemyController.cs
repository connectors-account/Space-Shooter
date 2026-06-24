using UnityEngine;

namespace SpaceShooter
{
    public enum EnemyType
    {
        Straight,   // moves straight down
        Zigzag,     // sine-wave horizontal movement
        Chaser,     // homes toward the player
        Shooter     // moves down slowly and fires at the player
    }

    /// <summary>
    /// Enemy behaviour with several movement patterns, optional shooting,
    /// score reward, power-up drop chance and death handling.
    /// </summary>
    [RequireComponent(typeof(HealthSystem))]
    public class EnemyController : MonoBehaviour
    {
        [Header("Type & Movement")]
        [SerializeField] private EnemyType type = EnemyType.Straight;
        [SerializeField] private float moveSpeed = 3f;
        [SerializeField] private float zigzagAmplitude = 2f;
        [SerializeField] private float zigzagFrequency = 2f;
        [SerializeField] private float chaseSpeed = 2f;

        [Header("Combat")]
        [SerializeField] private float collisionDamage = 20f;
        [SerializeField] private int scoreValue = 100;
        [SerializeField] private GameObject bulletPrefab;
        [SerializeField] private Transform firePoint;
        [SerializeField] private float fireInterval = 1.5f;
        [SerializeField] private float bulletSpeed = 7f;
        [SerializeField] private float bulletDamage = 15f;

        [Header("Drops & FX")]
        [SerializeField] private GameObject explosionEffect;
        [SerializeField] private GameObject[] powerUpPrefabs;
        [Range(0f, 1f)]
        [SerializeField] private float powerUpDropChance = 0.15f;

        private HealthSystem health;
        private Transform player;
        private float startX;
        private float spawnTime;
        private float fireTimer;
        private float despawnY;

        public float CollisionDamage => collisionDamage;
        public EnemyType Type => type;

        private void Awake()
        {
            health = GetComponent<HealthSystem>();
        }

        private void OnEnable()
        {
            health.OnDeath += HandleDeath;
        }

        private void OnDisable()
        {
            health.OnDeath -= HandleDeath;
        }

        private void Start()
        {
            startX = transform.position.x;
            spawnTime = Time.time;
            fireTimer = fireInterval;

            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) player = playerObj.transform;

            if (Camera.main != null)
            {
                despawnY = Camera.main.ViewportToWorldPoint(new Vector3(0f, 0f, 0f)).y - 2f;
            }
            else
            {
                despawnY = -12f;
            }
        }

        private void Update()
        {
            Move();
            HandleShooting();

            // Clean up enemies that leave the bottom of the screen.
            if (transform.position.y < despawnY)
            {
                Destroy(gameObject);
            }
        }

        private void Move()
        {
            switch (type)
            {
                case EnemyType.Straight:
                case EnemyType.Shooter:
                    transform.Translate(Vector2.down * moveSpeed * Time.deltaTime, Space.World);
                    break;

                case EnemyType.Zigzag:
                    float elapsed = Time.time - spawnTime;
                    float offsetX = Mathf.Sin(elapsed * zigzagFrequency) * zigzagAmplitude;
                    Vector3 pos = transform.position;
                    pos.y -= moveSpeed * Time.deltaTime;
                    pos.x = startX + offsetX;
                    transform.position = pos;
                    break;

                case EnemyType.Chaser:
                    // Always drift down, and steer horizontally toward the player.
                    Vector3 newPos = transform.position;
                    newPos.y -= moveSpeed * Time.deltaTime;
                    if (player != null)
                    {
                        newPos.x = Mathf.MoveTowards(newPos.x, player.position.x, chaseSpeed * Time.deltaTime);
                    }
                    transform.position = newPos;
                    break;
            }
        }

        private void HandleShooting()
        {
            if (type != EnemyType.Shooter || bulletPrefab == null) return;

            fireTimer -= Time.deltaTime;
            if (fireTimer <= 0f)
            {
                fireTimer = fireInterval;
                Shoot();
            }
        }

        private void Shoot()
        {
            Vector3 origin = firePoint != null ? firePoint.position : transform.position;
            Vector2 dir = Vector2.down;

            if (player != null)
            {
                dir = (player.position - origin).normalized;
            }

            GameObject bullet = Instantiate(bulletPrefab, origin, Quaternion.identity);
            BulletController bc = bullet.GetComponent<BulletController>();
            if (bc != null)
            {
                bc.Configure(BulletOwner.Enemy, dir, bulletSpeed, bulletDamage);
            }

            AudioManager.Instance?.PlayEnemyShoot();
        }

        /// <summary>Kill the enemy. When <paramref name="grantScore"/> is true the player earns points.</summary>
        public void Die(bool grantScore = true)
        {
            if (grantScore)
            {
                ScoreManager.Instance?.AddScore(scoreValue);
                TryDropPowerUp();
            }

            if (explosionEffect != null)
            {
                Instantiate(explosionEffect, transform.position, Quaternion.identity);
            }

            AudioManager.Instance?.PlayExplosion();
            Destroy(gameObject);
        }

        private void HandleDeath()
        {
            Die(true);
        }

        private void TryDropPowerUp()
        {
            if (powerUpPrefabs == null || powerUpPrefabs.Length == 0) return;
            if (Random.value > powerUpDropChance) return;

            GameObject prefab = powerUpPrefabs[Random.Range(0, powerUpPrefabs.Length)];
            if (prefab != null)
            {
                Instantiate(prefab, transform.position, Quaternion.identity);
            }
        }
    }
}

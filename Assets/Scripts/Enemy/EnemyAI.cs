using UnityEngine;

namespace SpaceShooter.Enemy
{
    /// <summary>
    /// Enemy movement patterns and shooting behavior.
    /// </summary>
    public class EnemyAI : MonoBehaviour
    {
        public enum EnemyType
        {
            Straight,
            ZigZag,
            SineDive
        }

        [Header("Type")]
        [SerializeField] private EnemyType enemyType = EnemyType.Straight;

        [Header("Movement")]
        [SerializeField] private float moveSpeed = 2.5f;
        [SerializeField] private float zigzagAmplitude = 2f;
        [SerializeField] private float zigzagFrequency = 2.5f;

        [Header("Combat")]
        [SerializeField] private Combat.Bullet enemyBulletPrefab;
        [SerializeField] private Transform firePoint;
        [SerializeField] private float baseShootCooldown = 1.6f;

        [Header("Score + Damage")]
        [SerializeField] private int scoreValue = 100;
        [SerializeField] private int contactDamage = 15;
        [SerializeField] private Combat.Damageable damageable;

        private float spawnX;
        private float elapsed;
        private float nextShootTime;

        public int ScoreValue => scoreValue;
        public int ContactDamage => contactDamage;

        private void Awake()
        {
            spawnX = transform.position.x;
            if (damageable == null)
            {
                damageable = GetComponent<Combat.Damageable>();
            }
        }

        private void Start()
        {
            nextShootTime = Time.time + Random.Range(0.4f, 1.2f);
            if (damageable != null)
            {
                damageable.OnDied += HandleDied;
            }
        }

        private void OnDestroy()
        {
            if (damageable != null)
            {
                damageable.OnDied -= HandleDied;
            }
        }

        private void Update()
        {
            if (Core.GameManager.Instance == null || !Core.GameManager.Instance.IsGameplayActive())
            {
                return;
            }

            elapsed += Time.deltaTime;
            MovePattern();
            TryShoot();

            if (transform.position.y < -6.3f)
            {
                Destroy(gameObject);
            }
        }

        public void Configure(EnemyType type, float speed, int health, int score, float shootCooldown)
        {
            enemyType = type;
            moveSpeed = speed;
            scoreValue = score;
            baseShootCooldown = shootCooldown;

            if (damageable != null)
            {
                damageable.SetMaxHealth(health);
            }
        }

        private void MovePattern()
        {
            Vector3 pos = transform.position;

            switch (enemyType)
            {
                case EnemyType.Straight:
                    pos += Vector3.down * (moveSpeed * Time.deltaTime);
                    break;

                case EnemyType.ZigZag:
                    pos.y -= moveSpeed * Time.deltaTime;
                    pos.x = spawnX + Mathf.Sin(elapsed * zigzagFrequency) * zigzagAmplitude;
                    break;

                case EnemyType.SineDive:
                    pos.y -= (moveSpeed * 1.2f) * Time.deltaTime;
                    pos.x = spawnX + Mathf.Sin(elapsed * (zigzagFrequency * 0.8f)) * (zigzagAmplitude * 0.6f);
                    break;
            }

            transform.position = pos;
        }

        private void TryShoot()
        {
            if (enemyBulletPrefab == null || firePoint == null)
            {
                return;
            }

            if (Time.time < nextShootTime)
            {
                return;
            }

            Vector2 direction = Vector2.down;

            var player = FindObjectOfType<Player.PlayerController>();
            if (player != null)
            {
                direction = ((Vector2)player.transform.position - (Vector2)firePoint.position).normalized;
            }

            Combat.Bullet bullet = Instantiate(enemyBulletPrefab, firePoint.position, Quaternion.identity);
            bullet.Initialize(direction, Combat.Faction.Enemy);

            nextShootTime = Time.time + baseShootCooldown;
        }

        private void HandleDied(GameObject source)
        {
            Systems.ScoreSystem.Instance?.AddScore(scoreValue);
            Systems.PowerUpSystem.Instance?.TrySpawnPowerUp(transform.position);
            Audio.SoundManager.Instance?.PlayExplosion();
        }
    }
}

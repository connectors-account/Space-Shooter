using SpaceShooter.Audio;
using SpaceShooter.Combat;
using SpaceShooter.Core;
using SpaceShooter.Powerups;
using UnityEngine;

namespace SpaceShooter.Enemy
{
    public enum EnemyType
    {
        Basic,
        Fast,
        Tank
    }

    [RequireComponent(typeof(Collider2D))]
    public class EnemyController : MonoBehaviour
    {
        [Header("Enemy Stats")]
        [SerializeField] private EnemyType enemyType;
        [SerializeField] private int baseHealth = 2;
        [SerializeField] private float baseSpeed = 2f;
        [SerializeField] private int collisionDamage = 20;
        [SerializeField] private int scoreValue = 100;

        [Header("Combat")]
        [SerializeField] private GameObject enemyBulletPrefab;
        [SerializeField] private Transform firePoint;
        [SerializeField] private float shotInterval = 1.8f;

        private int health;
        private float speed;
        private float phase;
        private float nextShotTime;
        private Camera cam;

        public EnemyType Type => enemyType;

        private void Awake()
        {
            cam = Camera.main;
            gameObject.layer = GameLayers.GetLayerOrDefault(GameLayers.Enemy);
            ConfigureByType();
            phase = Random.Range(0f, 10f);
        }

        private void Update()
        {
            if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Playing)
            {
                return;
            }

            Move();
            TryShoot();

            if (cam != null && transform.position.y < ScreenBounds.MinWorld(cam).y - 1.3f)
            {
                Destroy(gameObject);
            }
        }

        public void Initialize(EnemyType type, float speedMultiplier, int healthBonus)
        {
            enemyType = type;
            ConfigureByType();
            speed *= speedMultiplier;
            health += healthBonus;
        }

        private void ConfigureByType()
        {
            switch (enemyType)
            {
                case EnemyType.Basic:
                    health = baseHealth;
                    speed = baseSpeed;
                    scoreValue = 100;
                    shotInterval = 2f;
                    break;
                case EnemyType.Fast:
                    health = Mathf.Max(1, baseHealth - 1);
                    speed = baseSpeed * 1.8f;
                    scoreValue = 150;
                    shotInterval = 1.7f;
                    break;
                case EnemyType.Tank:
                    health = baseHealth + 5;
                    speed = baseSpeed * 0.65f;
                    scoreValue = 300;
                    shotInterval = 2.3f;
                    break;
            }
        }

        private void Move()
        {
            Vector3 dir = Vector3.down;
            if (enemyType == EnemyType.Fast)
            {
                float side = Mathf.Sin((Time.time + phase) * 6f) * 0.9f;
                dir += Vector3.right * side;
            }
            else if (enemyType == EnemyType.Tank)
            {
                float side = Mathf.Sin((Time.time + phase) * 2f) * 0.35f;
                dir += Vector3.right * side;
            }

            transform.position += dir.normalized * speed * Time.deltaTime;
        }

        private void TryShoot()
        {
            if (enemyBulletPrefab == null || firePoint == null) return;
            if (Time.time < nextShotTime) return;

            float chance = enemyType == EnemyType.Tank ? 0.52f : 0.25f;
            if (Random.value > chance) return;

            nextShotTime = Time.time + shotInterval;
            GameObject bulletObj = Instantiate(enemyBulletPrefab, firePoint.position, Quaternion.identity);
            Bullet bullet = bulletObj.GetComponent<Bullet>();
            if (bullet != null)
            {
                int damage = enemyType == EnemyType.Tank ? 15 : 10;
                float bulletSpeed = enemyType == EnemyType.Fast ? 8f : 6.5f;
                bullet.Initialize(BulletOwner.Enemy, damage, Vector2.down, bulletSpeed);
            }
        }

        public void TakeDamage(int amount)
        {
            health -= amount;
            if (health <= 0)
            {
                Die();
            }
        }

        private void Die()
        {
            GameManager.Instance?.AddScore(scoreValue);
            PowerUpSpawner.Instance?.TrySpawn(transform.position);
            SoundManager.Instance?.PlayExplosion();
            Destroy(gameObject);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            Player.PlayerController player = other.GetComponent<Player.PlayerController>();
            if (player != null)
            {
                player.TakeDamage(collisionDamage);
                Die();
            }
        }
    }
}

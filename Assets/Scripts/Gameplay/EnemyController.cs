using SpaceShooter.Core;
using UnityEngine;

namespace SpaceShooter.Gameplay
{
    public class EnemyController : MonoBehaviour
    {
        public enum EnemyType
        {
            Basic,
            ZigZag,
            Tank
        }

        private EnemyType enemyType;
        private int health;
        private int scoreValue;
        private float speed;
        private float fireInterval;
        private float nextFireTime;
        private float elapsed;
        private float startX;

        public void ConfigureByType(EnemyType type)
        {
            enemyType = type;
            startX = transform.position.x;

            switch (enemyType)
            {
                case EnemyType.Basic:
                    health = 20;
                    speed = 2.6f;
                    scoreValue = 100;
                    fireInterval = 1.8f;
                    break;
                case EnemyType.ZigZag:
                    health = 15;
                    speed = 3f;
                    scoreValue = 130;
                    fireInterval = 1.3f;
                    break;
                default:
                    health = 45;
                    speed = 1.6f;
                    scoreValue = 220;
                    fireInterval = 1.15f;
                    break;
            }

            nextFireTime = Time.time + Random.Range(0.3f, fireInterval);
        }

        private void Update()
        {
            if (GameManager.Instance == null || GameManager.Instance.State != GameManager.GameState.Playing)
            {
                return;
            }

            elapsed += Time.deltaTime;
            MovePattern();
            ShootPattern();

            if (transform.position.y < -6.5f)
            {
                Destroy(gameObject);
                GameManager.Instance.ReportEnemyDestroyed(0);
            }
        }

        private void MovePattern()
        {
            Vector3 delta = Vector3.down * speed * Time.deltaTime;

            if (enemyType == EnemyType.ZigZag)
            {
                float x = startX + Mathf.Sin(elapsed * 4f) * 1.4f;
                transform.position = new Vector3(x, transform.position.y + delta.y, 0f);
                return;
            }

            if (enemyType == EnemyType.Tank)
            {
                if (transform.position.y > 1.2f)
                {
                    transform.position += delta;
                }
                else
                {
                    float xDrift = Mathf.Sin(elapsed * 1.8f) * 0.8f;
                    transform.position = new Vector3(startX + xDrift, transform.position.y, 0f);
                }
                return;
            }

            transform.position += delta;
        }

        private void ShootPattern()
        {
            if (Time.time < nextFireTime)
            {
                return;
            }

            nextFireTime = Time.time + fireInterval;
            PlayerController player = FindObjectOfType<PlayerController>();

            switch (enemyType)
            {
                case EnemyType.Basic:
                    EntityFactory.CreateBullet(transform.position + Vector3.down * 0.5f, Vector2.down, BulletController.BulletOwner.Enemy, 7f, 10);
                    break;
                case EnemyType.ZigZag:
                    Vector2 direction = player == null
                        ? Vector2.down
                        : ((Vector2)player.transform.position - (Vector2)transform.position).normalized;
                    EntityFactory.CreateBullet(transform.position + Vector3.down * 0.5f, direction, BulletController.BulletOwner.Enemy, 8.2f, 10);
                    break;
                case EnemyType.Tank:
                    EntityFactory.CreateBullet(transform.position + Vector3.down * 0.5f, (Vector2.down + Vector2.left * 0.2f).normalized, BulletController.BulletOwner.Enemy, 7f, 12);
                    EntityFactory.CreateBullet(transform.position + Vector3.down * 0.5f, Vector2.down, BulletController.BulletOwner.Enemy, 7f, 12);
                    EntityFactory.CreateBullet(transform.position + Vector3.down * 0.5f, (Vector2.down + Vector2.right * 0.2f).normalized, BulletController.BulletOwner.Enemy, 7f, 12);
                    break;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            BulletController bullet = other.GetComponent<BulletController>();
            if (bullet == null || bullet.Owner != BulletController.BulletOwner.Player)
            {
                return;
            }

            health -= bullet.Damage;
            Destroy(other.gameObject);

            if (health <= 0)
            {
                TrySpawnPowerUp();
                FindObjectOfType<AudioManager>()?.PlayEnemyHit();
                Destroy(gameObject);
                GameManager.Instance.ReportEnemyDestroyed(scoreValue);
            }
        }

        private void TrySpawnPowerUp()
        {
            float chance = Random.value;
            if (chance > 0.20f)
            {
                return;
            }

            PowerUpController.PowerUpType type;
            float pick = Random.value;
            if (pick < 0.34f)
            {
                type = PowerUpController.PowerUpType.Health;
            }
            else if (pick < 0.67f)
            {
                type = PowerUpController.PowerUpType.RapidFire;
            }
            else
            {
                type = PowerUpController.PowerUpType.Shield;
            }

            EntityFactory.CreatePowerUp(type, transform.position);
        }
    }
}

using SpaceShooter.Audio;
using SpaceShooter.Combat;
using SpaceShooter.Core;
using SpaceShooter.PowerUps;
using SpaceShooter.Player;
using SpaceShooter.Utils;
using UnityEngine;

namespace SpaceShooter.Enemy
{
    public enum EnemyType
    {
        Chaser,
        ZigZag,
        Turret
    }

    [RequireComponent(typeof(SpriteRenderer), typeof(CircleCollider2D), typeof(Rigidbody2D))]
    public class EnemyController : MonoBehaviour
    {
        public static int ActiveEnemyCount { get; private set; }

        public EnemyType Type { get; private set; }
        private int _health;
        private float _speed;
        private int _scoreValue;
        private float _nextShootTime;
        private float _zigZagPhase;

        private void OnEnable()
        {
            ActiveEnemyCount++;
        }

        private void OnDestroy()
        {
            ActiveEnemyCount = Mathf.Max(0, ActiveEnemyCount - 1);
        }

        public void Initialize(EnemyType type, int health, float speed, int scoreValue)
        {
            Type = type;
            _health = health;
            _speed = speed;
            _scoreValue = scoreValue;
            _zigZagPhase = Random.Range(0f, Mathf.PI * 2f);

            var renderer = GetComponent<SpriteRenderer>();
            renderer.sprite = type switch
            {
                EnemyType.Chaser => SpriteFactory.GetSprite(new Color(1f, 0.35f, 0.35f), ShapeType.Square, 32),
                EnemyType.ZigZag => SpriteFactory.GetSprite(new Color(1f, 0.75f, 0.25f), ShapeType.Triangle, 36),
                EnemyType.Turret => SpriteFactory.GetSprite(new Color(0.9f, 0.2f, 0.9f), ShapeType.Circle, 34),
                _ => SpriteFactory.GetSprite(Color.red, ShapeType.Square, 32)
            };

            var collider = GetComponent<CircleCollider2D>();
            collider.isTrigger = true;
            collider.radius = 0.45f;

            var rb = GetComponent<Rigidbody2D>();
            rb.isKinematic = true;
            rb.gravityScale = 0f;

            _nextShootTime = Time.time + Random.Range(0.4f, 1.5f);
        }

        private void Update()
        {
            if (GameManager.Instance == null || GameManager.Instance.State != GameState.Playing)
            {
                return;
            }

            Move();
            ShootIfNeeded();

            if (transform.position.y < -6.5f)
            {
                Destroy(gameObject);
            }
        }

        public void TakeDamage(int amount)
        {
            _health -= Mathf.Max(1, amount);
            if (_health > 0)
            {
                return;
            }

            Die();
        }

        private void Move()
        {
            var position = transform.position;

            switch (Type)
            {
                case EnemyType.Chaser:
                    if (PlayerController.Instance != null)
                    {
                        var towardPlayer = (PlayerController.Instance.transform.position - transform.position).normalized;
                        position += new Vector3(towardPlayer.x * 0.8f, -1f, 0f) * (_speed * Time.deltaTime);
                    }
                    else
                    {
                        position += Vector3.down * (_speed * Time.deltaTime);
                    }
                    break;

                case EnemyType.ZigZag:
                    var horizontal = Mathf.Sin(Time.time * 3f + _zigZagPhase) * 2f;
                    position += new Vector3(horizontal, -1f, 0f) * (_speed * Time.deltaTime);
                    break;

                case EnemyType.Turret:
                    position += Vector3.down * (_speed * 0.45f * Time.deltaTime);
                    break;
            }

            transform.position = position;
        }

        private void ShootIfNeeded()
        {
            if (Time.time < _nextShootTime)
            {
                return;
            }

            switch (Type)
            {
                case EnemyType.Chaser:
                    SpawnEnemyBullet(Vector2.down, 6f, 1);
                    _nextShootTime = Time.time + 1.3f;
                    break;

                case EnemyType.ZigZag:
                    SpawnEnemyBullet(new Vector2(-0.25f, -1f), 5.5f, 1);
                    SpawnEnemyBullet(new Vector2(0f, -1f), 5.5f, 1);
                    SpawnEnemyBullet(new Vector2(0.25f, -1f), 5.5f, 1);
                    _nextShootTime = Time.time + 1.8f;
                    break;

                case EnemyType.Turret:
                    for (var i = 0; i < 8; i++)
                    {
                        var angle = i * Mathf.PI * 2f / 8f;
                        var direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                        if (direction.y <= -0.05f)
                        {
                            SpawnEnemyBullet(direction, 4.6f, 1);
                        }
                    }
                    _nextShootTime = Time.time + 2.4f;
                    break;
            }
        }

        private void SpawnEnemyBullet(Vector2 direction, float speed, int damage)
        {
            var bulletObject = new GameObject("EnemyBullet");
            bulletObject.transform.position = transform.position;
            var bullet = bulletObject.AddComponent<Bullet>();
            bullet.Initialize(direction, speed, damage, false, new Color(1f, 0.45f, 0.2f));
        }

        private void Die()
        {
            GameManager.Instance?.AddScore(_scoreValue);
            PowerUp.SpawnRandom(transform.position);
            AudioManager.Instance?.PlayExplosion();
            Destroy(gameObject);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            var player = other.GetComponent<PlayerController>();
            if (player == null)
            {
                return;
            }

            player.TakeDamage(1);
            Die();
        }
    }
}

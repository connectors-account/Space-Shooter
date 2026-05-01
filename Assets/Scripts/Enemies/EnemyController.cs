using SpaceShooter.Core;
using SpaceShooter.Projectiles;
using SpaceShooter.Visual;
using UnityEngine;

namespace SpaceShooter.Enemies
{
    public enum EnemyType
    {
        Grunt,
        Sine,
        Shooter
    }

    public class EnemyController : MonoBehaviour, IDamageable
    {
        private GameManager _gameManager;
        private ObjectPoolManager _pool;
        private GameConfig _config;

        private float _speed;
        private int _health;
        private int _scoreValue;
        private float _shootCooldown;
        private float _shootTimer;
        private float _lifeTime;
        private float _seed;

        public EnemyType EnemyType { get; private set; }
        public Faction Faction => Faction.Enemy;

        public void Initialize(GameManager gameManager, ObjectPoolManager pool, GameConfig config, EnemyType enemyType, float difficultyScale)
        {
            _gameManager = gameManager;
            _pool = pool;
            _config = config;
            EnemyType = enemyType;
            _seed = Random.Range(-1f, 1f);

            switch (EnemyType)
            {
                case EnemyType.Grunt:
                    _health = 30;
                    _speed = config.EnemyBaseMoveSpeed * (1f + difficultyScale * 0.7f);
                    _scoreValue = 100;
                    _shootCooldown = 999f;
                    break;
                case EnemyType.Sine:
                    _health = 45;
                    _speed = config.EnemyBaseMoveSpeed * (0.85f + difficultyScale * 0.65f);
                    _scoreValue = 160;
                    _shootCooldown = 1.6f;
                    break;
                default:
                    _health = 65;
                    _speed = config.EnemyBaseMoveSpeed * (0.75f + difficultyScale * 0.6f);
                    _scoreValue = 220;
                    _shootCooldown = 1.15f;
                    break;
            }

            _shootTimer = Random.Range(0.1f, _shootCooldown);
            _lifeTime = 15f;
        }

        private void Update()
        {
            if (_gameManager.CurrentState != GameState.Playing) return;

            _lifeTime -= Time.deltaTime;
            if (_lifeTime <= 0f || transform.position.y < -6.8f)
            {
                Despawn(false);
                return;
            }

            var movement = Vector3.down * (_speed * Time.deltaTime);
            if (EnemyType == EnemyType.Sine)
            {
                movement.x = Mathf.Sin(Time.time * 3f + _seed * 4f) * 1.8f * Time.deltaTime;
            }
            else if (EnemyType == EnemyType.Shooter)
            {
                movement.x = Mathf.Sin(Time.time * 2f + _seed * 3f) * 1.2f * Time.deltaTime;
            }

            transform.position += movement;

            _shootTimer -= Time.deltaTime;
            if (_shootTimer <= 0f)
            {
                FirePattern();
                _shootTimer = _shootCooldown;
            }
        }

        public void ApplyDamage(int amount, Vector3 hitPosition)
        {
            if (_gameManager.CurrentState != GameState.Playing) return;
            _health -= amount;
            EffectManager.Instance?.SpawnHit(hitPosition);
            if (_health <= 0)
            {
                Despawn(true);
            }
        }

        private void FirePattern()
        {
            if (EnemyType == EnemyType.Grunt) return;

            var basePos = transform.position + Vector3.down * 0.6f;
            if (EnemyType == EnemyType.Sine)
            {
                SpawnEnemyBullet(basePos, Vector2.down);
            }
            else
            {
                SpawnEnemyBullet(basePos, (Vector2.down + Vector2.left * 0.25f).normalized);
                SpawnEnemyBullet(basePos, Vector2.down);
                SpawnEnemyBullet(basePos, (Vector2.down + Vector2.right * 0.25f).normalized);
            }
            Sound.SoundManager.Instance?.PlaySfx("enemy_shoot");
        }

        private void SpawnEnemyBullet(Vector3 position, Vector2 direction)
        {
            var bullet = _pool.Get("bullet_enemy", position, Quaternion.identity);
            if (bullet == null) return;
            bullet.GetComponent<Projectile>().Initialize(_pool, Faction.Enemy, direction, _config.EnemyBulletSpeed, _config.EnemyBulletDamage);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.TryGetComponent(out Player.PlayerHealth playerHealth))
            {
                playerHealth.ApplyDamage(20, transform.position);
                Despawn(true);
            }
        }

        private void Despawn(bool killedByPlayer)
        {
            if (killedByPlayer)
            {
                _gameManager.AddScore(_scoreValue);
                _gameManager.TrySpawnPowerUp(transform.position);
                EffectManager.Instance?.SpawnExplosion(transform.position);
                Sound.SoundManager.Instance?.PlaySfx("explosion");
            }

            _gameManager.NotifyEnemyDespawned(this);
            _pool.Release(gameObject);
        }
    }
}

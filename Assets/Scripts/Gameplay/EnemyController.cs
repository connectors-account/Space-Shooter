using SpaceShooter.Core;
using SpaceShooter.Managers;
using UnityEngine;

namespace SpaceShooter.Gameplay
{
    public enum EnemyType
    {
        Basic,
        Zigzag,
        Tank,
        Spinner
    }

    [RequireComponent(typeof(Collider2D))]
    public class EnemyController : MonoBehaviour, IPoolable
    {
        [SerializeField] private int baseHealth = 3;
        [SerializeField] private int scoreValue = 40;

        private EnemyType _type;
        private EnemySpawner _spawner;
        private PoolManager _pool;
        private PooledIdentity _identity;
        private Transform _player;

        private int _health;
        private float _speed;
        private float _shotInterval;
        private float _nextShotTime;
        private float _spawnTime;

        public void Setup(EnemyType type, int wave, EnemySpawner spawner, PoolManager pool, Transform player)
        {
            _type = type;
            _spawner = spawner;
            _pool = pool;
            _player = player;
            _spawnTime = Time.time;

            var waveScalar = 1f + (wave - 1) * 0.15f;
            _health = Mathf.RoundToInt(baseHealth * waveScalar);
            _speed = 1.8f + (wave * 0.22f);
            _shotInterval = Mathf.Max(0.5f, 1.8f - wave * 0.08f);
            _nextShotTime = Time.time + Random.Range(0.4f, _shotInterval);

            if (_type == EnemyType.Tank)
            {
                _health = Mathf.RoundToInt(_health * 2.4f);
                _speed *= 0.72f;
            }
            else if (_type == EnemyType.Spinner)
            {
                _shotInterval *= 0.75f;
            }
            else if (_type == EnemyType.Zigzag)
            {
                _speed *= 1.15f;
            }
        }

        private void Awake()
        {
            _identity = GetComponent<PooledIdentity>();
            var col = GetComponent<Collider2D>();
            col.isTrigger = true;
        }

        private void Update()
        {
            Move();
            Shoot();

            if (transform.position.y < -7f)
            {
                _spawner.NotifyEnemyReturned();
                _identity.ReturnSelfToPool();
            }
        }

        private void Move()
        {
            var position = transform.position;
            position.y -= _speed * Time.deltaTime;

            if (_type == EnemyType.Zigzag)
            {
                position.x += Mathf.Sin((Time.time - _spawnTime) * 4.4f) * 2.2f * Time.deltaTime;
            }
            else if (_type == EnemyType.Spinner)
            {
                position.x += Mathf.Sin((Time.time - _spawnTime) * 7f) * 3f * Time.deltaTime;
                transform.Rotate(0f, 0f, 160f * Time.deltaTime);
            }

            transform.position = position;
        }

        private void Shoot()
        {
            if (Time.time < _nextShotTime)
            {
                return;
            }

            switch (_type)
            {
                case EnemyType.Basic:
                case EnemyType.Tank:
                    Fire(Vector2.down);
                    break;
                case EnemyType.Zigzag:
                    var aimedDirection = _player != null
                        ? ((Vector2)(_player.position - transform.position)).normalized
                        : Vector2.down;
                    Fire(aimedDirection);
                    break;
                case EnemyType.Spinner:
                    FireSpread(4, 20f, -90f);
                    break;
            }

            _nextShotTime = Time.time + _shotInterval;
        }

        private void Fire(Vector2 direction)
        {
            var bulletObj = _pool.Spawn(GameBootstrap.EnemyBulletPool, transform.position, Quaternion.identity);
            if (bulletObj == null)
            {
                return;
            }

            var projectile = bulletObj.GetComponent<Projectile>();
            projectile.Fire(direction, false, 8f, 1);
        }

        private void FireSpread(int count, float angleStep, float centerAngle)
        {
            for (var i = 0; i < count; i++)
            {
                var angle = centerAngle + angleStep * (i - (count - 1) * 0.5f);
                var direction = Quaternion.Euler(0f, 0f, angle) * Vector2.right;
                Fire(direction);
            }
        }

        public void TakeDamage(int damage)
        {
            _health -= damage;
            if (_health > 0)
            {
                return;
            }

            AudioManager.Instance?.PlayExplosion();
            _spawner.NotifyEnemyDestroyed(scoreValue);
            TryDropPowerUp();
            _identity.ReturnSelfToPool();
        }

        private void TryDropPowerUp()
        {
            if (Random.value > 0.22f)
            {
                return;
            }

            var types = (PowerUpType[])System.Enum.GetValues(typeof(PowerUpType));
            var selected = types[Random.Range(0, types.Length)];
            var poolKey = GameBootstrap.GetPowerUpPool(selected);
            var powerUp = _pool.Spawn(poolKey, transform.position, Quaternion.identity);
            if (powerUp == null)
            {
                return;
            }

            var pickup = powerUp.GetComponent<PowerUpPickup>();
            pickup.Configure(selected, GameBootstrap.GetPowerUpColor(selected));
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            var projectile = other.GetComponent<Projectile>();
            if (projectile != null && projectile.FromPlayer)
            {
                TakeDamage(projectile.Damage);
                other.GetComponent<PooledIdentity>()?.ReturnSelfToPool();
                return;
            }

            if (!other.CompareTag("Player"))
            {
                return;
            }

            var player = other.GetComponent<PlayerController>();
            if (player == null)
            {
                return;
            }

            player.TakeDamage(1);
            _spawner.NotifyEnemyDestroyed(scoreValue / 2);
            _identity.ReturnSelfToPool();
        }

        public void OnSpawned()
        {
        }

        public void OnReturnedToPool()
        {
            transform.rotation = Quaternion.identity;
        }
    }
}

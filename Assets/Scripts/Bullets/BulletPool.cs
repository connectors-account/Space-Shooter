using UnityEngine;
using SpaceShooter.Utilities;

namespace SpaceShooter.Bullets
{
    /// <summary>
    /// Singleton that maintains separate object pools for player and enemy bullets.
    /// Default pool size is 50 each; pools expand on demand.
    /// </summary>
    public class BulletPool : MonoBehaviour
    {
        public static BulletPool Instance { get; private set; }

        [Header("Prefabs")]
        [SerializeField] private Bullet playerBulletPrefab;
        [SerializeField] private Bullet enemyBulletPrefab;

        [Header("Pool Sizes")]
        [SerializeField] private int playerPoolSize = 50;
        [SerializeField] private int enemyPoolSize = 50;

        private ObjectPool<Bullet> _playerPool;
        private ObjectPool<Bullet> _enemyPool;

        private Transform _playerParent;
        private Transform _enemyParent;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            _playerParent = new GameObject("PlayerBullets").transform;
            _enemyParent = new GameObject("EnemyBullets").transform;
            _playerParent.SetParent(transform);
            _enemyParent.SetParent(transform);

            if (playerBulletPrefab != null)
            {
                _playerPool = new ObjectPool<Bullet>(playerBulletPrefab, playerPoolSize, _playerParent);
            }
            if (enemyBulletPrefab != null)
            {
                _enemyPool = new ObjectPool<Bullet>(enemyBulletPrefab, enemyPoolSize, _enemyParent);
            }
        }

        /// <summary>Retrieves a player bullet, initializes it, and returns it active.</summary>
        public Bullet GetPlayerBullet(Vector3 position, Vector2 direction, float speed, int damage)
        {
            if (_playerPool == null) return null;
            Bullet b = _playerPool.Get(position, Quaternion.identity);
            b.Initialize(direction, speed, damage, BulletOwner.Player);
            return b;
        }

        /// <summary>Retrieves an enemy bullet, initializes it, and returns it active.</summary>
        public Bullet GetEnemyBullet(Vector3 position, Vector2 direction, float speed, int damage)
        {
            if (_enemyPool == null) return null;
            Bullet b = _enemyPool.Get(position, Quaternion.identity);
            b.Initialize(direction, speed, damage, BulletOwner.Enemy);
            return b;
        }

        /// <summary>Returns a bullet to its correct pool based on owner.</summary>
        public void ReturnBullet(Bullet bullet)
        {
            if (bullet == null) return;
            if (bullet.Owner == BulletOwner.Player)
            {
                _playerPool?.Return(bullet);
            }
            else
            {
                _enemyPool?.Return(bullet);
            }
        }

        /// <summary>Deactivates every active bullet (used when resetting the field).</summary>
        public void ReturnAll()
        {
            _playerPool?.ReturnAll();
            _enemyPool?.ReturnAll();
        }
    }
}

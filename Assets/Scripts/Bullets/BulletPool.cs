using UnityEngine;
using SpaceShooter.Utilities;

namespace SpaceShooter.Bullets
{
    /// <summary>
    /// Singleton that owns object pools for both player and enemy bullets.
    /// </summary>
    public class BulletPool : Singleton<BulletPool>
    {
        [Header("Prefabs")]
        [SerializeField] private Bullet playerBulletPrefab;
        [SerializeField] private EnemyBullet enemyBulletPrefab;

        [Header("Pool Configuration")]
        [SerializeField] private int poolSize = 50;

        private ObjectPool<Bullet> _playerPool;
        private ObjectPool<EnemyBullet> _enemyPool;

        private Transform _playerContainer;
        private Transform _enemyContainer;

        // This pool lives in the Game scene, not across scenes.
        protected override bool PersistAcrossScenes => false;

        protected override void OnAwakeInitialize()
        {
            _playerContainer = new GameObject("PlayerBullets").transform;
            _enemyContainer = new GameObject("EnemyBullets").transform;
            _playerContainer.SetParent(transform);
            _enemyContainer.SetParent(transform);

            if (playerBulletPrefab != null)
            {
                _playerPool = new ObjectPool<Bullet>(playerBulletPrefab, _playerContainer, poolSize);
            }
            else
            {
                Debug.LogError("BulletPool: playerBulletPrefab is not assigned.");
            }

            if (enemyBulletPrefab != null)
            {
                _enemyPool = new ObjectPool<EnemyBullet>(enemyBulletPrefab, _enemyContainer, poolSize);
            }
            else
            {
                Debug.LogError("BulletPool: enemyBulletPrefab is not assigned.");
            }
        }

        public Bullet GetPlayerBullet(Vector3 position, Quaternion rotation)
        {
            if (_playerPool == null)
            {
                return null;
            }

            return _playerPool.Get(position, rotation);
        }

        public EnemyBullet GetEnemyBullet(Vector3 position, Quaternion rotation)
        {
            if (_enemyPool == null)
            {
                return null;
            }

            return _enemyPool.Get(position, rotation);
        }

        public void ReturnPlayerBullet(Bullet bullet)
        {
            _playerPool?.Return(bullet);
        }

        public void ReturnEnemyBullet(EnemyBullet bullet)
        {
            _enemyPool?.Return(bullet);
        }
    }
}

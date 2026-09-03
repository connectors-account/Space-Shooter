using UnityEngine;
using SpaceShooter.Core;

namespace SpaceShooter.Bullets
{
    /// <summary>
    /// Singleton wrapping two GameObjectPools (player bullets, enemy bullets).
    /// Provides typed Get helpers that configure each bullet on retrieval.
    /// </summary>
    public class BulletPool : MonoBehaviour
    {
        #region Singleton
        public static BulletPool Instance { get; private set; }
        #endregion

        #region Inspector Fields
        [Header("Prefabs")]
        [SerializeField] private GameObject _playerBulletPrefab;
        [SerializeField] private GameObject _enemyBulletPrefab;

        [Header("Prewarm Counts")]
        [SerializeField] private int _playerPrewarm = GameConstants.PLAYER_BULLET_POOL_SIZE;
        [SerializeField] private int _enemyPrewarm = GameConstants.ENEMY_BULLET_POOL_SIZE;
        #endregion

        #region Pools
        private GameObjectPool _playerBulletPool;
        private GameObjectPool _enemyBulletPool;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            SetupPools();
        }

        private void SetupPools()
        {
            GameObject playerRoot = new GameObject("PlayerBulletPool");
            playerRoot.transform.SetParent(transform, false);
            _playerBulletPool = playerRoot.AddComponent<GameObjectPool>();
            _playerBulletPool.Initialize(_playerBulletPrefab, _playerPrewarm, playerRoot.transform);

            GameObject enemyRoot = new GameObject("EnemyBulletPool");
            enemyRoot.transform.SetParent(transform, false);
            _enemyBulletPool = enemyRoot.AddComponent<GameObjectPool>();
            _enemyBulletPool.Initialize(_enemyBulletPrefab, _enemyPrewarm, enemyRoot.transform);
        }
        #endregion

        #region Public API
        /// <summary>Fetches and configures a player bullet travelling in dir.</summary>
        public Bullet GetPlayerBullet(Vector3 pos, Vector2 dir)
        {
            if (_playerBulletPool == null) return null;
            GameObject go = _playerBulletPool.Get(pos);
            Bullet b = go.GetComponent<Bullet>();
            if (b == null) b = go.AddComponent<Bullet>();
            b.Configure(pos, dir, false, GameConstants.PLAYER_BULLET_SPEED, GameConstants.PLAYER_BULLET_DAMAGE);
            return b;
        }

        /// <summary>Fetches and configures an enemy bullet travelling in dir.</summary>
        public Bullet GetEnemyBullet(Vector3 pos, Vector2 dir)
        {
            if (_enemyBulletPool == null) return null;
            GameObject go = _enemyBulletPool.Get(pos);
            Bullet b = go.GetComponent<Bullet>();
            if (b == null) b = go.AddComponent<Bullet>();
            b.Configure(pos, dir, true, GameConstants.ENEMY_BULLET_SPEED, GameConstants.ENEMY_BULLET_DAMAGE);
            return b;
        }

        /// <summary>Returns a bullet to the correct pool based on its side.</summary>
        public void ReturnBullet(Bullet bullet)
        {
            if (bullet == null) return;
            if (bullet.IsEnemyBullet)
                _enemyBulletPool?.Return(bullet.gameObject);
            else
                _playerBulletPool?.Return(bullet.gameObject);
        }
        #endregion
    }
}

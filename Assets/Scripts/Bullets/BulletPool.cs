using System.Collections.Generic;
using UnityEngine;

namespace SpaceShooter
{
    /// <summary>
    /// Object pool for player and enemy bullets. Pre-instantiates a fixed number
    /// of each type and grows automatically when a pool runs dry.
    /// </summary>
    public class BulletPool : Singleton<BulletPool>
    {
        [Tooltip("Prefab used for player bullets.")]
        public PlayerBullet playerBulletPrefab;

        [Tooltip("Prefab used for enemy bullets.")]
        public EnemyBullet enemyBulletPrefab;

        [Tooltip("Number of each bullet type pre-instantiated on startup.")]
        public int initialPoolSize = 30;

        [Tooltip("How many extra bullets to create when a pool is empty.")]
        public int expandStep = 10;

        private readonly Queue<BulletBase> _playerInactive = new Queue<BulletBase>();
        private readonly Queue<BulletBase> _enemyInactive = new Queue<BulletBase>();
        private int _playerTotal;
        private int _enemyTotal;
        private bool _initialized;

        protected override void Awake()
        {
            base.Awake();
            // Only auto-initialize if prefabs are wired in the inspector.
            if (playerBulletPrefab != null || enemyBulletPrefab != null)
            {
                InitializePools();
            }
        }

        /// <summary>Pre-instantiates <see cref="initialPoolSize"/> bullets of each type.</summary>
        public void InitializePools()
        {
            if (_initialized) return;

            for (int i = 0; i < initialPoolSize; i++)
            {
                if (playerBulletPrefab != null) CreateBullet(BulletType.Player);
                if (enemyBulletPrefab != null) CreateBullet(BulletType.Enemy);
            }

            _initialized = playerBulletPrefab != null || enemyBulletPrefab != null;
        }

        /// <summary>Serves an activated, positioned bullet of the requested type.</summary>
        public BulletBase GetBullet(BulletType type, Vector3 position, Vector2 direction)
        {
            Queue<BulletBase> queue = QueueFor(type);
            if (queue.Count == 0)
            {
                ExpandPool(type, Mathf.Max(1, expandStep));
            }

            if (queue.Count == 0) return null; // no prefab configured

            BulletBase bullet = queue.Dequeue();
            bullet.gameObject.SetActive(true);
            bullet.Launch(position, direction);
            return bullet;
        }

        /// <summary>Deactivates a bullet and returns it to its pool.</summary>
        public void ReturnBullet(BulletBase bullet)
        {
            if (bullet == null) return;
            if (!bullet.gameObject.activeSelf && QueueFor(bullet.Type).Contains(bullet)) return;

            bullet.gameObject.SetActive(false);
            QueueFor(bullet.Type).Enqueue(bullet);
        }

        public int GetInactiveCount(BulletType type) => QueueFor(type).Count;

        public int GetActiveCount(BulletType type)
        {
            return TotalFor(type) - QueueFor(type).Count;
        }

        public int GetTotalCount(BulletType type) => TotalFor(type);

        private void ExpandPool(BulletType type, int amount)
        {
            for (int i = 0; i < amount; i++)
            {
                CreateBullet(type);
            }
        }

        private void CreateBullet(BulletType type)
        {
            BulletBase prefab = type == BulletType.Player
                ? (BulletBase)playerBulletPrefab
                : enemyBulletPrefab;
            if (prefab == null) return;

            BulletBase bullet = Instantiate(prefab, transform);
            bullet.Type = type;
            bullet.gameObject.SetActive(false);

            QueueFor(type).Enqueue(bullet);
            if (type == BulletType.Player) _playerTotal++;
            else _enemyTotal++;
        }

        private Queue<BulletBase> QueueFor(BulletType type)
        {
            return type == BulletType.Player ? _playerInactive : _enemyInactive;
        }

        private int TotalFor(BulletType type)
        {
            return type == BulletType.Player ? _playerTotal : _enemyTotal;
        }
    }
}

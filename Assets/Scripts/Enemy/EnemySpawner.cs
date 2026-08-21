using System;
using System.Collections.Generic;
using UnityEngine;
using SpaceShooter.Utilities;

namespace SpaceShooter.Enemy
{
    public enum EnemyType
    {
        Basic,
        Fast,
        Tank
    }

    /// <summary>
    /// Spawns standard enemies at the top of the screen at random X positions,
    /// using object pooling per enemy type. Tracks the number of live enemies.
    /// </summary>
    public class EnemySpawner : MonoBehaviour
    {
        [Header("Enemy Prefabs")]
        [SerializeField] private EnemyBase basicPrefab;
        [SerializeField] private EnemyBase fastPrefab;
        [SerializeField] private EnemyBase tankPrefab;

        [Header("Pool Settings")]
        [SerializeField] private int poolSizePerType = 20;
        [SerializeField] private float spawnMargin = 1f;
        [SerializeField] private float spawnYOffset = 1.5f;

        private readonly Dictionary<EnemyType, ObjectPool<EnemyBase>> _pools =
            new Dictionary<EnemyType, ObjectPool<EnemyBase>>();

        private int _aliveCount;
        public int AliveCount => _aliveCount;

        public event Action OnAllEnemiesCleared;

        private Transform _poolParent;

        private void Awake()
        {
            _poolParent = new GameObject("EnemyPool").transform;
            _poolParent.SetParent(transform);

            if (basicPrefab != null)
                _pools[EnemyType.Basic] = new ObjectPool<EnemyBase>(basicPrefab, poolSizePerType, _poolParent);
            if (fastPrefab != null)
                _pools[EnemyType.Fast] = new ObjectPool<EnemyBase>(fastPrefab, poolSizePerType, _poolParent);
            if (tankPrefab != null)
                _pools[EnemyType.Tank] = new ObjectPool<EnemyBase>(tankPrefab, poolSizePerType, _poolParent);
        }

        /// <summary>Spawns one enemy of the given type at a random X at the top edge.</summary>
        public EnemyBase SpawnEnemy(EnemyType type, float speedMultiplier)
        {
            if (!_pools.TryGetValue(type, out ObjectPool<EnemyBase> pool) || pool == null)
            {
                return null;
            }

            Vector3 spawnPos = GetRandomTopPosition();
            EnemyBase enemy = pool.Get(spawnPos, Quaternion.identity);

            // Apply per-wave speed scaling to movement.
            var movement = enemy.GetComponent<EnemyMovement>();
            if (movement != null)
            {
                movement.SetSpeedMultiplier(speedMultiplier);
            }

            _aliveCount++;
            enemy.OnEnemyDied += HandleEnemyDied;
            return enemy;
        }

        private void HandleEnemyDied(EnemyBase enemy)
        {
            _aliveCount = Mathf.Max(0, _aliveCount - 1);
            if (_aliveCount == 0)
            {
                OnAllEnemiesCleared?.Invoke();
            }
        }

        private Vector3 GetRandomTopPosition()
        {
            float x = 0f;
            float y = 6f;
            if (ScreenBounds.Instance != null)
            {
                x = UnityEngine.Random.Range(
                    ScreenBounds.Instance.MinX + spawnMargin,
                    ScreenBounds.Instance.MaxX - spawnMargin);
                y = ScreenBounds.Instance.MaxY + spawnYOffset;
            }
            return new Vector3(x, y, 0f);
        }

        /// <summary>Returns all live enemies to their pools (used on reset / game over).</summary>
        public void ClearAll()
        {
            foreach (var pool in _pools.Values)
            {
                pool.ReturnAll();
            }
            _aliveCount = 0;
        }
    }
}

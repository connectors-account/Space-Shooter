using System.Collections.Generic;
using UnityEngine;
using SpaceShooter.Core;
using SpaceShooter.Utilities;

namespace SpaceShooter.Enemy
{
    /// <summary>
    /// Spawns enemies from the object pool at the top of the screen at a random
    /// X within the horizontal bounds. Tracks the enemies it has spawned so the
    /// WaveManager can tell when a wave is cleared. Called by the WaveManager.
    /// </summary>
    public class EnemySpawner : MonoBehaviour
    {
        [SerializeField] private float horizontalPadding = 0.8f;
        [SerializeField] private float spawnHeightOffset = 1.5f;

        private Camera _camera;
        private Transform _player;

        /// <summary>Currently alive enemies spawned by this spawner.</summary>
        private readonly HashSet<EnemyBase> _alive = new HashSet<EnemyBase>();

        public int AliveCount => _alive.Count;

        private void Awake()
        {
            _camera = Camera.main;
            if (_camera == null) _camera = FindObjectOfType<Camera>();
        }

        public void SetPlayer(Transform player) => _player = player;

        private Transform ResolvePlayer()
        {
            if (_player != null) return _player;
            var go = GameObject.FindGameObjectWithTag(Constants.TagPlayer);
            if (go != null) _player = go.transform;
            return _player;
        }

        private string PoolKeyFor(EnemyType type)
        {
            switch (type)
            {
                case EnemyType.Drone:   return Constants.PoolEnemyDrone;
                case EnemyType.Fighter: return Constants.PoolEnemyFighter;
                case EnemyType.Bomber:  return Constants.PoolEnemyBomber;
                case EnemyType.Boss:    return Constants.PoolEnemyBoss;
                default:                return Constants.PoolEnemyDrone;
            }
        }

        private Vector3 RandomTopPosition()
        {
            float x = 0f, topY = 5f;
            if (_camera != null && _camera.orthographic)
            {
                float halfH = _camera.orthographicSize;
                float halfW = halfH * _camera.aspect;
                Vector3 c = _camera.transform.position;
                x = Random.Range(c.x - halfW + horizontalPadding, c.x + halfW - horizontalPadding);
                topY = c.y + halfH + spawnHeightOffset;
            }
            return new Vector3(x, topY, 0f);
        }

        /// <summary>Spawn a single enemy of the given type. Returns the spawned enemy.</summary>
        public EnemyBase Spawn(EnemyType type, float difficultyMultiplier)
        {
            if (ObjectPool.Instance == null) return null;

            string key = PoolKeyFor(type);
            Vector3 pos = type == EnemyType.Boss
                ? new Vector3(_camera != null ? _camera.transform.position.x : 0f,
                              _camera != null ? _camera.transform.position.y + _camera.orthographicSize + 2f : 7f, 0f)
                : RandomTopPosition();

            var go = ObjectPool.Instance.Acquire(key, pos, Quaternion.identity);
            if (go == null) return null;

            var enemy = go.GetComponent<EnemyBase>();
            if (enemy == null) return null;

            enemy.Initialise(key, ResolvePlayer(), difficultyMultiplier);

            // Track lifecycle.
            _alive.Add(enemy);
            enemy.OnEnemyDied -= HandleEnemyDied; // avoid double subscription
            enemy.OnEnemyDied += HandleEnemyDied;

            return enemy;
        }

        private void HandleEnemyDied(EnemyBase enemy)
        {
            _alive.Remove(enemy);
            enemy.OnEnemyDied -= HandleEnemyDied;
        }

        public void ClearTracking()
        {
            foreach (var e in _alive)
                if (e != null) e.OnEnemyDied -= HandleEnemyDied;
            _alive.Clear();
        }
    }
}

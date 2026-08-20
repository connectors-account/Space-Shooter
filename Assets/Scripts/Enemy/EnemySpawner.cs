using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpaceShooter.Utilities;

namespace SpaceShooter.Enemy
{
    public enum EnemyKind { TypeA, TypeB, Boss }

    /// <summary>
    /// Spawns enemies for a wave. Prefabs are assigned by the setup/editor script or wired in
    /// the inspector. Tracks live enemies and raises an event when the wave is fully cleared.
    /// </summary>
    public class EnemySpawner : MonoBehaviour
    {
        [Header("Prefabs")]
        public GameObject enemyTypeAPrefab;
        public GameObject enemyTypeBPrefab;
        public GameObject bossPrefab;
        public GameObject enemyBulletPrefab;
        public GameObject[] powerUpPrefabs;

        [Header("Spawn")]
        public float topPadding = 1f;
        public float defaultSpawnDelay = 0.6f;

        public int ActiveCount => _active.Count;
        public event Action OnAllEnemiesCleared;

        private readonly List<EnemyBase> _active = new List<EnemyBase>();
        private Coroutine _spawnRoutine;
        private bool _spawning;

        /// <summary>Spawn a wave from a WaveData definition. Fires OnAllEnemiesCleared when done.</summary>
        public void SpawnWave(WaveData wave)
        {
            if (_spawnRoutine != null) StopCoroutine(_spawnRoutine);
            _spawnRoutine = StartCoroutine(SpawnRoutine(wave));
        }

        private IEnumerator SpawnRoutine(WaveData wave)
        {
            _spawning = true;
            float delay = wave.spawnInterval > 0 ? wave.spawnInterval : defaultSpawnDelay;

            if (wave.isBossWave)
            {
                SpawnEnemy(EnemyKind.Boss, new Vector3(0f, TopY() + 1f, 0f), wave.finalBoss);
            }
            else
            {
                for (int i = 0; i < wave.enemyCount; i++)
                {
                    EnemyKind kind = wave.enemyTypes[i % wave.enemyTypes.Length];
                    SpawnEnemy(kind, RandomTopPosition(), false);
                    yield return new WaitForSeconds(delay);
                }
            }

            _spawning = false;
            // If everything was instantly cleared (edge case), check now.
            CheckCleared();
        }

        private float TopY()
        {
            return ScreenBounds.Instance != null
                ? ScreenBounds.Instance.Top - topPadding
                : 4f;
        }

        private Vector3 RandomTopPosition()
        {
            float left = ScreenBounds.Instance != null ? ScreenBounds.Instance.Left + 1f : -7f;
            float right = ScreenBounds.Instance != null ? ScreenBounds.Instance.Right - 1f : 7f;
            float x = UnityEngine.Random.Range(left, right);
            return new Vector3(x, TopY(), 0f);
        }

        private void SpawnEnemy(EnemyKind kind, Vector3 pos, bool finalBoss)
        {
            GameObject prefab = kind switch
            {
                EnemyKind.TypeA => enemyTypeAPrefab,
                EnemyKind.TypeB => enemyTypeBPrefab,
                EnemyKind.Boss => bossPrefab,
                _ => enemyTypeAPrefab
            };
            if (prefab == null)
            {
                Debug.LogWarning($"[EnemySpawner] Missing prefab for {kind}");
                return;
            }

            var go = Instantiate(prefab, pos, Quaternion.identity);
            var enemy = go.GetComponent<EnemyBase>();
            if (enemy == null) return;

            enemy.enemyBulletPrefab = enemyBulletPrefab;
            enemy.powerUpPrefabs = powerUpPrefabs;
            if (kind == EnemyKind.Boss && enemy is BossEnemy boss)
                boss.finalBoss = finalBoss;

            enemy.OnDeath += HandleEnemyDeath;
            _active.Add(enemy);
        }

        private void HandleEnemyDeath(EnemyBase enemy)
        {
            _active.Remove(enemy);
            CheckCleared();
        }

        private void CheckCleared()
        {
            if (!_spawning && _active.Count == 0)
            {
                OnAllEnemiesCleared?.Invoke();
            }
        }

        public void ClearAll()
        {
            foreach (var e in new List<EnemyBase>(_active))
            {
                if (e != null) Destroy(e.gameObject);
            }
            _active.Clear();
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpaceShooter.Data;
using SpaceShooter.Enemy;
using SpaceShooter.Utilities;

namespace SpaceShooter.Spawning
{
    /// <summary>
    /// Spawns the enemies described by a WaveData asset in the requested formation,
    /// tracks how many are alive and raises OnWaveCleared when the last one dies.
    /// </summary>
    public class EnemySpawner : Singleton<EnemySpawner>
    {
        [Header("Prefabs")]
        [Tooltip("Prefab with a StandardEnemy (or derived) component for regular enemies.")]
        [SerializeField] private GameObject enemyPrefab;
        [Tooltip("Prefab with a BossEnemy component.")]
        [SerializeField] private GameObject bossPrefab;

        [Header("Spawn Area")]
        [SerializeField] private float horizontalPadding = 1f;
        [SerializeField] private float spawnHeightOffset = 1.5f;

        private int _activeEnemies;
        private bool _spawningComplete;
        private Camera _camera;

        /// <summary>Raised when every enemy from the current wave has been destroyed.</summary>
        public event Action OnWaveCleared;

        public int ActiveEnemies => _activeEnemies;

        protected override bool PersistAcrossScenes => false;

        protected override void OnAwakeInitialize()
        {
            _camera = Camera.main;
        }

        /// <summary>
        /// Begins spawning a wave. Returns the coroutine so callers can track it if desired.
        /// </summary>
        public void SpawnWave(WaveData wave)
        {
            if (wave == null)
            {
                Debug.LogError("EnemySpawner.SpawnWave called with null WaveData.");
                return;
            }

            StopAllCoroutines();
            _activeEnemies = 0;
            _spawningComplete = false;
            StartCoroutine(SpawnWaveRoutine(wave));
        }

        /// <summary>
        /// Spawns a procedurally scaled wave for infinite mode.
        /// </summary>
        public void SpawnInfiniteWave(EnemyData enemyData, int count, float difficultyMultiplier)
        {
            StopAllCoroutines();
            _activeEnemies = 0;
            _spawningComplete = false;
            StartCoroutine(SpawnInfiniteRoutine(enemyData, count, difficultyMultiplier));
        }

        private IEnumerator SpawnInfiniteRoutine(EnemyData enemyData, int count, float difficultyMultiplier)
        {
            for (int i = 0; i < count; i++)
            {
                Vector3 pos = GetFormationPosition(Formation.Random, i, count);
                SpawnEnemy(enemyData, pos, MovementPattern.SineWave);
                yield return new WaitForSeconds(Mathf.Max(0.15f, 0.5f / difficultyMultiplier));
            }
            _spawningComplete = true;
            CheckWaveCleared();
        }

        private IEnumerator SpawnWaveRoutine(WaveData wave)
        {
            foreach (EnemySpawnEntry entry in wave.enemies)
            {
                if (entry == null || entry.enemyData == null)
                {
                    continue;
                }

                for (int i = 0; i < entry.count; i++)
                {
                    Vector3 pos = GetFormationPosition(entry.formation, i, entry.count);
                    MovementPattern pattern = PatternForFormation(entry.formation);
                    SpawnEnemy(entry.enemyData, pos, pattern);
                    yield return new WaitForSeconds(entry.spawnInterval);
                }
            }

            // Spawn the boss last, if present.
            if (wave.hasBoss && wave.bossData != null)
            {
                yield return new WaitForSeconds(1f);
                SpawnBoss(wave.bossData);
            }

            _spawningComplete = true;
            CheckWaveCleared();
        }

        private void SpawnEnemy(EnemyData data, Vector3 position, MovementPattern pattern)
        {
            if (enemyPrefab == null)
            {
                Debug.LogError("EnemySpawner: enemyPrefab is not assigned.");
                return;
            }

            GameObject go = Instantiate(enemyPrefab, position, Quaternion.identity);
            var enemy = go.GetComponent<EnemyBase>();
            if (enemy != null)
            {
                enemy.SetData(data);
                if (enemy is StandardEnemy standard)
                {
                    standard.SetMovementPattern(pattern);
                }
            }

            _activeEnemies++;
        }

        private void SpawnBoss(EnemyData bossData)
        {
            if (bossPrefab == null)
            {
                Debug.LogError("EnemySpawner: bossPrefab is not assigned.");
                return;
            }

            Vector3 topCenter = GetTopCenter();
            GameObject go = Instantiate(bossPrefab, topCenter, Quaternion.identity);
            var boss = go.GetComponent<EnemyBase>();
            if (boss != null)
            {
                boss.SetData(bossData);
            }

            _activeEnemies++;
        }

        /// <summary>
        /// Called by EnemyBase.OnDeath. Decrements the active count and checks for clear.
        /// </summary>
        public void EnemyDestroyed(EnemyBase enemy)
        {
            _activeEnemies = Mathf.Max(0, _activeEnemies - 1);
            CheckWaveCleared();
        }

        private void CheckWaveCleared()
        {
            if (_spawningComplete && _activeEnemies <= 0)
            {
                OnWaveCleared?.Invoke();
            }
        }

        // ------------------------------------------------------------------
        // Formation helpers
        // ------------------------------------------------------------------
        private Vector3 GetFormationPosition(Formation formation, int index, int total)
        {
            if (_camera == null)
            {
                _camera = Camera.main;
            }

            Vector3 leftTop = _camera != null ? _camera.ViewportToWorldPoint(new Vector3(0f, 1f, 0f)) : new Vector3(-8f, 5f, 0f);
            Vector3 rightTop = _camera != null ? _camera.ViewportToWorldPoint(new Vector3(1f, 1f, 0f)) : new Vector3(8f, 5f, 0f);

            float minX = leftTop.x + horizontalPadding;
            float maxX = rightTop.x - horizontalPadding;
            float topY = leftTop.y + spawnHeightOffset;

            switch (formation)
            {
                case Formation.Line:
                {
                    float tX = total > 1 ? (float)index / (total - 1) : 0.5f;
                    return new Vector3(Mathf.Lerp(minX, maxX, tX), topY, 0f);
                }

                case Formation.VShape:
                {
                    float center = (minX + maxX) * 0.5f;
                    int half = total / 2;
                    int offset = index - half;
                    float x = center + offset * ((maxX - minX) / Mathf.Max(1, total)) * 0.5f;
                    float y = topY + Mathf.Abs(offset) * 0.5f;
                    return new Vector3(Mathf.Clamp(x, minX, maxX), y, 0f);
                }

                case Formation.Flanks:
                {
                    // Alternate hard left / hard right.
                    bool left = index % 2 == 0;
                    float x = left ? minX + 0.5f : maxX - 0.5f;
                    float y = topY + (index / 2) * 0.6f;
                    return new Vector3(x, y, 0f);
                }

                case Formation.Random:
                default:
                    return new Vector3(UnityEngine.Random.Range(minX, maxX), topY + UnityEngine.Random.Range(0f, 2f), 0f);
            }
        }

        private MovementPattern PatternForFormation(Formation formation)
        {
            switch (formation)
            {
                case Formation.Line: return MovementPattern.StraightDown;
                case Formation.VShape: return MovementPattern.SineWave;
                case Formation.Flanks: return MovementPattern.ZigZag;
                case Formation.Random: return MovementPattern.SineWave;
                default: return MovementPattern.StraightDown;
            }
        }

        private Vector3 GetTopCenter()
        {
            if (_camera == null)
            {
                _camera = Camera.main;
            }
            Vector3 top = _camera != null ? _camera.ViewportToWorldPoint(new Vector3(0.5f, 1f, 0f)) : new Vector3(0f, 6f, 0f);
            top.y += spawnHeightOffset;
            top.z = 0f;
            return top;
        }
    }
}

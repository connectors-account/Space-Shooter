// ============================================================================
// EnemySpawner.cs — Wave-based enemy spawning system
// Manages progressive difficulty: each wave adds more enemies, faster spawns,
// and introduces tougher enemy types.
// ============================================================================
using System.Collections;
using UnityEngine;
using SpaceShooter.Core;

namespace SpaceShooter.Enemies
{
    [System.Serializable]
    public class EnemyWeight
    {
        public string poolTag;            // must match a tag in ObjectPool
        public int minWave = 1;           // wave at which this type starts appearing
        [Range(0f, 1f)] public float weight = 0.25f;
    }

    public class EnemySpawner : MonoBehaviour
    {
        [Header("Spawn Settings")]
        [SerializeField] private float initialSpawnInterval = 2f;
        [SerializeField] private float minimumSpawnInterval = 0.4f;
        [SerializeField] private float spawnIntervalDecay = 0.05f;  // per wave
        [SerializeField] private int enemiesPerWaveBase = 6;
        [SerializeField] private int enemiesPerWaveGrowth = 2;
        [SerializeField] private float timeBetweenWaves = 3f;

        [Header("Enemy Types")]
        [SerializeField] private EnemyWeight[] enemyTypes;

        private float _currentSpawnInterval;
        private bool _spawning;

        private void Start()
        {
            // Subscribe to game state so we start/stop spawning correctly
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameStateChanged += HandleStateChange;

                // If we loaded directly into the gameplay scene while state is Playing
                if (GameManager.Instance.CurrentState == GameState.Playing)
                    BeginSpawning();
            }
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.OnGameStateChanged -= HandleStateChange;
        }

        private void HandleStateChange(GameState state)
        {
            if (state == GameState.Playing && !_spawning)
                BeginSpawning();
            else if (state == GameState.GameOver)
                StopAllCoroutines();
        }

        private void BeginSpawning()
        {
            _spawning = true;
            _currentSpawnInterval = initialSpawnInterval;
            StartCoroutine(WaveLoop());
        }

        // ====================================================================
        // Wave loop
        // ====================================================================
        private IEnumerator WaveLoop()
        {
            while (true)
            {
                GameManager.Instance?.AdvanceWave();
                int wave = GameManager.Instance != null ? GameManager.Instance.CurrentWave : 1;
                int enemyCount = enemiesPerWaveBase + (wave - 1) * enemiesPerWaveGrowth;

                // Spawn enemies for this wave
                for (int i = 0; i < enemyCount; i++)
                {
                    SpawnRandomEnemy(wave);
                    yield return new WaitForSeconds(_currentSpawnInterval);
                }

                // Decrease interval for next wave
                _currentSpawnInterval = Mathf.Max(minimumSpawnInterval,
                    _currentSpawnInterval - spawnIntervalDecay);

                // Brief pause between waves
                yield return new WaitForSeconds(timeBetweenWaves);
            }
        }

        // ====================================================================
        // Spawn helpers
        // ====================================================================
        private void SpawnRandomEnemy(int wave)
        {
            if (ObjectPool.Instance == null || enemyTypes == null || enemyTypes.Length == 0) return;

            // Build a weighted list of enemies available at the current wave
            float totalWeight = 0f;
            foreach (var et in enemyTypes)
            {
                if (wave >= et.minWave) totalWeight += et.weight;
            }

            if (totalWeight <= 0f) return;

            float roll = Random.value * totalWeight;
            float cumulative = 0f;
            string chosenTag = enemyTypes[0].poolTag;

            foreach (var et in enemyTypes)
            {
                if (wave < et.minWave) continue;
                cumulative += et.weight;
                if (roll <= cumulative)
                {
                    chosenTag = et.poolTag;
                    break;
                }
            }

            // Random X within screen bounds
            float spawnX = 0f;
            if (GameBounds.Instance != null)
            {
                Camera cam = Camera.main;
                float halfWidth = cam.orthographicSize * cam.aspect;
                spawnX = Random.Range(-halfWidth + 1f, halfWidth - 1f);
            }

            float spawnY = GameBounds.Instance != null ? GameBounds.Instance.Top : 7f;
            Vector3 pos = new Vector3(spawnX, spawnY, 0f);

            ObjectPool.Instance.Get(chosenTag, pos, Quaternion.identity);
        }
    }
}

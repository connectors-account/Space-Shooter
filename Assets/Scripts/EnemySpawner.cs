using System.Collections;
using UnityEngine;

namespace SpaceShooter
{
    /// <summary>
    /// Spawns enemies for each wave with increasing intensity.
    /// </summary>
    public class EnemySpawner : MonoBehaviour
    {
        [System.Serializable]
        public class EnemySpawnEntry
        {
            public EnemyController prefab;
            [Range(1, 100)] public int weight = 10;
        }

        [Header("Spawn Catalog")]
        [SerializeField] private EnemySpawnEntry[] enemyEntries;

        [Header("Spawn Area")]
        [SerializeField] private float spawnTopY = 6.5f;
        [SerializeField] private float minSpawnX = -8.2f;
        [SerializeField] private float maxSpawnX = 8.2f;

        [Header("Wave Timing")]
        [SerializeField] private float baseSpawnInterval = 1.2f;
        [SerializeField] private float minimumSpawnInterval = 0.35f;
        [SerializeField] private float spawnIntervalReductionPerWave = 0.08f;

        private Coroutine spawnRoutine;

        public bool IsWaveSpawning { get; private set; }

        public void BeginWave(int waveNumber, int enemyCount)
        {
            StopWaveSpawning();
            spawnRoutine = StartCoroutine(SpawnWaveRoutine(waveNumber, enemyCount));
        }

        public void StopWaveSpawning()
        {
            if (spawnRoutine != null)
            {
                StopCoroutine(spawnRoutine);
                spawnRoutine = null;
            }

            IsWaveSpawning = false;
        }

        private IEnumerator SpawnWaveRoutine(int waveNumber, int enemyCount)
        {
            IsWaveSpawning = true;
            float currentInterval = Mathf.Max(minimumSpawnInterval, baseSpawnInterval - (waveNumber - 1) * spawnIntervalReductionPerWave);

            for (int i = 0; i < enemyCount; i++)
            {
                if (GameManager.Instance == null || !GameManager.Instance.IsGameplayActive)
                {
                    break;
                }

                SpawnEnemy(waveNumber);
                yield return new WaitForSeconds(currentInterval);
            }

            IsWaveSpawning = false;
        }

        private void SpawnEnemy(int waveNumber)
        {
            EnemyController enemyPrefab = GetWeightedEnemyPrefab();
            if (enemyPrefab == null)
            {
                Debug.LogError("EnemySpawner has no valid enemy prefab assigned.");
                return;
            }

            Vector3 spawnPosition = new Vector3(Random.Range(minSpawnX, maxSpawnX), spawnTopY, 0f);
            EnemyController enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
            enemy.ConfigureFromWave(waveNumber);

            GameManager.Instance?.RegisterEnemySpawned();
        }

        private EnemyController GetWeightedEnemyPrefab()
        {
            if (enemyEntries == null || enemyEntries.Length == 0)
            {
                return null;
            }

            int totalWeight = 0;
            foreach (EnemySpawnEntry entry in enemyEntries)
            {
                if (entry.prefab != null)
                {
                    totalWeight += Mathf.Max(1, entry.weight);
                }
            }

            if (totalWeight == 0)
            {
                return null;
            }

            int roll = Random.Range(0, totalWeight);
            int cumulative = 0;

            foreach (EnemySpawnEntry entry in enemyEntries)
            {
                if (entry.prefab == null)
                {
                    continue;
                }

                cumulative += Mathf.Max(1, entry.weight);
                if (roll < cumulative)
                {
                    return entry.prefab;
                }
            }

            return enemyEntries[0].prefab;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceShooter.Managers
{
    /// <summary>
    /// Spawns enemies by wave and tracks active spawned entities.
    /// </summary>
    public class SpawnManager : MonoBehaviour
    {
        [Header("Enemy Prefabs")]
        [SerializeField] private GameObject basicEnemyPrefab;
        [SerializeField] private GameObject zigzagEnemyPrefab;
        [SerializeField] private GameObject tankEnemyPrefab;

        [Header("Spawn Bounds")]
        [SerializeField] private float spawnY = 6.5f;
        [SerializeField] private float spawnXMin = -8.5f;
        [SerializeField] private float spawnXMax = 8.5f;

        [Header("Spawn Timing")]
        [SerializeField] private float waveStartDelay = 1.0f;
        [SerializeField] private float baseSpawnInterval = 1.3f;
        [SerializeField] private float minSpawnInterval = 0.45f;
        [SerializeField] private float spawnIntervalReductionPerWave = 0.08f;

        private readonly List<GameObject> activeEnemies = new List<GameObject>();

        private int currentWave;
        private int enemiesToSpawn;
        private int spawnedCount;
        private Coroutine spawnRoutine;

        public int ActiveEnemyCount
        {
            get
            {
                activeEnemies.RemoveAll(item => item == null);
                return activeEnemies.Count;
            }
        }

        public void StartWave(int wave, int totalEnemies)
        {
            currentWave = wave;
            enemiesToSpawn = Mathf.Max(0, totalEnemies);
            spawnedCount = 0;

            if (spawnRoutine != null)
            {
                StopCoroutine(spawnRoutine);
            }

            spawnRoutine = StartCoroutine(SpawnWaveRoutine());
        }

        public void StopSpawning()
        {
            if (spawnRoutine != null)
            {
                StopCoroutine(spawnRoutine);
                spawnRoutine = null;
            }
        }

        public void ClearAllEnemies()
        {
            StopSpawning();

            for (int i = 0; i < activeEnemies.Count; i++)
            {
                if (activeEnemies[i] != null)
                {
                    Destroy(activeEnemies[i]);
                }
            }

            activeEnemies.Clear();
            DestroyTaggedObjects("EnemyBullet");
            DestroyTaggedObjects("PlayerBullet");
            DestroyTaggedObjects("PowerUp");
        }

        private IEnumerator SpawnWaveRoutine()
        {
            yield return new WaitForSeconds(waveStartDelay);

            float interval = Mathf.Max(minSpawnInterval, baseSpawnInterval - (currentWave - 1) * spawnIntervalReductionPerWave);
            while (spawnedCount < enemiesToSpawn)
            {
                SpawnSingleEnemy();
                spawnedCount++;
                yield return new WaitForSeconds(interval);
            }

            spawnRoutine = null;
        }

        private void SpawnSingleEnemy()
        {
            GameObject prefab = PickEnemyPrefabForWave();
            if (prefab == null)
            {
                return;
            }

            float x = Random.Range(spawnXMin, spawnXMax);
            Vector3 spawnPosition = new Vector3(x, spawnY, 0f);
            GameObject enemy = Instantiate(prefab, spawnPosition, Quaternion.identity);
            enemy.tag = "Enemy";
            activeEnemies.Add(enemy);

            Enemy.EnemyController enemyController = enemy.GetComponent<Enemy.EnemyController>();
            if (enemyController != null)
            {
                enemyController.OnEnemyDestroyed += points =>
                {
                    activeEnemies.Remove(enemy);
                    GameManager.Instance?.HandleEnemyDestroyed(points);
                    GameManager.Instance?.OnEnemyKilledInWave();
                };
            }
        }

        private GameObject PickEnemyPrefabForWave()
        {
            float roll = Random.value;

            if (currentWave <= 2)
            {
                if (roll < 0.75f) return basicEnemyPrefab;
                return zigzagEnemyPrefab != null ? zigzagEnemyPrefab : basicEnemyPrefab;
            }

            if (currentWave <= 5)
            {
                if (roll < 0.45f) return basicEnemyPrefab;
                if (roll < 0.82f) return zigzagEnemyPrefab != null ? zigzagEnemyPrefab : basicEnemyPrefab;
                return tankEnemyPrefab != null ? tankEnemyPrefab : basicEnemyPrefab;
            }

            if (roll < 0.30f) return basicEnemyPrefab;
            if (roll < 0.65f) return zigzagEnemyPrefab != null ? zigzagEnemyPrefab : basicEnemyPrefab;
            return tankEnemyPrefab != null ? tankEnemyPrefab : basicEnemyPrefab;
        }

        private static void DestroyTaggedObjects(string tagName)
        {
            GameObject[] objects = GameObject.FindGameObjectsWithTag(tagName);
            for (int i = 0; i < objects.Length; i++)
            {
                Destroy(objects[i]);
            }
        }
    }
}

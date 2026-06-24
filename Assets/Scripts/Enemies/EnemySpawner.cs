using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceShooter
{
    /// <summary>
    /// Wave-based enemy spawner with difficulty progression.
    /// Each wave spawns an increasing number of enemies, with stronger enemy
    /// types becoming more common and spawn intervals shrinking over time.
    /// The next wave starts once all enemies of the current wave are cleared.
    /// </summary>
    public class EnemySpawner : MonoBehaviour
    {
        [Header("Enemy Prefabs (index 0=Straight,1=Zigzag,2=Chaser,3=Shooter)")]
        [SerializeField] private GameObject[] enemyPrefabs;

        [Header("Spawn Area")]
        [SerializeField] private float horizontalPadding = 0.8f;
        [SerializeField] private float spawnHeightOffset = 1.5f;

        [Header("Wave Settings")]
        [SerializeField] private int baseEnemiesPerWave = 5;
        [SerializeField] private int enemiesIncrementPerWave = 2;
        [SerializeField] private float baseSpawnInterval = 1.2f;
        [SerializeField] private float minSpawnInterval = 0.35f;
        [SerializeField] private float timeBetweenWaves = 3f;

        private int currentWave = 0;
        private int enemiesAlive = 0;
        private int enemiesToSpawn = 0;
        private bool spawning = false;
        private Camera cam;
        private float leftX, rightX, topY;

        public int CurrentWave => currentWave;

        private void Start()
        {
            cam = Camera.main;
            CalculateSpawnBounds();
        }

        private void CalculateSpawnBounds()
        {
            if (cam == null) cam = Camera.main;
            if (cam == null) return;

            Vector3 min = cam.ViewportToWorldPoint(new Vector3(0f, 0f, 0f));
            Vector3 max = cam.ViewportToWorldPoint(new Vector3(1f, 1f, 0f));
            leftX = min.x + horizontalPadding;
            rightX = max.x - horizontalPadding;
            topY = max.y + spawnHeightOffset;
        }

        public void StartSpawning()
        {
            StopAllCoroutines();
            currentWave = 0;
            enemiesAlive = 0;
            StartCoroutine(WaveRoutine());
        }

        public void StopSpawning()
        {
            StopAllCoroutines();
            spawning = false;
        }

        private IEnumerator WaveRoutine()
        {
            while (true)
            {
                currentWave++;
                GameManager.Instance?.SetWave(currentWave);
                UIManager.Instance?.ShowWaveBanner(currentWave);
                AudioManager.Instance?.PlayWaveStart();

                yield return new WaitForSeconds(timeBetweenWaves);

                enemiesToSpawn = baseEnemiesPerWave + (currentWave - 1) * enemiesIncrementPerWave;
                enemiesAlive = enemiesToSpawn;
                float interval = Mathf.Max(minSpawnInterval, baseSpawnInterval - (currentWave - 1) * 0.08f);

                spawning = true;
                for (int i = 0; i < enemiesToSpawn; i++)
                {
                    SpawnEnemy();
                    yield return new WaitForSeconds(interval);
                }
                spawning = false;

                // Wait until the player clears the wave before continuing.
                yield return new WaitUntil(() => enemiesAlive <= 0);
            }
        }

        private void SpawnEnemy()
        {
            if (enemyPrefabs == null || enemyPrefabs.Length == 0) return;

            GameObject prefab = PickEnemyForWave();
            if (prefab == null) return;

            float x = Random.Range(leftX, rightX);
            Vector3 spawnPos = new Vector3(x, topY, 0f);

            GameObject enemy = Instantiate(prefab, spawnPos, Quaternion.identity);

            // Track lifetime so we know when the wave is cleared.
            EnemyDeathNotifier notifier = enemy.AddComponent<EnemyDeathNotifier>();
            notifier.Init(this);
        }

        /// <summary>
        /// Weighted enemy selection. Early waves are mostly simple "Straight"
        /// enemies; tougher types appear as the wave number rises.
        /// </summary>
        private GameObject PickEnemyForWave()
        {
            List<int> pool = new List<int>();

            // Straight always available.
            AddToPool(pool, 0, 4);

            if (currentWave >= 2) AddToPool(pool, 1, 1 + currentWave / 2); // Zigzag
            if (currentWave >= 3) AddToPool(pool, 3, currentWave / 2);     // Shooter
            if (currentWave >= 4) AddToPool(pool, 2, currentWave / 3);     // Chaser

            int chosenIndex = pool[Random.Range(0, pool.Count)];
            chosenIndex = Mathf.Clamp(chosenIndex, 0, enemyPrefabs.Length - 1);
            return enemyPrefabs[chosenIndex];
        }

        private void AddToPool(List<int> pool, int index, int weight)
        {
            if (index >= enemyPrefabs.Length) return;
            for (int i = 0; i < weight; i++) pool.Add(index);
        }

        public void NotifyEnemyDestroyed()
        {
            enemiesAlive = Mathf.Max(0, enemiesAlive - 1);
        }
    }

    /// <summary>
    /// Tiny helper auto-added to each spawned enemy. Notifies the spawner when
    /// the enemy is destroyed so wave completion can be detected.
    /// </summary>
    public class EnemyDeathNotifier : MonoBehaviour
    {
        private EnemySpawner spawner;
        private bool notified = false;

        public void Init(EnemySpawner s)
        {
            spawner = s;
        }

        private void OnDestroy()
        {
            if (notified) return;
            notified = true;
            if (spawner != null) spawner.NotifyEnemyDestroyed();
        }
    }
}

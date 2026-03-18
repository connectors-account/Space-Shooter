using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace SpaceShooter.Managers
{
    /// <summary>
    /// Manages enemy spawning in waves with increasing difficulty.
    /// Spawns enemies at random positions above the screen.
    /// </summary>
    public class SpawnManager : MonoBehaviour
    {
        [Header("Enemy Prefabs")]
        [SerializeField] private GameObject basicEnemyPrefab;
        [SerializeField] private GameObject zigzagEnemyPrefab;
        [SerializeField] private GameObject tankEnemyPrefab;

        [Header("Spawn Settings")]
        [SerializeField] private float spawnYPosition = 6.5f;
        [SerializeField] private float spawnXMin = -8f;
        [SerializeField] private float spawnXMax = 8f;
        [SerializeField] private float baseSpawnInterval = 1.5f;
        [SerializeField] private float minSpawnInterval = 0.4f;
        [SerializeField] private float spawnIntervalReduction = 0.1f; // per wave

        // ---- Runtime State ----
        private int currentWave;
        private int enemiesToSpawn;
        private int enemiesSpawned;
        private bool isSpawning;
        private Coroutine spawnCoroutine;

        // ---- Active Enemies Tracking ----
        private List<GameObject> activeEnemies = new List<GameObject>();

        public int ActiveEnemyCount
        {
            get
            {
                // Clean null references
                activeEnemies.RemoveAll(e => e == null);
                return activeEnemies.Count;
            }
        }

        /// <summary>
        /// Begins spawning enemies for a new wave.
        /// Called by GameManager.
        /// </summary>
        public void StartWave(int wave, int totalEnemies)
        {
            currentWave = wave;
            enemiesToSpawn = totalEnemies;
            enemiesSpawned = 0;
            isSpawning = true;

            if (spawnCoroutine != null)
                StopCoroutine(spawnCoroutine);

            spawnCoroutine = StartCoroutine(SpawnWaveRoutine());
        }

        /// <summary>Stops all spawning (e.g., on game over).</summary>
        public void StopSpawning()
        {
            isSpawning = false;
            if (spawnCoroutine != null)
            {
                StopCoroutine(spawnCoroutine);
                spawnCoroutine = null;
            }
        }

        /// <summary>Destroys all active enemies (e.g., on restart).</summary>
        public void ClearAllEnemies()
        {
            StopSpawning();

            foreach (GameObject enemy in activeEnemies)
            {
                if (enemy != null)
                    Destroy(enemy);
            }
            activeEnemies.Clear();

            // Also destroy any stray enemy bullets
            GameObject[] enemyBullets = GameObject.FindGameObjectsWithTag("EnemyBullet");
            foreach (GameObject bullet in enemyBullets)
            {
                Destroy(bullet);
            }

            // And player bullets
            GameObject[] playerBullets = GameObject.FindGameObjectsWithTag("PlayerBullet");
            foreach (GameObject bullet in playerBullets)
            {
                Destroy(bullet);
            }

            // And power-ups
            GameObject[] powerUps = GameObject.FindGameObjectsWithTag("PowerUp");
            foreach (GameObject pu in powerUps)
            {
                Destroy(pu);
            }
        }

        /// <summary>
        /// Main spawn coroutine. Spawns enemies with decreasing intervals.
        /// </summary>
        private IEnumerator SpawnWaveRoutine()
        {
            // Brief delay before wave starts
            yield return new WaitForSeconds(1.5f);

            float interval = Mathf.Max(
                baseSpawnInterval - (currentWave - 1) * spawnIntervalReduction,
                minSpawnInterval
            );

            while (enemiesSpawned < enemiesToSpawn && isSpawning)
            {
                SpawnEnemy();
                enemiesSpawned++;

                yield return new WaitForSeconds(interval);
            }

            isSpawning = false;
        }

        /// <summary>
        /// Spawns a single enemy based on wave-appropriate type distribution.
        /// </summary>
        private void SpawnEnemy()
        {
            // Determine enemy type based on wave and randomness
            GameObject prefab = ChooseEnemyPrefab();
            if (prefab == null) return;

            // Random x position
            float spawnX = Random.Range(spawnXMin, spawnXMax);
            Vector3 spawnPos = new Vector3(spawnX, spawnYPosition, 0f);

            GameObject enemy = Instantiate(prefab, spawnPos, Quaternion.identity);
            enemy.tag = "Enemy";

            // Track active enemies
            activeEnemies.Add(enemy);

            // Subscribe to enemy death for wave counting
            Enemy.EnemyController ec = enemy.GetComponent<Enemy.EnemyController>();
            if (ec != null)
            {
                ec.OnEnemyDestroyed += (score) =>
                {
                    activeEnemies.Remove(enemy);
                    GameManager.Instance?.OnEnemyKilled();
                };
            }
        }

        /// <summary>
        /// Chooses which enemy prefab to spawn based on current wave.
        /// Higher waves introduce tougher enemies.
        /// </summary>
        private GameObject ChooseEnemyPrefab()
        {
            float roll = Random.value;

            if (currentWave <= 2)
            {
                // Waves 1-2: Mostly basic enemies
                if (roll < 0.8f) return basicEnemyPrefab;
                return zigzagEnemyPrefab != null ? zigzagEnemyPrefab : basicEnemyPrefab;
            }
            else if (currentWave <= 4)
            {
                // Waves 3-4: Mix of basic and zigzag, rare tanks
                if (roll < 0.5f) return basicEnemyPrefab;
                if (roll < 0.85f) return zigzagEnemyPrefab != null ? zigzagEnemyPrefab : basicEnemyPrefab;
                return tankEnemyPrefab != null ? tankEnemyPrefab : basicEnemyPrefab;
            }
            else
            {
                // Wave 5+: All types with more tanks
                if (roll < 0.35f) return basicEnemyPrefab;
                if (roll < 0.65f) return zigzagEnemyPrefab != null ? zigzagEnemyPrefab : basicEnemyPrefab;
                return tankEnemyPrefab != null ? tankEnemyPrefab : basicEnemyPrefab;
            }
        }
    }
}

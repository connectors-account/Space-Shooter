using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceShooter.Managers
{
    /// <summary>
    /// Manages enemy wave spawning with increasing difficulty.
    /// Each wave has more enemies; enemy types are mixed in at higher waves.
    /// </summary>
    public class WaveSpawner : MonoBehaviour
    {
        [Header("Enemy Pool Tags")]
        [SerializeField] private string basicEnemyTag = "BasicEnemy";
        [SerializeField] private string fastEnemyTag = "FastEnemy";
        [SerializeField] private string tankEnemyTag = "TankEnemy";

        [Header("Spawn Settings")]
        [SerializeField] private float spawnYOffset = 1f;       // above screen top
        [SerializeField] private float timeBetweenSpawns = 0.8f;
        [SerializeField] private float timeBetweenWaves = 4f;
        [SerializeField] private int baseEnemiesPerWave = 5;
        [SerializeField] private int enemiesPerWaveIncrease = 2;

        // State
        private int currentWave;
        private bool isSpawning;
        private Coroutine spawnCoroutine;

        public int CurrentWave => currentWave;

        public void StartSpawning()
        {
            currentWave = 0;
            isSpawning = true;

            if (spawnCoroutine != null)
                StopCoroutine(spawnCoroutine);

            spawnCoroutine = StartCoroutine(SpawnWaves());
        }

        public void StopSpawning()
        {
            isSpawning = false;
            if (spawnCoroutine != null)
            {
                StopCoroutine(spawnCoroutine);
                spawnCoroutine = null;
            }
        }

        private IEnumerator SpawnWaves()
        {
            yield return new WaitForSeconds(1.5f); // initial delay

            while (isSpawning)
            {
                currentWave++;
                int enemyCount = baseEnemiesPerWave + (currentWave - 1) * enemiesPerWaveIncrease;

                GameManager.Instance?.OnWaveStarted(currentWave);

                // Spawn all enemies in this wave
                for (int i = 0; i < enemyCount; i++)
                {
                    if (!isSpawning) yield break;

                    SpawnEnemy();

                    // Decrease time between spawns as waves progress
                    float delay = Mathf.Max(0.3f, timeBetweenSpawns - currentWave * 0.03f);
                    yield return new WaitForSeconds(delay);
                }

                // Wait for wave cooldown
                yield return new WaitForSeconds(timeBetweenWaves);
            }
        }

        private void SpawnEnemy()
        {
            // Random spawn X position within screen bounds
            float screenEdge = Camera.main != null
                ? Camera.main.orthographicSize * Camera.main.aspect
                : 8f;
            float x = Random.Range(-screenEdge + 1f, screenEdge - 1f);

            float screenTop = Camera.main != null ? Camera.main.orthographicSize : 5f;
            Vector3 spawnPos = new Vector3(x, screenTop + spawnYOffset, 0f);

            // Choose enemy type based on wave number
            string enemyTag = ChooseEnemyType();

            GameObject enemy = ObjectPoolManager.Instance?.GetFromPool(enemyTag, spawnPos, Quaternion.identity);
            if (enemy != null)
            {
                Enemies.EnemyBase enemyComp = enemy.GetComponent<Enemies.EnemyBase>();
                if (enemyComp != null)
                {
                    enemyComp.ConfigureForWave(currentWave);
                }
            }
        }

        private string ChooseEnemyType()
        {
            float roll = Random.value;

            if (currentWave >= 5 && roll < 0.15f)
            {
                return tankEnemyTag;
            }
            else if (currentWave >= 3 && roll < 0.4f)
            {
                return fastEnemyTag;
            }
            else
            {
                return basicEnemyTag;
            }
        }
    }
}

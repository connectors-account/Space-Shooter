using System.Collections;
using SpaceShooter.Audio;
using SpaceShooter.Core;
using UnityEngine;

namespace SpaceShooter.Enemies
{
    public class EnemySpawner : MonoBehaviour
    {
        [System.Serializable]
        private class EnemySpawnEntry
        {
            public EnemyBase prefab;
            [Range(0f, 1f)] public float chance = 1f;
        }

        [Header("Waves")]
        [SerializeField] private EnemySpawnEntry[] enemyPrefabs;
        [SerializeField] private float baseSpawnInterval = 1.25f;
        [SerializeField] private int baseEnemiesPerWave = 8;
        [SerializeField] private float waveBreakDuration = 2.5f;

        [Header("Spawn Area")]
        [SerializeField] private float spawnY = 6f;
        [SerializeField] private float minX = -8f;
        [SerializeField] private float maxX = 8f;

        private int spawnedThisWave;
        private int enemiesRemaining;
        private bool waveRunning;

        private void Start()
        {
            StartCoroutine(WaveLoop());
        }

        private IEnumerator WaveLoop()
        {
            while (true)
            {
                if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Playing)
                {
                    yield return null;
                    continue;
                }

                if (!waveRunning)
                {
                    waveRunning = true;
                    spawnedThisWave = 0;
                    int targetWave = Mathf.Max(1, GameManager.Instance.Wave);
                    enemiesRemaining = baseEnemiesPerWave + ((targetWave - 1) * 3);
                    AudioManager.Instance?.PlayWaveStart();
                }

                if (spawnedThisWave < enemiesRemaining)
                {
                    SpawnRandomEnemy();
                    spawnedThisWave++;
                    float scaledInterval = Mathf.Max(0.35f, baseSpawnInterval - (0.06f * GameManager.Instance.Wave));
                    yield return new WaitForSeconds(scaledInterval);
                }
                else
                {
                    while (FindObjectsOfType<EnemyBase>().Length > 0)
                    {
                        yield return new WaitForSeconds(0.5f);
                    }

                    yield return new WaitForSeconds(waveBreakDuration);
                    GameManager.Instance.AdvanceWave();
                    waveRunning = false;
                }
            }
        }

        private void SpawnRandomEnemy()
        {
            EnemyBase prefab = GetWeightedEnemy();
            if (prefab == null)
            {
                return;
            }

            float spawnX = Random.Range(minX, maxX);
            Vector3 spawnPosition = new Vector3(spawnX, spawnY, 0f);
            Instantiate(prefab, spawnPosition, Quaternion.identity);
        }

        private EnemyBase GetWeightedEnemy()
        {
            if (enemyPrefabs == null || enemyPrefabs.Length == 0)
            {
                return null;
            }

            float total = 0f;
            foreach (EnemySpawnEntry entry in enemyPrefabs)
            {
                total += Mathf.Max(0f, entry.chance);
            }

            if (total <= 0f)
            {
                return enemyPrefabs[0].prefab;
            }

            float roll = Random.Range(0f, total);
            float cumulative = 0f;
            foreach (EnemySpawnEntry entry in enemyPrefabs)
            {
                cumulative += Mathf.Max(0f, entry.chance);
                if (roll <= cumulative)
                {
                    return entry.prefab;
                }
            }

            return enemyPrefabs[enemyPrefabs.Length - 1].prefab;
        }
    }
}

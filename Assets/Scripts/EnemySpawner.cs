using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Wave-based enemy spawner with difficulty scaling.
/// Attach to an empty GameObject. Assign enemy prefabs in the Inspector.
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy Prefabs (assign in Inspector)")]
    public GameObject[] enemyPrefabs; // index 0 = basic, 1 = zigzag, 2 = diver, etc.

    [Header("Spawn Settings")]
    public float spawnYPosition = 6.5f;
    public float spawnXRange = 5f;
    public float timeBetweenWaves = 4f;
    public int baseEnemiesPerWave = 4;
    public float spawnInterval = 0.6f;

    [Header("Difficulty Scaling")]
    public int extraEnemiesPerWave = 2;
    public float spawnIntervalDecay = 0.95f; // multiply each wave
    public float minSpawnInterval = 0.2f;

    int currentWave;
    bool spawning;

    void OnEnable()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnGameStarted += BeginSpawning;
    }

    void OnDisable()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnGameStarted -= BeginSpawning;
    }

    void BeginSpawning()
    {
        currentWave = 0;
        StopAllCoroutines();
        StartCoroutine(SpawnLoop());
    }

    IEnumerator SpawnLoop()
    {
        yield return new WaitForSeconds(1.5f); // brief grace period

        while (true)
        {
            if (GameManager.Instance.IsGameOver) yield break;

            currentWave++;
            GameManager.Instance.SetWave(currentWave);

            int count = baseEnemiesPerWave + (currentWave - 1) * extraEnemiesPerWave;
            float interval = Mathf.Max(spawnInterval * Mathf.Pow(spawnIntervalDecay, currentWave - 1), minSpawnInterval);

            for (int i = 0; i < count; i++)
            {
                if (GameManager.Instance.IsGameOver) yield break;
                SpawnEnemy();
                yield return new WaitForSeconds(interval);
            }

            // Wait until most enemies are gone or timeout
            yield return new WaitForSeconds(timeBetweenWaves);
        }
    }

    void SpawnEnemy()
    {
        if (enemyPrefabs == null || enemyPrefabs.Length == 0) return;

        // Pick enemy type — higher waves introduce tougher types
        int maxIdx = Mathf.Min(currentWave / 2, enemyPrefabs.Length - 1);
        int idx = Random.Range(0, maxIdx + 1);

        float x = Random.Range(-spawnXRange, spawnXRange);
        Vector3 pos = new Vector3(x, spawnYPosition, 0f);
        Instantiate(enemyPrefabs[idx], pos, Quaternion.identity);
    }
}

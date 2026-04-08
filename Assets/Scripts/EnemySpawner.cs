using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Spawns waves of enemies with increasing difficulty.
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy Prefabs")]
    public GameObject enemyBasicPrefab;   // straight-moving scout
    public GameObject enemySinePrefab;    // sine-wave mover
    public GameObject enemyShooterPrefab; // shoots back

    [Header("Spawn Settings")]
    public float spawnYOffset = 6f;       // above screen
    public float spawnXRange  = 4f;       // horizontal range
    public float timeBetweenWaves = 4f;
    public float timeBetweenSpawns = 0.6f;

    [Header("Difficulty")]
    public int   baseEnemiesPerWave = 4;
    public int   enemiesPerWaveIncrease = 2;
    public float speedIncreasePerWave = 0.3f;

    private int currentWave = 0;
    private bool spawning = false;

    public void StartSpawning()
    {
        currentWave = 0;
        spawning = true;
        StartCoroutine(SpawnLoop());
    }

    public void StopSpawning()
    {
        spawning = false;
        StopAllCoroutines();
    }

    IEnumerator SpawnLoop()
    {
        while (spawning)
        {
            currentWave++;
            int enemyCount = baseEnemiesPerWave + (currentWave - 1) * enemiesPerWaveIncrease;

            if (UIManager.Instance != null)
                UIManager.Instance.ShowWaveBanner(currentWave);

            yield return new WaitForSeconds(1.5f); // brief pause for banner

            for (int i = 0; i < enemyCount; i++)
            {
                if (!spawning) yield break;
                SpawnEnemy();
                yield return new WaitForSeconds(timeBetweenSpawns);
            }

            // Wait until all enemies are gone or a timeout
            float timeout = 15f;
            while (GameObject.FindGameObjectsWithTag("Enemy").Length > 0 && timeout > 0)
            {
                timeout -= Time.deltaTime;
                yield return null;
            }

            yield return new WaitForSeconds(timeBetweenWaves);
        }
    }

    void SpawnEnemy()
    {
        // Pick a random prefab weighted by wave
        GameObject prefab = PickPrefab();
        if (prefab == null) return;

        float x = Random.Range(-spawnXRange, spawnXRange);
        Vector3 pos = new Vector3(x, spawnYOffset, 0f);
        GameObject enemy = Instantiate(prefab, pos, Quaternion.identity);

        // Scale difficulty
        Enemy e = enemy.GetComponent<Enemy>();
        if (e != null)
        {
            e.moveSpeed += currentWave * speedIncreasePerWave;
            e.maxHealth += (currentWave / 3); // tougher every 3 waves
            e.scoreValue = 100 + currentWave * 25;
        }
    }

    GameObject PickPrefab()
    {
        List<GameObject> pool = new List<GameObject>();

        // Always include basic
        if (enemyBasicPrefab != null) pool.Add(enemyBasicPrefab);

        // After wave 2, add sine
        if (currentWave >= 2 && enemySinePrefab != null) pool.Add(enemySinePrefab);

        // After wave 4, add shooter
        if (currentWave >= 4 && enemyShooterPrefab != null) pool.Add(enemyShooterPrefab);

        if (pool.Count == 0)
        {
            Debug.LogWarning("EnemySpawner: No enemy prefabs assigned!");
            return null;
        }

        return pool[Random.Range(0, pool.Count)];
    }
}

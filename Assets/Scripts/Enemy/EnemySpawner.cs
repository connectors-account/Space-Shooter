using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Spawns waves of enemies with increasing difficulty.
/// Manages wave progression, enemy types, and spawn patterns.
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy Prefabs")]
    public GameObject straightEnemyPrefab;
    public GameObject zigzagEnemyPrefab;
    public GameObject swooperEnemyPrefab;
    public GameObject bossEnemyPrefab;

    [Header("Spawn Settings")]
    public float spawnYPosition = 7f;
    public float spawnXRange = 6f;
    public float timeBetweenWaves = 3f;
    public float spawnInterval = 1f;

    [Header("Wave Settings")]
    public int currentWave = 0;
    public int baseEnemiesPerWave = 5;
    public int enemiesPerWaveIncrease = 2;

    private int enemiesRemainingInWave;
    private int enemiesAlive;
    private bool spawning = false;

    void Start()
    {
        // Spawning is started by GameManager
    }

    public void StartSpawning()
    {
        StartCoroutine(WaveLoop());
    }

    public void StopSpawning()
    {
        spawning = false;
        StopAllCoroutines();
    }

    IEnumerator WaveLoop()
    {
        spawning = true;

        while (spawning)
        {
            currentWave++;
            int enemyCount = baseEnemiesPerWave + (currentWave - 1) * enemiesPerWaveIncrease;
            enemiesRemainingInWave = enemyCount;
            enemiesAlive = 0;

            if (UIManager.Instance != null)
                UIManager.Instance.UpdateWave(currentWave);

            if (UIManager.Instance != null)
                UIManager.Instance.ShowWaveAnnouncement(currentWave);

            yield return new WaitForSeconds(2f);

            // Spawn boss every 5 waves
            bool isBossWave = (currentWave % 5 == 0) && bossEnemyPrefab != null;

            if (isBossWave)
            {
                SpawnEnemy(bossEnemyPrefab, 0f);
                enemiesAlive++;
                enemiesRemainingInWave = 1;
            }
            else
            {
                // Spawn regular enemies
                for (int i = 0; i < enemyCount; i++)
                {
                    if (!spawning) yield break;

                    GameObject prefab = ChooseEnemyPrefab();
                    float xPos = Random.Range(-spawnXRange, spawnXRange);
                    SpawnEnemy(prefab, xPos);
                    enemiesAlive++;

                    float interval = Mathf.Max(0.3f, spawnInterval - currentWave * 0.03f);
                    yield return new WaitForSeconds(interval);
                }
            }

            // Wait until all enemies are destroyed
            while (enemiesAlive > 0)
            {
                enemiesAlive = CountEnemies();
                yield return new WaitForSeconds(0.5f);
            }

            yield return new WaitForSeconds(timeBetweenWaves);
        }
    }

    GameObject ChooseEnemyPrefab()
    {
        // More enemy types unlock as waves progress
        List<GameObject> available = new List<GameObject>();

        if (straightEnemyPrefab != null)
            available.Add(straightEnemyPrefab);

        if (currentWave >= 2 && zigzagEnemyPrefab != null)
            available.Add(zigzagEnemyPrefab);

        if (currentWave >= 3 && swooperEnemyPrefab != null)
            available.Add(swooperEnemyPrefab);

        if (available.Count == 0) return straightEnemyPrefab;

        return available[Random.Range(0, available.Count)];
    }

    void SpawnEnemy(GameObject prefab, float xPos)
    {
        if (prefab == null) return;
        Vector3 spawnPos = new Vector3(xPos, spawnYPosition, 0f);
        Instantiate(prefab, spawnPos, Quaternion.identity);
    }

    int CountEnemies()
    {
        return GameObject.FindGameObjectsWithTag("Enemy").Length;
    }

    public int GetCurrentWave()
    {
        return currentWave;
    }
}

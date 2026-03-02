using UnityEngine;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    [System.Serializable]
    public class EnemySpawnInfo
    {
        public GameObject prefab;
        public float spawnWeight = 1f;
        public int minWaveToSpawn = 1;
    }

    [Header("Spawn Settings")]
    public List<EnemySpawnInfo> enemyTypes = new List<EnemySpawnInfo>();
    public float spawnInterval = 2f;
    public float minSpawnInterval = 0.5f;
    public float spawnIntervalDecreasePerWave = 0.1f;
    public float spawnXMin = -7f;
    public float spawnXMax = 7f;
    public float spawnY = 6f;

    [Header("Wave Settings")]
    public int enemiesPerWave = 10;
    public int enemiesIncreasePerWave = 5;

    private float nextSpawnTime = 0f;
    private int enemiesSpawnedThisWave = 0;
    private int currentWave = 1;
    private bool isSpawning = false;

    public static EnemySpawner Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        if (!isSpawning) return;
        if (GameManager.Instance != null && GameManager.Instance.IsGamePaused()) return;

        if (Time.time >= nextSpawnTime && enemiesSpawnedThisWave < GetEnemiesForCurrentWave())
        {
            SpawnEnemy();
            nextSpawnTime = Time.time + GetCurrentSpawnInterval();
            enemiesSpawnedThisWave++;
        }
    }

    public void StartSpawning()
    {
        isSpawning = true;
        enemiesSpawnedThisWave = 0;
    }

    public void StopSpawning()
    {
        isSpawning = false;
    }

    public void SetWave(int wave)
    {
        currentWave = wave;
        enemiesSpawnedThisWave = 0;
    }

    float GetCurrentSpawnInterval()
    {
        float interval = spawnInterval - (currentWave - 1) * spawnIntervalDecreasePerWave;
        return Mathf.Max(interval, minSpawnInterval);
    }

    int GetEnemiesForCurrentWave()
    {
        return enemiesPerWave + (currentWave - 1) * enemiesIncreasePerWave;
    }

    void SpawnEnemy()
    {
        if (enemyTypes == null || enemyTypes.Count == 0) return;

        // Select enemy based on weight and wave requirements
        List<EnemySpawnInfo> availableEnemies = new List<EnemySpawnInfo>();
        float totalWeight = 0f;

        foreach (var enemy in enemyTypes)
        {
            if (enemy.prefab != null && currentWave >= enemy.minWaveToSpawn)
            {
                availableEnemies.Add(enemy);
                totalWeight += enemy.spawnWeight;
            }
        }

        if (availableEnemies.Count == 0) return;

        // Weighted random selection
        float randomValue = Random.Range(0f, totalWeight);
        float cumulativeWeight = 0f;
        EnemySpawnInfo selectedEnemy = availableEnemies[0];

        foreach (var enemy in availableEnemies)
        {
            cumulativeWeight += enemy.spawnWeight;
            if (randomValue <= cumulativeWeight)
            {
                selectedEnemy = enemy;
                break;
            }
        }

        // Spawn at random X position
        float spawnX = Random.Range(spawnXMin, spawnXMax);
        Vector3 spawnPosition = new Vector3(spawnX, spawnY, 0f);

        Instantiate(selectedEnemy.prefab, spawnPosition, Quaternion.identity);
    }

    public bool HasFinishedSpawningWave()
    {
        return enemiesSpawnedThisWave >= GetEnemiesForCurrentWave();
    }

    public void SpawnBoss(GameObject bossPrefab)
    {
        if (bossPrefab == null) return;
        
        Vector3 spawnPosition = new Vector3(0f, spawnY, 0f);
        Instantiate(bossPrefab, spawnPosition, Quaternion.identity);
    }
}

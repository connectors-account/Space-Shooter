using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class WaveData
{
    public string waveName;
    public int basicEnemyCount;
    public int zigzagEnemyCount;
    public int tankEnemyCount;
    public float spawnInterval;
    public float waveDelay;
}

public class SpawnManager : MonoBehaviour
{
    [Header("Enemy Prefab")]
    public GameObject enemyPrefab;

    [Header("Power-up Prefab")]
    public GameObject powerUpPrefab;

    [Header("Spawn Settings")]
    public float spawnXMin = -7f;
    public float spawnXMax = 7f;
    public float spawnY = 6f;
    public float powerUpSpawnChance = 0.15f;

    [Header("Wave Configuration")]
    public List<WaveData> waves = new List<WaveData>();

    private int currentWave = 0;
    private int enemiesRemainingInWave = 0;
    private int enemiesAlive = 0;
    private bool isSpawning = false;
    private GameManager gameManager;
    private UIManager uiManager;

    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        uiManager = FindObjectOfType<UIManager>();

        if (waves.Count == 0)
        {
            CreateDefaultWaves();
        }
    }

    void CreateDefaultWaves()
    {
        waves.Add(new WaveData { waveName = "Wave 1", basicEnemyCount = 5, zigzagEnemyCount = 0, tankEnemyCount = 0, spawnInterval = 1.5f, waveDelay = 2f });
        waves.Add(new WaveData { waveName = "Wave 2", basicEnemyCount = 4, zigzagEnemyCount = 3, tankEnemyCount = 0, spawnInterval = 1.2f, waveDelay = 2f });
        waves.Add(new WaveData { waveName = "Wave 3", basicEnemyCount = 5, zigzagEnemyCount = 3, tankEnemyCount = 1, spawnInterval = 1f, waveDelay = 2f });
        waves.Add(new WaveData { waveName = "Wave 4", basicEnemyCount = 6, zigzagEnemyCount = 4, tankEnemyCount = 2, spawnInterval = 0.9f, waveDelay = 2f });
        waves.Add(new WaveData { waveName = "Final Wave", basicEnemyCount = 8, zigzagEnemyCount = 5, tankEnemyCount = 3, spawnInterval = 0.8f, waveDelay = 3f });
    }

    public void StartSpawning()
    {
        if (!isSpawning)
        {
            currentWave = 0;
            isSpawning = true;
            StartCoroutine(SpawnWaves());
        }
    }

    public void StopSpawning()
    {
        isSpawning = false;
        StopAllCoroutines();
    }

    IEnumerator SpawnWaves()
    {
        while (currentWave < waves.Count && isSpawning)
        {
            WaveData wave = waves[currentWave];

            if (uiManager != null)
                uiManager.ShowWaveText(wave.waveName);

            if (gameManager != null)
                gameManager.SetCurrentWave(currentWave + 1);

            yield return new WaitForSeconds(wave.waveDelay);

            if (!isSpawning) yield break;

            List<EnemyType> enemiesToSpawn = new List<EnemyType>();

            for (int i = 0; i < wave.basicEnemyCount; i++)
                enemiesToSpawn.Add(EnemyType.Basic);
            for (int i = 0; i < wave.zigzagEnemyCount; i++)
                enemiesToSpawn.Add(EnemyType.Zigzag);
            for (int i = 0; i < wave.tankEnemyCount; i++)
                enemiesToSpawn.Add(EnemyType.Tank);

            ShuffleList(enemiesToSpawn);

            enemiesRemainingInWave = enemiesToSpawn.Count;
            enemiesAlive = 0;

            foreach (EnemyType enemyType in enemiesToSpawn)
            {
                if (!isSpawning) yield break;

                SpawnEnemy(enemyType);
                enemiesAlive++;
                yield return new WaitForSeconds(wave.spawnInterval);
            }

            while (enemiesAlive > 0 && isSpawning)
            {
                yield return new WaitForSeconds(0.1f);
            }

            currentWave++;
        }

        if (isSpawning && currentWave >= waves.Count)
        {
            if (gameManager != null)
                gameManager.Victory();
        }
    }

    void SpawnEnemy(EnemyType type)
    {
        if (enemyPrefab == null) return;

        float randomX = Random.Range(spawnXMin, spawnXMax);
        Vector3 spawnPosition = new Vector3(randomX, spawnY, 0f);

        GameObject enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
        EnemyController enemyController = enemy.GetComponent<EnemyController>();

        if (enemyController != null)
        {
            enemyController.SetEnemyType(type);
        }
    }

    public void OnEnemyDestroyed()
    {
        enemiesAlive--;

        if (Random.value < powerUpSpawnChance && powerUpPrefab != null)
        {
            SpawnPowerUp();
        }
    }

    void SpawnPowerUp()
    {
        float randomX = Random.Range(spawnXMin, spawnXMax);
        Vector3 spawnPosition = new Vector3(randomX, spawnY, 0f);
        Instantiate(powerUpPrefab, spawnPosition, Quaternion.identity);
    }

    void ShuffleList<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            T temp = list[i];
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }

    public int GetCurrentWave()
    {
        return currentWave + 1;
    }

    public int GetTotalWaves()
    {
        return waves.Count;
    }

    public int GetEnemiesAlive()
    {
        return enemiesAlive;
    }
}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Manages wave-based enemy spawning with increasing difficulty.
/// </summary>
public class WaveSpawner : MonoBehaviour
{
    public static WaveSpawner Instance { get; private set; }

    [Header("Spawn Settings")]
    public List<GameObject> enemyPrefabs = new List<GameObject>();
    public float spawnXMin = -7f;
    public float spawnXMax = 7f;
    public float spawnY = 6.5f;
    public float timeBetweenSpawns = 1f;
    public float timeBetweenWaves = 3f;

    [Header("Wave Progression")]
    public int baseEnemiesPerWave = 5;
    public int enemiesPerWaveIncrease = 2;
    public float spawnRateDecreasePerWave = 0.05f;
    public float minTimeBetweenSpawns = 0.3f;

    private int currentWave = 0;
    private int enemiesRemainingInWave;
    private int enemiesAlive;
    private int enemiesToSpawn;
    private bool isSpawning;

    public int CurrentWave => currentWave;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void StartSpawning()
    {
        currentWave = 0;
        StartCoroutine(SpawnWaveLoop());
    }

    public void StopSpawning()
    {
        isSpawning = false;
        StopAllCoroutines();
    }

    IEnumerator SpawnWaveLoop()
    {
        isSpawning = true;

        while (isSpawning)
        {
            currentWave++;
            enemiesToSpawn = baseEnemiesPerWave + (currentWave - 1) * enemiesPerWaveIncrease;
            enemiesRemainingInWave = enemiesToSpawn;
            enemiesAlive = 0;

            if (UIManager.Instance != null)
                UIManager.Instance.ShowWaveAnnouncement(currentWave);

            yield return new WaitForSeconds(2f);

            // Spawn enemies for this wave
            float currentSpawnInterval = Mathf.Max(
                timeBetweenSpawns - (currentWave - 1) * spawnRateDecreasePerWave,
                minTimeBetweenSpawns
            );

            for (int i = 0; i < enemiesToSpawn && isSpawning; i++)
            {
                SpawnEnemy();
                yield return new WaitForSeconds(currentSpawnInterval);
            }

            // Wait for all enemies to be destroyed before next wave
            while (enemiesAlive > 0 && isSpawning)
            {
                yield return new WaitForSeconds(0.5f);
            }

            if (isSpawning)
            {
                if (UIManager.Instance != null)
                    UIManager.Instance.ShowWaveClear();

                yield return new WaitForSeconds(timeBetweenWaves);
            }
        }
    }

    void SpawnEnemy()
    {
        if (enemyPrefabs.Count == 0) return;

        // Pick a random enemy type, with harder types more likely in later waves
        int maxIndex = Mathf.Min(enemyPrefabs.Count, 1 + currentWave / 3);
        int prefabIndex = Random.Range(0, maxIndex);

        float xPos = Random.Range(spawnXMin, spawnXMax);
        Vector3 spawnPos = new Vector3(xPos, spawnY, 0f);

        GameObject enemy = Instantiate(enemyPrefabs[prefabIndex], spawnPos, Quaternion.identity);

        // Scale difficulty with wave
        EnemyBase eb = enemy.GetComponent<EnemyBase>();
        if (eb != null)
        {
            // Increase health slightly each wave
            eb.maxHealth += (currentWave - 1) / 2;
            eb.moveSpeed += (currentWave - 1) * 0.1f;

            // Assign random movement patterns for variety
            if (currentWave >= 3)
            {
                int patternCount = System.Enum.GetValues(typeof(EnemyBase.EnemyMovePattern)).Length;
                eb.movePattern = (EnemyBase.EnemyMovePattern)Random.Range(0, patternCount);
            }
        }

        enemiesAlive++;
    }

    public void OnEnemyDestroyed()
    {
        enemiesAlive = Mathf.Max(0, enemiesAlive - 1);
    }
}

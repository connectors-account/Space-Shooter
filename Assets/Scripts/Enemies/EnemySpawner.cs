using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Manages wave-based enemy spawning with progressive difficulty.
/// Each wave increases enemy count and introduces tougher enemy types.
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    public static EnemySpawner Instance { get; private set; }

    [Header("Enemy Prefabs")]
    [SerializeField] private GameObject dronePrefab;
    [SerializeField] private GameObject fighterPrefab;
    [SerializeField] private GameObject bomberPrefab;

    [Header("Spawn Settings")]
    [SerializeField] private float spawnYPosition = 7f;
    [SerializeField] private float spawnXRange = 7f;
    [SerializeField] private float timeBetweenSpawns = 1.5f;
    [SerializeField] private float timeBetweenWaves = 4f;
    [SerializeField] private float minTimeBetweenSpawns = 0.4f;
    [SerializeField] private float spawnRateDecreasePerWave = 0.1f;

    [Header("Wave Settings")]
    [SerializeField] private int baseEnemiesPerWave = 5;
    [SerializeField] private int enemiesIncreasePerWave = 2;

    private int currentWave;
    private int enemiesRemainingInWave;
    private int enemiesAliveCount;
    private bool isSpawning;

    public int CurrentWave => currentWave;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    /// <summary>
    /// Starts the wave spawning sequence from wave 1.
    /// </summary>
    public void StartSpawning()
    {
        currentWave = 0;
        StartCoroutine(WaveLoop());
    }

    /// <summary>
    /// Stops all spawning coroutines.
    /// </summary>
    public void StopSpawning()
    {
        StopAllCoroutines();
        isSpawning = false;
    }

    /// <summary>
    /// Main wave loop: spawns waves with increasing difficulty.
    /// </summary>
    private IEnumerator WaveLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(timeBetweenWaves);

            currentWave++;
            int enemyCount = baseEnemiesPerWave + (currentWave - 1) * enemiesIncreasePerWave;
            enemiesRemainingInWave = enemyCount;
            enemiesAliveCount = 0;

            GameManager.Instance.UpdateWaveUI(currentWave);

            yield return StartCoroutine(SpawnWave(enemyCount));

            // Wait for all enemies in this wave to be destroyed
            while (enemiesAliveCount > 0)
                yield return new WaitForSeconds(0.5f);
        }
    }

    /// <summary>
    /// Spawns a wave of enemies one at a time with delays between each.
    /// </summary>
    private IEnumerator SpawnWave(int count)
    {
        isSpawning = true;
        float currentSpawnInterval = Mathf.Max(
            timeBetweenSpawns - (currentWave - 1) * spawnRateDecreasePerWave,
            minTimeBetweenSpawns
        );

        for (int i = 0; i < count; i++)
        {
            SpawnEnemy();
            yield return new WaitForSeconds(currentSpawnInterval);
        }

        isSpawning = false;
    }

    /// <summary>
    /// Selects and spawns an appropriate enemy based on the current wave.
    /// Wave 1-2: Drones only.
    /// Wave 3-4: Drones + Fighters.
    /// Wave 5+: All types including Bombers.
    /// </summary>
    private void SpawnEnemy()
    {
        float spawnX = Random.Range(-spawnXRange, spawnXRange);
        Vector3 spawnPos = new Vector3(spawnX, spawnYPosition, 0);

        GameObject prefab = ChooseEnemyPrefab();
        if (prefab == null) return;

        Instantiate(prefab, spawnPos, Quaternion.identity);
        enemiesAliveCount++;
    }

    /// <summary>
    /// Chooses an enemy prefab based on the current wave difficulty.
    /// </summary>
    private GameObject ChooseEnemyPrefab()
    {
        float roll = Random.value;

        if (currentWave <= 2)
        {
            // Only drones
            return dronePrefab;
        }
        else if (currentWave <= 4)
        {
            // Drones and fighters
            if (roll < 0.6f) return dronePrefab;
            return fighterPrefab;
        }
        else
        {
            // All enemy types
            if (roll < 0.4f) return dronePrefab;
            if (roll < 0.75f) return fighterPrefab;
            return bomberPrefab;
        }
    }

    /// <summary>
    /// Called by enemies when they are destroyed (died or went off screen).
    /// </summary>
    public void OnEnemyDestroyed()
    {
        enemiesAliveCount = Mathf.Max(0, enemiesAliveCount - 1);
    }
}

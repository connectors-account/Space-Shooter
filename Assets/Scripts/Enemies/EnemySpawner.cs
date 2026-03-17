using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Manages enemy wave spawning with progressive difficulty.
/// Attach to an empty GameObject in the scene.
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    public static EnemySpawner Instance { get; private set; }

    [Header("Enemy Prefabs")]
    public GameObject basicEnemyPrefab;
    public GameObject fastEnemyPrefab;
    public GameObject tankyEnemyPrefab;

    [Header("Spawn Settings")]
    public float spawnYOffset = 1f; // how far above screen top to spawn
    public float timeBetweenWaves = 5f;
    public float spawnInterval = 1f; // time between individual spawns in a wave

    [Header("Wave Progression")]
    public int baseEnemiesPerWave = 5;
    public int enemiesPerWaveIncrease = 2;
    public int maxEnemiesPerWave = 30;

    private int currentWave = 0;
    private int enemiesAlive = 0;
    private int enemiesToSpawnThisWave = 0;
    private int enemiesSpawnedThisWave = 0;
    private bool isSpawning = false;
    private Camera mainCamera;

    public int CurrentWave => currentWave;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        mainCamera = Camera.main;
    }

    public void StartSpawning()
    {
        currentWave = 0;
        enemiesAlive = 0;
        StartCoroutine(WaveLoop());
    }

    public void StopSpawning()
    {
        isSpawning = false;
        StopAllCoroutines();
    }

    IEnumerator WaveLoop()
    {
        isSpawning = true;

        while (isSpawning)
        {
            currentWave++;
            enemiesToSpawnThisWave = Mathf.Min(
                baseEnemiesPerWave + (currentWave - 1) * enemiesPerWaveIncrease,
                maxEnemiesPerWave);
            enemiesSpawnedThisWave = 0;

            // Update HUD
            HUDManager.Instance?.UpdateWave(currentWave);

            // Spawn enemies for this wave
            yield return StartCoroutine(SpawnWave());

            // Wait until all enemies in this wave are destroyed
            while (enemiesAlive > 0)
            {
                yield return null;
            }

            // Brief pause between waves
            yield return new WaitForSeconds(timeBetweenWaves);
        }
    }

    IEnumerator SpawnWave()
    {
        while (enemiesSpawnedThisWave < enemiesToSpawnThisWave)
        {
            SpawnEnemy();
            enemiesSpawnedThisWave++;
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    void SpawnEnemy()
    {
        // Determine spawn position - random X across screen, above viewport
        float screenTop = mainCamera.ViewportToWorldPoint(new Vector3(0.5f, 1f, 0)).y;
        float screenLeft = mainCamera.ViewportToWorldPoint(new Vector3(0.05f, 0, 0)).x;
        float screenRight = mainCamera.ViewportToWorldPoint(new Vector3(0.95f, 0, 0)).x;

        float spawnX = Random.Range(screenLeft, screenRight);
        float spawnY = screenTop + spawnYOffset;
        Vector3 spawnPos = new Vector3(spawnX, spawnY, 0);

        // Pick enemy type based on wave and randomness
        GameObject prefab = ChooseEnemyType();
        if (prefab == null) return;

        Instantiate(prefab, spawnPos, Quaternion.identity);
        enemiesAlive++;
    }

    GameObject ChooseEnemyType()
    {
        // Build a weighted pool based on current wave
        List<(GameObject prefab, float weight)> pool = new List<(GameObject, float)>();

        if (basicEnemyPrefab != null)
            pool.Add((basicEnemyPrefab, 50f));

        // Fast enemies appear from wave 2
        if (fastEnemyPrefab != null && currentWave >= 2)
            pool.Add((fastEnemyPrefab, 20f + currentWave * 2f));

        // Tanky enemies appear from wave 3
        if (tankyEnemyPrefab != null && currentWave >= 3)
            pool.Add((tankyEnemyPrefab, 5f + currentWave * 3f));

        if (pool.Count == 0) return basicEnemyPrefab;

        // Weighted random selection
        float totalWeight = 0;
        foreach (var entry in pool) totalWeight += entry.weight;

        float roll = Random.Range(0f, totalWeight);
        float cumulative = 0;
        foreach (var entry in pool)
        {
            cumulative += entry.weight;
            if (roll <= cumulative)
                return entry.prefab;
        }

        return pool[0].prefab;
    }

    public void OnEnemyDestroyed()
    {
        enemiesAlive = Mathf.Max(0, enemiesAlive - 1);
    }

    /// <summary>
    /// Destroys all currently alive enemies (used on game restart).
    /// </summary>
    public void ClearAllEnemies()
    {
        foreach (GameObject enemy in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            Destroy(enemy);
        }
        enemiesAlive = 0;
    }
}

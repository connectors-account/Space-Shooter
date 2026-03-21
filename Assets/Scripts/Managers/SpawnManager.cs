using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Manages enemy wave spawning with progressive difficulty.
/// </summary>
public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance { get; private set; }

    [Header("Enemy Prefabs")]
    public GameObject basicEnemyPrefab;
    public GameObject fastEnemyPrefab;
    public GameObject tankEnemyPrefab;
    public GameObject bomberEnemyPrefab;

    [Header("Power-Up Prefabs")]
    public GameObject shieldPowerUpPrefab;
    public GameObject multiShotPowerUpPrefab;
    public GameObject speedBoostPowerUpPrefab;
    public GameObject healthPowerUpPrefab;

    [Header("Spawn Settings")]
    public float spawnYPosition = 6f;
    public float spawnXRange = 8f;
    public float timeBetweenWaves = 3f;
    public float powerUpSpawnChance = 0.15f;
    public float powerUpSpawnInterval = 10f;

    [Header("Wave Progression")]
    public int baseEnemiesPerWave = 5;
    public int enemiesPerWaveIncrease = 2;
    public float spawnDelayBase = 1.5f;
    public float spawnDelayMinimum = 0.3f;
    public float spawnDelayReduction = 0.1f;

    private int currentWave;
    private int enemiesAlive;
    private int enemiesToSpawn;
    private int enemiesSpawned;
    private bool isSpawning;
    private Coroutine spawnCoroutine;
    private Coroutine powerUpCoroutine;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        StartCoroutine(StartFirstWave());
    }

    private IEnumerator StartFirstWave()
    {
        yield return new WaitForSeconds(1.5f);
        StartNextWave();
        powerUpCoroutine = StartCoroutine(SpawnPowerUpsRoutine());
    }

    /// <summary>Begin the next wave of enemies.</summary>
    public void StartNextWave()
    {
        currentWave++;
        GameManager.Instance?.SetWave(currentWave);

        enemiesToSpawn = baseEnemiesPerWave + (currentWave - 1) * enemiesPerWaveIncrease;
        enemiesSpawned = 0;
        enemiesAlive = 0;
        isSpawning = true;

        if (spawnCoroutine != null) StopCoroutine(spawnCoroutine);
        spawnCoroutine = StartCoroutine(SpawnWaveRoutine());
    }

    private IEnumerator SpawnWaveRoutine()
    {
        // Brief pause before wave starts
        UIManager.Instance?.ShowWaveAnnouncement(currentWave);
        yield return new WaitForSeconds(2f);

        while (enemiesSpawned < enemiesToSpawn)
        {
            SpawnEnemy();
            enemiesSpawned++;

            float delay = Mathf.Max(spawnDelayMinimum, spawnDelayBase - (currentWave - 1) * spawnDelayReduction);
            yield return new WaitForSeconds(delay);
        }
        isSpawning = false;
    }

    private void SpawnEnemy()
    {
        GameObject prefab = ChooseEnemyPrefab();
        if (prefab == null) return;

        float xPos = Random.Range(-spawnXRange, spawnXRange);
        Vector3 spawnPos = new Vector3(xPos, spawnYPosition, 0f);
        Instantiate(prefab, spawnPos, Quaternion.identity);
        enemiesAlive++;
    }

    private GameObject ChooseEnemyPrefab()
    {
        // Weighted selection based on current wave
        float roll = Random.value;

        if (currentWave >= 5 && roll < 0.1f && bomberEnemyPrefab != null)
            return bomberEnemyPrefab;
        if (currentWave >= 3 && roll < 0.3f && tankEnemyPrefab != null)
            return tankEnemyPrefab;
        if (currentWave >= 2 && roll < 0.5f && fastEnemyPrefab != null)
            return fastEnemyPrefab;

        return basicEnemyPrefab;
    }

    /// <summary>Called when an enemy is destroyed.</summary>
    public void OnEnemyDestroyed()
    {
        enemiesAlive--;

        if (!isSpawning && enemiesAlive <= 0)
        {
            StartCoroutine(WaveClearDelay());
        }
    }

    private IEnumerator WaveClearDelay()
    {
        yield return new WaitForSeconds(timeBetweenWaves);
        if (GameManager.Instance.CurrentState == GameManager.GameState.Playing)
        {
            StartNextWave();
        }
    }

    private IEnumerator SpawnPowerUpsRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(powerUpSpawnInterval);

            if (GameManager.Instance.CurrentState != GameManager.GameState.Playing) continue;
            if (Random.value > powerUpSpawnChance * (1f + currentWave * 0.05f)) continue;

            SpawnRandomPowerUp();
        }
    }

    private void SpawnRandomPowerUp()
    {
        List<GameObject> available = new List<GameObject>();
        if (shieldPowerUpPrefab != null) available.Add(shieldPowerUpPrefab);
        if (multiShotPowerUpPrefab != null) available.Add(multiShotPowerUpPrefab);
        if (speedBoostPowerUpPrefab != null) available.Add(speedBoostPowerUpPrefab);
        if (healthPowerUpPrefab != null) available.Add(healthPowerUpPrefab);

        if (available.Count == 0) return;

        GameObject prefab = available[Random.Range(0, available.Count)];
        float xPos = Random.Range(-spawnXRange, spawnXRange);
        Vector3 spawnPos = new Vector3(xPos, spawnYPosition, 0f);
        Instantiate(prefab, spawnPos, Quaternion.identity);
    }

    /// <summary>Stop all spawning (used on game over).</summary>
    public void StopSpawning()
    {
        isSpawning = false;
        if (spawnCoroutine != null) StopCoroutine(spawnCoroutine);
        if (powerUpCoroutine != null) StopCoroutine(powerUpCoroutine);
    }
}

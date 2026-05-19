using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Manages enemy wave spawning with increasing difficulty.
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    public static EnemySpawner Instance { get; private set; }

    [Header("Enemy Prefabs")]
    [SerializeField] private GameObject dronePrefab;
    [SerializeField] private GameObject fighterPrefab;
    [SerializeField] private GameObject bomberPrefab;
    [SerializeField] private GameObject swooperPrefab;

    [Header("Spawn Settings")]
    [SerializeField] private float spawnXMin = -7f;
    [SerializeField] private float spawnXMax = 7f;
    [SerializeField] private float spawnYMin = 6f;
    [SerializeField] private float spawnYMax = 7f;
    [SerializeField] private float waveDelay = 3f;
    [SerializeField] private float spawnInterval = 0.5f;

    [Header("Wave Settings")]
    [SerializeField] private int baseEnemiesPerWave = 5;
    [SerializeField] private int enemiesPerWaveIncrease = 2;
    [SerializeField] private float difficultyMultiplier = 1.1f;

    private int currentWave;
    private int enemiesAlive;
    private int totalEnemiesInWave;
    private int enemiesSpawnedInWave;
    private bool isSpawning;
    private Coroutine spawnCoroutine;

    public int CurrentWave => currentWave;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void StartSpawning()
    {
        currentWave = 0;
        enemiesAlive = 0;
        isSpawning = true;
        StartNextWave();
    }

    public void StopSpawning()
    {
        isSpawning = false;
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }
    }

    public void ClearAllEnemies()
    {
        StopSpawning();

        // Destroy all enemies
        EnemyBase[] enemies = FindObjectsOfType<EnemyBase>();
        foreach (var enemy in enemies)
        {
            Destroy(enemy.gameObject);
        }

        // Destroy all enemy bullets
        Bullet[] bullets = FindObjectsOfType<Bullet>();
        foreach (var bullet in bullets)
        {
            if (!bullet.IsPlayerBullet)
                Destroy(bullet.gameObject);
        }

        enemiesAlive = 0;
    }

    private void StartNextWave()
    {
        if (!isSpawning) return;

        currentWave++;
        totalEnemiesInWave = baseEnemiesPerWave + (currentWave - 1) * enemiesPerWaveIncrease;
        enemiesSpawnedInWave = 0;

        UIManager.Instance?.ShowWaveAnnouncement(currentWave);
        AudioManager.Instance?.PlaySFX("WaveStart");

        spawnCoroutine = StartCoroutine(SpawnWave());
    }

    private IEnumerator SpawnWave()
    {
        yield return new WaitForSeconds(waveDelay);

        while (enemiesSpawnedInWave < totalEnemiesInWave && isSpawning)
        {
            // Wait if game is paused
            while (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing)
                yield return null;

            SpawnEnemy();
            enemiesSpawnedInWave++;

            float adjustedInterval = Mathf.Max(0.2f, spawnInterval / (1f + currentWave * 0.1f));
            yield return new WaitForSeconds(adjustedInterval);
        }
    }

    private void SpawnEnemy()
    {
        GameObject prefab = ChooseEnemyType();
        if (prefab == null) return;

        float x = Random.Range(spawnXMin, spawnXMax);
        float y = Random.Range(spawnYMin, spawnYMax);
        Vector3 spawnPos = new Vector3(x, y, 0);

        // For swoopers, spawn from sides
        if (prefab == swooperPrefab)
        {
            bool fromLeft = Random.value > 0.5f;
            spawnPos = new Vector3(fromLeft ? -10f : 10f, Random.Range(1f, 4f), 0);
        }

        GameObject enemy = Instantiate(prefab, spawnPos, Quaternion.identity);

        // Scale difficulty with wave number
        EnemyBase enemyBase = enemy.GetComponent<EnemyBase>();
        if (enemyBase != null)
        {
            float multiplier = Mathf.Pow(difficultyMultiplier, currentWave - 1);
            // Health scaling is handled via the enemy's own Start
        }

        enemiesAlive++;
    }

    private GameObject ChooseEnemyType()
    {
        // Weighted random based on wave number
        float roll = Random.value;

        if (currentWave <= 2)
        {
            // Early waves: mostly drones
            return dronePrefab;
        }
        else if (currentWave <= 4)
        {
            // Mid-early: drones and fighters
            if (roll < 0.6f) return dronePrefab;
            if (roll < 0.9f) return fighterPrefab;
            return swooperPrefab;
        }
        else if (currentWave <= 7)
        {
            // Mid: mixed
            if (roll < 0.3f) return dronePrefab;
            if (roll < 0.6f) return fighterPrefab;
            if (roll < 0.8f) return swooperPrefab;
            return bomberPrefab;
        }
        else
        {
            // Late: heavy enemies
            if (roll < 0.2f) return dronePrefab;
            if (roll < 0.45f) return fighterPrefab;
            if (roll < 0.7f) return swooperPrefab;
            return bomberPrefab;
        }
    }

    public void OnEnemyDestroyed()
    {
        enemiesAlive = Mathf.Max(0, enemiesAlive - 1);

        // Check if wave is complete
        if (enemiesAlive <= 0 && enemiesSpawnedInWave >= totalEnemiesInWave)
        {
            // Wave complete
            if (isSpawning)
            {
                StartCoroutine(WaveCompleteDelay());
            }
        }
    }

    private IEnumerator WaveCompleteDelay()
    {
        yield return new WaitForSeconds(2f);
        if (isSpawning)
            StartNextWave();
    }
}

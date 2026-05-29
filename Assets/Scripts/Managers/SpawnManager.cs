using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Manages enemy wave spawning with progressive difficulty.
/// Spawns power-ups at random positions when requested.
/// </summary>
public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance { get; private set; }

    [Header("Enemy Prefabs")]
    public GameObject enemyStraightPrefab;
    public GameObject enemyZigzagPrefab;
    public GameObject enemyTrackerPrefab;

    [Header("Power-Up Prefabs")]
    public GameObject shieldPowerUpPrefab;
    public GameObject rapidFirePowerUpPrefab;
    public GameObject spreadShotPowerUpPrefab;

    [Header("Spawn Settings")]
    public float spawnXRange = 4f;
    public float spawnYPosition = 6f;
    public float timeBetweenWaves = 3f;
    public float spawnInterval = 1.2f;

    [Header("Wave Config")]
    public int totalWaves = 10;

    private int currentWave;
    private bool isSpawning;
    private List<GameObject> activeEnemies = new List<GameObject>();

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
        StartCoroutine(WaveLoop());
    }

    IEnumerator WaveLoop()
    {
        yield return new WaitForSeconds(1.5f); // Initial delay

        for (int wave = 1; wave <= totalWaves; wave++)
        {
            if (GameManager.Instance != null && GameManager.Instance.IsGameOver) yield break;

            currentWave = wave;
            GameManager.Instance?.SetWave(wave);

            yield return StartCoroutine(SpawnWave(wave));

            // Wait until all enemies from this wave are destroyed
            yield return new WaitUntil(() => AreAllEnemiesDead());

            if (GameManager.Instance != null && GameManager.Instance.IsGameOver) yield break;

            // Brief pause between waves
            yield return new WaitForSeconds(timeBetweenWaves);
        }

        // All waves complete — victory!
        GameManager.Instance?.AllWavesComplete();
    }

    IEnumerator SpawnWave(int waveNumber)
    {
        isSpawning = true;

        // Calculate enemies per wave: starts at 4, increases by 2 per wave
        int enemyCount = 4 + (waveNumber - 1) * 2;

        // Determine enemy type distribution based on wave
        for (int i = 0; i < enemyCount; i++)
        {
            if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
            {
                isSpawning = false;
                yield break;
            }

            GameObject enemyPrefab = ChooseEnemyForWave(waveNumber, i);
            SpawnEnemy(enemyPrefab, waveNumber);

            // Decrease spawn interval as waves progress
            float adjustedInterval = Mathf.Max(0.3f, spawnInterval - waveNumber * 0.08f);
            yield return new WaitForSeconds(adjustedInterval);
        }

        isSpawning = false;
    }

    GameObject ChooseEnemyForWave(int wave, int enemyIndex)
    {
        // Wave 1-3: mostly straight, some zigzag
        // Wave 4-6: mix of straight, zigzag, few trackers
        // Wave 7-10: all types, more trackers

        float roll = Random.value;

        if (wave <= 3)
        {
            if (roll < 0.7f) return enemyStraightPrefab;
            else return enemyZigzagPrefab;
        }
        else if (wave <= 6)
        {
            if (roll < 0.4f) return enemyStraightPrefab;
            else if (roll < 0.75f) return enemyZigzagPrefab;
            else return enemyTrackerPrefab;
        }
        else
        {
            if (roll < 0.25f) return enemyStraightPrefab;
            else if (roll < 0.55f) return enemyZigzagPrefab;
            else return enemyTrackerPrefab;
        }
    }

    void SpawnEnemy(GameObject prefab, int waveNumber)
    {
        if (prefab == null) return;

        float x = Random.Range(-spawnXRange, spawnXRange);
        Vector3 spawnPos = new Vector3(x, spawnYPosition, 0f);

        GameObject enemy = Instantiate(prefab, spawnPos, Quaternion.identity);
        activeEnemies.Add(enemy);

        // Scale difficulty: increase health and speed slightly per wave
        EnemyController ec = enemy.GetComponent<EnemyController>();
        if (ec != null)
        {
            // Add bonus health every 3 waves
            ec.health += (waveNumber - 1) / 3;
            // Slight speed increase
            ec.moveSpeed += waveNumber * 0.15f;
            // Increase score for tougher enemies
            ec.scoreValue = 100 + (waveNumber - 1) * 20;
        }
    }

    bool AreAllEnemiesDead()
    {
        // Clean null refs (destroyed enemies)
        activeEnemies.RemoveAll(e => e == null);
        return !isSpawning && activeEnemies.Count == 0;
    }

    public void SpawnRandomPowerUp(Vector3 position)
    {
        GameObject[] powerUpPrefabs = { shieldPowerUpPrefab, rapidFirePowerUpPrefab, spreadShotPowerUpPrefab };

        // Filter out nulls
        List<GameObject> validPrefabs = new List<GameObject>();
        foreach (var p in powerUpPrefabs)
        {
            if (p != null) validPrefabs.Add(p);
        }

        if (validPrefabs.Count == 0) return;

        int index = Random.Range(0, validPrefabs.Count);
        Instantiate(validPrefabs[index], position, Quaternion.identity);
    }

    public void ClearAllEnemies()
    {
        foreach (var enemy in activeEnemies)
        {
            if (enemy != null) Destroy(enemy);
        }
        activeEnemies.Clear();

        // Also destroy any stray bullets
        foreach (var bullet in GameObject.FindGameObjectsWithTag("EnemyBullet"))
        {
            Destroy(bullet);
        }
        foreach (var bullet in GameObject.FindGameObjectsWithTag("PlayerBullet"))
        {
            Destroy(bullet);
        }
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}

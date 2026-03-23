using UnityEngine;
using System.Collections;

/// <summary>
/// Spawns enemy ships at random positions at the top of the screen.
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private GameObject[] enemyPrefabs;
    [SerializeField] private float initialSpawnRate = 2f;
    [SerializeField] private float minimumSpawnRate = 0.5f;
    [SerializeField] private float spawnRateDecrease = 0.1f;
    [SerializeField] private float difficultyIncreaseInterval = 30f;

    [Header("Spawn Area")]
    [SerializeField] private float spawnY = 6f;
    [SerializeField] private float spawnXMin = -7f;
    [SerializeField] private float spawnXMax = 7f;

    [Header("Enemy Configuration")]
    [SerializeField] private float enemySpeedMin = 2f;
    [SerializeField] private float enemySpeedMax = 4f;
    [SerializeField] private bool enemiesCanShoot = true;

    private float currentSpawnRate;
    private float nextSpawnTime;
    private float nextDifficultyIncrease;
    private bool isSpawning = true;

    private void Start()
    {
        currentSpawnRate = initialSpawnRate;
        nextSpawnTime = Time.time + currentSpawnRate;
        nextDifficultyIncrease = Time.time + difficultyIncreaseInterval;
    }

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
        {
            isSpawning = false;
            return;
        }

        if (!isSpawning)
            return;

        // Handle spawning
        if (Time.time >= nextSpawnTime)
        {
            SpawnEnemy();
            nextSpawnTime = Time.time + currentSpawnRate;
        }

        // Increase difficulty over time
        if (Time.time >= nextDifficultyIncrease)
        {
            IncreaseDifficulty();
            nextDifficultyIncrease = Time.time + difficultyIncreaseInterval;
        }
    }

    private void SpawnEnemy()
    {
        if (enemyPrefabs == null || enemyPrefabs.Length == 0)
        {
            Debug.LogWarning("No enemy prefabs assigned to EnemySpawner!");
            return;
        }

        // Select random enemy prefab
        int randomIndex = Random.Range(0, enemyPrefabs.Length);
        GameObject enemyPrefab = enemyPrefabs[randomIndex];

        if (enemyPrefab == null)
        {
            Debug.LogWarning("Enemy prefab at index " + randomIndex + " is null!");
            return;
        }

        // Calculate spawn position
        float spawnX = Random.Range(spawnXMin, spawnXMax);
        Vector3 spawnPosition = new Vector3(spawnX, spawnY, 0f);

        // Instantiate enemy
        GameObject enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.Euler(0f, 0f, 180f));

        // Configure enemy properties
        EnemyController enemyController = enemy.GetComponent<EnemyController>();
        if (enemyController != null)
        {
            float randomSpeed = Random.Range(enemySpeedMin, enemySpeedMax);
            enemyController.Configure(randomSpeed, 1, 100, enemiesCanShoot);
        }
    }

    private void IncreaseDifficulty()
    {
        // Decrease spawn rate (spawn enemies more frequently)
        currentSpawnRate = Mathf.Max(minimumSpawnRate, currentSpawnRate - spawnRateDecrease);
        
        // Increase enemy speed range
        enemySpeedMin = Mathf.Min(enemySpeedMin + 0.2f, 5f);
        enemySpeedMax = Mathf.Min(enemySpeedMax + 0.3f, 8f);

        Debug.Log($"Difficulty increased! Spawn rate: {currentSpawnRate:F2}s, Speed: {enemySpeedMin:F1}-{enemySpeedMax:F1}");
    }

    /// <summary>
    /// Start or resume spawning.
    /// </summary>
    public void StartSpawning()
    {
        isSpawning = true;
    }

    /// <summary>
    /// Stop spawning enemies.
    /// </summary>
    public void StopSpawning()
    {
        isSpawning = false;
    }

    /// <summary>
    /// Reset spawner to initial settings.
    /// </summary>
    public void ResetSpawner()
    {
        currentSpawnRate = initialSpawnRate;
        enemySpeedMin = 2f;
        enemySpeedMax = 4f;
        isSpawning = true;
        nextSpawnTime = Time.time + currentSpawnRate;
        nextDifficultyIncrease = Time.time + difficultyIncreaseInterval;
    }

    /// <summary>
    /// Destroy all existing enemies in the scene.
    /// </summary>
    public void ClearAllEnemies()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemies)
        {
            Destroy(enemy);
        }

        // Also clear enemy bullets
        GameObject[] enemyBullets = GameObject.FindGameObjectsWithTag("EnemyBullet");
        foreach (GameObject bullet in enemyBullets)
        {
            Destroy(bullet);
        }
    }
}

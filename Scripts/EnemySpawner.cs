using UnityEngine;

/// <summary>
/// Spawns enemies at regular intervals with wave progression
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private GameObject[] enemyPrefabs;
    [SerializeField] private float initialSpawnRate = 2f;
    [SerializeField] private float minimumSpawnRate = 0.5f;
    [SerializeField] private float spawnRateDecrease = 0.1f;

    [Header("Spawn Area")]
    [SerializeField] private float spawnRangeX = 7f;
    [SerializeField] private float spawnPositionY = 6f;

    [Header("Wave Settings")]
    [SerializeField] private int enemiesPerWave = 5;
    [SerializeField] private float wavePauseTime = 3f;

    private float currentSpawnRate;
    private float nextSpawnTime;
    private int currentWave = 1;
    private int enemiesSpawnedThisWave = 0;
    private int enemiesDestroyedThisWave = 0;
    private bool isWavePaused = false;
    private float waveResumeTime;

    private GameManager gameManager;

    private void Start()
    {
        currentSpawnRate = initialSpawnRate;
        nextSpawnTime = Time.time + 1f; // Small delay before first spawn
        gameManager = FindObjectOfType<GameManager>();

        // Validate enemy prefabs
        if (enemyPrefabs == null || enemyPrefabs.Length == 0)
        {
            Debug.LogError("EnemySpawner: No enemy prefabs assigned!");
        }
    }

    private void Update()
    {
        // Don't spawn if game is over
        if (gameManager != null && gameManager.IsGameOver())
            return;

        // Handle wave pause
        if (isWavePaused)
        {
            if (Time.time >= waveResumeTime)
            {
                StartNextWave();
            }
            return;
        }

        // Spawn enemies
        if (Time.time >= nextSpawnTime)
        {
            SpawnEnemy();
            nextSpawnTime = Time.time + currentSpawnRate;
        }
    }

    private void SpawnEnemy()
    {
        if (enemyPrefabs == null || enemyPrefabs.Length == 0)
            return;

        // Random X position within spawn range
        float randomX = Random.Range(-spawnRangeX, spawnRangeX);
        Vector3 spawnPosition = new Vector3(randomX, spawnPositionY, 0f);

        // Select random enemy type (or based on wave for variety)
        int enemyIndex = 0;
        if (enemyPrefabs.Length > 1)
        {
            // Higher chance of harder enemies in later waves
            float maxIndex = Mathf.Min(currentWave, enemyPrefabs.Length);
            enemyIndex = Random.Range(0, (int)maxIndex);
        }

        // Spawn enemy
        GameObject enemy = Instantiate(enemyPrefabs[enemyIndex], spawnPosition, Quaternion.identity);
        
        // Scale enemy stats based on wave
        Enemy enemyScript = enemy.GetComponent<Enemy>();
        if (enemyScript != null)
        {
            enemyScript.SetWaveMultiplier(1f + (currentWave - 1) * 0.2f);
        }

        enemiesSpawnedThisWave++;

        // Check if wave is complete
        if (enemiesSpawnedThisWave >= enemiesPerWave + (currentWave - 1) * 2)
        {
            // Wave spawning complete, wait for all enemies to be destroyed
            // Actually, we'll just move to next wave after spawning is done
            // and give a brief pause
            isWavePaused = true;
            waveResumeTime = Time.time + wavePauseTime;
        }
    }

    private void StartNextWave()
    {
        currentWave++;
        enemiesSpawnedThisWave = 0;
        enemiesDestroyedThisWave = 0;
        isWavePaused = false;

        // Increase difficulty - faster spawn rate
        currentSpawnRate = Mathf.Max(minimumSpawnRate, currentSpawnRate - spawnRateDecrease);

        // Notify GameManager of new wave
        if (gameManager != null)
        {
            gameManager.OnNewWave(currentWave);
        }

        Debug.Log($"Wave {currentWave} started! Spawn rate: {currentSpawnRate}s");
    }

    // Called when an enemy is destroyed
    public void OnEnemyDestroyed()
    {
        enemiesDestroyedThisWave++;
    }

    // Get current wave number
    public int GetCurrentWave()
    {
        return currentWave;
    }

    // Reset spawner (for game restart)
    public void ResetSpawner()
    {
        currentWave = 1;
        enemiesSpawnedThisWave = 0;
        enemiesDestroyedThisWave = 0;
        currentSpawnRate = initialSpawnRate;
        isWavePaused = false;
        nextSpawnTime = Time.time + 1f;

        // Destroy all existing enemies
        Enemy[] existingEnemies = FindObjectsOfType<Enemy>();
        foreach (Enemy enemy in existingEnemies)
        {
            Destroy(enemy.gameObject);
        }
    }
}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// EnemySpawner manages wave-based enemy spawning.
/// Handles different enemy types and difficulty progression.
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    public static EnemySpawner Instance { get; private set; }

    [Header("Enemy Prefabs")]
    [Tooltip("Basic enemy prefab")]
    public GameObject basicEnemyPrefab;
    
    [Tooltip("ZigZag enemy prefab")]
    public GameObject zigzagEnemyPrefab;
    
    [Tooltip("Dive bomber enemy prefab")]
    public GameObject diveBomberPrefab;

    [Header("Spawn Settings")]
    [Tooltip("Minimum X position for spawning")]
    public float minSpawnX = -7f;
    
    [Tooltip("Maximum X position for spawning")]
    public float maxSpawnX = 7f;
    
    [Tooltip("Y position where enemies spawn")]
    public float spawnY = 6f;
    
    [Tooltip("Time between spawns within a wave")]
    public float spawnInterval = 1f;

    [Header("Wave Settings")]
    [Tooltip("Current wave number")]
    public int currentWave = 0;
    
    [Tooltip("Base number of enemies per wave")]
    public int baseEnemiesPerWave = 5;
    
    [Tooltip("Additional enemies per wave increase")]
    public int enemiesPerWaveIncrease = 2;
    
    [Tooltip("Time between waves")]
    public float timeBetweenWaves = 3f;

    private int enemiesRemainingInWave = 0;
    private int enemiesAlive = 0;
    private bool isSpawning = false;
    private bool canSpawn = false;

    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Spawning starts when game starts
        canSpawn = false;
    }

    /// <summary>
    /// Start the spawning system
    /// </summary>
    public void StartSpawning()
    {
        canSpawn = true;
        currentWave = 0;
        StartCoroutine(SpawnWaves());
    }

    /// <summary>
    /// Stop all spawning
    /// </summary>
    public void StopSpawning()
    {
        canSpawn = false;
        StopAllCoroutines();
    }

    /// <summary>
    /// Reset spawner state
    /// </summary>
    public void ResetSpawner()
    {
        StopSpawning();
        currentWave = 0;
        enemiesRemainingInWave = 0;
        enemiesAlive = 0;
        
        // Destroy all existing enemies
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemies)
        {
            Destroy(enemy);
        }
        
        // Destroy all bullets
        GameObject[] playerBullets = GameObject.FindGameObjectsWithTag("PlayerBullet");
        GameObject[] enemyBullets = GameObject.FindGameObjectsWithTag("EnemyBullet");
        
        foreach (GameObject bullet in playerBullets)
        {
            Destroy(bullet);
        }
        foreach (GameObject bullet in enemyBullets)
        {
            Destroy(bullet);
        }
    }

    /// <summary>
    /// Main wave spawning coroutine
    /// </summary>
    IEnumerator SpawnWaves()
    {
        while (canSpawn)
        {
            // Start new wave
            currentWave++;
            int enemiesThisWave = baseEnemiesPerWave + (currentWave - 1) * enemiesPerWaveIncrease;
            enemiesRemainingInWave = enemiesThisWave;
            enemiesAlive = 0;
            
            // Notify UI of wave start
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowWaveText(currentWave);
            }
            
            // Wait a moment before spawning
            yield return new WaitForSeconds(1.5f);
            
            // Spawn enemies for this wave
            isSpawning = true;
            
            for (int i = 0; i < enemiesThisWave && canSpawn; i++)
            {
                SpawnEnemy();
                enemiesRemainingInWave--;
                yield return new WaitForSeconds(spawnInterval);
            }
            
            isSpawning = false;
            
            // Wait until all enemies are defeated
            while (enemiesAlive > 0 && canSpawn)
            {
                yield return new WaitForSeconds(0.5f);
            }
            
            // Wave complete - wait before next wave
            yield return new WaitForSeconds(timeBetweenWaves);
        }
    }

    /// <summary>
    /// Spawn a single enemy
    /// </summary>
    void SpawnEnemy()
    {
        // Choose enemy type based on wave and randomness
        GameObject enemyPrefab = ChooseEnemyType();
        
        if (enemyPrefab != null)
        {
            // Random spawn position
            float spawnX = Random.Range(minSpawnX, maxSpawnX);
            Vector3 spawnPosition = new Vector3(spawnX, spawnY, 0f);
            
            GameObject enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
            enemy.tag = "Enemy";
            enemiesAlive++;
        }
    }

    /// <summary>
    /// Choose enemy type based on current wave
    /// </summary>
    GameObject ChooseEnemyType()
    {
        List<GameObject> availableTypes = new List<GameObject>();
        
        // Basic enemy always available
        if (basicEnemyPrefab != null)
        {
            availableTypes.Add(basicEnemyPrefab);
        }
        
        // ZigZag enemy from wave 2
        if (currentWave >= 2 && zigzagEnemyPrefab != null)
        {
            availableTypes.Add(zigzagEnemyPrefab);
        }
        
        // Dive bomber from wave 3
        if (currentWave >= 3 && diveBomberPrefab != null)
        {
            availableTypes.Add(diveBomberPrefab);
        }
        
        // Weighted selection - harder enemies more likely in later waves
        if (availableTypes.Count > 0)
        {
            // Simple random selection for now
            int index = Random.Range(0, availableTypes.Count);
            return availableTypes[index];
        }
        
        return basicEnemyPrefab;
    }

    /// <summary>
    /// Called when an enemy is destroyed
    /// </summary>
    public void OnEnemyDestroyed()
    {
        enemiesAlive--;
        enemiesAlive = Mathf.Max(0, enemiesAlive);
    }

    /// <summary>
    /// Get current wave number
    /// </summary>
    public int GetCurrentWave()
    {
        return currentWave;
    }

    /// <summary>
    /// Get number of enemies currently alive
    /// </summary>
    public int GetEnemiesAlive()
    {
        return enemiesAlive;
    }
}

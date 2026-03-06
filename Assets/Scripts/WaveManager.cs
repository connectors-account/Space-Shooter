using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Manages wave configuration, enemy spawning patterns, and difficulty progression.
/// </summary>
public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance { get; private set; }
    
    [System.Serializable]
    public class WaveConfig
    {
        public int basicEnemyCount = 5;
        public int zigzagEnemyCount = 0;
        public int shooterEnemyCount = 0;
        public float spawnInterval = 1.5f;
        public float enemySpeedMultiplier = 1f;
        public int enemyHealthMultiplier = 1;
    }
    
    [Header("Enemy Prefabs")]
    [SerializeField] private GameObject basicEnemyPrefab;
    [SerializeField] private GameObject zigzagEnemyPrefab;
    [SerializeField] private GameObject shooterEnemyPrefab;
    
    [Header("Spawn Settings")]
    [SerializeField] private float spawnY = 6f;
    [SerializeField] private float minSpawnX = -7f;
    [SerializeField] private float maxSpawnX = 7f;
    
    [Header("Difficulty Scaling")]
    [SerializeField] private float difficultyIncreaseRate = 0.1f;
    [SerializeField] private int baseEnemyCount = 5;
    [SerializeField] private float baseSpawnInterval = 1.5f;
    [SerializeField] private float minSpawnInterval = 0.3f;
    
    // State
    private bool isSpawning;
    private Coroutine spawnCoroutine;
    private int totalEnemiesInWave;
    
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
    /// Start spawning a new wave
    /// </summary>
    public void StartWave(int waveNumber)
    {
        if (isSpawning) return;
        
        WaveConfig config = GenerateWaveConfig(waveNumber);
        spawnCoroutine = StartCoroutine(SpawnWave(config, waveNumber));
    }
    
    /// <summary>
    /// Stop current wave spawning
    /// </summary>
    public void StopSpawning()
    {
        isSpawning = false;
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }
    }
    
    /// <summary>
    /// Generate wave configuration based on wave number
    /// </summary>
    private WaveConfig GenerateWaveConfig(int waveNumber)
    {
        WaveConfig config = new WaveConfig();
        
        float difficultyMultiplier = 1f + (waveNumber - 1) * difficultyIncreaseRate;
        
        // Base enemy count increases with waves
        config.basicEnemyCount = Mathf.RoundToInt(baseEnemyCount + (waveNumber - 1) * 2);
        
        // Introduce zigzag enemies at wave 3
        if (waveNumber >= 3)
        {
            config.zigzagEnemyCount = Mathf.RoundToInt((waveNumber - 2) * 1.5f);
        }
        
        // Introduce shooter enemies at wave 5
        if (waveNumber >= 5)
        {
            config.shooterEnemyCount = Mathf.RoundToInt((waveNumber - 4) * 1f);
        }
        
        // Spawn interval decreases (faster spawning)
        config.spawnInterval = Mathf.Max(minSpawnInterval, baseSpawnInterval - (waveNumber - 1) * 0.1f);
        
        // Enemy speed increases
        config.enemySpeedMultiplier = 1f + (waveNumber - 1) * 0.05f;
        
        // Enemy health increases every 3 waves
        config.enemyHealthMultiplier = 1 + (waveNumber - 1) / 3;
        
        return config;
    }
    
    /// <summary>
    /// Spawn all enemies for a wave
    /// </summary>
    private IEnumerator SpawnWave(WaveConfig config, int waveNumber)
    {
        isSpawning = true;
        
        // Build spawn queue
        List<EnemyController.EnemyType> spawnQueue = new List<EnemyController.EnemyType>();
        
        for (int i = 0; i < config.basicEnemyCount; i++)
            spawnQueue.Add(EnemyController.EnemyType.Basic);
        
        for (int i = 0; i < config.zigzagEnemyCount; i++)
            spawnQueue.Add(EnemyController.EnemyType.Zigzag);
        
        for (int i = 0; i < config.shooterEnemyCount; i++)
            spawnQueue.Add(EnemyController.EnemyType.Shooter);
        
        // Shuffle spawn queue
        ShuffleList(spawnQueue);
        
        totalEnemiesInWave = spawnQueue.Count;
        GameManager.Instance?.SetEnemyCount(totalEnemiesInWave);
        
        // Spawn enemies
        foreach (var enemyType in spawnQueue)
        {
            if (!isSpawning) yield break;
            
            SpawnEnemy(enemyType, config);
            
            yield return new WaitForSeconds(config.spawnInterval);
        }
        
        isSpawning = false;
    }
    
    /// <summary>
    /// Spawn a single enemy
    /// </summary>
    private void SpawnEnemy(EnemyController.EnemyType type, WaveConfig config)
    {
        GameObject prefab = GetPrefabForType(type);
        if (prefab == null) return;
        
        // Random spawn position
        float spawnX = Random.Range(minSpawnX, maxSpawnX);
        Vector3 spawnPos = new Vector3(spawnX, spawnY, 0);
        
        GameObject enemy = Instantiate(prefab, spawnPos, Quaternion.Euler(0, 0, 180));
        
        // Configure enemy
        EnemyController controller = enemy.GetComponent<EnemyController>();
        if (controller != null)
        {
            float baseSpeed = GetBaseSpeedForType(type);
            int baseHealth = GetBaseHealthForType(type);
            
            controller.Initialize(
                type,
                baseSpeed * config.enemySpeedMultiplier,
                baseHealth * config.enemyHealthMultiplier
            );
        }
    }
    
    private GameObject GetPrefabForType(EnemyController.EnemyType type)
    {
        switch (type)
        {
            case EnemyController.EnemyType.Basic:
                return basicEnemyPrefab;
            case EnemyController.EnemyType.Zigzag:
                return zigzagEnemyPrefab;
            case EnemyController.EnemyType.Shooter:
                return shooterEnemyPrefab;
            default:
                return basicEnemyPrefab;
        }
    }
    
    private float GetBaseSpeedForType(EnemyController.EnemyType type)
    {
        switch (type)
        {
            case EnemyController.EnemyType.Basic:
                return 3f;
            case EnemyController.EnemyType.Zigzag:
                return 2.5f;
            case EnemyController.EnemyType.Shooter:
                return 2f;
            default:
                return 3f;
        }
    }
    
    private int GetBaseHealthForType(EnemyController.EnemyType type)
    {
        switch (type)
        {
            case EnemyController.EnemyType.Basic:
                return 25;
            case EnemyController.EnemyType.Zigzag:
                return 35;
            case EnemyController.EnemyType.Shooter:
                return 50;
            default:
                return 25;
        }
    }
    
    private void ShuffleList<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            T temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }
    }
}

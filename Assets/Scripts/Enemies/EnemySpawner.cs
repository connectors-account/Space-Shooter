using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Spawns enemies at regular intervals.
/// Attach this script to an empty GameObject in the scene.
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [System.Serializable]
    public class EnemyType
    {
        public string name;
        public GameObject prefab;
        [Range(0f, 1f)] public float spawnChance = 0.5f;
        public int minWaveToSpawn = 1;
        public Color enemyColor = Color.red;
    }
    
    [Header("Spawn Settings")]
    [Tooltip("Time between enemy spawns in seconds")]
    [SerializeField] private float spawnInterval = 2f;
    
    [Tooltip("Minimum spawn interval (for difficulty scaling)")]
    [SerializeField] private float minSpawnInterval = 0.5f;
    
    [Tooltip("How much to decrease spawn interval per wave")]
    [SerializeField] private float spawnIntervalDecrease = 0.1f;
    
    [Header("Spawn Position Settings")]
    [Tooltip("Y position where enemies spawn")]
    [SerializeField] private float spawnY = 6f;
    
    [Tooltip("Horizontal range for spawning (-range to +range)")]
    [SerializeField] private float spawnRangeX = 7f;
    
    [Header("Wave Settings")]
    [Tooltip("Number of enemies per wave")]
    [SerializeField] private int enemiesPerWave = 5;
    
    [Tooltip("Additional enemies per wave")]
    [SerializeField] private int additionalEnemiesPerWave = 2;
    
    [Tooltip("Time between waves")]
    [SerializeField] private float timeBetweenWaves = 5f;
    
    [Header("Enemy Types")]
    [SerializeField] private List<EnemyType> enemyTypes = new List<EnemyType>();
    
    // State tracking
    private int currentWave = 0;
    private int enemiesSpawnedThisWave = 0;
    private int enemiesRemainingThisWave = 0;
    private bool isSpawning = false;
    private float currentSpawnInterval;
    
    // Singleton
    public static EnemySpawner Instance { get; private set; }
    
    // Properties
    public int CurrentWave => currentWave;
    public bool IsSpawning => isSpawning;
    
    /// <summary>
    /// Initialize singleton.
    /// </summary>
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// Initialize default enemy type if none configured.
    /// </summary>
    private void Start()
    {
        currentSpawnInterval = spawnInterval;
        
        // Add a default enemy type if none configured
        if (enemyTypes.Count == 0)
        {
            enemyTypes.Add(new EnemyType
            {
                name = "Basic Enemy",
                prefab = null, // Will create default
                spawnChance = 1f,
                minWaveToSpawn = 1,
                enemyColor = Color.red
            });
        }
    }
    
    /// <summary>
    /// Start spawning enemies.
    /// </summary>
    public void StartSpawning()
    {
        if (!isSpawning)
        {
            isSpawning = true;
            currentWave = 0;
            currentSpawnInterval = spawnInterval;
            StartCoroutine(WaveSpawnCoroutine());
        }
    }
    
    /// <summary>
    /// Stop spawning enemies.
    /// </summary>
    public void StopSpawning()
    {
        isSpawning = false;
        StopAllCoroutines();
    }
    
    /// <summary>
    /// Coroutine that manages wave-based spawning.
    /// </summary>
    private IEnumerator WaveSpawnCoroutine()
    {
        while (isSpawning)
        {
            // Start new wave
            currentWave++;
            int enemiesToSpawn = enemiesPerWave + (additionalEnemiesPerWave * (currentWave - 1));
            enemiesSpawnedThisWave = 0;
            enemiesRemainingThisWave = enemiesToSpawn;
            
            Debug.Log($"Starting Wave {currentWave} with {enemiesToSpawn} enemies");
            
            // Notify UI
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowWaveText(currentWave);
            }
            
            // Spawn enemies for this wave
            while (enemiesSpawnedThisWave < enemiesToSpawn && isSpawning)
            {
                SpawnEnemy();
                enemiesSpawnedThisWave++;
                yield return new WaitForSeconds(currentSpawnInterval);
            }
            
            // Wait for wave to be cleared or timeout
            float waveTimer = 0f;
            while (enemiesRemainingThisWave > 0 && waveTimer < 30f && isSpawning)
            {
                enemiesRemainingThisWave = CountActiveEnemies();
                waveTimer += Time.deltaTime;
                yield return null;
            }
            
            // Decrease spawn interval for next wave (increase difficulty)
            currentSpawnInterval = Mathf.Max(minSpawnInterval, currentSpawnInterval - spawnIntervalDecrease);
            
            // Wait before next wave
            if (isSpawning)
            {
                yield return new WaitForSeconds(timeBetweenWaves);
            }
        }
    }
    
    /// <summary>
    /// Spawn a single enemy.
    /// </summary>
    private void SpawnEnemy()
    {
        // Select enemy type
        EnemyType selectedType = SelectEnemyType();
        
        // Calculate spawn position
        float spawnX = Random.Range(-spawnRangeX, spawnRangeX);
        Vector3 spawnPosition = new Vector3(spawnX, spawnY, 0f);
        
        GameObject enemy;
        
        if (selectedType.prefab != null)
        {
            // Use prefab
            enemy = Instantiate(selectedType.prefab, spawnPosition, Quaternion.identity);
        }
        else
        {
            // Create default enemy
            enemy = CreateDefaultEnemy(spawnPosition, selectedType);
        }
        
        // Set enemy tag
        enemy.tag = "Enemy";
    }
    
    /// <summary>
    /// Select an enemy type based on spawn chances and wave requirements.
    /// </summary>
    /// <returns>Selected enemy type</returns>
    private EnemyType SelectEnemyType()
    {
        List<EnemyType> validTypes = enemyTypes.FindAll(e => e.minWaveToSpawn <= currentWave);
        
        if (validTypes.Count == 0)
        {
            return enemyTypes[0];
        }
        
        // Calculate total chance
        float totalChance = 0f;
        foreach (var type in validTypes)
        {
            totalChance += type.spawnChance;
        }
        
        // Random selection
        float roll = Random.Range(0f, totalChance);
        float cumulative = 0f;
        
        foreach (var type in validTypes)
        {
            cumulative += type.spawnChance;
            if (roll <= cumulative)
            {
                return type;
            }
        }
        
        return validTypes[validTypes.Count - 1];
    }
    
    /// <summary>
    /// Create a default enemy when no prefab is assigned.
    /// </summary>
    /// <param name="position">Spawn position</param>
    /// <param name="type">Enemy type configuration</param>
    /// <returns>Created enemy GameObject</returns>
    private GameObject CreateDefaultEnemy(Vector3 position, EnemyType type)
    {
        // Create enemy from primitive
        GameObject enemy = GameObject.CreatePrimitive(PrimitiveType.Cube);
        enemy.name = type.name ?? "Enemy";
        enemy.transform.position = position;
        enemy.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
        enemy.tag = "Enemy";
        
        // Remove 3D collider
        Destroy(enemy.GetComponent<BoxCollider>());
        
        // Add 2D collider
        BoxCollider2D collider = enemy.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        collider.size = new Vector2(0.8f, 0.8f);
        
        // Add Rigidbody2D
        Rigidbody2D rb = enemy.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        
        // Add enemy components
        EnemyController controller = enemy.AddComponent<EnemyController>();
        EnemyHealth health = enemy.AddComponent<EnemyHealth>();
        
        // Configure based on wave
        float speedMultiplier = 1f + (currentWave * 0.1f);
        int healthMultiplier = 1 + (currentWave / 3);
        
        controller.SetMoveSpeed(3f * speedMultiplier);
        health.SetMaxHealth(20 * healthMultiplier);
        health.SetScoreValue(100 + (currentWave * 10));
        
        // Randomly assign movement pattern
        EnemyController.MovementPattern pattern;
        int patternRoll = Random.Range(0, 100);
        
        if (patternRoll < 50)
        {
            pattern = EnemyController.MovementPattern.Straight;
        }
        else if (patternRoll < 75)
        {
            pattern = EnemyController.MovementPattern.Zigzag;
        }
        else if (patternRoll < 90)
        {
            pattern = EnemyController.MovementPattern.Sine;
        }
        else
        {
            pattern = EnemyController.MovementPattern.Homing;
        }
        
        controller.SetMovementPattern(pattern);
        
        // Add shooting for some enemies
        if (currentWave >= 2 && Random.value > 0.5f)
        {
            EnemyShooting shooting = enemy.AddComponent<EnemyShooting>();
            shooting.SetFireRate(2f - (currentWave * 0.1f));
        }
        
        // Set color
        MeshRenderer renderer = enemy.GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            renderer.material.color = type.enemyColor;
        }
        
        return enemy;
    }
    
    /// <summary>
    /// Count the number of active enemies in the scene.
    /// </summary>
    /// <returns>Number of active enemies</returns>
    private int CountActiveEnemies()
    {
        return GameObject.FindGameObjectsWithTag("Enemy").Length;
    }
    
    /// <summary>
    /// Reset the spawner state.
    /// </summary>
    public void Reset()
    {
        StopSpawning();
        currentWave = 0;
        currentSpawnInterval = spawnInterval;
        
        // Destroy all existing enemies
        foreach (GameObject enemy in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            Destroy(enemy);
        }
    }
}

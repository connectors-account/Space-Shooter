// =============================================================================
// EnemySpawner.cs
// Wave-based enemy spawning system. Each wave increases in difficulty by
// spawning more enemies, introducing new enemy types, and increasing speed.
// This is a singleton. Attach to an empty "EnemySpawner" GameObject.
// =============================================================================
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Configuration for a single wave of enemies.
/// </summary>
[System.Serializable]
public class WaveConfig
{
    [Tooltip("Number of enemies to spawn in this wave.")]
    public int enemyCount = 5;

    [Tooltip("Time between each enemy spawn in this wave.")]
    public float spawnInterval = 1.5f;

    [Tooltip("Speed multiplier applied to enemies in this wave.")]
    public float speedMultiplier = 1.0f;

    [Tooltip("Whether Zigzag enemies can appear in this wave.")]
    public bool includeZigzag = false;

    [Tooltip("Whether Charger enemies can appear in this wave.")]
    public bool includeCharger = false;

    [Tooltip("Chance (0-1) of spawning a Zigzag enemy instead of Basic.")]
    [Range(0f, 1f)]
    public float zigzagChance = 0.3f;

    [Tooltip("Chance (0-1) of spawning a Charger enemy instead of Basic.")]
    [Range(0f, 1f)]
    public float chargerChance = 0.2f;
}

public class EnemySpawner : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Singleton
    // -------------------------------------------------------------------------
    public static EnemySpawner Instance { get; private set; }

    // -------------------------------------------------------------------------
    // Enemy Prefabs
    // -------------------------------------------------------------------------
    [Header("Enemy Prefabs")]
    [Tooltip("Basic enemy prefab (straight movement).")]
    public GameObject basicEnemyPrefab;

    [Tooltip("Zigzag enemy prefab (wave movement).")]
    public GameObject zigzagEnemyPrefab;

    [Tooltip("Charger enemy prefab (rushes the player).")]
    public GameObject chargerEnemyPrefab;

    // -------------------------------------------------------------------------
    // Spawn Settings
    // -------------------------------------------------------------------------
    [Header("Spawn Settings")]
    [Tooltip("Minimum X position for spawning enemies.")]
    public float minSpawnX = -8f;

    [Tooltip("Maximum X position for spawning enemies.")]
    public float maxSpawnX = 8f;

    [Tooltip("Y position where enemies spawn (above the screen).")]
    public float spawnY = 6.5f;

    [Tooltip("Delay before the first wave starts.")]
    public float initialDelay = 2f;

    [Tooltip("Delay between waves.")]
    public float timeBetweenWaves = 3f;

    // -------------------------------------------------------------------------
    // Wave Configuration
    // -------------------------------------------------------------------------
    [Header("Wave Configuration")]
    [Tooltip("Predefined wave configurations. After these are exhausted, " +
             "procedural waves are generated with increasing difficulty.")]
    public WaveConfig[] predefinedWaves;

    // -------------------------------------------------------------------------
    // Internal State
    // -------------------------------------------------------------------------
    private int currentWaveIndex = 0;
    private int enemiesRemainingInWave = 0;
    private int enemiesSpawnedInWave = 0;
    private bool isSpawning = false;

    // -------------------------------------------------------------------------
    // Unity Lifecycle
    // -------------------------------------------------------------------------

    /// <summary>
    /// Setup singleton reference.
    /// </summary>
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    /// <summary>
    /// If no predefined waves are set, create default ones.
    /// Start the spawning coroutine.
    /// </summary>
    void Start()
    {
        if (predefinedWaves == null || predefinedWaves.Length == 0)
        {
            CreateDefaultWaves();
        }

        StartCoroutine(SpawnWaves());
    }

    /// <summary>
    /// Cleanup singleton reference.
    /// </summary>
    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    // -------------------------------------------------------------------------
    // Default Wave Generation
    // -------------------------------------------------------------------------

    /// <summary>
    /// Creates a set of default predefined waves with escalating difficulty.
    /// </summary>
    private void CreateDefaultWaves()
    {
        predefinedWaves = new WaveConfig[10];

        // Wave 1: Simple intro — few basic enemies
        predefinedWaves[0] = new WaveConfig
        {
            enemyCount = 4,
            spawnInterval = 2.0f,
            speedMultiplier = 0.8f,
            includeZigzag = false,
            includeCharger = false
        };

        // Wave 2: Slightly more enemies
        predefinedWaves[1] = new WaveConfig
        {
            enemyCount = 6,
            spawnInterval = 1.8f,
            speedMultiplier = 0.9f,
            includeZigzag = false,
            includeCharger = false
        };

        // Wave 3: Introduce zigzag enemies
        predefinedWaves[2] = new WaveConfig
        {
            enemyCount = 7,
            spawnInterval = 1.5f,
            speedMultiplier = 1.0f,
            includeZigzag = true,
            includeCharger = false,
            zigzagChance = 0.25f
        };

        // Wave 4: More zigzag, faster spawns
        predefinedWaves[3] = new WaveConfig
        {
            enemyCount = 9,
            spawnInterval = 1.3f,
            speedMultiplier = 1.0f,
            includeZigzag = true,
            includeCharger = false,
            zigzagChance = 0.35f
        };

        // Wave 5: Introduce chargers
        predefinedWaves[4] = new WaveConfig
        {
            enemyCount = 10,
            spawnInterval = 1.2f,
            speedMultiplier = 1.1f,
            includeZigzag = true,
            includeCharger = true,
            zigzagChance = 0.3f,
            chargerChance = 0.15f
        };

        // Wave 6-10: Progressively harder
        for (int i = 5; i < 10; i++)
        {
            predefinedWaves[i] = new WaveConfig
            {
                enemyCount = 10 + (i - 4) * 2,
                spawnInterval = Mathf.Max(0.5f, 1.2f - (i - 4) * 0.1f),
                speedMultiplier = 1.1f + (i - 4) * 0.1f,
                includeZigzag = true,
                includeCharger = true,
                zigzagChance = Mathf.Min(0.5f, 0.3f + (i - 4) * 0.05f),
                chargerChance = Mathf.Min(0.35f, 0.15f + (i - 4) * 0.05f)
            };
        }
    }

    // -------------------------------------------------------------------------
    // Wave Spawning Coroutine
    // -------------------------------------------------------------------------

    /// <summary>
    /// Main coroutine that manages wave-by-wave enemy spawning.
    /// Waits for each wave to be cleared before starting the next.
    /// </summary>
    private IEnumerator SpawnWaves()
    {
        // Wait before starting the first wave
        yield return new WaitForSeconds(initialDelay);

        while (true)
        {
            // Check if the game is still running
            if (GameManager.Instance != null && GameManager.Instance.currentState != GameState.Playing)
            {
                yield return new WaitForSeconds(0.5f);
                continue;
            }

            // Get the wave configuration (predefined or procedural)
            WaveConfig wave = GetWaveConfig(currentWaveIndex);

            // Advance wave in GameManager
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AdvanceWave();
            }

            // Spawn all enemies for this wave
            yield return StartCoroutine(SpawnWaveEnemies(wave));

            // Wait for all enemies in the wave to be destroyed
            while (enemiesRemainingInWave > 0)
            {
                yield return new WaitForSeconds(0.25f);
            }

            currentWaveIndex++;

            // Pause between waves
            yield return new WaitForSeconds(timeBetweenWaves);
        }
    }

    /// <summary>
    /// Spawns all enemies for a single wave with configured intervals.
    /// </summary>
    private IEnumerator SpawnWaveEnemies(WaveConfig wave)
    {
        enemiesRemainingInWave = wave.enemyCount;
        enemiesSpawnedInWave = 0;
        isSpawning = true;

        for (int i = 0; i < wave.enemyCount; i++)
        {
            // Wait if paused
            while (GameManager.Instance != null && GameManager.Instance.currentState == GameState.Paused)
            {
                yield return null;
            }

            // Stop spawning if the game is over
            if (GameManager.Instance != null && GameManager.Instance.currentState == GameState.GameOver)
            {
                yield break;
            }

            SpawnEnemy(wave);
            enemiesSpawnedInWave++;

            yield return new WaitForSeconds(wave.spawnInterval);
        }

        isSpawning = false;
    }

    // -------------------------------------------------------------------------
    // Enemy Spawning Logic
    // -------------------------------------------------------------------------

    /// <summary>
    /// Spawns a single enemy based on the wave configuration.
    /// Selects enemy type based on configured chances.
    /// </summary>
    private void SpawnEnemy(WaveConfig wave)
    {
        // Determine which enemy type to spawn
        GameObject prefabToSpawn = basicEnemyPrefab;
        EnemyType selectedType = EnemyType.Basic;

        float roll = Random.value;

        if (wave.includeCharger && roll < wave.chargerChance && chargerEnemyPrefab != null)
        {
            prefabToSpawn = chargerEnemyPrefab;
            selectedType = EnemyType.Charger;
        }
        else if (wave.includeZigzag && roll < wave.chargerChance + wave.zigzagChance && zigzagEnemyPrefab != null)
        {
            prefabToSpawn = zigzagEnemyPrefab;
            selectedType = EnemyType.Zigzag;
        }

        if (prefabToSpawn == null)
        {
            Debug.LogWarning("EnemySpawner: No enemy prefab assigned for type " + selectedType);
            enemiesRemainingInWave--;
            return;
        }

        // Random spawn position along the top of the screen
        float randomX = Random.Range(minSpawnX, maxSpawnX);
        Vector3 spawnPosition = new Vector3(randomX, spawnY, 0f);

        // Instantiate the enemy
        GameObject enemy = Instantiate(prefabToSpawn, spawnPosition, Quaternion.identity);

        // Apply the wave's speed multiplier
        EnemyController ec = enemy.GetComponent<EnemyController>();
        if (ec != null)
        {
            ec.moveSpeed *= wave.speedMultiplier;
            ec.chargeSpeed *= wave.speedMultiplier;
        }
    }

    // -------------------------------------------------------------------------
    // Wave Configuration Retrieval
    // -------------------------------------------------------------------------

    /// <summary>
    /// Returns the wave config for the given index.
    /// If beyond predefined waves, generates a procedural wave.
    /// </summary>
    private WaveConfig GetWaveConfig(int index)
    {
        if (index < predefinedWaves.Length)
        {
            return predefinedWaves[index];
        }
        else
        {
            // Procedural wave generation: scales infinitely
            return GenerateProceduralWave(index);
        }
    }

    /// <summary>
    /// Creates a procedurally generated wave that scales with wave number.
    /// The difficulty increases logarithmically to stay challenging but fair.
    /// </summary>
    private WaveConfig GenerateProceduralWave(int waveIndex)
    {
        return new WaveConfig
        {
            enemyCount = Mathf.Min(30, 10 + waveIndex),
            spawnInterval = Mathf.Max(0.3f, 1.0f - waveIndex * 0.03f),
            speedMultiplier = Mathf.Min(2.5f, 1.0f + waveIndex * 0.08f),
            includeZigzag = true,
            includeCharger = true,
            zigzagChance = Mathf.Min(0.5f, 0.3f + waveIndex * 0.02f),
            chargerChance = Mathf.Min(0.4f, 0.2f + waveIndex * 0.02f)
        };
    }

    // -------------------------------------------------------------------------
    // Public Methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Called by EnemyController when an enemy is destroyed.
    /// Decrements the remaining enemy count for the current wave.
    /// </summary>
    public void OnEnemyDestroyed()
    {
        enemiesRemainingInWave = Mathf.Max(0, enemiesRemainingInWave - 1);
    }

    /// <summary>Returns the current wave index (0-based).</summary>
    public int GetCurrentWaveIndex() { return currentWaveIndex; }

    /// <summary>Returns how many enemies are left in the current wave.</summary>
    public int GetEnemiesRemaining() { return enemiesRemainingInWave; }
}

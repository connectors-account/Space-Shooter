// ============================================================================
// EnemySpawner.cs - Wave-based enemy spawning with progressive difficulty
// ============================================================================
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Manages wave-based enemy spawning. Each wave defines which enemy types appear,
/// how many, and the spawn interval. Difficulty increases each wave by spawning
/// tougher enemies faster with scaled-up stats.
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    // ---- Enemy Prefab References ----
    [Header("Enemy Prefabs")]
    [Tooltip("Basic straight-flying enemy prefab.")]
    [SerializeField] private GameObject enemyStraightPrefab;
    [Tooltip("Zigzag-flying enemy prefab.")]
    [SerializeField] private GameObject enemyZigzagPrefab;
    [Tooltip("Circling enemy prefab.")]
    [SerializeField] private GameObject enemyCirclingPrefab;
    [Tooltip("Diver/kamikaze enemy prefab.")]
    [SerializeField] private GameObject enemyDiverPrefab;

    // ---- Wave Settings ----
    [Header("Wave Settings")]
    [Tooltip("Seconds to wait before the first wave starts.")]
    [SerializeField] private float initialDelay = 2f;
    [Tooltip("Seconds between the end of one wave and the start of the next.")]
    [SerializeField] private float timeBetweenWaves = 4f;
    [Tooltip("Base number of enemies in wave 1.")]
    [SerializeField] private int baseEnemiesPerWave = 6;
    [Tooltip("Additional enemies added per wave.")]
    [SerializeField] private int enemiesPerWaveIncrement = 3;
    [Tooltip("Base spawn interval in seconds between individual enemies.")]
    [SerializeField] private float baseSpawnInterval = 1.2f;
    [Tooltip("Minimum spawn interval (speed cap).")]
    [SerializeField] private float minSpawnInterval = 0.3f;

    // ---- Runtime ----
    private int enemiesAlive;
    private int enemiesToSpawn;
    private bool isSpawning;
    private Coroutine waveCoroutine;

    // ========================================================================
    // Unity Lifecycle
    // ========================================================================

    private void Start()
    {
        waveCoroutine = StartCoroutine(WaveLoop());
    }

    private void OnDisable()
    {
        if (waveCoroutine != null) StopCoroutine(waveCoroutine);
    }

    // ========================================================================
    // Wave Loop
    // ========================================================================

    /// <summary>
    /// Main coroutine that runs the wave loop for the entire game session.
    /// </summary>
    private IEnumerator WaveLoop()
    {
        yield return new WaitForSeconds(initialDelay);

        while (true)
        {
            if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameManager.GameState.Playing)
            {
                yield return null;
                continue;
            }

            int wave = GameManager.Instance != null ? GameManager.Instance.CurrentWave : 1;
            float difficulty = GameManager.Instance != null ? GameManager.Instance.GetDifficultyMultiplier() : 1f;

            // Calculate wave parameters.
            int totalEnemies = baseEnemiesPerWave + (wave - 1) * enemiesPerWaveIncrement;
            float spawnInterval = Mathf.Max(baseSpawnInterval - (wave - 1) * 0.08f, minSpawnInterval);

            // Notify systems about the new wave.
            AudioManager.Instance?.PlaySFX(AudioManager.SFX.WaveStart);

            // Build spawn list for this wave.
            List<GameObject> spawnList = BuildSpawnList(wave, totalEnemies);

            // Spawn enemies one by one.
            isSpawning = true;
            enemiesAlive = 0;

            foreach (GameObject prefab in spawnList)
            {
                if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameManager.GameState.Playing)
                    break;

                SpawnEnemy(prefab, difficulty);
                yield return new WaitForSeconds(spawnInterval);
            }

            isSpawning = false;

            // Wait until all enemies from this wave are gone (or a timeout).
            float waveTimeout = 30f;
            float elapsed = 0f;
            while (enemiesAlive > 0 && elapsed < waveTimeout)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }

            // Advance wave.
            GameManager.Instance?.AdvanceWave();

            // Brief pause between waves.
            yield return new WaitForSeconds(timeBetweenWaves);
        }
    }

    // ========================================================================
    // Spawn List Building
    // ========================================================================

    /// <summary>
    /// Creates a randomized list of enemy prefabs for the given wave.
    /// Earlier waves favor simpler enemies; later waves introduce tougher types.
    /// </summary>
    private List<GameObject> BuildSpawnList(int wave, int totalEnemies)
    {
        List<GameObject> list = new List<GameObject>(totalEnemies);
        List<WeightedEnemy> weightedPool = new List<WeightedEnemy>();

        // Straight enemies are always available.
        if (enemyStraightPrefab != null)
            weightedPool.Add(new WeightedEnemy(enemyStraightPrefab, 40));

        // Zigzag unlocks at wave 2.
        if (enemyZigzagPrefab != null && wave >= 2)
            weightedPool.Add(new WeightedEnemy(enemyZigzagPrefab, 25 + wave));

        // Circling unlocks at wave 3.
        if (enemyCirclingPrefab != null && wave >= 3)
            weightedPool.Add(new WeightedEnemy(enemyCirclingPrefab, 15 + wave));

        // Diver unlocks at wave 4.
        if (enemyDiverPrefab != null && wave >= 4)
            weightedPool.Add(new WeightedEnemy(enemyDiverPrefab, 10 + wave));

        // Calculate total weight.
        int totalWeight = 0;
        foreach (var we in weightedPool) totalWeight += we.weight;

        // Fill the spawn list via weighted random.
        for (int i = 0; i < totalEnemies; i++)
        {
            int roll = Random.Range(0, totalWeight);
            int cumulative = 0;
            foreach (var we in weightedPool)
            {
                cumulative += we.weight;
                if (roll < cumulative)
                {
                    list.Add(we.prefab);
                    break;
                }
            }
        }

        return list;
    }

    // ========================================================================
    // Enemy Spawning
    // ========================================================================

    /// <summary>
    /// Spawns a single enemy at a random position along the top of the screen.
    /// </summary>
    private void SpawnEnemy(GameObject prefab, float difficulty)
    {
        if (prefab == null) return;

        float spawnX = GameBounds.Instance != null ? GameBounds.Instance.RandomTopX() : Random.Range(-6f, 6f);
        float spawnY = GameBounds.Instance != null ? GameBounds.Instance.TopSpawnY() : 7f;
        Vector3 spawnPos = new Vector3(spawnX, spawnY, 0f);

        GameObject enemy = Instantiate(prefab, spawnPos, Quaternion.identity);

        // Apply difficulty scaling.
        EnemyBase enemyScript = enemy.GetComponent<EnemyBase>();
        if (enemyScript != null)
        {
            enemyScript.ApplyDifficultyScaling(difficulty);
        }

        // Track alive count.
        enemiesAlive++;
        EnemyDeathTracker tracker = enemy.AddComponent<EnemyDeathTracker>();
        tracker.Initialize(this);
    }

    /// <summary>Called by EnemyDeathTracker when an enemy is destroyed or pooled.</summary>
    public void OnEnemyRemoved()
    {
        enemiesAlive = Mathf.Max(0, enemiesAlive - 1);
    }

    // ========================================================================
    // Helper Structs
    // ========================================================================

    private struct WeightedEnemy
    {
        public GameObject prefab;
        public int weight;

        public WeightedEnemy(GameObject prefab, int weight)
        {
            this.prefab = prefab;
            this.weight = weight;
        }
    }
}

// ============================================================================
// EnemyDeathTracker.cs - Small helper that notifies the spawner on disable.
// ============================================================================

/// <summary>
/// Attached dynamically to spawned enemies. Fires a callback when the enemy
/// is deactivated or destroyed, so the spawner can track alive counts.
/// </summary>
public class EnemyDeathTracker : MonoBehaviour
{
    private EnemySpawner spawner;
    private bool hasNotified;

    public void Initialize(EnemySpawner spawner)
    {
        this.spawner = spawner;
        hasNotified = false;
    }

    private void OnDisable()
    {
        if (!hasNotified && spawner != null)
        {
            spawner.OnEnemyRemoved();
            hasNotified = true;
        }
    }

    private void OnDestroy()
    {
        if (!hasNotified && spawner != null)
        {
            spawner.OnEnemyRemoved();
            hasNotified = true;
        }
    }
}

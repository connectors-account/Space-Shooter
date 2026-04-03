using UnityEngine;
using System.Collections;

/// <summary>
/// EnemySpawner handles wave-based enemy spawning.
/// It listens to the GameManager's wave change event and spawns
/// enemies in increasing numbers and difficulty across 5 waves.
///
/// Wave composition:
///   Wave 1: 4 Basic enemies
///   Wave 2: 5 Basic + 2 Fast
///   Wave 3: 4 Basic + 3 Fast + 1 Tank
///   Wave 4: 3 Basic + 4 Fast + 2 Tank
///   Wave 5: 4 Basic + 4 Fast + 3 Tank (boss wave)
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    // ============================================================
    // PREFABS
    // ============================================================
    [Header("Enemy Prefabs")]
    [Tooltip("Basic enemy prefab")]
    public GameObject basicEnemyPrefab;

    [Tooltip("Fast enemy prefab")]
    public GameObject fastEnemyPrefab;

    [Tooltip("Tank enemy prefab")]
    public GameObject tankEnemyPrefab;

    // ============================================================
    // SPAWN SETTINGS
    // ============================================================
    [Header("Spawn Settings")]
    [Tooltip("Horizontal spawn range (enemies appear at random X within this)")]
    public float spawnRangeX = 7f;

    [Tooltip("Y position where enemies spawn (above the screen)")]
    public float spawnY = 6f;

    [Tooltip("Seconds between each enemy spawn within a wave")]
    public float spawnInterval = 0.8f;

    // ============================================================
    // INTERNAL
    // ============================================================
    private Coroutine spawnCoroutine;

    // ============================================================
    // UNITY LIFECYCLE
    // ============================================================

    void OnEnable()
    {
        // Subscribe to wave changes from GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnWaveChanged += OnWaveStarted;
        }
    }

    void OnDisable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnWaveChanged -= OnWaveStarted;
        }
    }

    // We also check in Start in case GameManager fires before OnEnable
    void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnWaveChanged -= OnWaveStarted; // prevent double-sub
            GameManager.Instance.OnWaveChanged += OnWaveStarted;
        }
    }

    // ============================================================
    // WAVE SPAWNING
    // ============================================================

    /// <summary>
    /// Called when a new wave begins. Determines the wave composition
    /// and starts the spawn coroutine.
    /// </summary>
    void OnWaveStarted(int waveNumber)
    {
        // Stop any in-progress spawning
        if (spawnCoroutine != null)
            StopCoroutine(spawnCoroutine);

        // Determine enemy counts for this wave
        int basicCount, fastCount, tankCount;
        GetWaveComposition(waveNumber, out basicCount, out fastCount, out tankCount);

        // Tell GameManager how many enemies to expect
        int totalEnemies = basicCount + fastCount + tankCount;
        if (GameManager.Instance != null)
        {
            GameManager.Instance.EnemiesRemaining = totalEnemies;
        }

        // Begin spawning
        spawnCoroutine = StartCoroutine(SpawnWave(basicCount, fastCount, tankCount));
    }

    /// <summary>
    /// Define the composition of each wave.
    /// Waves get progressively harder with more enemies and tougher types.
    /// </summary>
    void GetWaveComposition(int wave, out int basic, out int fast, out int tank)
    {
        switch (wave)
        {
            case 1:
                basic = 4; fast = 0; tank = 0;
                break;
            case 2:
                basic = 5; fast = 2; tank = 0;
                break;
            case 3:
                basic = 4; fast = 3; tank = 1;
                break;
            case 4:
                basic = 3; fast = 4; tank = 2;
                break;
            case 5:
                basic = 4; fast = 4; tank = 3;
                break;
            default:
                // For any waves beyond 5 (extensibility)
                basic = 3 + wave; fast = 2 + wave; tank = wave;
                break;
        }
    }

    /// <summary>
    /// Coroutine that spawns enemies one at a time with delays.
    /// Enemies are shuffled so types appear in random order.
    /// </summary>
    IEnumerator SpawnWave(int basicCount, int fastCount, int tankCount)
    {
        // Build a list of enemy types to spawn
        var spawnList = new System.Collections.Generic.List<EnemyController.EnemyType>();

        for (int i = 0; i < basicCount; i++)
            spawnList.Add(EnemyController.EnemyType.Basic);
        for (int i = 0; i < fastCount; i++)
            spawnList.Add(EnemyController.EnemyType.Fast);
        for (int i = 0; i < tankCount; i++)
            spawnList.Add(EnemyController.EnemyType.Tank);

        // Shuffle the list (Fisher-Yates) so enemies come in random order
        for (int i = spawnList.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            var temp = spawnList[i];
            spawnList[i] = spawnList[j];
            spawnList[j] = temp;
        }

        // Spawn each enemy with a delay
        foreach (var type in spawnList)
        {
            SpawnEnemy(type);
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    /// <summary>
    /// Instantiate a single enemy of the given type at a random X position.
    /// </summary>
    void SpawnEnemy(EnemyController.EnemyType type)
    {
        // Choose the correct prefab
        GameObject prefab = null;
        switch (type)
        {
            case EnemyController.EnemyType.Basic: prefab = basicEnemyPrefab; break;
            case EnemyController.EnemyType.Fast:  prefab = fastEnemyPrefab;  break;
            case EnemyController.EnemyType.Tank:  prefab = tankEnemyPrefab;  break;
        }

        if (prefab == null)
        {
            Debug.LogWarning($"EnemySpawner: No prefab assigned for enemy type {type}");
            // Still decrement the counter so wave can end
            if (GameManager.Instance != null)
                GameManager.Instance.EnemyDestroyed();
            return;
        }

        // Random horizontal position within spawn range
        float randomX = Random.Range(-spawnRangeX, spawnRangeX);
        Vector3 spawnPos = new Vector3(randomX, spawnY, 0f);

        GameObject enemy = Instantiate(prefab, spawnPos, Quaternion.identity);
        enemy.tag = "Enemy";

        // Set the enemy type (the EnemyController.Start() will configure stats)
        EnemyController ec = enemy.GetComponent<EnemyController>();
        if (ec != null)
        {
            ec.enemyType = type;
        }
    }
}

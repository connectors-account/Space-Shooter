using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Manages wave-based enemy spawning with increasing difficulty.
/// Attach to an empty GameObject in the scene.
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private float spawnYPosition = 6.5f;
    [SerializeField] private float spawnXRange = 7.5f;
    [SerializeField] private float timeBetweenSpawns = 1.2f;
    [SerializeField] private float timeBetweenWaves = 4f;

    [Header("Wave Settings")]
    [SerializeField] private int baseEnemiesPerWave = 5;
    [SerializeField] private int enemiesPerWaveIncrease = 2;
    [SerializeField] private int maxEnemiesPerWave = 30;

    private int currentWave = 0;
    private int enemiesRemainingInWave = 0;
    private int enemiesAlive = 0;
    private bool isSpawning = false;
    private Coroutine spawnCoroutine;

    public int CurrentWave => currentWave;

    private void Start()
    {
        // Spawner waits for GameManager to start it
    }

    /// <summary>
    /// Begin the wave spawning cycle.
    /// </summary>
    public void StartSpawning()
    {
        currentWave = 0;
        isSpawning = true;
        StartNextWave();
    }

    /// <summary>
    /// Stop all spawning.
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
    /// Called when an enemy is destroyed (for wave tracking).
    /// </summary>
    public void OnEnemyDestroyed()
    {
        enemiesAlive = Mathf.Max(0, enemiesAlive - 1);

        if (enemiesAlive <= 0 && enemiesRemainingInWave <= 0 && isSpawning)
        {
            // Wave complete
            StartCoroutine(WaveCompleteDelay());
        }
    }

    private void StartNextWave()
    {
        if (!isSpawning) return;

        currentWave++;
        int enemyCount = Mathf.Min(baseEnemiesPerWave + (currentWave - 1) * enemiesPerWaveIncrease, maxEnemiesPerWave);
        enemiesRemainingInWave = enemyCount;
        enemiesAlive = 0;

        GameManager.Instance?.OnWaveStart(currentWave);

        // Adjust spawn rate for difficulty
        float adjustedSpawnTime = Mathf.Max(0.4f, timeBetweenSpawns - currentWave * 0.05f);
        spawnCoroutine = StartCoroutine(SpawnWave(enemyCount, adjustedSpawnTime));
    }

    private IEnumerator SpawnWave(int count, float spawnInterval)
    {
        // Brief pause before wave starts
        yield return new WaitForSeconds(1.5f);

        for (int i = 0; i < count; i++)
        {
            if (!isSpawning) yield break;

            SpawnEnemy();
            enemiesRemainingInWave--;

            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private IEnumerator WaveCompleteDelay()
    {
        GameManager.Instance?.OnWaveComplete(currentWave);
        yield return new WaitForSeconds(timeBetweenWaves);

        if (isSpawning)
        {
            StartNextWave();
        }
    }

    private void SpawnEnemy()
    {
        if (enemyPrefab == null) return;

        float xPos = Random.Range(-spawnXRange, spawnXRange);
        Vector3 spawnPos = new Vector3(xPos, spawnYPosition, 0f);

        GameObject enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
        EnemyController ec = enemy.GetComponent<EnemyController>();

        if (ec != null)
        {
            EnemyController.EnemyType type = ChooseEnemyType();
            float difficulty = (currentWave - 1) * 0.5f;
            ec.Configure(type, difficulty);
        }

        // Track enemy destruction for wave progression
        HealthSystem hs = enemy.GetComponent<HealthSystem>();
        if (hs != null)
        {
            hs.OnDeath += OnEnemyDestroyed;
        }

        // Also handle case where enemy goes off-screen (no death event)
        var tracker = enemy.AddComponent<EnemyDestroyTracker>();
        tracker.Initialize(this);

        enemiesAlive++;
    }

    private EnemyController.EnemyType ChooseEnemyType()
    {
        float roll = Random.value;

        if (currentWave <= 2)
        {
            // Early waves: mostly basic enemies
            if (roll < 0.7f) return EnemyController.EnemyType.Basic;
            if (roll < 0.9f) return EnemyController.EnemyType.Fast;
            return EnemyController.EnemyType.Zigzag;
        }
        else if (currentWave <= 5)
        {
            // Mid waves: introduce zigzag and tanks
            if (roll < 0.35f) return EnemyController.EnemyType.Basic;
            if (roll < 0.55f) return EnemyController.EnemyType.Zigzag;
            if (roll < 0.8f) return EnemyController.EnemyType.Fast;
            return EnemyController.EnemyType.Tank;
        }
        else
        {
            // Late waves: all types, more tanks
            if (roll < 0.25f) return EnemyController.EnemyType.Basic;
            if (roll < 0.45f) return EnemyController.EnemyType.Zigzag;
            if (roll < 0.65f) return EnemyController.EnemyType.Fast;
            return EnemyController.EnemyType.Tank;
        }
    }
}

/// <summary>
/// Helper component to track enemy destruction when going off-screen
/// (enemies destroyed by going off-screen don't trigger OnDeath).
/// </summary>
public class EnemyDestroyTracker : MonoBehaviour
{
    private EnemySpawner spawner;
    private bool tracked = false;

    public void Initialize(EnemySpawner spawner)
    {
        this.spawner = spawner;
    }

    private void OnDestroy()
    {
        // Only notify spawner if not already tracked via death event
        HealthSystem hs = GetComponent<HealthSystem>();
        if (hs != null && hs.IsAlive && spawner != null)
        {
            spawner.OnEnemyDestroyed();
        }
    }
}

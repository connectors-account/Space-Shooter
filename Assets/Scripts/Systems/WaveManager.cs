using UnityEngine;
using System.Collections;

/// <summary>
/// WaveManager handles wave progression, difficulty scaling,
/// and enemy spawn patterns for each wave.
/// </summary>
public class WaveManager : MonoBehaviour
{
    // Singleton instance
    public static WaveManager Instance { get; private set; }

    [Header("Wave Settings")]
    [SerializeField] private float timeBetweenWaves = 5f;
    [SerializeField] private float spawnInterval = 1.5f;
    [SerializeField] private int baseEnemiesPerWave = 5;
    [SerializeField] private int enemiesPerWaveIncrease = 2;
    [SerializeField] private int bossWaveInterval = 5; // Boss appears every X waves

    [Header("Difficulty Scaling")]
    [SerializeField] private float spawnIntervalDecrease = 0.1f;
    [SerializeField] private float minSpawnInterval = 0.5f;

    // Wave state
    private int currentWave = 0;
    private int enemiesRemainingToSpawn;
    private int enemiesAlive;
    private bool isSpawning;
    private bool isBossWave;
    private bool bossActive;

    // Events
    public delegate void WaveEvent(int waveNumber);
    public static event WaveEvent OnWaveStart;
    public static event WaveEvent OnWaveComplete;
    public static event System.Action OnBossWaveStart;

    public int CurrentWave => currentWave;
    public bool IsSpawning => isSpawning;
    public bool IsBossWave => isBossWave;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void OnEnable()
    {
        Enemy.OnEnemyKilled += OnEnemyKilled;
    }

    private void OnDisable()
    {
        Enemy.OnEnemyKilled -= OnEnemyKilled;
    }

    /// <summary>
    /// Start the wave system
    /// </summary>
    public void StartWaves()
    {
        currentWave = 0;
        isSpawning = false;
        bossActive = false;
        StartCoroutine(WaveLoop());
    }

    /// <summary>
    /// Stop all wave activity
    /// </summary>
    public void StopWaves()
    {
        StopAllCoroutines();
        isSpawning = false;
        
        if (EnemySpawner.Instance != null)
        {
            EnemySpawner.Instance.ClearAllEnemies();
        }
    }

    /// <summary>
    /// Main wave loop coroutine
    /// </summary>
    private IEnumerator WaveLoop()
    {
        // Initial delay before first wave
        yield return new WaitForSeconds(2f);

        while (true)
        {
            // Check if game is over
            if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
            {
                yield break;
            }

            // Start next wave
            currentWave++;
            
            // Check if this is a boss wave
            isBossWave = (currentWave % bossWaveInterval == 0);

            // Notify listeners
            OnWaveStart?.Invoke(currentWave);

            if (isBossWave)
            {
                yield return StartCoroutine(SpawnBossWave());
            }
            else
            {
                yield return StartCoroutine(SpawnWave());
            }

            // Wait for all enemies to be defeated
            yield return new WaitUntil(() => enemiesAlive <= 0 && !bossActive);

            // Wave complete
            OnWaveComplete?.Invoke(currentWave);

            // Award wave completion bonus
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.AddWaveBonus(currentWave);
            }

            // Wait between waves
            yield return new WaitForSeconds(timeBetweenWaves);
        }
    }

    /// <summary>
    /// Spawn enemies for the current wave
    /// </summary>
    private IEnumerator SpawnWave()
    {
        isSpawning = true;

        // Calculate enemies for this wave
        enemiesRemainingToSpawn = baseEnemiesPerWave + (currentWave - 1) * enemiesPerWaveIncrease;
        enemiesAlive = enemiesRemainingToSpawn;

        // Calculate spawn interval (decreases with wave)
        float currentSpawnInterval = Mathf.Max(
            minSpawnInterval,
            spawnInterval - (currentWave - 1) * spawnIntervalDecrease
        );

        while (enemiesRemainingToSpawn > 0)
        {
            // Pause check
            while (GameManager.Instance != null && GameManager.Instance.IsPaused)
            {
                yield return null;
            }

            // Spawn pattern based on wave number
            if (currentWave >= 3 && enemiesRemainingToSpawn >= 3 && Random.value < 0.3f)
            {
                // Formation spawn
                SpawnFormation();
            }
            else
            {
                // Single enemy spawn
                SpawnSingleEnemy();
            }

            yield return new WaitForSeconds(currentSpawnInterval);
        }

        isSpawning = false;
    }

    /// <summary>
    /// Spawn a single enemy based on wave difficulty
    /// </summary>
    private void SpawnSingleEnemy()
    {
        if (EnemySpawner.Instance == null) return;

        // Adjust spawn weights based on wave
        if (currentWave <= 2)
        {
            // Early waves - mostly small enemies
            EnemySpawner.Instance.SpawnEnemy("SmallEnemy");
        }
        else if (currentWave <= 5)
        {
            // Mid waves - mix of small and medium
            string[] types = { "SmallEnemy", "MediumEnemy" };
            EnemySpawner.Instance.SpawnEnemy(types[Random.Range(0, types.Length)]);
        }
        else
        {
            // Later waves - all enemy types
            EnemySpawner.Instance.SpawnRandomEnemy();
        }

        enemiesRemainingToSpawn--;
    }

    /// <summary>
    /// Spawn a formation of enemies
    /// </summary>
    private void SpawnFormation()
    {
        if (EnemySpawner.Instance == null) return;

        int formationSize = Mathf.Min(3, enemiesRemainingToSpawn);
        EnemySpawner.Instance.SpawnFormation("SmallEnemy", formationSize, 1.5f);
        enemiesRemainingToSpawn -= formationSize;
    }

    /// <summary>
    /// Spawn boss wave
    /// </summary>
    private IEnumerator SpawnBossWave()
    {
        OnBossWaveStart?.Invoke();
        
        isSpawning = true;
        bossActive = true;
        
        // Spawn some minions first
        enemiesRemainingToSpawn = 3;
        enemiesAlive = enemiesRemainingToSpawn + 1; // +1 for boss

        for (int i = 0; i < 3; i++)
        {
            EnemySpawner.Instance.SpawnEnemy("SmallEnemy");
            enemiesRemainingToSpawn--;
            yield return new WaitForSeconds(0.5f);
        }

        // Wait a moment then spawn boss
        yield return new WaitForSeconds(1f);
        EnemySpawner.Instance.SpawnBoss();

        isSpawning = false;
    }

    /// <summary>
    /// Called when boss is defeated
    /// </summary>
    public void OnBossDefeated()
    {
        bossActive = false;
        Debug.Log("Boss defeated!");
    }

    /// <summary>
    /// Called when any enemy is killed
    /// </summary>
    private void OnEnemyKilled(int scoreValue)
    {
        enemiesAlive--;
        enemiesAlive = Mathf.Max(0, enemiesAlive);
    }

    /// <summary>
    /// Reset wave manager state
    /// </summary>
    public void Reset()
    {
        StopWaves();
        currentWave = 0;
        enemiesAlive = 0;
        enemiesRemainingToSpawn = 0;
        isSpawning = false;
        isBossWave = false;
        bossActive = false;
    }
}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Wave-based enemy spawner. Progressively increases difficulty by
/// adding more enemies, faster enemies, and harder movement patterns.
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy Prefabs")]
    [Tooltip("Array of enemy prefabs, indexed by difficulty tier (0 = basic)")]
    [SerializeField] private GameObject[] enemyPrefabs;

    [Header("Spawn Settings")]
    [SerializeField] private float spawnYPosition = 7f;
    [SerializeField] private float spawnXRange = 7f;
    [SerializeField] private float timeBetweenSpawns = 1.2f;
    [SerializeField] private float timeBetweenWaves = 4f;

    [Header("Wave Scaling")]
    [SerializeField] private int baseEnemiesPerWave = 5;
    [SerializeField] private int enemiesPerWaveIncrease = 2;
    [SerializeField] private float spawnRateDecreasePerWave = 0.05f;
    [SerializeField] private float minTimeBetweenSpawns = 0.3f;
    [SerializeField] private float speedMultiplierPerWave = 0.08f;

    // ── State ───────────────────────────────────────────────────────────
    private int _currentWave;
    private int _enemiesAlive;
    private bool _spawning;

    // Track spawned enemies so we know when a wave is clear
    private readonly List<GameObject> _activeEnemies = new List<GameObject>();

    // ────────────────────────────────────────────────────────────────────
    // Unity Lifecycle
    // ────────────────────────────────────────────────────────────────────
    private void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStateChanged += OnGameStateChanged;
        }
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnGameStateChanged -= OnGameStateChanged;
    }

    // ────────────────────────────────────────────────────────────────────
    // Public API
    // ────────────────────────────────────────────────────────────────────

    /// <summary>Begin spawning waves. Call once when game starts.</summary>
    public void BeginSpawning()
    {
        _currentWave = 0;
        _spawning = true;
        StartCoroutine(WaveLoop());
    }

    /// <summary>Stop all spawning immediately.</summary>
    public void StopSpawning()
    {
        _spawning = false;
        StopAllCoroutines();
    }

    /// <summary>Destroy all currently alive enemies.</summary>
    public void ClearAllEnemies()
    {
        foreach (var enemy in _activeEnemies)
        {
            if (enemy != null)
                Destroy(enemy);
        }
        _activeEnemies.Clear();
        _enemiesAlive = 0;
    }

    // ────────────────────────────────────────────────────────────────────
    // Wave Logic
    // ────────────────────────────────────────────────────────────────────
    private IEnumerator WaveLoop()
    {
        // Small delay before first wave
        yield return new WaitForSeconds(2f);

        while (_spawning)
        {
            _currentWave++;
            GameManager.Instance?.AdvanceWave();

            int enemyCount = baseEnemiesPerWave + (_currentWave - 1) * enemiesPerWaveIncrease;
            float currentSpawnRate = Mathf.Max(
                minTimeBetweenSpawns,
                timeBetweenSpawns - (_currentWave - 1) * spawnRateDecreasePerWave);
            float speedMult = 1f + (_currentWave - 1) * speedMultiplierPerWave;

            yield return StartCoroutine(SpawnWave(enemyCount, currentSpawnRate, speedMult));

            // Wait until wave is cleared
            yield return new WaitUntil(() => AreAllEnemiesDead());

            // Breather between waves
            yield return new WaitForSeconds(timeBetweenWaves);
        }
    }

    private IEnumerator SpawnWave(int count, float spawnInterval, float speedMultiplier)
    {
        for (int i = 0; i < count; i++)
        {
            if (!_spawning) yield break;

            SpawnEnemy(speedMultiplier);
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void SpawnEnemy(float speedMultiplier)
    {
        if (enemyPrefabs == null || enemyPrefabs.Length == 0) return;

        // Pick prefab tier based on wave
        int maxTier = Mathf.Min(_currentWave / 3, enemyPrefabs.Length - 1);
        int tier = Random.Range(0, maxTier + 1);
        GameObject prefab = enemyPrefabs[tier];
        if (prefab == null) return;

        // Random X position
        float x = Random.Range(-spawnXRange, spawnXRange);
        Vector3 pos = new Vector3(x, spawnYPosition, 0f);

        GameObject enemy = Instantiate(prefab, pos, Quaternion.identity);

        // Assign movement pattern based on tier / randomness
        EnemyController ec = enemy.GetComponent<EnemyController>();
        if (ec != null)
        {
            var patterns = System.Enum.GetValues(typeof(EnemyController.MovementPattern));
            var selectedPattern = (EnemyController.MovementPattern)patterns
                .GetValue(Random.Range(0, patterns.Length));
            ec.Configure(selectedPattern, speedMultiplier);
        }

        _activeEnemies.Add(enemy);
        _enemiesAlive++;
    }

    private bool AreAllEnemiesDead()
    {
        _activeEnemies.RemoveAll(e => e == null);
        _enemiesAlive = _activeEnemies.Count;
        return _enemiesAlive == 0;
    }

    // ────────────────────────────────────────────────────────────────────
    // State listener
    // ────────────────────────────────────────────────────────────────────
    private void OnGameStateChanged(GameManager.GameState newState)
    {
        if (newState == GameManager.GameState.GameOver)
        {
            StopSpawning();
        }
        else if (newState == GameManager.GameState.Playing && !_spawning)
        {
            BeginSpawning();
        }
    }
}

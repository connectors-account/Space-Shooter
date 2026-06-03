using System.Collections;
using UnityEngine;

/// <summary>
/// Spawns enemies in waves.
///   - Each wave spawns a number of enemies, one at a time, at random X positions
///     along the top of the screen.
///   - The number of enemies increases each wave, and the delay between spawns
///     shrinks, ramping up difficulty.
///   - There is a rest period between waves.
/// Attach this to an empty GameObject in the scene.
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy Prefab")]
    [Tooltip("The enemy prefab to spawn.")]
    public GameObject enemyPrefab;

    [Header("Wave Settings")]
    [Tooltip("Number of enemies in the first wave.")]
    public int baseEnemiesPerWave = 4;

    [Tooltip("Extra enemies added to each subsequent wave.")]
    public int enemiesAddedPerWave = 2;

    [Tooltip("Seconds between individual enemy spawns within a wave.")]
    public float spawnInterval = 1f;

    [Tooltip("Minimum spawn interval as difficulty ramps up.")]
    public float minSpawnInterval = 0.3f;

    [Tooltip("How much the spawn interval shrinks each wave.")]
    public float spawnIntervalDecay = 0.1f;

    [Tooltip("Rest time (seconds) between waves.")]
    public float timeBetweenWaves = 3f;

    [Tooltip("Maximum number of waves. Set to 0 for endless waves.")]
    public int maxWaves = 0;

    [Header("Spawn Area")]
    [Tooltip("Horizontal half-width of the spawn area (X range).")]
    public float spawnRangeX = 7.5f;

    [Tooltip("Y position where enemies appear (above the visible screen top).")]
    public float spawnY = 6f;

    // Runtime tracking of the current wave number.
    private int currentWave;

    /// <summary>
    /// Start kicks off the continuous wave-spawning coroutine.
    /// </summary>
    private void Start()
    {
        if (enemyPrefab == null)
        {
            Debug.LogWarning("EnemySpawner: No enemy prefab assigned!");
            return;
        }

        StartCoroutine(SpawnWaves());
    }

    /// <summary>
    /// Coroutine that loops through waves until the game ends or maxWaves is hit.
    /// </summary>
    private IEnumerator SpawnWaves()
    {
        while (true)
        {
            // Stop spawning if the game is over.
            if (GameManager.Instance != null &&
                GameManager.Instance.State != GameManager.GameState.Playing)
                yield break;

            currentWave++;

            // Tell the UI which wave we're on (optional).
            if (UIManager.Instance != null)
                UIManager.Instance.UpdateWave(currentWave);

            // Number of enemies grows with each wave.
            int enemyCount = baseEnemiesPerWave + (currentWave - 1) * enemiesAddedPerWave;

            // Interval shrinks each wave but never below the minimum.
            float interval = Mathf.Max(
                minSpawnInterval,
                spawnInterval - (currentWave - 1) * spawnIntervalDecay);

            // Spawn each enemy in the wave with a delay between them.
            for (int i = 0; i < enemyCount; i++)
            {
                // Abort mid-wave if the game ends.
                if (GameManager.Instance != null &&
                    GameManager.Instance.State != GameManager.GameState.Playing)
                    yield break;

                SpawnEnemy();
                yield return new WaitForSeconds(interval);
            }

            // Stop if we've reached the wave cap.
            if (maxWaves > 0 && currentWave >= maxWaves)
            {
                // Optionally trigger a win when all waves are cleared.
                // We wait a moment to let remaining enemies be dealt with.
                yield return new WaitForSeconds(timeBetweenWaves);
                if (GameManager.Instance != null &&
                    GameManager.Instance.State == GameManager.GameState.Playing)
                    GameManager.Instance.WinGame();
                yield break;
            }

            // Rest before starting the next wave.
            yield return new WaitForSeconds(timeBetweenWaves);
        }
    }

    /// <summary>
    /// Instantiates one enemy at a random X position along the top.
    /// </summary>
    private void SpawnEnemy()
    {
        float x = Random.Range(-spawnRangeX, spawnRangeX);
        Vector3 spawnPos = new Vector3(x, spawnY, 0f);
        Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
    }
}

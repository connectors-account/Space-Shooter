using System.Collections;
using UnityEngine;

/// <summary>
/// Spawns enemies in escalating waves. Each wave spawns a number of enemies that
/// grows with the wave index; difficulty scales by tightening the spawn interval.
/// When all enemies of a wave are cleared, the next wave starts after a short delay.
/// </summary>
public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance { get; private set; }

    [Header("Prefabs")]
    [Tooltip("Enemy prefab(s) to spawn. A random one is chosen each spawn.")]
    public GameObject[] enemyPrefabs;

    [Header("Wave Settings")]
    [Tooltip("Number of enemies in the first wave.")]
    public int baseEnemiesPerWave = 5;

    [Tooltip("Additional enemies added each subsequent wave.")]
    public int enemiesIncrementPerWave = 2;

    [Tooltip("Seconds between individual enemy spawns within a wave.")]
    public float spawnInterval = 1.0f;

    [Tooltip("Minimum spawn interval as waves get harder.")]
    public float minSpawnInterval = 0.25f;

    [Tooltip("How much the spawn interval shrinks each wave.")]
    public float spawnIntervalDecrement = 0.07f;

    [Tooltip("Delay between clearing a wave and starting the next one.")]
    public float timeBetweenWaves = 2.5f;

    [Header("Spawn Area")]
    [Tooltip("Horizontal half-width of the spawn band (world units from center).")]
    public float spawnHalfWidth = 7f;

    [Tooltip("Y position (world units) where enemies appear, above the screen.")]
    public float spawnY = 6f;

    private int enemiesAlive = 0;
    private int enemiesLeftToSpawn = 0;
    private bool spawning = false;
    private Coroutine spawnRoutine;

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
    /// Kicks off the wave loop. Called by GameManager when a game starts.
    /// </summary>
    public void BeginSpawning()
    {
        StopSpawning();
        spawning = true;
        spawnRoutine = StartCoroutine(WaveLoop());
    }

    /// <summary>
    /// Halts all spawning (game over / restart).
    /// </summary>
    public void StopSpawning()
    {
        spawning = false;
        if (spawnRoutine != null)
        {
            StopCoroutine(spawnRoutine);
            spawnRoutine = null;
        }
    }

    private IEnumerator WaveLoop()
    {
        while (spawning)
        {
            // Advance to the next wave.
            GameManager.Instance?.NextWave();
            int currentWave = GameManager.Instance != null ? GameManager.Instance.Wave : 1;

            int enemyCount = baseEnemiesPerWave + enemiesIncrementPerWave * (currentWave - 1);
            float currentInterval = Mathf.Max(minSpawnInterval,
                spawnInterval - spawnIntervalDecrement * (currentWave - 1));

            enemiesLeftToSpawn = enemyCount;
            enemiesAlive = 0;

            // Spawn all enemies for this wave.
            for (int i = 0; i < enemyCount; i++)
            {
                if (!spawning)
                {
                    yield break;
                }

                SpawnSingleEnemy();
                enemiesLeftToSpawn--;
                yield return new WaitForSeconds(currentInterval);
            }

            // Wait until every enemy of this wave has been cleared.
            while (spawning && enemiesAlive > 0)
            {
                yield return null;
            }

            // Short breather before the next wave.
            yield return new WaitForSeconds(timeBetweenWaves);
        }
    }

    private void SpawnSingleEnemy()
    {
        if (enemyPrefabs == null || enemyPrefabs.Length == 0)
        {
            return;
        }

        GameObject prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
        float x = Random.Range(-spawnHalfWidth, spawnHalfWidth);
        Vector3 spawnPos = new Vector3(x, spawnY, 0f);

        Instantiate(prefab, spawnPos, Quaternion.identity);
        enemiesAlive++;
    }

    /// <summary>
    /// Called by enemies when they are destroyed/despawned so the manager can
    /// track when a wave is fully cleared.
    /// </summary>
    public void NotifyEnemyRemoved()
    {
        enemiesAlive = Mathf.Max(0, enemiesAlive - 1);
    }
}

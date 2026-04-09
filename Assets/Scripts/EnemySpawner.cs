using UnityEngine;

/// <summary>
/// Spawns enemies at random x-positions along the top of the screen
/// at a configurable interval. Difficulty increases over time.
/// Attach to an empty GameObject in the scene.
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [Header("Spawning")]
    [Tooltip("Enemy prefab to spawn.")]
    public GameObject enemyPrefab;

    [Tooltip("Starting interval between spawns (seconds).")]
    public float spawnInterval = 2f;

    [Tooltip("Minimum spawn interval (difficulty cap).")]
    public float minSpawnInterval = 0.5f;

    [Tooltip("How much faster spawning gets each second.")]
    public float difficultyRamp = 0.02f;

    [Header("Spawn Area")]
    [Tooltip("Y position where enemies spawn.")]
    public float spawnY = 6f;

    [Tooltip("Random x-range for spawning.")]
    public float spawnXRange = 8f;

    [Header("Power-Ups")]
    [Tooltip("Health power-up prefab (optional).")]
    public GameObject powerUpPrefab;

    [Tooltip("Chance (0-1) of spawning a power-up instead of an enemy.")]
    [Range(0f, 1f)]
    public float powerUpChance = 0.08f;

    // Internal timer
    private float timer;
    private float currentInterval;

    void Start()
    {
        currentInterval = spawnInterval;
        timer = currentInterval; // spawn one immediately-ish
    }

    void Update()
    {
        // Don't spawn if the game is over
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver) return;

        timer += Time.deltaTime;

        if (timer >= currentInterval)
        {
            SpawnObject();
            timer = 0f;

            // Gradually increase difficulty
            currentInterval = Mathf.Max(minSpawnInterval,
                                        currentInterval - difficultyRamp * Time.deltaTime);
        }

        // Slow continuous ramp outside of spawn events too
        currentInterval = Mathf.Max(minSpawnInterval,
                                    currentInterval - difficultyRamp * Time.deltaTime * 0.1f);
    }

    /// <summary>
    /// Spawn an enemy or (rarely) a power-up at a random position along the top.
    /// </summary>
    void SpawnObject()
    {
        float randomX = Random.Range(-spawnXRange, spawnXRange);
        Vector3 spawnPos = new Vector3(randomX, spawnY, 0f);

        // Small chance to spawn a power-up instead
        if (powerUpPrefab != null && Random.value < powerUpChance)
        {
            Instantiate(powerUpPrefab, spawnPos, Quaternion.identity);
        }
        else if (enemyPrefab != null)
        {
            Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
        }
    }
}

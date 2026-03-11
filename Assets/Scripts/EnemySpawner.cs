using UnityEngine;

/// <summary>
/// Spawns enemy ships at random X positions along the top of the screen.
/// Attach this to an empty "EnemySpawner" GameObject in the scene.
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [Header("Spawning Settings")]
    [Tooltip("Enemy prefab to spawn.")]
    public GameObject enemyPrefab;

    [Tooltip("Starting interval between spawns (seconds).")]
    public float spawnInterval = 1.5f;

    [Tooltip("Minimum spawn interval (game gets harder over time).")]
    public float minSpawnInterval = 0.4f;

    [Tooltip("How much the interval decreases each spawn (difficulty ramp).")]
    public float intervalDecrement = 0.005f;

    [Header("Spawn Area")]
    [Tooltip("Y position where enemies appear.")]
    public float spawnY = 7f;

    [Tooltip("Horizontal range for random spawn X position.")]
    public float spawnXRange = 7.5f;

    // ---- Internal ----
    private float timer;
    private bool spawning = true;

    // =========================================================================
    // Unity Lifecycle
    // =========================================================================

    private void Start()
    {
        timer = spawnInterval;
    }

    private void Update()
    {
        if (!spawning) return;

        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            SpawnEnemy();

            // Gradually increase difficulty
            spawnInterval = Mathf.Max(minSpawnInterval, spawnInterval - intervalDecrement);
            timer = spawnInterval;
        }
    }

    // =========================================================================
    // Spawning
    // =========================================================================

    /// <summary>
    /// Creates one enemy at a random X position along the top.
    /// </summary>
    private void SpawnEnemy()
    {
        if (enemyPrefab == null)
        {
            Debug.LogWarning("EnemySpawner: enemyPrefab is not assigned!");
            return;
        }

        float randomX = Random.Range(-spawnXRange, spawnXRange);
        Vector3 spawnPos = new Vector3(randomX, spawnY, 0f);
        Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
    }

    // =========================================================================
    // Public Control
    // =========================================================================

    /// <summary>
    /// Stops spawning (called by GameManager on Game Over).
    /// </summary>
    public void StopSpawning()
    {
        spawning = false;
    }
}

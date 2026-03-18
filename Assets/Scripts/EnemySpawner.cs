using UnityEngine;

/// <summary>
/// Spawns enemy waves at random X positions above the screen.
/// Attach this to an empty GameObject in the scene.
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [Header("Spawning")]
    public GameObject[] enemyPrefabs;  // Array of enemy prefabs for variety
    public float initialSpawnRate = 2f;
    public float minimumSpawnRate = 0.5f;
    public float spawnRateDecrease = 0.05f; // Gets faster over time

    [Header("Spawn Area")]
    public float spawnYPosition = 6.5f;
    public float spawnXRange = 7f;

    private float currentSpawnRate;
    private float nextSpawnTime;

    void Start()
    {
        currentSpawnRate = initialSpawnRate;
        nextSpawnTime = Time.time + 1f; // Short delay before first spawn
    }

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.isGameOver)
            return;

        if (Time.time >= nextSpawnTime)
        {
            SpawnEnemy();

            // Gradually increase difficulty by reducing spawn interval
            currentSpawnRate = Mathf.Max(minimumSpawnRate, currentSpawnRate - spawnRateDecrease);
            nextSpawnTime = Time.time + currentSpawnRate;
        }
    }

    /// <summary>
    /// Instantiates a random enemy prefab at a random X position above the screen.
    /// </summary>
    void SpawnEnemy()
    {
        if (enemyPrefabs == null || enemyPrefabs.Length == 0)
        {
            Debug.LogWarning("EnemySpawner: No enemy prefabs assigned!");
            return;
        }

        float randomX = Random.Range(-spawnXRange, spawnXRange);
        Vector3 spawnPosition = new Vector3(randomX, spawnYPosition, 0f);

        int index = Random.Range(0, enemyPrefabs.Length);
        Instantiate(enemyPrefabs[index], spawnPosition, Quaternion.identity);
    }
}

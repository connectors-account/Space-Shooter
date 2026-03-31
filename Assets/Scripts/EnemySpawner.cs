using UnityEngine;

/// <summary>
/// EnemySpawner periodically creates enemy ships at random horizontal positions
/// above the visible screen area.
/// Attach this script to an empty "EnemySpawner" GameObject in the scene.
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [Header("Spawning")]
    [Tooltip("The enemy prefab to instantiate")]
    public GameObject enemyPrefab;

    [Tooltip("Seconds between enemy spawns")]
    public float spawnInterval = 1.5f;

    [Tooltip("Minimum seconds between spawns (difficulty ramps down to this)")]
    public float minSpawnInterval = 0.4f;

    [Tooltip("How much the interval decreases every spawn")]
    public float difficultyRamp = 0.01f;

    [Header("Spawn Area")]
    [Tooltip("How far left/right enemies can spawn")]
    public float horizontalRange = 7.5f;

    [Tooltip("Y position where enemies appear (above camera view)")]
    public float spawnY = 6.5f;

    // Internal timer
    private float timer;

    void Update()
    {
        // Don't spawn when the game is over
        if (GameManager.Instance != null && GameManager.Instance.isGameOver)
            return;

        timer += Time.deltaTime;

        if (timer >= spawnInterval)
        {
            SpawnEnemy();
            timer = 0f;

            // Gradually increase difficulty
            if (spawnInterval > minSpawnInterval)
            {
                spawnInterval -= difficultyRamp;
            }
        }
    }

    /// <summary>
    /// Creates one enemy at a random X position along the top of the screen.
    /// </summary>
    private void SpawnEnemy()
    {
        if (enemyPrefab == null) return;

        float randomX = Random.Range(-horizontalRange, horizontalRange);
        Vector3 spawnPos = new Vector3(randomX, spawnY, 0f);

        Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
    }
}

using UnityEngine;

/// <summary>
/// Spawns enemies at intervals from the top of the play area.
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn")]
    [SerializeField] private GameObject[] enemyPrefabs;
    [SerializeField] private float spawnInterval = 1.5f;
    [SerializeField] private float minimumInterval = 0.5f;
    [SerializeField] private float intervalDecreasePerStep = 0.05f;
    [SerializeField] private float difficultyStepSeconds = 12f;

    [Header("Spawn Area")]
    [SerializeField] private float spawnY = 6f;
    [SerializeField] private float minSpawnX = -8f;
    [SerializeField] private float maxSpawnX = 8f;

    private float nextSpawnTime;
    private float nextDifficultyTime;

    private void Start()
    {
        nextSpawnTime = Time.time + spawnInterval;
        nextDifficultyTime = Time.time + difficultyStepSeconds;
    }

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
            return;

        if (Time.time >= nextSpawnTime)
        {
            SpawnEnemy();
            nextSpawnTime = Time.time + spawnInterval;
        }

        if (Time.time >= nextDifficultyTime)
        {
            spawnInterval = Mathf.Max(minimumInterval, spawnInterval - intervalDecreasePerStep);
            nextDifficultyTime = Time.time + difficultyStepSeconds;
        }
    }

    private void SpawnEnemy()
    {
        if (enemyPrefabs == null || enemyPrefabs.Length == 0)
        {
            Debug.LogWarning("EnemySpawner: no enemy prefabs assigned.");
            return;
        }

        int index = Random.Range(0, enemyPrefabs.Length);
        GameObject prefab = enemyPrefabs[index];
        if (prefab == null)
        {
            Debug.LogWarning("EnemySpawner: prefab entry is null.");
            return;
        }

        Vector3 spawnPos = new Vector3(Random.Range(minSpawnX, maxSpawnX), spawnY, 0f);
        Instantiate(prefab, spawnPos, Quaternion.identity);
    }
}

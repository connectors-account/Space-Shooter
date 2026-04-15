using System.Collections;
using UnityEngine;

/// <summary>
/// Spawns enemies in repeated waves.
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private float spawnY = 6f;
    [SerializeField] private float minX = -8f;
    [SerializeField] private float maxX = 8f;

    [Header("Wave Settings")]
    [SerializeField] private int enemiesPerWave = 5;
    [SerializeField] private float timeBetweenEnemies = 0.4f;
    [SerializeField] private float timeBetweenWaves = 2f;

    private void Start()
    {
        StartCoroutine(SpawnRoutine());
    }

    private IEnumerator SpawnRoutine()
    {
        while (true)
        {
            if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
            {
                yield return null;
                continue;
            }

            for (int i = 0; i < enemiesPerWave; i++)
            {
                SpawnEnemy();
                yield return new WaitForSeconds(timeBetweenEnemies);
            }

            yield return new WaitForSeconds(timeBetweenWaves);
        }
    }

    private void SpawnEnemy()
    {
        if (enemyPrefab == null)
        {
            Debug.LogWarning("EnemySpawner: enemyPrefab is not assigned.");
            return;
        }

        Vector3 spawnPos = new Vector3(Random.Range(minX, maxX), spawnY, 0f);
        Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
    }
}

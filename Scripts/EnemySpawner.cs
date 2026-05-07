using System.Collections;
using UnityEngine;

/// <summary>
/// Spawns enemies in progressive waves with scaling difficulty.
/// Attach this to an empty GameObject in the gameplay scene.
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject enemyPrefab;

    [Header("Spawn Area")]
    [SerializeField] private float minSpawnX = -8f;
    [SerializeField] private float maxSpawnX = 8f;
    [SerializeField] private float spawnY = 6f;

    [Header("Wave Settings")]
    [SerializeField] private int baseEnemiesPerWave = 6;
    [SerializeField] private int enemiesAddedPerWave = 2;
    [SerializeField] private float initialSpawnInterval = 1.1f;
    [SerializeField] private float spawnIntervalReductionPerWave = 0.08f;
    [SerializeField] private float minimumSpawnInterval = 0.35f;
    [SerializeField] private float timeBetweenWaves = 2.0f;

    [Header("Enemy Scaling")]
    [SerializeField] private float speedMultiplierIncreasePerWave = 0.08f;
    [SerializeField] private int extraHealthEveryNWaves = 3;
    [SerializeField] private int extraScorePerWave = 2;

    private Coroutine spawnRoutine;

    public void BeginSpawning()
    {
        StopSpawning();
        spawnRoutine = StartCoroutine(SpawnWavesRoutine());
    }

    public void StopSpawning()
    {
        if (spawnRoutine != null)
        {
            StopCoroutine(spawnRoutine);
            spawnRoutine = null;
        }
    }

    private IEnumerator SpawnWavesRoutine()
    {
        int wave = 1;

        while (GameManager.Instance != null && GameManager.Instance.IsGameActive)
        {
            GameManager.Instance.SetWave(wave);

            int enemiesThisWave = baseEnemiesPerWave + ((wave - 1) * enemiesAddedPerWave);
            float spawnInterval = Mathf.Max(minimumSpawnInterval, initialSpawnInterval - ((wave - 1) * spawnIntervalReductionPerWave));
            float speedMultiplier = 1f + ((wave - 1) * speedMultiplierIncreasePerWave);
            int extraHealth = extraHealthEveryNWaves > 0 ? (wave - 1) / extraHealthEveryNWaves : 0;
            int extraScore = (wave - 1) * extraScorePerWave;

            for (int i = 0; i < enemiesThisWave; i++)
            {
                if (GameManager.Instance == null || !GameManager.Instance.IsGameActive)
                {
                    yield break;
                }

                SpawnEnemy(speedMultiplier, extraHealth, extraScore);
                yield return new WaitForSeconds(spawnInterval);
            }

            yield return new WaitForSeconds(timeBetweenWaves);
            wave++;
        }

        spawnRoutine = null;
    }

    private void SpawnEnemy(float speedMultiplier, int extraHealth, int extraScore)
    {
        if (enemyPrefab == null)
        {
            Debug.LogWarning("EnemySpawner: enemyPrefab is not assigned.");
            return;
        }

        float randomX = Random.Range(minSpawnX, maxSpawnX);
        Vector3 spawnPosition = new Vector3(randomX, spawnY, 0f);

        GameObject enemyObject = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
        EnemyController enemy = enemyObject.GetComponent<EnemyController>();
        if (enemy != null)
        {
            enemy.Configure(speedMultiplier, extraHealth, extraScore);
        }
    }
}

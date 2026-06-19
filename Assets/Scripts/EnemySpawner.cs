using System.Collections;
using UnityEngine;

/// <summary>
/// Spawns enemies in escalating waves. Each wave spawns a batch of enemies at
/// random horizontal positions just above the top of the screen, waits for a
/// gap, then increases difficulty for the next wave.
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [Header("Prefabs")]
    [Tooltip("Enemy prefabs to choose from when spawning.")]
    public GameObject[] enemyPrefabs;

    [Header("Wave Settings")]
    [Tooltip("Number of enemies in the first wave.")]
    public int baseEnemiesPerWave = 4;
    [Tooltip("Extra enemies added each subsequent wave.")]
    public int enemiesAddedPerWave = 2;
    [Tooltip("Seconds between individual enemy spawns within a wave.")]
    public float spawnInterval = 0.8f;
    [Tooltip("Seconds of rest between waves.")]
    public float timeBetweenWaves = 3f;

    [Header("Spawn Area")]
    [Tooltip("Horizontal padding from the screen edges (world units).")]
    public float horizontalPadding = 0.8f;
    [Tooltip("How far above the top of the screen enemies appear.")]
    public float spawnHeightOffset = 1f;

    private int currentWave;
    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;
        if (enemyPrefabs == null || enemyPrefabs.Length == 0)
        {
            Debug.LogWarning("EnemySpawner has no enemy prefabs assigned.");
            return;
        }
        StartCoroutine(SpawnWaves());
    }

    private IEnumerator SpawnWaves()
    {
        // Infinite escalating waves until the game ends.
        while (true)
        {
            if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
                yield break;

            currentWave++;
            int count = baseEnemiesPerWave + (currentWave - 1) * enemiesAddedPerWave;

            if (UIManager.Instance != null)
                UIManager.Instance.ShowWaveBanner(currentWave);

            for (int i = 0; i < count; i++)
            {
                if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
                    yield break;

                SpawnOneEnemy();
                yield return new WaitForSeconds(spawnInterval);
            }

            yield return new WaitForSeconds(timeBetweenWaves);
        }
    }

    private void SpawnOneEnemy()
    {
        GameObject prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];

        // Compute a random X within the visible screen, then place above the top.
        float leftX = mainCamera.ViewportToWorldPoint(Vector3.zero).x + horizontalPadding;
        float rightX = mainCamera.ViewportToWorldPoint(Vector3.one).x - horizontalPadding;
        float topY = mainCamera.ViewportToWorldPoint(Vector3.one).y + spawnHeightOffset;

        float x = Random.Range(leftX, rightX);
        Vector3 spawnPos = new Vector3(x, topY, 0f);

        Instantiate(prefab, spawnPos, Quaternion.identity);
    }
}

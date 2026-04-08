using UnityEngine;
using System.Collections;

/// <summary>
/// Manages wave-based enemy spawning with increasing difficulty.
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    public static EnemySpawner Instance { get; private set; }

    [Header("Enemy Prefabs")]
    public GameObject straightEnemyPrefab;
    public GameObject zigzagEnemyPrefab;
    public GameObject swooperEnemyPrefab;
    public GameObject tankEnemyPrefab;

    [Header("Spawn Settings")]
    public float spawnYPosition = 6.5f;
    public float spawnXRange = 7f;
    public float minSpawnInterval = 0.5f;
    public float maxSpawnInterval = 2f;

    private bool isSpawning = false;
    private int enemiesToSpawn = 0;
    private float currentDifficulty = 1f;
    private int currentWaveNumber = 0;

    void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// Starts spawning enemies for the given wave.
    /// </summary>
    public void StartWave(int waveNumber, int enemyCount, float difficulty)
    {
        currentWaveNumber = waveNumber;
        enemiesToSpawn = enemyCount;
        currentDifficulty = difficulty;

        if (!isSpawning)
        {
            StartCoroutine(SpawnWaveCoroutine());
        }
    }

    /// <summary>
    /// Coroutine that spawns enemies one at a time with delays.
    /// </summary>
    private IEnumerator SpawnWaveCoroutine()
    {
        isSpawning = true;

        while (enemiesToSpawn > 0)
        {
            SpawnEnemy();
            enemiesToSpawn--;

            // Decrease interval as difficulty increases
            float interval = Mathf.Lerp(maxSpawnInterval, minSpawnInterval, (currentDifficulty - 1f) / 3f);
            interval += Random.Range(-0.3f, 0.3f);
            interval = Mathf.Max(0.3f, interval);

            yield return new WaitForSeconds(interval);
        }

        isSpawning = false;
    }

    /// <summary>
    /// Spawns a single enemy of a random type based on the current wave.
    /// </summary>
    private void SpawnEnemy()
    {
        GameObject prefab = ChooseEnemyType();
        if (prefab == null)
        {
            Debug.LogWarning("No enemy prefab assigned!");
            return;
        }

        float spawnX = Random.Range(-spawnXRange, spawnXRange);
        Vector3 spawnPos = new Vector3(spawnX, spawnYPosition, 0f);

        GameObject enemy = Instantiate(prefab, spawnPos, Quaternion.identity);

        EnemyController ec = enemy.GetComponent<EnemyController>();
        if (ec != null)
        {
            ec.SetDifficulty(currentDifficulty);
        }
    }

    /// <summary>
    /// Selects an enemy type based on wave number for progressive difficulty.
    /// Early waves = simple enemies. Later waves = harder enemy types.
    /// </summary>
    private GameObject ChooseEnemyType()
    {
        float roll = Random.value;

        if (currentWaveNumber <= 2)
        {
            // Waves 1-2: Only straight enemies
            return straightEnemyPrefab;
        }
        else if (currentWaveNumber <= 4)
        {
            // Waves 3-4: Straight + Zigzag
            if (roll < 0.6f) return straightEnemyPrefab;
            else return zigzagEnemyPrefab;
        }
        else if (currentWaveNumber <= 7)
        {
            // Waves 5-7: All types except tank (rare)
            if (roll < 0.35f) return straightEnemyPrefab;
            else if (roll < 0.65f) return zigzagEnemyPrefab;
            else if (roll < 0.9f) return swooperEnemyPrefab;
            else return tankEnemyPrefab;
        }
        else
        {
            // Wave 8+: All types with more tanks
            if (roll < 0.2f) return straightEnemyPrefab;
            else if (roll < 0.4f) return zigzagEnemyPrefab;
            else if (roll < 0.7f) return swooperEnemyPrefab;
            else return tankEnemyPrefab;
        }
    }

    /// <summary>
    /// Stops all spawning (used when game ends).
    /// </summary>
    public void StopSpawning()
    {
        isSpawning = false;
        StopAllCoroutines();
    }
}

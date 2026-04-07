using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// SpawnManager - Handles enemy spawning with wave progression and increasing difficulty.
/// Attach to an empty GameObject in the GamePlay scene.
/// </summary>
public class SpawnManager : MonoBehaviour
{
    [Header("Enemy Prefabs")]
    public GameObject straightEnemyPrefab;
    public GameObject zigzagEnemyPrefab;
    public GameObject chaserEnemyPrefab;

    [Header("Spawn Settings")]
    public float spawnYOffset = 1f;      // How far above screen top to spawn
    public float timeBetweenSpawns = 1.5f;
    public float timeBetweenWaves = 3f;
    public float difficultyMultiplier = 0.85f; // Each wave spawn interval *= this

    [Header("Wave Configuration")]
    public WaveData[] waves;

    private float screenHalfWidth;
    private float screenTop;
    private bool isSpawning;
    private int currentWaveIndex;

    /// <summary>
    /// Defines what enemies appear in each wave.
    /// </summary>
    [System.Serializable]
    public class WaveData
    {
        public string waveName = "Wave";
        public int straightCount = 5;
        public int zigzagCount = 2;
        public int chaserCount = 0;
        public float spawnInterval = 1.5f;
    }

    private void Start()
    {
        Camera cam = Camera.main;
        if (cam != null)
        {
            screenHalfWidth = cam.orthographicSize * cam.aspect;
            screenTop = cam.orthographicSize;
        }

        // Generate default waves if none configured
        if (waves == null || waves.Length == 0)
        {
            waves = GenerateDefaultWaves();
        }

        currentWaveIndex = 0;
        StartCoroutine(SpawnWaves());
    }

    /// <summary>
    /// Generate 5 progressively harder waves.
    /// </summary>
    private WaveData[] GenerateDefaultWaves()
    {
        return new WaveData[]
        {
            new WaveData { waveName = "Wave 1", straightCount = 5, zigzagCount = 0, chaserCount = 0, spawnInterval = 1.8f },
            new WaveData { waveName = "Wave 2", straightCount = 5, zigzagCount = 3, chaserCount = 0, spawnInterval = 1.5f },
            new WaveData { waveName = "Wave 3", straightCount = 6, zigzagCount = 4, chaserCount = 1, spawnInterval = 1.3f },
            new WaveData { waveName = "Wave 4", straightCount = 7, zigzagCount = 5, chaserCount = 2, spawnInterval = 1.1f },
            new WaveData { waveName = "Wave 5", straightCount = 8, zigzagCount = 6, chaserCount = 4, spawnInterval = 0.9f },
        };
    }

    /// <summary>
    /// Main coroutine: spawn each wave, wait for enemies to be cleared, then advance.
    /// </summary>
    private IEnumerator SpawnWaves()
    {
        // Small initial delay
        yield return new WaitForSeconds(1f);

        for (int w = 0; w < waves.Length; w++)
        {
            if (GameManager.Instance != null && GameManager.Instance.IsGameOver) yield break;

            currentWaveIndex = w;
            WaveData wave = waves[w];

            // Notify GameManager of the new wave (wave number is 1-based)
            // GameManager already starts at wave 1 so only advance for wave 2+
            if (w > 0 && GameManager.Instance != null)
            {
                GameManager.Instance.AdvanceWave();
            }

            // Build a spawn list and shuffle it
            List<GameObject> spawnList = BuildSpawnList(wave);
            ShuffleList(spawnList);

            isSpawning = true;

            foreach (GameObject prefab in spawnList)
            {
                if (GameManager.Instance != null && GameManager.Instance.IsGameOver) yield break;

                SpawnEnemy(prefab);
                yield return new WaitForSeconds(wave.spawnInterval);
            }

            isSpawning = false;

            // Wait until all enemies are destroyed before next wave
            yield return StartCoroutine(WaitForEnemiesCleared());

            // Brief pause between waves
            yield return new WaitForSeconds(timeBetweenWaves);
        }

        // All waves complete – player wins
        if (GameManager.Instance != null && !GameManager.Instance.IsGameOver)
        {
            GameManager.Instance.AdvanceWave(); // This triggers win since we exceed totalWaves
        }
    }

    private List<GameObject> BuildSpawnList(WaveData wave)
    {
        List<GameObject> list = new List<GameObject>();

        for (int i = 0; i < wave.straightCount; i++)
            if (straightEnemyPrefab != null) list.Add(straightEnemyPrefab);

        for (int i = 0; i < wave.zigzagCount; i++)
            if (zigzagEnemyPrefab != null) list.Add(zigzagEnemyPrefab);

        for (int i = 0; i < wave.chaserCount; i++)
            if (chaserEnemyPrefab != null) list.Add(chaserEnemyPrefab);

        return list;
    }

    private void SpawnEnemy(GameObject prefab)
    {
        float x = Random.Range(-screenHalfWidth + 1f, screenHalfWidth - 1f);
        float y = screenTop + spawnYOffset;
        Vector3 spawnPos = new Vector3(x, y, 0f);

        Instantiate(prefab, spawnPos, Quaternion.identity);
    }

    private IEnumerator WaitForEnemiesCleared()
    {
        while (true)
        {
            if (GameManager.Instance != null && GameManager.Instance.IsGameOver) yield break;

            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
            if (enemies.Length == 0) yield break;
            yield return new WaitForSeconds(0.5f);
        }
    }

    private void ShuffleList<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            T temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }
    }
}

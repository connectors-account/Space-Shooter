using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Manages enemy wave spawning with progressive difficulty.
/// </summary>
public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance { get; private set; }

    [Header("Enemy Prefabs")]
    [SerializeField] private GameObject basicEnemyPrefab;
    [SerializeField] private GameObject zigzagEnemyPrefab;
    [SerializeField] private GameObject bomberEnemyPrefab;
    [SerializeField] private GameObject eliteEnemyPrefab;

    [Header("Spawn Settings")]
    [SerializeField] private float spawnXMin = -7f;
    [SerializeField] private float spawnXMax = 7f;
    [SerializeField] private float spawnY = 6.5f;
    [SerializeField] private float timeBetweenWaves = 5f;
    [SerializeField] private float spawnInterval = 0.5f;

    [Header("Wave Configuration")]
    [SerializeField] private int baseEnemiesPerWave = 5;
    [SerializeField] private int enemiesPerWaveIncrease = 2;
    [SerializeField] private int maxEnemiesPerWave = 30;

    private int currentWave;
    private int enemiesAlive;
    private int enemiesToSpawn;
    private bool isSpawning;
    private bool waveActive;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        currentWave = 0;
        StartCoroutine(WaveLoop());
    }

    private IEnumerator WaveLoop()
    {
        // Initial delay
        yield return new WaitForSeconds(2f);

        while (true)
        {
            if (GameManager.Instance == null || !GameManager.Instance.IsPlaying)
            {
                yield return new WaitForSeconds(0.5f);
                continue;
            }

            currentWave++;
            GameManager.Instance.OnWaveStarted(currentWave);

            // Calculate enemies for this wave
            enemiesToSpawn = Mathf.Min(
                baseEnemiesPerWave + (currentWave - 1) * enemiesPerWaveIncrease,
                maxEnemiesPerWave
            );
            enemiesAlive = 0;
            waveActive = true;

            // Spawn the wave
            yield return StartCoroutine(SpawnWave());

            // Wait until all enemies are destroyed
            while (enemiesAlive > 0)
            {
                yield return new WaitForSeconds(0.25f);
            }

            waveActive = false;

            // Pause between waves
            float waitTime = Mathf.Max(timeBetweenWaves - currentWave * 0.2f, 2f);
            yield return new WaitForSeconds(waitTime);
        }
    }

    private IEnumerator SpawnWave()
    {
        isSpawning = true;

        for (int i = 0; i < enemiesToSpawn; i++)
        {
            if (GameManager.Instance == null || !GameManager.Instance.IsPlaying)
            {
                isSpawning = false;
                yield break;
            }

            SpawnEnemy();
            enemiesAlive++;

            float interval = Mathf.Max(spawnInterval - currentWave * 0.02f, 0.15f);
            yield return new WaitForSeconds(interval);
        }

        isSpawning = false;
    }

    private void SpawnEnemy()
    {
        float spawnX = Random.Range(spawnXMin, spawnXMax);
        Vector3 spawnPos = new Vector3(spawnX, spawnY, 0f);

        // Choose enemy type based on wave and randomness
        GameObject prefab = ChooseEnemyPrefab();
        if (prefab == null)
        {
            // Fallback: create a basic enemy from whatever prefab is available
            prefab = basicEnemyPrefab ?? zigzagEnemyPrefab ?? bomberEnemyPrefab ?? eliteEnemyPrefab;
        }
        if (prefab == null) return;

        GameObject enemy = Instantiate(prefab, spawnPos, Quaternion.identity);

        // Scale difficulty based on wave
        EnemyController ec = enemy.GetComponent<EnemyController>();
        if (ec != null)
        {
            float diffMult = GameManager.Instance != null ? GameManager.Instance.DifficultyMultiplier : 1f;
            // Enemies get faster and tougher in later waves
            // Config is set on the prefab; we just let the difficulty multiplier affect gameplay
        }
    }

    private GameObject ChooseEnemyPrefab()
    {
        float roll = Random.value;

        if (currentWave <= 2)
        {
            // Early waves: mostly basic
            return basicEnemyPrefab;
        }
        else if (currentWave <= 5)
        {
            // Mid waves: basic + zigzag
            if (roll < 0.6f) return basicEnemyPrefab;
            return zigzagEnemyPrefab ?? basicEnemyPrefab;
        }
        else if (currentWave <= 8)
        {
            // Later waves: mix of types
            if (roll < 0.35f) return basicEnemyPrefab;
            if (roll < 0.65f) return zigzagEnemyPrefab ?? basicEnemyPrefab;
            if (roll < 0.85f) return bomberEnemyPrefab ?? basicEnemyPrefab;
            return eliteEnemyPrefab ?? basicEnemyPrefab;
        }
        else
        {
            // Late game: heavy mix
            if (roll < 0.2f) return basicEnemyPrefab;
            if (roll < 0.45f) return zigzagEnemyPrefab ?? basicEnemyPrefab;
            if (roll < 0.7f) return bomberEnemyPrefab ?? basicEnemyPrefab;
            return eliteEnemyPrefab ?? basicEnemyPrefab;
        }
    }

    public void OnEnemyDestroyed()
    {
        enemiesAlive = Mathf.Max(0, enemiesAlive - 1);
    }

    /// <summary>
    /// Clear all enemies (used on game restart).
    /// </summary>
    public void ClearAllEnemies()
    {
        EnemyController[] enemies = FindObjectsByType<EnemyController>(FindObjectsSortMode.None);
        foreach (var enemy in enemies)
        {
            Destroy(enemy.gameObject);
        }

        BulletController[] bullets = FindObjectsByType<BulletController>(FindObjectsSortMode.None);
        foreach (var bullet in bullets)
        {
            Destroy(bullet.gameObject);
        }

        PowerUpController[] powerUps = FindObjectsByType<PowerUpController>(FindObjectsSortMode.None);
        foreach (var pu in powerUps)
        {
            Destroy(pu.gameObject);
        }

        enemiesAlive = 0;
        currentWave = 0;
        StopAllCoroutines();
        StartCoroutine(WaveLoop());
    }
}

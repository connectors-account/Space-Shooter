using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Manages wave-based enemy spawning with increasing difficulty.
/// Attach to an empty GameObject in the scene.
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy Prefabs")]
    [SerializeField] private GameObject basicEnemyPrefab;
    [SerializeField] private GameObject fastEnemyPrefab;
    [SerializeField] private GameObject tankEnemyPrefab;
    [SerializeField] private GameObject shooterEnemyPrefab;

    [Header("Bullet Prefab")]
    [SerializeField] private GameObject enemyBulletPrefab;

    [Header("Power-Up Prefabs")]
    [SerializeField] private GameObject[] powerUpPrefabs;

    [Header("Spawn Settings")]
    [SerializeField] private float spawnYPosition = 6f;
    [SerializeField] private float spawnXRange = 4f;
    [SerializeField] private float timeBetweenWaves = 3f;
    [SerializeField] private float spawnInterval = 1f;

    // State
    private int currentWave = 0;
    private int enemiesRemainingInWave = 0;
    private int enemiesAlive = 0;
    private bool isSpawning = false;
    private bool isActive = false;

    public int CurrentWave => currentWave;

    /// <summary>
    /// Starts the spawning system.
    /// </summary>
    public void StartSpawning()
    {
        isActive = true;
        currentWave = 0;
        StartCoroutine(SpawnWaveLoop());
    }

    /// <summary>
    /// Stops all spawning.
    /// </summary>
    public void StopSpawning()
    {
        isActive = false;
        StopAllCoroutines();
    }

    /// <summary>
    /// Called by GameManager when an enemy is destroyed.
    /// </summary>
    public void OnEnemyDestroyed()
    {
        enemiesAlive = Mathf.Max(0, enemiesAlive - 1);
    }

    /// <summary>
    /// Main loop: waits between waves, then spawns the next wave.
    /// </summary>
    private IEnumerator SpawnWaveLoop()
    {
        while (isActive)
        {
            yield return new WaitForSeconds(timeBetweenWaves);

            if (!isActive) yield break;

            currentWave++;
            GameManager.Instance?.OnNewWave(currentWave);
            AudioManager.Instance?.PlaySFX("WaveStart");

            yield return StartCoroutine(SpawnWave(currentWave));

            // Wait until all enemies in the wave are defeated
            while (enemiesAlive > 0 && isActive)
            {
                yield return new WaitForSeconds(0.5f);
            }
        }
    }

    /// <summary>
    /// Spawns enemies for a given wave. Difficulty scales with wave number.
    /// </summary>
    private IEnumerator SpawnWave(int wave)
    {
        int baseCount = 3;
        int enemyCount = baseCount + (wave - 1) * 2; // More enemies each wave
        enemyCount = Mathf.Min(enemyCount, 20); // Cap at 20

        float currentSpawnInterval = Mathf.Max(0.3f, spawnInterval - wave * 0.05f);
        float dropChance = Mathf.Min(0.3f, 0.1f + wave * 0.02f);

        isSpawning = true;

        for (int i = 0; i < enemyCount; i++)
        {
            if (!isActive) yield break;

            SpawnEnemy(wave, dropChance);
            yield return new WaitForSeconds(currentSpawnInterval);
        }

        isSpawning = false;
    }

    /// <summary>
    /// Spawns a single enemy with type and stats based on wave difficulty.
    /// </summary>
    private void SpawnEnemy(int wave, float dropChance)
    {
        float xPos = Random.Range(-spawnXRange, spawnXRange);
        Vector3 spawnPos = new Vector3(xPos, spawnYPosition, 0f);

        // Choose enemy type based on wave and random chance
        GameObject prefab = ChooseEnemyPrefab(wave);
        if (prefab == null)
        {
            // Fallback: use basic enemy
            prefab = basicEnemyPrefab;
            if (prefab == null) return;
        }

        GameObject enemy = Instantiate(prefab, spawnPos, Quaternion.identity);
        EnemyController ec = enemy.GetComponent<EnemyController>();

        if (ec != null)
        {
            // Configure enemy based on type and wave
            ConfigureEnemy(ec, prefab, wave, dropChance);
        }

        enemiesAlive++;
    }

    private GameObject ChooseEnemyPrefab(int wave)
    {
        float rand = Random.value;

        if (wave <= 2)
        {
            // Waves 1-2: mostly basic enemies
            return basicEnemyPrefab;
        }
        else if (wave <= 4)
        {
            // Waves 3-4: introduce fast enemies
            if (rand < 0.3f && fastEnemyPrefab != null) return fastEnemyPrefab;
            return basicEnemyPrefab;
        }
        else if (wave <= 6)
        {
            // Waves 5-6: introduce shooters
            if (rand < 0.2f && shooterEnemyPrefab != null) return shooterEnemyPrefab;
            if (rand < 0.5f && fastEnemyPrefab != null) return fastEnemyPrefab;
            return basicEnemyPrefab;
        }
        else
        {
            // Wave 7+: all types, introduce tanks
            if (rand < 0.15f && tankEnemyPrefab != null) return tankEnemyPrefab;
            if (rand < 0.35f && shooterEnemyPrefab != null) return shooterEnemyPrefab;
            if (rand < 0.6f && fastEnemyPrefab != null) return fastEnemyPrefab;
            return basicEnemyPrefab;
        }
    }

    private void ConfigureEnemy(EnemyController ec, GameObject prefab, int wave, float dropChance)
    {
        // Determine stats based on prefab type
        if (prefab == basicEnemyPrefab)
        {
            ec.Setup(
                EnemyController.MovementPattern.StraightDown,
                2f + wave * 0.2f,
                1 + wave / 4,
                100,
                false, 0f, null,
                powerUpPrefabs, dropChance
            );
        }
        else if (prefab == fastEnemyPrefab)
        {
            ec.Setup(
                EnemyController.MovementPattern.Zigzag,
                4f + wave * 0.3f,
                1,
                150,
                false, 0f, null,
                powerUpPrefabs, dropChance
            );
        }
        else if (prefab == tankEnemyPrefab)
        {
            ec.Setup(
                EnemyController.MovementPattern.StraightDown,
                1.5f + wave * 0.1f,
                3 + wave / 3,
                300,
                true, Mathf.Max(1f, 2.5f - wave * 0.1f), enemyBulletPrefab,
                powerUpPrefabs, dropChance * 2f
            );
        }
        else if (prefab == shooterEnemyPrefab)
        {
            ec.Setup(
                EnemyController.MovementPattern.Sine,
                2f + wave * 0.15f,
                2,
                200,
                true, Mathf.Max(0.8f, 2f - wave * 0.1f), enemyBulletPrefab,
                powerUpPrefabs, dropChance
            );
        }
    }

    /// <summary>
    /// Destroys all active enemies (used on game over/restart).
    /// </summary>
    public void ClearAllEnemies()
    {
        foreach (GameObject enemy in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            Destroy(enemy);
        }
        foreach (GameObject bullet in GameObject.FindGameObjectsWithTag("EnemyBullet"))
        {
            Destroy(bullet);
        }
        foreach (GameObject bullet in GameObject.FindGameObjectsWithTag("PlayerBullet"))
        {
            Destroy(bullet);
        }
        enemiesAlive = 0;
    }
}

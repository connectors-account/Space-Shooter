using UnityEngine;
using System.Collections;

/// <summary>
/// SpawnManager - Spawns enemies in waves and randomly spawns power-ups.
/// Attach to an empty GameObject named "SpawnManager" in the Game scene.
/// Assign enemy and power-up prefabs in the Inspector.
/// </summary>
public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance { get; private set; }

    [Header("Enemy Prefabs")]
    public GameObject basicEnemyPrefab;
    public GameObject zigzagEnemyPrefab;
    public GameObject shooterEnemyPrefab;

    [Header("Power-Up Prefabs")]
    public GameObject rapidFirePowerUpPrefab;
    public GameObject shieldPowerUpPrefab;

    [Header("Spawn Settings")]
    public float spawnXMin = -8f;
    public float spawnXMax = 8f;
    public float spawnYPosition = 7f;
    public float enemySpawnInterval = 1.5f;
    public float powerUpSpawnChance = 0.15f;

    [Header("Difficulty Scaling")]
    public float minSpawnInterval = 0.4f;
    public float speedIncreasePerWave = 0.3f;

    private bool isSpawning = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    /// <summary>
    /// Start spawning enemies for a new wave.
    /// </summary>
    public void StartWave(int enemyCount, int waveNumber)
    {
        isSpawning = true;
        StartCoroutine(SpawnWaveCoroutine(enemyCount, waveNumber));
    }

    /// <summary>
    /// Stop all spawning (called on game over).
    /// </summary>
    public void StopSpawning()
    {
        isSpawning = false;
        StopAllCoroutines();
    }

    /// <summary>
    /// Coroutine that spawns enemies one by one with delays.
    /// Higher waves introduce tougher enemy types and faster spawns.
    /// </summary>
    private IEnumerator SpawnWaveCoroutine(int enemyCount, int waveNumber)
    {
        // Brief delay before wave begins
        yield return new WaitForSeconds(1.5f);

        float interval = Mathf.Max(minSpawnInterval, enemySpawnInterval - (waveNumber * 0.1f));

        for (int i = 0; i < enemyCount; i++)
        {
            if (!isSpawning) yield break;

            SpawnEnemy(waveNumber);
            yield return new WaitForSeconds(interval);

            // Chance to spawn a power-up alongside an enemy
            if (Random.value < powerUpSpawnChance)
            {
                SpawnPowerUp();
            }
        }
    }

    /// <summary>
    /// Select and spawn an enemy type based on the current wave.
    /// </summary>
    private void SpawnEnemy(int waveNumber)
    {
        float xPos = Random.Range(spawnXMin, spawnXMax);
        Vector3 spawnPos = new Vector3(xPos, spawnYPosition, 0f);

        GameObject prefab = ChooseEnemyPrefab(waveNumber);
        GameObject enemy = Instantiate(prefab, spawnPos, Quaternion.identity);

        // Scale difficulty
        EnemyController ec = enemy.GetComponent<EnemyController>();
        if (ec != null)
        {
            ec.moveSpeed += speedIncreasePerWave * (waveNumber - 1);

            // From wave 3 onward, some enemies have extra health
            if (waveNumber >= 3 && Random.value < 0.3f)
            {
                ec.maxHealth = 2;
                ec.currentHealth = 2;
                ec.scoreValue = 200;
            }
        }
    }

    /// <summary>
    /// Pick an enemy prefab. Higher waves introduce zigzag and shooter types.
    /// Falls back to basic enemy if prefabs are not assigned.
    /// </summary>
    private GameObject ChooseEnemyPrefab(int waveNumber)
    {
        float roll = Random.value;

        if (waveNumber >= 5 && roll < 0.25f && shooterEnemyPrefab != null)
        {
            return shooterEnemyPrefab;
        }
        else if (waveNumber >= 2 && roll < 0.5f && zigzagEnemyPrefab != null)
        {
            return zigzagEnemyPrefab;
        }

        return basicEnemyPrefab != null ? basicEnemyPrefab : zigzagEnemyPrefab;
    }

    /// <summary>
    /// Spawn a random power-up at a random X position above screen.
    /// </summary>
    private void SpawnPowerUp()
    {
        float xPos = Random.Range(spawnXMin, spawnXMax);
        Vector3 spawnPos = new Vector3(xPos, spawnYPosition, 0f);

        GameObject prefab;
        if (Random.value < 0.5f && rapidFirePowerUpPrefab != null)
        {
            prefab = rapidFirePowerUpPrefab;
        }
        else if (shieldPowerUpPrefab != null)
        {
            prefab = shieldPowerUpPrefab;
        }
        else if (rapidFirePowerUpPrefab != null)
        {
            prefab = rapidFirePowerUpPrefab;
        }
        else
        {
            return;
        }

        Instantiate(prefab, spawnPos, Quaternion.identity);
        Debug.Log("Power-up spawned.");
    }
}

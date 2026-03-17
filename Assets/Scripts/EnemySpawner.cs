using UnityEngine;
using System.Collections;

/// <summary>
/// Manages wave-based enemy spawning with increasing difficulty.
/// Attach to an empty GameObject in the GamePlay scene.
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy Prefabs")]
    [SerializeField] private GameObject basicEnemyPrefab;
    [SerializeField] private GameObject zigzagEnemyPrefab;
    [SerializeField] private GameObject heavyEnemyPrefab;

    [Header("Spawn Settings")]
    [SerializeField] private float spawnYOffset = 1f;        // How far above screen top to spawn
    [SerializeField] private float timeBetweenSpawns = 0.8f; // Time between individual enemy spawns
    [SerializeField] private float timeBetweenWaves = 4f;    // Delay between waves
    [SerializeField] private int baseEnemiesPerWave = 5;     // Starting enemies per wave
    [SerializeField] private int enemiesPerWaveIncrease = 2; // Additional enemies each wave

    // Internal state
    private int currentWave = 0;
    private int enemiesAlive;
    private bool isSpawning;
    private Camera mainCamera;
    private float spawnMinX, spawnMaxX, spawnY;

    public int CurrentWave => currentWave;

    private void Start()
    {
        mainCamera = Camera.main;
        CalculateSpawnBounds();
    }

    /// <summary>
    /// Calculate the horizontal spawn range and vertical spawn position.
    /// </summary>
    private void CalculateSpawnBounds()
    {
        Vector3 topLeft = mainCamera.ViewportToWorldPoint(new Vector3(0, 1, 0));
        Vector3 topRight = mainCamera.ViewportToWorldPoint(new Vector3(1, 1, 0));
        spawnMinX = topLeft.x + 1f;
        spawnMaxX = topRight.x - 1f;
        spawnY = topRight.y + spawnYOffset;
    }

    /// <summary>
    /// Start spawning waves. Called by GameManager when the game begins.
    /// </summary>
    public void StartSpawning()
    {
        currentWave = 0;
        StopAllCoroutines();
        StartCoroutine(SpawnWavesRoutine());
    }

    /// <summary>
    /// Stop all spawning coroutines.
    /// </summary>
    public void StopSpawning()
    {
        StopAllCoroutines();
        isSpawning = false;
    }

    /// <summary>
    /// Main coroutine that spawns waves indefinitely with increasing difficulty.
    /// </summary>
    private IEnumerator SpawnWavesRoutine()
    {
        // Initial delay before first wave
        yield return new WaitForSeconds(2f);

        while (true)
        {
            currentWave++;
            int enemyCount = baseEnemiesPerWave + (currentWave - 1) * enemiesPerWaveIncrease;
            isSpawning = true;

            // Notify UI
            if (UIManager.Instance != null)
                UIManager.Instance.UpdateWaveText(currentWave);

            if (GameManager.Instance != null)
                GameManager.Instance.OnWaveStart(currentWave);

            // Spawn enemies for this wave
            yield return StartCoroutine(SpawnWave(enemyCount));

            // Wait for all enemies to be destroyed
            yield return StartCoroutine(WaitForWaveClear());

            isSpawning = false;

            // Pause between waves
            yield return new WaitForSeconds(timeBetweenWaves);
        }
    }

    /// <summary>
    /// Spawn a single wave of enemies with type distribution based on wave number.
    /// </summary>
    private IEnumerator SpawnWave(int count)
    {
        enemiesAlive = 0;

        for (int i = 0; i < count; i++)
        {
            if (GameManager.Instance != null && !GameManager.Instance.IsGameActive)
                yield break;

            SpawnEnemy();
            enemiesAlive++;

            // Decrease time between spawns as waves progress
            float adjustedSpawnTime = Mathf.Max(0.3f, timeBetweenSpawns - currentWave * 0.03f);
            yield return new WaitForSeconds(adjustedSpawnTime);
        }
    }

    /// <summary>
    /// Choose and instantiate an enemy type based on wave progression.
    /// </summary>
    private void SpawnEnemy()
    {
        float xPos = Random.Range(spawnMinX, spawnMaxX);
        Vector3 spawnPosition = new Vector3(xPos, spawnY, 0);

        // Choose enemy type based on wave number
        GameObject prefab = ChooseEnemyPrefab();
        if (prefab == null) return;

        GameObject enemy = Instantiate(prefab, spawnPosition, Quaternion.identity);
        EnemyController ec = enemy.GetComponent<EnemyController>();
        if (ec != null)
        {
            ec.SetDifficulty(currentWave);
        }
    }

    /// <summary>
    /// Select enemy prefab based on weighted random and wave progression.
    /// Higher waves introduce tougher enemy types more frequently.
    /// </summary>
    private GameObject ChooseEnemyPrefab()
    {
        float roll = Random.value;

        // Wave 1-2: mostly basic enemies
        if (currentWave <= 2)
        {
            if (roll < 0.8f) return basicEnemyPrefab;
            return zigzagEnemyPrefab != null ? zigzagEnemyPrefab : basicEnemyPrefab;
        }

        // Wave 3-5: mix of all types
        if (currentWave <= 5)
        {
            if (roll < 0.4f) return basicEnemyPrefab;
            if (roll < 0.75f) return zigzagEnemyPrefab != null ? zigzagEnemyPrefab : basicEnemyPrefab;
            return heavyEnemyPrefab != null ? heavyEnemyPrefab : basicEnemyPrefab;
        }

        // Wave 6+: heavier distribution of tough enemies
        if (roll < 0.25f) return basicEnemyPrefab;
        if (roll < 0.55f) return zigzagEnemyPrefab != null ? zigzagEnemyPrefab : basicEnemyPrefab;
        return heavyEnemyPrefab != null ? heavyEnemyPrefab : basicEnemyPrefab;
    }

    /// <summary>
    /// Wait until all enemies from the current wave are destroyed.
    /// </summary>
    private IEnumerator WaitForWaveClear()
    {
        while (true)
        {
            // Count remaining enemies in scene
            int remaining = GameObject.FindGameObjectsWithTag("Enemy").Length;
            if (remaining <= 0)
                yield break;

            yield return new WaitForSeconds(0.5f);
        }
    }

    /// <summary>
    /// Destroy all active enemies (used when restarting).
    /// </summary>
    public void ClearAllEnemies()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemies)
        {
            Destroy(enemy);
        }

        GameObject[] enemyBullets = GameObject.FindGameObjectsWithTag("EnemyBullet");
        foreach (GameObject bullet in enemyBullets)
        {
            Destroy(bullet);
        }

        GameObject[] playerBullets = GameObject.FindGameObjectsWithTag("PlayerBullet");
        foreach (GameObject bullet in playerBullets)
        {
            Destroy(bullet);
        }

        GameObject[] powerUps = GameObject.FindGameObjectsWithTag("PowerUp");
        foreach (GameObject pu in powerUps)
        {
            Destroy(pu);
        }
    }
}

// ============================================================================
// EnemySpawner.cs - Wave-based enemy spawning with progression
// ============================================================================
using System.Collections;
using UnityEngine;

/// <summary>
/// Manages spawning enemies in waves. Listens to GameManager for wave
/// completion events and starts new waves after a cooldown.
/// Place this on an empty GameObject in the GameScene.
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    // ---- Enemy Prefabs ----
    [Header("Enemy Prefabs (assign in Inspector)")]
    [Tooltip("Array of enemy prefabs. Index 0 = basic, higher = tougher.")]
    public GameObject[] enemyPrefabs;

    // ---- Spawn Settings ----
    [Header("Spawn Settings")]
    [Tooltip("Seconds between individual enemy spawns within a wave")]
    public float spawnInterval = 0.8f;

    [Tooltip("X range for random spawn positions")]
    public float spawnRangeX = 7f;

    [Tooltip("Y position where enemies spawn (above the screen)")]
    public float spawnY = 7f;

    // ---- Power-Up Drop ----
    [Header("Power-Up Drops")]
    [Tooltip("Power-up prefabs that can drop after killing enemies")]
    public GameObject[] powerUpPrefabs;

    [Tooltip("Chance (0-1) that a power-up drops at the end of a wave")]
    public float powerUpDropChance = 0.4f;

    // ---- Internal ----
    private bool _spawning = false;
    private Camera _mainCam;

    // ========================================================================
    // Unity Lifecycle
    // ========================================================================
    private void Start()
    {
        _mainCam = Camera.main;

        // Subscribe to wave completion
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnWaveCompleted += OnWaveCompleted;
        }

        // Start the first wave after a short delay
        StartCoroutine(StartFirstWave());
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnWaveCompleted -= OnWaveCompleted;
        }
    }

    // ========================================================================
    // Wave Flow
    // ========================================================================

    private IEnumerator StartFirstWave()
    {
        // Small delay so everything initializes
        yield return new WaitForSeconds(1.5f);
        StartWave();
    }

    private void OnWaveCompleted()
    {
        // Possibly drop a power-up between waves
        TryDropPowerUp();

        // Start next wave after cooldown
        StartCoroutine(WaveCooldownThenStart());
    }

    private IEnumerator WaveCooldownThenStart()
    {
        float cooldown = GameManager.Instance != null ?
            GameManager.Instance.waveCooldown : 3f;
        yield return new WaitForSeconds(cooldown);
        StartWave();
    }

    private void StartWave()
    {
        if (_spawning) return;
        if (GameManager.Instance == null) return;
        if (GameManager.Instance.CurrentState != GameManager.GameState.Playing) return;

        GameManager.Instance.StartNextWave();
        int count = GameManager.Instance.GetEnemyCountForWave(
            GameManager.Instance.CurrentWave);
        StartCoroutine(SpawnWave(count));
    }

    // ========================================================================
    // Spawning
    // ========================================================================

    private IEnumerator SpawnWave(int count)
    {
        _spawning = true;

        for (int i = 0; i < count; i++)
        {
            // Check if game is still running
            if (GameManager.Instance == null ||
                GameManager.Instance.CurrentState != GameManager.GameState.Playing)
            {
                _spawning = false;
                yield break;
            }

            SpawnEnemy();
            yield return new WaitForSeconds(spawnInterval);
        }

        _spawning = false;
    }

    private void SpawnEnemy()
    {
        if (enemyPrefabs == null || enemyPrefabs.Length == 0)
        {
            Debug.LogWarning("EnemySpawner: No enemy prefabs assigned!");
            return;
        }

        // Choose enemy type based on wave difficulty
        int wave = GameManager.Instance != null ? GameManager.Instance.CurrentWave : 1;
        int maxIndex = Mathf.Min(wave / 2, enemyPrefabs.Length - 1); // unlock tougher enemies in later waves
        int prefabIndex = Random.Range(0, maxIndex + 1);

        // Random X position within spawn range
        float xPos = Random.Range(-spawnRangeX, spawnRangeX);

        // Clamp to screen if camera is available
        if (_mainCam != null)
        {
            Vector3 minScreen = _mainCam.ViewportToWorldPoint(new Vector3(0.05f, 0, 0));
            Vector3 maxScreen = _mainCam.ViewportToWorldPoint(new Vector3(0.95f, 0, 0));
            xPos = Mathf.Clamp(xPos, minScreen.x, maxScreen.x);
        }

        Vector3 spawnPos = new Vector3(xPos, spawnY, 0);
        GameObject enemy = Instantiate(enemyPrefabs[prefabIndex], spawnPos, Quaternion.identity);

        // Randomly assign movement and shoot patterns for variety
        EnemyController ec = enemy.GetComponent<EnemyController>();
        if (ec != null)
        {
            // More variety in later waves
            if (wave >= 3)
            {
                ec.movementPattern = (EnemyController.MovementPattern)
                    Random.Range(0, System.Enum.GetValues(typeof(EnemyController.MovementPattern)).Length);
            }
            if (wave >= 2 && Random.value > 0.3f)
            {
                ec.shootPattern = (EnemyController.ShootPattern)
                    Random.Range(1, System.Enum.GetValues(typeof(EnemyController.ShootPattern)).Length);
            }
        }
    }

    // ========================================================================
    // Power-Up Drops
    // ========================================================================
    private void TryDropPowerUp()
    {
        if (powerUpPrefabs == null || powerUpPrefabs.Length == 0) return;
        if (Random.value > powerUpDropChance) return;

        // Drop at a random position in the upper portion of the screen
        float xPos = Random.Range(-spawnRangeX * 0.8f, spawnRangeX * 0.8f);
        Vector3 dropPos = new Vector3(xPos, spawnY * 0.8f, 0);

        int index = Random.Range(0, powerUpPrefabs.Length);
        Instantiate(powerUpPrefabs[index], dropPos, Quaternion.identity);
    }
}

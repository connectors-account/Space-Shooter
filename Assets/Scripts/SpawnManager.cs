using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Spawns enemies in 5 waves with increasing challenge.
/// </summary>
public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance { get; private set; }

    [System.Serializable]
    public class WaveDefinition
    {
        public int enemyCount = 8;
        public float spawnInterval = 1f;
        public float enemySpeedMultiplier = 1f;
        public float enemyShootIntervalMultiplier = 1f;
    }

    [Header("References")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private GameObject[] powerUpPrefabs;
    [SerializeField] private UIManager uiManager;

    [Header("Wave Setup (Exactly 5 Waves)")]
    [SerializeField] private List<WaveDefinition> waves = new List<WaveDefinition>
    {
        new WaveDefinition { enemyCount = 6,  spawnInterval = 1.20f, enemySpeedMultiplier = 1.0f, enemyShootIntervalMultiplier = 1.25f },
        new WaveDefinition { enemyCount = 8,  spawnInterval = 1.05f, enemySpeedMultiplier = 1.1f, enemyShootIntervalMultiplier = 1.15f },
        new WaveDefinition { enemyCount = 10, spawnInterval = 0.95f, enemySpeedMultiplier = 1.2f, enemyShootIntervalMultiplier = 1.00f },
        new WaveDefinition { enemyCount = 12, spawnInterval = 0.85f, enemySpeedMultiplier = 1.35f, enemyShootIntervalMultiplier = 0.90f },
        new WaveDefinition { enemyCount = 14, spawnInterval = 0.75f, enemySpeedMultiplier = 1.5f, enemyShootIntervalMultiplier = 0.80f }
    };

    [Header("Spawn Bounds")]
    [SerializeField] private float topSpawnOffset = 1.5f;
    [SerializeField] private float sidePadding = 0.75f;

    [Header("Timing")]
    [SerializeField] private float timeBetweenWaves = 2.5f;
    [SerializeField] private Vector2 powerUpSpawnIntervalRange = new Vector2(8f, 14f);

    private int activeEnemies;
    private float nextPowerUpSpawnTime;
    private int currentWaveIndex = -1;
    private bool spawning;
    private Coroutine spawnRoutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void BeginSpawning()
    {
        if (spawnRoutine != null)
        {
            StopCoroutine(spawnRoutine);
        }

        ClearRemainingEnemies();
        activeEnemies = 0;
        currentWaveIndex = -1;
        nextPowerUpSpawnTime = Time.time + Random.Range(powerUpSpawnIntervalRange.x, powerUpSpawnIntervalRange.y);
        spawning = true;
        spawnRoutine = StartCoroutine(SpawnWavesRoutine());
    }

    public void ReportEnemyDestroyed(EnemyController enemy, bool killedByPlayer)
    {
        activeEnemies = Mathf.Max(0, activeEnemies - 1);
    }

    private IEnumerator SpawnWavesRoutine()
    {
        for (int i = 0; i < waves.Count; i++)
        {
            currentWaveIndex = i;
            uiManager?.UpdateWave(i + 1, waves.Count);

            WaveDefinition wave = waves[i];
            for (int j = 0; j < wave.enemyCount; j++)
            {
                if (!spawning || GameManager.Instance.IsGameOver)
                {
                    yield break;
                }

                SpawnEnemyForWave(wave, i);
                TrySpawnPowerUp();
                yield return new WaitForSeconds(wave.spawnInterval);
            }

            // Wait for player to clean up existing enemies before next wave.
            while (activeEnemies > 0 && !GameManager.Instance.IsGameOver)
            {
                TrySpawnPowerUp();
                yield return null;
            }

            if (i < waves.Count - 1)
            {
                yield return new WaitForSeconds(timeBetweenWaves);
            }
        }

        spawning = false;
        GameManager.Instance?.OnAllWavesCompleted();
    }

    private void SpawnEnemyForWave(WaveDefinition wave, int waveIndex)
    {
        if (enemyPrefab == null)
        {
            return;
        }

        Vector3 spawnPos = GetSpawnPosition();
        GameObject enemyObj = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
        EnemyController enemy = enemyObj.GetComponent<EnemyController>();

        if (enemy != null)
        {
            EnemyController.MovementPattern pattern = (EnemyController.MovementPattern)(waveIndex % 3);
            enemy.ConfigureForWave(wave.enemySpeedMultiplier, wave.enemyShootIntervalMultiplier, pattern);
        }

        activeEnemies++;
    }

    private void TrySpawnPowerUp()
    {
        if (powerUpPrefabs == null || powerUpPrefabs.Length == 0)
        {
            return;
        }

        if (Time.time < nextPowerUpSpawnTime)
        {
            return;
        }

        nextPowerUpSpawnTime = Time.time + Random.Range(powerUpSpawnIntervalRange.x, powerUpSpawnIntervalRange.y);

        int index = Random.Range(0, powerUpPrefabs.Length);
        if (powerUpPrefabs[index] == null)
        {
            return;
        }

        Vector3 spawnPos = GetSpawnPosition();
        Instantiate(powerUpPrefabs[index], spawnPos, Quaternion.identity);
    }

    private Vector3 GetSpawnPosition()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            return new Vector3(Random.Range(-5f, 5f), 6f, 0f);
        }

        Vector3 left = cam.ViewportToWorldPoint(new Vector3(0f, 1f, cam.nearClipPlane));
        Vector3 right = cam.ViewportToWorldPoint(new Vector3(1f, 1f, cam.nearClipPlane));

        float x = Random.Range(left.x + sidePadding, right.x - sidePadding);
        float y = right.y + topSpawnOffset;
        return new Vector3(x, y, 0f);
    }

    private void ClearRemainingEnemies()
    {
        EnemyController[] enemies = FindObjectsOfType<EnemyController>();
        for (int i = 0; i < enemies.Length; i++)
        {
            Destroy(enemies[i].gameObject);
        }

        BulletController[] bullets = FindObjectsOfType<BulletController>();
        for (int i = 0; i < bullets.Length; i++)
        {
            Destroy(bullets[i].gameObject);
        }

        PowerUpController[] powerUps = FindObjectsOfType<PowerUpController>();
        for (int i = 0; i < powerUps.Length; i++)
        {
            Destroy(powerUps[i].gameObject);
        }
    }

}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Manages wave-based enemy spawning with increasing difficulty.
/// Each wave defines enemy counts, types, spawn rates, and stat scaling.
/// </summary>
public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance { get; private set; }

    [Header("Wave Settings")]
    [SerializeField] private float timeBetweenWaves = 4f;
    [SerializeField] private float spawnInterval = 1f;
    [SerializeField] private int baseEnemiesPerWave = 5;
    [SerializeField] private int enemiesPerWaveIncrease = 2;
    [SerializeField] private int bossEveryNWaves = 5;

    [Header("Difficulty Scaling")]
    [SerializeField] private float healthScalePerWave = 0.15f;
    [SerializeField] private float speedScalePerWave = 0.05f;
    [SerializeField] private float fireRateScalePerWave = 0.03f;

    public int CurrentWave { get; private set; }
    public bool IsSpawning { get; private set; }

    private int enemiesAliveThisWave;
    private int totalEnemiesThisWave;
    private int enemiesSpawnedThisWave;
    private bool waveActive;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        // Listen for game state changes
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStateChanged += OnGameStateChanged;
        }
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStateChanged -= OnGameStateChanged;
        }
    }

    private void OnGameStateChanged(GameState state)
    {
        if (state == GameState.Playing && !waveActive)
        {
            StartCoroutine(StartWaveLoop());
        }
        else if (state == GameState.GameOver)
        {
            StopAllCoroutines();
            waveActive = false;
            IsSpawning = false;
        }
    }

    private IEnumerator StartWaveLoop()
    {
        waveActive = true;

        // Initial delay
        yield return new WaitForSeconds(1.5f);

        while (GameManager.Instance.CurrentState == GameState.Playing)
        {
            CurrentWave++;
            GameManager.Instance.SetWave(CurrentWave);

            yield return StartCoroutine(SpawnWave());

            // Wait until all enemies of this wave are dead
            while (enemiesAliveThisWave > 0)
            {
                yield return new WaitForSeconds(0.5f);
                // Clean up count for any enemies that were destroyed without notifying
                CleanupDeadEnemyCount();
            }

            // Brief pause between waves
            yield return new WaitForSeconds(timeBetweenWaves);
        }

        waveActive = false;
    }

    private IEnumerator SpawnWave()
    {
        IsSpawning = true;
        totalEnemiesThisWave = CalculateEnemyCount();
        enemiesSpawnedThisWave = 0;
        enemiesAliveThisWave = totalEnemiesThisWave;

        bool isBossWave = (CurrentWave % bossEveryNWaves == 0);

        for (int i = 0; i < totalEnemiesThisWave; i++)
        {
            if (GameManager.Instance.CurrentState != GameState.Playing)
                yield break;

            SpawnEnemy(i, isBossWave && i == totalEnemiesThisWave - 1);
            enemiesSpawnedThisWave++;

            float interval = spawnInterval * Mathf.Max(0.3f, 1f - CurrentWave * 0.02f);
            yield return new WaitForSeconds(interval);
        }

        IsSpawning = false;
    }

    private void SpawnEnemy(int index, bool isBoss)
    {
        if (ObjectPool.Instance == null) return;

        Vector2 spawnPos = GameManager.Instance.GetRandomTopSpawnPosition();

        // Determine enemy type based on wave and randomness
        string enemyTag;
        EnemyMovementPattern pattern;
        int baseHp;
        float baseSpeed;
        int baseScore;
        float dropChance;
        float shootRate;
        bool canShoot;

        if (isBoss)
        {
            enemyTag = Tags.EnemyBoss;
            pattern = EnemyMovementPattern.Hover;
            baseHp = 20;
            baseSpeed = 1.5f;
            baseScore = 1000;
            dropChance = 1f; // Boss always drops power-up
            shootRate = 0.5f;
            canShoot = true;
        }
        else
        {
            float rand = Random.value;
            if (CurrentWave < 3 || rand < 0.5f)
            {
                // Basic enemy
                enemyTag = Tags.EnemyBasic;
                pattern = GetRandomPattern(basic: true);
                baseHp = 1;
                baseSpeed = 2f;
                baseScore = 100;
                dropChance = 0.08f;
                shootRate = 2f;
                canShoot = CurrentWave >= 2;
            }
            else if (rand < 0.75f)
            {
                // Fast enemy
                enemyTag = Tags.EnemyFast;
                pattern = GetRandomPattern(basic: false);
                baseHp = 1;
                baseSpeed = 4f;
                baseScore = 150;
                dropChance = 0.1f;
                shootRate = 3f;
                canShoot = false;
            }
            else
            {
                // Tank enemy
                enemyTag = Tags.EnemyTank;
                pattern = EnemyMovementPattern.StraightDown;
                baseHp = 3;
                baseSpeed = 1f;
                baseScore = 250;
                dropChance = 0.2f;
                shootRate = 1.5f;
                canShoot = true;
            }
        }

        // Scale stats with wave number
        float waveScale = CurrentWave - 1;
        int scaledHp = Mathf.RoundToInt(baseHp * (1f + waveScale * healthScalePerWave));
        float scaledSpeed = baseSpeed * (1f + waveScale * speedScalePerWave);
        float scaledFireRate = Mathf.Max(0.3f, shootRate * (1f - waveScale * fireRateScalePerWave));

        GameObject enemy = ObjectPool.Instance.Spawn(enemyTag, spawnPos, Quaternion.identity);
        if (enemy != null)
        {
            EnemyBase eb = enemy.GetComponent<EnemyBase>();
            if (eb != null)
            {
                eb.Configure(scaledHp, scaledSpeed, baseScore, dropChance, pattern, scaledFireRate, canShoot);
            }
        }
    }

    private int CalculateEnemyCount()
    {
        int count = baseEnemiesPerWave + (CurrentWave - 1) * enemiesPerWaveIncrease;
        return Mathf.Min(count, 30); // Cap at 30 enemies per wave
    }

    private EnemyMovementPattern GetRandomPattern(bool basic)
    {
        if (basic)
        {
            int r = Random.Range(0, 3);
            switch (r)
            {
                case 0: return EnemyMovementPattern.StraightDown;
                case 1: return EnemyMovementPattern.Zigzag;
                default: return EnemyMovementPattern.Sine;
            }
        }
        else
        {
            int r = Random.Range(0, 4);
            switch (r)
            {
                case 0: return EnemyMovementPattern.Zigzag;
                case 1: return EnemyMovementPattern.DiagonalLeft;
                case 2: return EnemyMovementPattern.DiagonalRight;
                default: return EnemyMovementPattern.TrackPlayer;
            }
        }
    }

    /// <summary>
    /// Called when an enemy dies to decrement the alive count.
    /// </summary>
    public void OnEnemyDestroyed()
    {
        enemiesAliveThisWave = Mathf.Max(0, enemiesAliveThisWave - 1);
    }

    private void CleanupDeadEnemyCount()
    {
        // Count actual active enemies
        int activeCount = 0;
        EnemyBase[] enemies = FindObjectsOfType<EnemyBase>();
        foreach (var e in enemies)
        {
            if (e.gameObject.activeSelf) activeCount++;
        }
        enemiesAliveThisWave = activeCount;
    }
}

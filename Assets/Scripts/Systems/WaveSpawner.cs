using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[System.Serializable]
public class Wave
{
    public string waveName;
    public List<WaveSegment> segments;
    public bool hasBoss = false;
    public float delayAfterWave = 3f;
}

[System.Serializable]
public class WaveSegment
{
    public EnemyType enemyType;
    public int count;
    public FormationType formation = FormationType.Random;
    public float spawnDelay = 0.5f;
    public float segmentDelay = 2f;
}

public class WaveSpawner : MonoBehaviour
{
    public static WaveSpawner Instance { get; private set; }

    [Header("Wave Settings")]
    public List<Wave> waves;
    public float timeBetweenWaves = 5f;
    public bool autoStartWaves = true;
    public int totalWaves = 10;

    [Header("Difficulty Scaling")]
    public float difficultyMultiplier = 1.1f;
    public int baseEnemyCount = 5;

    // State
    private int currentWaveIndex = 0;
    private bool isSpawning = false;
    private bool waveInProgress = false;
    private int enemiesAliveInWave = 0;
    private bool bossActive = false;

    // Events
    public static event Action<int> OnWaveStarted;
    public static event Action<int> OnWaveCompleted;
    public static event Action OnAllWavesCompleted;

    public int CurrentWave => currentWaveIndex + 1;
    public int TotalWaves => totalWaves;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // Generate waves if none defined
        if (waves == null || waves.Count == 0)
        {
            GenerateWaves();
        }

        // Subscribe to enemy death event
        EnemyBase.OnEnemyKilled += OnEnemyKilled;

        if (autoStartWaves)
        {
            StartCoroutine(StartFirstWaveDelayed());
        }
    }

    private IEnumerator StartFirstWaveDelayed()
    {
        yield return new WaitForSeconds(2f);
        StartNextWave();
    }

    private void GenerateWaves()
    {
        waves = new List<Wave>();

        for (int i = 0; i < totalWaves; i++)
        {
            Wave wave = new Wave();
            wave.waveName = $"Wave {i + 1}";
            wave.segments = new List<WaveSegment>();

            int enemyCount = Mathf.RoundToInt(baseEnemyCount * Mathf.Pow(difficultyMultiplier, i));
            int segmentCount = 1 + i / 3;

            for (int s = 0; s < segmentCount; s++)
            {
                WaveSegment segment = new WaveSegment();

                // Gradually introduce different enemy types
                if (i < 2)
                {
                    segment.enemyType = EnemyType.Basic;
                }
                else if (i < 4)
                {
                    segment.enemyType = (EnemyType)(UnityEngine.Random.Range(0, 2));
                }
                else if (i < 6)
                {
                    segment.enemyType = (EnemyType)(UnityEngine.Random.Range(0, 3));
                }
                else
                {
                    segment.enemyType = (EnemyType)(UnityEngine.Random.Range(0, 4));
                }

                segment.count = enemyCount / segmentCount;
                segment.formation = (FormationType)(s % 4);
                segment.spawnDelay = Mathf.Max(0.2f, 0.5f - i * 0.03f);
                segment.segmentDelay = 2f;

                wave.segments.Add(segment);
            }

            // Boss every 5 waves
            if ((i + 1) % 5 == 0)
            {
                wave.hasBoss = true;
            }

            wave.delayAfterWave = 3f;
            waves.Add(wave);
        }
    }

    public void StartNextWave()
    {
        if (currentWaveIndex >= waves.Count)
        {
            OnAllWavesCompleted?.Invoke();
            GameManager.Instance?.Victory();
            return;
        }

        if (!waveInProgress)
        {
            StartCoroutine(SpawnWave(waves[currentWaveIndex]));
        }
    }

    private IEnumerator SpawnWave(Wave wave)
    {
        waveInProgress = true;
        isSpawning = true;
        enemiesAliveInWave = 0;

        OnWaveStarted?.Invoke(currentWaveIndex + 1);
        AudioManager.Instance?.PlaySFX("WaveStart");

        // Spawn each segment
        foreach (var segment in wave.segments)
        {
            if (GameManager.Instance?.CurrentState != GameManager.GameState.Playing)
            {
                isSpawning = false;
                waveInProgress = false;
                yield break;
            }

            yield return StartCoroutine(SpawnSegment(segment));
            yield return new WaitForSeconds(segment.segmentDelay);
        }

        // Spawn boss if wave has one
        if (wave.hasBoss)
        {
            yield return new WaitForSeconds(2f);
            SpawnBoss();
        }

        isSpawning = false;

        // Wait for all enemies to be defeated
        yield return new WaitUntil(() => enemiesAliveInWave <= 0 && !bossActive);

        OnWaveCompleted?.Invoke(currentWaveIndex + 1);
        AudioManager.Instance?.PlaySFX("WaveComplete");

        currentWaveIndex++;
        waveInProgress = false;

        // Wait then start next wave
        yield return new WaitForSeconds(wave.delayAfterWave);

        if (currentWaveIndex < waves.Count)
        {
            StartNextWave();
        }
        else
        {
            OnAllWavesCompleted?.Invoke();
            GameManager.Instance?.Victory();
        }
    }

    private IEnumerator SpawnSegment(WaveSegment segment)
    {
        List<Vector3> positions = GetFormationPositions(segment.formation, segment.count);

        foreach (var pos in positions)
        {
            if (GameManager.Instance?.CurrentState != GameManager.GameState.Playing)
                yield break;

            EnemySpawner.Instance?.SpawnEnemyAt(segment.enemyType, pos);
            enemiesAliveInWave++;
            yield return new WaitForSeconds(segment.spawnDelay);
        }
    }

    private List<Vector3> GetFormationPositions(FormationType formation, int count)
    {
        List<Vector3> positions = new List<Vector3>();
        float spawnY = 6f;
        float minX = -7f;
        float maxX = 7f;

        switch (formation)
        {
            case FormationType.Line:
                float spacing = (maxX - minX) / (count + 1);
                for (int i = 0; i < count; i++)
                {
                    float x = minX + spacing * (i + 1);
                    positions.Add(new Vector3(x, spawnY, 0));
                }
                break;

            case FormationType.V:
                float vSpacing = 1.2f;
                for (int i = 0; i < count; i++)
                {
                    int side = i % 2 == 0 ? 1 : -1;
                    int row = i / 2;
                    float x = side * row * vSpacing;
                    float y = spawnY - row * 0.4f;
                    positions.Add(new Vector3(x, y, 0));
                }
                break;

            case FormationType.Circle:
                float radius = 2.5f;
                float angleStep = 180f / Mathf.Max(1, count - 1);
                for (int i = 0; i < count; i++)
                {
                    float angle = (90f + angleStep * i) * Mathf.Deg2Rad;
                    float x = Mathf.Cos(angle) * radius;
                    float y = spawnY + Mathf.Sin(angle) * radius * 0.3f - 1f;
                    positions.Add(new Vector3(x, y, 0));
                }
                break;

            case FormationType.Random:
            default:
                for (int i = 0; i < count; i++)
                {
                    float x = UnityEngine.Random.Range(minX, maxX);
                    float y = spawnY + UnityEngine.Random.Range(-0.5f, 0.5f);
                    positions.Add(new Vector3(x, y, 0));
                }
                break;
        }

        return positions;
    }

    private void SpawnBoss()
    {
        bossActive = true;
        EnemySpawner.Instance?.SpawnBoss();
        AudioManager.Instance?.PlaySFX("BossSpawn");
    }

    public void OnBossDefeated()
    {
        bossActive = false;
    }

    private void OnEnemyKilled(int score)
    {
        enemiesAliveInWave--;
    }

    public void ResetWaves()
    {
        StopAllCoroutines();
        currentWaveIndex = 0;
        waveInProgress = false;
        isSpawning = false;
        enemiesAliveInWave = 0;
        bossActive = false;
    }

    private void OnDestroy()
    {
        EnemyBase.OnEnemyKilled -= OnEnemyKilled;
    }
}

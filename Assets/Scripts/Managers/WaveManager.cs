// ============================================================================
// WaveManager.cs — Controls enemy wave spawning and progression
// ============================================================================
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class WaveDefinition
{
    public string waveName = "Wave";
    public EnemySpawnEntry[] enemies;
    public float delayBeforeWave = 2f;
    public float spawnInterval = 1.5f;
    public bool isBossWave;
}

[System.Serializable]
public class EnemySpawnEntry
{
    public GameObject enemyPrefab;
    public int count = 3;
    public SpawnPattern pattern = SpawnPattern.Random;
    public float customSpawnDelay = -1f; // -1 means use wave default
}

public enum SpawnPattern
{
    Random,
    LeftToRight,
    RightToLeft,
    VFormation,
    Circle,
    ZigZag
}

public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance { get; private set; }

    [Header("Wave Definitions")]
    [SerializeField] private WaveDefinition[] predefinedWaves;

    [Header("Auto-Generation Settings")]
    [SerializeField] private bool autoGenerateWaves = true;
    [SerializeField] private GameObject[] enemyPrefabs;        // 0=basic, 1=fast, 2=tank, 3=shooter
    [SerializeField] private GameObject bossPrefab;

    [Header("Spawn Area")]
    [SerializeField] private float spawnYPosition = 6f;
    [SerializeField] private float spawnXMin = -4f;
    [SerializeField] private float spawnXMax = 4f;

    [Header("Power-Up Spawning")]
    [SerializeField] private GameObject[] powerUpPrefabs;
    [SerializeField] [Range(0f, 1f)] private float powerUpDropChance = 0.15f;

    // Runtime
    private int currentWaveIndex;
    private int enemiesAliveCount;
    private bool isSpawning;
    private bool waveActive;
    private Coroutine spawnCoroutine;

    public int EnemiesAlive => enemiesAliveCount;
    public bool IsWaveActive => waveActive;

    // =========================================================================
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        if (autoGenerateWaves && (predefinedWaves == null || predefinedWaves.Length == 0))
        {
            predefinedWaves = GenerateWaves(10);
        }
        currentWaveIndex = 0;
        StartCoroutine(WaveLoop());
    }

    private void OnEnable()
    {
        GameManager.OnGameStateChanged += HandleGameStateChanged;
    }

    private void OnDisable()
    {
        GameManager.OnGameStateChanged -= HandleGameStateChanged;
    }

    private void HandleGameStateChanged(GameState state)
    {
        if (state == GameState.GameOver || state == GameState.Victory)
        {
            StopAllCoroutines();
            waveActive = false;
        }
    }

    // =========================================================================
    // Wave Loop
    // =========================================================================
    private IEnumerator WaveLoop()
    {
        // Wait for game to start
        yield return new WaitUntil(() =>
            GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.Playing);

        while (currentWaveIndex < predefinedWaves.Length)
        {
            if (GameManager.Instance.CurrentState != GameState.Playing)
            {
                yield return null;
                continue;
            }

            WaveDefinition wave = predefinedWaves[currentWaveIndex];

            // Announce wave
            GameManager.Instance.AdvanceWave();
            yield return new WaitForSeconds(wave.delayBeforeWave);

            // Spawn enemies for this wave
            waveActive = true;
            yield return StartCoroutine(SpawnWave(wave));

            // Wait until all enemies are defeated
            yield return new WaitUntil(() => enemiesAliveCount <= 0);
            waveActive = false;

            // Brief pause between waves
            yield return new WaitForSeconds(2f);

            currentWaveIndex++;
        }

        // All waves completed -> Victory!
        if (GameManager.Instance.CurrentState == GameState.Playing)
        {
            GameManager.Instance.SetState(GameState.Victory);
        }
    }

    private IEnumerator SpawnWave(WaveDefinition wave)
    {
        isSpawning = true;

        foreach (var entry in wave.enemies)
        {
            if (entry.enemyPrefab == null) continue;

            float interval = entry.customSpawnDelay > 0 ? entry.customSpawnDelay : wave.spawnInterval;

            List<Vector3> positions = GetSpawnPositions(entry.count, entry.pattern);

            for (int i = 0; i < entry.count; i++)
            {
                if (GameManager.Instance.CurrentState != GameState.Playing)
                {
                    isSpawning = false;
                    yield break;
                }

                Vector3 pos = positions[i];
                GameObject enemy = Instantiate(entry.enemyPrefab, pos, Quaternion.identity);

                // Register enemy
                EnemyBase enemyScript = enemy.GetComponent<EnemyBase>();
                if (enemyScript != null)
                {
                    enemyScript.OnEnemyDestroyed += HandleEnemyDestroyed;
                }
                enemiesAliveCount++;

                yield return new WaitForSeconds(interval);
            }
        }

        isSpawning = false;
    }

    // =========================================================================
    // Spawn Patterns
    // =========================================================================
    private List<Vector3> GetSpawnPositions(int count, SpawnPattern pattern)
    {
        List<Vector3> positions = new List<Vector3>();

        switch (pattern)
        {
            case SpawnPattern.Random:
                for (int i = 0; i < count; i++)
                    positions.Add(new Vector3(Random.Range(spawnXMin, spawnXMax), spawnYPosition, 0));
                break;

            case SpawnPattern.LeftToRight:
                for (int i = 0; i < count; i++)
                {
                    float t = count > 1 ? (float)i / (count - 1) : 0.5f;
                    positions.Add(new Vector3(Mathf.Lerp(spawnXMin, spawnXMax, t), spawnYPosition, 0));
                }
                break;

            case SpawnPattern.RightToLeft:
                for (int i = 0; i < count; i++)
                {
                    float t = count > 1 ? (float)i / (count - 1) : 0.5f;
                    positions.Add(new Vector3(Mathf.Lerp(spawnXMax, spawnXMin, t), spawnYPosition, 0));
                }
                break;

            case SpawnPattern.VFormation:
                for (int i = 0; i < count; i++)
                {
                    float offset = (i - count / 2f) * 0.8f;
                    float yOff = Mathf.Abs(offset) * 0.5f;
                    positions.Add(new Vector3(offset, spawnYPosition + yOff, 0));
                }
                break;

            case SpawnPattern.Circle:
                for (int i = 0; i < count; i++)
                {
                    float angle = (360f / count) * i * Mathf.Deg2Rad;
                    float radius = 2f;
                    positions.Add(new Vector3(Mathf.Cos(angle) * radius, spawnYPosition + Mathf.Sin(angle) * radius * 0.3f, 0));
                }
                break;

            case SpawnPattern.ZigZag:
                for (int i = 0; i < count; i++)
                {
                    float x = (i % 2 == 0) ? spawnXMin + 1f : spawnXMax - 1f;
                    positions.Add(new Vector3(x, spawnYPosition, 0));
                }
                break;
        }

        return positions;
    }

    // =========================================================================
    // Enemy Tracking
    // =========================================================================
    private void HandleEnemyDestroyed(EnemyBase enemy)
    {
        enemiesAliveCount = Mathf.Max(0, enemiesAliveCount - 1);
        enemy.OnEnemyDestroyed -= HandleEnemyDestroyed;

        // Chance to drop power-up
        TryDropPowerUp(enemy.transform.position);
    }

    public void RegisterExternalEnemy()
    {
        enemiesAliveCount++;
    }

    public void UnregisterExternalEnemy()
    {
        enemiesAliveCount = Mathf.Max(0, enemiesAliveCount - 1);
    }

    // =========================================================================
    // Power-Up Drop
    // =========================================================================
    private void TryDropPowerUp(Vector3 position)
    {
        if (powerUpPrefabs == null || powerUpPrefabs.Length == 0) return;
        if (Random.value > powerUpDropChance) return;

        int index = Random.Range(0, powerUpPrefabs.Length);
        if (powerUpPrefabs[index] != null)
        {
            Instantiate(powerUpPrefabs[index], position, Quaternion.identity);
        }
    }

    // =========================================================================
    // Auto-generate waves when no predefined data is set
    // =========================================================================
    private WaveDefinition[] GenerateWaves(int count)
    {
        WaveDefinition[] waves = new WaveDefinition[count];

        for (int w = 0; w < count; w++)
        {
            WaveDefinition wave = new WaveDefinition();
            wave.waveName = $"Wave {w + 1}";
            wave.delayBeforeWave = w == 0 ? 3f : 2f;
            wave.spawnInterval = Mathf.Max(0.4f, 1.5f - w * 0.1f);

            bool isBoss = (w == count - 1) || ((w + 1) % 5 == 0);
            wave.isBossWave = isBoss;

            List<EnemySpawnEntry> entries = new List<EnemySpawnEntry>();

            if (isBoss && bossPrefab != null)
            {
                entries.Add(new EnemySpawnEntry
                {
                    enemyPrefab = bossPrefab,
                    count = 1,
                    pattern = SpawnPattern.Random
                });
            }

            // Basic enemies scale with wave number
            if (enemyPrefabs.Length > 0 && enemyPrefabs[0] != null)
            {
                entries.Add(new EnemySpawnEntry
                {
                    enemyPrefab = enemyPrefabs[0],
                    count = 3 + w,
                    pattern = (SpawnPattern)(w % 6)
                });
            }

            // Fast enemies from wave 3+
            if (w >= 2 && enemyPrefabs.Length > 1 && enemyPrefabs[1] != null)
            {
                entries.Add(new EnemySpawnEntry
                {
                    enemyPrefab = enemyPrefabs[1],
                    count = 1 + w / 2,
                    pattern = SpawnPattern.ZigZag
                });
            }

            // Tank enemies from wave 5+
            if (w >= 4 && enemyPrefabs.Length > 2 && enemyPrefabs[2] != null)
            {
                entries.Add(new EnemySpawnEntry
                {
                    enemyPrefab = enemyPrefabs[2],
                    count = 1 + (w - 4) / 2,
                    pattern = SpawnPattern.VFormation
                });
            }

            // Shooter enemies from wave 7+
            if (w >= 6 && enemyPrefabs.Length > 3 && enemyPrefabs[3] != null)
            {
                entries.Add(new EnemySpawnEntry
                {
                    enemyPrefab = enemyPrefabs[3],
                    count = 1 + (w - 6),
                    pattern = SpawnPattern.Random
                });
            }

            wave.enemies = entries.ToArray();
            waves[w] = wave;
        }

        return waves;
    }
}

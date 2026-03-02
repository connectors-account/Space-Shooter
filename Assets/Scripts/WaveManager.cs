using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance { get; private set; }

    [Header("Wave Settings")]
    public int currentWave = 0;
    public float timeBetweenWaves = 3f;
    public int bossWaveInterval = 5; // Boss every 5 waves

    [Header("Boss")]
    public GameObject bossPrefab;

    [Header("Events")]
    public UnityEvent<int> OnWaveStarted;
    public UnityEvent<int> OnWaveCompleted;
    public UnityEvent OnBossWave;

    private int enemiesAlive = 0;
    private bool isWaveActive = false;
    private bool isBossWave = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void StartWaves()
    {
        currentWave = 0;
        StartNextWave();
    }

    public void StartNextWave()
    {
        currentWave++;
        isWaveActive = true;
        enemiesAlive = 0;

        // Check if this is a boss wave
        isBossWave = (currentWave % bossWaveInterval == 0);

        OnWaveStarted?.Invoke(currentWave);
        UIManager.Instance?.UpdateWaveCounter(currentWave);

        if (isBossWave)
        {
            OnBossWave?.Invoke();
            SpawnBoss();
        }
        else
        {
            EnemySpawner.Instance?.SetWave(currentWave);
            EnemySpawner.Instance?.StartSpawning();
        }
    }

    void SpawnBoss()
    {
        if (bossPrefab != null)
        {
            EnemySpawner.Instance?.SpawnBoss(bossPrefab);
            enemiesAlive = 1;
        }
        else
        {
            // No boss prefab, just start next wave
            StartCoroutine(WaveTransition());
        }
    }

    public void OnEnemySpawned()
    {
        enemiesAlive++;
    }

    public void OnEnemyKilled()
    {
        enemiesAlive--;

        // Check if wave is complete
        if (isWaveActive && enemiesAlive <= 0)
        {
            bool spawnerFinished = EnemySpawner.Instance == null || EnemySpawner.Instance.HasFinishedSpawningWave();
            
            if (spawnerFinished || isBossWave)
            {
                CompleteWave();
            }
        }
    }

    void CompleteWave()
    {
        isWaveActive = false;
        EnemySpawner.Instance?.StopSpawning();
        
        OnWaveCompleted?.Invoke(currentWave);

        // Award bonus points for completing wave
        int waveBonus = currentWave * 500;
        ScoreManager.Instance?.AddScore(waveBonus);

        StartCoroutine(WaveTransition());
    }

    IEnumerator WaveTransition()
    {
        UIManager.Instance?.ShowWaveComplete(currentWave);
        
        yield return new WaitForSeconds(timeBetweenWaves);

        if (GameManager.Instance != null && !GameManager.Instance.IsGameOver())
        {
            StartNextWave();
        }
    }

    public int GetCurrentWave()
    {
        return currentWave;
    }

    public bool IsBossWave()
    {
        return isBossWave;
    }

    void Update()
    {
        // Count enemies periodically for accuracy
        if (isWaveActive && !isBossWave)
        {
            enemiesAlive = GameObject.FindGameObjectsWithTag("Enemy").Length;
        }
    }
}

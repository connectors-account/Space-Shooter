using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [System.Serializable]
    public class WaveConfig
    {
        public int enemyCount = 8;
        public float spawnInterval = 1f;
        public int enemyHealth = 1;
        public int enemyScore = 100;
        public float enemySpeed = 2.5f;
        public bool enemiesCanShoot;
        public float enemyShootCooldown = 1.8f;
    }

    [Header("Prefabs")]
    [SerializeField] private EnemyController[] enemyPrefabs;

    [Header("Spawn Area")]
    [SerializeField] private float minX = -8f;
    [SerializeField] private float maxX = 8f;
    [SerializeField] private float spawnY = 6f;

    [Header("5 Waves (Difficulty Ramps)")]
    [SerializeField] private WaveConfig[] waveConfigs = new WaveConfig[5];

    private readonly List<EnemyController> activeEnemies = new List<EnemyController>();
    private Coroutine waveRoutine;
    private int spawnedThisWave;

    private void Awake()
    {
        EnsureWaveConfigs();
    }

    private void Reset()
    {
        waveConfigs = BuildDefaultWaves();
    }

    private void EnsureWaveConfigs()
    {
        if (waveConfigs == null || waveConfigs.Length != 5)
        {
            waveConfigs = BuildDefaultWaves();
            return;
        }

        for (int i = 0; i < waveConfigs.Length; i++)
        {
            if (waveConfigs[i] == null)
            {
                waveConfigs = BuildDefaultWaves();
                return;
            }
        }
    }

    private WaveConfig[] BuildDefaultWaves()
    {
        return new[]
        {
            new WaveConfig { enemyCount = 6,  spawnInterval = 1.2f, enemyHealth = 1, enemyScore = 100, enemySpeed = 2.2f, enemiesCanShoot = false, enemyShootCooldown = 1.6f },
            new WaveConfig { enemyCount = 8,  spawnInterval = 1.0f, enemyHealth = 1, enemyScore = 120, enemySpeed = 2.6f, enemiesCanShoot = true,  enemyShootCooldown = 1.7f },
            new WaveConfig { enemyCount = 10, spawnInterval = 0.9f, enemyHealth = 2, enemyScore = 160, enemySpeed = 3.0f, enemiesCanShoot = true,  enemyShootCooldown = 1.5f },
            new WaveConfig { enemyCount = 12, spawnInterval = 0.8f, enemyHealth = 2, enemyScore = 190, enemySpeed = 3.3f, enemiesCanShoot = true,  enemyShootCooldown = 1.3f },
            new WaveConfig { enemyCount = 15, spawnInterval = 0.7f, enemyHealth = 3, enemyScore = 240, enemySpeed = 3.7f, enemiesCanShoot = true,  enemyShootCooldown = 1.1f }
        };
    }

    public void BeginWave(int waveNumber)
    {
        if (waveConfigs == null || waveConfigs.Length == 0)
        {
            Debug.LogError("SpawnManager needs wave configs.");
            return;
        }

        int index = Mathf.Clamp(waveNumber - 1, 0, waveConfigs.Length - 1);

        if (waveRoutine != null)
        {
            StopCoroutine(waveRoutine);
        }

        spawnedThisWave = 0;
        waveRoutine = StartCoroutine(SpawnWaveRoutine(waveConfigs[index], waveNumber));
    }

    public void StopSpawning()
    {
        if (waveRoutine != null)
        {
            StopCoroutine(waveRoutine);
            waveRoutine = null;
        }
    }

    public void ClearAllEnemies()
    {
        StopSpawning();

        foreach (EnemyController enemy in activeEnemies)
        {
            if (enemy != null)
            {
                Destroy(enemy.gameObject);
            }
        }

        activeEnemies.Clear();

        foreach (GameObject bullet in GameObject.FindGameObjectsWithTag("EnemyBullet"))
        {
            Destroy(bullet);
        }

        foreach (GameObject bullet in GameObject.FindGameObjectsWithTag("PlayerBullet"))
        {
            Destroy(bullet);
        }

        foreach (GameObject powerUp in GameObject.FindGameObjectsWithTag("PowerUp"))
        {
            Destroy(powerUp);
        }
    }

    private IEnumerator SpawnWaveRoutine(WaveConfig config, int waveNumber)
    {
        yield return new WaitForSeconds(0.75f);

        while (spawnedThisWave < config.enemyCount && GameManager.Instance != null && GameManager.Instance.CurrentState == GameManager.GameState.Playing)
        {
            SpawnSingleEnemy(config, waveNumber);
            spawnedThisWave++;
            yield return new WaitForSeconds(config.spawnInterval);
        }

        waveRoutine = null;
    }

    private void SpawnSingleEnemy(WaveConfig config, int waveNumber)
    {
        if (enemyPrefabs == null || enemyPrefabs.Length == 0)
        {
            return;
        }

        EnemyController prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
        Vector3 position = new Vector3(Random.Range(minX, maxX), spawnY, 0f);

        EnemyController enemy = Instantiate(prefab, position, Quaternion.identity);
        enemy.tag = "Enemy";

        EnemyController.MovementType movement = EnemyController.MovementType.Straight;
        if (waveNumber >= 3)
        {
            movement = Random.value < 0.5f ? EnemyController.MovementType.Sine : EnemyController.MovementType.Drift;
        }

        float horizontal = Random.Range(-1f, 1f);

        enemy.Configure(
            config.enemyHealth,
            config.enemyScore,
            config.enemySpeed,
            horizontal,
            config.enemiesCanShoot,
            config.enemyShootCooldown,
            movement,
            HandleEnemyRemoved);

        activeEnemies.Add(enemy);
    }

    private void HandleEnemyRemoved(EnemyController enemy, bool killedByPlayer, int scoreValue)
    {
        activeEnemies.Remove(enemy);

        if (killedByPlayer)
        {
            GameManager.Instance?.AddScore(scoreValue);
            AudioManager.Instance?.PlayExplosion();
        }

        int activeCount = 0;
        for (int i = 0; i < activeEnemies.Count; i++)
        {
            if (activeEnemies[i] != null)
            {
                activeCount++;
            }
        }

        int expectedCount = GameManager.Instance == null ? spawnedThisWave : GetCurrentWaveEnemyCount();
        bool allSpawned = waveRoutine == null || spawnedThisWave >= expectedCount;
        if (allSpawned && activeCount == 0)
        {
            GameManager.Instance?.OnWaveCleared();
        }
    }

    private int GetCurrentWaveEnemyCount()
    {
        int index = Mathf.Clamp(GameManager.Instance.CurrentWave - 1, 0, waveConfigs.Length - 1);
        return waveConfigs[index].enemyCount;
    }
}

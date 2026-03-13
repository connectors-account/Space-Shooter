using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Manages enemy wave spawning with progressive difficulty.
/// Each wave increases enemy count, speed, and health.
/// Every 5th wave spawns a boss.
/// </summary>
public class WaveManager : MonoBehaviour
{
    [Header("Enemy Prefabs")]
    public GameObject enemyStraightPrefab;
    public GameObject enemyZigzagPrefab;
    public GameObject enemyDiverPrefab;
    public GameObject bossPrefab;

    [Header("Wave Settings")]
    public float timeBetweenWaves = 5f;
    public float spawnInterval = 1f;
    public int baseEnemiesPerWave = 5;
    public float difficultyMultiplier = 1.15f;

    [Header("State")]
    private int currentWave = 0;
    private int enemiesAlive = 0;
    private bool waveInProgress = false;
    private bool bossWave = false;
    private bool bossAlive = false;

    // Events
    public System.Action<int> OnWaveStart;
    public System.Action<int> OnWaveComplete;
    public System.Action OnBossSpawned;

    public int CurrentWave => currentWave;

    private void Start()
    {
        StartCoroutine(WaveLoop());
    }

    private IEnumerator WaveLoop()
    {
        // Wait for game to be active
        while (GameManager.Instance == null || !GameManager.Instance.isGameActive)
        {
            yield return new WaitForSeconds(0.5f);
        }

        // Initial delay
        yield return new WaitForSeconds(2f);

        while (GameManager.Instance != null && GameManager.Instance.isGameActive)
        {
            currentWave++;
            bossWave = (currentWave % 5 == 0);

            OnWaveStart?.Invoke(currentWave);

            if (bossWave)
            {
                yield return StartCoroutine(SpawnBossWave());
            }
            else
            {
                yield return StartCoroutine(SpawnEnemyWave());
            }

            // Wait for all enemies to be destroyed
            while (enemiesAlive > 0 || bossAlive)
            {
                // Count active enemies
                enemiesAlive = FindObjectsOfType<EnemyBase>().Length;
                bossAlive = FindObjectOfType<EnemyBoss>() != null;

                if (!GameManager.Instance.isGameActive) yield break;
                yield return new WaitForSeconds(0.5f);
            }

            OnWaveComplete?.Invoke(currentWave);

            // Wait between waves
            yield return new WaitForSeconds(timeBetweenWaves);
        }
    }

    private IEnumerator SpawnEnemyWave()
    {
        waveInProgress = true;
        float difficulty = Mathf.Pow(difficultyMultiplier, currentWave - 1);
        int enemyCount = Mathf.RoundToInt(baseEnemiesPerWave + (currentWave - 1) * 2);
        enemyCount = Mathf.Min(enemyCount, 25); // Cap at 25

        float currentSpawnInterval = Mathf.Max(0.3f, spawnInterval / difficulty);

        for (int i = 0; i < enemyCount; i++)
        {
            if (GameManager.Instance == null || !GameManager.Instance.isGameActive) yield break;

            SpawnRandomEnemy(difficulty);
            yield return new WaitForSeconds(currentSpawnInterval);
        }

        waveInProgress = false;
    }

    private void SpawnRandomEnemy(float difficulty)
    {
        if (GameBounds.Instance == null) return;

        Vector3 spawnPos = GameBounds.Instance.GetRandomTopSpawnPosition();
        GameObject prefab;

        // Choose enemy type based on wave difficulty
        float roll = Random.value;
        if (currentWave >= 3 && roll < 0.2f)
        {
            prefab = enemyDiverPrefab;
        }
        else if (currentWave >= 2 && roll < 0.5f)
        {
            prefab = enemyZigzagPrefab;
        }
        else
        {
            prefab = enemyStraightPrefab;
        }

        if (prefab != null)
        {
            GameObject enemy = Instantiate(prefab, spawnPos, Quaternion.identity);

            // Scale stats with difficulty
            EnemyBase enemyBase = enemy.GetComponent<EnemyBase>();
            if (enemyBase != null)
            {
                enemyBase.maxHealth = Mathf.RoundToInt(enemyBase.maxHealth * difficulty);
                enemyBase.moveSpeed *= Mathf.Min(difficulty, 2f);
                enemyBase.scoreValue = Mathf.RoundToInt(enemyBase.scoreValue * difficulty);
            }

            enemiesAlive++;
        }
    }

    private IEnumerator SpawnBossWave()
    {
        // Spawn some minions first
        int minionCount = 3 + currentWave / 5;
        float difficulty = Mathf.Pow(difficultyMultiplier, currentWave - 1);

        for (int i = 0; i < minionCount; i++)
        {
            if (GameManager.Instance == null || !GameManager.Instance.isGameActive) yield break;
            SpawnRandomEnemy(difficulty);
            yield return new WaitForSeconds(0.5f);
        }

        yield return new WaitForSeconds(2f);

        // Spawn boss
        if (bossPrefab != null && GameBounds.Instance != null)
        {
            Vector3 bossSpawnPos = new Vector3(0f, GameBounds.Instance.maxY + 2f, 0f);
            GameObject boss = Instantiate(bossPrefab, bossSpawnPos, Quaternion.identity);

            EnemyBoss bossScript = boss.GetComponent<EnemyBoss>();
            if (bossScript != null)
            {
                float bossMultiplier = 1f + (currentWave / 5 - 1) * 0.5f;
                bossScript.maxHealth = Mathf.RoundToInt(bossScript.maxHealth * bossMultiplier);
                bossScript.scoreValue = Mathf.RoundToInt(bossScript.scoreValue * bossMultiplier);
            }

            bossAlive = true;
            OnBossSpawned?.Invoke();
        }
    }

    public void OnBossDefeated()
    {
        bossAlive = false;
    }
}

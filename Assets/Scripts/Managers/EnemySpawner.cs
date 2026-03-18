using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Spawns enemies in waves with increasing difficulty.
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy Prefabs")]
    [SerializeField] private GameObject enemyStraightPrefab;
    [SerializeField] private GameObject enemyZigzagPrefab;
    [SerializeField] private GameObject enemyTankPrefab;
    [SerializeField] private GameObject enemyDiverPrefab;

    [Header("Spawn Settings")]
    [SerializeField] private float spawnYPosition = 6f;
    [SerializeField] private float spawnXRange = 5f;
    [SerializeField] private float minSpawnInterval = 0.5f;
    [SerializeField] private float maxSpawnInterval = 2f;

    private bool isSpawning = false;
    private int waveNumber = 0;

    /// <summary>
    /// Start spawning a wave.
    /// </summary>
    public void StartWave(int wave)
    {
        waveNumber = wave;
        StartCoroutine(SpawnWave());
    }

    private IEnumerator SpawnWave()
    {
        isSpawning = true;

        // Calculate wave composition
        int baseEnemies = 5;
        int totalEnemies = baseEnemies + (waveNumber - 1) * 3;
        totalEnemies = Mathf.Min(totalEnemies, 30); // cap

        float healthMultiplier = 1f + (waveNumber - 1) * 0.15f;
        float speedMultiplier = 1f + (waveNumber - 1) * 0.05f;
        speedMultiplier = Mathf.Min(speedMultiplier, 2f);

        GameManager.Instance?.SetWaveEnemyCount(totalEnemies);

        // Determine spawn interval (decreases with wave)
        float interval = Mathf.Lerp(maxSpawnInterval, minSpawnInterval,
            Mathf.Clamp01((waveNumber - 1) / 15f));

        for (int i = 0; i < totalEnemies; i++)
        {
            if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing)
            {
                isSpawning = false;
                yield break;
            }

            SpawnEnemy(healthMultiplier, speedMultiplier);

            yield return new WaitForSeconds(interval);
        }

        isSpawning = false;
    }

    private void SpawnEnemy(float healthMult, float speedMult)
    {
        float xPos = Random.Range(-spawnXRange, spawnXRange);
        Vector3 spawnPos = new Vector3(xPos, spawnYPosition, 0);

        GameObject prefab = ChooseEnemyType();
        if (prefab == null) return;

        GameObject enemy = Instantiate(prefab, spawnPos, Quaternion.identity);
        EnemyBase eb = enemy.GetComponent<EnemyBase>();
        if (eb != null)
        {
            eb.ScaleStats(healthMult, speedMult);
        }
    }

    private GameObject ChooseEnemyType()
    {
        // Weighted random based on wave number
        float roll = Random.value;

        if (waveNumber <= 2)
        {
            // Early waves: mostly straight enemies
            if (roll < 0.7f) return enemyStraightPrefab;
            if (roll < 0.9f) return enemyDiverPrefab;
            return enemyZigzagPrefab;
        }
        else if (waveNumber <= 5)
        {
            if (roll < 0.4f) return enemyStraightPrefab;
            if (roll < 0.65f) return enemyZigzagPrefab;
            if (roll < 0.85f) return enemyDiverPrefab;
            return enemyTankPrefab;
        }
        else
        {
            // Late waves: all types including more tanks
            if (roll < 0.25f) return enemyStraightPrefab;
            if (roll < 0.50f) return enemyZigzagPrefab;
            if (roll < 0.75f) return enemyDiverPrefab;
            return enemyTankPrefab;
        }
    }
}

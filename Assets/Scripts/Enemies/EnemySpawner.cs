using UnityEngine;
using System.Collections;

/// <summary>
/// Manages enemy wave spawning. Selects enemy types based on wave number
/// and spawns them at random positions along the top of the screen.
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy Pool Tags")]
    public string straightEnemyTag = "EnemyStraight";
    public string zigzagEnemyTag = "EnemyZigZag";
    public string diverEnemyTag = "EnemyDiver";
    public string tankEnemyTag = "EnemyTank";

    [Header("Spawn Settings")]
    public float spawnInterval = 1.5f;
    public float spawnYOffset = 1.5f;

    private int enemiesLeftToSpawn;
    private int currentWaveNumber;
    private Coroutine spawnCoroutine;

    /// <summary>
    /// Called by GameManager to start spawning a new wave.
    /// </summary>
    public void BeginWave(int waveNumber, int totalEnemies)
    {
        currentWaveNumber = waveNumber;
        enemiesLeftToSpawn = totalEnemies;

        if (spawnCoroutine != null)
            StopCoroutine(spawnCoroutine);

        spawnCoroutine = StartCoroutine(SpawnWaveCoroutine());
    }

    private IEnumerator SpawnWaveCoroutine()
    {
        while (enemiesLeftToSpawn > 0)
        {
            if (GameManager.Instance.CurrentState != GameState.Playing)
            {
                yield return null;
                continue;
            }

            SpawnEnemy();
            enemiesLeftToSpawn--;

            float adjustedInterval = Mathf.Max(0.3f, spawnInterval - currentWaveNumber * 0.05f);
            yield return new WaitForSeconds(adjustedInterval);
        }
    }

    private void SpawnEnemy()
    {
        if (ObjectPool.Instance == null || GameBounds.Instance == null) return;

        string enemyTag = ChooseEnemyType();
        float spawnX = Random.Range(GameBounds.Instance.MinX, GameBounds.Instance.MaxX);
        float spawnY = GameBounds.Instance.MaxY + spawnYOffset;
        Vector3 spawnPos = new Vector3(spawnX, spawnY, 0f);

        GameObject enemy = ObjectPool.Instance.Spawn(enemyTag, spawnPos, Quaternion.identity);
        if (enemy != null)
        {
            GameManager.Instance?.EnemySpawned();
        }
    }

    /// <summary>
    /// Selects enemy type based on current wave. Higher waves unlock harder enemies.
    /// </summary>
    private string ChooseEnemyType()
    {
        float roll = Random.value;

        // Wave 1-2: Only straight enemies
        if (currentWaveNumber <= 2)
        {
            return straightEnemyTag;
        }
        // Wave 3-4: Introduce zigzag enemies
        else if (currentWaveNumber <= 4)
        {
            if (roll < 0.6f) return straightEnemyTag;
            return zigzagEnemyTag;
        }
        // Wave 5-6: Introduce diver enemies
        else if (currentWaveNumber <= 6)
        {
            if (roll < 0.35f) return straightEnemyTag;
            if (roll < 0.65f) return zigzagEnemyTag;
            return diverEnemyTag;
        }
        // Wave 7+: All enemy types including tanks
        else
        {
            if (roll < 0.25f) return straightEnemyTag;
            if (roll < 0.50f) return zigzagEnemyTag;
            if (roll < 0.75f) return diverEnemyTag;
            return tankEnemyTag;
        }
    }

    public void StopSpawning()
    {
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }
    }
}

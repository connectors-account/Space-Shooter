using System.Collections;
using UnityEngine;

/// <summary>
/// Spawns waves of enemies with increasing difficulty. Each wave spawns a set
/// number of enemies; once they're all destroyed (or spawned and gone) the next
/// wave begins after a short break, with more/faster enemies.
///
/// Enable this component only while the game is in the Playing state
/// (the GameManager handles that).
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [Header("Prefabs & Pools")]
    [Tooltip("Enemy prefab to spawn (must have Enemy + HealthSystem).")]
    public GameObject enemyPrefab;

    [Tooltip("Bullet pool passed to spawned enemies so they can shoot.")]
    public BulletPool enemyBulletPool;

    [Tooltip("Power-up prefab passed to enemies for drops (optional).")]
    public GameObject powerUpPrefab;

    [Header("Spawn Area")]
    [Tooltip("Y position where enemies appear (top of screen).")]
    public float spawnY = 6f;

    [Tooltip("Horizontal range enemies can spawn within (-x to +x).")]
    public float spawnXRange = 8f;

    [Header("Wave Tuning")]
    [Tooltip("Enemies in the first wave.")]
    public int baseEnemiesPerWave = 5;

    [Tooltip("Extra enemies added each subsequent wave.")]
    public int enemiesAddedPerWave = 2;

    [Tooltip("Seconds between individual spawns within a wave.")]
    public float spawnInterval = 0.8f;

    [Tooltip("Pause between waves in seconds.")]
    public float timeBetweenWaves = 3f;

    [Tooltip("Each wave multiplies enemy speed by this (capped).")]
    public float speedRampPerWave = 0.12f;

    /// <summary>Current wave number (1-based). Exposed for the UI.</summary>
    public int CurrentWave { get; private set; }

    private Coroutine spawnRoutine;

    private void OnEnable()
    {
        // Begin the spawning loop whenever the spawner is enabled.
        spawnRoutine = StartCoroutine(SpawnLoop());
    }

    private void OnDisable()
    {
        // Stop spawning when disabled (e.g. on game over).
        if (spawnRoutine != null)
            StopCoroutine(spawnRoutine);
    }

    /// <summary>Reset wave progression so a new game starts at wave 1.</summary>
    public void ResetSpawner()
    {
        CurrentWave = 0;
    }

    /// <summary>Main loop: spawn a wave, wait, then spawn a tougher one.</summary>
    private IEnumerator SpawnLoop()
    {
        // Brief delay before the first wave so the player can get ready.
        yield return new WaitForSeconds(1.5f);

        while (true)
        {
            CurrentWave++;
            int enemyCount = baseEnemiesPerWave + (CurrentWave - 1) * enemiesAddedPerWave;

            // Notify UI of the new wave.
            if (UIManager.Instance != null)
                UIManager.Instance.ShowWaveBanner(CurrentWave);

            for (int i = 0; i < enemyCount; i++)
            {
                SpawnEnemy();
                yield return new WaitForSeconds(spawnInterval);
            }

            // Wait before the next wave.
            yield return new WaitForSeconds(timeBetweenWaves);
        }
    }

    /// <summary>Instantiate a single enemy with wave-scaled difficulty.</summary>
    private void SpawnEnemy()
    {
        if (enemyPrefab == null)
            return;

        float x = Random.Range(-spawnXRange, spawnXRange);
        Vector3 pos = new Vector3(x, spawnY, 0f);

        GameObject enemyObj = Instantiate(enemyPrefab, pos, Quaternion.identity);

        // Configure the enemy based on the current wave.
        Enemy enemy = enemyObj.GetComponent<Enemy>();
        if (enemy != null)
        {
            // Speed ramps up each wave but is capped so it stays fair.
            float speedMultiplier = 1f + Mathf.Min(CurrentWave * speedRampPerWave, 1.5f);
            enemy.moveSpeed *= speedMultiplier;

            // Enemies start shooting from wave 2 onward.
            if (CurrentWave >= 2)
                enemy.fireInterval = Mathf.Max(0.8f, 2.5f - CurrentWave * 0.15f);

            enemy.bulletPool = enemyBulletPool;
            enemy.powerUpPrefab = powerUpPrefab;
        }

        // Scale enemy health a little each wave for added challenge.
        HealthSystem hp = enemyObj.GetComponent<HealthSystem>();
        if (hp != null)
        {
            hp.maxHealth += (CurrentWave - 1) * 10;
            hp.ResetHealth();
        }
    }
}

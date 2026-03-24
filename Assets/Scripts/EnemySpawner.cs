using UnityEngine;
using System.Collections;

/// <summary>
/// EnemySpawner handles wave-based spawning of enemies.
/// Each wave increases in difficulty: more enemies, faster, tougher.
/// Every 5th wave spawns a boss enemy.
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    // ── Singleton ────────────────────────────────────────────
    public static EnemySpawner Instance { get; private set; }

    // ── Prefab ───────────────────────────────────────────────
    [Header("Enemy Prefab")]
    [SerializeField] private GameObject enemyPrefab;

    // ── Wave Settings ────────────────────────────────────────
    [Header("Wave Configuration")]
    [SerializeField] private int baseEnemiesPerWave = 5;
    [SerializeField] private int enemiesPerWaveIncrease = 2;
    [SerializeField] private float spawnInterval = 1.0f;
    [SerializeField] private float spawnIntervalReduction = 0.05f;
    [SerializeField] private float minSpawnInterval = 0.3f;
    [SerializeField] private float timeBetweenWaves = 3.0f;

    // ── Spawn Area ───────────────────────────────────────────
    [Header("Spawn Area")]
    [SerializeField] private float spawnYPosition = 6.5f;
    [SerializeField] private float spawnXMin = -7f;
    [SerializeField] private float spawnXMax = 7f;

    // ── Internal State ───────────────────────────────────────
    private int currentWave = 0;
    private int enemiesRemainingInWave = 0;
    private int enemiesAlive = 0;
    private bool spawning = false;

    // ──────────────────────────────────────────────────────────
    // Unity Lifecycle
    // ──────────────────────────────────────────────────────────

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    // ──────────────────────────────────────────────────────────
    // Public API
    // ──────────────────────────────────────────────────────────

    /// <summary>
    /// Start spawning waves. Called by GameManager when gameplay begins.
    /// </summary>
    public void StartSpawning()
    {
        currentWave = 0;
        spawning = true;
        StartCoroutine(WaveLoop());
    }

    /// <summary>
    /// Stop all spawning (e.g. on game over).
    /// </summary>
    public void StopSpawning()
    {
        spawning = false;
        StopAllCoroutines();
    }

    /// <summary>
    /// Called by EnemyController when an enemy is destroyed or leaves screen.
    /// </summary>
    public void OnEnemyDestroyed()
    {
        enemiesAlive = Mathf.Max(0, enemiesAlive - 1);
    }

    public int CurrentWave => currentWave;

    // ──────────────────────────────────────────────────────────
    // Wave Loop
    // ──────────────────────────────────────────────────────────

    private IEnumerator WaveLoop()
    {
        while (spawning)
        {
            currentWave++;

            // Notify GameManager and UI
            if (GameManager.Instance != null)
                GameManager.Instance.OnNewWave(currentWave);

            if (UIManager.Instance != null)
                UIManager.Instance.ShowWaveAnnouncement(currentWave);

            yield return new WaitForSeconds(1.5f); // Brief pause before spawning

            // Determine enemies for this wave
            bool isBossWave = (currentWave % 5 == 0);
            int enemyCount = isBossWave ? 1 : baseEnemiesPerWave + (currentWave - 1) * enemiesPerWaveIncrease;
            float interval = Mathf.Max(minSpawnInterval, spawnInterval - (currentWave - 1) * spawnIntervalReduction);

            enemiesRemainingInWave = enemyCount;
            enemiesAlive = 0;

            // Spawn enemies one at a time
            for (int i = 0; i < enemyCount; i++)
            {
                if (!spawning) yield break;

                if (isBossWave)
                    SpawnBoss();
                else
                    SpawnRandomEnemy();

                enemiesAlive++;
                enemiesRemainingInWave--;

                yield return new WaitForSeconds(interval);
            }

            // Wait until all enemies in this wave are destroyed
            while (enemiesAlive > 0 && spawning)
            {
                yield return new WaitForSeconds(0.25f);
            }

            // Brief pause between waves
            yield return new WaitForSeconds(timeBetweenWaves);
        }
    }

    // ──────────────────────────────────────────────────────────
    // Spawn Helpers
    // ──────────────────────────────────────────────────────────

    private void SpawnRandomEnemy()
    {
        if (enemyPrefab == null) return;

        Vector3 pos = new Vector3(Random.Range(spawnXMin, spawnXMax), spawnYPosition, 0f);
        GameObject enemy = Instantiate(enemyPrefab, pos, Quaternion.identity);

        EnemyController ec = enemy.GetComponent<EnemyController>();
        if (ec == null) return;

        // Pick a random type weighted by wave number
        float roll = Random.value;
        if (currentWave <= 2)
        {
            // Early waves: mostly straight
            if (roll < 0.7f)
                ec.Setup(EnemyController.EnemyType.Straight, 1, 100, 2.5f + currentWave * 0.2f, false);
            else
                ec.Setup(EnemyController.EnemyType.Zigzag, 1, 150, 2f + currentWave * 0.2f, false);
        }
        else if (currentWave <= 5)
        {
            // Mid waves: mix of types, some can shoot
            if (roll < 0.35f)
                ec.Setup(EnemyController.EnemyType.Straight, 2, 100, 3f + currentWave * 0.15f, true);
            else if (roll < 0.65f)
                ec.Setup(EnemyController.EnemyType.Zigzag, 2, 150, 2.5f + currentWave * 0.15f, true);
            else if (roll < 0.85f)
                ec.Setup(EnemyController.EnemyType.Sine, 2, 200, 2.5f, true);
            else
                ec.Setup(EnemyController.EnemyType.Charger, 1, 250, 3.5f, false);
        }
        else
        {
            // Hard waves: tougher, faster, all can shoot
            int hp = 2 + currentWave / 4;
            if (roll < 0.25f)
                ec.Setup(EnemyController.EnemyType.Straight, hp, 100 + currentWave * 10, 3.5f + currentWave * 0.1f, true);
            else if (roll < 0.50f)
                ec.Setup(EnemyController.EnemyType.Zigzag, hp, 150 + currentWave * 10, 3f + currentWave * 0.1f, true);
            else if (roll < 0.75f)
                ec.Setup(EnemyController.EnemyType.Sine, hp, 200 + currentWave * 10, 2.5f + currentWave * 0.1f, true);
            else
                ec.Setup(EnemyController.EnemyType.Charger, hp - 1, 250 + currentWave * 10, 4f, true);
        }
    }

    private void SpawnBoss()
    {
        if (enemyPrefab == null) return;

        Vector3 pos = new Vector3(0f, spawnYPosition + 1f, 0f);
        GameObject enemy = Instantiate(enemyPrefab, pos, Quaternion.identity);

        // Make the boss visually larger
        enemy.transform.localScale = Vector3.one * 2f;

        EnemyController ec = enemy.GetComponent<EnemyController>();
        if (ec != null)
        {
            int bossHP = 10 + currentWave * 2;
            int bossScore = 1000 + currentWave * 100;
            ec.Setup(EnemyController.EnemyType.Boss, bossHP, bossScore, 1.5f, true);
        }
    }
}

using UnityEngine;
using System.Collections;

/// <summary>
/// Manages enemy wave spawning.  Each wave spawns more / tougher enemies.
/// Notifies GameManager when a wave is cleared.
/// </summary>
public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance { get; private set; }

    [Header("Enemy Prefabs (at least one required)")]
    [SerializeField] private GameObject[] enemyPrefabs;

    [Header("Spawn Settings")]
    [SerializeField] private float spawnYOffset    = 6.5f;
    [SerializeField] private float spawnXRange     = 7f;
    [SerializeField] private float delayBetweenSpawns = 0.6f;
    [SerializeField] private float waveCooldown    = 3f;

    [Header("Wave Scaling")]
    [SerializeField] private int   baseEnemiesPerWave  = 5;
    [SerializeField] private int   extraEnemiesPerWave = 2;
    [SerializeField] private float speedIncreasePerWave = 0.3f;

    private int  enemiesAlive;
    private bool spawning;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        StartCoroutine(WaveLoop());
    }

    // Called by EnemyController when it dies or goes off-screen
    public void OnEnemyDestroyed()
    {
        enemiesAlive = Mathf.Max(0, enemiesAlive - 1);
    }

    private IEnumerator WaveLoop()
    {
        // Small initial delay
        yield return new WaitForSeconds(1.5f);

        while (true)
        {
            if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
            {
                yield return null;
                continue;
            }

            int wave = GameManager.Instance != null ? GameManager.Instance.CurrentWave : 1;
            int count = baseEnemiesPerWave + (wave - 1) * extraEnemiesPerWave;

            yield return StartCoroutine(SpawnWave(wave, count));

            // Wait until all enemies in the wave are destroyed
            while (enemiesAlive > 0)
                yield return null;

            // Wave cleared
            GameManager.Instance?.AdvanceWave();
            AudioManager.Instance?.PlaySFX("WaveClear");

            yield return new WaitForSeconds(waveCooldown);
        }
    }

    private IEnumerator SpawnWave(int wave, int count)
    {
        spawning = true;

        for (int i = 0; i < count; i++)
        {
            if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
                yield break;

            SpawnEnemy(wave);
            yield return new WaitForSeconds(delayBetweenSpawns);
        }

        spawning = false;
    }

    private void SpawnEnemy(int wave)
    {
        if (enemyPrefabs == null || enemyPrefabs.Length == 0) return;

        // Pick a random prefab
        int prefabIdx = Random.Range(0, enemyPrefabs.Length);
        if (enemyPrefabs[prefabIdx] == null) return;

        float x = Random.Range(-spawnXRange, spawnXRange);
        Vector3 pos = new Vector3(x, spawnYOffset, 0f);

        GameObject go = Instantiate(enemyPrefabs[prefabIdx], pos, Quaternion.identity);
        EnemyController enemy = go.GetComponent<EnemyController>();
        if (enemy != null)
        {
            // Scale difficulty with wave
            float speed = 2f + wave * speedIncreasePerWave;
            int hp = 1 + wave / 3;
            int score = 100 + wave * 20;
            bool shoots = wave >= 2 || Random.value > 0.5f;

            EnemyController.MovePattern pat = (EnemyController.MovePattern)Random.Range(0, 4);

            enemy.Setup(pat, speed, hp, score, shoots);
        }

        enemiesAlive++;
    }
}

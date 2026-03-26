using System.Collections;
using UnityEngine;

/// <summary>
/// Drives wave-based enemy spawning with escalating difficulty.
/// Place on an empty GameObject in GameScene.
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy Prefabs – assign in Inspector")]
    public GameObject basicEnemyPrefab;
    public GameObject zigzagEnemyPrefab;
    public GameObject tankEnemyPrefab;
    public GameObject bossEnemyPrefab;

    [Header("Spawn Settings")]
    public float spawnDelay       = 1.0f;   // seconds between individual spawns
    public float wavePauseDuration = 3.0f;  // pause between waves
    public int   baseEnemyCount   = 4;      // enemies in wave 1

    [Header("Difficulty Scaling per Wave")]
    public float speedMultiplierPerWave = 0.05f;  // +5 % move speed each wave
    public float fireRateReductionPerWave = 0.03f; // shoot interval decreases

    private int  enemiesAlive;
    private bool spawning;

    private void Start()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnStateChanged += OnStateChanged;
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnStateChanged -= OnStateChanged;
    }

    private void OnStateChanged(GameManager.State state)
    {
        if (state == GameManager.State.Playing && !spawning)
            StartCoroutine(SpawnWaves());
    }

    /// <summary>Call from a GameScene bootstrap script after GameManager.StartGame().</summary>
    public void BeginSpawning()
    {
        if (!spawning) StartCoroutine(SpawnWaves());
    }

    // ── Core wave loop ───────────────────────────────────────────────
    private IEnumerator SpawnWaves()
    {
        spawning = true;

        while (GameManager.Instance != null &&
               GameManager.Instance.CurrentState == GameManager.State.Playing)
        {
            GameManager.Instance.AdvanceWave();
            int wave = GameManager.Instance.WaveNumber;

            // Boss wave every 5 waves
            bool isBossWave = (wave % 5 == 0);

            if (isBossWave)
            {
                if (AudioManager.Instance != null)
                    AudioManager.Instance.PlayBossWarning();

                yield return new WaitForSeconds(1.5f); // dramatic pause

                SpawnEnemy(bossEnemyPrefab != null ? bossEnemyPrefab : basicEnemyPrefab, wave);
                enemiesAlive = 1;
            }
            else
            {
                int count = baseEnemyCount + (wave - 1) * 2;  // +2 per wave
                enemiesAlive = count;

                for (int i = 0; i < count; i++)
                {
                    if (GameManager.Instance.CurrentState != GameManager.State.Playing)
                        break;

                    GameObject prefab = ChoosePrefab(wave);
                    SpawnEnemy(prefab, wave);
                    yield return new WaitForSeconds(spawnDelay);
                }
            }

            // Wait until all enemies from this wave are destroyed
            yield return new WaitUntil(() => AreAllEnemiesDead() ||
                GameManager.Instance.CurrentState != GameManager.State.Playing);

            yield return new WaitForSeconds(wavePauseDuration);
        }

        spawning = false;
    }

    // ── Prefab selection by wave ──────────────────────────────────────
    private GameObject ChoosePrefab(int wave)
    {
        // Unlock new types at wave thresholds
        float roll = Random.value;

        if (wave >= 7 && tankEnemyPrefab != null && roll < 0.25f)
            return tankEnemyPrefab;
        if (wave >= 5 && tankEnemyPrefab != null && roll < 0.15f)
            return tankEnemyPrefab;
        if (wave >= 3 && zigzagEnemyPrefab != null && roll < 0.4f)
            return zigzagEnemyPrefab;

        return basicEnemyPrefab;
    }

    // ── Spawn helper ─────────────────────────────────────────────────
    private void SpawnEnemy(GameObject prefab, int wave)
    {
        if (prefab == null) return;

        float xMin = ScreenBounds.Instance != null ? ScreenBounds.Instance.Left  + 1f : -6f;
        float xMax = ScreenBounds.Instance != null ? ScreenBounds.Instance.Right - 1f :  6f;
        float yPos = ScreenBounds.Instance != null ? ScreenBounds.Instance.Top   + 1f :  6f;

        float x = Random.Range(xMin, xMax);
        Vector3 spawnPos = new Vector3(x, yPos, 0f);

        GameObject go = Instantiate(prefab, spawnPos, Quaternion.identity);
        Enemy enemy = go.GetComponent<Enemy>();
        if (enemy != null)
        {
            // Difficulty scaling
            float speedMult = 1f + wave * speedMultiplierPerWave;
            enemy.moveSpeed     *= speedMult;
            enemy.shootInterval  = Mathf.Max(0.3f, enemy.shootInterval - wave * fireRateReductionPerWave);
        }
    }

    /// <summary>Check scene for remaining enemies tagged "Enemy".</summary>
    private bool AreAllEnemiesDead()
    {
        return GameObject.FindGameObjectsWithTag("Enemy").Length == 0;
    }
}

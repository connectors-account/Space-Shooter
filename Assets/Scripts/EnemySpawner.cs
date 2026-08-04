using UnityEngine;

/// <summary>
/// Spawns enemies from the top of the screen.
/// Difficulty scales over time: spawn interval shrinks and harder enemies appear.
/// Attach to an empty GameObject in the Game scene.
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    // ── Inspector ──────────────────────────────────────────────────────────────
    [Header("Enemy Prefabs")]
    public GameObject straightEnemyPrefab;
    public GameObject zigzagEnemyPrefab;
    public GameObject shooterEnemyPrefab;

    [Header("Spawn Area")]
    public float spawnXMin = -8.0f;
    public float spawnXMax =  8.0f;
    public float spawnY    =  6.2f;

    [Header("Difficulty Curve")]
    [Tooltip("Initial seconds between spawns.")]
    public float startInterval = 1.6f;

    [Tooltip("Fastest possible spawn rate (seconds).")]
    public float minInterval = 0.35f;

    [Tooltip("Seconds until maximum difficulty is reached.")]
    public float rampDuration = 90f;

    // ── Private ────────────────────────────────────────────────────────────────
    float spawnTimer;
    float gameTime;

    // ── Unity ──────────────────────────────────────────────────────────────────
    void Start() => spawnTimer = startInterval;

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver) return;

        gameTime  += Time.deltaTime;
        spawnTimer -= Time.deltaTime;

        if (spawnTimer <= 0f)
        {
            SpawnEnemy();
            float t    = Mathf.Clamp01(gameTime / rampDuration);
            spawnTimer = Mathf.Lerp(startInterval, minInterval, t);
        }
    }

    // ── Spawn Logic ────────────────────────────────────────────────────────────
    void SpawnEnemy()
    {
        float     x        = Random.Range(spawnXMin, spawnXMax);
        Vector3   spawnPos = new Vector3(x, spawnY, 0f);
        float     t        = Mathf.Clamp01(gameTime / rampDuration);
        float     roll     = Random.value;
        GameObject prefab;

        // Early   (t < 0.25): 80% straight, 20% zigzag
        // Mid     (t < 0.55): 45% straight, 35% zigzag, 20% shooter
        // Late    (t >= 0.55): 25% straight, 40% zigzag, 35% shooter
        if (t < 0.25f)
        {
            prefab = roll < 0.80f ? straightEnemyPrefab : zigzagEnemyPrefab;
        }
        else if (t < 0.55f)
        {
            if      (roll < 0.45f) prefab = straightEnemyPrefab;
            else if (roll < 0.80f) prefab = zigzagEnemyPrefab;
            else                   prefab = shooterEnemyPrefab;
        }
        else
        {
            if      (roll < 0.25f) prefab = straightEnemyPrefab;
            else if (roll < 0.65f) prefab = zigzagEnemyPrefab;
            else                   prefab = shooterEnemyPrefab;
        }

        if (prefab != null)
            Instantiate(prefab, spawnPos, Quaternion.identity);
    }
}

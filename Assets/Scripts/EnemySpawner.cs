using UnityEngine;

/// <summary>
/// EnemySpawner – Spawns enemies at the top of the screen at random X positions.
/// Difficulty increases over time via GameManager.CurrentSpawnInterval.
/// Attach to an empty GameObject in the GamePlay scene.
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy Prefab")]
    [Tooltip("The enemy prefab to spawn")]
    public GameObject enemyPrefab;

    [Header("Spawn Settings")]
    [Tooltip("Chance (0-1) that an enemy will have a sway pattern")]
    public float swayChance = 0.3f;

    [Tooltip("Maximum sway amplitude for wavy enemies")]
    public float maxSwayAmplitude = 1.5f;

    [Tooltip("Base enemy speed")]
    public float baseEnemySpeed = 3f;

    [Tooltip("Extra speed added as game progresses (per 60 seconds)")]
    public float speedRampPerMinute = 0.5f;

    // Internal
    private float nextSpawnTime = 0f;
    private float spawnY;
    private float spawnXMin, spawnXMax;
    private float gameStartTime;

    void Start()
    {
        Camera cam = Camera.main;
        float halfHeight = cam.orthographicSize;
        float halfWidth = halfHeight * cam.aspect;

        spawnY = halfHeight + 0.5f;         // just above the top edge
        spawnXMin = -halfWidth + 0.5f;
        spawnXMax = halfWidth - 0.5f;

        gameStartTime = Time.time;
        nextSpawnTime = Time.time + 1f; // short initial delay
    }

    void Update()
    {
        if (GameManager.Instance == null ||
            GameManager.Instance.CurrentState != GameManager.GameState.Playing)
            return;

        if (Time.time >= nextSpawnTime)
        {
            SpawnEnemy();
            nextSpawnTime = Time.time + GameManager.Instance.CurrentSpawnInterval;
        }
    }

    void SpawnEnemy()
    {
        if (enemyPrefab == null) return;

        float x = Random.Range(spawnXMin, spawnXMax);
        Vector3 pos = new Vector3(x, spawnY, 0f);

        GameObject enemy = Instantiate(enemyPrefab, pos, Quaternion.identity);
        Enemy enemyScript = enemy.GetComponent<Enemy>();

        if (enemyScript != null)
        {
            // Ramp speed over time
            float elapsed = Time.time - gameStartTime;
            enemyScript.speed = baseEnemySpeed + (speedRampPerMinute * elapsed / 60f);

            // Random chance of sway movement
            if (Random.value < swayChance)
            {
                enemyScript.swayAmplitude = Random.Range(0.5f, maxSwayAmplitude);
                enemyScript.swayFrequency = Random.Range(1.5f, 3f);
            }
        }
    }
}

using UnityEngine;
using System.Collections;

/// <summary>
/// Manages wave-based enemy spawning with progressive difficulty.
/// Attach to an empty GameObject in the scene.
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private GameObject enemyBulletPrefab;
    [SerializeField] private GameObject powerUpPrefab;

    [Header("Spawn Settings")]
    [SerializeField] private float spawnYPosition = 6.5f;
    [SerializeField] private float spawnXRange = 7f;
    [SerializeField] private float timeBetweenWaves = 3f;
    [SerializeField] private int baseEnemiesPerWave = 4;
    [SerializeField] private float spawnInterval = 0.5f;

    // Wave tracking
    private int currentWave;
    private bool isSpawning;
    private bool gameActive;

    // Difficulty scaling
    private float difficultyMultiplier = 1f;
    private readonly float difficultyIncrement = 0.15f;

    private void Start()
    {
        // Create enemy prefab at runtime if not assigned
        if (enemyPrefab == null)
        {
            enemyPrefab = CreateEnemyPrefab();
        }
        if (enemyBulletPrefab == null)
        {
            enemyBulletPrefab = CreateBulletPrefab(false);
        }
        if (powerUpPrefab == null)
        {
            powerUpPrefab = CreatePowerUpPrefab();
        }
    }

    /// <summary>
    /// Starts the wave spawning system. Called by GameManager when game begins.
    /// </summary>
    public void StartSpawning()
    {
        currentWave = 0;
        difficultyMultiplier = 1f;
        gameActive = true;
        StartCoroutine(SpawnWaves());
    }

    /// <summary>
    /// Stops all spawning. Called on game over.
    /// </summary>
    public void StopSpawning()
    {
        gameActive = false;
        StopAllCoroutines();
    }

    /// <summary>
    /// Coroutine that manages wave progression.
    /// </summary>
    private IEnumerator SpawnWaves()
    {
        yield return new WaitForSeconds(1.5f); // Initial delay

        while (gameActive)
        {
            currentWave++;
            difficultyMultiplier = 1f + (currentWave - 1) * difficultyIncrement;

            int enemyCount = baseEnemiesPerWave + (currentWave - 1);
            enemyCount = Mathf.Min(enemyCount, 15); // Cap at 15 enemies per wave

            if (GameManager.Instance != null)
                GameManager.Instance.UpdateWave(currentWave);

            yield return StartCoroutine(SpawnWave(enemyCount));

            // Wait for most enemies to be cleared or timeout
            yield return new WaitForSeconds(timeBetweenWaves);
        }
    }

    /// <summary>
    /// Spawns a single wave of enemies with varied patterns.
    /// </summary>
    private IEnumerator SpawnWave(int count)
    {
        for (int i = 0; i < count && gameActive; i++)
        {
            SpawnEnemy();
            yield return new WaitForSeconds(spawnInterval / difficultyMultiplier);
        }
    }

    /// <summary>
    /// Spawns a single enemy with randomized properties based on difficulty.
    /// </summary>
    private void SpawnEnemy()
    {
        float xPos = Random.Range(-spawnXRange, spawnXRange);
        Vector3 spawnPos = new Vector3(xPos, spawnYPosition, 0);

        GameObject enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
        EnemyController ec = enemy.GetComponent<EnemyController>();

        if (ec != null)
        {
            // Assign prefab references
            ec.SetEnemyBulletPrefab(enemyBulletPrefab);
            ec.SetPowerUpPrefab(powerUpPrefab);

            // Randomize movement pattern
            EnemyController.MovementPattern pattern = GetRandomPattern();
            ec.SetPattern(pattern);

            // Scale difficulty
            float speed = Random.Range(2f, 4f) * Mathf.Sqrt(difficultyMultiplier);
            ec.SetMoveSpeed(speed);

            int hp = 1 + Mathf.FloorToInt((currentWave - 1) / 3f);
            hp = Mathf.Min(hp, 5);
            ec.SetHealth(hp);

            int score = 100 * hp;
            ec.SetScoreValue(score);

            // Higher waves — enemies shoot more frequently
            float fireRate = Mathf.Max(0.8f, 2.5f - currentWave * 0.15f);
            ec.SetFireRate(fireRate);

            // Some enemies can't shoot (variety)
            ec.SetCanShoot(Random.value > 0.3f);
        }
    }

    /// <summary>
    /// Returns a weighted random movement pattern.
    /// </summary>
    private EnemyController.MovementPattern GetRandomPattern()
    {
        float roll = Random.value;
        if (roll < 0.35f) return EnemyController.MovementPattern.Straight;
        if (roll < 0.60f) return EnemyController.MovementPattern.Zigzag;
        if (roll < 0.80f) return EnemyController.MovementPattern.Sine;
        return EnemyController.MovementPattern.Dive;
    }

    // ==================== Runtime Prefab Creators ====================
    // These create simple geometric GameObjects when no prefabs are assigned.

    private GameObject CreateEnemyPrefab()
    {
        GameObject prefab = new GameObject("EnemyPrefab");
        prefab.tag = "Enemy";
        prefab.layer = 0;

        SpriteRenderer sr = prefab.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteFactory.CreateTriangleSprite(Color.red);
        sr.color = Color.red;
        sr.sortingOrder = 3;

        // Flip triangle to point downward
        prefab.transform.localScale = new Vector3(0.7f, -0.7f, 1f);

        BoxCollider2D col = prefab.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(0.8f, 0.8f);

        Rigidbody2D rb = prefab.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.isKinematic = true;

        prefab.AddComponent<EnemyController>();

        prefab.SetActive(false); // Treat as prefab template
        return prefab;
    }

    private GameObject CreateBulletPrefab(bool isPlayer)
    {
        GameObject prefab = new GameObject(isPlayer ? "PlayerBulletPrefab" : "EnemyBulletPrefab");
        prefab.tag = isPlayer ? "PlayerBullet" : "EnemyBullet";

        SpriteRenderer sr = prefab.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteFactory.CreateRectSprite(isPlayer ? Color.green : Color.red);
        sr.color = isPlayer ? Color.green : Color.red;
        sr.sortingOrder = 4;

        prefab.transform.localScale = new Vector3(0.15f, 0.4f, 1f);

        BoxCollider2D col = prefab.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(1f, 1f);

        Rigidbody2D rb = prefab.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.isKinematic = true;

        prefab.AddComponent<BulletController>();

        prefab.SetActive(false);
        return prefab;
    }

    private GameObject CreatePowerUpPrefab()
    {
        GameObject prefab = new GameObject("PowerUpPrefab");
        prefab.tag = "PowerUp";

        SpriteRenderer sr = prefab.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteFactory.CreateDiamondSprite(Color.yellow);
        sr.color = Color.yellow;
        sr.sortingOrder = 4;

        prefab.transform.localScale = Vector3.one * 0.5f;

        CircleCollider2D col = prefab.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.5f;

        Rigidbody2D rb = prefab.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.isKinematic = true;

        prefab.AddComponent<PowerUpController>();

        prefab.SetActive(false);
        return prefab;
    }

    public int GetCurrentWave() => currentWave;
}

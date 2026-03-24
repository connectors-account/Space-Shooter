using UnityEngine;

/// <summary>
/// PowerUpSpawner creates power-up pickups either at a specific position
/// (e.g. where an enemy died) or at random intervals during gameplay.
/// </summary>
public class PowerUpSpawner : MonoBehaviour
{
    // ── Singleton ────────────────────────────────────────────
    public static PowerUpSpawner Instance { get; private set; }

    // ── Prefab ───────────────────────────────────────────────
    [Header("Power-Up Prefab")]
    [SerializeField] private GameObject powerUpPrefab;

    // ── Random Spawning ──────────────────────────────────────
    [Header("Random Spawn Settings")]
    [SerializeField] private float minSpawnInterval = 10f;
    [SerializeField] private float maxSpawnInterval = 20f;
    [SerializeField] private float spawnYPosition = 6.5f;
    [SerializeField] private float spawnXMin = -6f;
    [SerializeField] private float spawnXMax = 6f;

    // ── Internal ─────────────────────────────────────────────
    private float nextRandomSpawnTime;
    private bool spawningEnabled = false;

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

    private void Start()
    {
        ScheduleNextRandomSpawn();
        spawningEnabled = true;
    }

    private void Update()
    {
        if (!spawningEnabled) return;
        if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameManager.GameState.Playing) return;

        if (Time.time >= nextRandomSpawnTime)
        {
            SpawnRandomPowerUp();
            ScheduleNextRandomSpawn();
        }
    }

    // ──────────────────────────────────────────────────────────
    // Public API
    // ──────────────────────────────────────────────────────────

    /// <summary>
    /// Spawn a random power-up at a given world position (e.g. enemy death location).
    /// </summary>
    public void SpawnPowerUpAt(Vector3 position)
    {
        if (powerUpPrefab == null) return;

        GameObject pickup = Instantiate(powerUpPrefab, position, Quaternion.identity);
        PowerUpController pc = pickup.GetComponent<PowerUpController>();
        if (pc != null)
        {
            pc.SetType(GetRandomType());
        }
    }

    public void EnableSpawning(bool enabled)
    {
        spawningEnabled = enabled;
    }

    // ──────────────────────────────────────────────────────────
    // Internal Helpers
    // ──────────────────────────────────────────────────────────

    private void SpawnRandomPowerUp()
    {
        if (powerUpPrefab == null) return;

        Vector3 pos = new Vector3(Random.Range(spawnXMin, spawnXMax), spawnYPosition, 0f);
        GameObject pickup = Instantiate(powerUpPrefab, pos, Quaternion.identity);
        PowerUpController pc = pickup.GetComponent<PowerUpController>();
        if (pc != null)
        {
            pc.SetType(GetRandomType());
        }
    }

    private PowerUpController.PowerUpType GetRandomType()
    {
        float roll = Random.value;
        if (roll < 0.45f)
            return PowerUpController.PowerUpType.Health;
        else if (roll < 0.75f)
            return PowerUpController.PowerUpType.WeaponUpgrade;
        else
            return PowerUpController.PowerUpType.Shield;
    }

    private void ScheduleNextRandomSpawn()
    {
        nextRandomSpawnTime = Time.time + Random.Range(minSpawnInterval, maxSpawnInterval);
    }
}

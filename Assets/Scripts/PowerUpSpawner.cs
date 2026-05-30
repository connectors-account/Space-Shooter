using UnityEngine;

/// <summary>
/// PowerUpSpawner – Periodically spawns power-ups at random positions.
/// Attach to an empty GameObject in the GamePlay scene.
/// </summary>
public class PowerUpSpawner : MonoBehaviour
{
    [Header("Power-Up Prefabs")]
    public GameObject rapidFirePrefab;
    public GameObject shieldPrefab;

    [Header("Spawn Timing")]
    [Tooltip("Minimum seconds between power-up spawns")]
    public float minInterval = 8f;

    [Tooltip("Maximum seconds between power-up spawns")]
    public float maxInterval = 15f;

    // Internal
    private float nextSpawnTime;
    private float spawnY;
    private float spawnXMin, spawnXMax;

    void Start()
    {
        Camera cam = Camera.main;
        float halfHeight = cam.orthographicSize;
        float halfWidth = halfHeight * cam.aspect;

        spawnY = halfHeight + 0.5f;
        spawnXMin = -halfWidth + 1f;
        spawnXMax = halfWidth - 1f;

        ScheduleNext();
    }

    void Update()
    {
        if (GameManager.Instance == null ||
            GameManager.Instance.CurrentState != GameManager.GameState.Playing)
            return;

        if (Time.time >= nextSpawnTime)
        {
            SpawnRandomPowerUp();
            ScheduleNext();
        }
    }

    void ScheduleNext()
    {
        nextSpawnTime = Time.time + Random.Range(minInterval, maxInterval);
    }

    void SpawnRandomPowerUp()
    {
        // 50/50 chance of each type
        GameObject prefab = Random.value > 0.5f ? rapidFirePrefab : shieldPrefab;
        if (prefab == null) return;

        float x = Random.Range(spawnXMin, spawnXMax);
        Instantiate(prefab, new Vector3(x, spawnY, 0f), Quaternion.identity);
    }
}

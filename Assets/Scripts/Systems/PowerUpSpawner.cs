using UnityEngine;

/// <summary>
/// PowerUpSpawner randomly spawns power-ups during gameplay.
/// </summary>
public class PowerUpSpawner : MonoBehaviour
{
    [Header("Power-Up Prefab")]
    public GameObject healthPowerUpPrefab;

    [Header("Spawn Settings")]
    [Tooltip("Minimum time between spawns")]
    public float minSpawnInterval = 10f;
    
    [Tooltip("Maximum time between spawns")]
    public float maxSpawnInterval = 20f;
    
    [Tooltip("Minimum X position for spawning")]
    public float minSpawnX = -7f;
    
    [Tooltip("Maximum X position for spawning")]
    public float maxSpawnX = 7f;
    
    [Tooltip("Y position where power-ups spawn")]
    public float spawnY = 6f;

    private float nextSpawnTime;
    private bool canSpawn = false;

    void Start()
    {
        SetNextSpawnTime();
    }

    void Update()
    {
        if (!canSpawn) return;
        
        if (GameManager.Instance != null && GameManager.Instance.IsGamePaused())
            return;
        
        if (Time.time >= nextSpawnTime)
        {
            SpawnPowerUp();
            SetNextSpawnTime();
        }
    }

    /// <summary>
    /// Set the next spawn time randomly
    /// </summary>
    void SetNextSpawnTime()
    {
        nextSpawnTime = Time.time + Random.Range(minSpawnInterval, maxSpawnInterval);
    }

    /// <summary>
    /// Spawn a power-up
    /// </summary>
    void SpawnPowerUp()
    {
        if (healthPowerUpPrefab != null)
        {
            float spawnX = Random.Range(minSpawnX, maxSpawnX);
            Vector3 spawnPosition = new Vector3(spawnX, spawnY, 0f);
            
            Instantiate(healthPowerUpPrefab, spawnPosition, Quaternion.identity);
        }
    }

    /// <summary>
    /// Enable spawning
    /// </summary>
    public void StartSpawning()
    {
        canSpawn = true;
        SetNextSpawnTime();
    }

    /// <summary>
    /// Disable spawning
    /// </summary>
    public void StopSpawning()
    {
        canSpawn = false;
    }

    /// <summary>
    /// Reset spawner
    /// </summary>
    public void ResetSpawner()
    {
        canSpawn = false;
        
        // Destroy existing power-ups
        PowerUp[] powerUps = FindObjectsOfType<PowerUp>();
        foreach (PowerUp powerUp in powerUps)
        {
            Destroy(powerUp.gameObject);
        }
    }
}

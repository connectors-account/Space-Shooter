using UnityEngine;
using System.Collections;

/// <summary>
/// Simple enemy spawner component that can be used for testing or as a standalone spawner.
/// For wave-based spawning, use WaveManager instead.
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [Header("Spawner Settings")]
    [SerializeField] private bool autoSpawn = false;
    [SerializeField] private float spawnInterval = 2f;
    [SerializeField] private int maxEnemies = 10;
    
    [Header("Enemy Prefabs")]
    [SerializeField] private GameObject[] enemyPrefabs;
    
    [Header("Spawn Area")]
    [SerializeField] private float spawnY = 6f;
    [SerializeField] private float minSpawnX = -7f;
    [SerializeField] private float maxSpawnX = 7f;
    
    private bool isSpawning;
    private int currentEnemyCount;
    
    private void Start()
    {
        if (autoSpawn)
        {
            StartSpawning();
        }
    }
    
    /// <summary>
    /// Start automatic spawning
    /// </summary>
    public void StartSpawning()
    {
        if (!isSpawning)
        {
            isSpawning = true;
            StartCoroutine(SpawnLoop());
        }
    }
    
    /// <summary>
    /// Stop automatic spawning
    /// </summary>
    public void StopSpawning()
    {
        isSpawning = false;
        StopAllCoroutines();
    }
    
    /// <summary>
    /// Spawn a single enemy
    /// </summary>
    public void SpawnEnemy()
    {
        if (enemyPrefabs == null || enemyPrefabs.Length == 0) return;
        
        // Select random prefab
        int index = Random.Range(0, enemyPrefabs.Length);
        GameObject prefab = enemyPrefabs[index];
        
        if (prefab == null) return;
        
        // Random spawn position
        float spawnX = Random.Range(minSpawnX, maxSpawnX);
        Vector3 spawnPos = new Vector3(spawnX, spawnY, 0);
        
        // Spawn enemy
        GameObject enemy = Instantiate(prefab, spawnPos, Quaternion.Euler(0, 0, 180));
        
        // Track enemy count
        EnemyController controller = enemy.GetComponent<EnemyController>();
        if (controller != null)
        {
            HealthSystem health = enemy.GetComponent<HealthSystem>();
            if (health != null)
            {
                health.OnDeath += () => currentEnemyCount--;
            }
            currentEnemyCount++;
        }
    }
    
    /// <summary>
    /// Spawn enemy of specific type
    /// </summary>
    public void SpawnEnemy(int prefabIndex)
    {
        if (enemyPrefabs == null || prefabIndex < 0 || prefabIndex >= enemyPrefabs.Length) return;
        
        GameObject prefab = enemyPrefabs[prefabIndex];
        if (prefab == null) return;
        
        float spawnX = Random.Range(minSpawnX, maxSpawnX);
        Vector3 spawnPos = new Vector3(spawnX, spawnY, 0);
        
        Instantiate(prefab, spawnPos, Quaternion.Euler(0, 0, 180));
    }
    
    private IEnumerator SpawnLoop()
    {
        while (isSpawning)
        {
            if (currentEnemyCount < maxEnemies && GameManager.Instance != null && GameManager.Instance.IsPlaying)
            {
                SpawnEnemy();
            }
            
            yield return new WaitForSeconds(spawnInterval);
        }
    }
    
    /// <summary>
    /// Set spawn interval
    /// </summary>
    public void SetSpawnInterval(float interval)
    {
        spawnInterval = Mathf.Max(0.1f, interval);
    }
    
    /// <summary>
    /// Get current active enemy count
    /// </summary>
    public int GetActiveEnemyCount()
    {
        return currentEnemyCount;
    }
}

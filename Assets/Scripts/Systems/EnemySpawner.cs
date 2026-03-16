using UnityEngine;

/// <summary>
/// EnemySpawner handles spawning enemies from the object pool.
/// Works with WaveManager for wave-based spawning.
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    // Singleton instance
    public static EnemySpawner Instance { get; private set; }

    [Header("Spawn Settings")]
    [SerializeField] private float minSpawnX = -7f;
    [SerializeField] private float maxSpawnX = 7f;
    [SerializeField] private float spawnY = 6f;

    [Header("Enemy Pool Tags")]
    [SerializeField] private string smallEnemyTag = "SmallEnemy";
    [SerializeField] private string mediumEnemyTag = "MediumEnemy";
    [SerializeField] private string largeEnemyTag = "LargeEnemy";
    [SerializeField] private string trackerEnemyTag = "TrackerEnemy";
    [SerializeField] private string bossEnemyTag = "BossEnemy";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    /// <summary>
    /// Spawn a random enemy type
    /// </summary>
    public void SpawnRandomEnemy()
    {
        string[] enemyTags = { smallEnemyTag, mediumEnemyTag, largeEnemyTag, trackerEnemyTag };
        float[] weights = { 0.4f, 0.3f, 0.15f, 0.15f };

        string selectedTag = SelectWeightedRandom(enemyTags, weights);
        SpawnEnemy(selectedTag);
    }

    /// <summary>
    /// Spawn a specific enemy type by tag
    /// </summary>
    public void SpawnEnemy(string enemyTag)
    {
        if (ObjectPooler.Instance == null)
        {
            Debug.LogError("ObjectPooler not found!");
            return;
        }

        Vector3 spawnPosition = new Vector3(
            Random.Range(minSpawnX, maxSpawnX),
            spawnY,
            0f
        );

        ObjectPooler.Instance.SpawnFromPool(enemyTag, spawnPosition, Quaternion.identity);
    }

    /// <summary>
    /// Spawn enemy at specific position
    /// </summary>
    public void SpawnEnemyAtPosition(string enemyTag, Vector3 position)
    {
        if (ObjectPooler.Instance == null) return;
        ObjectPooler.Instance.SpawnFromPool(enemyTag, position, Quaternion.identity);
    }

    /// <summary>
    /// Spawn a formation of enemies
    /// </summary>
    public void SpawnFormation(string enemyTag, int count, float spacing)
    {
        float totalWidth = (count - 1) * spacing;
        float startX = -totalWidth / 2f;

        for (int i = 0; i < count; i++)
        {
            float xPos = startX + (i * spacing);
            xPos = Mathf.Clamp(xPos, minSpawnX, maxSpawnX);
            
            Vector3 spawnPosition = new Vector3(xPos, spawnY, 0f);
            ObjectPooler.Instance.SpawnFromPool(enemyTag, spawnPosition, Quaternion.identity);
        }
    }

    /// <summary>
    /// Spawn the boss enemy
    /// </summary>
    public void SpawnBoss()
    {
        if (ObjectPooler.Instance == null) return;

        Vector3 bossPosition = new Vector3(0f, spawnY, 0f);
        ObjectPooler.Instance.SpawnFromPool(bossEnemyTag, bossPosition, Quaternion.identity);
    }

    /// <summary>
    /// Select a random item based on weights
    /// </summary>
    private string SelectWeightedRandom(string[] items, float[] weights)
    {
        float totalWeight = 0f;
        foreach (float w in weights)
        {
            totalWeight += w;
        }

        float randomValue = Random.Range(0f, totalWeight);
        float cumulative = 0f;

        for (int i = 0; i < items.Length; i++)
        {
            cumulative += weights[i];
            if (randomValue <= cumulative)
            {
                return items[i];
            }
        }

        return items[0];
    }

    /// <summary>
    /// Deactivate all active enemies
    /// </summary>
    public void ClearAllEnemies()
    {
        if (ObjectPooler.Instance == null) return;

        ObjectPooler.Instance.ReturnAllToPool(smallEnemyTag);
        ObjectPooler.Instance.ReturnAllToPool(mediumEnemyTag);
        ObjectPooler.Instance.ReturnAllToPool(largeEnemyTag);
        ObjectPooler.Instance.ReturnAllToPool(trackerEnemyTag);
        ObjectPooler.Instance.ReturnAllToPool(bossEnemyTag);
    }
}

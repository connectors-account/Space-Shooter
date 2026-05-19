using UnityEngine;

/// <summary>
/// Manages spawning of power-up items when enemies are destroyed.
/// Provides a singleton for easy access from enemy scripts.
/// </summary>
public class PowerUpSpawner : MonoBehaviour
{
    public static PowerUpSpawner Instance { get; private set; }

    [Header("Power-Up Prefabs")]
    [SerializeField] private GameObject rapidFirePrefab;
    [SerializeField] private GameObject shieldPrefab;
    [SerializeField] private GameObject healthRestorePrefab;

    [Header("Spawn Weights")]
    [SerializeField] [Range(0f, 1f)] private float rapidFireWeight = 0.4f;
    [SerializeField] [Range(0f, 1f)] private float shieldWeight = 0.3f;
    [SerializeField] [Range(0f, 1f)] private float healthWeight = 0.3f;

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
    /// Spawns a random power-up at the given position based on configured weights.
    /// </summary>
    public void SpawnRandomPowerUp(Vector3 position)
    {
        float totalWeight = rapidFireWeight + shieldWeight + healthWeight;
        float roll = Random.value * totalWeight;

        GameObject prefab;

        if (roll < rapidFireWeight)
            prefab = rapidFirePrefab;
        else if (roll < rapidFireWeight + shieldWeight)
            prefab = shieldPrefab;
        else
            prefab = healthRestorePrefab;

        if (prefab != null)
            Instantiate(prefab, position, Quaternion.identity);
    }
}

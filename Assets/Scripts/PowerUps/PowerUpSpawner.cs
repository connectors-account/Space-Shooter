using UnityEngine;

/// <summary>
/// Manages spawning power-ups when enemies are destroyed.
/// Singleton pattern.
/// </summary>
public class PowerUpSpawner : MonoBehaviour
{
    public static PowerUpSpawner Instance { get; private set; }

    [SerializeField] private GameObject powerUpPrefab;

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
    /// Spawn a random power-up at the given position.
    /// </summary>
    public void SpawnRandomPowerUp(Vector3 position)
    {
        if (powerUpPrefab == null) return;

        GameObject obj = Instantiate(powerUpPrefab, position, Quaternion.identity);
        PowerUp pu = obj.GetComponent<PowerUp>();
        if (pu != null)
        {
            // Weighted random type
            float roll = Random.value;
            PowerUpType type;
            if (roll < 0.4f)
                type = PowerUpType.WeaponUpgrade;
            else if (roll < 0.7f)
                type = PowerUpType.HealthPack;
            else
                type = PowerUpType.Shield;

            pu.Initialize(type);
        }
    }
}

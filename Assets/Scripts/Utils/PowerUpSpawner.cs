using UnityEngine;

/// <summary>
/// PowerUpSpawner handles spawning power-ups at designated positions or randomly.
/// </summary>
public class PowerUpSpawner : MonoBehaviour
{
    // Singleton instance
    public static PowerUpSpawner Instance { get; private set; }

    [Header("Power-Up Pools")]
    [SerializeField] private string weaponUpgradeTag = "PowerUp_Weapon";
    [SerializeField] private string shieldTag = "PowerUp_Shield";
    [SerializeField] private string healthTag = "PowerUp_Health";
    [SerializeField] private string scoreBonusTag = "PowerUp_Score";

    [Header("Spawn Weights")]
    [SerializeField] private float weaponWeight = 0.35f;
    [SerializeField] private float shieldWeight = 0.2f;
    [SerializeField] private float healthWeight = 0.3f;
    [SerializeField] private float scoreWeight = 0.15f;

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
    /// Spawn a random power-up at the given position
    /// </summary>
    public void SpawnRandomPowerUp(Vector3 position)
    {
        string poolTag = SelectRandomPowerUpTag();
        SpawnPowerUp(poolTag, position);
    }

    /// <summary>
    /// Spawn a specific power-up
    /// </summary>
    public void SpawnPowerUp(string poolTag, Vector3 position)
    {
        if (ObjectPooler.Instance == null) return;
        ObjectPooler.Instance.SpawnFromPool(poolTag, position, Quaternion.identity);
    }

    /// <summary>
    /// Select a random power-up tag based on weights
    /// </summary>
    private string SelectRandomPowerUpTag()
    {
        float totalWeight = weaponWeight + shieldWeight + healthWeight + scoreWeight;
        float randomValue = Random.Range(0f, totalWeight);

        if (randomValue < weaponWeight)
            return weaponUpgradeTag;
        randomValue -= weaponWeight;

        if (randomValue < shieldWeight)
            return shieldTag;
        randomValue -= shieldWeight;

        if (randomValue < healthWeight)
            return healthTag;

        return scoreBonusTag;
    }

    /// <summary>
    /// Spawn a guaranteed power-up (used for boss drops, etc.)
    /// </summary>
    public void SpawnGuaranteedPowerUp(Vector3 position, PowerUp.PowerUpType type)
    {
        string tag = GetTagForType(type);
        SpawnPowerUp(tag, position);
    }

    /// <summary>
    /// Get pool tag for power-up type
    /// </summary>
    private string GetTagForType(PowerUp.PowerUpType type)
    {
        switch (type)
        {
            case PowerUp.PowerUpType.WeaponUpgrade:
                return weaponUpgradeTag;
            case PowerUp.PowerUpType.Shield:
                return shieldTag;
            case PowerUp.PowerUpType.Health:
                return healthTag;
            case PowerUp.PowerUpType.ScoreBonus:
                return scoreBonusTag;
            default:
                return weaponUpgradeTag;
        }
    }
}

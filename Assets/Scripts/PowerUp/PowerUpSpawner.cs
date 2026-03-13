using UnityEngine;

/// <summary>
/// Manages power-up drop rates from destroyed enemies.
/// </summary>
public class PowerUpSpawner : MonoBehaviour
{
    [Header("Power-Up Prefabs")]
    public GameObject weaponUpgradePrefab;
    public GameObject shieldPrefab;
    public GameObject healthPrefab;

    [Header("Drop Rates")]
    [Range(0f, 1f)] public float dropChance = 0.15f;
    [Range(0f, 1f)] public float weaponUpgradeWeight = 0.4f;
    [Range(0f, 1f)] public float shieldWeight = 0.25f;
    [Range(0f, 1f)] public float healthWeight = 0.35f;

    public void TrySpawnPowerUp(Vector3 position)
    {
        if (Random.value > dropChance) return;

        float totalWeight = weaponUpgradeWeight + shieldWeight + healthWeight;
        float roll = Random.Range(0f, totalWeight);

        GameObject prefab;
        if (roll < weaponUpgradeWeight)
            prefab = weaponUpgradePrefab;
        else if (roll < weaponUpgradeWeight + shieldWeight)
            prefab = shieldPrefab;
        else
            prefab = healthPrefab;

        if (prefab != null)
        {
            Instantiate(prefab, position, Quaternion.identity);
        }
    }
}

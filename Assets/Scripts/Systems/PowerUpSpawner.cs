using UnityEngine;

public class PowerUpSpawner : MonoBehaviour
{
    public static PowerUpSpawner Instance { get; private set; }

    [Header("Spawn Settings")]
    [Range(0f, 1f)]
    public float spawnChance = 0.15f;

    [Header("Power-Up Weights")]
    public float shieldWeight = 20f;
    public float rapidFireWeight = 25f;
    public float healthWeight = 30f;
    public float extraLifeWeight = 10f;
    public float scoreBonusWeight = 15f;

    [Header("Prefab")]
    public GameObject powerUpPrefab;

    private float totalWeight;

    private void Awake()
    {
        Instance = this;
        CalculateTotalWeight();
    }

    private void CalculateTotalWeight()
    {
        totalWeight = shieldWeight + rapidFireWeight + healthWeight + extraLifeWeight + scoreBonusWeight;
    }

    public void TrySpawnPowerUp(Vector3 position)
    {
        if (Random.value > spawnChance)
            return;

        SpawnPowerUp(position);
    }

    public void SpawnPowerUp(Vector3 position)
    {
        PowerUpType type = GetRandomPowerUpType();
        SpawnPowerUp(position, type);
    }

    public void SpawnPowerUp(Vector3 position, PowerUpType type)
    {
        GameObject powerUp;

        if (ObjectPooler.Instance != null && ObjectPooler.Instance.poolDictionary.ContainsKey("PowerUp"))
        {
            powerUp = ObjectPooler.Instance.SpawnFromPool("PowerUp", position, Quaternion.identity);
        }
        else if (powerUpPrefab != null)
        {
            powerUp = Instantiate(powerUpPrefab, position, Quaternion.identity);
        }
        else
        {
            Debug.LogWarning("No power-up prefab assigned!");
            return;
        }

        if (powerUp != null)
        {
            PowerUp powerUpComponent = powerUp.GetComponent<PowerUp>();
            if (powerUpComponent != null)
            {
                powerUpComponent.Initialize(type);
            }
        }
    }

    private PowerUpType GetRandomPowerUpType()
    {
        float random = Random.Range(0f, totalWeight);
        float cumulative = 0f;

        cumulative += shieldWeight;
        if (random < cumulative) return PowerUpType.Shield;

        cumulative += rapidFireWeight;
        if (random < cumulative) return PowerUpType.RapidFire;

        cumulative += healthWeight;
        if (random < cumulative) return PowerUpType.Health;

        cumulative += extraLifeWeight;
        if (random < cumulative) return PowerUpType.ExtraLife;

        return PowerUpType.ScoreBonus;
    }
}

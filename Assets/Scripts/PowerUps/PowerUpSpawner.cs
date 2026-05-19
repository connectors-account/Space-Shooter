using UnityEngine;

/// <summary>
/// Manages power-up creation and spawning.
/// </summary>
public class PowerUpSpawner : MonoBehaviour
{
    public static PowerUpSpawner Instance { get; private set; }

    [SerializeField] private GameObject powerUpPrefab;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void SpawnRandomPowerUp(Vector3 position)
    {
        if (powerUpPrefab == null) return;

        GameObject pickup = Instantiate(powerUpPrefab, position, Quaternion.identity);
        PowerUp pu = pickup.GetComponent<PowerUp>();
        if (pu == null) return;

        // Random type with weighted chances
        float roll = Random.value;
        if (roll < 0.4f)
            pu.Type = PowerUpType.Health;
        else if (roll < 0.75f)
            pu.Type = PowerUpType.RapidFire;
        else
            pu.Type = PowerUpType.Shield;

        // Color-code the power-up
        SpriteRenderer sr = pickup.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            switch (pu.Type)
            {
                case PowerUpType.Health:
                    sr.color = new Color(0.2f, 1f, 0.2f); // Green
                    break;
                case PowerUpType.RapidFire:
                    sr.color = new Color(1f, 0.8f, 0.1f); // Yellow
                    break;
                case PowerUpType.Shield:
                    sr.color = new Color(0.3f, 0.6f, 1f); // Blue
                    break;
            }
        }
    }
}

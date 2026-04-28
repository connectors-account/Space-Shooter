using UnityEngine;

public class PowerUpSystem : MonoBehaviour
{
    public enum PowerUpType
    {
        Shield,
        RapidFire,
        HealthRestore
    }

    public static PowerUpSystem Instance { get; private set; }

    [Header("Prefabs")]
    [SerializeField] private GameObject shieldPowerUpPrefab;
    [SerializeField] private GameObject rapidFirePowerUpPrefab;
    [SerializeField] private GameObject healthPowerUpPrefab;

    [Header("Spawn Settings")]
    [Range(0f, 1f)]
    [SerializeField] private float dropChance = 0.22f;

    [Header("Effect Values")]
    [SerializeField] private float shieldDuration = 8f;
    [SerializeField] private float rapidFireDuration = 6f;
    [SerializeField] private float rapidFireCooldown = 0.08f;
    [SerializeField] private int healthRestoreAmount = 35;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void TrySpawnPowerUp(Vector3 position)
    {
        if (!GameManager.Instance.IsGameplayActive || Random.value > dropChance)
        {
            return;
        }

        PowerUpType type = (PowerUpType)Random.Range(0, 3);
        GameObject prefab = GetPrefab(type);

        if (prefab == null)
        {
            return;
        }

        GameObject instance = Instantiate(prefab, position, Quaternion.identity);
        PowerUpRuntime runtime = instance.GetComponent<PowerUpRuntime>();
        if (runtime == null)
        {
            runtime = instance.AddComponent<PowerUpRuntime>();
        }

        runtime.Initialize(this, type);
    }

    private GameObject GetPrefab(PowerUpType type)
    {
        switch (type)
        {
            case PowerUpType.Shield:
                return shieldPowerUpPrefab;
            case PowerUpType.RapidFire:
                return rapidFirePowerUpPrefab;
            case PowerUpType.HealthRestore:
                return healthPowerUpPrefab;
            default:
                return null;
        }
    }

    public class PowerUpRuntime : MonoBehaviour
    {
        private PowerUpSystem owner;
        private PowerUpType type;

        public void Initialize(PowerUpSystem system, PowerUpType powerUpType)
        {
            owner = system;
            type = powerUpType;
            gameObject.tag = "PowerUp";
            Destroy(gameObject, 10f);
        }

        private void Update()
        {
            transform.position += Vector3.down * 2.3f * Time.deltaTime;

            if (transform.position.y < -6.5f)
            {
                Destroy(gameObject);
            }
        }

        public void ApplyTo(PlayerController player)
        {
            if (player == null || owner == null)
            {
                return;
            }

            switch (type)
            {
                case PowerUpType.Shield:
                    player.ApplyShield(owner.shieldDuration);
                    break;
                case PowerUpType.RapidFire:
                    player.ApplyRapidFire(owner.rapidFireDuration, owner.rapidFireCooldown);
                    break;
                case PowerUpType.HealthRestore:
                    player.RestoreHealth(owner.healthRestoreAmount);
                    break;
            }

            AudioManager.Instance?.PlaySfx(AudioSfx.PowerUpCollected);
            Destroy(gameObject);
        }
    }
}

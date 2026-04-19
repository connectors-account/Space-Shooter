using UnityEngine;

namespace SpaceShooter.Systems
{
    public enum PowerUpType
    {
        Shield,
        RapidFire,
        HealthRestore
    }

    /// <summary>
    /// Spawns power-ups and applies effects when the player collects them.
    /// </summary>
    public class PowerUpSystem : MonoBehaviour
    {
        public static PowerUpSystem Instance { get; private set; }

        [Header("Prefabs")]
        [SerializeField] private PowerUpPickup shieldPrefab;
        [SerializeField] private PowerUpPickup rapidFirePrefab;
        [SerializeField] private PowerUpPickup healthPrefab;

        [Header("Spawn")]
        [SerializeField, Range(0f, 1f)] private float dropChance = 0.22f;

        [Header("Effects")]
        [SerializeField] private float shieldDuration = 5f;
        [SerializeField] private float rapidFireDuration = 6f;
        [SerializeField] private float rapidFireMultiplier = 2f;
        [SerializeField] private int healthRestoreAmount = 30;

        public event System.Action<PowerUpType> OnPowerUpCollected;

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
            if (Random.value > dropChance)
            {
                return;
            }

            PowerUpPickup prefab = GetRandomPrefab();
            if (prefab != null)
            {
                Instantiate(prefab, position, Quaternion.identity);
            }
        }

        public void ApplyPowerUp(PowerUpType type, GameObject collector)
        {
            var playerController = collector.GetComponent<Player.PlayerController>();
            var playerHealth = collector.GetComponent<Player.PlayerHealth>();

            if (playerController == null || playerHealth == null)
            {
                return;
            }

            switch (type)
            {
                case PowerUpType.Shield:
                    playerHealth.ActivateShield(shieldDuration);
                    break;
                case PowerUpType.RapidFire:
                    playerController.ActivateRapidFire(rapidFireDuration, rapidFireMultiplier);
                    break;
                case PowerUpType.HealthRestore:
                    playerHealth.RestoreHealth(healthRestoreAmount);
                    break;
            }

            Audio.SoundManager.Instance?.PlayPowerUp();
            OnPowerUpCollected?.Invoke(type);
        }

        private PowerUpPickup GetRandomPrefab()
        {
            int roll = Random.Range(0, 3);
            return roll switch
            {
                0 => shieldPrefab,
                1 => rapidFirePrefab,
                _ => healthPrefab,
            };
        }
    }

    /// <summary>
    /// Component placed on power-up prefabs for drift and collect behavior.
    /// </summary>
    public class PowerUpPickup : MonoBehaviour
    {
        [SerializeField] private PowerUpType powerUpType;
        [SerializeField] private float driftSpeed = 1.5f;
        [SerializeField] private float spinSpeed = 90f;

        private void Update()
        {
            transform.position += Vector3.down * (driftSpeed * Time.deltaTime);
            transform.Rotate(0f, 0f, spinSpeed * Time.deltaTime);

            if (transform.position.y < -6.4f)
            {
                Destroy(gameObject);
            }
        }

        public void Consume(GameObject collector)
        {
            PowerUpSystem.Instance?.ApplyPowerUp(powerUpType, collector);
            Destroy(gameObject);
        }
    }
}

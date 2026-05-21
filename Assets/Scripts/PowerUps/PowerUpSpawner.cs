using UnityEngine;

namespace SpaceShooter.PowerUps
{
    /// <summary>
    /// Singleton that spawns power-ups from a pool when enemies die.
    /// </summary>
    public class PowerUpSpawner : MonoBehaviour
    {
        public static PowerUpSpawner Instance { get; private set; }

        [SerializeField] private string powerUpPoolTag = "PowerUp";

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void SpawnRandomPowerUp(Vector3 position)
        {
            GameObject pu = Managers.ObjectPoolManager.Instance?.GetFromPool(powerUpPoolTag, position, Quaternion.identity);
            if (pu != null)
            {
                PowerUpItem item = pu.GetComponent<PowerUpItem>();
                if (item != null)
                {
                    PowerUpType randomType = (PowerUpType)Random.Range(0, 3);
                    item.SetType(randomType);
                }
            }
        }
    }
}

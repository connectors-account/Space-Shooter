using SpaceShooter.Core;
using UnityEngine;

namespace SpaceShooter.PowerUps
{
    /// <summary>
    /// Spawns and pools <see cref="PowerUp"/> pickups. Enemies call <see cref="TryDrop"/> on death to
    /// randomly drop a power-up based on the configured drop chance.
    /// </summary>
    public class PowerUpManager : MonoBehaviour
    {
        /// <summary>Global access point.</summary>
        public static PowerUpManager Instance { get; private set; }

        private ObjectPool _pool;
        private GameConfig _config;
        private Transform _container;

        /// <summary>
        /// Builds the power-up pool. Called once by the bootstrap.
        /// </summary>
        public void Initialize(GameConfig config)
        {
            Instance = this;
            _config = config;
            _container = new GameObject("PowerUps").transform;
            _container.SetParent(transform, false);

            GameObject template = CreateTemplate();
            _pool = new ObjectPool(template, _container, prewarm: 8);
            template.SetActive(false);
        }

        private GameObject CreateTemplate()
        {
            var go = new GameObject("PowerUp");
            go.SetActive(false);
            go.AddComponent<SpriteRenderer>();
            go.AddComponent<PowerUp>();
            return go;
        }

        /// <summary>
        /// Rolls against the configured drop chance and, on success, spawns a random power-up.
        /// </summary>
        /// <param name="position">World position to drop at.</param>
        /// <param name="guaranteed">When true the drop always occurs (used by bosses).</param>
        public void TryDrop(Vector3 position, bool guaranteed = false)
        {
            if (!guaranteed && Random.value > _config.PowerUpDropChance)
            {
                return;
            }

            PowerUpType type = RandomType();
            Spawn(position, type);
        }

        /// <summary>
        /// Spawns a specific power-up at a position.
        /// </summary>
        public void Spawn(Vector3 position, PowerUpType type)
        {
            GameObject go = _pool.Get(position, Quaternion.identity);
            go.GetComponent<PowerUp>().Configure(_config, type);
        }

        private static PowerUpType RandomType()
        {
            var values = System.Enum.GetValues(typeof(PowerUpType));
            return (PowerUpType)values.GetValue(Random.Range(0, values.Length));
        }

        /// <summary>Returns a power-up instance to the pool.</summary>
        public void Release(GameObject go)
        {
            _pool?.Release(go);
        }

        /// <summary>Recycles all active power-ups.</summary>
        public void ReleaseAll()
        {
            _pool?.ReleaseAll();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}

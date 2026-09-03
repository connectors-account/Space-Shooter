using UnityEngine;
using SpaceShooter.Core;

namespace SpaceShooter.Pickups
{
    /// <summary>
    /// MonoBehaviour that owns the power-up object pool and exposes a static
    /// weighted-random spawn helper called by EnemyBase.Die().
    /// </summary>
    public class PowerUpSpawner : MonoBehaviour
    {
        #region Static Access
        /// <summary>Shared pool used to recycle power-up GameObjects.</summary>
        public static GameObjectPool Pool { get; private set; }
        private static PowerUpSpawner _instance;
        #endregion

        #region Inspector Fields
        [SerializeField] private GameObject _powerUpPrefab;
        [SerializeField] private int _prewarm = 8;

        [Header("Weighted Drop Table")]
        [SerializeField] private int _weightShield = 20;
        [SerializeField] private int _weightTriple = 20;
        [SerializeField] private int _weightSpread5 = 15;
        [SerializeField] private int _weightSpeed = 15;
        [SerializeField] private int _weightLaser = 10;
        [SerializeField] private int _weightHealth = 15;
        [SerializeField] private int _weightNuke = 5;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;

            GameObject root = new GameObject("PowerUpPool");
            root.transform.SetParent(transform, false);
            Pool = root.AddComponent<GameObjectPool>();
            Pool.Initialize(_powerUpPrefab, _prewarm, root.transform);
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
                Pool = null;
            }
        }
        #endregion

        #region Spawning
        /// <summary>
        /// Rolls the overall drop chance; if it passes, spawns a weighted-random
        /// power-up at the given position from the pool.
        /// </summary>
        public static void Spawn(Vector3 pos)
        {
            if (_instance == null || Pool == null) return;
            if (Random.value > GameConstants.POWERUP_DROP_CHANCE) return;
            SpawnGuaranteed(pos);
        }

        /// <summary>Spawns a weighted-random power-up ignoring the drop chance (boss drops).</summary>
        public static void SpawnGuaranteed(Vector3 pos)
        {
            if (_instance == null || Pool == null) return;
            PowerUpType type = _instance.RollType();
            SpawnType(pos, type);
        }

        /// <summary>Spawns a specific power-up type at a position.</summary>
        public static void SpawnType(Vector3 pos, PowerUpType type)
        {
            if (Pool == null) return;
            GameObject go = Pool.Get(pos);
            PowerUp pu = go.GetComponent<PowerUp>();
            if (pu == null) pu = go.AddComponent<PowerUp>();
            pu.Configure(type);
        }

        private PowerUpType RollType()
        {
            int total = _weightShield + _weightTriple + _weightSpread5 + _weightSpeed
                        + _weightLaser + _weightHealth + _weightNuke;
            int roll = Random.Range(0, Mathf.Max(1, total));

            if ((roll -= _weightShield) < 0) return PowerUpType.Shield;
            if ((roll -= _weightTriple) < 0) return PowerUpType.TripleShot;
            if ((roll -= _weightSpread5) < 0) return PowerUpType.Spread5;
            if ((roll -= _weightSpeed) < 0) return PowerUpType.SpeedBoost;
            if ((roll -= _weightLaser) < 0) return PowerUpType.Laser;
            if ((roll -= _weightHealth) < 0) return PowerUpType.HealthPack;
            return PowerUpType.Nuke;
        }
        #endregion
    }
}

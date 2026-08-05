using System.Collections.Generic;
using UnityEngine;

namespace SpaceShooter.Core
{
    /// <summary>
    /// A single pool configuration entry, editable in the Inspector.
    /// </summary>
    [System.Serializable]
    public class PoolConfig
    {
        [Tooltip("Unique key used to acquire objects from this pool (see Constants).")]
        public string key;

        [Tooltip("Prefab (or template GameObject) that will be instantiated.")]
        public GameObject prefab;

        [Tooltip("How many instances to create up front.")]
        public int prewarmCount = 16;

        [Tooltip("Allow the pool to grow beyond the prewarm count when empty.")]
        public bool expandable = true;
    }

    /// <summary>
    /// Generic, dictionary-keyed object pool. Works for bullets, enemies,
    /// VFX particles and power-ups. Pre-warms configured pools on Awake.
    ///
    /// Usage:
    ///   var go = ObjectPool.Instance.Acquire(Constants.PoolPlayerBullet);
    ///   ObjectPool.Instance.Release(Constants.PoolPlayerBullet, go);
    /// </summary>
    public class ObjectPool : MonoBehaviour
    {
        public static ObjectPool Instance { get; private set; }

        [Tooltip("Pools configured in the Inspector.")]
        [SerializeField] private List<PoolConfig> pools = new List<PoolConfig>();

        // Runtime data.
        private readonly Dictionary<string, Queue<GameObject>> _available =
            new Dictionary<string, Queue<GameObject>>();
        private readonly Dictionary<string, PoolConfig> _configByKey =
            new Dictionary<string, PoolConfig>();
        // Remember which pool an object belongs to (used if released without key).
        private readonly Dictionary<GameObject, string> _keyByObject =
            new Dictionary<GameObject, string>();

        private Transform _root;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            _root = transform;
            foreach (var cfg in pools)
            {
                if (cfg == null || string.IsNullOrEmpty(cfg.key) || cfg.prefab == null)
                    continue;
                RegisterPool(cfg);
            }
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        /// <summary>Register (and pre-warm) a pool at runtime.</summary>
        public void RegisterPool(PoolConfig cfg)
        {
            if (_configByKey.ContainsKey(cfg.key)) return;

            _configByKey[cfg.key] = cfg;
            var queue = new Queue<GameObject>();
            _available[cfg.key] = queue;

            for (int i = 0; i < cfg.prewarmCount; i++)
            {
                var obj = CreateInstance(cfg);
                obj.SetActive(false);
                queue.Enqueue(obj);
            }
        }

        /// <summary>
        /// Register a pool from a prefab at runtime if it does not already exist.
        /// Useful when systems set themselves up without Inspector wiring.
        /// </summary>
        public void EnsurePool(string key, GameObject prefab, int prewarmCount = 8, bool expandable = true)
        {
            if (_configByKey.ContainsKey(key)) return;
            RegisterPool(new PoolConfig
            {
                key = key,
                prefab = prefab,
                prewarmCount = prewarmCount,
                expandable = expandable
            });
        }

        private GameObject CreateInstance(PoolConfig cfg)
        {
            var obj = Instantiate(cfg.prefab, _root);
            obj.name = cfg.key + "_pooled";
            _keyByObject[obj] = cfg.key;
            return obj;
        }

        /// <summary>Acquire an inactive object from the pool identified by key.</summary>
        public GameObject Acquire(string key)
        {
            if (!_available.TryGetValue(key, out var queue))
            {
                Debug.LogWarning($"[ObjectPool] No pool registered for key '{key}'.");
                return null;
            }

            GameObject obj = null;
            // Find a genuinely inactive instance.
            if (queue.Count > 0)
            {
                obj = queue.Dequeue();
            }
            else if (_configByKey.TryGetValue(key, out var cfg) && cfg.expandable)
            {
                obj = CreateInstance(cfg);
            }

            if (obj == null)
            {
                Debug.LogWarning($"[ObjectPool] Pool '{key}' exhausted and not expandable.");
                return null;
            }

            obj.SetActive(true);
            return obj;
        }

        /// <summary>Acquire and position an object in one call.</summary>
        public GameObject Acquire(string key, Vector3 position, Quaternion rotation)
        {
            var obj = Acquire(key);
            if (obj != null)
            {
                obj.transform.SetPositionAndRotation(position, rotation);
            }
            return obj;
        }

        /// <summary>Return an object to its pool and deactivate it.</summary>
        public void Release(string key, GameObject obj)
        {
            if (obj == null) return;
            obj.SetActive(false);
            obj.transform.SetParent(_root, false);

            if (!_available.TryGetValue(key, out var queue))
            {
                // Unknown key: create a queue so we do not leak.
                queue = new Queue<GameObject>();
                _available[key] = queue;
            }
            queue.Enqueue(obj);
        }

        /// <summary>Return an object without knowing its key (looked up internally).</summary>
        public void Release(GameObject obj)
        {
            if (obj == null) return;
            if (_keyByObject.TryGetValue(obj, out var key))
            {
                Release(key, obj);
            }
            else
            {
                // Not a pooled object – just deactivate.
                obj.SetActive(false);
            }
        }

        /// <summary>Deactivate every active object across all pools.</summary>
        public void ReleaseAllActive()
        {
            foreach (var pair in _keyByObject)
            {
                var obj = pair.Key;
                if (obj != null && obj.activeSelf)
                {
                    Release(pair.Value, obj);
                }
            }
        }
    }
}

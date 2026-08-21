using System.Collections.Generic;
using UnityEngine;

namespace SpaceShooter.Core
{
    /// <summary>
    /// Generic tag-based object pool for bullets, enemies, explosions and power-ups.
    /// Auto-expands when a pool runs empty.
    /// </summary>
    public class ObjectPool : MonoBehaviour
    {
        [System.Serializable]
        public class Pool
        {
            public string tag;
            public GameObject prefab;
            public int size = 20;
        }

        public static ObjectPool Instance { get; private set; }

        [Header("Pools")]
        [SerializeField] private List<Pool> pools = new List<Pool>();

        private readonly Dictionary<string, Queue<GameObject>> poolDictionary = new Dictionary<string, Queue<GameObject>>();
        private readonly Dictionary<string, GameObject> prefabLookup = new Dictionary<string, GameObject>();
        private readonly Dictionary<string, Transform> containerLookup = new Dictionary<string, Transform>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            InitializePools();
        }

        private void InitializePools()
        {
            foreach (Pool pool in pools)
            {
                if (string.IsNullOrEmpty(pool.tag) || pool.prefab == null) continue;
                CreatePool(pool.tag, pool.prefab, pool.size);
            }
        }

        /// <summary>Create a pool at runtime (useful when prefabs are wired programmatically).</summary>
        public void CreatePool(string tag, GameObject prefab, int size)
        {
            if (poolDictionary.ContainsKey(tag)) return;

            GameObject container = new GameObject($"Pool_{tag}");
            container.transform.SetParent(transform);
            containerLookup[tag] = container.transform;
            prefabLookup[tag] = prefab;

            Queue<GameObject> objectQueue = new Queue<GameObject>();
            for (int i = 0; i < size; i++)
            {
                GameObject obj = CreateNewInstance(tag);
                objectQueue.Enqueue(obj);
            }
            poolDictionary[tag] = objectQueue;
        }

        private GameObject CreateNewInstance(string tag)
        {
            GameObject obj = Instantiate(prefabLookup[tag], containerLookup[tag]);
            obj.SetActive(false);
            return obj;
        }

        /// <summary>Fetch an object from a pool, expanding the pool if it is empty.</summary>
        public GameObject GetObject(string tag)
        {
            if (!poolDictionary.ContainsKey(tag))
            {
                Debug.LogWarning($"ObjectPool: pool with tag '{tag}' does not exist.");
                return null;
            }

            Queue<GameObject> queue = poolDictionary[tag];

            GameObject obj;
            if (queue.Count == 0)
            {
                // Auto-expand.
                obj = CreateNewInstance(tag);
            }
            else
            {
                obj = queue.Dequeue();
                // Guard against destroyed references.
                if (obj == null)
                {
                    obj = CreateNewInstance(tag);
                }
            }

            obj.SetActive(true);
            return obj;
        }

        public GameObject GetObject(string tag, Vector3 position, Quaternion rotation)
        {
            GameObject obj = GetObject(tag);
            if (obj != null)
            {
                obj.transform.SetPositionAndRotation(position, rotation);
            }
            return obj;
        }

        /// <summary>Return an object to its pool.</summary>
        public void ReturnObject(string tag, GameObject obj)
        {
            if (obj == null) return;

            obj.SetActive(false);

            if (!poolDictionary.ContainsKey(tag))
            {
                // No such pool: just deactivate & parent under this object to avoid leaks.
                obj.transform.SetParent(transform);
                return;
            }

            if (containerLookup.ContainsKey(tag))
            {
                obj.transform.SetParent(containerLookup[tag]);
            }
            poolDictionary[tag].Enqueue(obj);
        }

        public bool HasPool(string tag)
        {
            return poolDictionary.ContainsKey(tag);
        }
    }
}

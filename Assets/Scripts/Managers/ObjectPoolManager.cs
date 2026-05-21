using System.Collections.Generic;
using UnityEngine;

namespace SpaceShooter.Managers
{
    /// <summary>
    /// High-performance object pool manager. Pre-allocates pools at start,
    /// reuses deactivated objects to avoid runtime allocations.
    /// </summary>
    public class ObjectPoolManager : MonoBehaviour
    {
        public static ObjectPoolManager Instance { get; private set; }

        [System.Serializable]
        public class Pool
        {
            public string tag;
            public GameObject prefab;
            public int initialSize = 20;
        }

        [SerializeField] private List<Pool> pools = new List<Pool>();

        private Dictionary<string, Queue<GameObject>> poolDictionary;
        private Dictionary<string, Pool> poolDefinitions;

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
            poolDictionary = new Dictionary<string, Queue<GameObject>>();
            poolDefinitions = new Dictionary<string, Pool>();

            foreach (Pool pool in pools)
            {
                Queue<GameObject> objectPool = new Queue<GameObject>();
                poolDefinitions[pool.tag] = pool;

                GameObject container = new GameObject($"Pool_{pool.tag}");
                container.transform.SetParent(transform);

                for (int i = 0; i < pool.initialSize; i++)
                {
                    GameObject obj = Instantiate(pool.prefab, container.transform);
                    obj.SetActive(false);
                    objectPool.Enqueue(obj);
                }

                poolDictionary[pool.tag] = objectPool;
            }
        }

        /// <summary>
        /// Get an object from the named pool. If all objects are active,
        /// instantiates a new one and adds it to the pool.
        /// </summary>
        public GameObject GetFromPool(string tag, Vector3 position, Quaternion rotation)
        {
            if (!poolDictionary.ContainsKey(tag))
            {
                Debug.LogWarning($"ObjectPoolManager: Pool with tag '{tag}' does not exist.");
                return null;
            }

            Queue<GameObject> pool = poolDictionary[tag];

            // Try to find an inactive object
            int searched = 0;
            int poolCount = pool.Count;
            while (searched < poolCount)
            {
                GameObject obj = pool.Dequeue();
                pool.Enqueue(obj);

                if (!obj.activeInHierarchy)
                {
                    obj.transform.position = position;
                    obj.transform.rotation = rotation;
                    obj.SetActive(true);
                    return obj;
                }
                searched++;
            }

            // All objects are active – expand pool
            if (poolDefinitions.ContainsKey(tag))
            {
                GameObject newObj = Instantiate(poolDefinitions[tag].prefab, position, rotation);
                newObj.transform.SetParent(transform.Find($"Pool_{tag}"));
                pool.Enqueue(newObj);
                newObj.SetActive(true);
                return newObj;
            }

            return null;
        }

        /// <summary>
        /// Deactivate all objects in all pools.
        /// Useful when resetting the game.
        /// </summary>
        public void ReturnAllToPool()
        {
            foreach (var kvp in poolDictionary)
            {
                foreach (GameObject obj in kvp.Value)
                {
                    if (obj != null)
                        obj.SetActive(false);
                }
            }
        }
    }
}

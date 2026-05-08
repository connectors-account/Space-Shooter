using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Generic object pool for performance-critical objects like bullets and enemies.
/// </summary>
public class ObjectPool : MonoBehaviour
{
    public static ObjectPool Instance { get; private set; }

    [System.Serializable]
    public class Pool
    {
        public string tag;
        public GameObject prefab;
        public int initialSize = 20;
    }

    [SerializeField] private List<Pool> pools = new List<Pool>();
    private Dictionary<string, Queue<GameObject>> poolDictionary = new Dictionary<string, Queue<GameObject>>();
    private Dictionary<string, Pool> poolLookup = new Dictionary<string, Pool>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        InitializePools();
    }

    private void InitializePools()
    {
        foreach (Pool pool in pools)
        {
            Queue<GameObject> objectPool = new Queue<GameObject>();
            poolLookup[pool.tag] = pool;

            for (int i = 0; i < pool.initialSize; i++)
            {
                GameObject obj = CreateNewObject(pool);
                objectPool.Enqueue(obj);
            }

            poolDictionary[pool.tag] = objectPool;
        }
    }

    private GameObject CreateNewObject(Pool pool)
    {
        GameObject obj = Instantiate(pool.prefab, transform);
        obj.SetActive(false);
        return obj;
    }

    /// <summary>
    /// Register a pool at runtime (used by scene setup).
    /// </summary>
    public void RegisterPool(string tag, GameObject prefab, int initialSize = 20)
    {
        if (poolDictionary.ContainsKey(tag)) return;

        Pool pool = new Pool { tag = tag, prefab = prefab, initialSize = initialSize };
        pools.Add(pool);
        poolLookup[tag] = pool;

        Queue<GameObject> objectPool = new Queue<GameObject>();
        for (int i = 0; i < initialSize; i++)
        {
            GameObject obj = CreateNewObject(pool);
            objectPool.Enqueue(obj);
        }
        poolDictionary[tag] = objectPool;
    }

    /// <summary>
    /// Spawn an object from the pool at a given position and rotation.
    /// </summary>
    public GameObject Spawn(string tag, Vector3 position, Quaternion rotation)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning($"ObjectPool: No pool with tag '{tag}' found.");
            return null;
        }

        Queue<GameObject> pool = poolDictionary[tag];
        GameObject obj;

        if (pool.Count > 0)
        {
            obj = pool.Dequeue();
        }
        else
        {
            // Expand pool dynamically
            obj = CreateNewObject(poolLookup[tag]);
        }

        obj.transform.position = position;
        obj.transform.rotation = rotation;
        obj.SetActive(true);

        IPoolable poolable = obj.GetComponent<IPoolable>();
        poolable?.OnSpawnFromPool();

        return obj;
    }

    /// <summary>
    /// Return an object to its pool.
    /// </summary>
    public void Despawn(string tag, GameObject obj)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning($"ObjectPool: No pool with tag '{tag}' found. Destroying object.");
            Destroy(obj);
            return;
        }

        IPoolable poolable = obj.GetComponent<IPoolable>();
        poolable?.OnReturnToPool();

        obj.SetActive(false);
        poolDictionary[tag].Enqueue(obj);
    }

    /// <summary>
    /// Despawn all active objects across all pools.
    /// </summary>
    public void DespawnAll()
    {
        foreach (var kvp in poolDictionary)
        {
            // Find all active children with matching pool tag
            foreach (Transform child in transform)
            {
                if (child.gameObject.activeSelf)
                {
                    PooledObject po = child.GetComponent<PooledObject>();
                    if (po != null && po.poolTag == kvp.Key)
                    {
                        Despawn(kvp.Key, child.gameObject);
                    }
                }
            }
        }
    }
}

/// <summary>
/// Interface for objects that need initialization/cleanup when pooled.
/// </summary>
public interface IPoolable
{
    void OnSpawnFromPool();
    void OnReturnToPool();
}

/// <summary>
/// Simple component to track which pool an object belongs to.
/// </summary>
public class PooledObject : MonoBehaviour
{
    public string poolTag;
}

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ObjectPooler implements the Object Pool pattern for efficient
/// instantiation and reuse of frequently spawned objects like bullets and enemies.
/// </summary>
public class ObjectPooler : MonoBehaviour
{
    /// <summary>
    /// Represents a pool of objects
    /// </summary>
    [System.Serializable]
    public class Pool
    {
        public string tag;                  // Identifier for this pool
        public GameObject prefab;           // Prefab to instantiate
        public int size;                    // Initial pool size
        public bool expandable = true;      // Can pool grow if needed
    }

    // Singleton instance
    public static ObjectPooler Instance { get; private set; }

    [Header("Pool Configuration")]
    [SerializeField] private List<Pool> pools;

    // Dictionary for quick pool lookup
    private Dictionary<string, Queue<GameObject>> poolDictionary;
    private Dictionary<string, Pool> poolConfig;
    private Dictionary<string, Transform> poolContainers;

    private void Awake()
    {
        // Singleton setup
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        InitializePools();
    }

    /// <summary>
    /// Initialize all configured pools
    /// </summary>
    private void InitializePools()
    {
        poolDictionary = new Dictionary<string, Queue<GameObject>>();
        poolConfig = new Dictionary<string, Pool>();
        poolContainers = new Dictionary<string, Transform>();

        foreach (Pool pool in pools)
        {
            // Create container for organization
            GameObject container = new GameObject($"Pool_{pool.tag}");
            container.transform.SetParent(transform);
            poolContainers[pool.tag] = container.transform;

            // Create queue for this pool
            Queue<GameObject> objectPool = new Queue<GameObject>();

            // Instantiate initial pool objects
            for (int i = 0; i < pool.size; i++)
            {
                GameObject obj = CreatePooledObject(pool, container.transform);
                objectPool.Enqueue(obj);
            }

            poolDictionary[pool.tag] = objectPool;
            poolConfig[pool.tag] = pool;
        }
    }

    /// <summary>
    /// Create a new object for the pool
    /// </summary>
    private GameObject CreatePooledObject(Pool pool, Transform parent)
    {
        GameObject obj = Instantiate(pool.prefab, parent);
        obj.SetActive(false);
        return obj;
    }

    /// <summary>
    /// Spawn an object from the specified pool
    /// </summary>
    /// <param name="tag">Pool identifier</param>
    /// <param name="position">Spawn position</param>
    /// <param name="rotation">Spawn rotation</param>
    /// <returns>The spawned GameObject, or null if pool doesn't exist</returns>
    public GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning($"Pool with tag '{tag}' doesn't exist!");
            return null;
        }

        Queue<GameObject> pool = poolDictionary[tag];
        GameObject objectToSpawn = null;

        // Find an inactive object in the pool
        int attempts = pool.Count;
        while (attempts > 0)
        {
            objectToSpawn = pool.Dequeue();
            pool.Enqueue(objectToSpawn);

            if (!objectToSpawn.activeInHierarchy)
            {
                break;
            }
            
            objectToSpawn = null;
            attempts--;
        }

        // If no inactive object found and pool is expandable, create new one
        if (objectToSpawn == null)
        {
            if (poolConfig[tag].expandable)
            {
                objectToSpawn = CreatePooledObject(poolConfig[tag], poolContainers[tag]);
                pool.Enqueue(objectToSpawn);
            }
            else
            {
                Debug.LogWarning($"Pool '{tag}' is full and not expandable!");
                return null;
            }
        }

        // Setup the object
        objectToSpawn.transform.position = position;
        objectToSpawn.transform.rotation = rotation;
        objectToSpawn.SetActive(true);

        // Call interface method if implemented
        IPooledObject pooledObj = objectToSpawn.GetComponent<IPooledObject>();
        if (pooledObj != null)
        {
            pooledObj.OnObjectSpawn();
        }

        return objectToSpawn;
    }

    /// <summary>
    /// Return all active objects in a pool to inactive state
    /// </summary>
    public void ReturnAllToPool(string tag)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            return;
        }

        foreach (GameObject obj in poolDictionary[tag])
        {
            if (obj.activeInHierarchy)
            {
                obj.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Return all objects across all pools to inactive state
    /// </summary>
    public void ReturnAllToPool()
    {
        foreach (string tag in poolDictionary.Keys)
        {
            ReturnAllToPool(tag);
        }
    }

    /// <summary>
    /// Get the count of active objects in a pool
    /// </summary>
    public int GetActiveCount(string tag)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            return 0;
        }

        int count = 0;
        foreach (GameObject obj in poolDictionary[tag])
        {
            if (obj.activeInHierarchy)
            {
                count++;
            }
        }
        return count;
    }
}

/// <summary>
/// Interface for pooled objects that need initialization when spawned
/// </summary>
public interface IPooledObject
{
    void OnObjectSpawn();
}

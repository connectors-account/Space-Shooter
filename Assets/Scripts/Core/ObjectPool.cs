using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Generic object pool for reusing GameObjects (bullets, enemies, effects).
/// Reduces garbage collection overhead from frequent Instantiate/Destroy calls.
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

    public List<Pool> pools;
    private Dictionary<string, Queue<GameObject>> poolDictionary;
    private Dictionary<string, Pool> poolLookup;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        poolDictionary = new Dictionary<string, Queue<GameObject>>();
        poolLookup = new Dictionary<string, Pool>();

        foreach (Pool pool in pools)
        {
            Queue<GameObject> objectQueue = new Queue<GameObject>();
            poolLookup[pool.tag] = pool;

            for (int i = 0; i < pool.initialSize; i++)
            {
                GameObject obj = Instantiate(pool.prefab, transform);
                obj.SetActive(false);
                objectQueue.Enqueue(obj);
            }

            poolDictionary[pool.tag] = objectQueue;
        }
    }

    /// <summary>
    /// Retrieve a pooled object by tag. Spawns at the given position and rotation.
    /// Grows the pool automatically if exhausted.
    /// </summary>
    public GameObject Spawn(string tag, Vector3 position, Quaternion rotation)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning($"ObjectPool: No pool with tag '{tag}' exists.");
            return null;
        }

        Queue<GameObject> queue = poolDictionary[tag];
        GameObject objectToSpawn;

        if (queue.Count > 0)
        {
            objectToSpawn = queue.Dequeue();
        }
        else
        {
            // Auto-grow: instantiate a new one if pool is exhausted
            objectToSpawn = Instantiate(poolLookup[tag].prefab, transform);
        }

        objectToSpawn.SetActive(true);
        objectToSpawn.transform.position = position;
        objectToSpawn.transform.rotation = rotation;

        IPoolable poolable = objectToSpawn.GetComponent<IPoolable>();
        if (poolable != null)
        {
            poolable.OnSpawnFromPool();
        }

        return objectToSpawn;
    }

    /// <summary>
    /// Return an object to its pool instead of destroying it.
    /// </summary>
    public void ReturnToPool(string tag, GameObject obj)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning($"ObjectPool: No pool with tag '{tag}'. Destroying object instead.");
            Destroy(obj);
            return;
        }

        obj.SetActive(false);
        poolDictionary[tag].Enqueue(obj);
    }
}

/// <summary>
/// Interface for objects that need initialization when spawned from pool.
/// </summary>
public interface IPoolable
{
    void OnSpawnFromPool();
}

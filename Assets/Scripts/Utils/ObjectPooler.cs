// =============================================================================
// ObjectPooler.cs — Generic object pooling system
// =============================================================================
using UnityEngine;
using System.Collections.Generic;

namespace SpaceShooter.Utils
{
    /// <summary>
    /// Defines a pool entry with a prefab, size, and tag.
    /// </summary>
    [System.Serializable]
    public class PoolEntry
    {
        public string tag;
        public GameObject prefab;
        public int size = 20;
    }

    /// <summary>
    /// Generic object pooler to reduce Instantiate/Destroy overhead.
    /// </summary>
    public class ObjectPooler : MonoBehaviour
    {
        public static ObjectPooler Instance { get; private set; }

        [SerializeField] private List<PoolEntry> pools = new List<PoolEntry>();

        private Dictionary<string, Queue<GameObject>> poolDict;
        private Dictionary<string, PoolEntry> entryDict;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            poolDict = new Dictionary<string, Queue<GameObject>>();
            entryDict = new Dictionary<string, PoolEntry>();

            foreach (PoolEntry entry in pools)
            {
                Queue<GameObject> queue = new Queue<GameObject>();
                entryDict[entry.tag] = entry;

                for (int i = 0; i < entry.size; i++)
                {
                    GameObject obj = Instantiate(entry.prefab);
                    obj.SetActive(false);
                    obj.transform.SetParent(transform);
                    queue.Enqueue(obj);
                }

                poolDict[entry.tag] = queue;
            }
        }

        /// <summary>
        /// Gets an object from the pool. Creates new if pool is empty.
        /// </summary>
        public GameObject GetFromPool(string tag, Vector3 position, Quaternion rotation)
        {
            if (!poolDict.ContainsKey(tag))
            {
                Debug.LogWarning($"Pool with tag '{tag}' not found.");
                return null;
            }

            GameObject obj;
            if (poolDict[tag].Count > 0)
            {
                obj = poolDict[tag].Dequeue();
            }
            else
            {
                // Expand pool
                obj = Instantiate(entryDict[tag].prefab);
                obj.transform.SetParent(transform);
            }

            obj.transform.position = position;
            obj.transform.rotation = rotation;
            obj.SetActive(true);
            return obj;
        }

        /// <summary>
        /// Returns an object to the pool.
        /// </summary>
        public void ReturnToPool(string tag, GameObject obj)
        {
            if (!poolDict.ContainsKey(tag))
            {
                Destroy(obj);
                return;
            }

            obj.SetActive(false);
            poolDict[tag].Enqueue(obj);
        }
    }
}

// ============================================================================
// ObjectPool.cs — Generic object pooling system
// Pre-instantiates a configurable number of GameObjects and recycles them
// to avoid GC spikes from frequent Instantiate/Destroy calls (bullets, enemies).
// ============================================================================
using System.Collections.Generic;
using UnityEngine;

namespace SpaceShooter.Core
{
    [System.Serializable]
    public class PoolEntry
    {
        public string tag;            // lookup key (e.g. "PlayerBullet")
        public GameObject prefab;
        public int initialSize = 20;
    }

    public class ObjectPool : MonoBehaviour
    {
        public static ObjectPool Instance { get; private set; }

        [SerializeField] private List<PoolEntry> pools = new List<PoolEntry>();

        private Dictionary<string, Queue<GameObject>> _poolDict;
        private Dictionary<string, PoolEntry> _entryDict;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;

            _poolDict = new Dictionary<string, Queue<GameObject>>();
            _entryDict = new Dictionary<string, PoolEntry>();

            foreach (var entry in pools)
            {
                var queue = new Queue<GameObject>();
                _entryDict[entry.tag] = entry;

                for (int i = 0; i < entry.initialSize; i++)
                {
                    var obj = Instantiate(entry.prefab);
                    obj.SetActive(false);
                    obj.transform.SetParent(transform);
                    queue.Enqueue(obj);
                }
                _poolDict[entry.tag] = queue;
            }
        }

        /// <summary>
        /// Retrieve an inactive object from the pool, positioned and activated.
        /// If the pool is empty a new instance is created (pool grows on demand).
        /// </summary>
        public GameObject Get(string tag, Vector3 position, Quaternion rotation)
        {
            if (!_poolDict.ContainsKey(tag))
            {
                Debug.LogWarning($"ObjectPool: No pool with tag '{tag}'");
                return null;
            }

            GameObject obj;
            if (_poolDict[tag].Count > 0)
            {
                obj = _poolDict[tag].Dequeue();
            }
            else
            {
                // Pool exhausted — grow by one
                obj = Instantiate(_entryDict[tag].prefab);
                obj.transform.SetParent(transform);
            }

            obj.transform.position = position;
            obj.transform.rotation = rotation;
            obj.SetActive(true);
            return obj;
        }

        /// <summary>
        /// Return an object to its pool (deactivates it).
        /// </summary>
        public void ReturnToPool(string tag, GameObject obj)
        {
            if (!_poolDict.ContainsKey(tag))
            {
                Debug.LogWarning($"ObjectPool: Returning to unknown tag '{tag}', destroying.");
                Destroy(obj);
                return;
            }
            obj.SetActive(false);
            _poolDict[tag].Enqueue(obj);
        }
    }
}

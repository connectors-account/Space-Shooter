using System.Collections.Generic;
using UnityEngine;

namespace SpaceShooter.Core
{
    public class PoolManager : MonoBehaviour
    {
        private class Pool
        {
            public readonly GameObject Prefab;
            public readonly Transform Parent;
            public readonly Queue<GameObject> Queue = new Queue<GameObject>();
            public readonly bool CanExpand;

            public Pool(GameObject prefab, Transform parent, bool canExpand)
            {
                Prefab = prefab;
                Parent = parent;
                CanExpand = canExpand;
            }
        }

        private readonly Dictionary<string, Pool> _pools = new Dictionary<string, Pool>();

        public void RegisterPool(string key, GameObject prefab, int initialSize, bool canExpand = true)
        {
            if (_pools.ContainsKey(key) || prefab == null)
            {
                return;
            }

            var parent = new GameObject($"Pool_{key}").transform;
            parent.SetParent(transform, false);

            var pool = new Pool(prefab, parent, canExpand);
            _pools[key] = pool;

            for (var i = 0; i < Mathf.Max(1, initialSize); i++)
            {
                var instance = CreateInstance(pool, key);
                ReturnToPool(key, instance);
            }
        }

        public GameObject Spawn(string key, Vector3 position, Quaternion rotation)
        {
            if (!_pools.TryGetValue(key, out var pool))
            {
                Debug.LogError($"Pool key '{key}' is not registered.");
                return null;
            }

            GameObject obj = null;
            while (pool.Queue.Count > 0 && obj == null)
            {
                obj = pool.Queue.Dequeue();
            }

            if (obj == null)
            {
                if (!pool.CanExpand)
                {
                    return null;
                }

                obj = CreateInstance(pool, key);
            }

            obj.transform.SetPositionAndRotation(position, rotation);
            obj.SetActive(true);

            if (obj.TryGetComponent<IPoolable>(out var poolable))
            {
                poolable.OnSpawned();
            }

            return obj;
        }

        public void ReturnToPool(string key, GameObject obj)
        {
            if (obj == null)
            {
                return;
            }

            if (!_pools.TryGetValue(key, out var pool))
            {
                Destroy(obj);
                return;
            }

            if (obj.TryGetComponent<IPoolable>(out var poolable))
            {
                poolable.OnReturnedToPool();
            }

            obj.SetActive(false);
            obj.transform.SetParent(pool.Parent, false);
            pool.Queue.Enqueue(obj);
        }

        private GameObject CreateInstance(Pool pool, string key)
        {
            var obj = Instantiate(pool.Prefab, pool.Parent);
            obj.name = $"{pool.Prefab.name}_Pooled";

            if (obj.TryGetComponent<PooledIdentity>(out var identity))
            {
                identity.Configure(this, key);
            }

            return obj;
        }
    }

    public interface IPoolable
    {
        void OnSpawned();
        void OnReturnedToPool();
    }

    public class PooledIdentity : MonoBehaviour
    {
        public string PoolKey { get; private set; }

        private PoolManager _poolManager;

        public void Configure(PoolManager poolManager, string poolKey)
        {
            _poolManager = poolManager;
            PoolKey = poolKey;
        }

        public void ReturnSelfToPool()
        {
            _poolManager?.ReturnToPool(PoolKey, gameObject);
        }
    }
}

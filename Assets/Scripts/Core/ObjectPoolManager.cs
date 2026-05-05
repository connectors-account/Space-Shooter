using System.Collections.Generic;
using UnityEngine;

namespace SpaceShooter.Core
{
    public class PoolIdentity : MonoBehaviour
    {
        public string PoolKey;
    }

    /// <summary>
    /// Generic runtime pool for bullets, enemies and power-ups.
    /// </summary>
    public class ObjectPoolManager : MonoBehaviour
    {
        private readonly Dictionary<string, Queue<GameObject>> _pools = new();
        private readonly Dictionary<string, GameObject> _prefabs = new();

        public void RegisterPool(string key, GameObject prefab, int prewarmCount)
        {
            if (_prefabs.ContainsKey(key)) return;

            _prefabs[key] = prefab;
            _pools[key] = new Queue<GameObject>(prewarmCount);

            for (var i = 0; i < prewarmCount; i++)
            {
                var instance = InstantiateNew(key);
                instance.SetActive(false);
                _pools[key].Enqueue(instance);
            }
        }

        public GameObject Get(string key, Vector3 position, Quaternion rotation)
        {
            if (!_prefabs.ContainsKey(key))
            {
                Debug.LogError($"Pool key not registered: {key}");
                return null;
            }

            var queue = _pools[key];
            var instance = queue.Count > 0 ? queue.Dequeue() : InstantiateNew(key);
            instance.transform.SetPositionAndRotation(position, rotation);
            instance.SetActive(true);
            return instance;
        }

        public void Release(GameObject instance)
        {
            if (!instance.TryGetComponent(out PoolIdentity identity) || string.IsNullOrEmpty(identity.PoolKey))
            {
                Destroy(instance);
                return;
            }

            if (!_pools.ContainsKey(identity.PoolKey))
            {
                Destroy(instance);
                return;
            }

            instance.SetActive(false);
            _pools[identity.PoolKey].Enqueue(instance);
        }

        private GameObject InstantiateNew(string key)
        {
            var instance = Instantiate(_prefabs[key], transform);
            var identity = instance.GetComponent<PoolIdentity>() ?? instance.AddComponent<PoolIdentity>();
            identity.PoolKey = key;
            return instance;
        }
    }
}

using System.Collections.Generic;
using UnityEngine;

namespace SpaceShooter.Core
{
    /// <summary>
    /// Objects that can live inside an <see cref="ObjectPool"/> implement this interface so they
    /// can reset their state when reused.
    /// </summary>
    public interface IPoolable
    {
        /// <summary>Called when the object is taken from the pool and re-activated.</summary>
        void OnSpawned();

        /// <summary>Called immediately before the object is returned to the pool.</summary>
        void OnDespawned();
    }

    /// <summary>
    /// A lightweight, allocation-free GameObject pool. Used for bullets, enemies, power-ups and
    /// explosion particles to avoid per-frame Instantiate/Destroy garbage during gameplay.
    /// </summary>
    public class ObjectPool
    {
        private readonly GameObject _prefab;
        private readonly Transform _parent;
        private readonly Queue<GameObject> _available = new Queue<GameObject>();
        private readonly HashSet<GameObject> _active = new HashSet<GameObject>();

        /// <summary>Number of currently active (in-use) instances.</summary>
        public int ActiveCount => _active.Count;

        /// <summary>
        /// Creates a new pool for the given prefab.
        /// </summary>
        /// <param name="prefab">Prefab (or template GameObject) that will be cloned.</param>
        /// <param name="parent">Optional parent transform that owns the pooled instances.</param>
        /// <param name="prewarm">Number of instances to instantiate up front.</param>
        public ObjectPool(GameObject prefab, Transform parent = null, int prewarm = 0)
        {
            _prefab = prefab;
            _parent = parent;
            for (int i = 0; i < prewarm; i++)
            {
                GameObject go = CreateInstance();
                go.SetActive(false);
                _available.Enqueue(go);
            }
        }

        /// <summary>
        /// Retrieves an instance from the pool (creating one if none are free), positions it and
        /// activates it.
        /// </summary>
        /// <param name="position">World position to place the instance at.</param>
        /// <param name="rotation">World rotation to apply.</param>
        public GameObject Get(Vector3 position, Quaternion rotation)
        {
            GameObject go = _available.Count > 0 ? _available.Dequeue() : CreateInstance();
            go.transform.SetPositionAndRotation(position, rotation);
            go.SetActive(true);
            _active.Add(go);

            foreach (IPoolable poolable in go.GetComponents<IPoolable>())
            {
                poolable.OnSpawned();
            }
            return go;
        }

        /// <summary>
        /// Returns an instance to the pool, deactivating it. Safe to call multiple times.
        /// </summary>
        /// <param name="go">The instance previously obtained from this pool.</param>
        public void Release(GameObject go)
        {
            if (go == null || !_active.Contains(go))
            {
                return;
            }

            foreach (IPoolable poolable in go.GetComponents<IPoolable>())
            {
                poolable.OnDespawned();
            }

            _active.Remove(go);
            go.SetActive(false);
            if (_parent != null)
            {
                go.transform.SetParent(_parent, false);
            }
            _available.Enqueue(go);
        }

        /// <summary>
        /// Releases every currently active instance back to the pool.
        /// </summary>
        public void ReleaseAll()
        {
            // Copy because Release mutates the active set.
            var snapshot = new List<GameObject>(_active);
            foreach (GameObject go in snapshot)
            {
                Release(go);
            }
        }

        private GameObject CreateInstance()
        {
            GameObject go = Object.Instantiate(_prefab, _parent);
            go.name = _prefab.name;
            return go;
        }
    }
}

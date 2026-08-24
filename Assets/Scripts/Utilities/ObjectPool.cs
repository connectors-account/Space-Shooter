using System.Collections.Generic;
using UnityEngine;

namespace SpaceShooter.Utilities
{
    /// <summary>
    /// Generic object pool for any MonoBehaviour prefab.
    /// Pooled objects are parented under a container transform and toggled active/inactive.
    /// The pool auto-expands when empty.
    /// </summary>
    public class ObjectPool<T> where T : MonoBehaviour
    {
        private readonly T _prefab;
        private readonly Transform _parent;
        private readonly Queue<T> _available = new Queue<T>();
        private readonly HashSet<T> _inUse = new HashSet<T>();

        public int CountAvailable => _available.Count;
        public int CountInUse => _inUse.Count;
        public int CountTotal => CountAvailable + CountInUse;

        public ObjectPool(T prefab, Transform parent, int initialSize)
        {
            if (prefab == null)
            {
                Debug.LogError("ObjectPool created with a null prefab.");
            }

            _prefab = prefab;
            _parent = parent;

            for (int i = 0; i < Mathf.Max(0, initialSize); i++)
            {
                CreateNewInstance();
            }
        }

        private T CreateNewInstance()
        {
            T instance = Object.Instantiate(_prefab, _parent);
            instance.gameObject.SetActive(false);
            _available.Enqueue(instance);
            return instance;
        }

        /// <summary>
        /// Retrieves an inactive object from the pool (expanding if necessary),
        /// activates it and returns it.
        /// </summary>
        public T Get()
        {
            if (_available.Count == 0)
            {
                CreateNewInstance();
            }

            T instance = _available.Dequeue();

            // Guard against destroyed objects (e.g. scene reload).
            if (instance == null)
            {
                instance = Object.Instantiate(_prefab, _parent);
            }

            instance.gameObject.SetActive(true);
            _inUse.Add(instance);
            return instance;
        }

        /// <summary>
        /// Retrieves an object and positions it in one step.
        /// </summary>
        public T Get(Vector3 position, Quaternion rotation)
        {
            T instance = Get();
            instance.transform.SetPositionAndRotation(position, rotation);
            return instance;
        }

        /// <summary>
        /// Deactivates the object and returns it to the pool for reuse.
        /// </summary>
        public void Return(T obj)
        {
            if (obj == null)
            {
                return;
            }

            if (_inUse.Contains(obj))
            {
                _inUse.Remove(obj);
            }

            obj.gameObject.SetActive(false);

            if (!_available.Contains(obj))
            {
                _available.Enqueue(obj);
            }
        }

        /// <summary>
        /// Returns every currently active object to the pool.
        /// </summary>
        public void ReturnAll()
        {
            var copy = new List<T>(_inUse);
            foreach (var obj in copy)
            {
                Return(obj);
            }
        }
    }
}

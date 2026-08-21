using System.Collections.Generic;
using UnityEngine;

namespace SpaceShooter.Utilities
{
    /// <summary>
    /// Generic reusable object pool for MonoBehaviour components.
    /// Expands on demand when no inactive object is available.
    /// </summary>
    /// <typeparam name="T">Component type held by the pooled prefab.</typeparam>
    public class ObjectPool<T> where T : Component
    {
        private readonly T _prefab;
        private readonly Transform _parent;
        private readonly Queue<T> _inactive = new Queue<T>();
        private readonly List<T> _all = new List<T>();

        public int CountInactive => _inactive.Count;
        public int CountTotal => _all.Count;

        public ObjectPool(T prefab, int initialSize, Transform parent = null)
        {
            _prefab = prefab;
            _parent = parent;
            Prewarm(initialSize);
        }

        private void Prewarm(int size)
        {
            for (int i = 0; i < size; i++)
            {
                T obj = CreateNew();
                obj.gameObject.SetActive(false);
                _inactive.Enqueue(obj);
            }
        }

        private T CreateNew()
        {
            T obj = Object.Instantiate(_prefab, _parent);
            _all.Add(obj);
            return obj;
        }

        /// <summary>Retrieves an object from the pool, expanding if necessary.</summary>
        public T Get()
        {
            T obj = _inactive.Count > 0 ? _inactive.Dequeue() : CreateNew();
            obj.gameObject.SetActive(true);
            return obj;
        }

        public T Get(Vector3 position, Quaternion rotation)
        {
            T obj = Get();
            obj.transform.SetPositionAndRotation(position, rotation);
            return obj;
        }

        /// <summary>Returns an object to the pool for reuse.</summary>
        public void Return(T obj)
        {
            if (obj == null) return;
            obj.gameObject.SetActive(false);
            _inactive.Enqueue(obj);
        }

        /// <summary>Deactivates and returns all currently active objects.</summary>
        public void ReturnAll()
        {
            foreach (T obj in _all)
            {
                if (obj != null && obj.gameObject.activeSelf)
                {
                    Return(obj);
                }
            }
        }
    }
}

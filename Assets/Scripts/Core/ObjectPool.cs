using System;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceShooter.Core
{
    /// <summary>
    /// Generic, allocation-free object pool backed by a Stack.
    /// Use for plain C# objects or as a helper inside MonoBehaviour pools.
    /// </summary>
    /// <typeparam name="T">Type of object to pool.</typeparam>
    public class ObjectPool<T>
    {
        #region Fields
        private readonly Stack<T> _stack = new Stack<T>();
        private readonly Func<T> _factory;
        private readonly Action<T> _onGet;
        private readonly Action<T> _onReturn;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a pool.
        /// </summary>
        /// <param name="factory">Creates a new instance when the pool is empty.</param>
        /// <param name="onGet">Optional callback invoked when an item is fetched.</param>
        /// <param name="onReturn">Optional callback invoked when an item is returned.</param>
        public ObjectPool(Func<T> factory, Action<T> onGet = null, Action<T> onReturn = null)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _onGet = onGet;
            _onReturn = onReturn;
        }
        #endregion

        #region Public API
        /// <summary>Number of items currently available in the pool.</summary>
        public int CountInactive => _stack.Count;

        /// <summary>Pre-creates the requested number of items so first use has no spikes.</summary>
        public void Prewarm(int count)
        {
            for (int i = 0; i < count; i++)
            {
                T item = _factory();
                _stack.Push(item);
            }
        }

        /// <summary>Fetches an item from the pool, or creates one if empty.</summary>
        public T Get()
        {
            T item = _stack.Count > 0 ? _stack.Pop() : _factory();
            _onGet?.Invoke(item);
            return item;
        }

        /// <summary>Returns an item to the pool for reuse.</summary>
        public void Return(T obj)
        {
            if (obj == null) return;
            _onReturn?.Invoke(obj);
            _stack.Push(obj);
        }

        /// <summary>Empties the pool.</summary>
        public void Clear()
        {
            _stack.Clear();
        }
        #endregion
    }

    /// <summary>
    /// MonoBehaviour-based pool that recycles GameObject prefabs using SetActive
    /// instead of Instantiate/Destroy. Parents pooled objects under this transform.
    /// </summary>
    public class GameObjectPool : MonoBehaviour
    {
        #region Fields
        [SerializeField] private GameObject _prefab;
        [SerializeField] private int _prewarmCount = 0;

        private readonly Stack<GameObject> _inactive = new Stack<GameObject>();
        private Transform _root;
        #endregion

        #region Initialization
        /// <summary>Runtime factory-style initializer (used when created from code).</summary>
        public void Initialize(GameObject prefab, int prewarm = 0, Transform parent = null)
        {
            _prefab = prefab;
            _prewarmCount = prewarm;
            _root = parent != null ? parent : transform;
            Prewarm(_prewarmCount);
        }

        private void Awake()
        {
            if (_root == null) _root = transform;
        }

        private void Start()
        {
            if (_prefab != null && _inactive.Count == 0 && _prewarmCount > 0)
            {
                Prewarm(_prewarmCount);
            }
        }
        #endregion

        #region Public API
        /// <summary>Pre-instantiates the given number of objects (disabled).</summary>
        public void Prewarm(int count)
        {
            for (int i = 0; i < count; i++)
            {
                GameObject go = CreateNew();
                go.SetActive(false);
                _inactive.Push(go);
            }
        }

        /// <summary>Fetches an active GameObject positioned/rotated as requested.</summary>
        public GameObject Get(Vector3 position, Quaternion rotation)
        {
            GameObject go = _inactive.Count > 0 ? _inactive.Pop() : CreateNew();
            Transform t = go.transform;
            t.SetPositionAndRotation(position, rotation);
            go.SetActive(true);
            return go;
        }

        /// <summary>Fetches an active GameObject with default rotation.</summary>
        public GameObject Get(Vector3 position)
        {
            return Get(position, Quaternion.identity);
        }

        /// <summary>Disables the object and returns it to the pool.</summary>
        public void Return(GameObject go)
        {
            if (go == null) return;
            go.SetActive(false);
            go.transform.SetParent(_root, false);
            _inactive.Push(go);
        }
        #endregion

        #region Internal
        private GameObject CreateNew()
        {
            if (_prefab == null)
            {
                Debug.LogError("[GameObjectPool] Prefab is null; cannot create object.");
                return new GameObject("EmptyPooledObject");
            }
            GameObject go = Instantiate(_prefab, _root);
            return go;
        }
        #endregion
    }
}

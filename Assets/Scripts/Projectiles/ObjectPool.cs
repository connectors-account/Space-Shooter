using System.Collections.Generic;
using UnityEngine;

namespace SpaceShooter.Projectiles
{
    /// <summary>
    /// A simple GameObject pool. Pre-warms a number of instances of a prefab and hands them
    /// out via Get(); expands automatically when exhausted. Used for bullets to avoid GC churn.
    /// </summary>
    public enum PoolRole { None, PlayerBullets, EnemyBullets }

    public class ObjectPool : MonoBehaviour
    {
        public GameObject prefab;
        public int prewarmCount = 30;

        [Tooltip("If set, this pool auto-registers itself with BulletPattern on Start.")]
        public PoolRole role = PoolRole.None;

        private readonly Queue<GameObject> _available = new Queue<GameObject>();
        private readonly HashSet<GameObject> _all = new HashSet<GameObject>();

        private void Start()
        {
            Prewarm();
            if (role == PoolRole.PlayerBullets) BulletPattern.RegisterPlayerPool(this);
            else if (role == PoolRole.EnemyBullets) BulletPattern.RegisterEnemyPool(this);
        }

        public void Configure(GameObject prefabToPool, int count)
        {
            prefab = prefabToPool;
            prewarmCount = count;
            Prewarm();
        }

        private void Prewarm()
        {
            if (prefab == null) return;
            while (_all.Count < prewarmCount)
            {
                CreateInstance();
            }
        }

        private GameObject CreateInstance()
        {
            var go = Instantiate(prefab, transform);
            go.SetActive(false);
            _available.Enqueue(go);
            _all.Add(go);
            return go;
        }

        /// <summary>Fetch an inactive instance, creating one if the pool is empty.</summary>
        public GameObject Get()
        {
            if (prefab == null) return null;
            GameObject go;
            if (_available.Count > 0)
            {
                go = _available.Dequeue();
            }
            else
            {
                go = CreateInstance();
                _available.Dequeue(); // remove the one we just enqueued
            }
            go.SetActive(true);
            return go;
        }

        /// <summary>Return an instance to the pool for reuse.</summary>
        public void Return(GameObject go)
        {
            if (go == null) return;
            go.SetActive(false);
            go.transform.SetParent(transform);
            if (!_available.Contains(go)) _available.Enqueue(go);
        }
    }
}

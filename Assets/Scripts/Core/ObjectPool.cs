// ============================================================================
// ObjectPool.cs - Generic object pooling system
// Reduces garbage collection by reusing GameObjects instead of Instantiate/Destroy.
// ============================================================================
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// A simple, reusable object pool for any prefab type (bullets, enemies, effects).
/// Call Get() to retrieve an object and Return() to send it back to the pool.
/// </summary>
public class ObjectPool : MonoBehaviour
{
    [Header("Pool Configuration")]
    [Tooltip("The prefab to pool.")]
    [SerializeField] private GameObject prefab;
    [Tooltip("How many objects to pre-instantiate at start.")]
    [SerializeField] private int initialSize = 20;
    [Tooltip("If true, the pool grows automatically when exhausted.")]
    [SerializeField] private bool canGrow = true;

    private Queue<GameObject> pool = new Queue<GameObject>();
    private Transform poolParent;

    // ========================================================================
    // Initialization
    // ========================================================================

    private void Awake()
    {
        poolParent = new GameObject($"Pool_{(prefab != null ? prefab.name : "Unknown")}").transform;
        poolParent.SetParent(transform);
        Prewarm();
    }

    /// <summary>Pre-instantiates the initial pool objects.</summary>
    private void Prewarm()
    {
        if (prefab == null)
        {
            Debug.LogError("[ObjectPool] No prefab assigned!");
            return;
        }

        for (int i = 0; i < initialSize; i++)
        {
            GameObject obj = CreateNewInstance();
            obj.SetActive(false);
            pool.Enqueue(obj);
        }
    }

    // ========================================================================
    // Public API
    // ========================================================================

    /// <summary>
    /// Retrieves an inactive object from the pool, activates it, and returns it.
    /// If the pool is empty and canGrow is true, a new object is created.
    /// </summary>
    /// <param name="position">World position to place the object.</param>
    /// <param name="rotation">World rotation for the object.</param>
    /// <returns>An active GameObject ready for use, or null if pool is exhausted.</returns>
    public GameObject Get(Vector3 position, Quaternion rotation)
    {
        GameObject obj = null;

        // Try to find an inactive pooled object.
        while (pool.Count > 0)
        {
            obj = pool.Dequeue();
            if (obj != null) break;
        }

        // Grow the pool if necessary.
        if (obj == null)
        {
            if (!canGrow)
            {
                Debug.LogWarning($"[ObjectPool] Pool for '{prefab.name}' exhausted and cannot grow.");
                return null;
            }
            obj = CreateNewInstance();
        }

        obj.transform.position = position;
        obj.transform.rotation = rotation;
        obj.SetActive(true);
        return obj;
    }

    /// <summary>
    /// Returns an object to the pool and deactivates it.
    /// </summary>
    /// <param name="obj">The GameObject to return.</param>
    public void Return(GameObject obj)
    {
        if (obj == null) return;
        obj.SetActive(false);
        pool.Enqueue(obj);
    }

    /// <summary>
    /// Convenience overload: Get an object at a position with default rotation.
    /// </summary>
    public GameObject Get(Vector3 position)
    {
        return Get(position, Quaternion.identity);
    }

    // ========================================================================
    // Internals
    // ========================================================================

    private GameObject CreateNewInstance()
    {
        GameObject obj = Instantiate(prefab, poolParent);
        obj.name = prefab.name;
        return obj;
    }
}

// ============================================================================
// PoolManager.cs - Central registry for all object pools
// Provides a single access point to Get/Return pooled objects by prefab name.
// ============================================================================
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Singleton that holds references to every ObjectPool in the scene.
/// Other scripts request pooled objects through PoolManager instead of
/// managing individual pool references.
/// </summary>
public class PoolManager : MonoBehaviour
{
    public static PoolManager Instance { get; private set; }

    [Header("Pool Definitions")]
    [Tooltip("Drag all ObjectPool components here (bullets, enemies, effects, etc.).")]
    [SerializeField] private ObjectPool[] pools;

    private Dictionary<string, ObjectPool> poolLookup = new Dictionary<string, ObjectPool>();

    // ========================================================================
    // Unity Lifecycle
    // ========================================================================

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Build a fast lookup by prefab name.
        if (pools != null)
        {
            foreach (ObjectPool p in pools)
            {
                if (p == null) continue;
                // Use the pool's GameObject name if the prefab field is not directly accessible.
                string key = p.gameObject.name;
                if (!poolLookup.ContainsKey(key))
                {
                    poolLookup[key] = p;
                }
            }
        }
    }

    // ========================================================================
    // Public API
    // ========================================================================

    /// <summary>
    /// Retrieves a pooled object by pool name (the name of the ObjectPool's GameObject).
    /// </summary>
    public GameObject Get(string poolName, Vector3 position, Quaternion rotation)
    {
        if (poolLookup.TryGetValue(poolName, out ObjectPool pool))
        {
            return pool.Get(position, rotation);
        }
        Debug.LogWarning($"[PoolManager] No pool found with name '{poolName}'.");
        return null;
    }

    /// <summary>
    /// Returns an object to the appropriate pool by pool name.
    /// </summary>
    public void Return(string poolName, GameObject obj)
    {
        if (poolLookup.TryGetValue(poolName, out ObjectPool pool))
        {
            pool.Return(obj);
        }
        else
        {
            // Fallback: just deactivate the object.
            obj.SetActive(false);
        }
    }

    /// <summary>
    /// Registers a new pool at runtime (useful for dynamically created pools).
    /// </summary>
    public void RegisterPool(string poolName, ObjectPool pool)
    {
        if (!poolLookup.ContainsKey(poolName))
        {
            poolLookup[poolName] = pool;
        }
    }
}

// ============================================================================
// ObjectPooler.cs — Generic object pooling for bullets and effects
// ============================================================================
using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class PoolEntry
{
    public string tag;
    public GameObject prefab;
    public int size = 20;
    public bool expandable = true;
}

public class ObjectPooler : MonoBehaviour
{
    public static ObjectPooler Instance { get; private set; }

    [SerializeField] private List<PoolEntry> pools;

    private Dictionary<string, Queue<GameObject>> poolDict;
    private Dictionary<string, PoolEntry> entryDict;

    // =========================================================================
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        poolDict = new Dictionary<string, Queue<GameObject>>();
        entryDict = new Dictionary<string, PoolEntry>();

        foreach (var entry in pools)
        {
            Queue<GameObject> queue = new Queue<GameObject>();
            for (int i = 0; i < entry.size; i++)
            {
                GameObject obj = Instantiate(entry.prefab, transform);
                obj.SetActive(false);
                queue.Enqueue(obj);
            }
            poolDict[entry.tag] = queue;
            entryDict[entry.tag] = entry;
        }
    }

    // =========================================================================
    public GameObject Spawn(string tag, Vector3 position, Quaternion rotation)
    {
        if (!poolDict.ContainsKey(tag))
        {
            Debug.LogWarning($"[ObjectPooler] Pool '{tag}' doesn't exist.");
            return null;
        }

        Queue<GameObject> queue = poolDict[tag];
        GameObject obj;

        if (queue.Count == 0)
        {
            if (entryDict[tag].expandable)
            {
                obj = Instantiate(entryDict[tag].prefab, transform);
            }
            else
            {
                return null;
            }
        }
        else
        {
            obj = queue.Dequeue();
        }

        obj.transform.position = position;
        obj.transform.rotation = rotation;
        obj.SetActive(true);

        return obj;
    }

    public void ReturnToPool(string tag, GameObject obj)
    {
        if (!poolDict.ContainsKey(tag))
        {
            Destroy(obj);
            return;
        }

        obj.SetActive(false);
        poolDict[tag].Enqueue(obj);
    }
}

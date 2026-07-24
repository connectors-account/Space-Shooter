// ============================================================
//  ObjectPool.cs  –  Generic tag-based object pool
//  Attach to a "Pool Manager" GameObject in the Game scene.
// ============================================================
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    public static ObjectPool Instance { get; private set; }

    [System.Serializable]
    public class Pool
    {
        public string     tag;
        public GameObject prefab;
        public int        initialSize = 20;
    }

    [Header("Pool Definitions")]
    public List<Pool> pools = new();

    readonly Dictionary<string, Queue<GameObject>> _dict  = new();
    readonly Dictionary<string, Pool>              _defs  = new();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        foreach (var p in pools)
        {
            _defs[p.tag] = p;
            var q = new Queue<GameObject>();
            for (int i = 0; i < p.initialSize; i++)
                q.Enqueue(CreateNew(p));
            _dict[p.tag] = q;
        }
    }

    // ── Public API ───────────────────────────────────────────

    public GameObject Spawn(string tag, Vector3 pos, Quaternion rot)
    {
        if (!_dict.TryGetValue(tag, out var q)) return null;

        GameObject go = q.Count > 0 ? q.Dequeue() : CreateNew(_defs[tag]);
        go.transform.SetPositionAndRotation(pos, rot);
        go.SetActive(true);
        return go;
    }

    public void Despawn(string tag, GameObject go)
    {
        go.SetActive(false);
        if (_dict.TryGetValue(tag, out var q))
            q.Enqueue(go);
    }

    // ── Private ──────────────────────────────────────────────

    GameObject CreateNew(Pool p)
    {
        var go = Instantiate(p.prefab, transform);
        go.SetActive(false);
        return go;
    }
}

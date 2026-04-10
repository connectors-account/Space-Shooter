using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    [SerializeField] private GameObject prefab;
    [SerializeField] private int initialSize = 20;
    [SerializeField] private bool canExpand = true;

    private readonly Queue<GameObject> _pool = new Queue<GameObject>();

    public GameObject Prefab => prefab;

    private void Awake()
    {
        if (prefab == null)
        {
            Debug.LogError($"[{name}] ObjectPool prefab is not set.");
            return;
        }

        for (int i = 0; i < initialSize; i++)
        {
            CreateAndEnqueue();
        }
    }

    private GameObject CreateAndEnqueue()
    {
        GameObject go = Instantiate(prefab, transform);
        go.SetActive(false);
        _pool.Enqueue(go);
        return go;
    }

    public GameObject Get()
    {
        if (_pool.Count == 0)
        {
            if (!canExpand)
            {
                return null;
            }

            CreateAndEnqueue();
        }

        GameObject go = _pool.Dequeue();
        go.SetActive(true);
        return go;
    }

    public T Get<T>() where T : Component
    {
        GameObject go = Get();
        return go != null ? go.GetComponent<T>() : null;
    }

    public void Return(GameObject go)
    {
        if (go == null)
        {
            return;
        }

        go.SetActive(false);
        go.transform.SetParent(transform);
        _pool.Enqueue(go);
    }
}

using UnityEngine;

/// <summary>
/// Static helper that spawns power-ups. Reads prefabs from Resources folder.
/// Also can periodically spawn power-ups during gameplay.
/// </summary>
public class PowerUpSpawner : MonoBehaviour
{
    public GameObject shieldPrefab;
    public GameObject rapidFirePrefab;
    public GameObject healthPrefab;

    public float spawnInterval = 15f;
    public float spawnXRange = 4f;
    public float spawnY = 6f;

    private static PowerUpSpawner _instance;
    private float timer;

    void Awake()
    {
        _instance = this;
    }

    void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameManager.GameState.Playing)
            return;

        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            timer = 0f;
            SpawnRandom();
        }
    }

    void SpawnRandom()
    {
        float x = Random.Range(-spawnXRange, spawnXRange);
        Vector3 pos = new Vector3(x, spawnY, 0f);
        SpawnRandomAt(pos);
    }

    /// <summary>Spawn a random power-up at the given world position.</summary>
    public static void SpawnRandomAt(Vector3 position)
    {
        if (_instance == null) return;

        GameObject[] prefabs = new GameObject[]
        {
            _instance.shieldPrefab,
            _instance.rapidFirePrefab,
            _instance.healthPrefab
        };

        // Filter nulls
        var valid = new System.Collections.Generic.List<GameObject>();
        foreach (var p in prefabs)
            if (p != null) valid.Add(p);

        if (valid.Count == 0) return;

        GameObject chosen = valid[Random.Range(0, valid.Count)];
        Instantiate(chosen, position, Quaternion.identity);
    }
}

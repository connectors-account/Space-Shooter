// ============================================================
//  PowerUpSpawner.cs  –  Drops a random power-up at a position
//  Called by EnemyBase on death.
// ============================================================
using UnityEngine;

public class PowerUpSpawner : MonoBehaviour
{
    public static PowerUpSpawner Instance { get; private set; }

    [Header("Prefabs (one per type)")]
    public GameObject rapidFirePrefab;
    public GameObject tripleShotPrefab;
    public GameObject shieldPrefab;
    public GameObject healPrefab;
    public GameObject speedBoostPrefab;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    /// <summary>Spawn a random power-up at world position.</summary>
    public void DropAt(Vector3 position)
    {
        GameObject[] prefabs =
        {
            rapidFirePrefab, tripleShotPrefab, shieldPrefab,
            healPrefab, speedBoostPrefab
        };

        // Pick a non-null prefab
        int tries = 0;
        GameObject chosen = null;
        while (chosen == null && tries < 10)
        {
            chosen = prefabs[Random.Range(0, prefabs.Length)];
            tries++;
        }

        if (chosen == null)
        {
            // Fallback: use any available prefab
            foreach (var p in prefabs)
                if (p != null) { chosen = p; break; }
        }

        if (chosen == null) return;
        Instantiate(chosen, position, Quaternion.identity);
    }
}

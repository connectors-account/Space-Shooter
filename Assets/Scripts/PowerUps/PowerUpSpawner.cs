using UnityEngine;

/// <summary>
/// Singleton that spawns power-ups from the object pool.
/// Called by enemies on death to drop power-ups.
/// </summary>
public class PowerUpSpawner : MonoBehaviour
{
    public static PowerUpSpawner Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    /// <summary>
    /// Spawn a random power-up at the given position.
    /// </summary>
    public void SpawnRandomPowerUp(Vector2 position)
    {
        if (ObjectPool.Instance == null) return;

        GameObject obj = ObjectPool.Instance.Spawn(Tags.PowerUp, position, Quaternion.identity);
        if (obj != null)
        {
            PowerUp pu = obj.GetComponent<PowerUp>();
            if (pu != null)
            {
                float rand = Random.value;
                if (rand < 0.4f)
                    pu.SetType(PowerUp.PowerUpType.WeaponUpgrade);
                else if (rand < 0.7f)
                    pu.SetType(PowerUp.PowerUpType.Health);
                else
                    pu.SetType(PowerUp.PowerUpType.Shield);
            }
        }
    }
}

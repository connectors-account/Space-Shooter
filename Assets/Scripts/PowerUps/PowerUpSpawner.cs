using UnityEngine;

/// <summary>
/// Handles spawning power-ups at given positions when enemies are destroyed.
/// </summary>
public class PowerUpSpawner : MonoBehaviour
{
    public string powerUpPoolTag = "PowerUp";

    /// <summary>
    /// Spawn a random power-up at the given world position.
    /// </summary>
    public void SpawnRandomPowerUp(Vector3 position)
    {
        if (ObjectPool.Instance == null) return;

        GameObject powerUpObj = ObjectPool.Instance.Spawn(powerUpPoolTag, position, Quaternion.identity);
        if (powerUpObj != null)
        {
            PowerUp powerUp = powerUpObj.GetComponent<PowerUp>();
            if (powerUp != null)
            {
                PowerUp.PowerUpType randomType = (PowerUp.PowerUpType)Random.Range(0, 4);
                powerUp.SetType(randomType);
            }
        }
    }
}

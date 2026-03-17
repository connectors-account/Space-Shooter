using UnityEngine;

/// <summary>
/// Utility to clean up stray bullets when game restarts or ends.
/// Attach alongside GameManager.
/// </summary>
public class BulletCleaner : MonoBehaviour
{
    /// <summary>
    /// Destroys all bullets currently in the scene.
    /// Called on game restart or scene transitions.
    /// </summary>
    public static void ClearAllBullets()
    {
        Bullet[] bullets = FindObjectsOfType<Bullet>();
        foreach (Bullet b in bullets)
        {
            Destroy(b.gameObject);
        }
    }

    /// <summary>
    /// Destroys all power-ups currently in the scene.
    /// </summary>
    public static void ClearAllPowerUps()
    {
        PowerUp[] powerUps = FindObjectsOfType<PowerUp>();
        foreach (PowerUp p in powerUps)
        {
            Destroy(p.gameObject);
        }
    }
}

// ============================================================
//  BoundaryDestroyer.cs  –  Destroys objects that leave screen
//  Attach to enemies, power-ups, or any object that should
//  auto-cleanup when it scrolls off-screen.
// ============================================================
using UnityEngine;

public class BoundaryDestroyer : MonoBehaviour
{
    [Header("Notify WaveManager on death?")]
    public bool notifyWaveManager = true;

    void OnBecameInvisible()
    {
        // Off-screen – clean up
        if (notifyWaveManager)
            WaveManager.Instance?.EnemyKilled();

        Destroy(gameObject);
    }
}

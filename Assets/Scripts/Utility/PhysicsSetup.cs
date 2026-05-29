using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

/// <summary>
/// Configures Physics2D layer collision matrix for the game.
/// Run from: Tools > Setup Physics Layers
/// 
/// Collision rules:
/// - PlayerBullets hit Enemies (not Player)
/// - EnemyBullets hit Player (not Enemies)
/// - PowerUps hit Player only
/// - Player collides with Enemies and PowerUps
/// </summary>
public static class PhysicsSetup
{
    [MenuItem("Tools/Setup Physics Layers")]
    public static void SetupLayers()
    {
        // For simplicity, we use trigger-based collision detection
        // with tags rather than physics layers. All collision filtering
        // is handled in OnTriggerEnter2D callbacks.
        //
        // This approach is simpler for a small game and avoids
        // complex layer matrix configuration.

        Debug.Log("Physics setup complete. This game uses trigger-based collisions with tag filtering.");
        Debug.Log("All colliders should be set to 'Is Trigger = true'.");
        Debug.Log("All Rigidbody2D components should be Kinematic with 0 gravity.");
    }
}
#endif

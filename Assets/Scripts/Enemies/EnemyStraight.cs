// ============================================================================
// EnemyStraight.cs - Basic enemy that flies straight down
// The simplest enemy type; appears in early waves.
// ============================================================================
using UnityEngine;

/// <summary>
/// Flies straight down at a constant speed. The bread-and-butter enemy type.
/// </summary>
public class EnemyStraight : EnemyBase
{
    protected override void MovementPattern()
    {
        // Simply move downward at a constant speed.
        transform.Translate(Vector3.down * moveSpeed * Time.deltaTime, Space.World);
    }
}

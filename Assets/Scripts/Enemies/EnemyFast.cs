// ============================================================================
// EnemyFast.cs — Fast zigzagging enemy
// ============================================================================
using UnityEngine;

public class EnemyFast : EnemyBase
{
    protected override void Awake()
    {
        base.Awake();
        maxHealth = 1;
        scoreValue = 150;
        moveSpeed = 4f;
        movementPattern = MovementPattern.SineWave;
    }
}

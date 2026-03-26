// ============================================================================
// EnemyBasic.cs — Simple enemy that flies down
// ============================================================================
using UnityEngine;

public class EnemyBasic : EnemyBase
{
    protected override void Awake()
    {
        base.Awake();
        maxHealth = 2;
        scoreValue = 100;
        moveSpeed = 2f;
        movementPattern = MovementPattern.StraightDown;
    }
}

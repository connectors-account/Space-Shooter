// ============================================================================
// EnemyShooter.cs — Enemy that hovers and fires aimed shots
// ============================================================================
using UnityEngine;

public class EnemyShooter : EnemyBase
{
    protected override void Awake()
    {
        base.Awake();
        maxHealth = 4;
        scoreValue = 200;
        moveSpeed = 1.5f;
        movementPattern = MovementPattern.Hovering;
    }
}

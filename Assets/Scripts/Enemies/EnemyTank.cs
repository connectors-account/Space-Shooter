// ============================================================================
// EnemyTank.cs — Slow, high-HP enemy that hovers and shoots
// ============================================================================
using UnityEngine;

public class EnemyTank : EnemyBase
{
    protected override void Awake()
    {
        base.Awake();
        maxHealth = 8;
        scoreValue = 300;
        moveSpeed = 1f;
        movementPattern = MovementPattern.Hovering;
    }
}

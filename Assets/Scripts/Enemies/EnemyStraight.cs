using UnityEngine;

/// <summary>
/// Basic enemy that moves straight down. The simplest enemy type.
/// Spawns from the top and exits at the bottom.
/// </summary>
public class EnemyStraight : EnemyBase
{
    protected override void Move()
    {
        transform.Translate(Vector3.down * moveSpeed * Time.deltaTime);
    }
}

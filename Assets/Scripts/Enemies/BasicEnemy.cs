using UnityEngine;

/// <summary>
/// Basic enemy: moves straight down, occasional shooting.
/// </summary>
public class BasicEnemy : EnemyBase
{
    protected override void Start()
    {
        maxHealth = 1;
        moveSpeed = 3f;
        scoreValue = 100;
        fireRate = 3f;
        canShoot = true;
        dropChance = 0.1f;
        base.Start();
    }
}

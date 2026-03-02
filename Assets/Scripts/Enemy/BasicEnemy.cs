using UnityEngine;

/// <summary>
/// Basic enemy type that moves straight down.
/// Simple and predictable, good for early waves.
/// </summary>
public class BasicEnemy : EnemyBase
{
    protected override void Start()
    {
        base.Start();
        
        // Basic enemy settings
        health = 20;
        scoreValue = 100;
        moveSpeed = 3f;
        canShoot = false;
    }

    protected override void Move()
    {
        // Move straight down
        transform.position += Vector3.down * moveSpeed * Time.deltaTime;
    }
}

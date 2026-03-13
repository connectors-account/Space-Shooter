using UnityEngine;

/// <summary>
/// Basic enemy that moves straight down. Simple, numerous, low health.
/// </summary>
public class EnemyStraight : EnemyBase
{
    protected override void Start()
    {
        base.Start();
        maxHealth = 20;
        currentHealth = maxHealth;
        scoreValue = 50;
        moveSpeed = 3f;
        fireRate = 3f;
    }

    protected override void Move()
    {
        transform.Translate(Vector3.down * moveSpeed * Time.deltaTime, Space.World);
    }
}

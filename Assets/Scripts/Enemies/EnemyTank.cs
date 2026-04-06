using UnityEngine;

/// <summary>
/// Heavy enemy with more health, slower movement, and active shooting.
/// Moves down slowly while firing at the player.
/// </summary>
public class EnemyTank : EnemyBase
{
    private void Awake()
    {
        maxHealth = 5;
        scoreValue = 300;
        moveSpeed = 1.5f;
        canShoot = true;
        shootInterval = 1.5f;
        powerUpDropChance = 0.4f;
    }

    protected override void Move()
    {
        transform.Translate(Vector3.down * moveSpeed * Time.deltaTime);
    }
}

using UnityEngine;

namespace SpaceShooter.Enemies
{
    /// <summary>
    /// Tank enemy: slow, high health, shoots frequently, worth more points.
    /// </summary>
    public class TankEnemy : EnemyBase
    {
        protected override void Awake()
        {
            base.Awake();
            maxHealth = 5;
            moveSpeed = 1.5f;
            scoreValue = 300;
            canShoot = true;
            shootInterval = 1.5f;
            powerUpDropChance = 0.3f;
        }

        protected override void Move()
        {
            // Slow straight downward movement
            transform.Translate(Vector3.down * moveSpeed * Time.deltaTime);
        }
    }
}

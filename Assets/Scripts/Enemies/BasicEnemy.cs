using UnityEngine;

namespace SpaceShooter.Enemies
{
    /// <summary>
    /// Basic enemy that moves straight down. Simplest enemy type.
    /// </summary>
    public class BasicEnemy : EnemyBase
    {
        protected override void Awake()
        {
            base.Awake();
            maxHealth = 1;
            moveSpeed = 3f;
            scoreValue = 100;
            canShoot = true;
            shootInterval = 2.5f;
            powerUpDropChance = 0.1f;
        }

        protected override void Move()
        {
            // Straight downward movement
            transform.Translate(Vector3.down * moveSpeed * Time.deltaTime);
        }
    }
}

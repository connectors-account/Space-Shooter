using UnityEngine;

namespace SpaceShooter.Enemy
{
    public class EnemyTank : EnemyBase
    {
        protected override void Awake()
        {
            base.Awake();
            enemyType = EnemyType.Tank;
            maxHealth = 80;
            currentHealth = maxHealth;
            moveSpeed = 1.5f;
            scoreValue = 300;
            canShoot = true;
            fireRate = 1.5f;
            damage = 15;
            useWaveMovement = false;
        }
    }
}

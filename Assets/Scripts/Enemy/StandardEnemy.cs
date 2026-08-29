using UnityEngine;

namespace SpaceShooter.Enemy
{
    /// <summary>
    /// A concrete, general-purpose enemy that configures its mover and (optional) shooter
    /// from the assigned EnemyData. Use this component on standard enemy prefabs.
    /// </summary>
    public class StandardEnemy : EnemyBase
    {
        [Header("Behaviour")]
        [SerializeField] private MovementPattern movementPattern = MovementPattern.StraightDown;
        [SerializeField] private BulletPattern bulletPattern = BulletPattern.Single;

        public override void InitializeEnemy()
        {
            ApplyCommonData();

            if (mover != null)
            {
                float speed = data != null ? data.moveSpeed : 3f;
                mover.Initialize(movementPattern, speed);
            }

            if (shooter != null && data != null && data.shootInterval > 0f)
            {
                shooter.BeginFiring(bulletPattern, data.shootInterval, data.bulletDamage);
            }
            else if (shooter != null)
            {
                shooter.StopFiring();
            }
        }

        /// <summary>
        /// Allows the spawner to override the movement pattern per formation.
        /// </summary>
        public void SetMovementPattern(MovementPattern pattern)
        {
            movementPattern = pattern;
        }

        public void SetBulletPattern(BulletPattern pattern)
        {
            bulletPattern = pattern;
        }
    }
}

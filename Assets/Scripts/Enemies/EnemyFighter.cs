using UnityEngine;

namespace SpaceShooter
{
    /// <summary>
    /// Tougher enemy. 3 HP, worth 250 points, fires a bullet aimed at the player
    /// every 1.5 seconds. Moves in a zigzag pattern.
    /// </summary>
    public class EnemyFighter : EnemyBase
    {
        protected override void Awake()
        {
            shootInterval = 1.5f;
            base.Awake();

            if (Movement != null)
            {
                Movement.pattern = MovementPattern.Zigzag;
            }
        }

        public override void Shoot()
        {
            FireBullet(transform.position + Vector3.down * 0.5f, DirectionToPlayer());
        }

        public override int GetScoreValue() => 250;

        protected override int GetMaxHealth() => 3;
    }
}

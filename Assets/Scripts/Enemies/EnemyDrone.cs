using UnityEngine;

namespace SpaceShooter
{
    /// <summary>
    /// Basic enemy. 1 HP, worth 100 points, fires a single bullet straight down
    /// every 2 seconds. Moves straight down or in a gentle sine wave.
    /// </summary>
    public class EnemyDrone : EnemyBase
    {
        protected override void Awake()
        {
            shootInterval = 2f;
            base.Awake();

            if (Movement != null && Movement.pattern != MovementPattern.Sine)
            {
                Movement.pattern = MovementPattern.StraightDown;
            }
        }

        public override void Shoot()
        {
            FireBullet(transform.position + Vector3.down * 0.5f, Vector2.down);
        }

        public override int GetScoreValue() => 100;

        protected override int GetMaxHealth() => 1;
    }
}

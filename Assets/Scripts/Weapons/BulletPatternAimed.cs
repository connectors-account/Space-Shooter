using UnityEngine;
using SpaceShooter.Core;

namespace SpaceShooter.Weapons
{
    /// <summary>
    /// Fires bullets aimed directly at the player's current position. Optionally
    /// fires a small burst with a slight angular spread. Used by the bomber.
    /// </summary>
    public class BulletPatternAimed : BulletPattern
    {
        [SerializeField] private int burstCount = 1;
        [SerializeField] private float burstSpreadAngle = 10f;
        [SerializeField] private bool homing = false;

        public override void Fire(Vector2 origin, ObjectPool pool)
        {
            var player = ResolvePlayer();
            Vector2 toPlayer = player != null
                ? ((Vector2)player.position - origin).normalized
                : Vector2.down;

            int count = Mathf.Max(1, burstCount);
            if (count == 1)
            {
                SpawnBullet(pool, origin, toPlayer, homing);
                return;
            }

            float baseAngle = Mathf.Atan2(toPlayer.y, toPlayer.x) * Mathf.Rad2Deg;
            float start = baseAngle - burstSpreadAngle * 0.5f;
            float step = burstSpreadAngle / (count - 1);

            for (int i = 0; i < count; i++)
            {
                float ang = start + step * i;
                Vector2 dir = new Vector2(Mathf.Cos(ang * Mathf.Deg2Rad), Mathf.Sin(ang * Mathf.Deg2Rad));
                SpawnBullet(pool, origin, dir, homing);
            }
        }

        public void SetHoming(bool value) => homing = value;
    }
}

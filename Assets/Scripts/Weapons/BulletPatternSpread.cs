using UnityEngine;
using SpaceShooter.Core;

namespace SpaceShooter.Weapons
{
    /// <summary>
    /// Fires N bullets fanned out symmetrically around a central direction.
    /// Used by the enemy fighter (spread down) and, conceptually, the player's
    /// triple shot. Count is typically 3 or 5.
    /// </summary>
    public class BulletPatternSpread : BulletPattern
    {
        [SerializeField] private int bulletCount = 3;
        [SerializeField] private float totalSpreadAngle = 40f;
        [SerializeField] private Vector2 centreDirection = Vector2.down;

        public override void Fire(Vector2 origin, ObjectPool pool)
        {
            int count = Mathf.Max(1, bulletCount);
            float baseAngle = Mathf.Atan2(centreDirection.y, centreDirection.x) * Mathf.Rad2Deg;

            if (count == 1)
            {
                SpawnBullet(pool, origin, centreDirection.normalized);
                return;
            }

            float start = baseAngle - totalSpreadAngle * 0.5f;
            float step = totalSpreadAngle / (count - 1);

            for (int i = 0; i < count; i++)
            {
                float ang = start + step * i;
                Vector2 dir = new Vector2(Mathf.Cos(ang * Mathf.Deg2Rad), Mathf.Sin(ang * Mathf.Deg2Rad));
                SpawnBullet(pool, origin, dir);
            }
        }

        public void Configure(int count, float spreadAngle)
        {
            bulletCount = Mathf.Max(1, count);
            totalSpreadAngle = spreadAngle;
        }
    }
}

using UnityEngine;
using SpaceShooter.Core;

namespace SpaceShooter.Weapons
{
    /// <summary>
    /// Fires a ring of bullets whose starting angle rotates each call, creating
    /// a spinning spiral over successive volleys. Used by the boss's phase 1.
    /// </summary>
    public class BulletPatternSpiral : BulletPattern
    {
        [SerializeField] private int armsPerVolley = 4;
        [SerializeField] private float angleIncrement = 18f; // degrees added each call

        private float _currentAngle;

        public override void Fire(Vector2 origin, ObjectPool pool)
        {
            int arms = Mathf.Max(1, armsPerVolley);
            float step = 360f / arms;

            for (int i = 0; i < arms; i++)
            {
                float ang = _currentAngle + step * i;
                Vector2 dir = new Vector2(Mathf.Cos(ang * Mathf.Deg2Rad), Mathf.Sin(ang * Mathf.Deg2Rad));
                SpawnBullet(pool, origin, dir);
            }

            _currentAngle += angleIncrement;
            if (_currentAngle >= 360f) _currentAngle -= 360f;
        }

        public void Configure(int arms, float increment)
        {
            armsPerVolley = Mathf.Max(1, arms);
            angleIncrement = increment;
        }

        public void ResetSpiral() => _currentAngle = 0f;
    }
}

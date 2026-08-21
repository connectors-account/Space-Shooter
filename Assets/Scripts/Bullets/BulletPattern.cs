using UnityEngine;

namespace SpaceShooter.Bullets
{
    public enum PatternType
    {
        Single,
        Spread3,
        Spread5,
        Aimed,
        Spiral
    }

    /// <summary>
    /// Static helper that fires enemy bullet patterns from a source position.
    /// All patterns are fully implemented with vector math.
    /// </summary>
    public static class BulletPattern
    {
        /// <summary>
        /// Fires the given pattern of enemy bullets.
        /// </summary>
        /// <param name="type">Pattern to fire.</param>
        /// <param name="origin">World position to fire from.</param>
        /// <param name="baseDirection">Nominal firing direction (usually Vector2.down).</param>
        /// <param name="target">Target position (used by Aimed).</param>
        /// <param name="speed">Bullet speed.</param>
        /// <param name="damage">Bullet damage.</param>
        /// <param name="spiralAngle">Current spiral angle in degrees (for Spiral); increment between calls.</param>
        public static void Fire(PatternType type, Vector3 origin, Vector2 baseDirection,
            Vector3 target, float speed, int damage, float spiralAngle = 0f)
        {
            if (BulletPool.Instance == null) return;
            baseDirection = baseDirection.sqrMagnitude < 0.0001f ? Vector2.down : baseDirection.normalized;

            switch (type)
            {
                case PatternType.Single:
                    Spawn(origin, baseDirection, speed, damage);
                    break;

                case PatternType.Spread3:
                    FireArc(origin, baseDirection, 3, 20f, speed, damage);
                    break;

                case PatternType.Spread5:
                    FireArc(origin, baseDirection, 5, 20f, speed, damage);
                    break;

                case PatternType.Aimed:
                    Vector2 aimed = ((Vector2)(target - origin)).normalized;
                    if (aimed.sqrMagnitude < 0.0001f) aimed = baseDirection;
                    Spawn(origin, aimed, speed, damage);
                    break;

                case PatternType.Spiral:
                    float rad = spiralAngle * Mathf.Deg2Rad;
                    Vector2 spiralDir = new Vector2(Mathf.Sin(rad), -Mathf.Cos(rad)).normalized;
                    Spawn(origin, spiralDir, speed, damage);
                    // Fire an opposing arm for a fuller spiral.
                    Vector2 opposite = new Vector2(-spiralDir.x, spiralDir.y).normalized;
                    Spawn(origin, opposite, speed, damage);
                    break;
            }
        }

        private static void FireArc(Vector3 origin, Vector2 baseDirection, int count, float spreadDegrees,
            float speed, int damage)
        {
            if (count <= 1)
            {
                Spawn(origin, baseDirection, speed, damage);
                return;
            }

            float totalSpread = spreadDegrees * (count - 1);
            float startAngle = -totalSpread * 0.5f;
            float baseAngle = Mathf.Atan2(baseDirection.y, baseDirection.x) * Mathf.Rad2Deg;

            for (int i = 0; i < count; i++)
            {
                float angle = (baseAngle + startAngle + spreadDegrees * i) * Mathf.Deg2Rad;
                Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                Spawn(origin, dir, speed, damage);
            }
        }

        private static void Spawn(Vector3 origin, Vector2 direction, float speed, int damage)
        {
            BulletPool.Instance.GetEnemyBullet(origin, direction, speed, damage);
        }
    }
}

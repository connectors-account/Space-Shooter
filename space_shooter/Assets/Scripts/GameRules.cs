using System;

namespace SpaceShooter
{
    // Pure C# collision math, shared by the game and the standalone checks.
    public static class GameRules
    {
        public const float HalfWidth = 8f;
        public const float HalfHeight = 6f;
        public const float PlayerHalfWidth = 0.55f;
        public const float PlayerHalfHeight = 0.45f;
        public const float EnemyHalfSize = 0.45f;
        public const float BulletHalfWidth = 0.10f;
        public const float BulletHalfHeight = 0.25f;

        public static float Clamp(float value, float minimum, float maximum)
        {
            return Math.Max(minimum, Math.Min(maximum, value));
        }

        // Sweeps the relative movement of two axis-aligned boxes. This catches
        // fast bullets even when neither endpoint overlaps the moving enemy.
        public static bool SweptHit(float startX, float startY, float endX,
            float endY, float halfWidth, float halfHeight, out float hitTime)
        {
            float enter = 0f;
            float exit = 1f;
            bool hit = Slab(startX, endX - startX, halfWidth, ref enter, ref exit)
                && Slab(startY, endY - startY, halfHeight, ref enter, ref exit);
            hitTime = enter;
            return hit;
        }

        private static bool Slab(float origin, float movement, float extent,
            ref float enter, ref float exit)
        {
            if (Math.Abs(movement) < 0.000001f)
                return Math.Abs(origin) <= extent;
            float a = (-extent - origin) / movement;
            float b = (extent - origin) / movement;
            if (a > b) { float swap = a; a = b; b = swap; }
            enter = Math.Max(enter, a);
            exit = Math.Min(exit, b);
            return enter <= exit;
        }
    }
}

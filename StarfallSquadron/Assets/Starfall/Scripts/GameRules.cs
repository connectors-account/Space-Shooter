using System;

namespace Starfall
{
    // Engine-independent rules: exercised by Tools/RulesTests.cs on Mono.
    public static class GameRules
    {
        public const int WaveCount = 5;
        public const int MaxHealth = 5;
        public const float PlayerSpeed = 7f;
        public const float HalfWidth = 8.4f;
        public const float HalfHeight = 4.65f;
        public static int EnemyCount(int wave) { return wave == WaveCount ? 1 : 5 + wave * 3; }
        public static int EnemyHealth(int kind, int wave) { return kind == 3 ? 110 : 2 + kind * 2 + wave / 2; }
        public static int Points(int kind) { return kind == 3 ? 2000 : 100 + kind * 75; }
        public static int Heal(int health) { return Math.Min(MaxHealth, health + 2); }
        public static float Clamp(float value, float min, float max) { return Math.Max(min, Math.Min(max, value)); }

        // Segment against circle: fast projectiles cannot tunnel between frames.
        public static bool SegmentHit(float ax, float ay, float bx, float by, float cx, float cy, float radius)
        {
            float dx = bx - ax, dy = by - ay;
            float lengthSquared = dx * dx + dy * dy;
            float t = lengthSquared < 0.000001f ? 0 : Clamp(((cx - ax) * dx + (cy - ay) * dy) / lengthSquared, 0, 1);
            float ex = ax + t * dx - cx, ey = ay + t * dy - cy;
            return ex * ex + ey * ey <= radius * radius;
        }
    }
}

using System;

namespace StellarShooter
{
    // Pure rules, independent of Unity. Covered by Tools/RulesTests.cs.
    public static class GameRules
    {
        public const int MaxHealth = 5;
        public const float HalfWidth = 8.88f;
        public const float HalfHeight = 5f;
        public const float PlayerSpeed = 6f;
        public const float SpreadDuration = 12f;
        public const float ShieldDuration = 8f;
        public static int EnemyCount(int wave) => 5 + Math.Min(Math.Max(wave, 1), 9) * 2;
        public static int EnemyHealth(int wave, int kind) => 1 + Math.Min(Math.Max(wave - 1, 0) / 4, 2) + (kind == 2 ? 1 : 0);
        public static float EnemySpeed(int wave) => 1.15f + Math.Min(Math.Max(wave, 1), 12) * .07f;
        public static float FireInterval(int wave) => Math.Max(.8f, 2.2f - Math.Max(wave, 1) * .08f);
        public static int KillScore(int kind) => 100 + Math.Max(0, Math.Min(kind, 2)) * 50;
        public static int Heal(int health) => Math.Min(MaxHealth, health + 2);
        public static int PatternCount(int kind) => kind == 0 ? 1 : kind == 1 ? 3 : 8;
        public static float PatternAngle(int kind, int index) => kind == 0 ? 0 : kind == 1 ? (index - 1) * 18f : index * 45f;
    }
}

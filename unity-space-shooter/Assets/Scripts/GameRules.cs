using System;

namespace Starward
{
    // Pure C#: these rules are shared by gameplay and the offline tests.
    public static class GameRules
    {
        public const float Left = -9.5f, Right = 9.5f, Bottom = -5.1f, Top = 4.7f;
        public const int MaxHealth = 5, MaxWeapon = 3;
        public static int EnemyCount(int wave) { return 5 + Math.Min(25, Math.Max(1, wave) * 2); }
        public static float SpawnDelay(int wave) { return Math.Max(.35f, 1.15f - Math.Max(1, wave) * .05f); }
        public static float EnemySpeed(int wave) { return Math.Min(2.4f, .75f + Math.Max(1, wave) * .09f); }
        public static float FireDelay(int wave) { return Math.Max(.65f, 2f - Math.Max(1, wave) * .07f); }
        public static int EnemyHealth(int wave) { return 1 + Math.Min(3, Math.Max(1, wave) / 4); }
        public static int ShotCount(int weapon) { return Math.Max(1, Math.Min(MaxWeapon, weapon)); }
        public static float ShotAngle(int index, int count, float spread)
        {
            if (count < 1 || index < 0 || index >= count) throw new ArgumentOutOfRangeException();
            return (index - (count - 1) * .5f) * spread;
        }
        public static int Damage(int health, int amount) { return Math.Max(0, health - Math.Max(0, amount)); }
        public static int Heal(int health, int amount, int maximum) { return Math.Min(maximum, health + Math.Max(0, amount)); }
    }
}

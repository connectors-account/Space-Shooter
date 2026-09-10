using System;

namespace Starfall
{
    // Engine-independent rules, covered by the included validation harness.
    public static class GameRules
    {
        public const int MaxHealth = 5;
        public static int EnemiesInWave(int wave) { return Math.Min(26, 6 + Math.Max(1, wave) * 2); }
        public static int EnemyType(int wave, int index) { return wave < 2 ? 0 : wave < 4 ? index % 2 : index % 3; }
        public static int EnemyHealth(int wave, int type) { return 1 + type + Math.Min(3, Math.Max(0, wave - 1) / 5); }
        public static float FireInterval(bool rapid) { return rapid ? 0.09f : 0.2f; }
        public static int Heal(int health) { return Math.Min(MaxHealth, Math.Max(0, health) + 2); }
    }
}

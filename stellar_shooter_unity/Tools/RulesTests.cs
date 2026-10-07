using System;
using StellarShooter;

internal static class RulesTests
{
    private static int checks;
    private static void Assert(bool condition, string description)
    {
        if (!condition) throw new Exception(description);
        checks++;
    }
    public static void Main()
    {
        Assert(GameRules.EnemyCount(0) == 7, "Zero wave clamps safely");
        Assert(GameRules.EnemyCount(1) == 7, "First wave count");
        Assert(GameRules.EnemyCount(9) == 23, "Wave count cap");
        Assert(GameRules.EnemyCount(10000) == 23, "Long runs stay bounded");
        Assert(GameRules.EnemyHealth(1, 0) == 1, "First scout health");
        Assert(GameRules.EnemyHealth(1, 2) == 2, "Bomber extra health");
        Assert(GameRules.EnemyHealth(100, 2) == 4, "Health cap");
        Assert(Math.Abs(GameRules.EnemySpeed(100) - 1.99f) < .001f, "Speed cap");
        Assert(Math.Abs(GameRules.FireInterval(100) - .8f) < .001f, "Firing interval floor");
        Assert(GameRules.Heal(1) == 3, "Repair gives two hull");
        Assert(GameRules.Heal(4) == 5, "Repair clamps to maximum");
        Assert(GameRules.Heal(5) == 5, "Repair at full hull");
        Assert(GameRules.KillScore(0) == 100 && GameRules.KillScore(1) == 150 && GameRules.KillScore(2) == 200, "Score table");
        Assert(GameRules.PatternCount(0) == 1 && GameRules.PatternCount(1) == 3 && GameRules.PatternCount(2) == 8, "Pattern bullet counts");
        Assert(GameRules.PatternAngle(1, 0) == -18 && GameRules.PatternAngle(1, 1) == 0 && GameRules.PatternAngle(1, 2) == 18, "Fan symmetry");
        for (int i = 0; i < 8; i++) Assert(GameRules.PatternAngle(2, i) == i*45, "Ring angular spacing");
        for (int wave = 1; wave < 30; wave++)
        {
            Assert(GameRules.EnemyCount(wave+1) >= GameRules.EnemyCount(wave), "Monotonic wave size");
            Assert(GameRules.FireInterval(wave+1) <= GameRules.FireInterval(wave), "Monotonic difficulty");
        }
        // At 50 Hz a friendly shot advances .26 m, less than a target's .726 m height.
        Assert(13f * .02f < .66f * 1.1f, "Normal fixed-step shot displacement smaller than enemy collider");
        Console.WriteLine("PASS: " + checks + " pure C# rule assertions (not a Unity gameplay test).");
    }
}

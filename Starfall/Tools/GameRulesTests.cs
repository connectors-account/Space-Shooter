using System;
using Starfall;

internal static class GameRulesTests
{
    static int checks;
    static void Equal<T>(T expected, T actual, string name)
    {
        checks++;
        if (!Equals(expected, actual)) throw new Exception(name + ": expected " + expected + ", got " + actual);
    }
    static void Main()
    {
        Equal(8, GameRules.EnemiesInWave(1), "first wave");
        Equal(8, GameRules.EnemiesInWave(0), "zero wave clamped");
        Equal(8, GameRules.EnemiesInWave(-5), "negative wave clamped");
        Equal(10, GameRules.EnemiesInWave(2), "second wave");
        Equal(26, GameRules.EnemiesInWave(1000), "spawn cap");
        Equal(0, GameRules.EnemyType(1, 5), "scouts only initially");
        Equal(1, GameRules.EnemyType(2, 1), "fan unlock");
        Equal(0, GameRules.EnemyType(3, 2), "spinner still locked");
        Equal(2, GameRules.EnemyType(4, 2), "spinner unlock");
        Equal(1, GameRules.EnemyHealth(1, 0), "scout base health");
        Equal(3, GameRules.EnemyHealth(4, 2), "spinner base health");
        Equal(2, GameRules.EnemyHealth(6, 0), "health scaling");
        Equal(6, GameRules.EnemyHealth(1000, 2), "health cap");
        Equal(5, GameRules.Heal(5), "full hull pickup");
        Equal(5, GameRules.Heal(4), "repair capped");
        Equal(3, GameRules.Heal(1), "repair two points");
        Equal(2, GameRules.Heal(0), "zero health input");
        Equal(2, GameRules.Heal(-10), "negative health input");
        Equal(.2f, GameRules.FireInterval(false), "normal firing");
        Equal(.09f, GameRules.FireInterval(true), "rapid firing");
        for (int wave = 1; wave <= 100; wave++)
        {
            int count = GameRules.EnemiesInWave(wave);
            Equal(true, count >= 8 && count <= 26, "bounded wave " + wave);
            for (int index = 0; index < count; index++)
            {
                int type = GameRules.EnemyType(wave, index);
                Equal(true, type >= 0 && type <= 2, "valid pattern");
                Equal(true, GameRules.EnemyHealth(wave, type) >= 1 && GameRules.EnemyHealth(wave, type) <= 6, "bounded enemy health");
            }
        }
        Console.WriteLine("PASS: " + checks + " engine-independent rule assertions. Unity gameplay was not exercised.");
    }
}

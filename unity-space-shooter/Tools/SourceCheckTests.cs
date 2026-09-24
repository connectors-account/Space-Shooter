// Runs against source-check stubs only; NOT a Unity test runner or physics test.
using System;
using Starward;
using UnityEngine;

public static class SourceCheckTests
{
    static int checks;
    static void Check(bool condition, string description)
    {
        if (!condition) throw new Exception("FAIL: " + description);
        checks++;
    }
    public static void Main()
    {
        Check(GameRules.EnemyCount(1) == 7, "first wave count");
        Check(GameRules.EnemyCount(0) == 7, "invalid wave lower bound");
        Check(GameRules.EnemyCount(1000) == 30, "wave count cap");
        Check(GameRules.EnemyHealth(1) == 1, "initial enemy health");
        Check(GameRules.EnemyHealth(4) == 2, "armored enemies from wave 4");
        Check(GameRules.EnemyHealth(1000) == 4, "health cap");
        Check(GameRules.SpawnDelay(1000) >= .35f, "spawn delay floor");
        Check(GameRules.FireDelay(1000) >= .65f, "fire delay floor");
        Check(GameRules.EnemySpeed(1000) <= 2.4f, "speed cap");
        for (int wave = 2; wave <= 12; wave++)
        {
            Check(GameRules.EnemyCount(wave) >= GameRules.EnemyCount(wave - 1), "count progression " + wave);
            Check(GameRules.SpawnDelay(wave) <= GameRules.SpawnDelay(wave - 1), "spawn progression " + wave);
        }
        Check(GameRules.ShotCount(0) == 1 && GameRules.ShotCount(9) == 3, "weapon bounds");
        Check(GameRules.ShotAngle(0,1,18) == 0, "straight shot");
        Check(GameRules.ShotAngle(0,3,18) == -18, "left fan shot");
        Check(GameRules.ShotAngle(1,3,18) == 0, "center fan shot");
        Check(GameRules.ShotAngle(2,3,18) == 18, "right fan shot");
        Check(GameRules.ShotAngle(0,2,9) == -4.5f && GameRules.ShotAngle(1,2,9) == 4.5f, "double weapon spread");
        bool threw = false;
        try { GameRules.ShotAngle(3,3,18); } catch (ArgumentOutOfRangeException) { threw = true; }
        Check(threw, "invalid shot index rejected");
        Check(GameRules.Damage(2,9) == 0, "damage clamps to zero");
        Check(GameRules.Damage(2,-1) == 2, "negative damage ignored");
        Check(GameRules.Heal(4,2,5) == 5, "repair clamps at maximum");
        Check(GameRules.Heal(3,-1,5) == 3, "negative healing ignored");
        // Exercises actual Health method bodies, but with a fake clock / MonoBehaviour base.
        Health health = new Health();
        int deaths = 0;
        health.Died += () => deaths++;
        Time.time = 10;
        health.ResetHealth(5,2);
        Check(!health.Damage(1) && health.Current == 5, "spawn protection");
        Time.time = 12;
        Check(health.Damage(1,1.25f) && health.Current == 4, "damage after protection expires");
        Check(!health.Damage(1), "repeat hit invulnerability");
        Time.time = 13.25f;
        Check(health.Damage(1) && health.Current == 3, "exact protection boundary");
        health.Heal(2);
        Check(health.Current == 5, "repair restores health");
        Check(health.Damage(99) && health.Current == 0 && deaths == 1, "death notification");
        Check(!health.Damage(1) && deaths == 1, "no duplicate death event");
        health.Heal(5);
        Check(health.Current == 0, "dead actor not resurrected by pickup");
        health.ResetHealth(5);
        Check(health.Current == 5 && !health.Invulnerable, "restart health reset");
        Console.WriteLine("PASS: " + checks + " source-level rule/health assertions. No Unity engine or Windows runtime exercised.");
    }
}

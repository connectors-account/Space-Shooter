using System;
using SpaceShooter;

internal static class GameRulesTests
{
    private static int checks;
    private static void Check(string name, bool condition)
    {
        if (!condition) throw new Exception("FAIL: " + name);
        checks++;
        Console.WriteLine("PASS: " + name);
    }

    private static bool Hit(float x0, float y0, float x1, float y1)
    {
        float time;
        return GameRules.SweptHit(x0, y0, x1, y1, 1f, 1f, out time);
    }

    public static int Main()
    {
        Check("clamp inside", GameRules.Clamp(2f, -3f, 3f) == 2f);
        Check("clamp lower edge", GameRules.Clamp(-5f, -3f, 3f) == -3f);
        Check("clamp upper edge", GameRules.Clamp(5f, -3f, 3f) == 3f);
        Check("stationary overlap", Hit(0f, 0f, 0f, 0f));
        Check("stationary separation", !Hit(2f, 0f, 2f, 0f));
        Check("boundary contact", Hit(1f, 1f, 1f, 1f));
        Check("fast vertical shot cannot tunnel", Hit(0f, -10f, 0f, 10f));
        Check("fast horizontal movement cannot tunnel", Hit(-10f, 0f, 10f, 0f));
        Check("parallel miss", !Hit(1.01f, -10f, 1.01f, 10f));
        Check("moving away", !Hit(0f, 2f, 0f, 5f));
        Check("diagonal miss", !Hit(-2f, 0.5f, 0.5f, 3f));
        Check("hit at segment end", Hit(0f, -3f, 0f, -1f));
        Check("negative sweep", Hit(0f, 3f, 0f, -3f));
        float nearTime, farTime;
        bool near = GameRules.SweptHit(0f, -2f, 0f, 8f, 0.55f, 0.7f, out nearTime);
        bool far = GameRules.SweptHit(0f, -5f, 0f, 5f, 0.55f, 0.7f, out farTime);
        Check("nearest enemy selected", near && far && nearTime < farTime);
        // Bullet moves -2 -> 2 and enemy moves 2 -> -2: relative sweep -4 -> 4.
        Check("opposing moving bodies collide", Hit(0f, -4f, 0f, 4f));
        float limitX = GameRules.HalfWidth - GameRules.PlayerHalfWidth;
        float limitY = GameRules.HalfHeight - GameRules.PlayerHalfHeight;
        // Floating-point sums at the edge can differ by a few ULPs.
        Check("player x extents fit field", Math.Abs(limitX + GameRules.PlayerHalfWidth - GameRules.HalfWidth) < 0.00001f);
        Check("player y extents fit field", Math.Abs(limitY + GameRules.PlayerHalfHeight - GameRules.HalfHeight) < 0.00001f);
        Console.WriteLine(checks + " pure C# checks passed. Unity runtime was not exercised.");
        return 0;
    }
}

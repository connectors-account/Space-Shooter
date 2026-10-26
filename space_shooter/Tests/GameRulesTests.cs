using System;
using SpaceShooter;

// Outside Assets: these tests do not enter the Unity player or need NUnit packages.
public static class GameRulesTests
{
    private static int checks;

    private static void Check(string name, bool condition)
    {
        if (!condition) throw new Exception("FAIL: " + name);
        checks++;
        Console.WriteLine("PASS: " + name);
    }

    private static bool Near(float a, float b) { return Math.Abs(a - b) < 0.0001f; }

    public static int Main()
    {
        float x = 0f;
        float y = 0f;
        GameRules.MovePlayer(ref x, ref y, 0f, 0f, 1f);
        Check("No input leaves player still", Near(x, 0f) && Near(y, 0f));
        GameRules.MovePlayer(ref x, ref y, 1f, 0f, 0.5f);
        Check("Right input follows speed", Near(x, 3f) && Near(y, 0f));
        GameRules.MovePlayer(ref x, ref y, -1f, 0f, 0.5f);
        Check("Left input reverses movement", Near(x, 0f));
        GameRules.MovePlayer(ref x, ref y, 0f, 1f, 0.5f);
        Check("Up input moves upward", Near(y, 3f));
        GameRules.MovePlayer(ref x, ref y, 0f, -1f, 0.5f);
        Check("Down input moves downward", Near(y, 0f));
        GameRules.MovePlayer(ref x, ref y, 1f, 1f, 0.5f);
        Check("Diagonal speed is normalized", Near((float)Math.Sqrt(x * x + y * y), 3f));
        GameRules.MovePlayer(ref x, ref y, 1f, 1f, 100f);
        Check("Top and right bounds", Near(x, 7.45f) && Near(y, 4.5f));
        GameRules.MovePlayer(ref x, ref y, -1f, -1f, 100f);
        Check("Bottom and left bounds", Near(x, -7.45f) && Near(y, -4.5f));
        float oldX = x, oldY = y;
        GameRules.MovePlayer(ref x, ref y, 1f, 1f, 0f);
        Check("Zero timestep freezes movement", Near(x, oldX) && Near(y, oldY));

        float hit;
        Check("Stationary overlap", GameRules.SweptOverlap(0f, 0f, 0f, 0f, 1f, 1f, out hit));
        Check("Stationary separation", !GameRules.SweptOverlap(2f, 0f, 2f, 0f, 1f, 1f, out hit));
        Check("Fast bullet crosses enemy", GameRules.SweptOverlap(0f, -10f, 0f, 10f, 0.59f, 0.62f, out hit));
        Check("Collision time within segment", hit > 0f && hit < 1f);
        Check("Horizontal miss", !GameRules.SweptOverlap(2f, -10f, 2f, 10f, 0.59f, 0.62f, out hit));
        Check("Moving away misses", !GameRules.SweptOverlap(0f, 2f, 0f, 5f, 1f, 1f, out hit));
        Check("Exact boundary contact counts", GameRules.SweptOverlap(1f, -2f, 1f, 2f, 1f, 1f, out hit));
        Check("Enemy crosses player", GameRules.SweptOverlap(0f, 4f, 0f, -4f, 1.05f, 0.9f, out hit));
        Check("Diagonal corner miss", !GameRules.SweptOverlap(-3f, 0f, 0f, 3f, 1f, 1f, out hit));
        Check("Reverse-direction sweep", GameRules.SweptOverlap(4f, 0f, -4f, 0f, 1f, 1f, out hit));
        Check("Initial overlap hits at zero", Near(InitialOverlapTime(), 0f));
        Console.WriteLine("All " + checks + " pure C# rule checks passed.");
        return 0;
    }

    private static float InitialOverlapTime()
    {
        float hit;
        if (!GameRules.SweptOverlap(0f, 0f, 4f, 0f, 1f, 1f, out hit))
            throw new Exception("Initial overlap was missed.");
        return hit;
    }
}

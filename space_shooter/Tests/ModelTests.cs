using System;
using SpaceShooter;

internal static class ModelTests
{
    private static int checks;
    private static void Check(bool condition, string name)
    {
        if (!condition) throw new Exception("FAIL: " + name);
        checks++;
        Console.WriteLine("PASS: " + name);
    }
    private static void Near(float actual, float expected, string name)
    { Check(Math.Abs(actual - expected) < 0.002f, name); }
    private static ShooterModel Fresh() { return new ShooterModel(123); }
    private static void Frames(ShooterModel m, int count, float x, float y, bool fire)
    { for (int i = 0; i < count; i++) m.Advance(1f / 60f, x, y, fire); }

    public static int Main()
    {
        try
        {
            var m = Fresh();
            Check(!m.GameOver && m.Score == 0 && m.Enemies.Count == 0 && m.Bullets.Count == 0, "initial state");
            Frames(m, 30, 1, 0, false);
            Near(m.PlayerX, 3f, "right movement at configured speed");
            Frames(m, 30, -1, 0, false);
            Near(m.PlayerX, 0f, "left movement");
            Frames(m, 30, 0, 1, false);
            Near(m.PlayerY, -0.8f, "up movement");
            Frames(m, 30, 0, -1, false);
            Near(m.PlayerY, -3.8f, "down movement");
            m = Fresh();
            Frames(m, 30, 1, 1, false);
            Near((float)Math.Sqrt(m.PlayerX * m.PlayerX + (m.PlayerY + 3.8f) * (m.PlayerY + 3.8f)), 3f, "diagonal normalization");
            float x = m.PlayerX, y = m.PlayerY;
            Frames(m, 10, 0, 0, false);
            Check(m.PlayerX == x && m.PlayerY == y, "instant stop after input release");
            foreach (int direction in new[] { -1, 1 })
            {
                m = Fresh();
                // Isolate boundary movement from randomly spawned enemies.
                for (int i = 0; i < 200; i++)
                {
                    m.Enemies.Clear();
                    m.Advance(1f / 60f, direction, direction, false);
                }
                Near(m.PlayerX, direction * 7.6f, "horizontal boundary " + direction);
                Near(m.PlayerY, direction < 0 ? -4.6f : 4.2f, "vertical boundary " + direction);
            }
            m = Fresh();
            Frames(m, 60, 0, 0, true);
            Check(m.Bullets.Count >= 4 && m.Bullets.Count <= 6, "held fire cadence and bullet cleanup");
            Check(m.Bullets[0].Y > m.PlayerY, "bullets travel upward");
            Frames(m, 60, 0, 0, false);
            Check(m.Bullets.Count == 0, "offscreen bullets removed");
            m = Fresh();
            Frames(m, 80, 0, 0, false);
            Check(m.Enemies.Count == 1 && m.Enemies[0].Y < 5.5f && Math.Abs(m.Enemies[0].X) <= 7.4f, "spawn interval, range and descent");
            m = Fresh();
            m.Enemies.Add(new ShooterModel.Body(2, 1));
            m.Bullets.Add(new ShooterModel.Body(2, 0.5f));
            m.Advance(0.01f, 0, 0, false);
            Check(m.Score == 10 && m.Enemies.Count == 0 && m.Bullets.Count == 0, "bullet collision removes both and scores");
            m.Advance(0.01f, 0, 0, false);
            Check(m.Score == 10, "no duplicate score");
            m = Fresh();
            m.Enemies.Add(new ShooterModel.Body(2, 1));
            m.Enemies.Add(new ShooterModel.Body(2, 1));
            m.Bullets.Add(new ShooterModel.Body(2, 0.5f));
            m.Advance(0.01f, 0, 0, false);
            Check(m.Score == 10 && m.Enemies.Count == 1, "one bullet hits only one enemy");
            m = Fresh();
            m.Enemies.Add(new ShooterModel.Body(2, 0));
            m.Bullets.Add(new ShooterModel.Body(2, -0.9f));
            m.Advance(0.1f, 0, 0, false);
            Check(m.Score == 10, "substeps prevent bullet tunnelling on slow frame");
            m = Fresh();
            m.Enemies.Add(new ShooterModel.Body(m.PlayerX, m.PlayerY));
            m.Advance(0.01f, 0, 0, false);
            Check(m.GameOver, "ship collision ends game");
            x = m.PlayerX; y = m.Enemies[0].Y;
            m.Advance(0.1f, 1, 0, true);
            Check(m.PlayerX == x && m.Enemies[0].Y == y && m.Bullets.Count == 0, "game over freezes simulation");
            m.Restart();
            Check(!m.GameOver && m.Score == 0 && m.Enemies.Count == 0 && m.Bullets.Count == 0 && m.PlayerX == 0 && m.PlayerY == -3.8f, "restart resets full state");
            m.Enemies.Add(new ShooterModel.Body(7, -5.39f));
            m.Advance(0.02f, 0, 0, false);
            Check(m.GameOver, "escaped enemy ends game");
            m = Fresh();
            m.Advance(0, 1, 0, true);
            Check(m.PlayerX == 0 && m.Bullets.Count == 0, "zero delta has no effects");
            m.Advance(10, 1, 0, false);
            Near(m.PlayerX, 0.6f, "long frame catch-up is capped");
            bool rejected = false;
            try { m.Advance(float.NaN, 0, 0, false); } catch (ArgumentOutOfRangeException) { rejected = true; }
            Check(rejected, "invalid delta rejected");
            rejected = false;
            try { m.Advance(0.1f, float.PositiveInfinity, 0, false); } catch (ArgumentException) { rejected = true; }
            Check(rejected, "invalid input rejected");
            m = Fresh();
            for (int i = 0; i < 10000; i++)
            {
                m.Enemies.Clear();
                m.Advance(1f / 60f, 0, 0, true);
                if (m.Bullets.Count > 6) throw new Exception("Unbounded bullet list");
            }
            Check(!m.GameOver, "long-running fire keeps bullet count bounded");
            Console.WriteLine("All " + checks + " model checks passed. Unity integration and Windows runtime NOT tested.");
            return 0;
        }
        catch (Exception error) { Console.Error.WriteLine(error); return 1; }
    }
}

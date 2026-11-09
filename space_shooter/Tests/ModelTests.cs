using System;
using SpaceShooter;

internal static class ModelTests
{
    private static int passed;
    private static void Check(bool condition, string name)
    {
        if (!condition) throw new Exception("FAIL: " + name);
        passed++;
        Console.WriteLine("PASS: " + name);
    }
    private static bool Near(float a, float b) { return Math.Abs(a - b) < 0.001f; }
    private static ShooterModel.Body Enemy(int id, float x, float y)
    { return new ShooterModel.Body(id, x, y, 0.9f, 0.7f); }
    private static ShooterModel.Body Bullet(int id, float x, float y)
    { return new ShooterModel.Body(id, x, y, 0.16f, 0.45f); }

    private static void Main()
    {
        var m = new ShooterModel(123);
        Check(m.Score == 0 && !m.GameOver && m.Enemies.Count == 0, "initial state");
        m.Tick(0.05f, 1, 0, false);
        Check(Near(m.Player.X, 0.35f), "right movement speed");
        m.Tick(0.05f, -1, 0, false);
        Check(Near(m.Player.X, 0), "left movement");
        m.Tick(0.05f, 0, 1, false);
        Check(Near(m.Player.Y, -6.65f), "up movement");
        m.Tick(0.05f, 0, -1, false);
        Check(Near(m.Player.Y, -7), "down movement");
        m.Tick(0.05f, 1, 1, false);
        float diagonal = (float)Math.Sqrt(m.Player.X * m.Player.X + (m.Player.Y + 7) * (m.Player.Y + 7));
        Check(Near(diagonal, 0.35f), "diagonal speed normalized");
        float oldX = m.Player.X;
        m.Tick(0.05f, 0, 0, false);
        Check(Near(m.Player.X, oldX), "release stops immediately");
        m.Player.X = 7.59f; m.Player.Y = 6.49f;
        m.Tick(0.05f, 1, 1, false);
        Check(Near(m.Player.X, 7.6f) && Near(m.Player.Y, 6.5f), "upper right bounds");
        m.Player.X = -7.59f; m.Player.Y = -7.99f;
        m.Tick(0.05f, -1, -1, false);
        Check(Near(m.Player.X, -7.6f) && Near(m.Player.Y, -8f), "lower left bounds");
        m.Restart(); m.Tick(0.05f, 0, 0, true);
        Check(m.Bullets.Count == 1 && m.Bullets[0].Y > m.Player.Y, "shoot upward");
        m.Tick(0.05f, 0, 0, true);
        Check(m.Bullets.Count == 1, "fire cooldown");
        for (int i = 0; i < 3; i++) m.Tick(0.05f, 0, 0, true);
        Check(m.Bullets.Count == 2, "held fire repeats");
        m.Restart();
        for (int i = 0; i < 26; i++) m.Tick(0.05f, 0, 0, false);
        Check(m.Enemies.Count == 1 && Math.Abs(m.Enemies[0].X) <= 7, "spawn timing and horizontal bounds");
        float oldY = m.Enemies[0].Y;
        m.Tick(0.05f, 0, 0, false);
        Check(Near(m.Enemies[0].Y, oldY - 0.15f), "enemy moves downward");
        m.Restart(); m.Enemies.Add(Enemy(100, 0, 0)); m.Bullets.Add(Bullet(101, 0, -0.9f));
        m.Tick(0.05f, 0, 0, false);
        Check(m.Score == 10 && m.Enemies.Count == 0 && m.Bullets.Count == 0, "swept bullet collision awards score");
        m.Restart(); m.Enemies.Add(Enemy(100, 0, 0)); m.Bullets.Add(Bullet(101, 2, -0.9f));
        m.Tick(0.05f, 0, 0, false);
        Check(m.Score == 0 && m.Enemies.Count == 1, "non-overlapping bullet misses");
        m.Restart(); m.Enemies.Add(Enemy(100, 0, 0));
        m.Bullets.Add(Bullet(101, 0, -0.9f)); m.Bullets.Add(Bullet(102, 0, -0.9f));
        m.Tick(0.05f, 0, 0, false);
        Check(m.Score == 10 && m.Bullets.Count == 1, "no duplicate kill scoring");
        m.Restart(); m.Enemies.Add(Enemy(100, 0, -6.2f));
        m.Tick(0.05f, 0, 0, false);
        Check(m.GameOver && m.EndReason.Contains("hit"), "enemy player collision ends game");
        oldX = m.Player.X; oldY = m.Enemies[0].Y;
        m.Tick(0.05f, 1, 1, true);
        Check(Near(m.Player.X, oldX) && Near(m.Enemies[0].Y, oldY) && m.Bullets.Count == 0, "game over freezes simulation");
        m.Restart(); m.Enemies.Add(Enemy(100, 4, -9.3f)); m.Tick(0.05f, 0, 0, false);
        Check(m.GameOver && m.EndReason.Contains("escaped"), "escaped enemy ends game");
        m.Restart();
        Check(!m.GameOver && m.Score == 0 && m.Bullets.Count == 0 && m.Enemies.Count == 0
            && Near(m.Player.X, 0) && Near(m.Player.Y, -7) && m.EndReason == "", "restart resets all round state");
        m.Bullets.Add(Bullet(101, 0, 10)); m.Tick(0.05f, 0, 0, false);
        Check(m.Bullets.Count == 0, "offscreen bullet removed");
        m.Tick(0, 1, 0, true); m.Tick(-1, 1, 0, true); m.Tick(float.NaN, 1, 0, true);
        Check(Near(m.Player.X, 0) && m.Bullets.Count == 0, "invalid time ignored");
        m.Tick(100, 1, 0, false);
        Check(Near(m.Player.X, 0.35f), "large frame time clamped");
        Console.WriteLine("All " + passed + " simulation checks passed. Unity integration is not tested by this runner.");
    }
}

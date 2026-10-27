using System;
using MinimalSpaceShooter;

// Standalone model tests, intentionally outside Assets (no Unity package needed).
public static class ModelTests
{
    private static int passed;
    private static void Check(string name, bool condition)
    {
        if (!condition) throw new Exception("FAIL: " + name);
        Console.WriteLine("PASS: " + name);
        passed++;
    }
    private static bool Near(float a, float b) { return Math.Abs(a - b) < 0.0001f; }
    private static void Run(ShooterModel model, int steps, float x, float y, bool fire)
    {
        for (int i = 0; i < steps; i++) model.Step(0.01f, x, y, fire);
    }
    public static void Main()
    {
        var model = new ShooterModel(1);
        Check("initial state", model.Score == 0 && !model.GameOver && Near(model.PlayerY, -3.3f));
        model.Step(0.05f, 1, 0, false);
        Check("horizontal movement", Near(model.PlayerX, 0.3f));
        float x = model.PlayerX;
        model.Step(0.05f, 0, 0, false);
        Check("release stops immediately", Near(model.PlayerX, x));
        model.Restart();
        model.Step(0.05f, 1, 1, false);
        float dy = model.PlayerY + 3.3f;
        Check("diagonal speed normalized", Near((float)Math.Sqrt(model.PlayerX * model.PlayerX + dy * dy), 0.3f));
        model.Restart(); Run(model, 200, 1, 0, false);
        Check("right bound", Near(model.PlayerX, 7.5f));
        model.Restart(); Run(model, 200, -1, 0, false);
        Check("left bound", Near(model.PlayerX, -7.5f));
        model.Restart(); Run(model, 130, 0, 1, false);
        Check("top bound", Near(model.PlayerY, 3.15f));
        model.Restart(); Run(model, 30, 0, -1, false);
        Check("bottom bound", Near(model.PlayerY, -4.05f));
        model.Restart(); model.Step(0.01f, 0, 0, true);
        Check("first shot immediate", model.Bullets.Count == 1);
        float bulletY = model.Bullets[0].Y;
        model.Step(0.01f, 0, 0, true);
        Check("shot motion and cooldown", model.Bullets.Count == 1 && Near(model.Bullets[0].Y - bulletY, 0.12f));
        Run(model, 20, 0, 0, true);
        Check("held fire repeats", model.Bullets.Count == 2);
        model.Restart(); Run(model, 86, 0, 0, false);
        Check("enemy spawns within horizontal bounds", model.Enemies.Count == 1 && Math.Abs(model.Enemies[0].X) <= 7.4f);
        float enemyY = model.Enemies[0].Y;
        model.Step(0.01f, 0, 0, false);
        Check("enemy moves downward", Near(enemyY - model.Enemies[0].Y, 0.02f));
        model.Restart();
        model.bullets.Add(new ShooterModel.Body(100, 0, 0));
        model.enemies.Add(new ShooterModel.Body(101, 0, 0.7f));
        model.enemies.Add(new ShooterModel.Body(102, 0, 0.7f));
        model.Step(0.05f, 0, 0, false);
        Check("one shot removes one enemy and scores once", model.Score == 10 && model.Bullets.Count == 0 && model.Enemies.Count == 1);
        model.Restart();
        model.bullets.Add(new ShooterModel.Body(100, 0, 0));
        model.enemies.Add(new ShooterModel.Body(101, 2, 0.7f));
        model.Step(0.05f, 0, 0, false);
        Check("horizontal miss does not score", model.Score == 0 && model.Enemies.Count == 1);
        model.Restart();
        model.bullets.Add(new ShooterModel.Body(100, 0, 5));
        model.Step(0.01f, 0, 0, false);
        Check("offscreen bullet cleaned up", model.Bullets.Count == 0);
        model.Restart();
        model.enemies.Add(new ShooterModel.Body(100, 0, -3.3f));
        model.Step(0.01f, 0, 0, false);
        Check("player collision ends game", model.GameOver);
        x = model.PlayerX;
        model.Step(0.05f, 1, 1, true);
        Check("game over freezes simulation", Near(model.PlayerX, x) && model.Bullets.Count == 0);
        model.Restart();
        Check("restart clears entities and score", !model.GameOver && model.Enemies.Count == 0 && model.Bullets.Count == 0 && model.Score == 0 && Near(model.PlayerX, 0));
        model.enemies.Add(new ShooterModel.Body(100, 7, -5));
        model.Step(0.01f, 0, 0, false);
        Check("escaped enemy ends game", model.GameOver);
        model.Restart(); Run(model, 600, 0, 0, false);
        Check("idle eventually loses", model.GameOver);
        bool rejected = false;
        try { model.Step(float.NaN, 0, 0, false); } catch (ArgumentOutOfRangeException) { rejected = true; }
        Check("invalid timestep rejected", rejected);
        rejected = false;
        try { model.Step(0.1f, 0, 0, false); } catch (ArgumentOutOfRangeException) { rejected = true; }
        Check("oversized timestep rejected", rejected);
        rejected = false;
        try { model.Step(0.01f, float.NaN, 0, false); } catch (ArgumentException) { rejected = true; }
        Check("invalid input rejected", rejected);
        Console.WriteLine("Total: " + passed + " checks passed. Unity rendering and Windows build are NOT covered.");
    }
}

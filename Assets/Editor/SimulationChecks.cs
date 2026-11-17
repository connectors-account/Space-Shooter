using System;
using UnityEditor;
using UnityEngine;

namespace SpaceShooter.Editor
{
    public static class SimulationChecks
    {
        [MenuItem("Space Shooter/Run Simulation Checks")]
        public static void Run()
        {
            float time;
            Check(ShooterSimulation.Sweep(0, -10, 0, 10, .6f, .6f, out time), "Fast swept hit");
            Check(time > 0 && time < 1, "Hit time lies on segment");
            Check(!ShooterSimulation.Sweep(2, -10, 2, 10, .6f, .6f, out time), "Parallel miss");
            Check(ShooterSimulation.Sweep(0, 0, 0, 0, .6f, .6f, out time), "Initial overlap");
            var game = new ShooterSimulation(1);
            game.Step(.05f, 1, 1, false);
            float dx = game.PlayerX, dy = game.PlayerY + 4.5f;
            Check(Math.Abs(Math.Sqrt(dx * dx + dy * dy) - .25) < .0001, "Normalized diagonal speed");
            game.Reset(1);
            game.Step(.05f, 0, 0, true);
            Check(game.Bullets.Count == 1, "Shooting");
            game.Step(.05f, 0, 0, true);
            Check(game.Bullets.Count == 1, "Shot cooldown");
            game.Reset(1);
            game.Enemies.Add(new ShooterSimulation.Body { Id = 100, X = 0, Y = 0 });
            game.Bullets.Add(new ShooterSimulation.Body { Id = 101, X = 0, Y = -.9f });
            game.Step(.05f, 0, 0, false);
            Check(game.Score == 10 && game.Enemies.Count == 0 && game.Bullets.Count == 0,
                "Hit removes enemy and bullet; scores once");
            game.Enemies.Add(new ShooterSimulation.Body { Id = 102, X = 0, Y = game.PlayerY });
            game.Step(.01f, 0, 0, false);
            Check(game.GameOver, "Player collision ends game");
            float stoppedX = game.PlayerX;
            game.Step(.05f, 1, 0, true);
            Check(game.PlayerX == stoppedX && game.Score == 10, "Game-over freezes simulation");
            game.Reset(2);
            Check(!game.GameOver && game.Score == 0 && game.Enemies.Count == 0
                && game.Bullets.Count == 0 && game.PlayerY == -4.5f, "Restart clears state");
            game.Enemies.Add(new ShooterSimulation.Body { Id = 103, X = 6, Y = -5.55f });
            game.Step(.05f, 0, 0, false);
            Check(game.GameOver, "Escaped enemy ends game");
            game.Reset(3);
            for (int i = 0; i < 40; i++) game.Step(.05f, 1, -1, false);
            Check(game.PlayerX <= 7.55f && game.PlayerY >= -5.5f, "Player stays inside bounds");
            Check(game.Enemies.Count > 0, "Timed enemy spawn");
            game.Reset(4);
            game.Step(0, 1, 1, true);
            Check(game.PlayerX == 0 && game.Bullets.Count == 0, "Zero timestep is ignored");
            Debug.Log("Space Shooter: all 15 simulation checks passed. This does not test rendering or input.");
        }
        private static void Check(bool condition, string name)
        {
            if (!condition) throw new InvalidOperationException("Simulation check failed: " + name);
        }
    }
}

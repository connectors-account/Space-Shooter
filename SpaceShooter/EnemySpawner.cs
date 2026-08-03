using System;
using System.Collections.Generic;

namespace SpaceShooter
{
    /// <summary>
    /// Handles wave-based enemy spawning.
    /// Each wave spawns (5 + wave*3) enemies with a mix of types
    /// determined by the current wave difficulty.
    /// </summary>
    public class EnemySpawner
    {
        private int   _wave;
        private int   _spawned;
        private int   _totalThisWave;
        private float _spawnTimer;
        private float _spawnInterval;

        private readonly Random _rng = new();

        /// <summary>True when every enemy for this wave has been spawned.</summary>
        public bool AllSpawned => _spawned >= _totalThisWave;

        public void StartWave(int wave)
        {
            _wave          = wave;
            _spawned       = 0;
            _totalThisWave = 5 + wave * 3;
            _spawnInterval = Math.Max(0.5f, 1.6f - wave * 0.1f);
            _spawnTimer    = 1.2f;   // short delay before first enemy
        }

        public void Update(float dt, List<Enemy> enemies, int screenW)
        {
            if (AllSpawned) return;

            _spawnTimer -= dt;
            if (_spawnTimer > 0) return;

            _spawnTimer = _spawnInterval;
            SpawnOne(enemies, screenW);
        }

        private void SpawnOne(List<Enemy> enemies, int screenW)
        {
            float x   = _rng.Next(45, screenW - 45);
            float y   = -55f;
            int   roll = _rng.Next(100);

            EnemyType type;
            if      (_wave >= 3 && roll < 20) type = EnemyType.Tank;
            else if (roll < 50)               type = EnemyType.Zigzag;
            else                              type = EnemyType.Basic;

            enemies.Add(new Enemy(x, y, type, _wave));
            _spawned++;
        }
    }
}

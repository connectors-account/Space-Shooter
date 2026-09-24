using UnityEngine;

namespace Starward
{
    public sealed class WaveDirector : MonoBehaviour
    {
        GameManager game;
        int wave, remaining, alive, spawned;
        float nextSpawn, nextWave;
        bool running;
        public int Remaining { get { return remaining + alive; } }
        public bool Intermission { get { return running && remaining == 0 && alive == 0; } }
        public void Begin(GameManager owner)
        {
            game = owner;
            wave = remaining = alive = spawned = 0;
            nextWave = Time.time + 1;
            running = true;
        }
        public void Stop() { running = false; remaining = alive = 0; }
        public void EnemyRemoved() { alive = Mathf.Max(0, alive - 1); }
        void Update()
        {
            if (!running || !game.IsPlaying) return;
            if (remaining == 0 && alive == 0)
            {
                if (Time.time < nextWave) return;
                wave++;
                remaining = GameRules.EnemyCount(wave);
                game.NewWave(wave);
                nextSpawn = Time.time;
            }
            if (remaining > 0 && Time.time >= nextSpawn)
            {
                // Low-discrepancy deterministic lanes, spread across the arena.
                float x = -8.2f + ((spawned * 7) % 17) * 1.025f;
                Enemy enemy = Instantiate(game.enemyPrefab, new Vector3(x, 6.2f, 0), Quaternion.identity, game.entities);
                enemy.Initialize(game, wave, spawned % 3 == 2);
                remaining--; alive++; spawned++;
                nextSpawn = Time.time + GameRules.SpawnDelay(wave);
            }
            // Start the intermission clock after the last live enemy is removed.
            if (remaining > 0 || alive > 0) nextWave = Time.time + 2.5f;
        }
    }
}

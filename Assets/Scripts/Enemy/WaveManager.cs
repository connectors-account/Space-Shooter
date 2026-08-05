using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpaceShooter.Core;
using SpaceShooter.Audio;
using SpaceShooter.Utilities;

namespace SpaceShooter.Enemy
{
    /// <summary>
    /// Coroutine-based wave runner. For each wave it spawns the configured
    /// enemies, waits until they are all dead, pauses briefly, then advances.
    /// Difficulty scales up each wave (speed, spawn rate, enemy count). When the
    /// authored wave list is exhausted it generates escalating procedural waves
    /// so play can continue indefinitely, inserting a boss wave periodically.
    /// </summary>
    public class WaveManager : MonoBehaviour
    {
        [Header("Wiring")]
        [SerializeField] private EnemySpawner spawner;

        [Header("Authored waves (optional)")]
        [SerializeField] private List<WaveData> waves = new List<WaveData>();

        [Header("Pacing")]
        [SerializeField] private float pauseBetweenWaves = 2.5f;
        [SerializeField] private int bossEveryNWaves = 5;

        [Header("Difficulty scaling")]
        [SerializeField] private float difficultyPerWave = 0.08f;
        [SerializeField] private float spawnIntervalScale = 0.96f;

        public event Action<int> OnWaveStart;      // wave number (1-based)
        public event Action<int> OnWaveComplete;   // wave number
        public event Action OnBossSpawn;
        public event Action OnAllWavesComplete;

        public int CurrentWave { get; private set; }

        private Coroutine _runner;

        private void Start()
        {
            if (spawner == null) spawner = FindObjectOfType<EnemySpawner>();
            if (spawner == null)
            {
                var go = new GameObject("EnemySpawner");
                spawner = go.AddComponent<EnemySpawner>();
            }
            BeginWaves();
        }

        public void BeginWaves()
        {
            if (_runner != null) StopCoroutine(_runner);
            CurrentWave = 0;
            _runner = StartCoroutine(RunWaves());
        }

        public void StopWaves()
        {
            if (_runner != null)
            {
                StopCoroutine(_runner);
                _runner = null;
            }
        }

        private IEnumerator RunWaves()
        {
            // Small initial delay so the scene settles.
            yield return new WaitForSeconds(1f);

            while (true)
            {
                CurrentWave++;
                WaveData wave = GetWaveData(CurrentWave);
                float difficulty = 1f + (CurrentWave - 1) * difficultyPerWave;
                difficulty *= Mathf.Max(0.5f, wave.difficultyMultiplier);

                OnWaveStart?.Invoke(CurrentWave);
                if (AudioManager.Instance != null)
                    AudioManager.Instance.PlaySFX(wave.hasBoss ? Constants.SfxBossSpawn : Constants.SfxWaveStart);

                if (wave.hasBoss)
                    OnBossSpawn?.Invoke();

                yield return StartCoroutine(SpawnWave(wave, difficulty));

                // Wait until every spawned enemy is dead.
                while (spawner != null && spawner.AliveCount > 0)
                {
                    // Abort if the game ended.
                    if (GameManager.Instance != null && GameManager.Instance.State == GameState.GameOver)
                        yield break;
                    yield return null;
                }

                OnWaveComplete?.Invoke(CurrentWave);

                if (CurrentWave == waves.Count && waves.Count > 0)
                    OnAllWavesComplete?.Invoke();

                yield return new WaitForSeconds(pauseBetweenWaves);

                if (GameManager.Instance != null && GameManager.Instance.State == GameState.GameOver)
                    yield break;
            }
        }

        private IEnumerator SpawnWave(WaveData wave, float difficulty)
        {
            float interval = wave.spawnInterval * Mathf.Pow(spawnIntervalScale, CurrentWave - 1);
            interval = Mathf.Max(0.15f, interval);

            foreach (var entry in wave.entries)
            {
                if (entry == null) continue;
                for (int i = 0; i < entry.count; i++)
                {
                    if (GameManager.Instance != null && GameManager.Instance.State == GameState.GameOver)
                        yield break;

                    // Pause spawning while the game is paused.
                    while (GameManager.Instance != null && GameManager.Instance.IsPaused)
                        yield return null;

                    spawner.Spawn(entry.enemyType, difficulty);
                    yield return new WaitForSeconds(entry.enemyType == EnemyType.Boss ? 0.1f : interval);
                }
            }
        }

        /// <summary>
        /// Return authored wave data if available, else synthesise a wave whose
        /// composition and difficulty grow with the wave number.
        /// </summary>
        private WaveData GetWaveData(int waveNumber)
        {
            if (waveNumber <= waves.Count && waves[waveNumber - 1] != null)
                return waves[waveNumber - 1];

            return GenerateProceduralWave(waveNumber);
        }

        private WaveData GenerateProceduralWave(int waveNumber)
        {
            var wave = ScriptableObject.CreateInstance<WaveData>();
            bool boss = bossEveryNWaves > 0 && waveNumber % bossEveryNWaves == 0;

            if (boss)
            {
                wave.hasBoss = true;
                wave.spawnInterval = 0.5f;
                wave.entries = new[]
                {
                    new WaveEntry { enemyType = EnemyType.Drone, count = 4 },
                    new WaveEntry { enemyType = EnemyType.Boss, count = 1 }
                };
            }
            else
            {
                int drones = 4 + waveNumber;
                int fighters = Mathf.Max(0, waveNumber - 1);
                int bombers = waveNumber >= 3 ? (waveNumber / 3) : 0;

                var entries = new List<WaveEntry>
                {
                    new WaveEntry { enemyType = EnemyType.Drone, count = drones }
                };
                if (fighters > 0) entries.Add(new WaveEntry { enemyType = EnemyType.Fighter, count = fighters });
                if (bombers > 0) entries.Add(new WaveEntry { enemyType = EnemyType.Bomber, count = bombers });

                wave.entries = entries.ToArray();
                wave.spawnInterval = Mathf.Max(0.3f, 0.9f - waveNumber * 0.03f);
            }
            return wave;
        }
    }
}

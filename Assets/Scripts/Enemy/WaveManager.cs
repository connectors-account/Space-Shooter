using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpaceShooter.Core;
using SpaceShooter.Utilities;

namespace SpaceShooter.Enemy
{
    /// <summary>
    /// Definition of a single wave. Not a Unity asset — waves are declared inline in code.
    /// </summary>
    [Serializable]
    public struct WaveData
    {
        public int enemyCount;
        public EnemyKind[] enemyTypes;
        public float spawnInterval;
        public bool isBossWave;
        public bool finalBoss;

        public WaveData(int count, EnemyKind[] types, float interval, bool boss = false, bool final = false)
        {
            enemyCount = count;
            enemyTypes = types;
            spawnInterval = interval;
            isBossWave = boss;
            finalBoss = final;
        }
    }

    /// <summary>
    /// Drives wave progression: builds the 10 inline waves, starts each via the spawner,
    /// runs a 3-second countdown between waves, and reports completion to the GameManager.
    /// </summary>
    public class WaveManager : MonoBehaviour
    {
        public EnemySpawner spawner;
        public float countdownSeconds = 3f;

        public int CurrentWaveIndex { get; private set; } = -1;
        public int TotalWaves => _waves.Count;

        // Events: wave number is 1-based. Countdown passes remaining seconds each tick.
        public event Action<int> OnWaveStart;
        public event Action<int> OnWaveComplete;
        public event Action<int, float> OnCountdownTick; // (nextWaveNumber, secondsRemaining)

        private readonly List<WaveData> _waves = new List<WaveData>();
        private bool _running;

        private static readonly EnemyKind[] A = { EnemyKind.TypeA };
        private static readonly EnemyKind[] B = { EnemyKind.TypeB };
        private static readonly EnemyKind[] AB = { EnemyKind.TypeA, EnemyKind.TypeB };
        private static readonly EnemyKind[] ABB = { EnemyKind.TypeA, EnemyKind.TypeB, EnemyKind.TypeB };

        private void Awake()
        {
            BuildWaves();
        }

        private void BuildWaves()
        {
            _waves.Clear();
            _waves.Add(new WaveData(5, A, 0.7f));                       // 1: 5 TypeA
            _waves.Add(new WaveData(8, A, 0.6f));                       // 2: 8 TypeA
            _waves.Add(new WaveData(8, ABB, 0.6f));                     // 3: 5 A + 3 B (mixed pattern)
            _waves.Add(new WaveData(10, B, 0.5f));                      // 4: 10 TypeB
            _waves.Add(new WaveData(1, A, 0f, boss: true));             // 5: Boss
            _waves.Add(new WaveData(12, AB, 0.5f));                     // 6
            _waves.Add(new WaveData(14, ABB, 0.45f));                   // 7
            _waves.Add(new WaveData(16, ABB, 0.4f));                    // 8
            _waves.Add(new WaveData(18, B, 0.35f));                     // 9
            _waves.Add(new WaveData(1, A, 0f, boss: true, final: true));// 10: Final boss
        }

        private void Start()
        {
            if (spawner != null) spawner.OnAllEnemiesCleared += HandleWaveCleared;

            // Ensure we are in the Playing state even if the GameScene was opened directly
            // (i.e. not routed through the SceneLoader from the main menu).
            if (GameManager.Instance != null && GameManager.Instance.State != GameState.Playing)
                GameManager.Instance.StartNewGame();

            BeginWaveSequence();
        }

        private void OnDestroy()
        {
            if (spawner != null) spawner.OnAllEnemiesCleared -= HandleWaveCleared;
        }

        public void BeginWaveSequence()
        {
            if (_running) return;
            _running = true;
            CurrentWaveIndex = -1;
            StartCoroutine(StartNextWaveAfterCountdown());
        }

        private IEnumerator StartNextWaveAfterCountdown()
        {
            int nextWaveNumber = CurrentWaveIndex + 2; // 1-based number of the wave about to start
            float t = countdownSeconds;
            while (t > 0f)
            {
                OnCountdownTick?.Invoke(nextWaveNumber, t);
                t -= Time.deltaTime;
                yield return null;
            }
            OnCountdownTick?.Invoke(nextWaveNumber, 0f);
            StartNextWave();
        }

        private void StartNextWave()
        {
            CurrentWaveIndex++;
            if (CurrentWaveIndex >= _waves.Count)
            {
                _running = false;
                if (GameManager.Instance != null) GameManager.Instance.TriggerVictory();
                return;
            }

            int waveNumber = CurrentWaveIndex + 1;
            OnWaveStart?.Invoke(waveNumber);
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("wave_complete");

            if (spawner != null) spawner.SpawnWave(_waves[CurrentWaveIndex]);
        }

        private void HandleWaveCleared()
        {
            if (!_running) return;

            int waveNumber = CurrentWaveIndex + 1;
            OnWaveComplete?.Invoke(waveNumber);

            if (ScoreManager.Instance != null) ScoreManager.Instance.AddRaw(500); // clear bonus
            if (GameManager.Instance != null)
            {
                GameManager.Instance.CompleteWave(waveNumber);
                if (ScoreManager.Instance != null)
                    GameManager.Instance.SyncScore(ScoreManager.Instance.CurrentScore);
            }

            if (waveNumber >= _waves.Count)
            {
                _running = false; // victory already handled by GameManager.CompleteWave
                return;
            }

            StartCoroutine(StartNextWaveAfterCountdown());
        }

        public WaveData GetWave(int index)
        {
            if (index < 0 || index >= _waves.Count) return default;
            return _waves[index];
        }
    }
}

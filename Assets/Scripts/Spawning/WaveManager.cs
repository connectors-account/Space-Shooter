using System;
using System.Collections;
using UnityEngine;

namespace SpaceShooter
{
    /// <summary>Immutable description of a single wave.</summary>
    [Serializable]
    public struct WaveData
    {
        public EnemyType[] enemyTypes;
        public int[] counts;
        public float spawnInterval;
        public FormationType formationType;
        public bool isBossWave;
    }

    /// <summary>
    /// Drives wave progression: 10 waves with a boss on wave 5 (and a final boss on
    /// wave 10). Tracks living enemies and advances when the current wave is cleared.
    /// </summary>
    public class WaveManager : Singleton<WaveManager>
    {
        [Tooltip("Total number of waves in a run.")]
        public int TotalWaves = 10;

        [Tooltip("Seconds of countdown shown between waves.")]
        public float interWaveDelay = 3f;

        [Tooltip("When true, the manager drives the live EnemySpawner. Tests set this false.")]
        public bool autoSpawn = true;

        [SerializeField] private EnemySpawner spawner;

        public int CurrentWave { get; private set; }
        public int EnemiesRemaining { get; private set; }
        public bool IsRunning { get; private set; }

        public event Action<int> OnWaveStart;
        public event Action<int> OnWaveComplete;
        public event Action OnBossSpawn;
        public event Action OnAllWavesComplete;
        public event Action<float> OnCountdownTick;

        protected override void Awake()
        {
            base.Awake();
            if (spawner == null) spawner = FindObjectOfType<EnemySpawner>();
        }

        /// <summary>Test-friendly initialization; registers the singleton instance.</summary>
        public void Initialize()
        {
            RegisterSingleton();
        }

        /// <summary>Starts the run at wave 1.</summary>
        public void StartWaves()
        {
            IsRunning = true;
            CurrentWave = 0;
            if (autoSpawn)
            {
                StartCoroutine(BeginWaveAfterDelay(1, 1f));
            }
            else
            {
                BeginWave(1);
            }
        }

        /// <summary>Begins the specified wave immediately.</summary>
        public void BeginWave(int waveNumber)
        {
            CurrentWave = waveNumber;
            WaveData data = GetWaveData(waveNumber);
            EnemiesRemaining = GetTotalEnemies(data);
            IsRunning = true;

            OnWaveStart?.Invoke(waveNumber);
            if (data.isBossWave) OnBossSpawn?.Invoke();

            if (autoSpawn && spawner != null)
            {
                StartCoroutine(SpawnWaveRoutine(data));
            }
        }

        /// <summary>Reduces the living enemy count and advances the wave when cleared.</summary>
        public void NotifyEnemyKilled()
        {
            if (EnemiesRemaining > 0) EnemiesRemaining--;
            if (EnemiesRemaining > 0) return;

            OnWaveComplete?.Invoke(CurrentWave);

            if (CurrentWave < TotalWaves)
            {
                if (autoSpawn) StartCoroutine(BeginWaveAfterDelay(CurrentWave + 1, interWaveDelay));
                else BeginWave(CurrentWave + 1);
            }
            else
            {
                IsRunning = false;
                OnAllWavesComplete?.Invoke();
            }
        }

        /// <summary>Total number of enemies described by a wave.</summary>
        public int GetTotalEnemies(WaveData data)
        {
            int total = 0;
            if (data.counts != null)
            {
                for (int i = 0; i < data.counts.Length; i++) total += data.counts[i];
            }
            return total;
        }

        /// <summary>Returns the configuration for a given wave number (1-based).</summary>
        public WaveData GetWaveData(int waveNumber)
        {
            switch (waveNumber)
            {
                case 1:
                    return Wave(new[] { EnemyType.Drone }, new[] { 5 }, 0.8f, FormationType.Line, false);
                case 2:
                    return Wave(new[] { EnemyType.Drone }, new[] { 8 }, 0.6f, FormationType.Line, false);
                case 3:
                    return Wave(new[] { EnemyType.Drone, EnemyType.Fighter }, new[] { 5, 3 }, 0.7f, FormationType.VShape, false);
                case 4:
                    return Wave(new[] { EnemyType.Fighter }, new[] { 10 }, 0.6f, FormationType.VShape, false);
                case 5:
                    return Wave(new[] { EnemyType.Boss }, new[] { 1 }, 0f, FormationType.None, true);
                case 6:
                    return Wave(new[] { EnemyType.Drone, EnemyType.Fighter }, new[] { 8, 5 }, 0.6f, FormationType.Diamond, false);
                case 7:
                    return Wave(new[] { EnemyType.Fighter }, new[] { 14 }, 0.5f, FormationType.VShape, false);
                case 8:
                    return Wave(new[] { EnemyType.Drone, EnemyType.Fighter }, new[] { 10, 8 }, 0.5f, FormationType.Diamond, false);
                case 9:
                    return Wave(new[] { EnemyType.Fighter, EnemyType.Drone }, new[] { 12, 10 }, 0.4f, FormationType.Diamond, false);
                case 10:
                    return Wave(new[] { EnemyType.Boss }, new[] { 1 }, 0f, FormationType.None, true);
                default:
                    // Endless fallback: scale drones with the wave number.
                    return Wave(new[] { EnemyType.Drone }, new[] { Mathf.Max(5, waveNumber) }, 0.5f, FormationType.Line, false);
            }
        }

        private static WaveData Wave(EnemyType[] types, int[] counts, float interval, FormationType formation, bool boss)
        {
            return new WaveData
            {
                enemyTypes = types,
                counts = counts,
                spawnInterval = interval,
                formationType = formation,
                isBossWave = boss
            };
        }

        private IEnumerator BeginWaveAfterDelay(int waveNumber, float delay)
        {
            float remaining = delay;
            while (remaining > 0f)
            {
                OnCountdownTick?.Invoke(remaining);
                yield return new WaitForSeconds(1f);
                remaining -= 1f;
            }
            BeginWave(waveNumber);
        }

        private IEnumerator SpawnWaveRoutine(WaveData data)
        {
            if (data.isBossWave)
            {
                spawner.SpawnEnemy(EnemyType.Boss, new Vector3(0f, spawner.spawnY - 1f, 0f));
                yield break;
            }

            for (int t = 0; t < data.enemyTypes.Length; t++)
            {
                EnemyType type = data.enemyTypes[t];
                int count = data.counts[t];
                for (int i = 0; i < count; i++)
                {
                    spawner.SpawnEnemyRandom(type);
                    yield return new WaitForSeconds(data.spawnInterval);
                }
            }
        }
    }
}

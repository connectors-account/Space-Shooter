using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpaceShooter.Core;

namespace SpaceShooter.Systems
{
    /// <summary>
    /// Drives wave progression. Provides predefined waves 1-10 and procedurally generates the rest.
    /// Handles between-wave countdown and difficulty scaling.
    /// </summary>
    public class WaveManager : MonoBehaviour
    {
        [Serializable]
        public struct WaveData
        {
            public int waveNumber;
            public int enemyCount;
            public string[] enemyTypes;
            public float spawnInterval;
            public bool bossWave;
        }

        public static WaveManager Instance { get; private set; }

        [Header("Timing")]
        [SerializeField] private float betweenWaveDelay = 3f;
        [SerializeField] private float firstWaveDelay = 1.5f;

        public static event Action<int> OnWaveStart;
        public static event Action<int> OnWaveComplete;
        public static event Action OnAllWavesComplete;
        public static event Action<float> OnCountdownTick; // seconds remaining

        public int CurrentWave { get; private set; }
        public bool IsRunning { get; private set; }

        private readonly List<WaveData> predefinedWaves = new List<WaveData>();
        private bool waveInProgress;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            BuildPredefinedWaves();
        }

        private void Start()
        {
            StartCoroutine(BeginAfterDelay());
        }

        private IEnumerator BeginAfterDelay()
        {
            yield return new WaitForSeconds(firstWaveDelay);
            CurrentWave = 0;
            AdvanceWave();
        }

        private void BuildPredefinedWaves()
        {
            string[] a = { "EnemyA" };
            string[] ab = { "EnemyA", "EnemyB" };
            string[] b = { "EnemyB" };

            predefinedWaves.Add(new WaveData { waveNumber = 1, enemyCount = 5, enemyTypes = a, spawnInterval = 0.6f, bossWave = false });
            predefinedWaves.Add(new WaveData { waveNumber = 2, enemyCount = 7, enemyTypes = a, spawnInterval = 0.55f, bossWave = false });
            predefinedWaves.Add(new WaveData { waveNumber = 3, enemyCount = 8, enemyTypes = ab, spawnInterval = 0.5f, bossWave = false });
            predefinedWaves.Add(new WaveData { waveNumber = 4, enemyCount = 10, enemyTypes = ab, spawnInterval = 0.45f, bossWave = false });
            predefinedWaves.Add(new WaveData { waveNumber = 5, enemyCount = 1, enemyTypes = new[] { "EnemyBoss" }, spawnInterval = 0f, bossWave = true });
            predefinedWaves.Add(new WaveData { waveNumber = 6, enemyCount = 10, enemyTypes = ab, spawnInterval = 0.45f, bossWave = false });
            predefinedWaves.Add(new WaveData { waveNumber = 7, enemyCount = 12, enemyTypes = ab, spawnInterval = 0.4f, bossWave = false });
            predefinedWaves.Add(new WaveData { waveNumber = 8, enemyCount = 12, enemyTypes = b, spawnInterval = 0.4f, bossWave = false });
            predefinedWaves.Add(new WaveData { waveNumber = 9, enemyCount = 14, enemyTypes = ab, spawnInterval = 0.35f, bossWave = false });
            predefinedWaves.Add(new WaveData { waveNumber = 10, enemyCount = 1, enemyTypes = new[] { "EnemyBoss" }, spawnInterval = 0f, bossWave = true });
        }

        public WaveData GetWaveData(int waveNumber)
        {
            if (waveNumber >= 1 && waveNumber <= predefinedWaves.Count)
            {
                return predefinedWaves[waveNumber - 1];
            }
            return GenerateWave(waveNumber);
        }

        private WaveData GenerateWave(int waveNumber)
        {
            bool boss = waveNumber % 5 == 0;
            if (boss)
            {
                return new WaveData
                {
                    waveNumber = waveNumber,
                    enemyCount = 1,
                    enemyTypes = new[] { "EnemyBoss" },
                    spawnInterval = 0f,
                    bossWave = true
                };
            }

            int count = Mathf.Min(30, 8 + waveNumber);
            float interval = Mathf.Max(0.2f, 0.6f - waveNumber * 0.01f);
            return new WaveData
            {
                waveNumber = waveNumber,
                enemyCount = count,
                enemyTypes = new[] { "EnemyA", "EnemyB" },
                spawnInterval = interval,
                bossWave = false
            };
        }

        public void StartWave(int waveNumber)
        {
            CurrentWave = waveNumber;
            waveInProgress = true;
            IsRunning = true;
            if (GameManager.Instance != null) GameManager.Instance.SetWaveNumber(waveNumber);
            OnWaveStart?.Invoke(waveNumber);
        }

        public void NotifyWaveComplete()
        {
            if (!waveInProgress) return;
            waveInProgress = false;
            OnWaveComplete?.Invoke(CurrentWave);
            StartCoroutine(NextWaveCountdown());
        }

        private IEnumerator NextWaveCountdown()
        {
            float remaining = betweenWaveDelay;
            while (remaining > 0f)
            {
                OnCountdownTick?.Invoke(remaining);
                yield return null;
                remaining -= Time.deltaTime;
            }
            OnCountdownTick?.Invoke(0f);
            AdvanceWave();
        }

        private void AdvanceWave()
        {
            CurrentWave++;
            StartWave(CurrentWave);
        }
    }
}

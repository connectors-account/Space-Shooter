using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpaceShooter.Core;
using SpaceShooter.Data;
using SpaceShooter.UI;

namespace SpaceShooter.Spawning
{
    /// <summary>
    /// Drives wave progression. Starts wave 1, waits for the spawner to report the wave
    /// cleared, delays, then starts the next wave. After all authored waves it switches to
    /// an infinite mode with scaling difficulty.
    /// </summary>
    public class WaveManager : MonoBehaviour
    {
        [Header("Waves")]
        [SerializeField] private List<WaveData> waves = new List<WaveData>();

        [Header("Infinite Mode")]
        [Tooltip("Enemy type used for procedurally generated infinite waves.")]
        [SerializeField] private EnemyData infiniteEnemyData;
        [SerializeField] private int infiniteBaseCount = 8;
        [SerializeField] private float infiniteDifficultyStep = 0.15f;
        [SerializeField] private float infiniteTimeBetweenWaves = 2.5f;

        [Header("Startup")]
        [SerializeField] private float initialDelay = 1.5f;

        private int _waveIndex;
        private bool _infiniteMode;
        private int _infiniteWaveNumber;
        private EnemySpawner _spawner;

        private void Start()
        {
            _spawner = EnemySpawner.Instance;
            if (_spawner != null)
            {
                _spawner.OnWaveCleared += HandleWaveCleared;
            }

            if (GameManager.HasInstance)
            {
                GameManager.Instance.StartNewGame();
            }

            if (AudioManager.HasInstance)
            {
                AudioManager.Instance.PlayMusic(AudioManager.Instance.gameMusicClip);
            }

            StartCoroutine(BeginAfterDelay());
        }

        private void OnDestroy()
        {
            if (_spawner != null)
            {
                _spawner.OnWaveCleared -= HandleWaveCleared;
            }
        }

        private IEnumerator BeginAfterDelay()
        {
            yield return new WaitForSeconds(initialDelay);
            StartNextWave();
        }

        private void StartNextWave()
        {
            if (GameManager.HasInstance && GameManager.Instance.State == GameState.GameOver)
            {
                return;
            }

            if (!_infiniteMode && _waveIndex < waves.Count)
            {
                WaveData wave = waves[_waveIndex];
                _waveIndex++;

                if (GameManager.HasInstance)
                {
                    GameManager.Instance.NextWave();
                }

                AnnounceWave(GameManager.HasInstance ? GameManager.Instance.CurrentWave : _waveIndex, wave.hasBoss);
                _spawner.SpawnWave(wave);
            }
            else
            {
                // Switch to infinite mode.
                _infiniteMode = true;
                _infiniteWaveNumber++;

                if (GameManager.HasInstance)
                {
                    GameManager.Instance.NextWave();
                }

                float difficulty = 1f + _infiniteWaveNumber * infiniteDifficultyStep;
                int count = Mathf.RoundToInt(infiniteBaseCount + _infiniteWaveNumber * 2f);

                AnnounceWave(GameManager.HasInstance ? GameManager.Instance.CurrentWave : _infiniteWaveNumber, false);

                if (infiniteEnemyData != null)
                {
                    _spawner.SpawnInfiniteWave(infiniteEnemyData, count, difficulty);
                }
            }
        }

        private void HandleWaveCleared()
        {
            if (GameManager.HasInstance && GameManager.Instance.State == GameState.GameOver)
            {
                return;
            }

            StartCoroutine(NextWaveAfterDelay());
        }

        private IEnumerator NextWaveAfterDelay()
        {
            float delay = infiniteTimeBetweenWaves;
            if (!_infiniteMode && _waveIndex - 1 >= 0 && _waveIndex - 1 < waves.Count)
            {
                delay = waves[_waveIndex - 1].timeBetweenWaves;
            }

            yield return new WaitForSeconds(delay);
            StartNextWave();
        }

        private void AnnounceWave(int waveNumber, bool isBoss)
        {
            if (!UIManager.HasInstance)
            {
                return;
            }

            string message = isBoss ? "WARNING: BOSS INCOMING!" : $"WAVE {waveNumber}";
            UIManager.Instance.ShowMessage(message, 2f);
        }
    }
}

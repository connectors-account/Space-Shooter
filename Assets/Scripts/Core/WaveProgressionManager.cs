using System;
using System.Collections;
using SpaceShooter.Enemies;
using UnityEngine;

namespace SpaceShooter.Core
{
    public class WaveProgressionManager : MonoBehaviour
    {
        [SerializeField] private EnemySpawner enemySpawner;
        [SerializeField] private WaveDefinition[] waves;
        [SerializeField] private float interWaveDelay = 2.5f;
        [SerializeField] private bool loopWaves = true;

        public int CurrentWaveNumber { get; private set; }
        public event Action<int> OnWaveChanged;

        private Coroutine waveRoutine;

        private void Start()
        {
            if (enemySpawner == null)
            {
                enemySpawner = FindObjectOfType<EnemySpawner>();
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameStateChanged += HandleGameStateChanged;
            }

            TryStartWaves();
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameStateChanged -= HandleGameStateChanged;
            }
        }

        private void HandleGameStateChanged(GameState state)
        {
            if (state == GameState.GameOver && waveRoutine != null)
            {
                StopCoroutine(waveRoutine);
                waveRoutine = null;
            }
            else if (state == GameState.Playing)
            {
                TryStartWaves();
            }
        }

        private void TryStartWaves()
        {
            if (waveRoutine == null && GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.Playing)
            {
                waveRoutine = StartCoroutine(WaveLoop());
            }
        }

        private IEnumerator WaveLoop()
        {
            if (waves == null || waves.Length == 0 || enemySpawner == null)
            {
                yield break;
            }

            int waveIndex = 0;

            while (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.Playing)
            {
                WaveDefinition currentWave = waves[waveIndex];
                CurrentWaveNumber++;
                OnWaveChanged?.Invoke(CurrentWaveNumber);

                yield return StartCoroutine(enemySpawner.SpawnWave(currentWave));
                yield return StartCoroutine(WaitForWaveClear());
                yield return new WaitForSeconds(interWaveDelay);

                waveIndex++;
                if (waveIndex >= waves.Length)
                {
                    if (loopWaves)
                    {
                        waveIndex = 0;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            waveRoutine = null;
        }

        private IEnumerator WaitForWaveClear()
        {
            while (true)
            {
                EnemyBase[] enemies = FindObjectsOfType<EnemyBase>();
                if (enemies.Length == 0)
                {
                    yield break;
                }

                yield return new WaitForSeconds(0.4f);
            }
        }
    }
}

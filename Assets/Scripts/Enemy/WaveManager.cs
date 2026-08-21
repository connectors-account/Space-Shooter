using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpaceShooter.Core;
using SpaceShooter.Utilities;

namespace SpaceShooter.Enemy
{
    /// <summary>
    /// Serializable configuration for a single wave.
    /// </summary>
    [Serializable]
    public struct WaveConfig
    {
        public int waveNumber;
        public int enemyCount;
        public EnemyType[] enemyTypes;
        public float spawnInterval;
        public float speedMultiplier;
        public bool hasBoss;
    }

    /// <summary>
    /// Drives the 10-wave progression with increasing difficulty.
    /// Waves 5 and 10 spawn a boss. Announces each wave via events and
    /// waits 3 seconds between waves.
    /// </summary>
    public class WaveManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private EnemySpawner spawner;
        [SerializeField] private BossEnemy bossPrefab;

        [Header("Timing")]
        [SerializeField] private float betweenWaveDelay = 3f;
        [SerializeField] private float announceHold = 2f;

        [SerializeField] private List<WaveConfig> waves = new List<WaveConfig>();

        private Coroutine _runRoutine;
        private bool _bossAlive;

        public event Action<int, bool> OnWaveAnnounced; // waveNumber, isBossWave
        public event Action OnAllWavesComplete;

        private void Awake()
        {
            if (spawner == null)
            {
                spawner = FindObjectOfType<EnemySpawner>();
            }
            if (waves == null || waves.Count == 0)
            {
                BuildDefaultWaves();
            }
        }

        private void OnEnable()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameStart += BeginRun;
            }
        }

        private void OnDisable()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameStart -= BeginRun;
            }
        }

        /// <summary>Builds the 10 default waves with escalating difficulty.</summary>
        private void BuildDefaultWaves()
        {
            waves = new List<WaveConfig>();
            EnemyType[] basic = { EnemyType.Basic };
            EnemyType[] basicFast = { EnemyType.Basic, EnemyType.Fast };
            EnemyType[] all = { EnemyType.Basic, EnemyType.Fast, EnemyType.Tank };
            EnemyType[] fastTank = { EnemyType.Fast, EnemyType.Tank };

            waves.Add(new WaveConfig { waveNumber = 1, enemyCount = 6, enemyTypes = basic, spawnInterval = 1.2f, speedMultiplier = 1.0f, hasBoss = false });
            waves.Add(new WaveConfig { waveNumber = 2, enemyCount = 8, enemyTypes = basic, spawnInterval = 1.1f, speedMultiplier = 1.1f, hasBoss = false });
            waves.Add(new WaveConfig { waveNumber = 3, enemyCount = 10, enemyTypes = basicFast, spawnInterval = 1.0f, speedMultiplier = 1.2f, hasBoss = false });
            waves.Add(new WaveConfig { waveNumber = 4, enemyCount = 12, enemyTypes = basicFast, spawnInterval = 0.9f, speedMultiplier = 1.3f, hasBoss = false });
            waves.Add(new WaveConfig { waveNumber = 5, enemyCount = 0, enemyTypes = basicFast, spawnInterval = 0.9f, speedMultiplier = 1.3f, hasBoss = true });
            waves.Add(new WaveConfig { waveNumber = 6, enemyCount = 14, enemyTypes = all, spawnInterval = 0.85f, speedMultiplier = 1.4f, hasBoss = false });
            waves.Add(new WaveConfig { waveNumber = 7, enemyCount = 16, enemyTypes = all, spawnInterval = 0.8f, speedMultiplier = 1.5f, hasBoss = false });
            waves.Add(new WaveConfig { waveNumber = 8, enemyCount = 18, enemyTypes = fastTank, spawnInterval = 0.75f, speedMultiplier = 1.6f, hasBoss = false });
            waves.Add(new WaveConfig { waveNumber = 9, enemyCount = 20, enemyTypes = all, spawnInterval = 0.7f, speedMultiplier = 1.7f, hasBoss = false });
            waves.Add(new WaveConfig { waveNumber = 10, enemyCount = 0, enemyTypes = all, spawnInterval = 0.7f, speedMultiplier = 1.8f, hasBoss = true });
        }

        private void Start()
        {
            // Auto-start the session when the game scene loads (whether entered from the
            // menu or opened directly for testing).
            if (GameManager.Instance != null && GameManager.Instance.State != GameState.Playing)
            {
                GameManager.Instance.StartGame();
            }
        }

        public void BeginRun()
        {
            if (_runRoutine != null)
            {
                StopCoroutine(_runRoutine);
            }
            _runRoutine = StartCoroutine(RunWaves());
        }

        private IEnumerator RunWaves()
        {
            yield return new WaitForSeconds(1f);

            for (int i = 0; i < waves.Count; i++)
            {
                WaveConfig config = waves[i];
                GameManager.Instance?.SetWave(config.waveNumber);
                OnWaveAnnounced?.Invoke(config.waveNumber, config.hasBoss);

                yield return new WaitForSeconds(announceHold);

                if (config.hasBoss)
                {
                    yield return StartCoroutine(RunBossWave());
                }
                else
                {
                    yield return StartCoroutine(RunStandardWave(config));
                }

                if (i < waves.Count - 1)
                {
                    yield return new WaitForSeconds(betweenWaveDelay);
                }
            }

            OnAllWavesComplete?.Invoke();
        }

        private IEnumerator RunStandardWave(WaveConfig config)
        {
            int spawned = 0;
            var wait = new WaitForSeconds(config.spawnInterval);

            while (spawned < config.enemyCount)
            {
                if (GameManager.Instance != null && GameManager.Instance.State != GameState.Playing)
                {
                    yield return null;
                    continue;
                }

                EnemyType type = config.enemyTypes[UnityEngine.Random.Range(0, config.enemyTypes.Length)];
                if (spawner != null)
                {
                    spawner.SpawnEnemy(type, config.speedMultiplier);
                }
                spawned++;
                yield return wait;
            }

            // Wait until every enemy from this wave is cleared.
            while (spawner != null && spawner.AliveCount > 0)
            {
                yield return null;
            }
        }

        private IEnumerator RunBossWave()
        {
            if (bossPrefab == null)
            {
                yield break;
            }

            _bossAlive = true;
            BossEnemy boss = Instantiate(bossPrefab, new Vector3(0f, 8f, 0f), Quaternion.identity);
            boss.OnEnemyDied += _ => _bossAlive = false;

            while (_bossAlive)
            {
                if (boss == null)
                {
                    _bossAlive = false;
                    break;
                }
                yield return null;
            }
        }

        public void StopRun()
        {
            if (_runRoutine != null)
            {
                StopCoroutine(_runRoutine);
                _runRoutine = null;
            }
        }
    }
}

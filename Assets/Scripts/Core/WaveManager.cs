using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpaceShooter.Enemy;

namespace SpaceShooter.Core
{
    /// <summary>
    /// Owns the wave list and drives the spawn flow: announce, delay, spawn,
    /// track living enemies, advance. Cycles waves with rising difficulty past wave 10.
    /// </summary>
    public class WaveManager : MonoBehaviour
    {
        #region Singleton
        public static WaveManager Instance { get; private set; }
        #endregion

        #region Events
        /// <summary>Fired when a wave begins. Args: wave number, wave name.</summary>
        public static event Action<int, string> OnWaveStart;
        /// <summary>Fired when a (non-boss) wave is cleared.</summary>
        public static event Action<int> OnWaveComplete;
        /// <summary>Fired when the boss is defeated.</summary>
        public static event Action OnBossDefeated;
        #endregion

        #region Inspector Fields
        [Header("Enemy Prefabs")]
        [SerializeField] private GameObject _diverPrefab;
        [SerializeField] private GameObject _formationPrefab;
        [SerializeField] private GameObject _circlerPrefab;
        [SerializeField] private GameObject _bossPrefab;

        [Header("Config")]
        [SerializeField] private Transform _enemyParent;
        #endregion

        #region Runtime State
        private readonly List<WaveData> _waves = new List<WaveData>();
        private int _currentWaveIndex = -1;
        private int _cycleCount = 0;
        private int _livingEnemies = 0;
        private bool _running = false;
        private Coroutine _waveRoutine;

        /// <summary>1-based current wave number displayed to the player.</summary>
        public int CurrentWaveNumber { get; private set; }
        /// <summary>Current difficulty multiplier applied to health/score after cycling.</summary>
        public float DifficultyMultiplier { get; private set; } = 1f;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            BuildWaves();
        }
        #endregion

        #region Wave Definitions
        private void BuildWaves()
        {
            _waves.Clear();

            _waves.Add(new WaveData(1, "FIRST CONTACT", false, new List<EnemySpawnEntry>
            {
                new EnemySpawnEntry(EnemyType.Diver, 5, 0.4f, new Vector2(0f, 6f), false),
            }));

            _waves.Add(new WaveData(2, "PROBING RUN", false, new List<EnemySpawnEntry>
            {
                new EnemySpawnEntry(EnemyType.Diver, 6, 0.4f, new Vector2(0f, 6f), false),
                new EnemySpawnEntry(EnemyType.Formation, 3, 0.4f, new Vector2(-3f, 6f), true),
            }));

            _waves.Add(new WaveData(3, "DARK FLEET", false, new List<EnemySpawnEntry>
            {
                new EnemySpawnEntry(EnemyType.Formation, 6, 0.4f, new Vector2(-4f, 6f), true),
                new EnemySpawnEntry(EnemyType.Diver, 4, 0.4f, new Vector2(0f, 6f), false),
            }));

            _waves.Add(new WaveData(4, "ORBITAL AMBUSH", false, new List<EnemySpawnEntry>
            {
                new EnemySpawnEntry(EnemyType.Circler, 3, 0.5f, new Vector2(0f, 6f), false),
                new EnemySpawnEntry(EnemyType.Diver, 5, 0.4f, new Vector2(0f, 6f), false),
            }));

            _waves.Add(new WaveData(5, "PHALANX", false, new List<EnemySpawnEntry>
            {
                new EnemySpawnEntry(EnemyType.Formation, 8, 0.35f, new Vector2(-5f, 6f), true),
                new EnemySpawnEntry(EnemyType.Circler, 2, 0.5f, new Vector2(0f, 6f), false),
            }));

            _waves.Add(new WaveData(6, "SWARM", false, new List<EnemySpawnEntry>
            {
                new EnemySpawnEntry(EnemyType.Diver, 10, 0.3f, new Vector2(0f, 6f), false),
                new EnemySpawnEntry(EnemyType.Circler, 3, 0.5f, new Vector2(0f, 6f), false),
            }));

            _waves.Add(new WaveData(7, "IRON WALL", false, new List<EnemySpawnEntry>
            {
                new EnemySpawnEntry(EnemyType.Formation, 10, 0.3f, new Vector2(-5f, 6f), true),
                new EnemySpawnEntry(EnemyType.Diver, 6, 0.35f, new Vector2(0f, 6f), false),
            }));

            _waves.Add(new WaveData(8, "GYRE", false, new List<EnemySpawnEntry>
            {
                new EnemySpawnEntry(EnemyType.Circler, 5, 0.45f, new Vector2(0f, 6f), false),
                new EnemySpawnEntry(EnemyType.Formation, 6, 0.35f, new Vector2(-4f, 6f), true),
            }));

            _waves.Add(new WaveData(9, "FINAL VANGUARD", false, new List<EnemySpawnEntry>
            {
                new EnemySpawnEntry(EnemyType.Diver, 8, 0.3f, new Vector2(0f, 6f), false),
                new EnemySpawnEntry(EnemyType.Formation, 8, 0.3f, new Vector2(-5f, 6f), true),
                new EnemySpawnEntry(EnemyType.Circler, 4, 0.45f, new Vector2(0f, 6f), false),
            }));

            _waves.Add(new WaveData(10, "DREADNOUGHT", true, new List<EnemySpawnEntry>
            {
                new EnemySpawnEntry(EnemyType.Boss, 1, 0f, new Vector2(0f, GameConstants.BOSS_Y_POSITION), false),
            }));
        }
        #endregion

        #region Flow Control
        /// <summary>Starts the wave sequence from the beginning of the current cycle.</summary>
        public void BeginWaves()
        {
            StopWaves();
            _currentWaveIndex = -1;
            _cycleCount = 0;
            DifficultyMultiplier = 1f;
            _livingEnemies = 0;
            _running = true;
            StartNextWave();
        }

        /// <summary>Stops the wave flow and clears any running coroutine.</summary>
        public void StopWaves()
        {
            _running = false;
            if (_waveRoutine != null)
            {
                StopCoroutine(_waveRoutine);
                _waveRoutine = null;
            }
        }

        /// <summary>Advances to the next wave (wrapping and increasing difficulty).</summary>
        public void StartNextWave()
        {
            if (!_running) return;
            _currentWaveIndex++;

            if (_currentWaveIndex >= _waves.Count)
            {
                _currentWaveIndex = 0;
                _cycleCount++;
                DifficultyMultiplier = 1f + _cycleCount * GameConstants.WAVE_DIFFICULTY_STEP;
            }

            if (_waveRoutine != null) StopCoroutine(_waveRoutine);
            _waveRoutine = StartCoroutine(RunWave(_waves[_currentWaveIndex]));
        }

        private IEnumerator RunWave(WaveData wave)
        {
            CurrentWaveNumber = wave.waveNumber + _cycleCount * _waves.Count;
            _livingEnemies = 0;

            if (wave.isBossWave && GameManager.Instance != null)
                GameManager.Instance.EnterBossIntro();

            OnWaveStart?.Invoke(CurrentWaveNumber, wave.waveName);
            yield return new WaitForSeconds(GameConstants.WAVE_ANNOUNCE_DELAY);

            if (wave.isBossWave && GameManager.Instance != null)
                GameManager.Instance.ExitBossIntro();

            foreach (EnemySpawnEntry entry in wave.enemies)
            {
                yield return StartCoroutine(SpawnEntry(entry));
            }
        }

        private IEnumerator SpawnEntry(EnemySpawnEntry entry)
        {
            if (entry.useFormation)
            {
                // Spawn as a formation group across the top of the screen.
                float spacing = 1.6f;
                float startX = entry.spawnPosition.x;
                for (int i = 0; i < entry.count; i++)
                {
                    Vector3 pos = new Vector3(startX + i * spacing, entry.spawnPosition.y, 0f);
                    SpawnEnemy(entry.type, pos, i, entry.count);
                    yield return new WaitForSeconds(entry.spawnDelay * 0.5f);
                }
            }
            else
            {
                for (int i = 0; i < entry.count; i++)
                {
                    float x = entry.type == EnemyType.Boss
                        ? entry.spawnPosition.x
                        : UnityEngine.Random.Range(GameConstants.CAMERA_LEFT + 1f, GameConstants.CAMERA_RIGHT - 1f);
                    Vector3 pos = new Vector3(x, entry.spawnPosition.y, 0f);
                    SpawnEnemy(entry.type, pos, i, entry.count);
                    yield return new WaitForSeconds(entry.spawnDelay);
                }
            }
        }

        private void SpawnEnemy(EnemyType type, Vector3 position, int formationIndex, int formationTotal)
        {
            GameObject prefab = GetPrefab(type);
            if (prefab == null)
            {
                Debug.LogWarning($"[WaveManager] No prefab assigned for enemy type {type}.");
                return;
            }

            GameObject go = Instantiate(prefab, position, Quaternion.identity, _enemyParent);
            _livingEnemies++;

            EnemyBase enemy = go.GetComponent<EnemyBase>();
            if (enemy != null)
            {
                enemy.Configure(DifficultyMultiplier, formationIndex, formationTotal);
            }
        }

        private GameObject GetPrefab(EnemyType type)
        {
            switch (type)
            {
                case EnemyType.Diver: return _diverPrefab;
                case EnemyType.Formation: return _formationPrefab;
                case EnemyType.Circler: return _circlerPrefab;
                case EnemyType.Boss: return _bossPrefab;
                default: return null;
            }
        }
        #endregion

        #region Enemy Callbacks
        /// <summary>Called by enemies when they die. Advances the wave when cleared.</summary>
        public void EnemyKilled()
        {
            _livingEnemies = Mathf.Max(0, _livingEnemies - 1);

            WaveData current = _currentWaveIndex >= 0 && _currentWaveIndex < _waves.Count
                ? _waves[_currentWaveIndex]
                : null;

            if (_livingEnemies == 0 && _running && current != null && !current.isBossWave)
            {
                if (AudioManager.Instance != null)
                    AudioManager.Instance.PlaySFX(AudioManager.Instance.WaveComplete);
                OnWaveComplete?.Invoke(CurrentWaveNumber);
                StartCoroutine(NextWaveAfterDelay());
            }
        }

        private IEnumerator NextWaveAfterDelay()
        {
            yield return new WaitForSeconds(GameConstants.WAVE_COMPLETE_DELAY);
            StartNextWave();
        }

        /// <summary>Called by BossEnemy on death. Ends the boss wave and continues.</summary>
        public void BossDefeated()
        {
            _livingEnemies = 0;
            OnBossDefeated?.Invoke();
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX(AudioManager.Instance.WaveComplete);
            StartCoroutine(NextWaveAfterDelay());
        }

        /// <summary>Kills every living enemy on screen (Nuke power-up).</summary>
        public void NukeAllEnemies()
        {
            EnemyBase[] enemies = FindObjectsOfType<EnemyBase>();
            foreach (EnemyBase e in enemies)
            {
                if (e != null && e.gameObject.activeInHierarchy)
                {
                    e.TakeDamage(e.CurrentHealth);
                }
            }
        }

        /// <summary>Returns the number of enemies currently alive.</summary>
        public int LivingEnemyCount => _livingEnemies;
        #endregion
    }
}

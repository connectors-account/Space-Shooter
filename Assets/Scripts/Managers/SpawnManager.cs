using System;
using System.Collections;
using SpaceShooter.Core;
using SpaceShooter.Enemies;
using UnityEngine;

namespace SpaceShooter.Managers
{
    /// <summary>
    /// Spawns waves of enemies with steadily increasing difficulty and pools all enemy instances.
    /// Every <see cref="GameConfig.BossEveryNWaves"/> waves a boss is spawned instead of a normal
    /// formation. Raises <see cref="WaveCleared"/> once every enemy in the current wave is gone.
    /// </summary>
    public class SpawnManager : MonoBehaviour
    {
        /// <summary>Global access point.</summary>
        public static SpawnManager Instance { get; private set; }

        private GameConfig _config;
        private ObjectPool _pool;
        private Transform _container;
        private Transform _playerTransform;

        private int _aliveCount;
        private int _currentWave;
        private bool _waveActive;
        private Coroutine _spawnRoutine;

        /// <summary>Raised when all enemies of the active wave have been defeated.</summary>
        public event Action WaveCleared;

        /// <summary>True if a boss wave is currently in progress.</summary>
        public bool IsBossWave => _config != null && _currentWave % _config.BossEveryNWaves == 0;

        /// <summary>
        /// Builds the enemy pool. Called once by the bootstrap.
        /// </summary>
        /// <param name="config">Shared configuration.</param>
        /// <param name="playerTransform">Player transform for aimed enemy fire.</param>
        public void Initialize(GameConfig config, Transform playerTransform)
        {
            Instance = this;
            _config = config;
            _playerTransform = playerTransform;

            _container = new GameObject("Enemies").transform;
            _container.SetParent(transform, false);

            GameObject template = CreateTemplate();
            _pool = new ObjectPool(template, _container, prewarm: 16);
            template.SetActive(false);
        }

        private GameObject CreateTemplate()
        {
            var go = new GameObject("Enemy");
            go.SetActive(false);
            go.AddComponent<SpriteRenderer>();
            go.AddComponent<Enemy>();
            return go;
        }

        /// <summary>
        /// Begins spawning the supplied wave number (1-based).
        /// </summary>
        /// <param name="waveNumber">The wave to spawn.</param>
        public void BeginWave(int waveNumber)
        {
            _currentWave = waveNumber;
            _waveActive = true;
            _aliveCount = 0;
            _spawning = true;

            if (_spawnRoutine != null)
            {
                StopCoroutine(_spawnRoutine);
            }
            _spawnRoutine = StartCoroutine(SpawnWaveRoutine(waveNumber));
        }

        private IEnumerator SpawnWaveRoutine(int waveNumber)
        {
            if (waveNumber % _config.BossEveryNWaves == 0)
            {
                SpawnBoss(waveNumber);
                _spawning = false;
                _spawnRoutine = null;
                yield break;
            }

            int count = _config.BaseEnemiesPerWave + (waveNumber - 1) * _config.EnemiesAddedPerWave;
            float difficulty = 1f + (waveNumber - 1) * 0.12f;

            for (int i = 0; i < count; i++)
            {
                SpawnNormalEnemy(waveNumber, difficulty);
                yield return new WaitForSeconds(Mathf.Lerp(0.8f, 0.35f, waveNumber / (float)_config.TotalWaves));
            }

            _spawning = false;
            _spawnRoutine = null;
            // The final enemy may already be dead by the time spawning finishes.
            CheckWaveComplete();
        }

        private void SpawnNormalEnemy(int waveNumber, float difficulty)
        {
            EnemyType type = PickEnemyType(waveNumber);
            float x = UnityEngine.Random.Range(-_config.HalfWidth + 1f, _config.HalfWidth - 1f);
            Vector3 pos = new Vector3(x, _config.HalfHeight + 1f, 0f);

            int health;
            int score;
            float speed;
            switch (type)
            {
                case EnemyType.Zigzag:
                    health = Mathf.RoundToInt(50 * difficulty);
                    score = 150;
                    speed = 2.2f * Mathf.Min(difficulty, 1.6f);
                    break;
                case EnemyType.Circular:
                    health = Mathf.RoundToInt(60 * difficulty);
                    score = 200;
                    speed = 1.8f * Mathf.Min(difficulty, 1.6f);
                    break;
                default: // Basic
                    health = Mathf.RoundToInt(40 * difficulty);
                    score = 100;
                    speed = 2.6f * Mathf.Min(difficulty, 1.8f);
                    break;
            }

            Spawn(type, pos, health, score, speed);
        }

        private void SpawnBoss(int waveNumber)
        {
            Vector3 pos = new Vector3(0f, _config.HalfHeight + 2f, 0f);
            int bossTier = waveNumber / _config.BossEveryNWaves;
            int health = 1200 + (bossTier - 1) * 800;
            int score = 2000 * bossTier;
            Spawn(EnemyType.Boss, pos, health, score, 1.5f);
        }

        private void Spawn(EnemyType type, Vector3 position, int health, int score, float speed)
        {
            GameObject go = _pool.Get(position, Quaternion.identity);
            Enemy enemy = go.GetComponent<Enemy>();
            enemy.Configure(_config, type, health, score, speed, _playerTransform);
            enemy.Died += OnEnemyDied;
            _aliveCount++;
        }

        private EnemyType PickEnemyType(int waveNumber)
        {
            // Introduce variety as waves progress.
            float roll = UnityEngine.Random.value;
            if (waveNumber < 2)
            {
                return EnemyType.Basic;
            }
            if (waveNumber < 4)
            {
                return roll < 0.6f ? EnemyType.Basic : EnemyType.Zigzag;
            }
            if (roll < 0.45f) return EnemyType.Basic;
            if (roll < 0.75f) return EnemyType.Zigzag;
            return EnemyType.Circular;
        }

        private void OnEnemyDied(Enemy enemy, int scoreValue)
        {
            enemy.Died -= OnEnemyDied;

            if (scoreValue > 0)
            {
                GameManager.Instance?.AddScore(scoreValue);
            }

            _aliveCount = Mathf.Max(0, _aliveCount - 1);
            CheckWaveComplete();
        }

        private void CheckWaveComplete()
        {
            if (!_waveActive)
            {
                return;
            }

            if (_aliveCount <= 0 && !_spawning)
            {
                _waveActive = false;
                WaveCleared?.Invoke();
            }
        }

        private bool _spawning;

        /// <summary>Returns an enemy instance to the pool.</summary>
        public void ReleaseEnemy(GameObject go)
        {
            _pool?.Release(go);
        }

        /// <summary>Recycles every active enemy (used on restart / game over).</summary>
        public void ReleaseAll()
        {
            if (_spawnRoutine != null)
            {
                StopCoroutine(_spawnRoutine);
                _spawnRoutine = null;
            }
            _spawning = false;
            _waveActive = false;
            _aliveCount = 0;
            _pool?.ReleaseAll();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}

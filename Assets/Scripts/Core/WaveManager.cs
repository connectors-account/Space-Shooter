using SpaceShooter.Enemies;
using UnityEngine;

namespace SpaceShooter.Core
{
    public class WaveManager : MonoBehaviour
    {
        private GameManager _gameManager;
        private ObjectPoolManager _pool;
        private GameConfig _config;

        private int _toSpawnThisWave;
        private int _spawnedThisWave;
        private float _spawnTimer;
        private float _waveBreakTimer;
        private int _aliveEnemies;

        public void Initialize(GameManager gameManager, ObjectPoolManager pool, GameConfig config)
        {
            _gameManager = gameManager;
            _pool = pool;
            _config = config;
            _toSpawnThisWave = 0;
            _spawnedThisWave = 0;
            _waveBreakTimer = 1.5f;
            _aliveEnemies = 0;
        }

        private void Update()
        {
            if (_gameManager.CurrentState != GameState.Playing) return;

            if (_toSpawnThisWave == 0)
            {
                _waveBreakTimer -= Time.deltaTime;
                if (_waveBreakTimer <= 0f)
                {
                    BeginWave(_gameManager.CurrentWave + 1);
                }
                return;
            }

            _spawnTimer -= Time.deltaTime;
            if (_spawnedThisWave < _toSpawnThisWave && _spawnTimer <= 0f)
            {
                SpawnEnemyForWave();
                _spawnedThisWave++;
                _spawnTimer = Mathf.Max(0.2f, _config.SpawnInterval - _gameManager.DifficultyScale * 0.08f);
            }

            if (_spawnedThisWave >= _toSpawnThisWave && _aliveEnemies <= 0)
            {
                _toSpawnThisWave = 0;
                _spawnedThisWave = 0;
                _waveBreakTimer = _config.WaveBreakDuration;
            }
        }

        public void OnEnemySpawned() => _aliveEnemies++;

        public void OnEnemyDespawned() => _aliveEnemies = Mathf.Max(0, _aliveEnemies - 1);

        private void BeginWave(int wave)
        {
            _gameManager.SetWave(wave);
            _toSpawnThisWave = _config.BaseEnemiesPerWave + (wave - 1) * _config.ExtraEnemiesPerWave;
            _spawnedThisWave = 0;
            _spawnTimer = 0.4f;
        }

        private void SpawnEnemyForWave()
        {
            var difficulty = _gameManager.DifficultyScale;
            var roll = Random.value;
            EnemyType enemyType;

            var shooterChance = Mathf.Clamp01(0.12f + difficulty * 0.18f);
            var sineChance = Mathf.Clamp01(0.28f + difficulty * 0.15f);

            if (roll < shooterChance)
            {
                enemyType = EnemyType.Shooter;
            }
            else if (roll < shooterChance + sineChance)
            {
                enemyType = EnemyType.Sine;
            }
            else
            {
                enemyType = EnemyType.Grunt;
            }

            var x = Random.Range(-_config.PlayAreaHalfWidth + 0.8f, _config.PlayAreaHalfWidth - 0.8f);
            var spawnPos = new Vector3(x, _config.PlayAreaHalfHeight + 1.2f, 0f);
            var key = $"enemy_{enemyType.ToString().ToLower()}";
            var enemy = _pool.Get(key, spawnPos, Quaternion.identity);
            if (enemy == null) return;

            enemy.GetComponent<EnemyController>().Initialize(_gameManager, _pool, _config, enemyType, difficulty);
            OnEnemySpawned();
        }
    }
}

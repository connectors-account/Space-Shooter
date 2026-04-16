using System.Collections;
using SpaceShooter.Core;
using UnityEngine;

namespace SpaceShooter.Gameplay
{
    public class EnemySpawner : MonoBehaviour
    {
        private PoolManager _pool;
        private GameSession _session;
        private Transform _player;

        private int _wave;
        private int _spawnedInWave;
        private int _destroyedInWave;
        private int _targetInWave;
        private bool _spawning;

        public event System.Action<int> WaveStarted;

        public void Setup(PoolManager pool, GameSession session, Transform player)
        {
            _pool = pool;
            _session = session;
            _player = player;
            _wave = 1;
            StartWave(_wave);
        }

        public void StopSpawner()
        {
            _spawning = false;
            StopAllCoroutines();
        }

        public void NotifyEnemyDestroyed(int score)
        {
            _destroyedInWave++;
            _session.AddScore(score);

            if (_destroyedInWave >= _targetInWave)
            {
                StartCoroutine(BeginNextWaveAfterDelay());
            }
        }

        public void NotifyEnemyReturned()
        {
            _destroyedInWave++;
            if (_destroyedInWave >= _targetInWave)
            {
                StartCoroutine(BeginNextWaveAfterDelay());
            }
        }

        private IEnumerator BeginNextWaveAfterDelay()
        {
            if (!_spawning)
            {
                yield break;
            }

            _spawning = false;
            yield return new WaitForSeconds(2f);
            _wave++;
            StartWave(_wave);
        }

        private void StartWave(int wave)
        {
            _wave = wave;
            _session.SetWave(_wave);
            WaveStarted?.Invoke(_wave);

            _spawnedInWave = 0;
            _destroyedInWave = 0;
            _targetInWave = 5 + (_wave - 1) * 3;
            _spawning = true;

            StopAllCoroutines();
            StartCoroutine(SpawnRoutine());
        }

        private IEnumerator SpawnRoutine()
        {
            yield return new WaitForSeconds(1f);

            while (_spawning && _spawnedInWave < _targetInWave)
            {
                SpawnEnemy();
                _spawnedInWave++;
                var interval = Mathf.Max(0.3f, 1.2f - _wave * 0.06f);
                yield return new WaitForSeconds(interval);
            }
        }

        private void SpawnEnemy()
        {
            var type = ChooseEnemyType();
            var poolKey = GameBootstrap.GetEnemyPool(type);
            var x = Random.Range(-8.5f, 8.5f);
            var enemyObj = _pool.Spawn(poolKey, new Vector3(x, 6f, 0f), Quaternion.identity);
            if (enemyObj == null)
            {
                return;
            }

            var enemy = enemyObj.GetComponent<EnemyController>();
            enemy.Setup(type, _wave, this, _pool, _player);
        }

        private EnemyType ChooseEnemyType()
        {
            var roll = Random.value;
            if (_wave < 3)
            {
                return roll < 0.75f ? EnemyType.Basic : EnemyType.Zigzag;
            }

            if (_wave < 6)
            {
                if (roll < 0.45f) return EnemyType.Basic;
                if (roll < 0.75f) return EnemyType.Zigzag;
                return EnemyType.Tank;
            }

            if (roll < 0.3f) return EnemyType.Basic;
            if (roll < 0.58f) return EnemyType.Zigzag;
            if (roll < 0.82f) return EnemyType.Tank;
            return EnemyType.Spinner;
        }
    }
}

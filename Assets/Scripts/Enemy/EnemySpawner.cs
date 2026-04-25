using SpaceShooter.Core;
using UnityEngine;

namespace SpaceShooter.Enemy
{
    public class EnemySpawner : MonoBehaviour
    {
        private float _spawnInterval = 1.4f;
        private float _nextSpawnTime;
        private int _toSpawnInWave;
        private bool _waveActive;

        private void Start()
        {
            StartWave(1);
        }

        private void Update()
        {
            if (GameManager.Instance == null || GameManager.Instance.State != GameState.Playing)
            {
                return;
            }

            if (_waveActive && _toSpawnInWave > 0 && Time.time >= _nextSpawnTime)
            {
                SpawnEnemy();
                _toSpawnInWave--;
                _nextSpawnTime = Time.time + _spawnInterval;
            }

            if (_waveActive && _toSpawnInWave <= 0 && EnemyController.ActiveEnemyCount == 0)
            {
                StartWave(GameManager.Instance.Wave + 1);
            }
        }

        private void StartWave(int wave)
        {
            _waveActive = true;
            _toSpawnInWave = 5 + wave * 2;
            _spawnInterval = Mathf.Max(0.35f, 1.4f - wave * 0.08f);
            _nextSpawnTime = Time.time + 1f;

            GameManager.Instance.SetWave(wave);
        }

        private void SpawnEnemy()
        {
            var wave = GameManager.Instance.Wave;
            var spawnX = Random.Range(-8.3f, 8.3f);
            var spawnY = 5.8f;

            var type = ChooseEnemyType(wave);
            var enemyObject = new GameObject(type + "Enemy");
            enemyObject.transform.position = new Vector3(spawnX, spawnY, 0f);

            var enemy = enemyObject.AddComponent<EnemyController>();

            var health = type switch
            {
                EnemyType.Chaser => 1 + wave / 6,
                EnemyType.ZigZag => 2 + wave / 5,
                EnemyType.Turret => 3 + wave / 4,
                _ => 1
            };

            var speed = type switch
            {
                EnemyType.Chaser => 2.4f + wave * 0.1f,
                EnemyType.ZigZag => 2.0f + wave * 0.09f,
                EnemyType.Turret => 1.4f + wave * 0.07f,
                _ => 2f
            };

            var score = type switch
            {
                EnemyType.Chaser => 10,
                EnemyType.ZigZag => 20,
                EnemyType.Turret => 35,
                _ => 10
            };

            enemy.Initialize(type, health, speed, score);
        }

        private static EnemyType ChooseEnemyType(int wave)
        {
            var random = Random.value;

            var chaserWeight = Mathf.Clamp01(0.7f - wave * 0.02f);
            var zigzagWeight = Mathf.Clamp01(0.2f + wave * 0.01f);
            var turretWeight = Mathf.Clamp01(0.1f + wave * 0.015f);

            var total = chaserWeight + zigzagWeight + turretWeight;
            random *= total;

            if (random < chaserWeight)
            {
                return EnemyType.Chaser;
            }

            if (random < chaserWeight + zigzagWeight)
            {
                return EnemyType.ZigZag;
            }

            return EnemyType.Turret;
        }
    }
}

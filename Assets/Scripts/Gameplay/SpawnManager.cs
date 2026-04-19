using SpaceShooter.Core;
using UnityEngine;

namespace SpaceShooter.Gameplay
{
    public class SpawnManager : MonoBehaviour
    {
        private GameManager gameManager;

        private int currentWave;
        private int enemiesScheduled;
        private int enemiesSpawned;
        private int enemiesAlive;

        private float spawnInterval;
        private float nextSpawnTime;

        public bool IsWaveClear => enemiesScheduled > 0 && enemiesSpawned >= enemiesScheduled && enemiesAlive <= 0;

        public void Initialize()
        {
            spawnInterval = 1.3f;
        }

        public void Bind(GameManager manager)
        {
            gameManager = manager;
        }

        public void BeginWave(int wave, int enemyCount)
        {
            currentWave = wave;
            enemiesScheduled = enemyCount;
            enemiesSpawned = 0;
            enemiesAlive = 0;

            float difficultyRamp = Mathf.Clamp((wave - 1) * 0.08f, 0f, 0.7f);
            spawnInterval = 1.25f - difficultyRamp;
            nextSpawnTime = Time.time + 0.25f;
        }

        public void NotifyEnemyDestroyed()
        {
            enemiesAlive = Mathf.Max(0, enemiesAlive - 1);
        }

        private void Update()
        {
            if (gameManager == null || gameManager.State != GameManager.GameState.Playing)
            {
                return;
            }

            if (enemiesSpawned >= enemiesScheduled)
            {
                return;
            }

            if (Time.time < nextSpawnTime)
            {
                return;
            }

            SpawnOneEnemy();
            nextSpawnTime = Time.time + spawnInterval;
        }

        private void SpawnOneEnemy()
        {
            EnemyController.EnemyType enemyType = PickEnemyType();
            float x = Random.Range(-7.8f, 7.8f);
            Vector3 position = new Vector3(x, 6f, 0f);

            EntityFactory.CreateEnemy(enemyType, position);
            enemiesSpawned += 1;
            enemiesAlive += 1;
        }

        private EnemyController.EnemyType PickEnemyType()
        {
            float roll = Random.value;
            if (currentWave <= 2)
            {
                return roll < 0.75f ? EnemyController.EnemyType.Basic : EnemyController.EnemyType.ZigZag;
            }

            if (currentWave <= 5)
            {
                if (roll < 0.5f) return EnemyController.EnemyType.Basic;
                if (roll < 0.85f) return EnemyController.EnemyType.ZigZag;
                return EnemyController.EnemyType.Tank;
            }

            if (roll < 0.35f) return EnemyController.EnemyType.Basic;
            if (roll < 0.7f) return EnemyController.EnemyType.ZigZag;
            return EnemyController.EnemyType.Tank;
        }

        public void ClearAllDynamicEntities()
        {
            EnemyController[] enemies = FindObjectsOfType<EnemyController>();
            for (int i = 0; i < enemies.Length; i++)
            {
                Destroy(enemies[i].gameObject);
            }

            BulletController[] bullets = FindObjectsOfType<BulletController>();
            for (int i = 0; i < bullets.Length; i++)
            {
                Destroy(bullets[i].gameObject);
            }

            PowerUpController[] powerUps = FindObjectsOfType<PowerUpController>();
            for (int i = 0; i < powerUps.Length; i++)
            {
                Destroy(powerUps[i].gameObject);
            }

            enemiesAlive = 0;
            enemiesSpawned = 0;
            enemiesScheduled = 0;
        }
    }
}

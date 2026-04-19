using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceShooter.Enemy
{
    /// <summary>
    /// Spawns waves of enemies with progressive difficulty.
    /// </summary>
    public class EnemySpawner : MonoBehaviour
    {
        [SerializeField] private EnemyAI enemyPrefab;

        [Header("Spawn Area")]
        [SerializeField] private float minX = -8.5f;
        [SerializeField] private float maxX = 8.5f;
        [SerializeField] private float spawnY = 6.2f;

        [Header("Wave Tuning")]
        [SerializeField] private int baseEnemiesPerWave = 4;
        [SerializeField] private int addedEnemiesPerWave = 1;
        [SerializeField] private float baseSpawnGap = 0.8f;
        [SerializeField] private float minSpawnGap = 0.25f;
        [SerializeField] private float waveBreak = 1.6f;

        public int CurrentWave { get; private set; }

        public event System.Action<int> OnWaveStarted;

        private readonly List<GameObject> aliveEnemies = new List<GameObject>();
        private Coroutine spawnRoutine;
        private bool canSpawn;

        public void BeginSpawning(float initialDelay)
        {
            StopSpawningAndClear();
            canSpawn = true;
            spawnRoutine = StartCoroutine(SpawnLoop(initialDelay));
        }

        public void ResetSpawner()
        {
            CurrentWave = 0;
            StopSpawningAndClear();
        }

        public void StopSpawningAndClear()
        {
            canSpawn = false;

            if (spawnRoutine != null)
            {
                StopCoroutine(spawnRoutine);
                spawnRoutine = null;
            }

            for (int i = aliveEnemies.Count - 1; i >= 0; i--)
            {
                if (aliveEnemies[i] != null)
                {
                    Destroy(aliveEnemies[i]);
                }
            }

            aliveEnemies.Clear();

            foreach (var bullet in FindObjectsOfType<Combat.Bullet>())
            {
                Destroy(bullet.gameObject);
            }
        }

        private IEnumerator SpawnLoop(float initialDelay)
        {
            yield return new WaitForSeconds(initialDelay);

            while (canSpawn && Core.GameManager.Instance != null && Core.GameManager.Instance.IsGameplayActive())
            {
                CurrentWave++;
                OnWaveStarted?.Invoke(CurrentWave);
                Audio.SoundManager.Instance?.PlayWaveStart();

                int enemiesToSpawn = baseEnemiesPerWave + (CurrentWave - 1) * addedEnemiesPerWave;
                float gap = Mathf.Max(minSpawnGap, baseSpawnGap - (CurrentWave - 1) * 0.05f);

                for (int i = 0; i < enemiesToSpawn; i++)
                {
                    SpawnSingleEnemy(CurrentWave);
                    yield return new WaitForSeconds(gap);
                }

                // Wait until wave defeated.
                while (aliveEnemies.Exists(enemy => enemy != null))
                {
                    yield return null;
                }

                yield return new WaitForSeconds(waveBreak);
            }
        }

        private void SpawnSingleEnemy(int wave)
        {
            if (enemyPrefab == null)
            {
                Debug.LogWarning("EnemySpawner: enemyPrefab is not assigned.");
                return;
            }

            float x = Random.Range(minX, maxX);
            EnemyAI enemy = Instantiate(enemyPrefab, new Vector3(x, spawnY, 0f), Quaternion.identity);

            EnemyAI.EnemyType type = PickEnemyTypeForWave(wave);
            int health = 15 + (wave - 1) * 3;
            int score = 50 + (wave - 1) * 10;
            float speed = 2.2f + (wave - 1) * 0.12f;
            float fireCooldown = Mathf.Max(0.45f, 1.8f - (wave - 1) * 0.07f);

            enemy.Configure(type, speed, health, score, fireCooldown);
            aliveEnemies.Add(enemy.gameObject);

            var damageable = enemy.GetComponent<Combat.Damageable>();
            if (damageable != null)
            {
                damageable.OnDied += _ => aliveEnemies.Remove(enemy.gameObject);
            }
        }

        private EnemyAI.EnemyType PickEnemyTypeForWave(int wave)
        {
            // Early waves mostly straight, later waves include more advanced patterns.
            int roll = Random.Range(0, 100);
            if (wave < 3)
            {
                return roll < 80 ? EnemyAI.EnemyType.Straight : EnemyAI.EnemyType.ZigZag;
            }

            if (wave < 6)
            {
                return roll < 45 ? EnemyAI.EnemyType.Straight : (roll < 80 ? EnemyAI.EnemyType.ZigZag : EnemyAI.EnemyType.SineDive);
            }

            return roll < 25 ? EnemyAI.EnemyType.Straight : (roll < 60 ? EnemyAI.EnemyType.ZigZag : EnemyAI.EnemyType.SineDive);
        }
    }
}

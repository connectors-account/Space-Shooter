using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpaceShooter.Core;
using SpaceShooter.Systems;

namespace SpaceShooter.Enemy
{
    /// <summary>
    /// Spawns enemies for each wave using formations and the ObjectPool.
    /// Tracks active enemies and notifies the WaveManager when a wave is cleared.
    /// </summary>
    public class EnemySpawner : MonoBehaviour
    {
        public enum Formation { Line, VShape, Diamond, Random }

        public static EnemySpawner Instance { get; private set; }

        [Header("Spawn Settings")]
        [SerializeField] private float spawnEdgePadding = 1.2f;
        [SerializeField] private float spawnHeightOffset = 1f;
        [SerializeField] private float formationSpacing = 1.5f;
        [SerializeField] private float perEnemyDelay = 0.3f;
        [SerializeField] private string bossPoolTag = "EnemyBoss";

        private Camera cam;
        private readonly HashSet<EnemyBase> activeEnemies = new HashSet<EnemyBase>();
        private int expectedThisWave;
        private int spawnedThisWave;
        private int destroyedThisWave;
        private bool waveActive;

        private float healthMultiplier = 1f;
        private float speedBonus = 0f;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            cam = Camera.main;
        }

        private void OnEnable()
        {
            WaveManager.OnWaveStart += HandleWaveStart;
        }

        private void OnDisable()
        {
            WaveManager.OnWaveStart -= HandleWaveStart;
        }

        private void HandleWaveStart(int waveNumber)
        {
            if (WaveManager.Instance == null) return;
            WaveManager.WaveData data = WaveManager.Instance.GetWaveData(waveNumber);
            healthMultiplier = 1f + 0.1f * (waveNumber - 1);
            speedBonus = 0.1f * (waveNumber - 1);
            StartCoroutine(SpawnWave(data));
        }

        private IEnumerator SpawnWave(WaveManager.WaveData data)
        {
            waveActive = true;
            activeEnemies.Clear();
            spawnedThisWave = 0;
            destroyedThisWave = 0;

            if (data.bossWave)
            {
                expectedThisWave = 1;
                SpawnBoss();
                yield break;
            }

            expectedThisWave = data.enemyCount;
            Formation formation = (Formation)Random.Range(0, System.Enum.GetValues(typeof(Formation)).Length);
            List<Vector3> positions = BuildFormation(formation, data.enemyCount);

            for (int i = 0; i < data.enemyCount; i++)
            {
                string type = data.enemyTypes != null && data.enemyTypes.Length > 0
                    ? data.enemyTypes[Random.Range(0, data.enemyTypes.Length)]
                    : "EnemyA";
                SpawnEnemy(type, positions[i]);
                yield return new WaitForSeconds(perEnemyDelay);
            }
        }

        private List<Vector3> BuildFormation(Formation formation, int count)
        {
            List<Vector3> result = new List<Vector3>();
            float topY = GetTopY() + spawnHeightOffset;
            float halfWidth = GetHalfWidth() - spawnEdgePadding;

            switch (formation)
            {
                case Formation.Line:
                    for (int i = 0; i < count; i++)
                    {
                        float x = count > 1 ? Mathf.Lerp(-halfWidth, halfWidth, i / (float)(count - 1)) : 0f;
                        result.Add(new Vector3(x, topY, 0f));
                    }
                    break;

                case Formation.VShape:
                    for (int i = 0; i < count; i++)
                    {
                        int offset = i - count / 2;
                        float x = Mathf.Clamp(offset * formationSpacing, -halfWidth, halfWidth);
                        float y = topY + Mathf.Abs(offset) * formationSpacing * 0.6f;
                        result.Add(new Vector3(x, y, 0f));
                    }
                    break;

                case Formation.Diamond:
                    for (int i = 0; i < count; i++)
                    {
                        float angle = (360f / count) * i * Mathf.Deg2Rad;
                        float radius = formationSpacing * 1.5f;
                        float x = Mathf.Clamp(Mathf.Cos(angle) * radius, -halfWidth, halfWidth);
                        float y = topY + Mathf.Sin(angle) * radius;
                        result.Add(new Vector3(x, y, 0f));
                    }
                    break;

                default: // Random
                    for (int i = 0; i < count; i++)
                    {
                        float x = Random.Range(-halfWidth, halfWidth);
                        result.Add(new Vector3(x, topY + Random.Range(0f, 2f), 0f));
                    }
                    break;
            }
            return result;
        }

        public void SpawnEnemy(string type, Vector3 position)
        {
            if (ObjectPool.Instance == null || !ObjectPool.Instance.HasPool(type)) return;
            GameObject obj = ObjectPool.Instance.GetObject(type, position, Quaternion.identity);
            if (obj == null) return;
            EnemyBase enemy = obj.GetComponent<EnemyBase>();
            if (enemy != null)
            {
                enemy.ApplyDifficulty(healthMultiplier, speedBonus);
                activeEnemies.Add(enemy);
                spawnedThisWave++;
            }
        }

        private void SpawnBoss()
        {
            float topY = GetTopY() + 2f;
            Vector3 pos = new Vector3(0f, topY, 0f);
            if (ObjectPool.Instance == null || !ObjectPool.Instance.HasPool(bossPoolTag)) return;
            GameObject obj = ObjectPool.Instance.GetObject(bossPoolTag, pos, Quaternion.identity);
            if (obj == null) return;
            EnemyBase boss = obj.GetComponent<EnemyBase>();
            if (boss != null)
            {
                boss.ApplyDifficulty(healthMultiplier, 0f);
                activeEnemies.Add(boss);
                spawnedThisWave++;
            }
        }

        public void OnEnemyDestroyed(EnemyBase enemy)
        {
            if (activeEnemies.Remove(enemy))
            {
                destroyedThisWave++;
            }

            if (waveActive && spawnedThisWave >= expectedThisWave && activeEnemies.Count == 0)
            {
                waveActive = false;
                if (WaveManager.Instance != null) WaveManager.Instance.NotifyWaveComplete();
            }
        }

        /// <summary>Kill every active enemy (bomb power-up).</summary>
        public void ClearAllEnemies()
        {
            var snapshot = new List<EnemyBase>(activeEnemies);
            foreach (var enemy in snapshot)
            {
                if (enemy != null && !enemy.IsBoss) enemy.KillFromBomb();
            }
        }

        private float GetTopY()
        {
            if (cam == null) cam = Camera.main;
            if (cam == null) return 6f;
            return cam.ViewportToWorldPoint(new Vector3(0.5f, 1f, Mathf.Abs(cam.transform.position.z))).y;
        }

        private float GetHalfWidth()
        {
            if (cam == null) cam = Camera.main;
            if (cam == null) return 8f;
            return cam.ViewportToWorldPoint(new Vector3(1f, 0.5f, Mathf.Abs(cam.transform.position.z))).x;
        }
    }
}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SpaceShooter.Enemy;

namespace SpaceShooter.Managers
{
    [System.Serializable]
    public class WaveData
    {
        public string waveName;
        public int basicEnemyCount;
        public int fastEnemyCount;
        public int tankEnemyCount;
        public int shooterEnemyCount;
        public bool hasBoss;
        public float spawnDelay = 1f;
        public float waveDelay = 3f;
    }

    public class WaveManager : MonoBehaviour
    {
        public static WaveManager Instance { get; private set; }

        [Header("Wave Settings")]
        [SerializeField] private List<WaveData> predefinedWaves;
        [SerializeField] private bool useInfiniteWaves = true;
        [SerializeField] private float difficultyScaling = 1.2f;

        [Header("Spawn Settings")]
        [SerializeField] private Transform[] spawnPoints;
        [SerializeField] private float minX = -7f;
        [SerializeField] private float maxX = 7f;
        [SerializeField] private float spawnY = 6f;

        [Header("Enemy Prefabs")]
        [SerializeField] private GameObject basicEnemyPrefab;
        [SerializeField] private GameObject fastEnemyPrefab;
        [SerializeField] private GameObject tankEnemyPrefab;
        [SerializeField] private GameObject shooterEnemyPrefab;
        [SerializeField] private GameObject bossPrefab;

        [Header("State")]
        [SerializeField] private int currentWave = 0;
        [SerializeField] private int enemiesAlive = 0;
        [SerializeField] private bool isSpawning = false;
        [SerializeField] private bool waveInProgress = false;

        public event System.Action<int> OnWaveStart;
        public event System.Action<int> OnWaveComplete;
        public event System.Action OnAllWavesComplete;

        public int CurrentWave => currentWave;
        public bool WaveInProgress => waveInProgress;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            InitializeDefaultWaves();
            StartCoroutine(StartWaveSystem());
        }

        private void InitializeDefaultWaves()
        {
            if (predefinedWaves == null || predefinedWaves.Count == 0)
            {
                predefinedWaves = new List<WaveData>
                {
                    new WaveData { waveName = "Wave 1", basicEnemyCount = 5, fastEnemyCount = 0, tankEnemyCount = 0, shooterEnemyCount = 0, hasBoss = false, spawnDelay = 1.5f, waveDelay = 3f },
                    new WaveData { waveName = "Wave 2", basicEnemyCount = 5, fastEnemyCount = 3, tankEnemyCount = 0, shooterEnemyCount = 0, hasBoss = false, spawnDelay = 1.2f, waveDelay = 3f },
                    new WaveData { waveName = "Wave 3", basicEnemyCount = 6, fastEnemyCount = 4, tankEnemyCount = 1, shooterEnemyCount = 0, hasBoss = false, spawnDelay = 1f, waveDelay = 3f },
                    new WaveData { waveName = "Wave 4", basicEnemyCount = 4, fastEnemyCount = 5, tankEnemyCount = 2, shooterEnemyCount = 2, hasBoss = false, spawnDelay = 1f, waveDelay = 3f },
                    new WaveData { waveName = "Boss Wave", basicEnemyCount = 0, fastEnemyCount = 0, tankEnemyCount = 0, shooterEnemyCount = 0, hasBoss = true, spawnDelay = 1f, waveDelay = 5f },
                };
            }
        }

        private IEnumerator StartWaveSystem()
        {
            yield return new WaitForSeconds(2f);
            StartNextWave();
        }

        public void StartNextWave()
        {
            if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
                return;

            currentWave++;
            GameManager.Instance?.SetWave(currentWave);
            OnWaveStart?.Invoke(currentWave);

            WaveData waveData = GetWaveData(currentWave);
            StartCoroutine(SpawnWave(waveData));
        }

        private WaveData GetWaveData(int wave)
        {
            if (wave <= predefinedWaves.Count)
            {
                return predefinedWaves[wave - 1];
            }
            else if (useInfiniteWaves)
            {
                return GenerateInfiniteWave(wave);
            }
            return predefinedWaves[predefinedWaves.Count - 1];
        }

        private WaveData GenerateInfiniteWave(int wave)
        {
            float multiplier = Mathf.Pow(difficultyScaling, wave - predefinedWaves.Count);
            bool isBossWave = wave % 5 == 0;

            return new WaveData
            {
                waveName = $"Wave {wave}",
                basicEnemyCount = isBossWave ? 2 : Mathf.RoundToInt(5 * multiplier),
                fastEnemyCount = isBossWave ? 2 : Mathf.RoundToInt(3 * multiplier),
                tankEnemyCount = isBossWave ? 1 : Mathf.RoundToInt(2 * multiplier),
                shooterEnemyCount = isBossWave ? 1 : Mathf.RoundToInt(2 * multiplier),
                hasBoss = isBossWave,
                spawnDelay = Mathf.Max(0.5f, 1.5f - (wave * 0.05f)),
                waveDelay = 3f
            };
        }

        private IEnumerator SpawnWave(WaveData wave)
        {
            waveInProgress = true;
            isSpawning = true;
            enemiesAlive = 0;

            AudioManager.Instance?.PlaySound("WaveStart");

            yield return new WaitForSeconds(1f);

            for (int i = 0; i < wave.basicEnemyCount; i++)
            {
                SpawnEnemy(basicEnemyPrefab);
                yield return new WaitForSeconds(wave.spawnDelay);
            }

            for (int i = 0; i < wave.fastEnemyCount; i++)
            {
                SpawnEnemy(fastEnemyPrefab);
                yield return new WaitForSeconds(wave.spawnDelay * 0.8f);
            }

            for (int i = 0; i < wave.tankEnemyCount; i++)
            {
                SpawnEnemy(tankEnemyPrefab);
                yield return new WaitForSeconds(wave.spawnDelay * 1.5f);
            }

            for (int i = 0; i < wave.shooterEnemyCount; i++)
            {
                SpawnEnemy(shooterEnemyPrefab);
                yield return new WaitForSeconds(wave.spawnDelay);
            }

            if (wave.hasBoss && bossPrefab != null)
            {
                yield return new WaitForSeconds(2f);
                SpawnBoss();
            }

            isSpawning = false;
        }

        private void SpawnEnemy(GameObject prefab)
        {
            if (prefab == null) return;

            Vector3 spawnPosition;
            if (spawnPoints != null && spawnPoints.Length > 0)
            {
                spawnPosition = spawnPoints[Random.Range(0, spawnPoints.Length)].position;
            }
            else
            {
                spawnPosition = new Vector3(Random.Range(minX, maxX), spawnY, 0);
            }

            GameObject enemy = Instantiate(prefab, spawnPosition, Quaternion.identity);
            enemiesAlive++;

            int healthMultiplier = Mathf.Max(1, currentWave / 5);
            EnemyBase enemyBase = enemy.GetComponent<EnemyBase>();
            if (enemyBase != null)
            {
                enemyBase.Initialize(healthMultiplier, healthMultiplier);
            }
        }

        private void SpawnBoss()
        {
            Vector3 spawnPosition = new Vector3(0, spawnY + 2f, 0);
            GameObject boss = Instantiate(bossPrefab, spawnPosition, Quaternion.identity);
            enemiesAlive++;

            BossEnemy bossEnemy = boss.GetComponent<BossEnemy>();
            if (bossEnemy != null)
            {
                bossEnemy.OnBossDefeated += OnBossDefeated;
            }
        }

        private void OnBossDefeated()
        {
            AudioManager.Instance?.PlaySound("BossDefeat");
        }

        public void OnEnemyDestroyed()
        {
            enemiesAlive--;

            if (enemiesAlive <= 0 && !isSpawning)
            {
                WaveComplete();
            }
        }

        private void WaveComplete()
        {
            waveInProgress = false;
            OnWaveComplete?.Invoke(currentWave);
            AudioManager.Instance?.PlaySound("WaveComplete");

            if (currentWave >= predefinedWaves.Count && !useInfiniteWaves)
            {
                OnAllWavesComplete?.Invoke();
            }
            else
            {
                StartCoroutine(WaveCompleteDelay());
            }
        }

        private IEnumerator WaveCompleteDelay()
        {
            yield return new WaitForSeconds(3f);
            StartNextWave();
        }

        public void ResetWaves()
        {
            StopAllCoroutines();
            currentWave = 0;
            enemiesAlive = 0;
            isSpawning = false;
            waveInProgress = false;
        }
    }
}

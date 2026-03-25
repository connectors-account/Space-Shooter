// =============================================================================
// EnemySpawner.cs — Wave-based enemy spawning system
// =============================================================================
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace SpaceShooter.Enemy
{
    /// <summary>
    /// Defines a single wave of enemies.
    /// </summary>
    [System.Serializable]
    public class WaveData
    {
        public string waveName = "Wave";
        public EnemyWaveEntry[] enemies;
        public float spawnDelay = 1f;
        public float waveDelay = 3f;
        public bool isBossWave = false;
    }

    /// <summary>
    /// Entry for a single enemy type within a wave.
    /// </summary>
    [System.Serializable]
    public class EnemyWaveEntry
    {
        public GameObject enemyPrefab;
        public int count = 3;
        public float spawnInterval = 0.5f;
    }

    /// <summary>
    /// Manages spawning of enemy waves with progressive difficulty.
    /// When all predefined waves are exhausted, generates procedural waves.
    /// </summary>
    public class EnemySpawner : MonoBehaviour
    {
        [Header("Wave Definitions")]
        [SerializeField] private List<WaveData> predefinedWaves = new List<WaveData>();

        [Header("Enemy Prefabs (for procedural waves)")]
        [SerializeField] private GameObject basicEnemyPrefab;
        [SerializeField] private GameObject fastEnemyPrefab;
        [SerializeField] private GameObject tankEnemyPrefab;
        [SerializeField] private GameObject bossPrefab;

        [Header("Spawn Settings")]
        [SerializeField] private float spawnYOffset = 1f;
        [SerializeField] private float bossWaveInterval = 5;

        private int currentWaveIndex;
        private bool isSpawning;
        private bool waitingForBoss;
        private float screenHalfWidth;
        private int totalWavesCompleted;

        /// <summary>Current wave number (1-based) for UI display.</summary>
        public int CurrentWave => totalWavesCompleted + 1;

        private void Start()
        {
            Camera cam = Camera.main;
            if (cam != null)
                screenHalfWidth = cam.orthographicSize * cam.aspect - 1f;
        }

        /// <summary>
        /// Begins spawning waves from the start.
        /// </summary>
        public void StartSpawning()
        {
            currentWaveIndex = 0;
            totalWavesCompleted = 0;
            waitingForBoss = false;
            StopAllCoroutines();
            StartCoroutine(SpawnLoop());
        }

        /// <summary>
        /// Stops all spawning.
        /// </summary>
        public void StopSpawning()
        {
            StopAllCoroutines();
            isSpawning = false;
        }

        /// <summary>
        /// Main spawn loop: iterates through predefined waves, then generates procedural ones.
        /// </summary>
        private IEnumerator SpawnLoop()
        {
            isSpawning = true;
            yield return new WaitForSeconds(2f); // Initial delay

            while (isSpawning)
            {
                WaveData wave;

                if (currentWaveIndex < predefinedWaves.Count)
                {
                    wave = predefinedWaves[currentWaveIndex];
                }
                else
                {
                    wave = GenerateProceduralWave();
                }

                // Announce wave
                Managers.GameManager.Instance?.AnnounceWave(CurrentWave, wave.isBossWave);
                yield return new WaitForSeconds(wave.waveDelay);

                // Spawn all entries in this wave
                yield return StartCoroutine(SpawnWave(wave));

                // Wait for all enemies to be defeated
                yield return StartCoroutine(WaitForEnemiesCleared());

                currentWaveIndex++;
                totalWavesCompleted++;
            }
        }

        /// <summary>
        /// Spawns all enemies defined in a wave.
        /// </summary>
        private IEnumerator SpawnWave(WaveData wave)
        {
            if (wave.enemies == null) yield break;

            foreach (EnemyWaveEntry entry in wave.enemies)
            {
                if (entry.enemyPrefab == null) continue;

                for (int i = 0; i < entry.count; i++)
                {
                    SpawnEnemy(entry.enemyPrefab);
                    yield return new WaitForSeconds(entry.spawnInterval);
                }
                yield return new WaitForSeconds(wave.spawnDelay);
            }
        }

        /// <summary>
        /// Spawns a single enemy at a random X position above the screen.
        /// </summary>
        private void SpawnEnemy(GameObject prefab)
        {
            float spawnX = Random.Range(-screenHalfWidth, screenHalfWidth);
            Camera cam = Camera.main;
            float spawnY = cam != null ? cam.orthographicSize + spawnYOffset : 6f;
            Vector3 pos = new Vector3(spawnX, spawnY, 0f);
            Instantiate(prefab, pos, Quaternion.identity);
        }

        /// <summary>
        /// Waits until all enemies are destroyed.
        /// </summary>
        private IEnumerator WaitForEnemiesCleared()
        {
            yield return new WaitForSeconds(1f);
            while (GameObject.FindGameObjectsWithTag("Enemy").Length > 0)
            {
                yield return new WaitForSeconds(0.5f);
            }
        }

        /// <summary>
        /// Generates a procedural wave that increases in difficulty.
        /// Every bossWaveInterval waves, a boss is spawned.
        /// </summary>
        private WaveData GenerateProceduralWave()
        {
            WaveData wave = new WaveData();
            int difficulty = totalWavesCompleted + 1;
            bool isBoss = difficulty % (int)bossWaveInterval == 0;

            wave.waveName = isBoss ? $"BOSS WAVE {difficulty}" : $"Wave {difficulty}";
            wave.isBossWave = isBoss;
            wave.waveDelay = 2f;
            wave.spawnDelay = 0.8f;

            List<EnemyWaveEntry> entries = new List<EnemyWaveEntry>();

            if (isBoss && bossPrefab != null)
            {
                entries.Add(new EnemyWaveEntry
                {
                    enemyPrefab = bossPrefab,
                    count = 1,
                    spawnInterval = 0f
                });
                // Add some escorts
                if (fastEnemyPrefab != null)
                {
                    entries.Add(new EnemyWaveEntry
                    {
                        enemyPrefab = fastEnemyPrefab,
                        count = Mathf.Min(2 + difficulty / 3, 8),
                        spawnInterval = 0.4f
                    });
                }
            }
            else
            {
                // Mix of enemies based on difficulty
                if (basicEnemyPrefab != null)
                {
                    entries.Add(new EnemyWaveEntry
                    {
                        enemyPrefab = basicEnemyPrefab,
                        count = Mathf.Min(3 + difficulty, 12),
                        spawnInterval = 0.5f
                    });
                }

                if (difficulty >= 2 && fastEnemyPrefab != null)
                {
                    entries.Add(new EnemyWaveEntry
                    {
                        enemyPrefab = fastEnemyPrefab,
                        count = Mathf.Min(1 + difficulty / 2, 8),
                        spawnInterval = 0.4f
                    });
                }

                if (difficulty >= 4 && tankEnemyPrefab != null)
                {
                    entries.Add(new EnemyWaveEntry
                    {
                        enemyPrefab = tankEnemyPrefab,
                        count = Mathf.Min(difficulty / 4, 4),
                        spawnInterval = 1f
                    });
                }
            }

            wave.enemies = entries.ToArray();
            return wave;
        }

        /// <summary>
        /// Called by GameManager when the boss is defeated.
        /// </summary>
        public void OnBossDefeated()
        {
            waitingForBoss = false;
        }
    }
}

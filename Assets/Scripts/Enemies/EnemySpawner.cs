using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace SpaceShooter.Core
{
    [System.Serializable]
    public class EnemyWave
    {
        public string waveName = "Wave";
        public List<EnemySpawnInfo> enemies = new List<EnemySpawnInfo>();
        public float waveDelay = 3f;
        public bool isBossWave = false;
    }
    
    [System.Serializable]
    public class EnemySpawnInfo
    {
        public GameObject enemyPrefab;
        public int count = 1;
        public float spawnInterval = 0.5f;
        public SpawnPattern pattern = SpawnPattern.Random;
    }
    
    public enum SpawnPattern
    {
        Random,
        Left,
        Right,
        Center,
        Formation
    }
    
    /// <summary>
    /// Handles enemy wave spawning and progression
    /// </summary>
    public class EnemySpawner : MonoBehaviour
    {
        [Header("Spawn Settings")]
        [SerializeField] private float spawnYPosition = 6f;
        [SerializeField] private float spawnXMin = -7f;
        [SerializeField] private float spawnXMax = 7f;
        [SerializeField] private float initialDelay = 2f;
        
        [Header("Wave Configuration")]
        [SerializeField] private List<EnemyWave> waves = new List<EnemyWave>();
        [SerializeField] private bool loopWaves = true;
        [SerializeField] private float difficultyScaling = 0.1f;
        
        [Header("Default Enemy Prefabs")]
        [SerializeField] private GameObject basicEnemyPrefab;
        [SerializeField] private GameObject zigZagEnemyPrefab;
        [SerializeField] private GameObject diveEnemyPrefab;
        [SerializeField] private GameObject bossEnemyPrefab;
        
        private int currentWaveIndex = 0;
        private int loopCount = 0;
        private List<EnemyBase> activeEnemies = new List<EnemyBase>();
        private bool isSpawning = false;
        private Coroutine spawnCoroutine;
        
        public int CurrentWave => currentWaveIndex + 1;
        public int ActiveEnemyCount => activeEnemies.Count;
        public bool IsSpawning => isSpawning;
        
        private void Start()
        {
            if (waves.Count == 0)
            {
                CreateDefaultWaves();
            }
            
            StartSpawning();
        }
        
        private void CreateDefaultWaves()
        {
            // Wave 1: Basic enemies
            EnemyWave wave1 = new EnemyWave
            {
                waveName = "Wave 1 - Basic",
                waveDelay = 3f
            };
            wave1.enemies.Add(new EnemySpawnInfo
            {
                enemyPrefab = basicEnemyPrefab,
                count = 5,
                spawnInterval = 1f,
                pattern = SpawnPattern.Random
            });
            waves.Add(wave1);
            
            // Wave 2: Mixed enemies
            EnemyWave wave2 = new EnemyWave
            {
                waveName = "Wave 2 - Mixed",
                waveDelay = 3f
            };
            wave2.enemies.Add(new EnemySpawnInfo
            {
                enemyPrefab = basicEnemyPrefab,
                count = 3,
                spawnInterval = 1f,
                pattern = SpawnPattern.Left
            });
            wave2.enemies.Add(new EnemySpawnInfo
            {
                enemyPrefab = zigZagEnemyPrefab,
                count = 3,
                spawnInterval = 0.8f,
                pattern = SpawnPattern.Right
            });
            waves.Add(wave2);
            
            // Wave 3: ZigZag focus
            EnemyWave wave3 = new EnemyWave
            {
                waveName = "Wave 3 - ZigZag",
                waveDelay = 3f
            };
            wave3.enemies.Add(new EnemySpawnInfo
            {
                enemyPrefab = zigZagEnemyPrefab,
                count = 6,
                spawnInterval = 0.7f,
                pattern = SpawnPattern.Random
            });
            waves.Add(wave3);
            
            // Wave 4: Dive attackers
            EnemyWave wave4 = new EnemyWave
            {
                waveName = "Wave 4 - Divers",
                waveDelay = 3f
            };
            wave4.enemies.Add(new EnemySpawnInfo
            {
                enemyPrefab = diveEnemyPrefab,
                count = 5,
                spawnInterval = 1.2f,
                pattern = SpawnPattern.Random
            });
            waves.Add(wave4);
            
            // Wave 5: Boss
            EnemyWave wave5 = new EnemyWave
            {
                waveName = "Wave 5 - BOSS",
                waveDelay = 4f,
                isBossWave = true
            };
            wave5.enemies.Add(new EnemySpawnInfo
            {
                enemyPrefab = bossEnemyPrefab,
                count = 1,
                spawnInterval = 0f,
                pattern = SpawnPattern.Center
            });
            waves.Add(wave5);
        }
        
        public void StartSpawning()
        {
            if (spawnCoroutine != null)
            {
                StopCoroutine(spawnCoroutine);
            }
            spawnCoroutine = StartCoroutine(SpawnWaves());
        }
        
        public void StopSpawning()
        {
            if (spawnCoroutine != null)
            {
                StopCoroutine(spawnCoroutine);
                spawnCoroutine = null;
            }
            isSpawning = false;
        }
        
        private IEnumerator SpawnWaves()
        {
            yield return new WaitForSeconds(initialDelay);
            
            while (true)
            {
                if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing)
                {
                    yield return new WaitForSeconds(0.1f);
                    continue;
                }
                
                if (currentWaveIndex >= waves.Count)
                {
                    if (loopWaves)
                    {
                        currentWaveIndex = 0;
                        loopCount++;
                    }
                    else
                    {
                        // All waves completed
                        if (GameManager.Instance != null)
                        {
                            GameManager.Instance.Victory();
                        }
                        yield break;
                    }
                }
                
                EnemyWave currentWave = waves[currentWaveIndex];
                
                // Update game manager wave
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.SetWave(currentWaveIndex + 1 + (loopCount * waves.Count));
                }
                
                Debug.Log($"Starting {currentWave.waveName}");
                isSpawning = true;
                
                // Spawn all enemy groups in this wave
                foreach (EnemySpawnInfo enemyInfo in currentWave.enemies)
                {
                    yield return StartCoroutine(SpawnEnemyGroup(enemyInfo));
                }
                
                isSpawning = false;
                
                // Wait for all enemies to be destroyed
                yield return new WaitUntil(() => activeEnemies.Count == 0);
                
                // Wait before next wave
                yield return new WaitForSeconds(currentWave.waveDelay);
                
                currentWaveIndex++;
            }
        }
        
        private IEnumerator SpawnEnemyGroup(EnemySpawnInfo enemyInfo)
        {
            if (enemyInfo.enemyPrefab == null) yield break;
            
            float difficultyMultiplier = 1f + (loopCount * difficultyScaling);
            
            for (int i = 0; i < enemyInfo.count; i++)
            {
                Vector3 spawnPosition = GetSpawnPosition(enemyInfo.pattern, i, enemyInfo.count);
                
                GameObject enemy = Instantiate(enemyInfo.enemyPrefab, spawnPosition, Quaternion.identity);
                
                EnemyBase enemyBase = enemy.GetComponent<EnemyBase>();
                if (enemyBase != null)
                {
                    enemyBase.OnEnemyDestroyed += OnEnemyDestroyed;
                    activeEnemies.Add(enemyBase);
                }
                
                yield return new WaitForSeconds(enemyInfo.spawnInterval / difficultyMultiplier);
            }
        }
        
        private Vector3 GetSpawnPosition(SpawnPattern pattern, int index, int total)
        {
            float x = 0f;
            
            switch (pattern)
            {
                case SpawnPattern.Random:
                    x = Random.Range(spawnXMin, spawnXMax);
                    break;
                    
                case SpawnPattern.Left:
                    x = Random.Range(spawnXMin, spawnXMin / 2f);
                    break;
                    
                case SpawnPattern.Right:
                    x = Random.Range(spawnXMax / 2f, spawnXMax);
                    break;
                    
                case SpawnPattern.Center:
                    x = 0f;
                    break;
                    
                case SpawnPattern.Formation:
                    float spacing = (spawnXMax - spawnXMin) / (total + 1);
                    x = spawnXMin + spacing * (index + 1);
                    break;
            }
            
            return new Vector3(x, spawnYPosition, 0f);
        }
        
        private void OnEnemyDestroyed(EnemyBase enemy)
        {
            enemy.OnEnemyDestroyed -= OnEnemyDestroyed;
            activeEnemies.Remove(enemy);
        }
        
        public void ClearAllEnemies()
        {
            foreach (EnemyBase enemy in activeEnemies)
            {
                if (enemy != null)
                {
                    Destroy(enemy.gameObject);
                }
            }
            activeEnemies.Clear();
        }
    }
}

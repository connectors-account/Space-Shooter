using System.Collections;
using System.Collections.Generic;
using SpaceShooter.Core;
using UnityEngine;

namespace SpaceShooter.Enemy
{
    public class EnemySpawner : MonoBehaviour
    {
        [System.Serializable]
        public class WaveConfig
        {
            public int waveNumber;
            public float spawnInterval = 0.8f;
            public float speedMultiplier = 1f;
            public int healthBonus = 0;
            public int basicCount = 8;
            public int fastCount = 0;
            public int tankCount = 0;
        }

        [System.Serializable]
        public class EnemyPrefabMap
        {
            public EnemyType type;
            public GameObject prefab;
        }

        [SerializeField] private EnemyPrefabMap[] enemyPrefabs;
        [SerializeField] private Transform enemyParent;
        [SerializeField] private float topSpawnPadding = 0.8f;
        [SerializeField] private float sideSpawnPadding = 0.8f;
        [SerializeField] private WaveConfig[] waves = new WaveConfig[5];

        private readonly List<GameObject> liveEnemies = new List<GameObject>();
        private readonly Dictionary<EnemyType, GameObject> prefabByType = new Dictionary<EnemyType, GameObject>();

        private Camera cam;
        private int waveIndex;
        private bool spawning;

        public bool AllWavesCompleted => waveIndex >= waves.Length && liveEnemies.Count == 0 && !spawning;

        private void Awake()
        {
            cam = Camera.main;
            foreach (EnemyPrefabMap map in enemyPrefabs)
            {
                if (map != null && map.prefab != null && !prefabByType.ContainsKey(map.type))
                {
                    prefabByType.Add(map.type, map.prefab);
                }
            }

            if (waves == null || waves.Length != 5)
            {
                waves = BuildDefaultWaves();
            }
        }

        private void Update()
        {
            if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Playing)
            {
                return;
            }

            liveEnemies.RemoveAll(enemy => enemy == null);

            if (!spawning && waveIndex < waves.Length && liveEnemies.Count == 0)
            {
                StartCoroutine(SpawnWave(waves[waveIndex]));
                waveIndex++;
            }

            if (AllWavesCompleted)
            {
                GameManager.Instance.GameOver();
            }
        }

        private IEnumerator SpawnWave(WaveConfig wave)
        {
            spawning = true;
            GameManager.Instance?.SetWave(wave.waveNumber);

            List<EnemyType> spawnList = new List<EnemyType>();
            AddType(spawnList, EnemyType.Basic, wave.basicCount);
            AddType(spawnList, EnemyType.Fast, wave.fastCount);
            AddType(spawnList, EnemyType.Tank, wave.tankCount);

            Shuffle(spawnList);

            foreach (EnemyType enemyType in spawnList)
            {
                SpawnEnemy(enemyType, wave.speedMultiplier, wave.healthBonus);
                yield return new WaitForSeconds(wave.spawnInterval);
            }

            spawning = false;
        }

        private void SpawnEnemy(EnemyType type, float speedMultiplier, int healthBonus)
        {
            if (!prefabByType.TryGetValue(type, out GameObject prefab) || prefab == null)
            {
                return;
            }

            Vector2 min = Core.ScreenBounds.MinWorld(cam);
            Vector2 max = Core.ScreenBounds.MaxWorld(cam);
            float x = Random.Range(min.x + sideSpawnPadding, max.x - sideSpawnPadding);
            float y = max.y + topSpawnPadding;

            GameObject enemyObj = Instantiate(prefab, new Vector3(x, y, 0f), Quaternion.identity, enemyParent);
            EnemyController enemy = enemyObj.GetComponent<EnemyController>();
            if (enemy != null)
            {
                enemy.Initialize(type, speedMultiplier, healthBonus);
            }
            liveEnemies.Add(enemyObj);
        }

        private static void AddType(List<EnemyType> list, EnemyType type, int count)
        {
            for (int i = 0; i < count; i++)
            {
                list.Add(type);
            }
        }

        private static void Shuffle<T>(IList<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                T tmp = list[i];
                list[i] = list[j];
                list[j] = tmp;
            }
        }

        private static WaveConfig[] BuildDefaultWaves()
        {
            return new[]
            {
                new WaveConfig { waveNumber = 1, spawnInterval = 0.8f, speedMultiplier = 1f, healthBonus = 0, basicCount = 8, fastCount = 2, tankCount = 0 },
                new WaveConfig { waveNumber = 2, spawnInterval = 0.75f, speedMultiplier = 1.1f, healthBonus = 0, basicCount = 9, fastCount = 4, tankCount = 1 },
                new WaveConfig { waveNumber = 3, spawnInterval = 0.65f, speedMultiplier = 1.15f, healthBonus = 1, basicCount = 10, fastCount = 6, tankCount = 2 },
                new WaveConfig { waveNumber = 4, spawnInterval = 0.58f, speedMultiplier = 1.25f, healthBonus = 2, basicCount = 12, fastCount = 8, tankCount = 3 },
                new WaveConfig { waveNumber = 5, spawnInterval = 0.5f, speedMultiplier = 1.35f, healthBonus = 3, basicCount = 14, fastCount = 10, tankCount = 4 }
            };
        }
    }
}

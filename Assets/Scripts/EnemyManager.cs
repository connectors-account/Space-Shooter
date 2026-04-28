using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public enum MovementPattern
    {
        Straight,
        ZigZag,
        Sine,
        Dive
    }

    [System.Serializable]
    public class WaveConfig
    {
        public int enemyCount = 8;
        public float spawnInterval = 0.8f;
        public float enemySpeedMultiplier = 1f;
        public float fireChancePerSecond = 0.2f;
        public int enemyHealth = 30;
        public int scorePerKill = 100;
        public MovementPattern[] allowedPatterns = { MovementPattern.Straight };
    }

    public static EnemyManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private GameObject enemyPrefab;

    [Header("Spawn Area")]
    [SerializeField] private float spawnMinX = -8.5f;
    [SerializeField] private float spawnMaxX = 8.5f;
    [SerializeField] private float spawnY = 6f;

    [Header("Waves")]
    [SerializeField] private List<WaveConfig> waves = new List<WaveConfig>();

    private readonly List<EnemyRuntime> aliveEnemies = new List<EnemyRuntime>();
    private Coroutine waveRoutine;

    public int CurrentWaveIndex { get; private set; }
    public int TotalWaves => waves.Count;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (waves.Count == 0)
        {
            InitializeDefaultWaves();
        }
    }

    public void BeginWaves()
    {
        StopAllCoroutines();
        CurrentWaveIndex = 0;
        DespawnAllEnemies();
        waveRoutine = StartCoroutine(WaveRoutine());
    }

    public void StopWaves()
    {
        if (waveRoutine != null)
        {
            StopCoroutine(waveRoutine);
            waveRoutine = null;
        }

        DespawnAllEnemies();
    }

    private IEnumerator WaveRoutine()
    {
        while (CurrentWaveIndex < waves.Count)
        {
            WaveConfig wave = waves[CurrentWaveIndex];
            int waveNumber = CurrentWaveIndex + 1;

            GameManager.Instance?.OnWaveStarted(waveNumber);

            for (int i = 0; i < wave.enemyCount; i++)
            {
                if (!GameManager.Instance.IsGameplayActive)
                {
                    yield break;
                }

                SpawnEnemy(wave);
                yield return new WaitForSeconds(wave.spawnInterval);
            }

            while (aliveEnemies.Count > 0)
            {
                yield return null;
            }

            GameManager.Instance?.OnWaveCompleted(waveNumber);
            CurrentWaveIndex++;
            yield return new WaitForSeconds(2f);
        }

        GameManager.Instance?.OnAllWavesCompleted();
        waveRoutine = null;
    }

    private void SpawnEnemy(WaveConfig wave)
    {
        if (enemyPrefab == null)
        {
            Debug.LogWarning("Enemy prefab is missing on EnemyManager.");
            return;
        }

        float x = Random.Range(spawnMinX, spawnMaxX);
        Vector3 spawnPos = new Vector3(x, spawnY, 0f);
        GameObject enemyObject = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);

        EnemyRuntime runtime = enemyObject.GetComponent<EnemyRuntime>();
        if (runtime == null)
        {
            runtime = enemyObject.AddComponent<EnemyRuntime>();
        }

        MovementPattern pattern = wave.allowedPatterns[Random.Range(0, wave.allowedPatterns.Length)];
        runtime.Initialize(this, wave, pattern);
        aliveEnemies.Add(runtime);
    }

    private void DespawnAllEnemies()
    {
        for (int i = aliveEnemies.Count - 1; i >= 0; i--)
        {
            if (aliveEnemies[i] != null)
            {
                Destroy(aliveEnemies[i].gameObject);
            }
        }

        aliveEnemies.Clear();
    }

    private void InitializeDefaultWaves()
    {
        waves = new List<WaveConfig>
        {
            new WaveConfig { enemyCount = 8,  spawnInterval = 0.9f, enemySpeedMultiplier = 1.0f, fireChancePerSecond = 0.10f, enemyHealth = 30, scorePerKill = 100, allowedPatterns = new[] { MovementPattern.Straight, MovementPattern.ZigZag } },
            new WaveConfig { enemyCount = 12, spawnInterval = 0.75f, enemySpeedMultiplier = 1.2f, fireChancePerSecond = 0.14f, enemyHealth = 35, scorePerKill = 120, allowedPatterns = new[] { MovementPattern.Straight, MovementPattern.ZigZag, MovementPattern.Sine } },
            new WaveConfig { enemyCount = 16, spawnInterval = 0.65f, enemySpeedMultiplier = 1.35f, fireChancePerSecond = 0.18f, enemyHealth = 40, scorePerKill = 140, allowedPatterns = new[] { MovementPattern.ZigZag, MovementPattern.Sine, MovementPattern.Dive } },
            new WaveConfig { enemyCount = 20, spawnInterval = 0.55f, enemySpeedMultiplier = 1.55f, fireChancePerSecond = 0.24f, enemyHealth = 45, scorePerKill = 170, allowedPatterns = new[] { MovementPattern.Straight, MovementPattern.Sine, MovementPattern.Dive } },
            new WaveConfig { enemyCount = 24, spawnInterval = 0.45f, enemySpeedMultiplier = 1.8f, fireChancePerSecond = 0.30f, enemyHealth = 55, scorePerKill = 220, allowedPatterns = new[] { MovementPattern.ZigZag, MovementPattern.Sine, MovementPattern.Dive } }
        };
    }

    public class EnemyRuntime : MonoBehaviour
    {
        private EnemyManager manager;
        private WaveConfig wave;
        private MovementPattern pattern;
        private float speed;
        private float spawnTime;
        private int health;

        public void Initialize(EnemyManager sourceManager, WaveConfig config, MovementPattern movementPattern)
        {
            manager = sourceManager;
            wave = config;
            pattern = movementPattern;
            speed = 2.5f * wave.enemySpeedMultiplier;
            health = wave.enemyHealth;
            spawnTime = Time.time;

            gameObject.tag = "Enemy";
        }

        private void Update()
        {
            Move();
            HandleEnemyShooting();

            if (transform.position.y < -7f)
            {
                Destroy(gameObject);
            }
        }

        private void Move()
        {
            Vector3 movement = Vector3.down * speed * Time.deltaTime;
            float elapsed = Time.time - spawnTime;

            switch (pattern)
            {
                case MovementPattern.ZigZag:
                    movement.x = Mathf.Sin(elapsed * 6f) * 3.2f * Time.deltaTime;
                    break;
                case MovementPattern.Sine:
                    movement.x = Mathf.Sin(elapsed * 2.5f) * 1.8f * Time.deltaTime;
                    break;
                case MovementPattern.Dive:
                    movement.x = Mathf.Sin(elapsed * 4f) * 0.7f * Time.deltaTime;
                    movement.y = -(speed + elapsed * 0.35f) * Time.deltaTime;
                    break;
            }

            transform.position += movement;
        }

        private void HandleEnemyShooting()
        {
            if (!GameManager.Instance.IsGameplayActive)
            {
                return;
            }

            float chance = wave.fireChancePerSecond * Time.deltaTime;
            if (Random.value <= chance)
            {
                BulletSystem.SpawnEnemyBullet(transform.position + Vector3.down * 0.6f, Vector2.down);
            }
        }

        public void TakeDamage(int damage)
        {
            health -= damage;
            if (health <= 0)
            {
                Die();
            }
        }

        private void Die()
        {
            GameManager.Instance?.AddScore(wave.scorePerKill);
            PowerUpSystem.Instance?.TrySpawnPowerUp(transform.position);
            AudioManager.Instance?.PlaySfx(AudioSfx.Explosion);
            Destroy(gameObject);
        }

        private void OnDestroy()
        {
            if (manager != null)
            {
                manager.aliveEnemies.Remove(this);
            }
        }
    }
}

using System.Collections;
using UnityEngine;

namespace SpaceShooter
{
    /// <summary>
    /// Central game orchestration: score, waves, game state transitions, and win/lose flow.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public enum GameState
        {
            MainMenu,
            Playing,
            Paused,
            GameOver
        }

        public static GameManager Instance { get; private set; }

        [Header("Scene References")]
        [SerializeField] private EnemySpawner enemySpawner;
        [SerializeField] private PlayerController player;

        [Header("Wave Settings")]
        [SerializeField] private int initialWave = 1;
        [SerializeField] private int baseEnemiesPerWave = 6;
        [SerializeField] private int enemiesPerWaveIncrement = 2;
        [SerializeField] private float timeBetweenWaves = 2f;

        [Header("Power-Up Drops")]
        [SerializeField] private PowerUpController[] powerUpPrefabs;
        [SerializeField, Range(0f, 1f)] private float powerUpDropChance = 0.18f;

        [Header("State (Debug)")]
        [SerializeField] private GameState gameState = GameState.Playing;
        [SerializeField] private int score;
        [SerializeField] private int currentWave;
        [SerializeField] private int enemiesAlive;
        [SerializeField] private int enemiesLeftToSpawn;

        public bool IsGameplayActive => gameState == GameState.Playing;
        public int Score => score;
        public int CurrentWave => currentWave;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void Start()
        {
            if (enemySpawner == null)
            {
                enemySpawner = FindObjectOfType<EnemySpawner>();
            }

            if (player == null)
            {
                player = FindObjectOfType<PlayerController>();
            }

            StartGameplaySession();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                TogglePause();
            }
        }

        public void StartGameplaySession()
        {
            score = 0;
            currentWave = initialWave - 1;
            enemiesAlive = 0;
            enemiesLeftToSpawn = 0;
            gameState = GameState.Playing;

            Time.timeScale = 1f;
            UIManager.Instance?.RefreshAll(score, player != null ? player.CurrentHealth : 0, player != null ? player.MaxHealth : 1, initialWave);
            UIManager.Instance?.ShowGameplayHud();
            MenuManager.Instance?.HideAllOverlays();

            AudioManager.Instance?.PlayMusicLoop();
            StartNextWave();
        }

        public void StartNextWave()
        {
            if (!IsGameplayActive || enemySpawner == null)
            {
                return;
            }

            currentWave += 1;
            int enemiesThisWave = baseEnemiesPerWave + (currentWave - 1) * enemiesPerWaveIncrement;
            enemiesLeftToSpawn = enemiesThisWave;

            UIManager.Instance?.UpdateWave(currentWave);
            AudioManager.Instance?.PlaySfx(AudioManager.SfxType.WaveStart);

            enemySpawner.BeginWave(currentWave, enemiesThisWave);
        }

        public void RegisterEnemySpawned()
        {
            enemiesAlive += 1;
            enemiesLeftToSpawn = Mathf.Max(0, enemiesLeftToSpawn - 1);
        }

        public void RegisterEnemyDestroyed(int scoreReward, Vector3 enemyPosition)
        {
            enemiesAlive = Mathf.Max(0, enemiesAlive - 1);
            score += Mathf.Max(0, scoreReward);
            UIManager.Instance?.UpdateScore(score);

            TrySpawnPowerUp(enemyPosition);
            CheckWaveCompletion();
        }

        public void RegisterEnemyDespawned()
        {
            enemiesAlive = Mathf.Max(0, enemiesAlive - 1);
            CheckWaveCompletion();
        }

        public void HandlePlayerDefeated()
        {
            if (gameState == GameState.GameOver)
            {
                return;
            }

            gameState = GameState.GameOver;
            enemySpawner?.StopWaveSpawning();
            AudioManager.Instance?.PlaySfx(AudioManager.SfxType.GameOver);

            int highScore = Mathf.Max(PlayerPrefs.GetInt("HighScore", 0), score);
            PlayerPrefs.SetInt("HighScore", highScore);
            PlayerPrefs.Save();

            UIManager.Instance?.ShowGameOver(score, highScore, currentWave);
            MenuManager.Instance?.ShowGameOverOverlay();
        }

        public void TogglePause()
        {
            if (gameState == GameState.GameOver)
            {
                return;
            }

            if (gameState == GameState.Playing)
            {
                gameState = GameState.Paused;
                Time.timeScale = 0f;
                MenuManager.Instance?.ShowPauseOverlay();
            }
            else if (gameState == GameState.Paused)
            {
                ResumeFromPause();
            }
        }

        public void ResumeFromPause()
        {
            if (gameState != GameState.Paused)
            {
                return;
            }

            gameState = GameState.Playing;
            Time.timeScale = 1f;
            MenuManager.Instance?.HidePauseOverlay();
        }

        private void CheckWaveCompletion()
        {
            if (!IsGameplayActive)
            {
                return;
            }

            if (enemiesAlive == 0 && enemiesLeftToSpawn == 0 && (enemySpawner == null || !enemySpawner.IsWaveSpawning))
            {
                StartCoroutine(BeginNextWaveAfterDelay());
            }
        }

        private IEnumerator BeginNextWaveAfterDelay()
        {
            UIManager.Instance?.ShowWaveCompleteBanner(currentWave);
            yield return new WaitForSeconds(timeBetweenWaves);

            if (IsGameplayActive)
            {
                StartNextWave();
            }
        }

        private void TrySpawnPowerUp(Vector3 enemyPosition)
        {
            if (powerUpPrefabs == null || powerUpPrefabs.Length == 0)
            {
                return;
            }

            if (Random.value > powerUpDropChance)
            {
                return;
            }

            PowerUpController prefab = powerUpPrefabs[Random.Range(0, powerUpPrefabs.Length)];
            if (prefab != null)
            {
                Instantiate(prefab, enemyPosition, Quaternion.identity);
            }
        }
    }
}

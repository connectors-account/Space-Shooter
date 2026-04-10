using UnityEngine;
using UnityEngine.SceneManagement;
using SpaceShooter.InputSystem;

namespace SpaceShooter.Managers
{
    /// <summary>
    /// Central game state manager. Handles score, wave progression, game states.
    /// Singleton pattern — persists across scenes.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        // ---- Singleton ----
        public static GameManager Instance { get; private set; }

        // ---- Game States ----
        public enum GameState
        {
            MainMenu,
            Playing,
            Paused,
            GameOver
        }

        [Header("Game Settings")]
        [SerializeField] private int startingWave = 1;
        [SerializeField] private int enemiesPerWaveBase = 5;
        [SerializeField] private int enemiesPerWaveIncrement = 3;
        [SerializeField] private float waveCooldown = 3f;

        // ---- Runtime State ----
        private GameState currentState = GameState.MainMenu;
        private int score;
        private int currentWave;
        private int enemiesRemainingInWave;
        private int totalEnemiesKilledInWave;
        private int highScore;

        // ---- Public Properties ----
        public GameState CurrentState => currentState;
        public int Score => score;
        public int CurrentWave => currentWave;
        public int HighScore => highScore;
        public int EnemiesRemainingInWave => enemiesRemainingInWave;

        // ---- Events ----
        public event System.Action<int> OnScoreChanged;
        public event System.Action<int> OnWaveChanged;
        public event System.Action<GameState> OnGameStateChanged;
        public event System.Action OnWaveComplete;

        private void Awake()
        {
            // Singleton setup
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Load high score from PlayerPrefs
            highScore = PlayerPrefs.GetInt("HighScore", 0);
        }

        // ========== STATE MANAGEMENT ==========

        /// <summary>Starts a new game from wave 1.</summary>
        public void StartGame()
        {
            score = 0;
            currentWave = 0;
            totalEnemiesKilledInWave = 0;

            SetState(GameState.Playing);
            OnScoreChanged?.Invoke(score);

            // Start first wave
            StartNextWave();
        }

        /// <summary>Pauses or unpauses the game.</summary>
        public void TogglePause()
        {
            if (currentState == GameState.Playing)
            {
                SetState(GameState.Paused);
                Time.timeScale = 0f;
            }
            else if (currentState == GameState.Paused)
            {
                SetState(GameState.Playing);
                Time.timeScale = 1f;
            }
        }

        /// <summary>Triggers game over state.</summary>
        public void GameOver()
        {
            SetState(GameState.GameOver);
            Time.timeScale = 1f;

            // Save high score
            if (score > highScore)
            {
                highScore = score;
                PlayerPrefs.SetInt("HighScore", highScore);
                PlayerPrefs.Save();
            }
        }

        /// <summary>Returns to main menu.</summary>
        public void ReturnToMenu()
        {
            SetState(GameState.MainMenu);
            Time.timeScale = 1f;
            score = 0;
            currentWave = 0;
        }

        /// <summary>Restarts the game.</summary>
        public void RestartGame()
        {
            Time.timeScale = 1f;
            StartGame();
        }

        private void SetState(GameState newState)
        {
            currentState = newState;
            OnGameStateChanged?.Invoke(currentState);
        }

        // ========== SCORE ==========

        /// <summary>Add to the player's score.</summary>
        public void AddScore(int points)
        {
            if (currentState != GameState.Playing) return;

            score += points;
            OnScoreChanged?.Invoke(score);
        }

        // ========== WAVE MANAGEMENT ==========

        /// <summary>Starts the next wave.</summary>
        public void StartNextWave()
        {
            currentWave++;
            totalEnemiesKilledInWave = 0;
            enemiesRemainingInWave = enemiesPerWaveBase + (currentWave - 1) * enemiesPerWaveIncrement;

            OnWaveChanged?.Invoke(currentWave);

            // Tell SpawnManager to start spawning
            SpawnManager spawner = FindObjectOfType<SpawnManager>();
            if (spawner != null)
            {
                spawner.StartWave(currentWave, enemiesRemainingInWave);
            }
        }

        /// <summary>Called when an enemy is destroyed during a wave.</summary>
        public void OnEnemyKilled()
        {
            totalEnemiesKilledInWave++;
            enemiesRemainingInWave--;

            if (enemiesRemainingInWave <= 0)
            {
                OnWaveComplete?.Invoke();
                // Start next wave after cooldown
                StartCoroutine(WaveCooldownRoutine());
            }
        }

        private System.Collections.IEnumerator WaveCooldownRoutine()
        {
            yield return new WaitForSeconds(waveCooldown);

            if (currentState == GameState.Playing)
            {
                StartNextWave();
            }
        }

        // ========== UTILITY ==========

        /// <summary>Get total enemies for a given wave number.</summary>
        public int GetTotalEnemiesForWave(int wave)
        {
            return enemiesPerWaveBase + (wave - 1) * enemiesPerWaveIncrement;
        }

        private void Update()
        {
            bool pausePressed = InputHandler.Instance != null
                ? InputHandler.Instance.PausePressedThisFrame
                : Input.GetKeyDown(KeyCode.Escape);

            if (pausePressed && (currentState == GameState.Playing || currentState == GameState.Paused))
            {
                TogglePause();
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }
    }
}

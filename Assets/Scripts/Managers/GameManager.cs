using UnityEngine;
using UnityEngine.SceneManagement;

namespace SpaceShooter.Managers
{
    public enum GameState
    {
        MainMenu,
        Playing,
        Paused,
        GameOver
    }

    /// <summary>
    /// Central game manager. Tracks score, state, and coordinates between systems.
    /// Persists across scene loads via DontDestroyOnLoad.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("References")]
        [SerializeField] private Player.PlayerController playerRef;

        // State
        private GameState currentState = GameState.MainMenu;
        private int score;
        private int highScore;

        // Events (UI listens to these)
        public System.Action<int> OnScoreChanged;
        public System.Action<int, int> OnHealthChanged; // current, max
        public System.Action<int> OnWaveChanged;
        public System.Action<GameState> OnGameStateChanged;
        public System.Action<int> OnGameOver; // final score

        // Properties
        public GameState CurrentState => currentState;
        public int Score => score;
        public int HighScore => highScore;

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
            highScore = PlayerPrefs.GetInt("HighScore", 0);
        }

        private void Update()
        {
            if (currentState == GameState.Playing && Input.GetKeyDown(KeyCode.Escape))
            {
                PauseGame();
            }
            else if (currentState == GameState.Paused && Input.GetKeyDown(KeyCode.Escape))
            {
                ResumeGame();
            }
        }

        public void StartGame()
        {
            score = 0;
            currentState = GameState.Playing;
            Time.timeScale = 1f;

            OnScoreChanged?.Invoke(score);
            OnGameStateChanged?.Invoke(currentState);

            // Find or set player reference
            if (playerRef == null)
            {
                playerRef = FindFirstObjectByType<Player.PlayerController>();
            }

            if (playerRef != null)
            {
                playerRef.ResetPlayer();
                OnHealthChanged?.Invoke(playerRef.CurrentLives, playerRef.MaxLives);
            }

            // Start wave spawner
            WaveSpawner spawner = FindFirstObjectByType<WaveSpawner>();
            if (spawner != null)
            {
                spawner.StartSpawning();
            }
        }

        public void PauseGame()
        {
            if (currentState != GameState.Playing) return;
            currentState = GameState.Paused;
            Time.timeScale = 0f;
            OnGameStateChanged?.Invoke(currentState);
        }

        public void ResumeGame()
        {
            if (currentState != GameState.Paused) return;
            currentState = GameState.Playing;
            Time.timeScale = 1f;
            OnGameStateChanged?.Invoke(currentState);
        }

        public void AddScore(int amount)
        {
            if (currentState != GameState.Playing) return;
            score += amount;
            OnScoreChanged?.Invoke(score);
        }

        public void OnPlayerHealthChanged(int current, int max)
        {
            OnHealthChanged?.Invoke(current, max);
        }

        public void OnPlayerDeath()
        {
            currentState = GameState.GameOver;
            Time.timeScale = 0f;

            if (score > highScore)
            {
                highScore = score;
                PlayerPrefs.SetInt("HighScore", highScore);
                PlayerPrefs.Save();
            }

            OnGameOver?.Invoke(score);
            OnGameStateChanged?.Invoke(currentState);
        }

        public void OnWaveStarted(int waveNumber)
        {
            OnWaveChanged?.Invoke(waveNumber);
        }

        public void RestartGame()
        {
            Time.timeScale = 1f;

            // Return everything to pool
            ObjectPoolManager.Instance?.ReturnAllToPool();

            StartGame();
        }

        public void GoToMainMenu()
        {
            Time.timeScale = 1f;
            currentState = GameState.MainMenu;
            OnGameStateChanged?.Invoke(currentState);

            // Return everything to pool
            ObjectPoolManager.Instance?.ReturnAllToPool();

            // Stop spawner
            WaveSpawner spawner = FindFirstObjectByType<WaveSpawner>();
            if (spawner != null)
                spawner.StopSpawning();
        }

        public void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}

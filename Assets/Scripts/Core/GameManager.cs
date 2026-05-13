// ============================================================================
// GameManager.cs — Central game state controller (singleton)
// Manages game states, scoring, wave progression, and acts as the hub
// that other systems query for current game status.
// ============================================================================
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SpaceShooter.Core
{
    /// <summary>
    /// Possible states the game can be in at any moment.
    /// </summary>
    public enum GameState
    {
        MainMenu,
        Playing,
        Paused,
        GameOver
    }

    public class GameManager : MonoBehaviour
    {
        // ---- Singleton ----
        public static GameManager Instance { get; private set; }

        // ---- Events (other scripts subscribe to these) ----
        public event System.Action<GameState> OnGameStateChanged;
        public event System.Action<int> OnScoreChanged;
        public event System.Action<int> OnWaveChanged;
        public event System.Action<int> OnComboChanged;

        // ---- Public read-only state ----
        public GameState CurrentState { get; private set; } = GameState.MainMenu;
        public int Score { get; private set; }
        public int HighScore { get; private set; }
        public int CurrentWave { get; private set; }
        public int ComboMultiplier { get; private set; } = 1;

        // ---- Combo settings ----
        [Header("Combo Settings")]
        [SerializeField] private float comboTimeWindow = 2f;   // seconds between kills to keep combo
        [SerializeField] private int maxCombo = 8;

        private float _comboTimer;

        // ====================================================================
        // Unity lifecycle
        // ====================================================================
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            HighScore = PlayerPrefs.GetInt("HighScore", 0);
        }

        private void Update()
        {
            // Combo decay timer
            if (CurrentState == GameState.Playing && ComboMultiplier > 1)
            {
                _comboTimer -= Time.deltaTime;
                if (_comboTimer <= 0f)
                {
                    ComboMultiplier = 1;
                    OnComboChanged?.Invoke(ComboMultiplier);
                }
            }

            // Pause toggle
            if (CurrentState == GameState.Playing && Input.GetKeyDown(KeyCode.Escape))
            {
                PauseGame();
            }
            else if (CurrentState == GameState.Paused && Input.GetKeyDown(KeyCode.Escape))
            {
                ResumeGame();
            }
        }

        // ====================================================================
        // State transitions
        // ====================================================================
        public void StartGame()
        {
            Score = 0;
            CurrentWave = 0;
            ComboMultiplier = 1;
            SetState(GameState.Playing);
            Time.timeScale = 1f;

            // Load gameplay scene (index 1)
            SceneManager.LoadScene(1);
        }

        public void PauseGame()
        {
            if (CurrentState != GameState.Playing) return;
            Time.timeScale = 0f;
            SetState(GameState.Paused);
        }

        public void ResumeGame()
        {
            if (CurrentState != GameState.Paused) return;
            Time.timeScale = 1f;
            SetState(GameState.Playing);
        }

        public void GameOver()
        {
            if (Score > HighScore)
            {
                HighScore = Score;
                PlayerPrefs.SetInt("HighScore", HighScore);
                PlayerPrefs.Save();
            }
            SetState(GameState.GameOver);
        }

        public void ReturnToMainMenu()
        {
            Time.timeScale = 1f;
            SetState(GameState.MainMenu);
            SceneManager.LoadScene(0);
        }

        public void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        // ====================================================================
        // Scoring
        // ====================================================================

        /// <summary>
        /// Call when an enemy is destroyed. Base points are multiplied by the current combo.
        /// </summary>
        public void AddScore(int basePoints)
        {
            // Refresh combo timer and increment multiplier
            _comboTimer = comboTimeWindow;
            if (ComboMultiplier < maxCombo)
            {
                ComboMultiplier++;
                OnComboChanged?.Invoke(ComboMultiplier);
            }

            int earned = basePoints * ComboMultiplier;
            Score += earned;
            OnScoreChanged?.Invoke(Score);
        }

        /// <summary>
        /// Call when the player takes damage — resets the combo streak.
        /// </summary>
        public void ResetCombo()
        {
            ComboMultiplier = 1;
            OnComboChanged?.Invoke(ComboMultiplier);
        }

        // ====================================================================
        // Wave progression
        // ====================================================================
        public void AdvanceWave()
        {
            CurrentWave++;
            OnWaveChanged?.Invoke(CurrentWave);
        }

        // ====================================================================
        // Helpers
        // ====================================================================
        private void SetState(GameState newState)
        {
            CurrentState = newState;
            OnGameStateChanged?.Invoke(newState);
        }
    }
}

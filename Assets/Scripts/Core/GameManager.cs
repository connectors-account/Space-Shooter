using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SpaceShooter.Core
{
    /// <summary>
    /// Central game controller. Singleton, persistent across scenes.
    /// Manages game state, scene loading, score/lives/wave tracking and high score persistence.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public enum GameState { MainMenu, Playing, Paused, GameOver }

        public static GameManager Instance { get; private set; }

        [Header("Scene Names")]
        [SerializeField] private string mainMenuSceneName = "MainMenu";
        [SerializeField] private string gameSceneName = "GameScene";

        [Header("Gameplay Defaults")]
        [SerializeField] private int startingLives = 3;

        // --- Events ---
        public event Action OnGameStart;
        public event Action OnGameOver;
        public event Action OnPause;
        public event Action OnResume;
        public event Action<GameState> OnStateChanged;

        // --- Runtime data ---
        public GameState CurrentState { get; private set; } = GameState.MainMenu;
        public int CurrentScore { get; private set; }
        public int HighScore { get; private set; }
        public int Lives { get; private set; }
        public int WaveNumber { get; private set; }

        private const string HighScoreKey = "SpaceShooter_HighScore";

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            HighScore = PlayerPrefs.GetInt(HighScoreKey, 0);
            Lives = startingLives;
        }

        private void SetState(GameState newState)
        {
            CurrentState = newState;
            OnStateChanged?.Invoke(newState);
        }

        /// <summary>Load the game scene and begin a fresh run.</summary>
        public void StartGame()
        {
            CurrentScore = 0;
            Lives = startingLives;
            WaveNumber = 0;
            Time.timeScale = 1f;
            SetState(GameState.Playing);
            SceneManager.LoadScene(gameSceneName);
            // Fire after load so gameplay systems (which register in their own Start) can react.
            OnGameStart?.Invoke();
        }

        public void RestartGame()
        {
            StartGame();
        }

        public void LoadMainMenu()
        {
            Time.timeScale = 1f;
            SetState(GameState.MainMenu);
            SceneManager.LoadScene(mainMenuSceneName);
        }

        public void PauseGame()
        {
            if (CurrentState != GameState.Playing) return;
            Time.timeScale = 0f;
            SetState(GameState.Paused);
            OnPause?.Invoke();
        }

        public void ResumeGame()
        {
            if (CurrentState != GameState.Paused) return;
            Time.timeScale = 1f;
            SetState(GameState.Playing);
            OnResume?.Invoke();
        }

        public void GameOver()
        {
            if (CurrentState == GameState.GameOver) return;
            SetState(GameState.GameOver);
            SaveHighScore();
            OnGameOver?.Invoke();
        }

        // --- Score / lives / wave helpers ---
        public void SetScore(int score)
        {
            CurrentScore = score;
            if (CurrentScore > HighScore)
            {
                HighScore = CurrentScore;
            }
        }

        public void AddLife()
        {
            Lives++;
        }

        public bool LoseLife()
        {
            Lives = Mathf.Max(0, Lives - 1);
            return Lives <= 0;
        }

        public void SetWaveNumber(int wave)
        {
            WaveNumber = wave;
        }

        public bool IsNewHighScore()
        {
            return CurrentScore >= HighScore && CurrentScore > 0;
        }

        private void SaveHighScore()
        {
            if (CurrentScore > PlayerPrefs.GetInt(HighScoreKey, 0))
            {
                PlayerPrefs.SetInt(HighScoreKey, CurrentScore);
                PlayerPrefs.Save();
                HighScore = CurrentScore;
            }
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

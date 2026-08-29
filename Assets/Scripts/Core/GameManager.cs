using System;
using UnityEngine;
using SpaceShooter.Utilities;

namespace SpaceShooter.Core
{
    /// <summary>
    /// Overall game state the rest of the systems react to.
    /// </summary>
    public enum GameState
    {
        Menu,
        Playing,
        Paused,
        GameOver
    }

    /// <summary>
    /// Central authority for score, lives, wave progression and game state.
    /// Persists across scenes and stores the high score in PlayerPrefs.
    /// </summary>
    public class GameManager : Singleton<GameManager>
    {
        [Header("Runtime State (read-only in inspector)")]
        [SerializeField] private int score;
        [SerializeField] private int highScore;
        [SerializeField] private int lives = Constants.PlayerStartLives;
        [SerializeField] private int currentWave;
        [SerializeField] private GameState gameState = GameState.Menu;

        [Header("Configuration")]
        [SerializeField] private int startingLives = Constants.PlayerStartLives;

        // ------------------------------------------------------------------
        // Events
        // ------------------------------------------------------------------
        public event Action<int> OnScoreChanged;
        public event Action<int> OnLivesChanged;
        public event Action<int> OnWaveChanged;
        public event Action OnGameOver;
        public event Action<GameState> OnGameStateChanged;

        // ------------------------------------------------------------------
        // Public accessors
        // ------------------------------------------------------------------
        public int Score => score;
        public int HighScore => highScore;
        public int Lives => lives;
        public int CurrentWave => currentWave;
        public GameState State => gameState;
        public bool IsPlaying => gameState == GameState.Playing;
        public bool IsPaused => gameState == GameState.Paused;

        protected override void OnAwakeInitialize()
        {
            highScore = PlayerPrefs.GetInt(Constants.PrefKeys.HighScore, 0);
        }

        /// <summary>
        /// Resets all runtime values and sets state to Playing. Call when the Game scene loads.
        /// </summary>
        public void StartNewGame()
        {
            score = 0;
            lives = startingLives;
            currentWave = 0;

            OnScoreChanged?.Invoke(score);
            OnLivesChanged?.Invoke(lives);
            OnWaveChanged?.Invoke(currentWave);

            SetState(GameState.Playing);
        }

        public void AddScore(int amount)
        {
            if (amount == 0)
            {
                return;
            }

            score = Mathf.Max(0, score + amount);
            OnScoreChanged?.Invoke(score);

            if (score > highScore)
            {
                highScore = score;
                PlayerPrefs.SetInt(Constants.PrefKeys.HighScore, highScore);
                PlayerPrefs.Save();
            }
        }

        public void LoseLife()
        {
            lives = Mathf.Max(0, lives - 1);
            OnLivesChanged?.Invoke(lives);

            if (lives <= 0)
            {
                GameOver();
            }
        }

        public void AddLife()
        {
            lives++;
            OnLivesChanged?.Invoke(lives);
        }

        public void NextWave()
        {
            currentWave++;
            OnWaveChanged?.Invoke(currentWave);
        }

        public void PauseGame()
        {
            if (gameState != GameState.Playing)
            {
                return;
            }

            Time.timeScale = 0f;
            SetState(GameState.Paused);
        }

        public void ResumeGame()
        {
            if (gameState != GameState.Paused)
            {
                return;
            }

            Time.timeScale = 1f;
            SetState(GameState.Playing);
        }

        public void GameOver()
        {
            if (gameState == GameState.GameOver)
            {
                return;
            }

            Time.timeScale = 1f;
            SetState(GameState.GameOver);
            OnGameOver?.Invoke();
        }

        /// <summary>
        /// Resets timescale and state so a fresh game can start; scene loading is handled by SceneLoader.
        /// </summary>
        public void RestartGame()
        {
            Time.timeScale = 1f;
            StartNewGame();
        }

        public void SetMenuState()
        {
            Time.timeScale = 1f;
            SetState(GameState.Menu);
        }

        private void SetState(GameState newState)
        {
            if (gameState == newState)
            {
                return;
            }

            gameState = newState;
            OnGameStateChanged?.Invoke(gameState);
        }
    }
}

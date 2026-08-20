using System;
using UnityEngine;
using SpaceShooter.Utilities;

namespace SpaceShooter.Core
{
    public enum GameState
    {
        MainMenu,
        Playing,
        Paused,
        GameOver,
        Victory
    }

    /// <summary>
    /// Central game-flow controller. Singleton, persists across scenes.
    /// Owns the high-level state machine, current wave, score and lives mirrors,
    /// and broadcasts events the UI and other systems subscribe to.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Run State")]
        public GameState State = GameState.MainMenu;
        public int CurrentWave = 0;
        public int TotalWaves = 10;
        public int Score = 0;
        public int Lives = 3;

        // --- Events ---
        public event Action OnGameOver;
        public event Action OnVictory;
        public event Action<int> OnWaveComplete;   // wave number just completed
        public event Action<int> OnScoreChanged;    // new score
        public event Action<int> OnLivesChanged;    // new lives count
        public event Action<GameState> OnStateChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void SetState(GameState newState)
        {
            if (State == newState) return;
            State = newState;

            switch (newState)
            {
                case GameState.Playing:
                    Time.timeScale = 1f;
                    break;
                case GameState.Paused:
                    Time.timeScale = 0f;
                    break;
                case GameState.GameOver:
                    Time.timeScale = 1f;
                    break;
                case GameState.Victory:
                    Time.timeScale = 1f;
                    break;
                case GameState.MainMenu:
                    Time.timeScale = 1f;
                    break;
            }

            OnStateChanged?.Invoke(newState);
        }

        /// <summary>Reset all counters and begin a fresh run.</summary>
        public void StartNewGame()
        {
            CurrentWave = 0;
            Score = 0;
            Lives = 3;
            if (ScoreManager.Instance != null) ScoreManager.Instance.ResetScore();
            OnScoreChanged?.Invoke(Score);
            OnLivesChanged?.Invoke(Lives);
            SetState(GameState.Playing);
        }

        public void AddScore(int amount)
        {
            Score += amount;
            OnScoreChanged?.Invoke(Score);
        }

        public void SyncScore(int absoluteScore)
        {
            Score = absoluteScore;
            OnScoreChanged?.Invoke(Score);
        }

        public void SetLives(int lives)
        {
            Lives = Mathf.Max(0, lives);
            OnLivesChanged?.Invoke(Lives);
            if (Lives <= 0)
            {
                TriggerGameOver();
            }
        }

        public void LoseLife()
        {
            SetLives(Lives - 1);
        }

        public void CompleteWave(int waveNumber)
        {
            CurrentWave = waveNumber;
            OnWaveComplete?.Invoke(waveNumber);

            if (waveNumber >= TotalWaves)
            {
                TriggerVictory();
            }
        }

        public void TriggerGameOver()
        {
            if (State == GameState.GameOver) return;
            SetState(GameState.GameOver);
            if (ScoreManager.Instance != null) ScoreManager.Instance.SaveHighScore();
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("game_over");
            OnGameOver?.Invoke();
        }

        public void TriggerVictory()
        {
            if (State == GameState.Victory) return;
            SetState(GameState.Victory);
            if (ScoreManager.Instance != null) ScoreManager.Instance.SaveHighScore();
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("wave_complete");
            OnVictory?.Invoke();
        }

        public void PauseGame()
        {
            if (State == GameState.Playing) SetState(GameState.Paused);
        }

        public void ResumeGame()
        {
            if (State == GameState.Paused) SetState(GameState.Playing);
        }

        public bool IsPlaying => State == GameState.Playing;
    }
}

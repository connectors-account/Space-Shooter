using System;
using UnityEngine;

namespace SpaceShooter.Core
{
    public enum GameState
    {
        MainMenu,
        Playing,
        Paused,
        GameOver
    }

    /// <summary>
    /// Central game controller. Singleton that persists across scenes.
    /// Tracks score, high score, wave number and game state, and broadcasts events.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        private const string HighScoreKey = "HighScore";

        [Header("Runtime State (read-only)")]
        [SerializeField] private GameState state = GameState.MainMenu;
        [SerializeField] private int score;
        [SerializeField] private int highScore;
        [SerializeField] private int waveNumber;
        [SerializeField] private int lives = 3;

        public GameState State => state;
        public int Score => score;
        public int HighScore => highScore;
        public int WaveNumber => waveNumber;
        public int Lives => lives;

        // Events
        public event Action<int> OnScoreChanged;
        public event Action<int> OnWaveChanged;
        public event Action<int> OnLivesChanged;
        public event Action OnGameOver;
        public event Action OnGameStart;
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

            highScore = PlayerPrefs.GetInt(HighScoreKey, 0);
        }

        private void SetState(GameState newState)
        {
            state = newState;
            OnStateChanged?.Invoke(state);
        }

        /// <summary>Adds points to the score and updates high score if beaten.</summary>
        public void AddScore(int amount)
        {
            if (amount == 0) return;
            score += Mathf.Max(0, amount);
            OnScoreChanged?.Invoke(score);

            if (score > highScore)
            {
                highScore = score;
            }
        }

        public void SetWave(int wave)
        {
            waveNumber = wave;
            OnWaveChanged?.Invoke(waveNumber);
        }

        public void SetLives(int value)
        {
            lives = Mathf.Max(0, value);
            OnLivesChanged?.Invoke(lives);
            if (lives <= 0 && state == GameState.Playing)
            {
                GameOver();
            }
        }

        public void LoseLife()
        {
            SetLives(lives - 1);
        }

        /// <summary>Resets runtime values and begins a new game session.</summary>
        public void StartGame()
        {
            score = 0;
            waveNumber = 0;
            lives = 3;
            Time.timeScale = 1f;

            OnScoreChanged?.Invoke(score);
            OnWaveChanged?.Invoke(waveNumber);
            OnLivesChanged?.Invoke(lives);

            SetState(GameState.Playing);
            OnGameStart?.Invoke();
        }

        public void PauseGame()
        {
            if (state != GameState.Playing) return;
            Time.timeScale = 0f;
            SetState(GameState.Paused);
        }

        public void ResumeGame()
        {
            if (state != GameState.Paused) return;
            Time.timeScale = 1f;
            SetState(GameState.Playing);
        }

        public void GameOver()
        {
            if (state == GameState.GameOver) return;

            if (score >= highScore)
            {
                highScore = score;
                PlayerPrefs.SetInt(HighScoreKey, highScore);
                PlayerPrefs.Save();
            }

            Time.timeScale = 0f;
            SetState(GameState.GameOver);
            OnGameOver?.Invoke();
        }

        /// <summary>Returns true if the current score set a new high score record.</summary>
        public bool IsNewHighScore()
        {
            return score >= PlayerPrefs.GetInt(HighScoreKey, 0) && score > 0;
        }

        public void RestartGame()
        {
            Time.timeScale = 1f;
            if (SceneLoader.Instance != null)
            {
                SceneLoader.Instance.LoadGameScene();
            }
            StartGame();
        }

        private void OnApplicationQuit()
        {
            if (score > PlayerPrefs.GetInt(HighScoreKey, 0))
            {
                PlayerPrefs.SetInt(HighScoreKey, score);
                PlayerPrefs.Save();
            }
        }
    }
}

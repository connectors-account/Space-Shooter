using UnityEngine;
using UnityEngine.SceneManagement;
using System;

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
    /// Manages game state, scoring, and scene transitions
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }
        
        [Header("Game Settings")]
        [SerializeField] private int startingLives = 3;
        [SerializeField] private float respawnDelay = 2f;
        
        [Header("Scene Names")]
        [SerializeField] private string mainMenuSceneName = "MainMenu";
        [SerializeField] private string gameSceneName = "Game";
        [SerializeField] private string gameOverSceneName = "GameOver";
        
        // Game state
        private GameState currentState = GameState.MainMenu;
        private int currentScore = 0;
        private int highScore = 0;
        private int currentWave = 1;
        private int lives;
        private int enemiesKilled = 0;
        
        // Events
        public event Action<int> OnScoreChanged;
        public event Action<int> OnWaveChanged;
        public event Action<int> OnLivesChanged;
        public event Action<GameState> OnGameStateChanged;
        public event Action<int> OnEnemyKilled;
        
        // Properties
        public GameState CurrentState => currentState;
        public int CurrentScore => currentScore;
        public int HighScore => highScore;
        public int CurrentWave => currentWave;
        public int Lives => lives;
        public int EnemiesKilled => enemiesKilled;
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                LoadHighScore();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        private void Start()
        {
            lives = startingLives;
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
            currentScore = 0;
            currentWave = 1;
            lives = startingLives;
            enemiesKilled = 0;
            
            ChangeState(GameState.Playing);
            Time.timeScale = 1f;
            
            SceneManager.LoadScene(gameSceneName);
            
            OnScoreChanged?.Invoke(currentScore);
            OnWaveChanged?.Invoke(currentWave);
            OnLivesChanged?.Invoke(lives);
        }
        
        public void PauseGame()
        {
            if (currentState != GameState.Playing) return;
            
            ChangeState(GameState.Paused);
            Time.timeScale = 0f;
        }
        
        public void ResumeGame()
        {
            if (currentState != GameState.Paused) return;
            
            ChangeState(GameState.Playing);
            Time.timeScale = 1f;
        }
        
        public void GameOver()
        {
            ChangeState(GameState.GameOver);
            Time.timeScale = 0f;
            
            if (currentScore > highScore)
            {
                highScore = currentScore;
                SaveHighScore();
            }
        }
        
        public void Victory()
        {
            ChangeState(GameState.Victory);
            
            if (currentScore > highScore)
            {
                highScore = currentScore;
                SaveHighScore();
            }
        }
        
        public void ReturnToMainMenu()
        {
            ChangeState(GameState.MainMenu);
            Time.timeScale = 1f;
            SceneManager.LoadScene(mainMenuSceneName);
        }
        
        public void RestartGame()
        {
            StartGame();
        }
        
        public void QuitGame()
        {
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }
        
        public void AddScore(int points)
        {
            currentScore += points;
            OnScoreChanged?.Invoke(currentScore);
        }
        
        public void SetWave(int wave)
        {
            currentWave = wave;
            OnWaveChanged?.Invoke(currentWave);
        }
        
        public void IncrementWave()
        {
            currentWave++;
            OnWaveChanged?.Invoke(currentWave);
        }
        
        public void EnemyDestroyed(int points)
        {
            enemiesKilled++;
            AddScore(points);
            OnEnemyKilled?.Invoke(enemiesKilled);
        }
        
        public void LoseLife()
        {
            lives--;
            OnLivesChanged?.Invoke(lives);
            
            if (lives <= 0)
            {
                GameOver();
            }
        }
        
        public void GainLife()
        {
            lives++;
            OnLivesChanged?.Invoke(lives);
        }
        
        private void ChangeState(GameState newState)
        {
            currentState = newState;
            OnGameStateChanged?.Invoke(currentState);
        }
        
        private void SaveHighScore()
        {
            PlayerPrefs.SetInt("HighScore", highScore);
            PlayerPrefs.Save();
        }
        
        private void LoadHighScore()
        {
            highScore = PlayerPrefs.GetInt("HighScore", 0);
        }
        
        public void ResetHighScore()
        {
            highScore = 0;
            SaveHighScore();
        }
    }
}

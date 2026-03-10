using UnityEngine;
using UnityEngine.SceneManagement;
using System;

namespace SpaceShooter.Managers
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Game State")]
        [SerializeField] private bool isPaused = false;
        [SerializeField] private bool isGameOver = false;
        [SerializeField] private int currentWave = 0;

        [Header("Score Settings")]
        [SerializeField] private int score = 0;
        [SerializeField] private int highScore = 0;

        [Header("References")]
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private Transform playerSpawnPoint;

        public event Action<int> OnScoreChanged;
        public event Action<int> OnWaveChanged;
        public event Action OnGameOver;
        public event Action OnGamePaused;
        public event Action OnGameResumed;
        public event Action OnGameStarted;

        public bool IsPaused => isPaused;
        public bool IsGameOver => isGameOver;
        public int Score => score;
        public int HighScore => highScore;
        public int CurrentWave => currentWave;

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
            Time.timeScale = 1f;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape) && !isGameOver)
            {
                TogglePause();
            }
        }

        public void StartGame()
        {
            isGameOver = false;
            isPaused = false;
            score = 0;
            currentWave = 0;
            Time.timeScale = 1f;
            OnScoreChanged?.Invoke(score);
            OnGameStarted?.Invoke();
            SceneManager.LoadScene("GameScene");
        }

        public void AddScore(int points)
        {
            if (isGameOver) return;
            
            score += points;
            OnScoreChanged?.Invoke(score);

            if (score > highScore)
            {
                highScore = score;
                SaveHighScore();
            }
        }

        public void SetWave(int wave)
        {
            currentWave = wave;
            OnWaveChanged?.Invoke(currentWave);
        }

        public void TogglePause()
        {
            if (isGameOver) return;

            isPaused = !isPaused;
            Time.timeScale = isPaused ? 0f : 1f;

            if (isPaused)
                OnGamePaused?.Invoke();
            else
                OnGameResumed?.Invoke();
        }

        public void GameOver()
        {
            if (isGameOver) return;

            isGameOver = true;
            Time.timeScale = 0f;
            
            if (score > highScore)
            {
                highScore = score;
                SaveHighScore();
            }
            
            OnGameOver?.Invoke();
        }

        public void RestartGame()
        {
            isGameOver = false;
            isPaused = false;
            score = 0;
            currentWave = 0;
            Time.timeScale = 1f;
            OnScoreChanged?.Invoke(score);
            SceneManager.LoadScene("GameScene");
        }

        public void LoadMainMenu()
        {
            isGameOver = false;
            isPaused = false;
            Time.timeScale = 1f;
            SceneManager.LoadScene("MainMenu");
        }

        public void QuitGame()
        {
            SaveHighScore();
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
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

        public void SpawnPlayer()
        {
            if (playerPrefab != null && playerSpawnPoint != null)
            {
                Instantiate(playerPrefab, playerSpawnPoint.position, Quaternion.identity);
            }
        }
    }
}

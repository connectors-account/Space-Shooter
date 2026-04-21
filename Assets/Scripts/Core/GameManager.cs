using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SpaceShooter.Core
{
    public enum GameState
    {
        MainMenu,
        Playing,
        Paused,
        GameOver
    }

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Scene Names")]
        [SerializeField] private string mainMenuSceneName = "MainMenu";
        [SerializeField] private string gameSceneName = "Game";

        [Header("Player Settings")]
        [SerializeField] private int startingLives = 3;

        public event Action<int> OnScoreChanged;
        public event Action<int> OnWaveChanged;
        public event Action<float, float> OnPlayerHealthChanged;
        public event Action<int> OnLivesChanged;
        public event Action<GameState> OnGameStateChanged;

        public GameState CurrentState { get; private set; } = GameState.MainMenu;
        public int Score { get; private set; }
        public int Wave { get; private set; } = 1;
        public int Lives { get; private set; }

        private float playerMaxHealth = 100f;
        private float playerHealth = 100f;

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

        private void Start()
        {
            Lives = startingLives;
            NotifyAll();
        }

        public void StartNewGame()
        {
            Score = 0;
            Wave = 1;
            Lives = startingLives;
            SetGameState(GameState.Playing);
            SceneManager.LoadScene(gameSceneName);
        }

        public void ReturnToMainMenu()
        {
            SetGameState(GameState.MainMenu);
            SceneManager.LoadScene(mainMenuSceneName);
        }

        public void SetPaused(bool paused)
        {
            if (CurrentState == GameState.GameOver || CurrentState == GameState.MainMenu)
            {
                return;
            }

            SetGameState(paused ? GameState.Paused : GameState.Playing);
            Time.timeScale = paused ? 0f : 1f;
        }

        public void RegisterPlayerHealth(float maxHealth)
        {
            playerMaxHealth = maxHealth;
            playerHealth = maxHealth;
            OnPlayerHealthChanged?.Invoke(playerHealth, playerMaxHealth);
        }

        public void SetPlayerHealth(float health)
        {
            playerHealth = Mathf.Clamp(health, 0f, playerMaxHealth);
            OnPlayerHealthChanged?.Invoke(playerHealth, playerMaxHealth);
        }

        public void AddScore(int value)
        {
            Score += Mathf.Max(0, value);
            OnScoreChanged?.Invoke(Score);
        }

        public void AdvanceWave()
        {
            Wave += 1;
            OnWaveChanged?.Invoke(Wave);
        }

        public void OnPlayerDestroyed()
        {
            Lives -= 1;
            OnLivesChanged?.Invoke(Lives);

            if (Lives <= 0)
            {
                SetGameState(GameState.GameOver);
                Time.timeScale = 1f;
                return;
            }

            SetGameState(GameState.Playing);
        }

        private void SetGameState(GameState newState)
        {
            CurrentState = newState;
            OnGameStateChanged?.Invoke(CurrentState);
        }

        private void NotifyAll()
        {
            OnScoreChanged?.Invoke(Score);
            OnWaveChanged?.Invoke(Wave);
            OnPlayerHealthChanged?.Invoke(playerHealth, playerMaxHealth);
            OnLivesChanged?.Invoke(Lives);
            OnGameStateChanged?.Invoke(CurrentState);
        }
    }
}

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

        public const string HighScoreKey = "SPACE_SHOOTER_HIGH_SCORE";

        public GameState CurrentState { get; private set; } = GameState.MainMenu;
        public int Score { get; private set; }
        public int HighScore { get; private set; }
        public int CurrentWave { get; private set; }

        public event Action<int> OnScoreChanged;
        public event Action<int> OnWaveChanged;
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
            HighScore = PlayerPrefs.GetInt(HighScoreKey, 0);
            SceneManager.sceneLoaded += HandleSceneLoaded;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                SceneManager.sceneLoaded -= HandleSceneLoaded;
            }
        }

        private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == "MainMenu")
            {
                SetState(GameState.MainMenu);
                Time.timeScale = 1f;
            }
        }

        public void StartGame()
        {
            Score = 0;
            CurrentWave = 0;
            SetState(GameState.Playing);
            Time.timeScale = 1f;
            SceneManager.LoadScene("Gameplay");
            OnScoreChanged?.Invoke(Score);
            OnWaveChanged?.Invoke(CurrentWave);
        }

        public void AddScore(int amount)
        {
            if (CurrentState != GameState.Playing) return;

            Score += amount;
            OnScoreChanged?.Invoke(Score);

            if (Score > HighScore)
            {
                HighScore = Score;
                PlayerPrefs.SetInt(HighScoreKey, HighScore);
                PlayerPrefs.Save();
            }
        }

        public void SetWave(int wave)
        {
            CurrentWave = wave;
            OnWaveChanged?.Invoke(CurrentWave);
        }

        public void TogglePause()
        {
            if (CurrentState == GameState.Playing)
            {
                SetState(GameState.Paused);
                Time.timeScale = 0f;
            }
            else if (CurrentState == GameState.Paused)
            {
                SetState(GameState.Playing);
                Time.timeScale = 1f;
            }
        }

        public void GameOver()
        {
            SetState(GameState.GameOver);
            Time.timeScale = 1f;
        }

        public void BackToMainMenu()
        {
            SetState(GameState.MainMenu);
            Time.timeScale = 1f;
            SceneManager.LoadScene("MainMenu");
        }

        public void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void SetState(GameState newState)
        {
            CurrentState = newState;
            OnStateChanged?.Invoke(CurrentState);
        }
    }
}

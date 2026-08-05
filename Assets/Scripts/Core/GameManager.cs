using System;
using UnityEngine;
using SpaceShooter.Utilities;

namespace SpaceShooter.Core
{
    /// <summary>Overall game states driven by the <see cref="GameManager"/>.</summary>
    public enum GameState
    {
        MainMenu,
        Playing,
        Paused,
        GameOver
    }

    /// <summary>
    /// Singleton game state-machine. Persists across scenes and broadcasts C#
    /// events whenever the state changes so UI and gameplay systems can react.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        /// <summary>Fired whenever the state changes (old, new).</summary>
        public event Action<GameState, GameState> OnStateChanged;

        public GameState State { get; private set; } = GameState.MainMenu;

        public bool IsPlaying => State == GameState.Playing;
        public bool IsPaused => State == GameState.Paused;

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

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        // -----------------------------------------------------------------
        // State transitions
        // -----------------------------------------------------------------
        private void SetState(GameState next)
        {
            if (State == next) return;
            var previous = State;
            State = next;
            OnStateChanged?.Invoke(previous, next);
        }

        /// <summary>Begin gameplay. Loads the Game scene and un-pauses time.</summary>
        public void StartGame()
        {
            Time.timeScale = 1f;
            SetState(GameState.Playing);
            if (SceneLoader.Instance != null)
                SceneLoader.Instance.LoadScene(Constants.SceneGame);
            else
                UnityEngine.SceneManagement.SceneManager.LoadScene(Constants.SceneGame);
        }

        /// <summary>Mark the state as Playing without loading a scene (called by the Game scene on load).</summary>
        public void EnterPlayingState()
        {
            Time.timeScale = 1f;
            SetState(GameState.Playing);
        }

        /// <summary>Mark the state as MainMenu without loading a scene (called by the MainMenu scene on load).</summary>
        public void EnterMainMenuState()
        {
            Time.timeScale = 1f;
            SetState(GameState.MainMenu);
        }

        public void PauseGame()
        {
            if (State != GameState.Playing) return;
            Time.timeScale = 0f;
            SetState(GameState.Paused);
        }

        public void ResumeGame()
        {
            if (State != GameState.Paused) return;
            Time.timeScale = 1f;
            SetState(GameState.Playing);
        }

        public void TogglePause()
        {
            if (State == GameState.Playing) PauseGame();
            else if (State == GameState.Paused) ResumeGame();
        }

        public void GameOver()
        {
            if (State == GameState.GameOver) return;
            Time.timeScale = 1f;
            SetState(GameState.GameOver);
        }

        public void RestartGame()
        {
            Time.timeScale = 1f;
            SetState(GameState.Playing);
            if (SceneLoader.Instance != null)
                SceneLoader.Instance.LoadScene(Constants.SceneGame);
            else
                UnityEngine.SceneManagement.SceneManager.LoadScene(Constants.SceneGame);
        }

        public void GoToMainMenu()
        {
            Time.timeScale = 1f;
            SetState(GameState.MainMenu);
            if (SceneLoader.Instance != null)
                SceneLoader.Instance.LoadScene(Constants.SceneMainMenu);
            else
                UnityEngine.SceneManagement.SceneManager.LoadScene(Constants.SceneMainMenu);
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

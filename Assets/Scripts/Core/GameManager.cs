using System;
using UnityEngine;

namespace SpaceShooter
{
    /// <summary>High level game states.</summary>
    public enum GameState
    {
        MainMenu,
        Playing,
        Paused,
        GameOver
    }

    /// <summary>
    /// Owns the overall game state machine and coordinates the score, wave and
    /// audio subsystems. Persists across scene loads.
    /// </summary>
    public class GameManager : Singleton<GameManager>
    {
        [SerializeField] private GameState currentState = GameState.MainMenu;

        public GameState CurrentState => currentState;
        public bool IsPaused => currentState == GameState.Paused;
        public bool IsPlaying => currentState == GameState.Playing;

        public event Action OnGameOver;
        public event Action OnPause;
        public event Action OnResume;
        public event Action<GameState> OnStateChanged;

        protected override void Awake()
        {
            persistAcrossScenes = true;
            base.Awake();
        }

        private void Update()
        {
            // Allow pause toggling with Escape during active gameplay.
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (currentState == GameState.Playing) PauseGame();
                else if (currentState == GameState.Paused) ResumeGame();
            }
        }

        /// <summary>Begins a fresh play session: resets score and starts wave 1.</summary>
        public void NewGame()
        {
            Time.timeScale = 1f;
            if (ScoreManager.Instance != null) ScoreManager.Instance.ResetScore();
            SetState(GameState.Playing);

            if (WaveManager.Instance != null) WaveManager.Instance.StartWaves();

            if (AudioManager.Instance != null && AudioManager.Instance.bgMusic != null)
            {
                AudioManager.Instance.PlayMusic(AudioManager.Instance.bgMusic);
            }
        }

        /// <summary>Pauses gameplay (freezes time) and raises <see cref="OnPause"/>.</summary>
        public void PauseGame()
        {
            if (currentState != GameState.Playing) return;
            Time.timeScale = 0f;
            SetState(GameState.Paused);
            OnPause?.Invoke();
        }

        /// <summary>Resumes gameplay (restores time) and raises <see cref="OnResume"/>.</summary>
        public void ResumeGame()
        {
            if (currentState != GameState.Paused) return;
            Time.timeScale = 1f;
            SetState(GameState.Playing);
            OnResume?.Invoke();
        }

        /// <summary>Toggles between paused and playing.</summary>
        public void TogglePause()
        {
            if (currentState == GameState.Playing) PauseGame();
            else if (currentState == GameState.Paused) ResumeGame();
        }

        /// <summary>Ends the current game, raising <see cref="OnGameOver"/>.</summary>
        public void GameOver()
        {
            if (currentState == GameState.GameOver) return;
            Time.timeScale = 1f;
            SetState(GameState.GameOver);
            OnGameOver?.Invoke();

            if (AudioManager.Instance != null) AudioManager.Instance.StopMusic();
        }

        /// <summary>Sets the state used when entering the main menu.</summary>
        public void EnterMainMenu()
        {
            Time.timeScale = 1f;
            SetState(GameState.MainMenu);
        }

        private void SetState(GameState newState)
        {
            currentState = newState;
            OnStateChanged?.Invoke(newState);
        }
    }
}

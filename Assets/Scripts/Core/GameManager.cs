using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SpaceShooter.Core
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Scene Names")]
        [SerializeField] private string gameplaySceneName = "Gameplay";
        [SerializeField] private string mainMenuSceneName = "MainMenu";

        [Header("Runtime")]
        [SerializeField] private GameState initialState = GameState.MainMenu;

        public GameState CurrentState { get; private set; }

        public event Action<GameState> OnGameStateChanged;
        public event Action OnPauseToggled;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            CurrentState = initialState;
            ApplyTimeScale();
        }

        private void Start()
        {
            OnGameStateChanged?.Invoke(CurrentState);
        }

        private void Update()
        {
            if (CurrentState == GameState.Playing && Input.GetKeyDown(KeyCode.Escape))
            {
                PauseGame();
            }
            else if (CurrentState == GameState.Paused && Input.GetKeyDown(KeyCode.Escape))
            {
                ResumeGame();
            }
        }

        public void StartGame()
        {
            ScoreManager.ResetScore();
            ChangeState(GameState.Playing);
            SceneManager.LoadScene(gameplaySceneName);
        }

        public void BackToMainMenu()
        {
            ChangeState(GameState.MainMenu);
            SceneManager.LoadScene(mainMenuSceneName);
        }

        public void RestartGameplay()
        {
            ScoreManager.ResetScore();
            ChangeState(GameState.Playing);
            SceneManager.LoadScene(gameplaySceneName);
        }

        public void PauseGame()
        {
            if (CurrentState != GameState.Playing)
            {
                return;
            }

            ChangeState(GameState.Paused);
            OnPauseToggled?.Invoke();
        }

        public void ResumeGame()
        {
            if (CurrentState != GameState.Paused)
            {
                return;
            }

            ChangeState(GameState.Playing);
            OnPauseToggled?.Invoke();
        }

        public void TriggerGameOver()
        {
            if (CurrentState == GameState.GameOver)
            {
                return;
            }

            ChangeState(GameState.GameOver);
        }

        public void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void ChangeState(GameState newState)
        {
            CurrentState = newState;
            ApplyTimeScale();
            OnGameStateChanged?.Invoke(CurrentState);
        }

        private void ApplyTimeScale()
        {
            Time.timeScale = CurrentState == GameState.Paused ? 0f : 1f;
        }
    }
}

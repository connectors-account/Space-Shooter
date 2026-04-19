using UnityEngine;

namespace SpaceShooter.Core
{
    /// <summary>
    /// Global game state coordinator (menu, playing, paused, game over), wave flow and restart flow.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public enum GameState
        {
            MainMenu,
            Playing,
            Paused,
            GameOver
        }

        public static GameManager Instance { get; private set; }

        [Header("Scene References")]
        [SerializeField] private Enemy.EnemySpawner enemySpawner;
        [SerializeField] private Player.PlayerController playerController;
        [SerializeField] private Systems.ScoreSystem scoreSystem;

        [Header("Wave Flow")]
        [SerializeField] private float firstWaveDelay = 1.5f;

        public GameState CurrentState { get; private set; } = GameState.MainMenu;

        public int CurrentWave => enemySpawner != null ? enemySpawner.CurrentWave : 0;

        public event System.Action<GameState> OnGameStateChanged;
        public event System.Action<int> OnWaveChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void Start()
        {
            ChangeState(GameState.MainMenu);

            if (playerController != null)
            {
                playerController.OnPlayerDied += HandlePlayerDeath;
            }

            if (enemySpawner != null)
            {
                enemySpawner.OnWaveStarted += wave => OnWaveChanged?.Invoke(wave);
            }
        }

        private void OnDestroy()
        {
            if (playerController != null)
            {
                playerController.OnPlayerDied -= HandlePlayerDeath;
            }

            if (Instance == this)
            {
                Instance = null;
            }
        }

        public void StartGame()
        {
            scoreSystem?.ResetScore();
            playerController?.ResetPlayer();
            enemySpawner?.ResetSpawner();

            Time.timeScale = 1f;
            ChangeState(GameState.Playing);

            if (enemySpawner != null)
            {
                enemySpawner.BeginSpawning(firstWaveDelay);
            }
        }

        public void TogglePause()
        {
            if (CurrentState == GameState.GameOver || CurrentState == GameState.MainMenu)
            {
                return;
            }

            if (CurrentState == GameState.Paused)
            {
                Time.timeScale = 1f;
                ChangeState(GameState.Playing);
            }
            else if (CurrentState == GameState.Playing)
            {
                Time.timeScale = 0f;
                ChangeState(GameState.Paused);
            }
        }

        public void ReturnToMainMenu()
        {
            Time.timeScale = 1f;
            enemySpawner?.StopSpawningAndClear();
            playerController?.ResetPlayer();
            scoreSystem?.ResetScore();
            ChangeState(GameState.MainMenu);
        }

        public void RestartGame()
        {
            StartGame();
        }

        private void HandlePlayerDeath()
        {
            enemySpawner?.StopSpawningAndClear();
            ChangeState(GameState.GameOver);
            Time.timeScale = 0f;
            Audio.SoundManager.Instance?.PlayGameOver();
        }

        private void ChangeState(GameState nextState)
        {
            CurrentState = nextState;
            OnGameStateChanged?.Invoke(CurrentState);
        }

        public bool IsGameplayActive()
        {
            return CurrentState == GameState.Playing;
        }
    }
}

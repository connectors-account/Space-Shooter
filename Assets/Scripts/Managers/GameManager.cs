using System;
using UnityEngine;

namespace SpaceShooter
{
    public enum GameState
    {
        MainMenu,
        Playing,
        Paused,
        GameOver
    }

    /// <summary>
    /// Central game-state controller (singleton). Owns the high-level flow:
    /// menu → playing → paused → game over, plus lives and wave tracking.
    /// Coordinates the player, spawner and UI.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Scene References")]
        [SerializeField] private PlayerController player;
        [SerializeField] private EnemySpawner spawner;
        [SerializeField] private Transform playerSpawnPoint;

        [Header("Game Rules")]
        [SerializeField] private int startingLives = 3;

        private GameState currentState = GameState.MainMenu;
        private int lives;
        private int currentWave;

        public GameState CurrentState => currentState;
        public int Lives => lives;
        public int CurrentWave => currentWave;

        public event Action<int> OnLivesChanged;
        public event Action<GameState> OnStateChanged;

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
            SetState(GameState.MainMenu);
            Time.timeScale = 1f;

            // Hide the player until the game starts.
            if (player != null) player.gameObject.SetActive(false);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (currentState == GameState.Playing) PauseGame();
                else if (currentState == GameState.Paused) ResumeGame();
            }
        }

        // ---------- Flow control ----------

        public void StartGame()
        {
            lives = startingLives;
            currentWave = 0;
            OnLivesChanged?.Invoke(lives);

            ScoreManager.Instance?.ResetScore();

            if (player != null)
            {
                Vector3 spawnPos = playerSpawnPoint != null ? playerSpawnPoint.position : Vector3.zero;
                player.ResetPlayer(spawnPos);
            }

            SetState(GameState.Playing);
            Time.timeScale = 1f;

            spawner?.StartSpawning();
            AudioManager.Instance?.PlayMusic();
        }

        public void PauseGame()
        {
            if (currentState != GameState.Playing) return;
            SetState(GameState.Paused);
            Time.timeScale = 0f;
        }

        public void ResumeGame()
        {
            if (currentState != GameState.Paused) return;
            SetState(GameState.Playing);
            Time.timeScale = 1f;
        }

        public void OnPlayerDied()
        {
            lives--;
            OnLivesChanged?.Invoke(lives);

            if (lives > 0)
            {
                // Respawn after a short delay.
                Invoke(nameof(RespawnPlayer), 1.5f);
            }
            else
            {
                GameOver();
            }
        }

        private void RespawnPlayer()
        {
            if (player == null) return;
            Vector3 spawnPos = playerSpawnPoint != null ? playerSpawnPoint.position : Vector3.zero;
            player.ResetPlayer(spawnPos);
            player.ActivateShield(2.5f); // brief spawn protection
        }

        public void GameOver()
        {
            SetState(GameState.GameOver);
            Time.timeScale = 0f;
            spawner?.StopSpawning();
            AudioManager.Instance?.StopMusic();
            AudioManager.Instance?.PlayGameOver();
            ScoreManager.Instance?.SaveHighScore();
        }

        public void RestartGame()
        {
            // Clear any leftover enemies / bullets / power-ups.
            ClearTaggedObjects("Enemy");
            ClearTaggedObjects("Bullet");
            ClearTaggedObjects("PowerUp");
            StartGame();
        }

        public void ReturnToMainMenu()
        {
            Time.timeScale = 1f;
            spawner?.StopSpawning();
            ClearTaggedObjects("Enemy");
            ClearTaggedObjects("Bullet");
            ClearTaggedObjects("PowerUp");
            if (player != null) player.gameObject.SetActive(false);
            AudioManager.Instance?.StopMusic();
            SetState(GameState.MainMenu);
        }

        public void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        // ---------- Helpers ----------

        public void SetWave(int wave)
        {
            currentWave = wave;
        }

        private void SetState(GameState newState)
        {
            currentState = newState;
            OnStateChanged?.Invoke(currentState);
        }

        private void ClearTaggedObjects(string tag)
        {
            GameObject[] objs = GameObject.FindGameObjectsWithTag(tag);
            foreach (var o in objs) Destroy(o);
        }
    }
}

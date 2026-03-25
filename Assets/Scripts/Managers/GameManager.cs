// =============================================================================
// GameManager.cs — Central game state manager (singleton)
// =============================================================================
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

namespace SpaceShooter.Managers
{
    /// <summary>
    /// Game states for overall flow control.
    /// </summary>
    public enum GameState
    {
        MainMenu,
        Playing,
        Paused,
        GameOver
    }

    /// <summary>
    /// Singleton manager controlling game state, scoring, lives, and scene transitions.
    /// Persists across scene loads.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Game Settings")]
        [SerializeField] private int startingLives = 3;
        [SerializeField] private Vector3 playerSpawnPosition = new Vector3(0f, -3.5f, 0f);

        [Header("Power-Up Prefabs")]
        [SerializeField] private GameObject[] powerUpPrefabs;

        // State
        private GameState currentState = GameState.MainMenu;
        private int score;
        private int highScore;
        private int lives;
        private Player.PlayerController playerRef;
        private Enemy.EnemySpawner spawnerRef;

        // Events for UI to subscribe to
        public event Action<int> OnScoreChanged;
        public event Action<int> OnLivesChanged;
        public event Action<int, int> OnHealthChanged;
        public event Action<int, bool> OnWaveAnnounce;
        public event Action<GameState> OnGameStateChanged;

        /// <summary>Current game state.</summary>
        public GameState CurrentState => currentState;

        /// <summary>Current score.</summary>
        public int Score => score;

        /// <summary>All-time high score (saved to PlayerPrefs).</summary>
        public int HighScore => highScore;

        /// <summary>Remaining player lives.</summary>
        public int Lives => lives;

        private const string HIGH_SCORE_KEY = "SpaceShooter_HighScore";

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            highScore = PlayerPrefs.GetInt(HIGH_SCORE_KEY, 0);
        }

        /// <summary>
        /// Loads the MainMenu scene.
        /// </summary>
        public void GoToMainMenu()
        {
            Time.timeScale = 1f;
            SetState(GameState.MainMenu);
            SceneManager.LoadScene("MainMenu");
        }

        /// <summary>
        /// Starts a new game: resets score/lives, loads gameplay scene.
        /// </summary>
        public void StartGame()
        {
            score = 0;
            lives = startingLives;
            OnScoreChanged?.Invoke(score);
            OnLivesChanged?.Invoke(lives);
            Time.timeScale = 1f;
            SceneManager.LoadScene("GamePlay");
            // State set after scene loads
            SceneManager.sceneLoaded += OnGameplaySceneLoaded;
        }

        private void OnGameplaySceneLoaded(Scene scene, LoadSceneMode mode)
        {
            SceneManager.sceneLoaded -= OnGameplaySceneLoaded;
            if (scene.name == "GamePlay")
            {
                SetState(GameState.Playing);
                FindGameplayReferences();
                if (spawnerRef != null)
                    spawnerRef.StartSpawning();
            }
        }

        /// <summary>
        /// Finds player and spawner references in the gameplay scene.
        /// </summary>
        private void FindGameplayReferences()
        {
            GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
            if (playerGO != null)
            {
                playerRef = playerGO.GetComponent<Player.PlayerController>();
                Player.HealthSystem hs = playerGO.GetComponent<Player.HealthSystem>();
                if (hs != null)
                {
                    hs.OnHealthChanged += (cur, max) => OnHealthChanged?.Invoke(cur, max);
                    OnHealthChanged?.Invoke(hs.CurrentHealth, hs.MaxHealth);
                }
            }

            spawnerRef = FindObjectOfType<Enemy.EnemySpawner>();
        }

        /// <summary>
        /// Toggles pause state.
        /// </summary>
        public void TogglePause()
        {
            if (currentState == GameState.Playing)
            {
                Time.timeScale = 0f;
                SetState(GameState.Paused);
            }
            else if (currentState == GameState.Paused)
            {
                Time.timeScale = 1f;
                SetState(GameState.Playing);
            }
        }

        /// <summary>
        /// Called by PlayerController when the player dies.
        /// Decrements lives, triggers respawn or game over.
        /// </summary>
        public void PlayerDied()
        {
            lives--;
            OnLivesChanged?.Invoke(lives);

            if (lives <= 0)
            {
                GameOver();
            }
            else
            {
                // Respawn after a short delay
                if (playerRef != null)
                {
                    playerRef.gameObject.SetActive(false);
                    StartCoroutine(RespawnPlayer());
                }
            }
        }

        private System.Collections.IEnumerator RespawnPlayer()
        {
            yield return new WaitForSeconds(1.5f);
            if (playerRef != null && currentState == GameState.Playing)
            {
                playerRef.Respawn(playerSpawnPosition);
            }
        }

        /// <summary>
        /// Transitions to game over state.
        /// </summary>
        private void GameOver()
        {
            SetState(GameState.GameOver);
            if (spawnerRef != null)
                spawnerRef.StopSpawning();

            // Update high score
            if (score > highScore)
            {
                highScore = score;
                PlayerPrefs.SetInt(HIGH_SCORE_KEY, highScore);
                PlayerPrefs.Save();
            }

            // Destroy remaining enemies and bullets
            DestroyAllTagged("Enemy");
            DestroyAllTagged("EnemyBullet");
            DestroyAllTagged("PlayerBullet");
            DestroyAllTagged("PowerUp");

            // Load game over scene after short delay
            StartCoroutine(LoadGameOverDelayed());
        }

        private System.Collections.IEnumerator LoadGameOverDelayed()
        {
            yield return new WaitForSeconds(2f);
            SceneManager.LoadScene("GameOver");
        }

        private void DestroyAllTagged(string tag)
        {
            GameObject[] objects = GameObject.FindGameObjectsWithTag(tag);
            foreach (GameObject go in objects)
                Destroy(go);
        }

        /// <summary>
        /// Adds to the score and fires the score changed event.
        /// </summary>
        public void AddScore(int amount)
        {
            score += amount;
            OnScoreChanged?.Invoke(score);
        }

        /// <summary>
        /// Grants the player an extra life.
        /// </summary>
        public void AddLife()
        {
            lives++;
            OnLivesChanged?.Invoke(lives);
        }

        /// <summary>
        /// Spawns a random power-up at the given position.
        /// </summary>
        public void SpawnRandomPowerUp(Vector3 position)
        {
            if (powerUpPrefabs == null || powerUpPrefabs.Length == 0) return;
            int idx = UnityEngine.Random.Range(0, powerUpPrefabs.Length);
            if (powerUpPrefabs[idx] != null)
            {
                Instantiate(powerUpPrefabs[idx], position, Quaternion.identity);
            }
        }

        /// <summary>
        /// Announces a new wave for UI display.
        /// </summary>
        public void AnnounceWave(int waveNumber, bool isBoss)
        {
            OnWaveAnnounce?.Invoke(waveNumber, isBoss);
        }

        /// <summary>
        /// Called when the boss is defeated.
        /// </summary>
        public void BossDefeated()
        {
            AddScore(2000);
            if (spawnerRef != null)
                spawnerRef.OnBossDefeated();
        }

        /// <summary>
        /// Quits the application.
        /// </summary>
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
            currentState = newState;
            OnGameStateChanged?.Invoke(newState);
        }

        private void Update()
        {
            // Global pause toggle
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (currentState == GameState.Playing || currentState == GameState.Paused)
                    TogglePause();
            }
        }
    }
}

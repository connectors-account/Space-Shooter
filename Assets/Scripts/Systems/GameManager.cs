using UnityEngine;
using UnityEngine.SceneManagement;
using System;

/// <summary>
/// GameManager is the central controller for game state.
/// Handles game flow, pausing, game over, and scene management.
/// </summary>
public class GameManager : MonoBehaviour
{
    // Singleton instance
    public static GameManager Instance { get; private set; }

    [Header("Game Settings")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private string gameSceneName = "GameScene";

    [Header("References")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Vector3 playerSpawnPosition = new Vector3(0, -3f, 0);

    // Game state
    private bool isPaused;
    private bool isGameOver;
    private bool isPlaying;
    private GameObject playerInstance;

    // Events
    public static event Action OnGameStart;
    public static event Action OnGamePause;
    public static event Action OnGameResume;
    public static event Action OnGameOver;
    public static event Action OnGameRestart;

    public bool IsPaused => isPaused;
    public bool IsGameOver => isGameOver;
    public bool IsPlaying => isPlaying;
    public GameObject Player => playerInstance;

    private void Awake()
    {
        // Singleton setup with persistence
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        PlayerHealth.OnPlayerDeath += HandlePlayerDeath;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        PlayerHealth.OnPlayerDeath -= HandlePlayerDeath;
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Update()
    {
        // Handle pause input
        if (isPlaying && !isGameOver && Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    /// <summary>
    /// Called when a scene is loaded
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == gameSceneName)
        {
            InitializeGame();
        }
    }

    /// <summary>
    /// Initialize game state for a new game
    /// </summary>
    private void InitializeGame()
    {
        isPaused = false;
        isGameOver = false;
        isPlaying = false;
        Time.timeScale = 1f;

        // Find or spawn player
        playerInstance = GameObject.FindGameObjectWithTag("Player");
        if (playerInstance == null && playerPrefab != null)
        {
            playerInstance = Instantiate(playerPrefab, playerSpawnPosition, Quaternion.identity);
        }
    }

    /// <summary>
    /// Start the game (called from UI)
    /// </summary>
    public void StartGame()
    {
        isPaused = false;
        isGameOver = false;
        isPlaying = true;
        Time.timeScale = 1f;

        // Reset score
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.ResetScore();
        }

        // Start waves
        if (WaveManager.Instance != null)
        {
            WaveManager.Instance.StartWaves();
        }

        OnGameStart?.Invoke();
        Debug.Log("Game Started!");
    }

    /// <summary>
    /// Toggle pause state
    /// </summary>
    public void TogglePause()
    {
        if (isPaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }

    /// <summary>
    /// Pause the game
    /// </summary>
    public void PauseGame()
    {
        if (isGameOver) return;

        isPaused = true;
        Time.timeScale = 0f;
        OnGamePause?.Invoke();
        Debug.Log("Game Paused");
    }

    /// <summary>
    /// Resume the game
    /// </summary>
    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        OnGameResume?.Invoke();
        Debug.Log("Game Resumed");
    }

    /// <summary>
    /// Handle player death
    /// </summary>
    private void HandlePlayerDeath()
    {
        if (isGameOver) return;

        isGameOver = true;
        isPlaying = false;

        // Stop waves
        if (WaveManager.Instance != null)
        {
            WaveManager.Instance.StopWaves();
        }

        // Clear all bullets
        if (ObjectPooler.Instance != null)
        {
            ObjectPooler.Instance.ReturnAllToPool("PlayerBullet");
            ObjectPooler.Instance.ReturnAllToPool("EnemyBullet");
        }

        OnGameOver?.Invoke();
        Debug.Log("Game Over!");
    }

    /// <summary>
    /// Restart the current game
    /// </summary>
    public void RestartGame()
    {
        Time.timeScale = 1f;
        OnGameRestart?.Invoke();
        SceneManager.LoadScene(gameSceneName);
    }

    /// <summary>
    /// Return to main menu
    /// </summary>
    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        isPaused = false;
        isGameOver = false;
        isPlaying = false;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    /// <summary>
    /// Load the game scene from main menu
    /// </summary>
    public void LoadGameScene()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    /// <summary>
    /// Quit the application
    /// </summary>
    public void QuitGame()
    {
        Debug.Log("Quitting game...");
        
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}

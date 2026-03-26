// ============================================================================
// GameManager.cs — Central game state controller (singleton)
// Handles game states, score, lives, wave progression, pause/resume
// ============================================================================
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public enum GameState
{
    MainMenu,
    Playing,
    Paused,
    GameOver,
    Victory
}

public class GameManager : MonoBehaviour
{
    // ---- Singleton ----
    public static GameManager Instance { get; private set; }

    // ---- Events ----
    public static event Action<GameState> OnGameStateChanged;
    public static event Action<int> OnScoreChanged;
    public static event Action<int> OnLivesChanged;
    public static event Action<int> OnWaveChanged;
    public static event Action<int> OnHighScoreChanged;

    // ---- Inspector Settings ----
    [Header("Game Settings")]
    [SerializeField] private int startingLives = 3;
    [SerializeField] private int maxLives = 5;
    [SerializeField] private int scoreForExtraLife = 10000;
    [SerializeField] private int totalWaves = 10;

    [Header("References")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform playerSpawnPoint;

    // ---- Runtime State ----
    private GameState currentState = GameState.MainMenu;
    private int score;
    private int lives;
    private int currentWave;
    private int highScore;
    private int nextExtraLifeScore;
    private bool isInitialized;

    // ---- Properties ----
    public GameState CurrentState => currentState;
    public int Score => score;
    public int Lives => lives;
    public int CurrentWave => currentWave;
    public int HighScore => highScore;
    public int TotalWaves => totalWaves;
    public int MaxLives => maxLives;
    public int StartingLives => startingLives;
    public GameObject PlayerShip { get; private set; }

    // =========================================================================
    // Unity Lifecycle
    // =========================================================================
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        highScore = PlayerPrefs.GetInt("HighScore", 0);
    }

    private void Start()
    {
        // If we're already in the game scene, auto-start
        if (SceneManager.GetActiveScene().name == "GameScene")
        {
            StartGame();
        }
        else
        {
            SetState(GameState.MainMenu);
        }
    }

    private void Update()
    {
        if (currentState == GameState.Playing || currentState == GameState.Paused)
        {
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
            {
                TogglePause();
            }
        }
    }

    // =========================================================================
    // State Management
    // =========================================================================
    public void SetState(GameState newState)
    {
        if (currentState == newState) return;

        currentState = newState;

        switch (newState)
        {
            case GameState.MainMenu:
                Time.timeScale = 1f;
                break;
            case GameState.Playing:
                Time.timeScale = 1f;
                break;
            case GameState.Paused:
                Time.timeScale = 0f;
                break;
            case GameState.GameOver:
                Time.timeScale = 1f;
                SaveHighScore();
                break;
            case GameState.Victory:
                Time.timeScale = 1f;
                SaveHighScore();
                break;
        }

        OnGameStateChanged?.Invoke(newState);
        Debug.Log($"[GameManager] State changed to: {newState}");
    }

    public void TogglePause()
    {
        if (currentState == GameState.Playing)
            SetState(GameState.Paused);
        else if (currentState == GameState.Paused)
            SetState(GameState.Playing);
    }

    // =========================================================================
    // Game Flow
    // =========================================================================
    public void StartGame()
    {
        score = 0;
        lives = startingLives;
        currentWave = 0;
        nextExtraLifeScore = scoreForExtraLife;
        isInitialized = true;

        OnScoreChanged?.Invoke(score);
        OnLivesChanged?.Invoke(lives);

        SpawnPlayer();
        SetState(GameState.Playing);

        Debug.Log("[GameManager] Game started!");
    }

    public void LoadGameScene()
    {
        SceneManager.LoadScene("GameScene");
    }

    public void LoadMainMenu()
    {
        SetState(GameState.MainMenu);
        SceneManager.LoadScene("MainMenu");
    }

    public void RestartGame()
    {
        SceneManager.LoadScene("GameScene");
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

    // =========================================================================
    // Player Management
    // =========================================================================
    private void SpawnPlayer()
    {
        if (playerPrefab == null)
        {
            Debug.LogWarning("[GameManager] playerPrefab not assigned. Looking in scene...");
            PlayerShip = GameObject.FindGameObjectWithTag("Player");
            return;
        }

        Vector3 spawnPos = playerSpawnPoint != null
            ? playerSpawnPoint.position
            : new Vector3(0f, -3.5f, 0f);

        PlayerShip = Instantiate(playerPrefab, spawnPos, Quaternion.identity);
    }

    public void RespawnPlayer()
    {
        if (lives <= 0)
        {
            SetState(GameState.GameOver);
            return;
        }

        Vector3 spawnPos = playerSpawnPoint != null
            ? playerSpawnPoint.position
            : new Vector3(0f, -3.5f, 0f);

        if (playerPrefab != null)
        {
            PlayerShip = Instantiate(playerPrefab, spawnPos, Quaternion.identity);

            // Brief invincibility on respawn
            var health = PlayerShip.GetComponent<PlayerHealth>();
            if (health != null) health.SetInvincible(2f);
        }
    }

    // =========================================================================
    // Score
    // =========================================================================
    public void AddScore(int points)
    {
        if (currentState != GameState.Playing) return;

        score += points;
        OnScoreChanged?.Invoke(score);

        // Extra life check
        if (score >= nextExtraLifeScore)
        {
            AddLife(1);
            nextExtraLifeScore += scoreForExtraLife;
        }
    }

    private void SaveHighScore()
    {
        if (score > highScore)
        {
            highScore = score;
            PlayerPrefs.SetInt("HighScore", highScore);
            PlayerPrefs.Save();
            OnHighScoreChanged?.Invoke(highScore);
        }
    }

    // =========================================================================
    // Lives
    // =========================================================================
    public void LoseLife()
    {
        lives--;
        lives = Mathf.Max(lives, 0);
        OnLivesChanged?.Invoke(lives);

        if (lives <= 0)
        {
            SetState(GameState.GameOver);
        }
        else
        {
            // Delay respawn slightly
            Invoke(nameof(RespawnPlayer), 1.5f);
        }
    }

    public void AddLife(int amount)
    {
        lives = Mathf.Min(lives + amount, maxLives);
        OnLivesChanged?.Invoke(lives);
    }

    // =========================================================================
    // Waves
    // =========================================================================
    public void AdvanceWave()
    {
        currentWave++;
        OnWaveChanged?.Invoke(currentWave);

        if (currentWave > totalWaves)
        {
            SetState(GameState.Victory);
        }

        Debug.Log($"[GameManager] Wave {currentWave} started");
    }

    public float GetDifficultyMultiplier()
    {
        // Scales from 1.0 at wave 1 to 2.5 at wave 10
        return 1f + (currentWave - 1) * 0.167f;
    }
}

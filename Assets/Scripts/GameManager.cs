using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

/// <summary>
/// Central game manager handling game state, scoring, and overall game flow.
/// Implements singleton pattern for global access.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    [Header("Game Settings")]
    [SerializeField] private int startingLives = 3;
    [SerializeField] private float gameStartDelay = 2f;
    
    [Header("Prefab References")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private GameObject[] powerUpPrefabs;
    
    [Header("Spawn Points")]
    [SerializeField] private Transform playerSpawnPoint;
    
    // Game State
    public enum GameState { Menu, Playing, Paused, GameOver }
    private GameState currentState = GameState.Menu;
    
    // Score and progression
    private int score;
    private int highScore;
    private int currentWave;
    private int enemiesRemaining;
    private int lives;
    
    // Properties
    public GameState CurrentState => currentState;
    public bool IsPlaying => currentState == GameState.Playing;
    public bool IsPaused => currentState == GameState.Paused;
    public int Score => score;
    public int HighScore => highScore;
    public int CurrentWave => currentWave;
    public int Lives => lives;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        LoadHighScore();
    }
    
    private void Update()
    {
        // Pause toggle
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (currentState == GameState.Playing)
            {
                PauseGame();
            }
            else if (currentState == GameState.Paused)
            {
                ResumeGame();
            }
        }
    }
    
    /// <summary>
    /// Start a new game
    /// </summary>
    public void StartGame()
    {
        score = 0;
        currentWave = 0;
        lives = startingLives;
        enemiesRemaining = 0;
        
        currentState = GameState.Playing;
        Time.timeScale = 1f;
        
        UIManager.Instance?.UpdateScore(score);
        UIManager.Instance?.UpdateWave(currentWave);
        UIManager.Instance?.ShowGameUI();
        
        // Spawn player
        SpawnPlayer();
        
        // Start first wave after delay
        Invoke(nameof(StartNextWave), gameStartDelay);
    }
    
    /// <summary>
    /// Pause the game
    /// </summary>
    public void PauseGame()
    {
        if (currentState != GameState.Playing) return;
        
        currentState = GameState.Paused;
        Time.timeScale = 0f;
        
        MenuManager.Instance?.ShowPauseMenu();
        AudioManager.Instance?.PlaySound("Pause");
    }
    
    /// <summary>
    /// Resume the game
    /// </summary>
    public void ResumeGame()
    {
        if (currentState != GameState.Paused) return;
        
        currentState = GameState.Playing;
        Time.timeScale = 1f;
        
        MenuManager.Instance?.HidePauseMenu();
    }
    
    /// <summary>
    /// End the game
    /// </summary>
    public void GameOver()
    {
        currentState = GameState.GameOver;
        
        // Update high score
        if (score > highScore)
        {
            highScore = score;
            SaveHighScore();
        }
        
        CancelInvoke();
        WaveManager.Instance?.StopSpawning();
        
        MenuManager.Instance?.ShowGameOverMenu(score, highScore);
        AudioManager.Instance?.PlaySound("GameOver");
    }
    
    /// <summary>
    /// Return to main menu
    /// </summary>
    public void ReturnToMenu()
    {
        currentState = GameState.Menu;
        Time.timeScale = 1f;
        
        // Clean up game objects
        CleanupGame();
        
        MenuManager.Instance?.ShowMainMenu();
    }
    
    /// <summary>
    /// Quit the game
    /// </summary>
    public void QuitGame()
    {
        SaveHighScore();
        
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
    
    /// <summary>
    /// Add score points
    /// </summary>
    public void AddScore(int points)
    {
        score += points;
        UIManager.Instance?.UpdateScore(score);
    }
    
    /// <summary>
    /// Called when an enemy is destroyed
    /// </summary>
    public void EnemyDestroyed()
    {
        enemiesRemaining--;
        
        if (enemiesRemaining <= 0 && currentState == GameState.Playing)
        {
            // Wave completed, start next wave
            Invoke(nameof(StartNextWave), 2f);
        }
    }
    
    /// <summary>
    /// Set the number of enemies in current wave
    /// </summary>
    public void SetEnemyCount(int count)
    {
        enemiesRemaining = count;
    }
    
    /// <summary>
    /// Start the next wave
    /// </summary>
    public void StartNextWave()
    {
        if (currentState != GameState.Playing) return;
        
        currentWave++;
        UIManager.Instance?.UpdateWave(currentWave);
        UIManager.Instance?.ShowWaveAnnouncement(currentWave);
        
        WaveManager.Instance?.StartWave(currentWave);
        AudioManager.Instance?.PlaySound("WaveStart");
    }
    
    /// <summary>
    /// Spawn player at spawn point
    /// </summary>
    private void SpawnPlayer()
    {
        if (playerPrefab == null)
        {
            Debug.LogError("Player prefab not assigned!");
            return;
        }
        
        Vector3 spawnPos = playerSpawnPoint != null ? playerSpawnPoint.position : new Vector3(0, -3, 0);
        Instantiate(playerPrefab, spawnPos, Quaternion.identity);
    }
    
    /// <summary>
    /// Spawn explosion effect at position
    /// </summary>
    public void SpawnExplosion(Vector3 position)
    {
        if (explosionPrefab != null)
        {
            GameObject explosion = Instantiate(explosionPrefab, position, Quaternion.identity);
            Destroy(explosion, 1f);
        }
    }
    
    /// <summary>
    /// Spawn random power-up at position
    /// </summary>
    public void SpawnPowerUp(Vector3 position)
    {
        if (powerUpPrefabs == null || powerUpPrefabs.Length == 0) return;
        
        int index = Random.Range(0, powerUpPrefabs.Length);
        if (powerUpPrefabs[index] != null)
        {
            Instantiate(powerUpPrefabs[index], position, Quaternion.identity);
        }
    }
    
    /// <summary>
    /// Clean up all game objects
    /// </summary>
    private void CleanupGame()
    {
        // Destroy all enemies
        foreach (var enemy in FindObjectsOfType<EnemyController>())
        {
            Destroy(enemy.gameObject);
        }
        
        // Destroy all bullets
        foreach (var bullet in FindObjectsOfType<BulletController>())
        {
            Destroy(bullet.gameObject);
        }
        
        // Destroy all power-ups
        foreach (var powerUp in FindObjectsOfType<PowerUpController>())
        {
            Destroy(powerUp.gameObject);
        }
        
        // Destroy player
        if (PlayerController.Instance != null)
        {
            Destroy(PlayerController.Instance.gameObject);
        }
    }
    
    private void SaveHighScore()
    {
        PlayerPrefs.SetInt("HighScore", highScore);
        PlayerPrefs.Save();
    }
    
    private void LoadHighScore()
    {
        highScore = PlayerPrefs.GetInt("HighScore", 0);
    }
}

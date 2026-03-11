using UnityEngine;
using UnityEngine.SceneManagement;
using System;

/// <summary>
/// Central game manager that controls game state and flow.
/// This is a singleton - only one instance should exist.
/// </summary>
public class GameManager : MonoBehaviour
{
    /// <summary>
    /// Possible game states.
    /// </summary>
    public enum GameState
    {
        MainMenu,
        Playing,
        Paused,
        GameOver
    }
    
    [Header("Game Settings")]
    [Tooltip("Starting number of player lives")]
    [SerializeField] private int startingLives = 3;
    
    [Header("References")]
    [Tooltip("Player GameObject reference")]
    [SerializeField] private GameObject playerPrefab;
    
    [Tooltip("Player spawn position")]
    [SerializeField] private Vector3 playerSpawnPosition = new Vector3(0f, -3.5f, 0f);
    
    // Singleton instance
    private static GameManager instance;
    public static GameManager Instance => instance;
    
    // Current game state
    private GameState currentState = GameState.MainMenu;
    
    // Player reference
    private GameObject playerInstance;
    private int currentLives;
    
    // Events
    public event Action<GameState> OnGameStateChanged;
    public event Action<int> OnLivesChanged;
    
    // Properties
    public GameState CurrentState => currentState;
    public bool IsGameActive => currentState == GameState.Playing;
    public bool IsPaused => currentState == GameState.Paused;
    public int CurrentLives => currentLives;
    
    /// <summary>
    /// Initialize singleton on awake.
    /// </summary>
    private void Awake()
    {
        // Singleton pattern
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        // Initialize state
        currentLives = startingLives;
    }
    
    /// <summary>
    /// Handle input for pause.
    /// </summary>
    private void Update()
    {
        // Handle pause input
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
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
    /// Start a new game.
    /// </summary>
    public void StartGame()
    {
        Debug.Log("Starting new game...");
        
        // Reset game state
        currentLives = startingLives;
        OnLivesChanged?.Invoke(currentLives);
        
        // Reset score
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.ResetScore();
        }
        
        // Create player
        SpawnPlayer();
        
        // Start enemy spawning
        if (EnemySpawner.Instance != null)
        {
            EnemySpawner.Instance.Reset();
            EnemySpawner.Instance.StartSpawning();
        }
        
        // Set game state
        SetGameState(GameState.Playing);
    }
    
    /// <summary>
    /// Spawn or respawn the player.
    /// </summary>
    private void SpawnPlayer()
    {
        // Destroy existing player if any
        if (playerInstance != null)
        {
            Destroy(playerInstance);
        }
        
        if (playerPrefab != null)
        {
            playerInstance = Instantiate(playerPrefab, playerSpawnPosition, Quaternion.identity);
        }
        else
        {
            // Create default player
            playerInstance = CreateDefaultPlayer();
        }
        
        // Subscribe to player death event
        PlayerHealth health = playerInstance.GetComponent<PlayerHealth>();
        if (health != null)
        {
            health.OnPlayerDeath += OnPlayerDeath;
        }
    }
    
    /// <summary>
    /// Create a default player when no prefab is assigned.
    /// </summary>
    /// <returns>Created player GameObject</returns>
    private GameObject CreateDefaultPlayer()
    {
        // Create player from primitive
        GameObject player = GameObject.CreatePrimitive(PrimitiveType.Cube);
        player.name = "Player";
        player.transform.position = playerSpawnPosition;
        player.transform.localScale = new Vector3(0.8f, 1f, 0.8f);
        player.tag = "Player";
        
        // Remove 3D collider
        Destroy(player.GetComponent<BoxCollider>());
        
        // Add 2D collider
        BoxCollider2D collider = player.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        collider.size = new Vector2(0.6f, 0.8f);
        
        // Add Rigidbody2D
        Rigidbody2D rb = player.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        
        // Add player components
        player.AddComponent<PlayerController>();
        player.AddComponent<PlayerHealth>();
        player.AddComponent<PlayerShooting>();
        
        // Set player color
        MeshRenderer renderer = player.GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            renderer.material.color = Color.cyan;
        }
        
        return player;
    }
    
    /// <summary>
    /// Handle player death event.
    /// </summary>
    private void OnPlayerDeath()
    {
        currentLives--;
        OnLivesChanged?.Invoke(currentLives);
        
        if (currentLives > 0)
        {
            // Respawn after delay
            StartCoroutine(RespawnPlayerCoroutine());
        }
        else
        {
            GameOver();
        }
    }
    
    /// <summary>
    /// Coroutine to respawn player after a delay.
    /// </summary>
    private System.Collections.IEnumerator RespawnPlayerCoroutine()
    {
        yield return new WaitForSeconds(2f);
        
        if (currentState == GameState.Playing)
        {
            SpawnPlayer();
        }
    }
    
    /// <summary>
    /// Pause the game.
    /// </summary>
    public void PauseGame()
    {
        if (currentState != GameState.Playing) return;
        
        Time.timeScale = 0f;
        SetGameState(GameState.Paused);
        
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowPauseMenu();
        }
    }
    
    /// <summary>
    /// Resume the game from pause.
    /// </summary>
    public void ResumeGame()
    {
        if (currentState != GameState.Paused) return;
        
        Time.timeScale = 1f;
        SetGameState(GameState.Playing);
        
        if (UIManager.Instance != null)
        {
            UIManager.Instance.HidePauseMenu();
        }
    }
    
    /// <summary>
    /// End the game (game over).
    /// </summary>
    public void GameOver()
    {
        Debug.Log("Game Over!");
        
        // Stop spawning enemies
        if (EnemySpawner.Instance != null)
        {
            EnemySpawner.Instance.StopSpawning();
        }
        
        SetGameState(GameState.GameOver);
        
        // Show game over UI
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowGameOverScreen();
        }
        
        // Check for high score
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.CheckHighScore();
        }
    }
    
    /// <summary>
    /// Restart the game.
    /// </summary>
    public void RestartGame()
    {
        // Clear all enemies and bullets
        ClearGameObjects();
        
        // Ensure time is running
        Time.timeScale = 1f;
        
        // Start new game
        StartGame();
    }
    
    /// <summary>
    /// Return to main menu.
    /// </summary>
    public void ReturnToMainMenu()
    {
        // Clear all enemies and bullets
        ClearGameObjects();
        
        // Ensure time is running
        Time.timeScale = 1f;
        
        SetGameState(GameState.MainMenu);
        
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowMainMenu();
        }
    }
    
    /// <summary>
    /// Clear all game objects (enemies, bullets, etc.)
    /// </summary>
    private void ClearGameObjects()
    {
        // Destroy player
        if (playerInstance != null)
        {
            Destroy(playerInstance);
        }
        
        // Destroy all enemies
        foreach (GameObject enemy in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            Destroy(enemy);
        }
        
        // Destroy all player bullets
        foreach (GameObject bullet in GameObject.FindGameObjectsWithTag("PlayerBullet"))
        {
            Destroy(bullet);
        }
        
        // Destroy all enemy bullets
        foreach (GameObject bullet in GameObject.FindGameObjectsWithTag("EnemyBullet"))
        {
            Destroy(bullet);
        }
    }
    
    /// <summary>
    /// Set the current game state and notify listeners.
    /// </summary>
    /// <param name="newState">New game state</param>
    private void SetGameState(GameState newState)
    {
        if (currentState != newState)
        {
            currentState = newState;
            OnGameStateChanged?.Invoke(currentState);
            Debug.Log($"Game state changed to: {currentState}");
        }
    }
    
    /// <summary>
    /// Quit the application.
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
    
    /// <summary>
    /// Get the player GameObject.
    /// </summary>
    /// <returns>Player GameObject or null</returns>
    public GameObject GetPlayer()
    {
        return playerInstance;
    }
}

using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// GameManager controls the overall game state.
/// Handles game flow: menu, playing, paused, game over.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameState
    {
        MainMenu,
        Playing,
        Paused,
        GameOver
    }

    [Header("Game State")]
    public GameState currentState = GameState.MainMenu;

    [Header("References")]
    public GameObject player;
    public EnemySpawner enemySpawner;
    public PowerUpSpawner powerUpSpawner;

    [Header("Audio")]
    public AudioClip backgroundMusic;
    public AudioClip gameOverSound;
    private AudioSource musicSource;

    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        // Setup audio source for music
        musicSource = GetComponent<AudioSource>();
        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
        }
        musicSource.loop = true;
        
        // Find references if not assigned
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
        }
        if (enemySpawner == null)
        {
            enemySpawner = FindObjectOfType<EnemySpawner>();
        }
        if (powerUpSpawner == null)
        {
            powerUpSpawner = FindObjectOfType<PowerUpSpawner>();
        }
        
        // Start at main menu
        SetState(GameState.MainMenu);
    }

    void Update()
    {
        // Handle pause toggle
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
        
        // Quick restart with R key when game over
        if (currentState == GameState.GameOver && Input.GetKeyDown(KeyCode.R))
        {
            RestartGame();
        }
    }

    /// <summary>
    /// Set the current game state
    /// </summary>
    void SetState(GameState newState)
    {
        currentState = newState;
        
        switch (newState)
        {
            case GameState.MainMenu:
                Time.timeScale = 1f;
                if (player != null) player.SetActive(false);
                if (UIManager.Instance != null) UIManager.Instance.ShowMainMenu();
                break;
                
            case GameState.Playing:
                Time.timeScale = 1f;
                if (player != null) player.SetActive(true);
                if (UIManager.Instance != null) UIManager.Instance.ShowGameUI();
                break;
                
            case GameState.Paused:
                Time.timeScale = 0f;
                if (UIManager.Instance != null) UIManager.Instance.ShowPauseMenu();
                break;
                
            case GameState.GameOver:
                Time.timeScale = 1f;
                if (UIManager.Instance != null) UIManager.Instance.ShowGameOver();
                break;
        }
    }

    /// <summary>
    /// Start a new game
    /// </summary>
    public void StartGame()
    {
        // Reset systems
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.ResetScore();
        }
        
        if (enemySpawner != null)
        {
            enemySpawner.ResetSpawner();
            enemySpawner.StartSpawning();
        }
        
        if (powerUpSpawner != null)
        {
            powerUpSpawner.ResetSpawner();
            powerUpSpawner.StartSpawning();
        }
        
        // Reset player
        if (player != null)
        {
            player.SetActive(true);
            
            PlayerController controller = player.GetComponent<PlayerController>();
            if (controller != null)
            {
                controller.ResetPlayer();
            }
            
            PlayerHealth health = player.GetComponent<PlayerHealth>();
            if (health != null)
            {
                health.ResetHealth();
            }
        }
        
        // Play background music
        if (backgroundMusic != null && musicSource != null)
        {
            musicSource.clip = backgroundMusic;
            musicSource.Play();
        }
        
        SetState(GameState.Playing);
    }

    /// <summary>
    /// Pause the game
    /// </summary>
    public void PauseGame()
    {
        if (currentState == GameState.Playing)
        {
            SetState(GameState.Paused);
        }
    }

    /// <summary>
    /// Resume the game from pause
    /// </summary>
    public void ResumeGame()
    {
        if (currentState == GameState.Paused)
        {
            SetState(GameState.Playing);
        }
    }

    /// <summary>
    /// Called when the player dies
    /// </summary>
    public void GameOver()
    {
        // Stop spawning
        if (enemySpawner != null)
        {
            enemySpawner.StopSpawning();
        }
        
        if (powerUpSpawner != null)
        {
            powerUpSpawner.StopSpawning();
        }
        
        // Stop music
        if (musicSource != null)
        {
            musicSource.Stop();
        }
        
        // Play game over sound
        if (gameOverSound != null)
        {
            AudioSource.PlayClipAtPoint(gameOverSound, Camera.main.transform.position);
        }
        
        SetState(GameState.GameOver);
    }

    /// <summary>
    /// Restart the game
    /// </summary>
    public void RestartGame()
    {
        StartGame();
    }

    /// <summary>
    /// Return to main menu
    /// </summary>
    public void GoToMainMenu()
    {
        // Stop spawning
        if (enemySpawner != null)
        {
            enemySpawner.ResetSpawner();
        }
        
        if (powerUpSpawner != null)
        {
            powerUpSpawner.ResetSpawner();
        }
        
        // Stop music
        if (musicSource != null)
        {
            musicSource.Stop();
        }
        
        SetState(GameState.MainMenu);
    }

    /// <summary>
    /// Quit the application
    /// </summary>
    public void QuitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    /// <summary>
    /// Check if game is paused
    /// </summary>
    public bool IsGamePaused()
    {
        return currentState == GameState.Paused || currentState == GameState.MainMenu || currentState == GameState.GameOver;
    }

    /// <summary>
    /// Get current game state
    /// </summary>
    public GameState GetCurrentState()
    {
        return currentState;
    }
}

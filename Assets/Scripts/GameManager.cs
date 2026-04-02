// =============================================================================
// GameManager.cs
// Central game state manager. Handles scoring, wave progression, game states
// (playing, paused, game over), and scene transitions.
// This is a singleton — only one instance exists at any time.
// Create an empty GameObject named "GameManager" and attach this script.
// =============================================================================
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Represents the possible states of the game.
/// </summary>
public enum GameState
{
    Menu,
    Playing,
    Paused,
    GameOver
}

public class GameManager : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Singleton
    // -------------------------------------------------------------------------
    public static GameManager Instance { get; private set; }

    // -------------------------------------------------------------------------
    // Game State
    // -------------------------------------------------------------------------
    [Header("Game State")]
    public GameState currentState = GameState.Menu;

    // -------------------------------------------------------------------------
    // Score
    // -------------------------------------------------------------------------
    [Header("Score")]
    [Tooltip("Current player score.")]
    private int score = 0;

    [Tooltip("Highest score achieved (persisted with PlayerPrefs).")]
    private int highScore = 0;

    // -------------------------------------------------------------------------
    // Wave System
    // -------------------------------------------------------------------------
    [Header("Wave System")]
    [Tooltip("Current wave number.")]
    private int currentWave = 0;

    [Tooltip("Total number of enemies destroyed.")]
    private int totalEnemiesDestroyed = 0;

    // -------------------------------------------------------------------------
    // Unity Lifecycle
    // -------------------------------------------------------------------------

    /// <summary>
    /// Enforce the singleton pattern. Persist across scene loads.
    /// </summary>
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Load high score from persistent storage
        highScore = PlayerPrefs.GetInt("HighScore", 0);
    }

    /// <summary>
    /// Subscribe to scene loaded events.
    /// </summary>
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    /// <summary>
    /// Unsubscribe from scene loaded events.
    /// </summary>
    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    /// <summary>
    /// Called when a new scene finishes loading.
    /// Resets game state if the gameplay scene is loaded.
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "GamePlay")
        {
            StartNewGame();
        }
    }

    /// <summary>
    /// Handle pause input every frame.
    /// </summary>
    void Update()
    {
        // Toggle pause with Escape key during gameplay
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

    // -------------------------------------------------------------------------
    // Game Flow
    // -------------------------------------------------------------------------

    /// <summary>
    /// Resets score and wave count, then starts gameplay.
    /// </summary>
    public void StartNewGame()
    {
        score = 0;
        currentWave = 0;
        totalEnemiesDestroyed = 0;
        currentState = GameState.Playing;
        Time.timeScale = 1f;

        // Update UI
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateScore(score);
            UIManager.Instance.UpdateWave(currentWave);
        }
    }

    /// <summary>
    /// Pauses the game by setting timeScale to 0.
    /// </summary>
    public void PauseGame()
    {
        currentState = GameState.Paused;
        Time.timeScale = 0f;

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowPauseMenu(true);
        }
    }

    /// <summary>
    /// Resumes the game from pause.
    /// </summary>
    public void ResumeGame()
    {
        currentState = GameState.Playing;
        Time.timeScale = 1f;

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowPauseMenu(false);
        }
    }

    /// <summary>
    /// Called when the player dies. Transitions to GameOver state.
    /// </summary>
    public void OnPlayerDeath()
    {
        currentState = GameState.GameOver;

        // Check and save high score
        if (score > highScore)
        {
            highScore = score;
            PlayerPrefs.SetInt("HighScore", highScore);
            PlayerPrefs.Save();
        }

        // Wait briefly, then show game over screen
        Invoke("LoadGameOverScene", 1.5f);
    }

    /// <summary>
    /// Loads the GameOver scene.
    /// </summary>
    private void LoadGameOverScene()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("GameOver");
    }

    // -------------------------------------------------------------------------
    // Score Management
    // -------------------------------------------------------------------------

    /// <summary>
    /// Adds points to the player's score and updates the UI.
    /// </summary>
    /// <param name="points">Points to add.</param>
    public void AddScore(int points)
    {
        score += points;
        totalEnemiesDestroyed++;

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateScore(score);
        }
    }

    /// <summary>Returns the current score.</summary>
    public int GetScore() { return score; }

    /// <summary>Returns the all-time high score.</summary>
    public int GetHighScore() { return highScore; }

    // -------------------------------------------------------------------------
    // Wave Management
    // -------------------------------------------------------------------------

    /// <summary>
    /// Advances to the next wave and updates the UI.
    /// </summary>
    public void AdvanceWave()
    {
        currentWave++;

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateWave(currentWave);
            UIManager.Instance.ShowWaveAnnouncement(currentWave);
        }

        AudioManager.Instance?.PlaySFX("WaveStart");
    }

    /// <summary>Returns the current wave number.</summary>
    public int GetCurrentWave() { return currentWave; }

    /// <summary>Returns the total number of enemies destroyed.</summary>
    public int GetTotalEnemiesDestroyed() { return totalEnemiesDestroyed; }

    // -------------------------------------------------------------------------
    // Scene Navigation
    // -------------------------------------------------------------------------

    /// <summary>
    /// Loads the main menu scene.
    /// </summary>
    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        currentState = GameState.Menu;
        SceneManager.LoadScene("MainMenu");
    }

    /// <summary>
    /// Loads the gameplay scene.
    /// </summary>
    public void GoToGamePlay()
    {
        SceneManager.LoadScene("GamePlay");
    }

    /// <summary>
    /// Quits the application.
    /// </summary>
    public void QuitGame()
    {
        PlayerPrefs.Save();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}

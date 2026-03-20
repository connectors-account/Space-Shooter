using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Central game state manager: scoring, wave tracking, game flow.
/// Singleton pattern. Attach to a persistent GameObject.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private EnemySpawner enemySpawner;
    [SerializeField] private UIManager uiManager;
    [SerializeField] private PlayerController player;

    // Game State
    public enum GameState { MainMenu, Playing, Paused, GameOver }
    private GameState currentState = GameState.MainMenu;

    // Score
    private int score = 0;
    private int highScore = 0;
    private int currentWave = 0;

    // Properties
    public GameState CurrentState => currentState;
    public int Score => score;
    public int HighScore => highScore;
    public int CurrentWave => currentWave;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        highScore = PlayerPrefs.GetInt("HighScore", 0);
    }

    private void Start()
    {
        // If we're in the game scene, auto-start
        if (SceneManager.GetActiveScene().name == "GameScene")
        {
            StartGame();
        }
    }

    private void Update()
    {
        // Pause toggle
        if (currentState == GameState.Playing && Input.GetKeyDown(KeyCode.Escape))
        {
            PauseGame();
        }
        else if (currentState == GameState.Paused && Input.GetKeyDown(KeyCode.Escape))
        {
            ResumeGame();
        }
    }

    /// <summary>
    /// Starts a new game session.
    /// </summary>
    public void StartGame()
    {
        score = 0;
        currentWave = 0;
        currentState = GameState.Playing;
        Time.timeScale = 1f;

        if (player != null) player.ResetPlayer();
        if (enemySpawner != null)
        {
            enemySpawner.ClearAllEnemies();
            enemySpawner.StartSpawning();
        }
        if (uiManager != null)
        {
            uiManager.ShowHUD();
            uiManager.UpdateScore(score);
            uiManager.UpdateWave(currentWave);
            if (player != null)
                uiManager.UpdateHealthDisplay(player.CurrentHealth, player.MaxHealth);
        }
    }

    /// <summary>
    /// Adds score and updates the UI.
    /// </summary>
    public void AddScore(int points)
    {
        if (currentState != GameState.Playing) return;
        score += points;
        if (uiManager != null) uiManager.UpdateScore(score);
    }

    /// <summary>
    /// Called by the spawner when a new wave begins.
    /// </summary>
    public void OnNewWave(int wave)
    {
        currentWave = wave;
        if (uiManager != null) uiManager.UpdateWave(wave);
    }

    /// <summary>
    /// Called when an enemy is destroyed.
    /// </summary>
    public void EnemyDestroyed()
    {
        if (enemySpawner != null) enemySpawner.OnEnemyDestroyed();
    }

    /// <summary>
    /// Triggers the Game Over state.
    /// </summary>
    public void GameOver()
    {
        if (currentState == GameState.GameOver) return;

        currentState = GameState.GameOver;

        if (enemySpawner != null) enemySpawner.StopSpawning();

        // Update high score
        if (score > highScore)
        {
            highScore = score;
            PlayerPrefs.SetInt("HighScore", highScore);
            PlayerPrefs.Save();
        }

        if (uiManager != null) uiManager.ShowGameOver(score, highScore);

        AudioManager.Instance?.PlaySFX("GameOver");
    }

    /// <summary>
    /// Pauses the game.
    /// </summary>
    public void PauseGame()
    {
        if (currentState != GameState.Playing) return;
        currentState = GameState.Paused;
        Time.timeScale = 0f;
        if (uiManager != null) uiManager.ShowPauseMenu();
    }

    /// <summary>
    /// Resumes from pause.
    /// </summary>
    public void ResumeGame()
    {
        if (currentState != GameState.Paused) return;
        currentState = GameState.Playing;
        Time.timeScale = 1f;
        if (uiManager != null) uiManager.HidePauseMenu();
    }

    /// <summary>
    /// Restarts the game (reloads scene).
    /// </summary>
    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("GameScene");
    }

    /// <summary>
    /// Returns to the main menu.
    /// </summary>
    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenuScene");
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

using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Game state enum for managing game flow.
/// </summary>
public enum GameState
{
    MainMenu,
    Playing,
    Paused,
    GameOver
}

/// <summary>
/// Central game manager: handles score, lives, game states, and wave flow.
/// Singleton pattern.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Settings")]
    [SerializeField] private int startingLives = 3;

    // State
    private GameState currentState = GameState.MainMenu;
    private int score = 0;
    private int highScore = 0;
    private int lives;
    private int currentWave = 0;
    private int enemiesRemainingInWave = 0;
    private int enemiesSpawnedInWave = 0;

    // Properties
    public GameState CurrentState => currentState;
    public int Score => score;
    public int HighScore => highScore;
    public int Lives => lives;
    public int CurrentWave => currentWave;

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

    /// <summary>
    /// Start a new game session.
    /// </summary>
    public void StartGame()
    {
        score = 0;
        lives = startingLives;
        currentWave = 0;

        currentState = GameState.Playing;

        UIManager.Instance?.UpdateScore(score);
        UIManager.Instance?.UpdateLives(lives);
        UIManager.Instance?.UpdateHealthBar(1f);
        UIManager.Instance?.ShowHUD();

        StartNextWave();
    }

    /// <summary>
    /// Start the next wave of enemies.
    /// </summary>
    public void StartNextWave()
    {
        currentWave++;
        UIManager.Instance?.UpdateWave(currentWave);
        UIManager.Instance?.ShowWaveAnnouncement(currentWave);

        EnemySpawner spawner = FindObjectOfType<EnemySpawner>();
        if (spawner != null)
        {
            spawner.StartWave(currentWave);
        }
    }

    /// <summary>
    /// Called by EnemySpawner when wave setup completes.
    /// </summary>
    public void SetWaveEnemyCount(int count)
    {
        enemiesRemainingInWave = count;
        enemiesSpawnedInWave = count;
    }

    /// <summary>
    /// Called when an enemy is destroyed.
    /// </summary>
    public void OnEnemyDestroyed()
    {
        enemiesRemainingInWave--;

        if (enemiesRemainingInWave <= 0 && currentState == GameState.Playing)
        {
            // Brief delay before next wave
            StartCoroutine(WaveCompleteDelay());
        }
    }

    private System.Collections.IEnumerator WaveCompleteDelay()
    {
        yield return new WaitForSeconds(2f);
        if (currentState == GameState.Playing)
        {
            StartNextWave();
        }
    }

    public void AddScore(int points)
    {
        score += points;
        UIManager.Instance?.UpdateScore(score);

        if (score > highScore)
        {
            highScore = score;
            PlayerPrefs.SetInt("HighScore", highScore);
            PlayerPrefs.Save();
        }
    }

    /// <summary>
    /// Called when the player dies.
    /// </summary>
    public void OnPlayerDeath()
    {
        lives--;
        UIManager.Instance?.UpdateLives(lives);

        if (lives <= 0)
        {
            GameOver();
        }
        else
        {
            // Respawn after a delay
            StartCoroutine(RespawnPlayer());
        }
    }

    private System.Collections.IEnumerator RespawnPlayer()
    {
        yield return new WaitForSeconds(1.5f);
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            player.Respawn();
        }
    }

    private void GameOver()
    {
        currentState = GameState.GameOver;
        UIManager.Instance?.ShowGameOverScreen(score, highScore);
        AudioManager.Instance?.PlaySFX("GameOver");
    }

    public void PauseGame()
    {
        if (currentState != GameState.Playing) return;
        currentState = GameState.Paused;
        Time.timeScale = 0f;
        UIManager.Instance?.ShowPauseMenu();
    }

    public void ResumeGame()
    {
        if (currentState != GameState.Paused) return;
        currentState = GameState.Playing;
        Time.timeScale = 1f;
        UIManager.Instance?.HidePauseMenu();
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        currentState = GameState.MainMenu;

        // Clean up all enemies and bullets
        foreach (var enemy in FindObjectsOfType<EnemyBase>())
            Destroy(enemy.gameObject);
        foreach (var bullet in FindObjectsOfType<Bullet>())
            Destroy(bullet.gameObject);
        foreach (var powerUp in FindObjectsOfType<PowerUp>())
            Destroy(powerUp.gameObject);

        StartGame();
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        currentState = GameState.MainMenu;

        // Clean up gameplay objects
        foreach (var enemy in FindObjectsOfType<EnemyBase>())
            Destroy(enemy.gameObject);
        foreach (var bullet in FindObjectsOfType<Bullet>())
            Destroy(bullet.gameObject);
        foreach (var powerUp in FindObjectsOfType<PowerUp>())
            Destroy(powerUp.gameObject);

        UIManager.Instance?.ShowMainMenu();
    }

    private void Update()
    {
        // Pause toggle
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (currentState == GameState.Playing)
                PauseGame();
            else if (currentState == GameState.Paused)
                ResumeGame();
        }
    }
}

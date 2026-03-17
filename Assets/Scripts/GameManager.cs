using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Central game state manager. Handles game flow, scoring, and scene transitions.
/// Persists across scenes using DontDestroyOnLoad.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private PlayerController player;
    [SerializeField] private EnemySpawner enemySpawner;

    // Game state
    private int score;
    private int highScore;
    private bool isGameActive;
    private bool isPaused;

    // Public accessors
    public int Score => score;
    public int HighScore => highScore;
    public bool IsGameActive => isGameActive;
    public bool IsPaused => isPaused;

    private const string HIGH_SCORE_KEY = "HighScore";

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Load saved high score
        highScore = PlayerPrefs.GetInt(HIGH_SCORE_KEY, 0);
    }

    private void Update()
    {
        // Toggle pause with Escape key
        if (Input.GetKeyDown(KeyCode.Escape) && isGameActive)
        {
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    /// <summary>
    /// Start a new game session. Called from menu or restart.
    /// </summary>
    public void StartGame()
    {
        score = 0;
        isGameActive = true;
        isPaused = false;
        Time.timeScale = 1f;

        SceneManager.LoadScene("GamePlay");

        // References will be re-bound after scene load
        SceneManager.sceneLoaded += OnGamePlaySceneLoaded;
    }

    /// <summary>
    /// Called when the GamePlay scene finishes loading.
    /// Binds references and starts spawning.
    /// </summary>
    private void OnGamePlaySceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != "GamePlay") return;

        SceneManager.sceneLoaded -= OnGamePlaySceneLoaded;

        // Find references in the new scene
        player = FindObjectOfType<PlayerController>();
        enemySpawner = FindObjectOfType<EnemySpawner>();

        // Update UI
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateScoreText(score);
            UIManager.Instance.UpdateWaveText(0);
            if (player != null)
                UIManager.Instance.UpdateHealthBar(player.CurrentHealth, player.MaxHealth);
        }

        // Start enemy spawning
        if (enemySpawner != null)
            enemySpawner.StartSpawning();
    }

    /// <summary>
    /// Add points to the current score.
    /// </summary>
    public void AddScore(int points)
    {
        score += points;

        if (UIManager.Instance != null)
            UIManager.Instance.UpdateScoreText(score);
    }

    /// <summary>
    /// Called when a new wave begins.
    /// </summary>
    public void OnWaveStart(int waveNumber)
    {
        if (UIManager.Instance != null)
            UIManager.Instance.ShowWaveAnnouncement(waveNumber);
    }

    /// <summary>
    /// Pause the game and show pause menu.
    /// </summary>
    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;

        if (UIManager.Instance != null)
            UIManager.Instance.ShowPauseMenu(true);
    }

    /// <summary>
    /// Resume the game from pause.
    /// </summary>
    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;

        if (UIManager.Instance != null)
            UIManager.Instance.ShowPauseMenu(false);
    }

    /// <summary>
    /// Trigger game over state.
    /// </summary>
    public void GameOver()
    {
        isGameActive = false;
        Time.timeScale = 0f;

        // Update high score
        if (score > highScore)
        {
            highScore = score;
            PlayerPrefs.SetInt(HIGH_SCORE_KEY, highScore);
            PlayerPrefs.Save();
        }

        // Stop spawner
        if (enemySpawner != null)
            enemySpawner.StopSpawning();

        // Show game over screen
        if (UIManager.Instance != null)
            UIManager.Instance.ShowGameOverScreen(score, highScore);
    }

    /// <summary>
    /// Return to the main menu.
    /// </summary>
    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        isGameActive = false;
        isPaused = false;
        SceneManager.LoadScene("MainMenu");
    }

    /// <summary>
    /// Restart the current game.
    /// </summary>
    public void RestartGame()
    {
        Time.timeScale = 1f;
        StartGame();
    }

    /// <summary>
    /// Quit the application.
    /// </summary>
    public void QuitGame()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}

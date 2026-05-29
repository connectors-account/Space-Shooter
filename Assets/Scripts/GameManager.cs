using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Central game state manager. Handles score, wave progression,
/// game state transitions, and high score persistence.
/// Singleton pattern - persists across scenes.
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

    [Header("Power-Up Prefab")]
    [SerializeField] private GameObject powerUpPrefab;

    private GameState currentState = GameState.MainMenu;
    private int score = 0;
    private int highScore = 0;
    private int currentWave = 0;
    private EnemySpawner enemySpawner;

    // Properties
    public GameState CurrentState => currentState;
    public int Score => score;
    public int HighScore => highScore;
    public int CurrentWave => currentWave;

    private const string HIGH_SCORE_KEY = "SpaceShooterHighScore";

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
    /// Start a new game. Call when transitioning from menu to gameplay.
    /// </summary>
    public void StartGame()
    {
        score = 0;
        currentWave = 0;
        currentState = GameState.Playing;
        Time.timeScale = 1f;

        SceneManager.sceneLoaded += OnGameSceneLoaded;
        SceneManager.LoadScene("GameScene");
    }

    private void OnGameSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= OnGameSceneLoaded;

        if (scene.name == "GameScene")
        {
            // Find the spawner in the new scene
            enemySpawner = FindFirstObjectByType<EnemySpawner>();
            if (enemySpawner != null)
            {
                enemySpawner.StartSpawning();
            }

            // Update UI
            UIManager ui = FindFirstObjectByType<UIManager>();
            if (ui != null)
            {
                ui.UpdateScore(score);
                ui.UpdateWave(1);
            }
        }
    }

    /// <summary>
    /// Add to the player's score.
    /// </summary>
    public void AddScore(int points)
    {
        score += points;

        UIManager ui = FindFirstObjectByType<UIManager>();
        if (ui != null)
        {
            ui.UpdateScore(score);
        }
    }

    /// <summary>
    /// Called by EnemySpawner when a new wave begins.
    /// </summary>
    public void OnWaveStart(int wave)
    {
        currentWave = wave;

        UIManager ui = FindFirstObjectByType<UIManager>();
        if (ui != null)
        {
            ui.UpdateWave(wave);
            ui.ShowWaveAnnouncement(wave);
        }
    }

    /// <summary>
    /// Called by EnemySpawner when a wave is complete.
    /// </summary>
    public void OnWaveComplete(int wave)
    {
        UIManager ui = FindFirstObjectByType<UIManager>();
        if (ui != null)
        {
            ui.ShowMessage("Wave " + wave + " Complete!", 2f);
        }
    }

    /// <summary>
    /// Spawn a power-up at the given position.
    /// </summary>
    public void SpawnPowerUp(Vector3 position)
    {
        if (powerUpPrefab == null) return;

        GameObject powerUp = Instantiate(powerUpPrefab, position, Quaternion.identity);
        PowerUpController pc = powerUp.GetComponent<PowerUpController>();
        if (pc != null)
        {
            pc.RandomizeType();
        }
    }

    /// <summary>
    /// Pause the game.
    /// </summary>
    public void PauseGame()
    {
        if (currentState != GameState.Playing) return;

        currentState = GameState.Paused;
        Time.timeScale = 0f;

        MenuManager menu = FindFirstObjectByType<MenuManager>();
        if (menu != null)
        {
            menu.ShowPauseMenu();
        }
    }

    /// <summary>
    /// Resume from pause.
    /// </summary>
    public void ResumeGame()
    {
        if (currentState != GameState.Paused) return;

        currentState = GameState.Playing;
        Time.timeScale = 1f;

        MenuManager menu = FindFirstObjectByType<MenuManager>();
        if (menu != null)
        {
            menu.HidePauseMenu();
        }
    }

    /// <summary>
    /// Trigger game over state.
    /// </summary>
    public void GameOver()
    {
        if (currentState == GameState.GameOver) return;

        currentState = GameState.GameOver;

        if (enemySpawner != null)
        {
            enemySpawner.StopSpawning();
        }

        // Check high score
        if (score > highScore)
        {
            highScore = score;
            SaveHighScore();
        }

        // Show game over screen after brief delay
        MenuManager menu = FindFirstObjectByType<MenuManager>();
        if (menu != null)
        {
            menu.ShowGameOverScreen(score, highScore, currentWave);
        }
    }

    /// <summary>
    /// Return to the main menu.
    /// </summary>
    public void ReturnToMainMenu()
    {
        currentState = GameState.MainMenu;
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenuScene");
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

    private void LoadHighScore()
    {
        highScore = PlayerPrefs.GetInt(HIGH_SCORE_KEY, 0);
    }

    private void SaveHighScore()
    {
        PlayerPrefs.SetInt(HIGH_SCORE_KEY, highScore);
        PlayerPrefs.Save();
    }
}

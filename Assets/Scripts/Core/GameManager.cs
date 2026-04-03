using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// GameManager is the central controller for the entire game.
/// It manages game state, scoring, wave progression, and coordinates
/// between all other systems. Singleton pattern ensures only one exists.
/// </summary>
public class GameManager : MonoBehaviour
{
    // ============================================================
    // SINGLETON
    // ============================================================
    public static GameManager Instance { get; private set; }

    // ============================================================
    // GAME STATE
    // ============================================================
    public enum GameState { MainMenu, Playing, Paused, GameOver, WaveComplete, Victory }
    public GameState CurrentState { get; private set; } = GameState.MainMenu;

    // ============================================================
    // SCORING
    // ============================================================
    [Header("Scoring")]
    [Tooltip("Points awarded per basic enemy kill")]
    public int baseEnemyScore = 100;
    [Tooltip("Points awarded per fast enemy kill")]
    public int fastEnemyScore = 150;
    [Tooltip("Points awarded per tank enemy kill")]
    public int tankEnemyScore = 250;

    /// <summary>Current player score this session.</summary>
    public int Score { get; private set; } = 0;

    /// <summary>Highest score recorded (persisted with PlayerPrefs).</summary>
    public int HighScore { get; private set; } = 0;

    // ============================================================
    // WAVE SYSTEM
    // ============================================================
    [Header("Wave Settings")]
    [Tooltip("Total number of waves before victory")]
    public int totalWaves = 5;
    [Tooltip("Seconds to wait between waves")]
    public float timeBetweenWaves = 3f;

    /// <summary>Current wave number (1-based).</summary>
    public int CurrentWave { get; private set; } = 0;

    /// <summary>How many enemies are still alive in the current wave.</summary>
    public int EnemiesRemaining { get; set; } = 0;

    // ============================================================
    // REFERENCES
    // ============================================================
    [Header("References")]
    public GameObject playerPrefab;
    public Transform playerSpawnPoint;

    /// <summary>Cached reference to the live player object.</summary>
    public GameObject Player { get; private set; }

    // ============================================================
    // EVENTS – other scripts subscribe to these
    // ============================================================
    public event System.Action<int> OnScoreChanged;      // passes new score
    public event System.Action<int> OnWaveChanged;        // passes new wave number
    public event System.Action<GameState> OnStateChanged; // passes new state
    public event System.Action OnGameOver;
    public event System.Action OnVictory;

    // ============================================================
    // UNITY LIFECYCLE
    // ============================================================

    void Awake()
    {
        // Singleton: keep this instance, destroy duplicates
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Load persisted high score
        HighScore = PlayerPrefs.GetInt("HighScore", 0);
    }

    void OnDestroy()
    {
        // Clear singleton reference when destroyed
        if (Instance == this) Instance = null;
    }

    // ============================================================
    // PUBLIC API
    // ============================================================

    /// <summary>
    /// Call this to begin a new game session (from main menu or restart).
    /// Resets score, spawns the player, and kicks off wave 1.
    /// </summary>
    public void StartGame()
    {
        Score = 0;
        CurrentWave = 0;
        OnScoreChanged?.Invoke(Score);

        // Spawn the player ship at the designated spawn point
        Vector3 spawnPos = playerSpawnPoint != null
            ? playerSpawnPoint.position
            : new Vector3(0f, -3.5f, 0f);

        if (playerPrefab != null)
        {
            Player = Instantiate(playerPrefab, spawnPos, Quaternion.identity);
        }
        else
        {
            // If no prefab assigned, try to find an existing player
            Player = GameObject.FindGameObjectWithTag("Player");
        }

        SetState(GameState.Playing);
        StartNextWave();
    }

    /// <summary>
    /// Add points to the player's score.
    /// Called by enemies when they die.
    /// </summary>
    public void AddScore(int points)
    {
        if (CurrentState != GameState.Playing) return;

        Score += points;
        OnScoreChanged?.Invoke(Score);

        // Update high score if beaten
        if (Score > HighScore)
        {
            HighScore = Score;
            PlayerPrefs.SetInt("HighScore", HighScore);
            PlayerPrefs.Save();
        }
    }

    /// <summary>
    /// Called when an enemy is destroyed. Decrements the counter
    /// and checks whether the wave is complete.
    /// </summary>
    public void EnemyDestroyed()
    {
        EnemiesRemaining = Mathf.Max(0, EnemiesRemaining - 1);

        if (EnemiesRemaining <= 0 && CurrentState == GameState.Playing)
        {
            OnWaveComplete();
        }
    }

    /// <summary>
    /// Trigger game-over state. Called when the player's health reaches 0.
    /// </summary>
    public void TriggerGameOver()
    {
        if (CurrentState == GameState.GameOver) return;

        SetState(GameState.GameOver);
        OnGameOver?.Invoke();

        // Clean up remaining enemies and bullets
        DestroyAllTagged("Enemy");
        DestroyAllTagged("EnemyBullet");
        DestroyAllTagged("PlayerBullet");
        DestroyAllTagged("PowerUp");
    }

    /// <summary>
    /// Pause or unpause the game by setting Time.timeScale.
    /// </summary>
    public void TogglePause()
    {
        if (CurrentState == GameState.Playing)
        {
            Time.timeScale = 0f;
            SetState(GameState.Paused);
        }
        else if (CurrentState == GameState.Paused)
        {
            Time.timeScale = 1f;
            SetState(GameState.Playing);
        }
    }

    /// <summary>
    /// Reload the gameplay scene to restart.
    /// </summary>
    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("GamePlay");
    }

    /// <summary>
    /// Return to the main menu scene.
    /// </summary>
    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    /// <summary>
    /// Quit the application entirely.
    /// </summary>
    public void QuitGame()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }

    // ============================================================
    // WAVE MANAGEMENT (INTERNAL)
    // ============================================================

    /// <summary>
    /// Advance to the next wave, or trigger victory if all waves done.
    /// </summary>
    void StartNextWave()
    {
        CurrentWave++;

        if (CurrentWave > totalWaves)
        {
            // Player has beaten all waves!
            SetState(GameState.Victory);
            OnVictory?.Invoke();
            return;
        }

        OnWaveChanged?.Invoke(CurrentWave);

        // The EnemySpawner listens to wave changes and handles spawning.
        // We tell it to begin via the event above.
    }

    /// <summary>
    /// Called when all enemies in the current wave are dead.
    /// Waits briefly, then starts the next wave.
    /// </summary>
    void OnWaveComplete()
    {
        SetState(GameState.WaveComplete);
        Invoke(nameof(ResumeAndNextWave), timeBetweenWaves);
    }

    void ResumeAndNextWave()
    {
        if (CurrentState == GameState.GameOver) return;
        SetState(GameState.Playing);
        StartNextWave();
    }

    // ============================================================
    // HELPERS
    // ============================================================

    /// <summary>
    /// Set the game state and fire the state-changed event.
    /// </summary>
    void SetState(GameState newState)
    {
        CurrentState = newState;
        OnStateChanged?.Invoke(newState);
    }

    /// <summary>
    /// Destroy every active GameObject with the given tag.
    /// </summary>
    void DestroyAllTagged(string tag)
    {
        foreach (GameObject obj in GameObject.FindGameObjectsWithTag(tag))
        {
            Destroy(obj);
        }
    }
}

// ============================================================================
// GameManager.cs - Central game state manager (Singleton)
// Handles: score tracking, wave management, game states, difficulty scaling
// ============================================================================
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// GameManager is the central hub for all game state.
/// It persists across scenes and manages score, waves, and game flow.
/// Uses the Singleton pattern so any script can access it via GameManager.Instance.
/// </summary>
public class GameManager : MonoBehaviour
{
    // ---- Singleton ----
    public static GameManager Instance { get; private set; }

    // ---- Game States ----
    public enum GameState { MainMenu, Playing, Paused, GameOver }
    public GameState CurrentState { get; private set; } = GameState.MainMenu;

    // ---- Events (other scripts subscribe to these) ----
    public event Action<int> OnScoreChanged;        // passes new score
    public event Action<int> OnWaveChanged;          // passes new wave number
    public event Action<GameState> OnGameStateChanged; // passes new state
    public event Action OnWaveCompleted;              // fired when all enemies in a wave are dead

    // ---- Score ----
    public int Score { get; private set; }
    public int HighScore { get; private set; }

    // ---- Wave System ----
    public int CurrentWave { get; private set; }
    private int _enemiesAliveThisWave;

    // ---- Difficulty Scaling ----
    [Header("Difficulty Scaling")]
    [Tooltip("Base number of enemies in wave 1")]
    public int baseEnemiesPerWave = 5;
    [Tooltip("Extra enemies added per wave")]
    public int enemiesPerWaveIncrease = 2;
    [Tooltip("Enemy speed multiplier increase per wave (e.g. 0.1 = +10% each wave)")]
    public float speedScalePerWave = 0.1f;
    [Tooltip("Enemy health multiplier increase per wave")]
    public float healthScalePerWave = 0.15f;
    [Tooltip("Delay in seconds between waves")]
    public float waveCooldown = 3f;

    // ---- Internal ----
    private bool _waveInProgress;

    // ========================================================================
    // Unity Lifecycle
    // ========================================================================
    private void Awake()
    {
        // Singleton enforcement
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Load persisted high score
        HighScore = PlayerPrefs.GetInt("HighScore", 0);
    }

    // ========================================================================
    // Public API – Score
    // ========================================================================

    /// <summary>Add points and notify listeners.</summary>
    public void AddScore(int points)
    {
        if (CurrentState != GameState.Playing) return;
        Score += points;
        if (Score > HighScore)
        {
            HighScore = Score;
            PlayerPrefs.SetInt("HighScore", HighScore);
        }
        OnScoreChanged?.Invoke(Score);
    }

    // ========================================================================
    // Public API – Game State Transitions
    // ========================================================================

    /// <summary>Start a new game (called from Main Menu).</summary>
    public void StartGame()
    {
        Score = 0;
        CurrentWave = 0;
        _waveInProgress = false;
        SetState(GameState.Playing);
        Time.timeScale = 1f;
        SceneManager.LoadScene("GameScene");
    }

    /// <summary>Pause the game.</summary>
    public void PauseGame()
    {
        if (CurrentState != GameState.Playing) return;
        Time.timeScale = 0f;
        SetState(GameState.Paused);
    }

    /// <summary>Resume from pause.</summary>
    public void ResumeGame()
    {
        if (CurrentState != GameState.Paused) return;
        Time.timeScale = 1f;
        SetState(GameState.Playing);
    }

    /// <summary>Trigger Game Over (called when player dies).</summary>
    public void GameOver()
    {
        SetState(GameState.GameOver);
        Time.timeScale = 0f;
    }

    /// <summary>Return to the main menu scene.</summary>
    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SetState(GameState.MainMenu);
        SceneManager.LoadScene("MainMenu");
    }

    /// <summary>Quit the application.</summary>
    public void QuitGame()
    {
        PlayerPrefs.Save();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // ========================================================================
    // Public API – Wave Management
    // ========================================================================

    /// <summary>Start the next wave. Called by EnemySpawner after cooldown.</summary>
    public void StartNextWave()
    {
        CurrentWave++;
        _enemiesAliveThisWave = GetEnemyCountForWave(CurrentWave);
        _waveInProgress = true;
        OnWaveChanged?.Invoke(CurrentWave);
    }

    /// <summary>Call when an enemy is destroyed to track wave progress.</summary>
    public void EnemyDestroyed()
    {
        _enemiesAliveThisWave--;
        if (_enemiesAliveThisWave <= 0 && _waveInProgress)
        {
            _waveInProgress = false;
            OnWaveCompleted?.Invoke();
        }
    }

    /// <summary>How many enemies should spawn in a given wave.</summary>
    public int GetEnemyCountForWave(int wave)
    {
        return baseEnemiesPerWave + (wave - 1) * enemiesPerWaveIncrease;
    }

    /// <summary>Difficulty multiplier for enemy speed in the current wave.</summary>
    public float GetSpeedMultiplier()
    {
        return 1f + (CurrentWave - 1) * speedScalePerWave;
    }

    /// <summary>Difficulty multiplier for enemy health in the current wave.</summary>
    public float GetHealthMultiplier()
    {
        return 1f + (CurrentWave - 1) * healthScalePerWave;
    }

    public bool IsWaveInProgress => _waveInProgress;

    // ========================================================================
    // Internal helpers
    // ========================================================================

    private void SetState(GameState newState)
    {
        CurrentState = newState;
        OnGameStateChanged?.Invoke(newState);
    }
}

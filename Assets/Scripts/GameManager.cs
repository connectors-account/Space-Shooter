using System;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Central game state controller. Tracks score, waves, and overall game flow
/// (menu -> playing -> game over). Implemented as a lightweight singleton so any
/// other script can reach it via GameManager.Instance.
/// </summary>
public class GameManager : MonoBehaviour
{
    public enum GameState
    {
        Menu,
        Playing,
        GameOver
    }

    public static GameManager Instance { get; private set; }

    [Header("Gameplay")]
    [Tooltip("Points awarded for each enemy destroyed (before wave multiplier).")]
    public int pointsPerEnemy = 100;

    // Runtime state
    public GameState State { get; private set; } = GameState.Menu;
    public int Score { get; private set; }
    public int Wave { get; private set; }
    public int HighScore { get; private set; }

    // Events that the UI (and others) can subscribe to.
    public event Action<int> OnScoreChanged;
    public event Action<int> OnWaveChanged;
    public event Action<GameState> OnStateChanged;

    private const string HighScoreKey = "SpaceShooter_HighScore";

    private void Awake()
    {
        // Enforce a single instance.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        HighScore = PlayerPrefs.GetInt(HighScoreKey, 0);
    }

    private void Start()
    {
        // Start in the menu state and make sure time is running.
        SetState(GameState.Menu);
        Time.timeScale = 1f;
    }

    private void Update()
    {
        // Quick restart / start with the Enter key for convenience.
        if (State == GameState.Menu && Input.GetKeyDown(KeyCode.Return))
        {
            StartGame();
        }
        else if (State == GameState.GameOver && Input.GetKeyDown(KeyCode.Return))
        {
            RestartGame();
        }
    }

    /// <summary>
    /// Begins a fresh game: resets score and wave, then hands control to SpawnManager.
    /// </summary>
    public void StartGame()
    {
        Score = 0;
        Wave = 0;
        OnScoreChanged?.Invoke(Score);
        OnWaveChanged?.Invoke(Wave);

        SetState(GameState.Playing);
        Time.timeScale = 1f;

        if (SpawnManager.Instance != null)
        {
            SpawnManager.Instance.BeginSpawning();
        }
    }

    /// <summary>
    /// Reloads the active scene to guarantee a clean restart.
    /// </summary>
    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    /// <summary>
    /// Adds points (scaled by the current wave) and updates the high score.
    /// </summary>
    public void AddScore(int basePoints)
    {
        if (State != GameState.Playing)
        {
            return;
        }

        int waveMultiplier = Mathf.Max(1, Wave);
        Score += basePoints * waveMultiplier;
        OnScoreChanged?.Invoke(Score);

        if (Score > HighScore)
        {
            HighScore = Score;
            PlayerPrefs.SetInt(HighScoreKey, HighScore);
            PlayerPrefs.Save();
        }
    }

    /// <summary>
    /// Convenience overload used by enemies when they die.
    /// </summary>
    public void AddEnemyKillScore()
    {
        AddScore(pointsPerEnemy);
    }

    /// <summary>
    /// Advances the wave counter. Called by SpawnManager between waves.
    /// </summary>
    public void NextWave()
    {
        Wave++;
        OnWaveChanged?.Invoke(Wave);
    }

    /// <summary>
    /// Called when the player dies. Stops spawning and shows the game-over screen.
    /// </summary>
    public void GameOver()
    {
        if (State == GameState.GameOver)
        {
            return;
        }

        SetState(GameState.GameOver);

        if (SpawnManager.Instance != null)
        {
            SpawnManager.Instance.StopSpawning();
        }
    }

    private void SetState(GameState newState)
    {
        State = newState;
        OnStateChanged?.Invoke(State);
    }
}

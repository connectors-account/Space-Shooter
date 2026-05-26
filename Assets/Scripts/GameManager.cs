using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Singleton GameManager that controls game state, scoring, wave progression,
/// and coordinates all other managers. Persists across the gameplay scene.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    // ── Game State ──────────────────────────────────────────────────────
    public enum GameState { MainMenu, Playing, Paused, GameOver }
    public GameState CurrentState { get; private set; } = GameState.MainMenu;

    // ── Score ───────────────────────────────────────────────────────────
    public int Score { get; private set; }
    public int HighScore { get; private set; }

    // ── Wave tracking ───────────────────────────────────────────────────
    public int CurrentWave { get; private set; }

    // ── Events (other scripts subscribe) ────────────────────────────────
    public System.Action<int> OnScoreChanged;
    public System.Action<GameState> OnGameStateChanged;
    public System.Action<int> OnWaveChanged;

    // ── Settings ────────────────────────────────────────────────────────
    [Header("Gameplay")]
    [Tooltip("Seconds of invincibility after respawn")]
    public float respawnInvincibilityTime = 2f;

    [Tooltip("Total player lives before game over")]
    public int startingLives = 3;

    private int _livesRemaining;
    public int LivesRemaining => _livesRemaining;

    // ────────────────────────────────────────────────────────────────────
    // Unity Lifecycle
    // ────────────────────────────────────────────────────────────────────
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        HighScore = PlayerPrefs.GetInt("HighScore", 0);
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    // ────────────────────────────────────────────────────────────────────
    // Public API
    // ────────────────────────────────────────────────────────────────────

    /// <summary>Start a new game session.</summary>
    public void StartGame()
    {
        Score = 0;
        CurrentWave = 0;
        _livesRemaining = startingLives;
        Time.timeScale = 1f;
        SetState(GameState.Playing);
        OnScoreChanged?.Invoke(Score);
    }

    /// <summary>Add points and fire the score-changed event.</summary>
    public void AddScore(int points)
    {
        if (CurrentState != GameState.Playing) return;
        Score += points;
        if (Score > HighScore)
        {
            HighScore = Score;
            PlayerPrefs.SetInt("HighScore", HighScore);
            PlayerPrefs.Save();
        }
        OnScoreChanged?.Invoke(Score);
    }

    /// <summary>Advance to the next wave.</summary>
    public void AdvanceWave()
    {
        CurrentWave++;
        OnWaveChanged?.Invoke(CurrentWave);
    }

    /// <summary>Call when the player ship is destroyed.</summary>
    public void PlayerDied()
    {
        _livesRemaining--;
        if (_livesRemaining <= 0)
        {
            SetState(GameState.GameOver);
        }
    }

    /// <summary>Toggle pause.</summary>
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

    /// <summary>Return to main menu.</summary>
    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        SetState(GameState.MainMenu);
    }

    /// <summary>Reload the current scene (quick restart).</summary>
    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        // StartGame() should be called from the scene's initialization
    }

    // ────────────────────────────────────────────────────────────────────
    // Internal
    // ────────────────────────────────────────────────────────────────────
    private void SetState(GameState newState)
    {
        CurrentState = newState;
        OnGameStateChanged?.Invoke(newState);
    }
}

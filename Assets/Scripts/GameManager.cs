using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// GameManager - Singleton that manages game state, scoring, wave progression, and lives.
/// Attach to an empty GameObject named "GameManager" in the GamePlay scene.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Settings")]
    public int totalWaves = 5;
    public int startingLives = 3;

    [Header("State (Read Only)")]
    [SerializeField] private int score;
    [SerializeField] private int currentWave;
    [SerializeField] private int playerLives;
    [SerializeField] private bool isGameOver;
    [SerializeField] private bool isPaused;

    // Public accessors
    public int Score => score;
    public int CurrentWave => currentWave;
    public int PlayerLives => playerLives;
    public bool IsGameOver => isGameOver;
    public bool IsPaused => isPaused;

    // Events for UI updates
    public System.Action<int> OnScoreChanged;
    public System.Action<int> OnLivesChanged;
    public System.Action<int> OnWaveChanged;
    public System.Action OnGameOverTriggered;
    public System.Action OnGameWon;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        InitializeGame();
    }

    /// <summary>
    /// Reset all game state to starting values.
    /// </summary>
    public void InitializeGame()
    {
        score = 0;
        currentWave = 1;
        playerLives = startingLives;
        isGameOver = false;
        isPaused = false;
        Time.timeScale = 1f;

        OnScoreChanged?.Invoke(score);
        OnLivesChanged?.Invoke(playerLives);
        OnWaveChanged?.Invoke(currentWave);
    }

    /// <summary>
    /// Add points to the player's score.
    /// </summary>
    public void AddScore(int points)
    {
        if (isGameOver) return;
        score += points;
        OnScoreChanged?.Invoke(score);
    }

    /// <summary>
    /// Reduce player lives by 1. Triggers game over if no lives remain.
    /// </summary>
    public void LoseLife()
    {
        if (isGameOver) return;
        playerLives--;
        playerLives = Mathf.Max(0, playerLives);
        OnLivesChanged?.Invoke(playerLives);

        if (playerLives <= 0)
        {
            TriggerGameOver();
        }
    }

    /// <summary>
    /// Restore one life (e.g., from a power-up). Capped at startingLives.
    /// </summary>
    public void RestoreLife()
    {
        if (isGameOver) return;
        playerLives = Mathf.Min(playerLives + 1, startingLives);
        OnLivesChanged?.Invoke(playerLives);
    }

    /// <summary>
    /// Advance to the next wave. If all waves are done, trigger win.
    /// </summary>
    public void AdvanceWave()
    {
        if (isGameOver) return;
        currentWave++;
        if (currentWave > totalWaves)
        {
            TriggerGameWon();
        }
        else
        {
            OnWaveChanged?.Invoke(currentWave);
        }
    }

    /// <summary>
    /// Trigger the game-over state.
    /// </summary>
    public void TriggerGameOver()
    {
        if (isGameOver) return;
        isGameOver = true;
        Time.timeScale = 0f;
        OnGameOverTriggered?.Invoke();
        // Delay scene load so the player can see what happened
        Invoke(nameof(LoadGameOverScene), 0.1f);
    }

    private void LoadGameOverScene()
    {
        Time.timeScale = 1f;
        // Store score for the Game Over screen
        PlayerPrefs.SetInt("FinalScore", score);
        PlayerPrefs.SetInt("FinalWave", currentWave);
        PlayerPrefs.SetInt("GameWon", 0);
        PlayerPrefs.Save();
        SceneManager.LoadScene("GameOver");
    }

    /// <summary>
    /// Player completed all waves.
    /// </summary>
    public void TriggerGameWon()
    {
        isGameOver = true;
        OnGameWon?.Invoke();
        PlayerPrefs.SetInt("FinalScore", score);
        PlayerPrefs.SetInt("FinalWave", totalWaves);
        PlayerPrefs.SetInt("GameWon", 1);
        PlayerPrefs.Save();
        SceneManager.LoadScene("GameOver");
    }

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("GamePlay");
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}

using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// GameManager - Central controller for game state, score, and difficulty.
/// Manages transitions between Menu, Playing, and GameOver states.
/// Attach this to an empty GameObject named "GameManager" in the GamePlay scene.
/// </summary>
public class GameManager : MonoBehaviour
{
    // ── Singleton ──
    public static GameManager Instance { get; private set; }

    // ── Game States ──
    public enum GameState { Menu, Playing, GameOver }
    public GameState CurrentState { get; private set; } = GameState.Playing;

    // ── Score ──
    public int Score { get; private set; } = 0;

    // ── Difficulty ──
    [Header("Difficulty Settings")]
    [Tooltip("Starting interval (seconds) between enemy spawns")]
    public float initialSpawnInterval = 2.0f;

    [Tooltip("Minimum spawn interval as difficulty increases")]
    public float minimumSpawnInterval = 0.4f;

    [Tooltip("How much the spawn interval decreases per second of gameplay")]
    public float difficultyRampRate = 0.02f;

    /// <summary>Current spawn interval, decreases over time.</summary>
    public float CurrentSpawnInterval { get; private set; }

    // ── Internal ──
    private float gameTime = 0f;

    // ────────────────────────────────────────────
    void Awake()
    {
        // Singleton pattern – only one GameManager allowed
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        CurrentSpawnInterval = initialSpawnInterval;
    }

    void Start()
    {
        CurrentState = GameState.Playing;
        Time.timeScale = 1f;
    }

    void Update()
    {
        if (CurrentState != GameState.Playing) return;

        // Track elapsed time and ramp difficulty
        gameTime += Time.deltaTime;
        CurrentSpawnInterval = Mathf.Max(
            minimumSpawnInterval,
            initialSpawnInterval - (difficultyRampRate * gameTime)
        );
    }

    // ── Public API ──

    /// <summary>Add points to the player's score.</summary>
    public void AddScore(int points)
    {
        if (CurrentState != GameState.Playing) return;
        Score += points;
    }

    /// <summary>Called by PlayerController when the player dies.</summary>
    public void TriggerGameOver()
    {
        if (CurrentState == GameState.GameOver) return;

        CurrentState = GameState.GameOver;
        Time.timeScale = 0f; // Pause the game

        // Tell the HUD to show the Game Over panel
        if (HUDManager.Instance != null)
            HUDManager.Instance.ShowGameOver(Score);
    }

    /// <summary>Restart the gameplay scene.</summary>
    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("GamePlay");
    }

    /// <summary>Return to the main menu scene.</summary>
    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}

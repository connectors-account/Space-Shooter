// ============================================================================
// GameManager.cs - Central game state manager (Singleton)
// Manages game flow, scoring, wave progression, pause/resume, and scene transitions.
// ============================================================================
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

/// <summary>
/// Singleton GameManager that persists across scenes and controls all high-level game state.
/// </summary>
public class GameManager : MonoBehaviour
{
    // ---- Singleton ----
    public static GameManager Instance { get; private set; }

    // ---- Game State ----
    public enum GameState { MainMenu, Playing, Paused, GameOver }
    public GameState CurrentState { get; private set; } = GameState.MainMenu;

    // ---- Score ----
    public int Score { get; private set; }
    public int HighScore { get; private set; }
    public int ComboMultiplier { get; private set; } = 1;
    public int ComboCount { get; private set; }

    // ---- Wave ----
    public int CurrentWave { get; private set; } = 1;

    // ---- Events ----
    /// <summary>Fired whenever the score changes. Passes the new total score.</summary>
    public event Action<int> OnScoreChanged;
    /// <summary>Fired whenever the combo multiplier changes.</summary>
    public event Action<int> OnComboChanged;
    /// <summary>Fired when a new wave starts. Passes the wave number.</summary>
    public event Action<int> OnWaveStarted;
    /// <summary>Fired when the game state changes.</summary>
    public event Action<GameState> OnGameStateChanged;

    // ---- Combo Settings ----
    [Header("Combo Settings")]
    [Tooltip("Kills needed to reach the next combo multiplier tier.")]
    [SerializeField] private int comboThreshold = 5;
    [Tooltip("Maximum combo multiplier.")]
    [SerializeField] private int maxComboMultiplier = 8;
    [Tooltip("Seconds without a kill before the combo resets.")]
    [SerializeField] private float comboResetTime = 3f;

    private float comboTimer;

    // ========================================================================
    // Unity Lifecycle
    // ========================================================================

    private void Awake()
    {
        // Enforce singleton pattern; destroy duplicates.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Load persisted high score.
        HighScore = PlayerPrefs.GetInt("HighScore", 0);
    }

    private void Update()
    {
        if (CurrentState != GameState.Playing) return;

        // Tick down the combo timer; reset combo on expiry.
        if (ComboMultiplier > 1)
        {
            comboTimer -= Time.deltaTime;
            if (comboTimer <= 0f)
            {
                ResetCombo();
            }
        }

        // Pause input (P or Escape).
        if (Input.GetKeyDown(KeyCode.P) || Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    // ========================================================================
    // Game Flow
    // ========================================================================

    /// <summary>
    /// Starts a brand-new game session: resets score, wave, combo, then loads the Game scene.
    /// </summary>
    public void StartGame()
    {
        Score = 0;
        CurrentWave = 1;
        ResetCombo();
        SetState(GameState.Playing);
        Time.timeScale = 1f;
        SceneManager.LoadScene("Game");
    }

    /// <summary>
    /// Toggles between Playing and Paused states.
    /// </summary>
    public void TogglePause()
    {
        if (CurrentState == GameState.Playing)
        {
            SetState(GameState.Paused);
            Time.timeScale = 0f;
        }
        else if (CurrentState == GameState.Paused)
        {
            SetState(GameState.Playing);
            Time.timeScale = 1f;
        }
    }

    /// <summary>
    /// Called when the player dies. Persists high score and loads GameOver scene.
    /// </summary>
    public void GameOver()
    {
        SetState(GameState.GameOver);
        Time.timeScale = 1f;

        // Persist high score.
        if (Score > HighScore)
        {
            HighScore = Score;
            PlayerPrefs.SetInt("HighScore", HighScore);
            PlayerPrefs.Save();
        }

        SceneManager.LoadScene("GameOver");
    }

    /// <summary>
    /// Returns to the main menu scene and resets state.
    /// </summary>
    public void ReturnToMainMenu()
    {
        SetState(GameState.MainMenu);
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    // ========================================================================
    // Scoring
    // ========================================================================

    /// <summary>
    /// Adds base points multiplied by the current combo multiplier.
    /// Also advances the combo counter.
    /// </summary>
    /// <param name="basePoints">Raw point value of the kill/event.</param>
    public void AddScore(int basePoints)
    {
        int gained = basePoints * ComboMultiplier;
        Score += gained;
        OnScoreChanged?.Invoke(Score);

        // Advance combo.
        ComboCount++;
        comboTimer = comboResetTime;

        if (ComboCount >= comboThreshold && ComboMultiplier < maxComboMultiplier)
        {
            ComboMultiplier++;
            ComboCount = 0;
            OnComboChanged?.Invoke(ComboMultiplier);
        }
    }

    /// <summary>
    /// Resets the combo multiplier and counter back to baseline.
    /// </summary>
    public void ResetCombo()
    {
        ComboMultiplier = 1;
        ComboCount = 0;
        comboTimer = 0f;
        OnComboChanged?.Invoke(ComboMultiplier);
    }

    // ========================================================================
    // Wave Progression
    // ========================================================================

    /// <summary>
    /// Advances to the next wave and notifies listeners.
    /// </summary>
    public void AdvanceWave()
    {
        CurrentWave++;
        OnWaveStarted?.Invoke(CurrentWave);
    }

    /// <summary>
    /// Returns a difficulty scale factor based on the current wave (1.0 at wave 1).
    /// Used by spawner and enemy scripts to increase challenge.
    /// </summary>
    public float GetDifficultyMultiplier()
    {
        return 1f + (CurrentWave - 1) * 0.15f;
    }

    // ========================================================================
    // Helpers
    // ========================================================================

    private void SetState(GameState newState)
    {
        CurrentState = newState;
        OnGameStateChanged?.Invoke(CurrentState);
    }
}

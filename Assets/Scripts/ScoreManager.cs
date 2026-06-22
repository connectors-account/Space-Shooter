using System;
using UnityEngine;

/// <summary>
/// Tracks the player's current score and the persistent high score.
/// Implemented as a singleton so any script (e.g. Enemy on death) can add points.
/// </summary>
public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    // Fired whenever the score changes so the UI can update its label.
    public event Action<int> OnScoreChanged;
    // Fired when the high score changes.
    public event Action<int> OnHighScoreChanged;

    /// <summary>Current run score.</summary>
    public int Score { get; private set; }

    /// <summary>Best score ever achieved, persisted via PlayerPrefs.</summary>
    public int HighScore { get; private set; }

    private const string HighScoreKey = "SpaceShooter_HighScore";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Load the saved high score from disk (0 if none stored yet).
        HighScore = PlayerPrefs.GetInt(HighScoreKey, 0);
    }

    private void Start()
    {
        // Broadcast initial values so the UI shows the correct numbers.
        OnScoreChanged?.Invoke(Score);
        OnHighScoreChanged?.Invoke(HighScore);
    }

    /// <summary>Add points to the current score (called when an enemy dies).</summary>
    public void AddScore(int amount)
    {
        Score += amount;
        OnScoreChanged?.Invoke(Score);
    }

    /// <summary>Reset the score to zero at the start of a new game.</summary>
    public void ResetScore()
    {
        Score = 0;
        OnScoreChanged?.Invoke(Score);
    }

    /// <summary>Persist the high score if the current score beat it.</summary>
    public void SaveHighScore()
    {
        if (Score > HighScore)
        {
            HighScore = Score;
            PlayerPrefs.SetInt(HighScoreKey, HighScore);
            PlayerPrefs.Save();
            OnHighScoreChanged?.Invoke(HighScore);
        }
    }
}

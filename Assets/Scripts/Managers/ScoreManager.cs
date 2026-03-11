using UnityEngine;
using System;

/// <summary>
/// Manages game score and high score persistence.
/// This is a singleton - only one instance should exist.
/// </summary>
public class ScoreManager : MonoBehaviour
{
    [Header("Score Settings")]
    [Tooltip("Key used for saving high score in PlayerPrefs")]
    [SerializeField] private string highScoreKey = "HighScore";
    
    // Singleton instance
    private static ScoreManager instance;
    public static ScoreManager Instance => instance;
    
    // Score values
    private int currentScore = 0;
    private int highScore = 0;
    
    // Events
    public event Action<int> OnScoreChanged;
    public event Action<int> OnHighScoreChanged;
    
    // Properties
    public int CurrentScore => currentScore;
    public int HighScore => highScore;
    
    /// <summary>
    /// Initialize singleton on awake.
    /// </summary>
    private void Awake()
    {
        // Singleton pattern
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        // Load high score from storage
        LoadHighScore();
    }
    
    /// <summary>
    /// Add points to the current score.
    /// </summary>
    /// <param name="points">Points to add</param>
    public void AddScore(int points)
    {
        if (points <= 0) return;
        
        currentScore += points;
        OnScoreChanged?.Invoke(currentScore);
        
        // Check for new high score during gameplay
        if (currentScore > highScore)
        {
            highScore = currentScore;
            OnHighScoreChanged?.Invoke(highScore);
        }
    }
    
    /// <summary>
    /// Subtract points from the current score.
    /// </summary>
    /// <param name="points">Points to subtract</param>
    public void SubtractScore(int points)
    {
        if (points <= 0) return;
        
        currentScore = Mathf.Max(0, currentScore - points);
        OnScoreChanged?.Invoke(currentScore);
    }
    
    /// <summary>
    /// Reset the current score to zero.
    /// </summary>
    public void ResetScore()
    {
        currentScore = 0;
        OnScoreChanged?.Invoke(currentScore);
    }
    
    /// <summary>
    /// Check and save high score if current score is higher.
    /// </summary>
    /// <returns>True if a new high score was achieved</returns>
    public bool CheckHighScore()
    {
        if (currentScore > highScore)
        {
            highScore = currentScore;
            SaveHighScore();
            OnHighScoreChanged?.Invoke(highScore);
            return true;
        }
        return false;
    }
    
    /// <summary>
    /// Save the high score to persistent storage.
    /// </summary>
    private void SaveHighScore()
    {
        PlayerPrefs.SetInt(highScoreKey, highScore);
        PlayerPrefs.Save();
        Debug.Log($"High score saved: {highScore}");
    }
    
    /// <summary>
    /// Load the high score from persistent storage.
    /// </summary>
    private void LoadHighScore()
    {
        highScore = PlayerPrefs.GetInt(highScoreKey, 0);
        Debug.Log($"High score loaded: {highScore}");
    }
    
    /// <summary>
    /// Reset the high score to zero.
    /// </summary>
    public void ResetHighScore()
    {
        highScore = 0;
        SaveHighScore();
        OnHighScoreChanged?.Invoke(highScore);
    }
    
    /// <summary>
    /// Get the multiplied score value.
    /// </summary>
    /// <param name="basePoints">Base point value</param>
    /// <param name="multiplier">Score multiplier</param>
    /// <returns>Multiplied score value</returns>
    public static int GetMultipliedScore(int basePoints, float multiplier)
    {
        return Mathf.RoundToInt(basePoints * multiplier);
    }
}

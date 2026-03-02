using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// ScoreManager handles all scoring logic.
/// Singleton pattern for easy access from any script.
/// </summary>
public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [Header("Score Settings")]
    private int currentScore = 0;
    private int highScore = 0;
    
    [Header("Events")]
    public UnityEvent<int> OnScoreChanged;
    public UnityEvent<int> OnHighScoreChanged;

    private const string HIGH_SCORE_KEY = "HighScore";

    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        // Load high score from PlayerPrefs
        LoadHighScore();
    }

    void Start()
    {
        // Initialize events if null
        if (OnScoreChanged == null)
            OnScoreChanged = new UnityEvent<int>();
        if (OnHighScoreChanged == null)
            OnHighScoreChanged = new UnityEvent<int>();
        
        // Notify UI of initial values
        OnScoreChanged?.Invoke(currentScore);
        OnHighScoreChanged?.Invoke(highScore);
    }

    /// <summary>
    /// Add points to the current score
    /// </summary>
    /// <param name="points">Points to add</param>
    public void AddScore(int points)
    {
        currentScore += points;
        OnScoreChanged?.Invoke(currentScore);
        
        // Check for new high score
        if (currentScore > highScore)
        {
            highScore = currentScore;
            SaveHighScore();
            OnHighScoreChanged?.Invoke(highScore);
        }
    }

    /// <summary>
    /// Get the current score
    /// </summary>
    public int GetScore()
    {
        return currentScore;
    }

    /// <summary>
    /// Get the high score
    /// </summary>
    public int GetHighScore()
    {
        return highScore;
    }

    /// <summary>
    /// Reset score to zero
    /// </summary>
    public void ResetScore()
    {
        currentScore = 0;
        OnScoreChanged?.Invoke(currentScore);
    }

    /// <summary>
    /// Save high score to PlayerPrefs
    /// </summary>
    void SaveHighScore()
    {
        PlayerPrefs.SetInt(HIGH_SCORE_KEY, highScore);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Load high score from PlayerPrefs
    /// </summary>
    void LoadHighScore()
    {
        highScore = PlayerPrefs.GetInt(HIGH_SCORE_KEY, 0);
    }

    /// <summary>
    /// Reset high score (for testing or reset option)
    /// </summary>
    public void ResetHighScore()
    {
        highScore = 0;
        SaveHighScore();
        OnHighScoreChanged?.Invoke(highScore);
    }
}

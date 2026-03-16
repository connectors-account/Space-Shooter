using UnityEngine;
using System;

/// <summary>
/// ScoreManager handles score tracking, high scores, and score display.
/// </summary>
public class ScoreManager : MonoBehaviour
{
    // Singleton instance
    public static ScoreManager Instance { get; private set; }

    [Header("Score Settings")]
    [SerializeField] private int waveCompletionBonus = 500;
    [SerializeField] private float waveBonusMultiplier = 1.5f;

    // Score state
    private int currentScore;
    private int highScore;
    private int combo;
    private float comboTimer;
    private const float COMBO_TIMEOUT = 2f;
    private const string HIGH_SCORE_KEY = "HighScore";

    // Events
    public static event Action<int> OnScoreChanged;
    public static event Action<int> OnHighScoreChanged;
    public static event Action<int> OnComboChanged;

    public int CurrentScore => currentScore;
    public int HighScore => highScore;
    public int Combo => combo;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        LoadHighScore();
    }

    private void OnEnable()
    {
        Enemy.OnEnemyKilled += AddScore;
    }

    private void OnDisable()
    {
        Enemy.OnEnemyKilled -= AddScore;
    }

    private void Update()
    {
        // Update combo timer
        if (combo > 0)
        {
            comboTimer -= Time.deltaTime;
            if (comboTimer <= 0)
            {
                ResetCombo();
            }
        }
    }

    /// <summary>
    /// Load high score from PlayerPrefs
    /// </summary>
    private void LoadHighScore()
    {
        highScore = PlayerPrefs.GetInt(HIGH_SCORE_KEY, 0);
        OnHighScoreChanged?.Invoke(highScore);
    }

    /// <summary>
    /// Save high score to PlayerPrefs
    /// </summary>
    private void SaveHighScore()
    {
        PlayerPrefs.SetInt(HIGH_SCORE_KEY, highScore);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Add score from killing an enemy
    /// </summary>
    public void AddScore(int points)
    {
        // Increase combo
        combo++;
        comboTimer = COMBO_TIMEOUT;
        OnComboChanged?.Invoke(combo);

        // Calculate score with combo multiplier
        float comboMultiplier = 1f + (combo - 1) * 0.1f; // 10% bonus per combo
        int finalPoints = Mathf.RoundToInt(points * comboMultiplier);

        currentScore += finalPoints;
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
    /// Add wave completion bonus
    /// </summary>
    public void AddWaveBonus(int waveNumber)
    {
        int bonus = Mathf.RoundToInt(waveCompletionBonus * Mathf.Pow(waveBonusMultiplier, waveNumber - 1));
        currentScore += bonus;
        OnScoreChanged?.Invoke(currentScore);

        // Play sound effect
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySound("WaveComplete");
        }

        Debug.Log($"Wave {waveNumber} complete! Bonus: {bonus}");

        // Check high score
        if (currentScore > highScore)
        {
            highScore = currentScore;
            SaveHighScore();
            OnHighScoreChanged?.Invoke(highScore);
        }
    }

    /// <summary>
    /// Reset combo counter
    /// </summary>
    private void ResetCombo()
    {
        combo = 0;
        comboTimer = 0f;
        OnComboChanged?.Invoke(combo);
    }

    /// <summary>
    /// Reset current score (called when starting new game)
    /// </summary>
    public void ResetScore()
    {
        currentScore = 0;
        combo = 0;
        comboTimer = 0f;
        OnScoreChanged?.Invoke(currentScore);
        OnComboChanged?.Invoke(combo);
    }

    /// <summary>
    /// Reset high score (for testing/debug)
    /// </summary>
    public void ResetHighScore()
    {
        highScore = 0;
        SaveHighScore();
        OnHighScoreChanged?.Invoke(highScore);
    }

    /// <summary>
    /// Get the final score for game over screen
    /// </summary>
    public int GetFinalScore()
    {
        return currentScore;
    }

    /// <summary>
    /// Check if current score is a new high score
    /// </summary>
    public bool IsNewHighScore()
    {
        return currentScore >= highScore && currentScore > 0;
    }
}

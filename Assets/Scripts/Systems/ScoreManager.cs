using UnityEngine;
using System;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [Header("Score Settings")]
    public int scoreMultiplierThreshold = 1000;
    public float multiplierDecayTime = 5f;

    // State
    private int currentScore = 0;
    private int highScore = 0;
    private int scoreMultiplier = 1;
    private float lastScoreTime;
    private int comboCount = 0;

    // Events
    public static event Action<int> OnScoreChanged;
    public static event Action<int> OnHighScoreChanged;
    public static event Action<int> OnMultiplierChanged;
    public static event Action<int> OnComboChanged;

    public int CurrentScore => currentScore;
    public int HighScore => highScore;
    public int Multiplier => scoreMultiplier;
    public int Combo => comboCount;

    private const string HIGH_SCORE_KEY = "HighScore";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        LoadHighScore();
    }

    private void Update()
    {
        // Decay multiplier if no score for a while
        if (scoreMultiplier > 1 && Time.time - lastScoreTime > multiplierDecayTime)
        {
            scoreMultiplier = 1;
            comboCount = 0;
            OnMultiplierChanged?.Invoke(scoreMultiplier);
            OnComboChanged?.Invoke(comboCount);
        }
    }

    public void AddScore(int points)
    {
        if (points <= 0) return;

        int actualPoints = points * scoreMultiplier;
        currentScore += actualPoints;
        lastScoreTime = Time.time;
        comboCount++;

        // Update multiplier based on combo
        int newMultiplier = 1 + (comboCount / 10);
        newMultiplier = Mathf.Min(newMultiplier, 5); // Cap at 5x

        if (newMultiplier != scoreMultiplier)
        {
            scoreMultiplier = newMultiplier;
            OnMultiplierChanged?.Invoke(scoreMultiplier);
        }

        OnScoreChanged?.Invoke(currentScore);
        OnComboChanged?.Invoke(comboCount);

        // Check for high score
        if (currentScore > highScore)
        {
            highScore = currentScore;
            SaveHighScore();
            OnHighScoreChanged?.Invoke(highScore);
        }
    }

    public void ResetScore()
    {
        currentScore = 0;
        scoreMultiplier = 1;
        comboCount = 0;
        lastScoreTime = Time.time;

        OnScoreChanged?.Invoke(currentScore);
        OnMultiplierChanged?.Invoke(scoreMultiplier);
        OnComboChanged?.Invoke(comboCount);
    }

    public void ResetCombo()
    {
        comboCount = 0;
        scoreMultiplier = 1;
        OnMultiplierChanged?.Invoke(scoreMultiplier);
        OnComboChanged?.Invoke(comboCount);
    }

    private void SaveHighScore()
    {
        PlayerPrefs.SetInt(HIGH_SCORE_KEY, highScore);
        PlayerPrefs.Save();
    }

    private void LoadHighScore()
    {
        highScore = PlayerPrefs.GetInt(HIGH_SCORE_KEY, 0);
    }

    public void ResetHighScore()
    {
        highScore = 0;
        PlayerPrefs.SetInt(HIGH_SCORE_KEY, 0);
        PlayerPrefs.Save();
        OnHighScoreChanged?.Invoke(highScore);
    }
}

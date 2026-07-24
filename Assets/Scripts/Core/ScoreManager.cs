// ============================================================
//  ScoreManager.cs  –  Score, kill-streak multiplier, high score
// ============================================================
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    private const string HS_KEY = "SpaceShooter_HighScore";

    public int Score     { get; private set; }
    public int HighScore { get; private set; }
    public int Multiplier { get; private set; } = 1;

    private int _streak;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        HighScore = PlayerPrefs.GetInt(HS_KEY, 0);
    }

    // ── Public API ───────────────────────────────────────────

    /// <summary>Add points, applying the current kill-streak multiplier.</summary>
    public void Add(int basePoints)
    {
        _streak++;
        Multiplier = _streak >= 10 ? 4
                   : _streak >=  5 ? 3
                   : _streak >=  3 ? 2
                   : 1;

        int earned = basePoints * Multiplier;
        Score += earned;
        if (Score > HighScore) HighScore = Score;

        UIManager.Instance?.RefreshScore(Score, Multiplier);
    }

    /// <summary>Player was hit – reset streak.</summary>
    public void BreakStreak()
    {
        _streak = 0;
        Multiplier = 1;
        UIManager.Instance?.RefreshScore(Score, Multiplier);
    }

    public void Reset()
    {
        Score = 0;
        _streak = 0;
        Multiplier = 1;
    }

    public void SaveHighScore()
    {
        if (Score > PlayerPrefs.GetInt(HS_KEY, 0))
        {
            PlayerPrefs.SetInt(HS_KEY, Score);
            PlayerPrefs.Save();
            HighScore = Score;
        }
    }
}

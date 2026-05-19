using UnityEngine;

/// <summary>
/// Manages scoring, combo multipliers, and high scores.
/// </summary>
public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [Header("Combo Settings")]
    [SerializeField] private float comboWindow = 2f;
    [SerializeField] private int maxMultiplier = 5;

    private int currentScore;
    private int highScore;
    private int comboMultiplier = 1;
    private float lastKillTime;

    private const string HIGH_SCORE_KEY = "HighScore";

    public int CurrentScore => currentScore;
    public int HighScore => highScore;
    public int ComboMultiplier => comboMultiplier;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        highScore = PlayerPrefs.GetInt(HIGH_SCORE_KEY, 0);
    }

    public void AddScore(int basePoints)
    {
        // Update combo
        if (Time.time - lastKillTime < comboWindow)
        {
            comboMultiplier = Mathf.Min(comboMultiplier + 1, maxMultiplier);
        }
        else
        {
            comboMultiplier = 1;
        }
        lastKillTime = Time.time;

        int points = basePoints * comboMultiplier;
        currentScore += points;

        // Update high score
        if (currentScore > highScore)
        {
            highScore = currentScore;
            PlayerPrefs.SetInt(HIGH_SCORE_KEY, highScore);
            PlayerPrefs.Save();
        }

        // Update UI
        UIManager.Instance?.UpdateScore(currentScore, comboMultiplier);
    }

    public void ResetScore()
    {
        currentScore = 0;
        comboMultiplier = 1;
        lastKillTime = 0;
        UIManager.Instance?.UpdateScore(0, 1);
    }
}

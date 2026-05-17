using UnityEngine;

/// <summary>
/// Manages scoring, high score persistence, and combo multipliers.
/// </summary>
public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    private int currentScore;
    private int highScore;
    private int combo;
    private float comboTimer;
    private float comboTimeout = 2f;

    public int CurrentScore => currentScore;
    public int HighScore => highScore;
    public int Combo => combo;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        highScore = PlayerPrefs.GetInt("HighScore", 0);
    }

    void Update()
    {
        if (combo > 1)
        {
            comboTimer -= Time.deltaTime;
            if (comboTimer <= 0f)
            {
                combo = 1;
            }
        }
    }

    public void ResetScore()
    {
        currentScore = 0;
        combo = 1;
        if (UIManager.Instance != null)
            UIManager.Instance.UpdateScoreDisplay(currentScore);
    }

    public void AddScore(int basePoints)
    {
        int points = basePoints * Mathf.Max(1, combo);
        currentScore += points;

        // Increment combo
        combo++;
        comboTimer = comboTimeout;

        // Update high score
        if (currentScore > highScore)
        {
            highScore = currentScore;
            PlayerPrefs.SetInt("HighScore", highScore);
            PlayerPrefs.Save();
        }

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateScoreDisplay(currentScore);
            if (combo > 2)
                UIManager.Instance.ShowCombo(combo);
        }
    }
}

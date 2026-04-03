using UnityEngine;

/// <summary>
/// Tracks and manages the player's score.
/// Persists high score using PlayerPrefs.
/// </summary>
public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    private int currentScore = 0;
    private int highScore = 0;
    private int comboMultiplier = 1;
    private float comboTimer = 0f;
    private float comboTimeout = 2f;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        highScore = PlayerPrefs.GetInt("HighScore", 0);
    }

    void Update()
    {
        if (comboTimer > 0)
        {
            comboTimer -= Time.deltaTime;
            if (comboTimer <= 0)
            {
                comboMultiplier = 1;
            }
        }
    }

    public void AddScore(int basePoints)
    {
        int points = basePoints * comboMultiplier;
        currentScore += points;

        // Increase combo
        comboTimer = comboTimeout;
        comboMultiplier = Mathf.Min(comboMultiplier + 1, 8);

        // Update high score
        if (currentScore > highScore)
        {
            highScore = currentScore;
            PlayerPrefs.SetInt("HighScore", highScore);
        }

        if (UIManager.Instance != null)
            UIManager.Instance.UpdateScore(currentScore, comboMultiplier);
    }

    public void ResetScore()
    {
        currentScore = 0;
        comboMultiplier = 1;
        comboTimer = 0f;

        if (UIManager.Instance != null)
            UIManager.Instance.UpdateScore(0, 1);
    }

    public int GetScore() { return currentScore; }
    public int GetHighScore() { return highScore; }
    public int GetCombo() { return comboMultiplier; }
}

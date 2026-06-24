using System;
using UnityEngine;

namespace SpaceShooter
{
    /// <summary>
    /// Tracks the current score and persists a high score using PlayerPrefs.
    /// Fires events so the UI can update without polling.
    /// </summary>
    public class ScoreManager : MonoBehaviour
    {
        public static ScoreManager Instance { get; private set; }

        private const string HighScoreKey = "SPACE_SHOOTER_HIGHSCORE";

        private int score;
        private int highScore;
        private int scoreMultiplier = 1;

        public int Score => score;
        public int HighScore => highScore;

        public event Action<int> OnScoreChanged;
        public event Action<int> OnHighScoreChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            highScore = PlayerPrefs.GetInt(HighScoreKey, 0);
        }

        private void Start()
        {
            OnScoreChanged?.Invoke(score);
            OnHighScoreChanged?.Invoke(highScore);
        }

        public void AddScore(int amount)
        {
            score += amount * scoreMultiplier;
            OnScoreChanged?.Invoke(score);

            if (score > highScore)
            {
                highScore = score;
                OnHighScoreChanged?.Invoke(highScore);
            }
        }

        public void SetMultiplier(int multiplier)
        {
            scoreMultiplier = Mathf.Max(1, multiplier);
        }

        public void ResetScore()
        {
            score = 0;
            scoreMultiplier = 1;
            OnScoreChanged?.Invoke(score);
        }

        public void SaveHighScore()
        {
            if (score >= highScore)
            {
                highScore = score;
                PlayerPrefs.SetInt(HighScoreKey, highScore);
                PlayerPrefs.Save();
                OnHighScoreChanged?.Invoke(highScore);
            }
        }
    }
}

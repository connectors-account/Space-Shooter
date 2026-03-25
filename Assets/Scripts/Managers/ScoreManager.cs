// =============================================================================
// ScoreManager.cs — Score tracking, combo system, and persistence
// =============================================================================
using UnityEngine;
using System;

namespace SpaceShooter.Managers
{
    /// <summary>
    /// Tracks score, multipliers, and persists high scores.
    /// Works alongside GameManager for score operations.
    /// </summary>
    public class ScoreManager : MonoBehaviour
    {
        public static ScoreManager Instance { get; private set; }

        [Header("Combo Settings")]
        [SerializeField] private float comboTimeWindow = 2f;
        [SerializeField] private int maxMultiplier = 8;

        private int currentScore;
        private int multiplier = 1;
        private float lastKillTime;
        private int killStreak;

        /// <summary>Current score.</summary>
        public int CurrentScore => currentScore;

        /// <summary>Current combo multiplier.</summary>
        public int Multiplier => multiplier;

        /// <summary>Fired when score updates. Args: newScore, multiplier.</summary>
        public event Action<int, int> OnScoreUpdated;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        /// <summary>
        /// Resets score and combo for a new game.
        /// </summary>
        public void ResetScore()
        {
            currentScore = 0;
            multiplier = 1;
            killStreak = 0;
            OnScoreUpdated?.Invoke(currentScore, multiplier);
        }

        /// <summary>
        /// Adds score with multiplier consideration and updates combo.
        /// </summary>
        public void AddScore(int baseScore)
        {
            // Update combo
            if (Time.time - lastKillTime < comboTimeWindow)
            {
                killStreak++;
                multiplier = Mathf.Min(1 + killStreak / 3, maxMultiplier);
            }
            else
            {
                killStreak = 0;
                multiplier = 1;
            }
            lastKillTime = Time.time;

            int earnedScore = baseScore * multiplier;
            currentScore += earnedScore;
            OnScoreUpdated?.Invoke(currentScore, multiplier);
        }

        /// <summary>
        /// Saves the high score if current is greater.
        /// </summary>
        public void SaveHighScore()
        {
            int saved = PlayerPrefs.GetInt("SpaceShooter_HighScore", 0);
            if (currentScore > saved)
            {
                PlayerPrefs.SetInt("SpaceShooter_HighScore", currentScore);
                PlayerPrefs.Save();
            }
        }

        /// <summary>
        /// Gets the saved high score.
        /// </summary>
        public int GetHighScore()
        {
            return PlayerPrefs.GetInt("SpaceShooter_HighScore", 0);
        }
    }
}

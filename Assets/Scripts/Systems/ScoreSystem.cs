using UnityEngine;

namespace SpaceShooter.Systems
{
    /// <summary>
    /// Tracks current score and high score (saved with PlayerPrefs).
    /// </summary>
    public class ScoreSystem : MonoBehaviour
    {
        public static ScoreSystem Instance { get; private set; }

        private const string HighScoreKey = "SpaceShooter.HighScore";

        public int CurrentScore { get; private set; }
        public int HighScore { get; private set; }

        public event System.Action<int> OnScoreChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            HighScore = PlayerPrefs.GetInt(HighScoreKey, 0);
        }

        public void AddScore(int points)
        {
            CurrentScore += Mathf.Max(0, points);

            if (CurrentScore > HighScore)
            {
                HighScore = CurrentScore;
                PlayerPrefs.SetInt(HighScoreKey, HighScore);
                PlayerPrefs.Save();
            }

            OnScoreChanged?.Invoke(CurrentScore);
        }

        public void ResetScore()
        {
            CurrentScore = 0;
            OnScoreChanged?.Invoke(CurrentScore);
        }
    }
}

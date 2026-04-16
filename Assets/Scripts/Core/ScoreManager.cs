using System;

namespace SpaceShooter.Core
{
    public static class ScoreManager
    {
        public static int CurrentScore { get; private set; }
        public static event Action<int> OnScoreChanged;

        public static void AddScore(int amount)
        {
            CurrentScore += amount;
            if (CurrentScore < 0)
            {
                CurrentScore = 0;
            }
            OnScoreChanged?.Invoke(CurrentScore);
        }

        public static void ResetScore()
        {
            CurrentScore = 0;
            OnScoreChanged?.Invoke(CurrentScore);
        }
    }
}

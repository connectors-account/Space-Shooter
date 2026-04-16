using UnityEngine;

namespace SpaceShooter.Core
{
    public class GameSession : MonoBehaviour
    {
        public static GameSession Instance { get; private set; }

        public int Score { get; private set; }
        public int Wave { get; private set; }
        public int HighScore { get; private set; }
        public bool IsRunActive { get; private set; }

        public event System.Action<int> ScoreChanged;
        public event System.Action<int> WaveChanged;

        private const string HighScoreKey = "SPACE_SHOOTER_HIGH_SCORE";

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            HighScore = PlayerPrefs.GetInt(HighScoreKey, 0);
        }

        public void StartNewRun()
        {
            Score = 0;
            Wave = 1;
            IsRunActive = true;
            ScoreChanged?.Invoke(Score);
            WaveChanged?.Invoke(Wave);
        }

        public void SetWave(int wave)
        {
            Wave = Mathf.Max(1, wave);
            WaveChanged?.Invoke(Wave);
        }

        public void AddScore(int points)
        {
            if (!IsRunActive)
            {
                return;
            }

            Score += Mathf.Max(0, points);
            ScoreChanged?.Invoke(Score);

            if (Score > HighScore)
            {
                HighScore = Score;
                PlayerPrefs.SetInt(HighScoreKey, HighScore);
                PlayerPrefs.Save();
            }
        }

        public void EndRun()
        {
            IsRunActive = false;
        }
    }
}

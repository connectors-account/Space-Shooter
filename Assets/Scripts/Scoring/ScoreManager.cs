using System;
using UnityEngine;

namespace SpaceShooter
{
    /// <summary>
    /// Tracks the player's current score, the persisted high score and a
    /// consecutive-kill score multiplier (x1 / x2 / x3).
    /// </summary>
    public class ScoreManager : Singleton<ScoreManager>
    {
        public const string HighScoreKey = "SpaceShooter.HighScore";

        [SerializeField] private int currentScore;
        [SerializeField] private int highScore;
        [SerializeField] private int consecutiveKills;

        private bool _initialized;

        /// <summary>Raised whenever the current score changes. Argument is the new score.</summary>
        public event Action<int> OnScoreChanged;

        /// <summary>Raised whenever the multiplier changes. Argument is the new multiplier.</summary>
        public event Action<int> OnMultiplierChanged;

        /// <summary>
        /// Current score multiplier. Increases every 5 consecutive kills, capped at x3.
        /// </summary>
        public int Multiplier => 1 + Mathf.Clamp(consecutiveKills / 5, 0, 2);

        protected override void Awake()
        {
            base.Awake();
            Initialize();
        }

        /// <summary>
        /// Loads the persisted high score. Safe to call multiple times and from tests
        /// (does not require the MonoBehaviour lifecycle to have run).
        /// </summary>
        public void Initialize()
        {
            RegisterSingleton();
            if (_initialized) return;
            highScore = PlayerPrefs.GetInt(HighScoreKey, 0);
            _initialized = true;
        }

        /// <summary>Adds <paramref name="points"/> (scaled by the current multiplier) to the score.</summary>
        public void AddScore(int points)
        {
            Initialize();
            if (points < 0) points = 0;
            currentScore += points * Multiplier;

            if (currentScore > highScore)
            {
                highScore = currentScore;
                PlayerPrefs.SetInt(HighScoreKey, highScore);
                PlayerPrefs.Save();
            }

            OnScoreChanged?.Invoke(currentScore);
        }

        /// <summary>Registers a kill, growing the consecutive-kill multiplier.</summary>
        public void RegisterKill()
        {
            int previous = Multiplier;
            consecutiveKills++;
            if (Multiplier != previous)
            {
                OnMultiplierChanged?.Invoke(Multiplier);
            }
        }

        /// <summary>Resets the multiplier chain, e.g. when the player takes damage.</summary>
        public void OnPlayerDamaged()
        {
            if (consecutiveKills == 0) return;
            consecutiveKills = 0;
            OnMultiplierChanged?.Invoke(Multiplier);
        }

        /// <summary>Clears the current score and multiplier for a new game.</summary>
        public void ResetScore()
        {
            currentScore = 0;
            consecutiveKills = 0;
            OnScoreChanged?.Invoke(currentScore);
            OnMultiplierChanged?.Invoke(Multiplier);
        }

        public int GetScore() => currentScore;

        public int GetHighScore()
        {
            Initialize();
            return highScore;
        }
    }
}

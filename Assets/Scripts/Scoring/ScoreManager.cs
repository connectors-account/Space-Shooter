using System;
using UnityEngine;
using SpaceShooter.Utilities;

namespace SpaceShooter.Scoring
{
    /// <summary>
    /// Singleton scoring system. Tracks the current score, a combo multiplier
    /// that decays after a period without kills, and the persisted high score.
    /// </summary>
    public class ScoreManager : MonoBehaviour
    {
        public static ScoreManager Instance { get; private set; }

        /// <summary>Fired when the current score changes (newScore).</summary>
        public event Action<int> OnScoreChanged;
        /// <summary>Fired when the running multiplier changes (newMultiplier).</summary>
        public event Action<float> OnMultiplierChanged;
        /// <summary>Fired the first moment the current score exceeds the stored high score.</summary>
        public event Action<int> OnHighScoreBeaten;

        [Header("Multiplier")]
        [SerializeField] private float multiplierStep = 0.5f;
        [SerializeField] private float maxMultiplier = 8f;
        [SerializeField] private float resetTime = Constants.MultiplierResetTime;

        public int Score { get; private set; }
        public float Multiplier { get; private set; } = 1f;
        public int HighScore { get; private set; }

        private float _timeSinceLastKill;
        private bool _highScoreBeatenFired;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            HighScore = PlayerPrefs.GetInt(Constants.PrefsHighScore, 0);
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void Update()
        {
            if (Multiplier > 1f)
            {
                _timeSinceLastKill += Time.deltaTime;
                if (_timeSinceLastKill >= resetTime)
                {
                    Multiplier = 1f;
                    OnMultiplierChanged?.Invoke(Multiplier);
                }
            }
        }

        /// <summary>Reset the score and multiplier for a fresh run.</summary>
        public void ResetForNewGame()
        {
            Score = 0;
            Multiplier = 1f;
            _timeSinceLastKill = 0f;
            _highScoreBeatenFired = false;
            HighScore = PlayerPrefs.GetInt(Constants.PrefsHighScore, 0);
            OnScoreChanged?.Invoke(Score);
            OnMultiplierChanged?.Invoke(Multiplier);
        }

        /// <summary>
        /// Add score. The base points are multiplied by the current running
        /// multiplier and the optional per-call multiplier, then the running
        /// multiplier is increased and its decay timer reset.
        /// </summary>
        public void AddScore(int basePoints, float perCallMultiplier = 1f)
        {
            int gained = Mathf.RoundToInt(basePoints * Multiplier * perCallMultiplier);
            Score += gained;
            OnScoreChanged?.Invoke(Score);

            // Grow the combo multiplier for successive kills.
            _timeSinceLastKill = 0f;
            Multiplier = Mathf.Min(maxMultiplier, Multiplier + multiplierStep);
            OnMultiplierChanged?.Invoke(Multiplier);

            if (!_highScoreBeatenFired && Score > HighScore && HighScore > 0)
            {
                _highScoreBeatenFired = true;
                OnHighScoreBeaten?.Invoke(Score);
            }
        }

        /// <summary>Add raw points without touching the multiplier (e.g. bonuses).</summary>
        public void AddFlatScore(int points)
        {
            Score += points;
            OnScoreChanged?.Invoke(Score);
            if (!_highScoreBeatenFired && Score > HighScore && HighScore > 0)
            {
                _highScoreBeatenFired = true;
                OnHighScoreBeaten?.Invoke(Score);
            }
        }

        /// <summary>Persist the score if it beats the stored high score. Returns true if a new record.</summary>
        public bool CommitHighScore()
        {
            bool newRecord = Score > HighScore;
            if (newRecord)
            {
                HighScore = Score;
                PlayerPrefs.SetInt(Constants.PrefsHighScore, HighScore);
            }
            HighScoreTable.AddScore(Score);
            PlayerPrefs.Save();
            return newRecord;
        }
    }
}

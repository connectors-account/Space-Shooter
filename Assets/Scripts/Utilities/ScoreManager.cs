using System;
using UnityEngine;

namespace SpaceShooter.Utilities
{
    /// <summary>
    /// Tracks the current score, a combo multiplier for rapid kills, and the persisted high score.
    /// </summary>
    public class ScoreManager : MonoBehaviour
    {
        public static ScoreManager Instance { get; private set; }

        private const string HighScoreKey = "SpaceShooter_HighScore";

        [Header("Combo")]
        [Tooltip("Seconds allowed between kills to keep the combo alive.")]
        public float comboWindow = 2f;
        [Tooltip("Highest multiplier the combo can reach.")]
        public int maxMultiplier = 8;

        public int CurrentScore { get; private set; }
        public int Multiplier { get; private set; } = 1;

        private float _lastKillTime = -999f;
        private int _comboCount;

        public event Action<int> OnScoreChanged;
        public event Action<int> OnMultiplierChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        /// <summary>Add points scaled by the current combo multiplier.</summary>
        public void AddScore(int baseAmount)
        {
            UpdateCombo();
            CurrentScore += baseAmount * Multiplier;
            OnScoreChanged?.Invoke(CurrentScore);
        }

        /// <summary>Add raw points without touching the combo (e.g. wave clear bonus).</summary>
        public void AddRaw(int amount)
        {
            CurrentScore += amount;
            OnScoreChanged?.Invoke(CurrentScore);
        }

        private void UpdateCombo()
        {
            if (Time.time - _lastKillTime <= comboWindow)
            {
                _comboCount++;
                int newMult = Mathf.Clamp(1 + _comboCount / 3, 1, maxMultiplier);
                if (newMult != Multiplier)
                {
                    Multiplier = newMult;
                    OnMultiplierChanged?.Invoke(Multiplier);
                }
            }
            else
            {
                _comboCount = 0;
                if (Multiplier != 1)
                {
                    Multiplier = 1;
                    OnMultiplierChanged?.Invoke(Multiplier);
                }
            }
            _lastKillTime = Time.time;
        }

        private void Update()
        {
            // Decay the multiplier once the combo window lapses.
            if (Multiplier > 1 && Time.time - _lastKillTime > comboWindow)
            {
                Multiplier = 1;
                _comboCount = 0;
                OnMultiplierChanged?.Invoke(Multiplier);
            }
        }

        public int GetScore() => CurrentScore;

        public int GetHighScore() => PlayerPrefs.GetInt(HighScoreKey, 0);

        /// <summary>Persist the score if it beats the stored high score. Returns true if a new record.</summary>
        public bool SaveHighScore()
        {
            if (CurrentScore > GetHighScore())
            {
                PlayerPrefs.SetInt(HighScoreKey, CurrentScore);
                PlayerPrefs.Save();
                return true;
            }
            return false;
        }

        public void ResetScore()
        {
            CurrentScore = 0;
            Multiplier = 1;
            _comboCount = 0;
            _lastKillTime = -999f;
            OnScoreChanged?.Invoke(CurrentScore);
            OnMultiplierChanged?.Invoke(Multiplier);
        }
    }
}

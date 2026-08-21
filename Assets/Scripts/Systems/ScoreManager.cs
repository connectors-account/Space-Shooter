using System;
using UnityEngine;
using SpaceShooter.Core;

namespace SpaceShooter.Systems
{
    /// <summary>
    /// Tracks score and a time-based combo multiplier (up to 8x). Persists the high score.
    /// </summary>
    public class ScoreManager : MonoBehaviour
    {
        public static ScoreManager Instance { get; private set; }

        [Header("Combo")]
        [SerializeField] private float comboWindow = 2f;
        [SerializeField] private int maxMultiplier = 8;

        public int Score { get; private set; }
        public int HighScore { get; private set; }
        public int Multiplier { get; private set; } = 1;

        public event Action<int> OnScoreChanged;
        public event Action<int> OnComboChanged;

        private float comboTimer;
        private int comboHits;

        private const string HighScoreKey = "SpaceShooter_HighScore";

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

        private void Start()
        {
            OnScoreChanged?.Invoke(Score);
            OnComboChanged?.Invoke(Multiplier);
        }

        private void Update()
        {
            if (comboTimer > 0f)
            {
                comboTimer -= Time.deltaTime;
                if (comboTimer <= 0f)
                {
                    ResetCombo();
                }
            }
        }

        public void AddScore(int points)
        {
            // A "hit" contributes to the combo.
            comboHits++;
            comboTimer = comboWindow;
            int newMultiplier = Mathf.Clamp(1 + comboHits / 3, 1, maxMultiplier);
            if (newMultiplier != Multiplier)
            {
                Multiplier = newMultiplier;
                OnComboChanged?.Invoke(Multiplier);
            }

            Score += points * Multiplier;
            OnScoreChanged?.Invoke(Score);

            if (GameManager.Instance != null) GameManager.Instance.SetScore(Score);

            if (Score > HighScore)
            {
                HighScore = Score;
            }
        }

        private void ResetCombo()
        {
            comboHits = 0;
            if (Multiplier != 1)
            {
                Multiplier = 1;
                OnComboChanged?.Invoke(Multiplier);
            }
        }

        public void SaveHighScore()
        {
            if (Score > PlayerPrefs.GetInt(HighScoreKey, 0))
            {
                PlayerPrefs.SetInt(HighScoreKey, Score);
                PlayerPrefs.Save();
                HighScore = Score;
            }
        }

        public static string FormatScore(int value)
        {
            return value.ToString("N0", System.Globalization.CultureInfo.InvariantCulture);
        }
    }
}

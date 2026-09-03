using System;
using UnityEngine;

namespace SpaceShooter.Core
{
    /// <summary>
    /// Tracks the current run score and the persisted high score.
    /// Persistent singleton. Broadcasts changes for the HUD and other listeners.
    /// </summary>
    public class ScoreManager : MonoBehaviour
    {
        #region Singleton
        public static ScoreManager Instance { get; private set; }
        #endregion

        #region Events
        /// <summary>Fired whenever the current score changes. Argument = new score.</summary>
        public static event Action<int> OnScoreChanged;
        /// <summary>Fired the moment the current score exceeds the stored high score.</summary>
        public static event Action<int> OnHighScoreBeaten;
        #endregion

        #region Fields
        private int _score;
        private int _highScore;
        private bool _highScoreBeatenThisRun;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            _highScore = PlayerPrefs.GetInt(GameConstants.PREF_HIGH_SCORE, 0);
        }
        #endregion

        #region Public API
        /// <summary>Adds points to the current score and updates the high score if beaten.</summary>
        public void AddScore(int pts)
        {
            if (pts == 0) return;
            _score += pts;
            if (_score < 0) _score = 0;

            OnScoreChanged?.Invoke(_score);

            if (_score > _highScore)
            {
                _highScore = _score;
                PlayerPrefs.SetInt(GameConstants.PREF_HIGH_SCORE, _highScore);
                PlayerPrefs.Save();

                if (!_highScoreBeatenThisRun)
                {
                    _highScoreBeatenThisRun = true;
                    OnHighScoreBeaten?.Invoke(_highScore);
                }
            }
        }

        /// <summary>Returns the current run score.</summary>
        public int GetScore() => _score;

        /// <summary>Returns the persisted high score.</summary>
        public int GetHighScore() => _highScore;

        /// <summary>Resets the current score to zero (call at run start).</summary>
        public void ResetScore()
        {
            _score = 0;
            _highScoreBeatenThisRun = false;
            OnScoreChanged?.Invoke(_score);
        }

        /// <summary>Returns true if the high score has already been beaten this run.</summary>
        public bool WasHighScoreBeaten() => _highScoreBeatenThisRun;
        #endregion
    }
}

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SpaceShooter.Core;
using SpaceShooter.Player;
using SpaceShooter.Pickups;

namespace SpaceShooter.UI
{
    /// <summary>
    /// In-game HUD. Subscribes to score, health, shield, wave and power-up events
    /// and updates the relevant UI elements on callback.
    /// </summary>
    public class HUDManager : MonoBehaviour
    {
        #region Inspector Fields
        [Header("Text")]
        [SerializeField] private TextMeshProUGUI _scoreText;
        [SerializeField] private TextMeshProUGUI _waveText;

        [Header("Health / Shield Icons")]
        [SerializeField] private Image[] _healthIcons = new Image[3];
        [SerializeField] private Image[] _shieldIcons = new Image[3];

        [Header("Power-Up")]
        [SerializeField] private Image _powerUpIcon;
        [SerializeField] private Image _powerUpTimerBar;
        [SerializeField] private GameObject _powerUpGroup;
        #endregion

        #region Private
        private float _powerUpDuration;
        private float _powerUpElapsed;
        private bool _powerUpActive;
        #endregion

        #region Unity Lifecycle
        private void OnEnable()
        {
            ScoreManager.OnScoreChanged += HandleScoreChanged;
            PlayerHealth.OnHealthChanged += HandleHealthChanged;
            PlayerHealth.OnShieldChanged += HandleShieldChanged;
            WaveManager.OnWaveStart += HandleWaveStart;
            PlayerPowerUp.OnPowerUpActivated += HandlePowerUpActivated;
            PlayerPowerUp.OnPowerUpExpired += HandlePowerUpExpired;

            InitDisplay();
        }

        private void OnDisable()
        {
            ScoreManager.OnScoreChanged -= HandleScoreChanged;
            PlayerHealth.OnHealthChanged -= HandleHealthChanged;
            PlayerHealth.OnShieldChanged -= HandleShieldChanged;
            WaveManager.OnWaveStart -= HandleWaveStart;
            PlayerPowerUp.OnPowerUpActivated -= HandlePowerUpActivated;
            PlayerPowerUp.OnPowerUpExpired -= HandlePowerUpExpired;
        }

        private void Update()
        {
            if (_powerUpActive && _powerUpTimerBar != null)
            {
                _powerUpElapsed += Time.deltaTime;
                float remaining = Mathf.Clamp01(1f - _powerUpElapsed / Mathf.Max(0.01f, _powerUpDuration));
                _powerUpTimerBar.fillAmount = remaining;
                if (remaining <= 0f) HandlePowerUpExpired();
            }
        }
        #endregion

        #region Init
        private void InitDisplay()
        {
            HandleScoreChanged(ScoreManager.Instance != null ? ScoreManager.Instance.GetScore() : 0);
            SetIcons(_healthIcons, GameConstants.PLAYER_MAX_HEALTH);
            SetIcons(_shieldIcons, 0);
            if (_waveText != null) _waveText.text = "WAVE 1";
            if (_powerUpGroup != null) _powerUpGroup.SetActive(false);
        }
        #endregion

        #region Event Handlers
        private void HandleScoreChanged(int score)
        {
            if (_scoreText != null) _scoreText.text = $"SCORE: {score}";
        }

        private void HandleHealthChanged(int current, int max)
        {
            SetIcons(_healthIcons, current);
        }

        private void HandleShieldChanged(int current, int max)
        {
            SetIcons(_shieldIcons, current);
        }

        private void HandleWaveStart(int waveNumber, string waveName)
        {
            if (_waveText != null) _waveText.text = $"WAVE {waveNumber}";
        }

        private void HandlePowerUpActivated(PowerUpType type, float duration)
        {
            _powerUpActive = true;
            _powerUpDuration = duration;
            _powerUpElapsed = 0f;

            if (_powerUpGroup != null) _powerUpGroup.SetActive(true);
            if (_powerUpIcon != null) _powerUpIcon.color = PowerUp.GetColour(type);
            if (_powerUpTimerBar != null) _powerUpTimerBar.fillAmount = 1f;
        }

        private void HandlePowerUpExpired()
        {
            _powerUpActive = false;
            if (_powerUpGroup != null) _powerUpGroup.SetActive(false);
        }
        #endregion

        #region Helpers
        private void SetIcons(Image[] icons, int activeCount)
        {
            if (icons == null) return;
            for (int i = 0; i < icons.Length; i++)
            {
                if (icons[i] != null)
                    icons[i].enabled = i < activeCount;
            }
        }
        #endregion
    }
}

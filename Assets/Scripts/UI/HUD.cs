using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SpaceShooter.Core;
using SpaceShooter.Player;

namespace SpaceShooter.UI
{
    /// <summary>
    /// In-game heads-up display: score, high score, wave, health bar (color gradient),
    /// lives, and an active power-up timer bar.
    /// </summary>
    public class HUD : MonoBehaviour
    {
        [Header("Score / Wave")]
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private TMP_Text highScoreText;
        [SerializeField] private TMP_Text waveText;
        [SerializeField] private TMP_Text livesText;

        [Header("Health Bar")]
        [SerializeField] private Image healthFill;
        [SerializeField] private Gradient healthGradient;

        [Header("Power-up Timer")]
        [SerializeField] private GameObject powerUpTimerRoot;
        [SerializeField] private Image powerUpTimerFill;
        [SerializeField] private TMP_Text powerUpLabel;

        [Header("References")]
        [SerializeField] private PlayerHealth playerHealth;
        [SerializeField] private PlayerPowerUp playerPowerUp;

        private void Start()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnScoreChanged += UpdateScore;
                GameManager.Instance.OnWaveChanged += UpdateWave;
                GameManager.Instance.OnLivesChanged += UpdateLives;

                UpdateScore(GameManager.Instance.Score);
                UpdateWave(GameManager.Instance.WaveNumber);
                UpdateLives(GameManager.Instance.Lives);
                if (highScoreText != null)
                {
                    highScoreText.text = $"HI: {GameManager.Instance.HighScore}";
                }
            }

            if (playerHealth != null)
            {
                playerHealth.OnHealthChanged += UpdateHealth;
                UpdateHealth(playerHealth.CurrentHealth, playerHealth.MaxHealth);
            }

            if (playerPowerUp != null)
            {
                playerPowerUp.OnPowerUpActivated += HandlePowerUpActivated;
                playerPowerUp.OnPowerUpTick += HandlePowerUpTick;
                playerPowerUp.OnPowerUpExpired += HandlePowerUpExpired;
            }

            if (powerUpTimerRoot != null)
            {
                powerUpTimerRoot.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnScoreChanged -= UpdateScore;
                GameManager.Instance.OnWaveChanged -= UpdateWave;
                GameManager.Instance.OnLivesChanged -= UpdateLives;
            }
            if (playerHealth != null)
            {
                playerHealth.OnHealthChanged -= UpdateHealth;
            }
            if (playerPowerUp != null)
            {
                playerPowerUp.OnPowerUpActivated -= HandlePowerUpActivated;
                playerPowerUp.OnPowerUpTick -= HandlePowerUpTick;
                playerPowerUp.OnPowerUpExpired -= HandlePowerUpExpired;
            }
        }

        private void UpdateScore(int score)
        {
            if (scoreText != null) scoreText.text = $"SCORE: {score}";
            if (highScoreText != null && GameManager.Instance != null)
            {
                highScoreText.text = $"HI: {GameManager.Instance.HighScore}";
            }
        }

        private void UpdateWave(int wave)
        {
            if (waveText != null)
            {
                waveText.text = wave > 0 ? $"WAVE {wave}" : "";
            }
        }

        private void UpdateLives(int lives)
        {
            if (livesText != null)
            {
                livesText.text = $"LIVES: {lives}";
            }
        }

        private void UpdateHealth(int current, int max)
        {
            if (healthFill == null) return;
            float ratio = max > 0 ? (float)current / max : 0f;
            healthFill.fillAmount = ratio;
            if (healthGradient != null)
            {
                healthFill.color = healthGradient.Evaluate(ratio);
            }
        }

        private void HandlePowerUpActivated(PowerUpType type)
        {
            if (powerUpTimerRoot != null) powerUpTimerRoot.SetActive(true);
            if (powerUpLabel != null) powerUpLabel.text = type.ToString().ToUpper();
        }

        private void HandlePowerUpTick(PowerUpType type, float remaining, float total)
        {
            if (powerUpTimerFill != null && total > 0f)
            {
                powerUpTimerFill.fillAmount = remaining / total;
            }
        }

        private void HandlePowerUpExpired(PowerUpType type)
        {
            if (playerPowerUp != null && !playerPowerUp.HasActivePowerUp && powerUpTimerRoot != null)
            {
                powerUpTimerRoot.SetActive(false);
            }
        }
    }
}

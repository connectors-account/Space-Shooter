// ============================================================================
// HUDManager.cs — In-game heads-up display
// Updates health bar, score, wave counter, combo multiplier, and lives.
// ============================================================================
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SpaceShooter.Core;
using SpaceShooter.Player;

namespace SpaceShooter.UI
{
    public class HUDManager : MonoBehaviour
    {
        [Header("Health")]
        [SerializeField] private Slider healthBar;
        [SerializeField] private Image healthFill;
        [SerializeField] private Color healthFullColor = Color.green;
        [SerializeField] private Color healthLowColor = Color.red;

        [Header("Text Elements")]
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI waveText;
        [SerializeField] private TextMeshProUGUI comboText;
        [SerializeField] private TextMeshProUGUI livesText;

        private void OnEnable()
        {
            // Subscribe to GameManager events
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnScoreChanged += UpdateScore;
                GameManager.Instance.OnWaveChanged += UpdateWave;
                GameManager.Instance.OnComboChanged += UpdateCombo;
            }

            // Subscribe to PlayerHealth events
            if (PlayerHealth.Instance != null)
            {
                PlayerHealth.Instance.OnHealthChanged += UpdateHealth;
                PlayerHealth.Instance.OnLivesChanged += UpdateLives;
            }

            // Initial display
            UpdateScore(GameManager.Instance != null ? GameManager.Instance.Score : 0);
            UpdateWave(GameManager.Instance != null ? GameManager.Instance.CurrentWave : 0);
            UpdateCombo(1);
        }

        private void OnDisable()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnScoreChanged -= UpdateScore;
                GameManager.Instance.OnWaveChanged -= UpdateWave;
                GameManager.Instance.OnComboChanged -= UpdateCombo;
            }
            if (PlayerHealth.Instance != null)
            {
                PlayerHealth.Instance.OnHealthChanged -= UpdateHealth;
                PlayerHealth.Instance.OnLivesChanged -= UpdateLives;
            }
        }

        // ---- Late subscription helper (player might spawn after HUD) ----
        private void Start()
        {
            // Re-check player health subscription in case player spawned after OnEnable
            if (PlayerHealth.Instance != null)
            {
                PlayerHealth.Instance.OnHealthChanged -= UpdateHealth;
                PlayerHealth.Instance.OnLivesChanged -= UpdateLives;
                PlayerHealth.Instance.OnHealthChanged += UpdateHealth;
                PlayerHealth.Instance.OnLivesChanged += UpdateLives;

                UpdateHealth(PlayerHealth.Instance.CurrentHealth, 100);
                UpdateLives(PlayerHealth.Instance.Lives);
            }
        }

        // ====================================================================
        // Update methods
        // ====================================================================
        private void UpdateHealth(int current, int max)
        {
            if (healthBar != null)
            {
                healthBar.maxValue = max;
                healthBar.value = current;
            }
            if (healthFill != null)
            {
                float t = max > 0 ? (float)current / max : 0f;
                healthFill.color = Color.Lerp(healthLowColor, healthFullColor, t);
            }
        }

        private void UpdateScore(int score)
        {
            if (scoreText != null)
                scoreText.text = $"SCORE: {score:N0}";
        }

        private void UpdateWave(int wave)
        {
            if (waveText != null)
                waveText.text = $"WAVE {wave}";
        }

        private void UpdateCombo(int combo)
        {
            if (comboText != null)
            {
                comboText.text = combo > 1 ? $"x{combo} COMBO" : "";
                comboText.color = combo >= 5 ? Color.yellow : Color.white;
            }
        }

        private void UpdateLives(int lives)
        {
            if (livesText != null)
                livesText.text = $"LIVES: {lives}";
        }
    }
}

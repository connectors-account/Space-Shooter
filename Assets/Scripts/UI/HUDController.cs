using SpaceShooter.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceShooter.UI
{
    public class HUDController : MonoBehaviour
    {
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private TMP_Text waveText;
        [SerializeField] private TMP_Text livesText;
        [SerializeField] private Slider healthSlider;

        private void OnEnable()
        {
            if (GameManager.Instance == null)
            {
                return;
            }

            GameManager.Instance.OnScoreChanged += UpdateScore;
            GameManager.Instance.OnWaveChanged += UpdateWave;
            GameManager.Instance.OnLivesChanged += UpdateLives;
            GameManager.Instance.OnPlayerHealthChanged += UpdateHealth;

            UpdateScore(GameManager.Instance.Score);
            UpdateWave(GameManager.Instance.Wave);
            UpdateLives(GameManager.Instance.Lives);
        }

        private void OnDisable()
        {
            if (GameManager.Instance == null)
            {
                return;
            }

            GameManager.Instance.OnScoreChanged -= UpdateScore;
            GameManager.Instance.OnWaveChanged -= UpdateWave;
            GameManager.Instance.OnLivesChanged -= UpdateLives;
            GameManager.Instance.OnPlayerHealthChanged -= UpdateHealth;
        }

        private void UpdateScore(int score)
        {
            if (scoreText != null)
            {
                scoreText.text = $"Score: {score}";
            }
        }

        private void UpdateWave(int wave)
        {
            if (waveText != null)
            {
                waveText.text = $"Wave: {wave}";
            }
        }

        private void UpdateLives(int lives)
        {
            if (livesText != null)
            {
                livesText.text = $"Lives: {lives}";
            }
        }

        private void UpdateHealth(float current, float max)
        {
            if (healthSlider == null)
            {
                return;
            }

            healthSlider.maxValue = max;
            healthSlider.value = current;
        }
    }
}

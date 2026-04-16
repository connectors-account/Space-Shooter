using SpaceShooter.Combat;
using SpaceShooter.Core;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceShooter.UI
{
    public class HUDController : MonoBehaviour
    {
        [SerializeField] private Text healthText;
        [SerializeField] private Text scoreText;
        [SerializeField] private Text waveText;
        [SerializeField] private Health playerHealth;
        [SerializeField] private WaveProgressionManager waveProgressionManager;

        private void Start()
        {
            if (playerHealth == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    playerHealth = player.GetComponent<Health>();
                }
            }

            if (waveProgressionManager == null)
            {
                waveProgressionManager = FindObjectOfType<WaveProgressionManager>();
            }

            ScoreManager.OnScoreChanged += UpdateScoreText;

            if (playerHealth != null)
            {
                playerHealth.OnHealthChanged += UpdateHealthText;
                UpdateHealthText(playerHealth.CurrentHealth, playerHealth.MaxHealth);
            }

            if (waveProgressionManager != null)
            {
                waveProgressionManager.OnWaveChanged += UpdateWaveText;
                UpdateWaveText(waveProgressionManager.CurrentWaveNumber);
            }

            UpdateScoreText(ScoreManager.CurrentScore);
        }

        private void OnDestroy()
        {
            ScoreManager.OnScoreChanged -= UpdateScoreText;

            if (playerHealth != null)
            {
                playerHealth.OnHealthChanged -= UpdateHealthText;
            }

            if (waveProgressionManager != null)
            {
                waveProgressionManager.OnWaveChanged -= UpdateWaveText;
            }
        }

        private void UpdateHealthText(int current, int max)
        {
            if (healthText != null)
            {
                healthText.text = $"Health: {current}/{max}";
            }
        }

        private void UpdateScoreText(int score)
        {
            if (scoreText != null)
            {
                scoreText.text = $"Score: {score}";
            }
        }

        private void UpdateWaveText(int wave)
        {
            if (waveText != null)
            {
                waveText.text = $"Wave: {Mathf.Max(1, wave)}";
            }
        }
    }
}

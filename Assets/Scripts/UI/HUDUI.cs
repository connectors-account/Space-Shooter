using UnityEngine;
using UnityEngine.UI;

namespace SpaceShooter.UI
{
    /// <summary>
    /// Handles in-game HUD updates (score, wave, and health).
    /// </summary>
    public class HUDUI : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private Text scoreText;
        [SerializeField] private Text waveText;
        [SerializeField] private Slider healthSlider;
        [SerializeField] private Text healthText;

        public void Show()
        {
            if (panel != null)
            {
                panel.SetActive(true);
            }
        }

        public void Hide()
        {
            if (panel != null)
            {
                panel.SetActive(false);
            }
        }

        public void SetScore(int score)
        {
            if (scoreText != null)
            {
                scoreText.text = $"Score: {score}";
            }
        }

        public void SetWave(int wave)
        {
            if (waveText != null)
            {
                waveText.text = $"Wave {wave}";
            }
        }

        public void SetHealth(int current, int max)
        {
            if (healthSlider != null)
            {
                healthSlider.maxValue = max;
                healthSlider.value = current;
            }

            if (healthText != null)
            {
                healthText.text = $"{current} / {max}";
            }
        }
    }
}

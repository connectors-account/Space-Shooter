using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace SpaceShooter.UI
{
    /// <summary>
    /// In-game heads-up display showing health, score, and wave number.
    /// </summary>
    public class HudUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject hudPanel;
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI waveText;
        [SerializeField] private TextMeshProUGUI healthText;
        [SerializeField] private Image[] healthIcons; // optional heart icons

        private void Start()
        {
            var gm = Managers.GameManager.Instance;
            if (gm != null)
            {
                gm.OnScoreChanged += UpdateScore;
                gm.OnWaveChanged += UpdateWave;
                gm.OnHealthChanged += UpdateHealth;
                gm.OnGameStateChanged += OnGameStateChanged;
            }

            Hide();
        }

        private void OnDestroy()
        {
            var gm = Managers.GameManager.Instance;
            if (gm != null)
            {
                gm.OnScoreChanged -= UpdateScore;
                gm.OnWaveChanged -= UpdateWave;
                gm.OnHealthChanged -= UpdateHealth;
                gm.OnGameStateChanged -= OnGameStateChanged;
            }
        }

        private void OnGameStateChanged(Managers.GameState state)
        {
            if (state == Managers.GameState.Playing)
                Show();
            else
                Hide();
        }

        private void UpdateScore(int score)
        {
            if (scoreText != null)
                scoreText.text = $"SCORE: {score}";
        }

        private void UpdateWave(int wave)
        {
            if (waveText != null)
                waveText.text = $"WAVE {wave}";

            // Brief flash animation for wave text
            if (waveText != null)
                StartCoroutine(FlashText(waveText));
        }

        private void UpdateHealth(int current, int max)
        {
            if (healthText != null)
                healthText.text = $"LIVES: {current}/{max}";

            // Update heart icons if available
            if (healthIcons != null)
            {
                for (int i = 0; i < healthIcons.Length; i++)
                {
                    if (healthIcons[i] != null)
                        healthIcons[i].enabled = i < current;
                }
            }
        }

        private System.Collections.IEnumerator FlashText(TextMeshProUGUI text)
        {
            Color original = text.color;
            text.color = Color.yellow;
            text.fontSize *= 1.3f;
            yield return new WaitForSeconds(0.5f);
            text.color = original;
            text.fontSize /= 1.3f;
        }

        public void Show()
        {
            if (hudPanel != null) hudPanel.SetActive(true);
        }

        public void Hide()
        {
            if (hudPanel != null) hudPanel.SetActive(false);
        }
    }
}

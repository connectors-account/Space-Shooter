// =============================================================================
// HUDController.cs — In-game heads-up display
// =============================================================================
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace SpaceShooter.UI
{
    /// <summary>
    /// Controls the in-game HUD showing score, lives, health, and wave info.
    /// </summary>
    public class HUDController : MonoBehaviour
    {
        [Header("Score")]
        [SerializeField] private Text scoreText;
        [SerializeField] private Text highScoreText;

        [Header("Lives")]
        [SerializeField] private Text livesText;
        [SerializeField] private Image[] lifeIcons;

        [Header("Health Bar")]
        [SerializeField] private Slider healthBar;
        [SerializeField] private Image healthFill;
        [SerializeField] private Color healthColorFull = Color.green;
        [SerializeField] private Color healthColorLow = Color.red;

        [Header("Wave Announcement")]
        [SerializeField] private Text waveText;
        [SerializeField] private CanvasGroup waveGroup;
        [SerializeField] private float waveFadeDuration = 2f;

        [Header("Pause Menu")]
        [SerializeField] private GameObject pausePanel;

        private Managers.GameManager gm;

        private void Start()
        {
            gm = Managers.GameManager.Instance;
            if (gm == null) return;

            // Subscribe to events
            gm.OnScoreChanged += UpdateScore;
            gm.OnLivesChanged += UpdateLives;
            gm.OnHealthChanged += UpdateHealth;
            gm.OnWaveAnnounce += ShowWaveAnnouncement;
            gm.OnGameStateChanged += OnGameStateChanged;

            // Initialize display
            UpdateScore(gm.Score);
            UpdateLives(gm.Lives);
            if (highScoreText != null)
                highScoreText.text = $"HI: {gm.HighScore:N0}";

            if (pausePanel != null)
                pausePanel.SetActive(false);

            if (waveGroup != null)
                waveGroup.alpha = 0f;
        }

        private void OnDestroy()
        {
            if (gm == null) return;
            gm.OnScoreChanged -= UpdateScore;
            gm.OnLivesChanged -= UpdateLives;
            gm.OnHealthChanged -= UpdateHealth;
            gm.OnWaveAnnounce -= ShowWaveAnnouncement;
            gm.OnGameStateChanged -= OnGameStateChanged;
        }

        /// <summary>
        /// Updates the score display.
        /// </summary>
        private void UpdateScore(int newScore)
        {
            if (scoreText != null)
                scoreText.text = $"SCORE: {newScore:N0}";
        }

        /// <summary>
        /// Updates the lives display.
        /// </summary>
        private void UpdateLives(int newLives)
        {
            if (livesText != null)
                livesText.text = $"x{newLives}";

            if (lifeIcons != null)
            {
                for (int i = 0; i < lifeIcons.Length; i++)
                {
                    if (lifeIcons[i] != null)
                        lifeIcons[i].enabled = i < newLives;
                }
            }
        }

        /// <summary>
        /// Updates the health bar slider and color.
        /// </summary>
        private void UpdateHealth(int current, int max)
        {
            if (healthBar != null)
            {
                healthBar.maxValue = max;
                healthBar.value = current;
            }

            if (healthFill != null)
            {
                float pct = max > 0 ? (float)current / max : 0f;
                healthFill.color = Color.Lerp(healthColorLow, healthColorFull, pct);
            }
        }

        /// <summary>
        /// Shows a wave announcement with fade-in/out.
        /// </summary>
        private void ShowWaveAnnouncement(int waveNumber, bool isBoss)
        {
            if (waveText == null || waveGroup == null) return;

            waveText.text = isBoss ? $"!! BOSS WAVE !!" : $"WAVE {waveNumber}";
            waveText.color = isBoss ? Color.red : Color.white;
            StopCoroutine("FadeWaveText");
            StartCoroutine(FadeWaveText());
        }

        private IEnumerator FadeWaveText()
        {
            // Fade in
            float t = 0f;
            while (t < 0.5f)
            {
                t += Time.deltaTime;
                waveGroup.alpha = t / 0.5f;
                yield return null;
            }
            waveGroup.alpha = 1f;

            // Hold
            yield return new WaitForSeconds(waveFadeDuration);

            // Fade out
            t = 0f;
            while (t < 0.5f)
            {
                t += Time.deltaTime;
                waveGroup.alpha = 1f - (t / 0.5f);
                yield return null;
            }
            waveGroup.alpha = 0f;
        }

        /// <summary>
        /// Handles game state changes (shows/hides pause panel).
        /// </summary>
        private void OnGameStateChanged(Managers.GameState state)
        {
            if (pausePanel != null)
                pausePanel.SetActive(state == Managers.GameState.Paused);
        }

        // Button callbacks
        public void OnResumeButtonClicked()
        {
            gm?.TogglePause();
        }

        public void OnMainMenuButtonClicked()
        {
            gm?.GoToMainMenu();
        }

        public void OnQuitButtonClicked()
        {
            gm?.QuitGame();
        }
    }
}

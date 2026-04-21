using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceShooter
{
    /// <summary>
    /// Runtime HUD controller for score, health, wave, and status banners.
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("HUD Root")]
        [SerializeField] private GameObject hudRoot;

        [Header("HUD Fields")]
        [SerializeField] private Text scoreText;
        [SerializeField] private Text healthText;
        [SerializeField] private Text waveText;
        [SerializeField] private Text weaponText;
        [SerializeField] private Text shieldText;

        [Header("Overlays")]
        [SerializeField] private Text waveBannerText;
        [SerializeField] private Text gameOverStatsText;

        private Coroutine bannerRoutine;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        public void RefreshAll(int score, int health, int maxHealth, int wave)
        {
            UpdateScore(score);
            UpdateHealth(health, maxHealth);
            UpdateWave(wave);
            UpdateWeaponLevel(1);
            SetShieldActive(false);
        }

        public void ShowGameplayHud()
        {
            if (hudRoot != null)
            {
                hudRoot.SetActive(true);
            }
        }

        public void UpdateScore(int score)
        {
            if (scoreText != null)
            {
                scoreText.text = $"Score: {score}";
            }
        }

        public void UpdateHealth(int currentHealth, int maxHealth)
        {
            if (healthText != null)
            {
                healthText.text = $"HP: {currentHealth}/{maxHealth}";
            }
        }

        public void UpdateWave(int waveNumber)
        {
            if (waveText != null)
            {
                waveText.text = $"Wave: {waveNumber}";
            }
        }

        public void UpdateWeaponLevel(int level)
        {
            if (weaponText != null)
            {
                weaponText.text = $"Weapon Lv: {Mathf.Clamp(level, 1, 3)}";
            }
        }

        public void SetShieldActive(bool active)
        {
            if (shieldText != null)
            {
                shieldText.text = active ? "Shield: ON" : "Shield: OFF";
            }
        }

        public void ShowWaveCompleteBanner(int waveNumber)
        {
            if (waveBannerText == null)
            {
                return;
            }

            if (bannerRoutine != null)
            {
                StopCoroutine(bannerRoutine);
            }

            bannerRoutine = StartCoroutine(WaveBannerRoutine($"Wave {waveNumber} Cleared!"));
        }

        public void ShowGameOver(int finalScore, int highScore, int waveReached)
        {
            if (gameOverStatsText != null)
            {
                gameOverStatsText.text =
                    $"GAME OVER\nFinal Score: {finalScore}\nHigh Score: {highScore}\nWave Reached: {waveReached}";
            }
        }

        private IEnumerator WaveBannerRoutine(string message)
        {
            waveBannerText.gameObject.SetActive(true);
            waveBannerText.text = message;
            yield return new WaitForSeconds(1.4f);
            waveBannerText.gameObject.SetActive(false);
        }
    }
}

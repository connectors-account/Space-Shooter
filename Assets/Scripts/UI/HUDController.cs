using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SpaceShooter.Core;
using SpaceShooter.Systems;
using SpaceShooter.Player;
using SpaceShooter.Enemy;

namespace SpaceShooter.UI
{
    /// <summary>
    /// In-game heads-up display: health bar, lives, score, wave, boss bar,
    /// power-up timer and combo multiplier.
    /// </summary>
    public class HUDController : MonoBehaviour
    {
        [Header("Health")]
        [SerializeField] private Image healthFill;

        [Header("Lives")]
        [SerializeField] private Transform livesContainer;
        [SerializeField] private GameObject heartIconPrefab;
        [SerializeField] private Sprite heartSprite;

        [Header("Score / Wave")]
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private TMP_Text waveText;
        [SerializeField] private TMP_Text comboText;

        [Header("Boss")]
        [SerializeField] private GameObject bossHealthRoot;
        [SerializeField] private Image bossHealthFill;

        [Header("Power-up Timer")]
        [SerializeField] private GameObject powerUpRoot;
        [SerializeField] private Image powerUpFill;

        [Header("Countdown")]
        [SerializeField] private TMP_Text countdownText;

        private PlayerShooter playerShooter;
        private readonly List<GameObject> heartIcons = new List<GameObject>();

        private void Start()
        {
            var playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                var health = playerObj.GetComponent<PlayerHealth>();
                if (health != null)
                {
                    health.OnHealthChanged += UpdateHealth;
                    UpdateHealth(health.CurrentHealth, health.MaxHealth);
                }
                playerShooter = playerObj.GetComponent<PlayerShooter>();
            }

            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.OnScoreChanged += UpdateScore;
                ScoreManager.Instance.OnComboChanged += UpdateCombo;
                UpdateScore(ScoreManager.Instance.Score);
                UpdateCombo(ScoreManager.Instance.Multiplier);
            }

            WaveManager.OnWaveStart += UpdateWave;
            WaveManager.OnCountdownTick += UpdateCountdown;
            EnemyBoss.OnBossSpawned += HandleBossSpawned;
            EnemyBoss.OnBossHealthChanged += UpdateBossHealth;
            EnemyBoss.OnBossDefeated += HandleBossDefeated;

            RefreshLives();

            if (bossHealthRoot != null) bossHealthRoot.SetActive(false);
            if (powerUpRoot != null) powerUpRoot.SetActive(false);
            if (countdownText != null) countdownText.gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.OnScoreChanged -= UpdateScore;
                ScoreManager.Instance.OnComboChanged -= UpdateCombo;
            }
            WaveManager.OnWaveStart -= UpdateWave;
            WaveManager.OnCountdownTick -= UpdateCountdown;
            EnemyBoss.OnBossSpawned -= HandleBossSpawned;
            EnemyBoss.OnBossHealthChanged -= UpdateBossHealth;
            EnemyBoss.OnBossDefeated -= HandleBossDefeated;
        }

        private void Update()
        {
            RefreshLives();
            UpdatePowerUpTimer();
        }

        private void UpdateHealth(int current, int max)
        {
            if (healthFill != null) healthFill.fillAmount = max > 0 ? (float)current / max : 0f;
        }

        private void UpdateScore(int score)
        {
            if (scoreText != null) scoreText.text = ScoreManager.FormatScore(score);
        }

        private void UpdateWave(int wave)
        {
            if (waveText != null) waveText.text = $"WAVE {wave}";
        }

        private void UpdateCombo(int multiplier)
        {
            if (comboText == null) return;
            if (multiplier > 1)
            {
                comboText.gameObject.SetActive(true);
                comboText.text = $"x{multiplier}";
                StartCoroutine(PulseCombo());
            }
            else
            {
                comboText.gameObject.SetActive(false);
            }
        }

        private IEnumerator PulseCombo()
        {
            if (comboText == null) yield break;
            Transform t = comboText.transform;
            t.localScale = Vector3.one * 1.5f;
            float elapsed = 0f;
            while (elapsed < 0.2f)
            {
                elapsed += Time.deltaTime;
                t.localScale = Vector3.Lerp(Vector3.one * 1.5f, Vector3.one, elapsed / 0.2f);
                yield return null;
            }
            t.localScale = Vector3.one;
        }

        private void RefreshLives()
        {
            if (livesContainer == null || GameManager.Instance == null) return;
            int lives = GameManager.Instance.Lives;
            if (heartIcons.Count == lives) return;

            foreach (var icon in heartIcons)
            {
                if (icon != null) Destroy(icon);
            }
            heartIcons.Clear();

            for (int i = 0; i < lives; i++)
            {
                GameObject icon;
                if (heartIconPrefab != null)
                {
                    icon = Instantiate(heartIconPrefab, livesContainer);
                }
                else
                {
                    icon = new GameObject("Heart", typeof(Image));
                    icon.transform.SetParent(livesContainer, false);
                    Image img = icon.GetComponent<Image>();
                    img.sprite = heartSprite;
                    img.color = Color.red;
                    img.rectTransform.sizeDelta = new Vector2(24, 24);
                }
                heartIcons.Add(icon);
            }
        }

        private void UpdatePowerUpTimer()
        {
            if (powerUpRoot == null || powerUpFill == null || playerShooter == null) return;
            if (playerShooter.PowerUpTimeRemaining > 0f && playerShooter.PowerUpTotalDuration > 0f)
            {
                powerUpRoot.SetActive(true);
                powerUpFill.fillAmount = playerShooter.PowerUpTimeRemaining / playerShooter.PowerUpTotalDuration;
            }
            else
            {
                powerUpRoot.SetActive(false);
            }
        }

        private void HandleBossSpawned(EnemyBoss boss)
        {
            if (bossHealthRoot != null) bossHealthRoot.SetActive(true);
        }

        private void UpdateBossHealth(int current, int max)
        {
            if (bossHealthFill != null) bossHealthFill.fillAmount = max > 0 ? (float)current / max : 0f;
        }

        private void HandleBossDefeated()
        {
            if (bossHealthRoot != null) bossHealthRoot.SetActive(false);
        }

        private void UpdateCountdown(float remaining)
        {
            if (countdownText == null) return;
            if (remaining > 0.05f)
            {
                countdownText.gameObject.SetActive(true);
                countdownText.text = $"NEXT WAVE IN {Mathf.CeilToInt(remaining)}";
            }
            else
            {
                countdownText.gameObject.SetActive(false);
            }
        }
    }
}

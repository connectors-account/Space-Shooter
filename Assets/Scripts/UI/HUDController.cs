using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SpaceShooter.Core;
using SpaceShooter.Utilities;
using SpaceShooter.Player;
using SpaceShooter.Enemy;
using SpaceShooter.PowerUps;

namespace SpaceShooter.UI
{
    /// <summary>
    /// In-game HUD: score (top-right), health bar + lives (top-left), wave indicator (top-center),
    /// boss health bar (bottom-center, hidden until a boss is active), and a power-up timer bar.
    /// References are wired by the setup script but can also be assigned in the inspector.
    /// </summary>
    public class HUDController : MonoBehaviour
    {
        [Header("Score / Wave")]
        public Text scoreText;
        public Text multiplierText;
        public Text waveText;
        public Text countdownText;

        [Header("Health / Lives")]
        public Image healthFill;
        public Text livesText;

        [Header("Boss")]
        public GameObject bossBarRoot;
        public Image bossHealthFill;

        [Header("Power-up timer")]
        public GameObject powerUpBarRoot;
        public Image powerUpFill;

        private PlayerHealth _playerHealth;
        private BossEnemy _trackedBoss;
        private readonly List<PowerUpBase> _activePowerUps = new List<PowerUpBase>();

        private void OnEnable()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnScoreChanged += SetScore;
                GameManager.Instance.OnLivesChanged += SetLives;
            }
            if (ScoreManager.Instance != null)
                ScoreManager.Instance.OnMultiplierChanged += SetMultiplier;
        }

        private void OnDisable()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnScoreChanged -= SetScore;
                GameManager.Instance.OnLivesChanged -= SetLives;
            }
            if (ScoreManager.Instance != null)
                ScoreManager.Instance.OnMultiplierChanged -= SetMultiplier;
        }

        private void Start()
        {
            HookPlayer();
            if (bossBarRoot != null) bossBarRoot.SetActive(false);
            if (powerUpBarRoot != null) powerUpBarRoot.SetActive(false);
            if (countdownText != null) countdownText.text = "";

            if (GameManager.Instance != null)
            {
                SetScore(GameManager.Instance.Score);
                SetLives(GameManager.Instance.Lives);
            }
        }

        private WaveManager _waveManager;

        private void HookWaves()
        {
            _waveManager = FindObjectOfType<WaveManager>();
            if (_waveManager == null) return;
            _waveManager.OnWaveStart += w => SetWave(w, _waveManager.TotalWaves);
            _waveManager.OnCountdownTick += (w, s) => SetCountdown(w, s);
        }

        private void HookPlayer()
        {
            HookWaves();
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player == null) return;
            _playerHealth = player.GetComponent<PlayerHealth>();
            if (_playerHealth != null)
            {
                _playerHealth.OnDamage += (cur, max) => SetHealth(cur, max);
                _playerHealth.OnHeal += (cur, max) => SetHealth(cur, max);
                SetHealth(_playerHealth.currentHealth, _playerHealth.maxHealth);
            }
        }

        private void Update()
        {
            // Boss bar tracking.
            if (_trackedBoss != null && bossHealthFill != null)
            {
                bossHealthFill.fillAmount = Mathf.Clamp01(_trackedBoss.CurrentHealth / (float)_trackedBoss.maxHealth);
            }

            // Power-up timer (show the freshest active timed power-up).
            _activePowerUps.RemoveAll(p => p == null);
            if (powerUpBarRoot != null)
            {
                if (_activePowerUps.Count > 0)
                {
                    powerUpBarRoot.SetActive(true);
                    if (powerUpFill != null)
                        powerUpFill.fillAmount = _activePowerUps[_activePowerUps.Count - 1].RemainingNormalized;
                }
                else
                {
                    powerUpBarRoot.SetActive(false);
                }
            }
        }

        // --- Setters -------------------------------------------------------

        public void SetScore(int score)
        {
            if (scoreText != null) scoreText.text = $"SCORE\n{score:N0}";
        }

        public void SetMultiplier(int mult)
        {
            if (multiplierText != null)
                multiplierText.text = mult > 1 ? $"x{mult}" : "";
        }

        public void SetLives(int lives)
        {
            if (livesText != null) livesText.text = "Lives: " + new string('*', Mathf.Max(0, lives));
        }

        public void SetHealth(int current, int max)
        {
            if (healthFill != null) healthFill.fillAmount = max > 0 ? Mathf.Clamp01(current / (float)max) : 0f;
        }

        public void SetWave(int wave, int total)
        {
            if (waveText != null) waveText.text = $"WAVE {wave} / {total}";
        }

        public void SetCountdown(int nextWave, float secondsRemaining)
        {
            if (countdownText == null) return;
            if (secondsRemaining > 0.05f)
                countdownText.text = $"WAVE {nextWave} IN {Mathf.CeilToInt(secondsRemaining)}";
            else
                countdownText.text = "";
        }

        // --- Boss hooks ----------------------------------------------------

        public void SetBossTracked(BossEnemy boss)
        {
            _trackedBoss = boss;
            if (bossBarRoot != null) bossBarRoot.SetActive(true);
            if (bossHealthFill != null) bossHealthFill.fillAmount = 1f;
        }

        public void ClearBossTracked(BossEnemy boss)
        {
            if (_trackedBoss == boss) _trackedBoss = null;
            if (bossBarRoot != null) bossBarRoot.SetActive(false);
        }

        // --- Power-up hooks ------------------------------------------------

        public void RegisterActivePowerUp(PowerUpBase p)
        {
            if (p != null && !_activePowerUps.Contains(p)) _activePowerUps.Add(p);
        }
    }
}

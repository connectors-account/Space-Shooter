using UnityEngine;
using UnityEngine.UI;

namespace SpaceShooter
{
    /// <summary>
    /// In-game heads-up display. Reactively updates health hearts, score, wave label,
    /// shield bar and the boss health bar by subscribing to gameplay events.
    /// </summary>
    public class HUDController : MonoBehaviour
    {
        [Header("Player references")]
        [SerializeField] private PlayerHealth playerHealth;

        [Header("UI elements")]
        [SerializeField] private Image[] healthHearts = new Image[3];
        [SerializeField] private Text scoreText;
        [SerializeField] private Text waveText;
        [SerializeField] private Text multiplierText;
        [SerializeField] private Slider shieldBar;
        [SerializeField] private Slider bossHpBar;

        private EnemyBoss _boss;

        private void Start()
        {
            if (playerHealth == null)
            {
                var playerGo = GameObject.FindWithTag("Player");
                if (playerGo != null) playerHealth = playerGo.GetComponent<PlayerHealth>();
            }
            Subscribe();
            RefreshAll();
        }

        private void OnDestroy() => Unsubscribe();

        private void Subscribe()
        {
            if (playerHealth != null)
            {
                playerHealth.OnHealthChanged += UpdateHealth;
                playerHealth.OnShieldChanged += UpdateShield;
            }
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.OnScoreChanged += UpdateScore;
                ScoreManager.Instance.OnMultiplierChanged += UpdateMultiplier;
            }
            if (WaveManager.Instance != null)
            {
                WaveManager.Instance.OnWaveStart += UpdateWave;
            }
            EnemyBoss.OnBossSpawn += OnBossSpawn;
        }

        private void Unsubscribe()
        {
            if (playerHealth != null)
            {
                playerHealth.OnHealthChanged -= UpdateHealth;
                playerHealth.OnShieldChanged -= UpdateShield;
            }
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.OnScoreChanged -= UpdateScore;
                ScoreManager.Instance.OnMultiplierChanged -= UpdateMultiplier;
            }
            if (WaveManager.Instance != null)
            {
                WaveManager.Instance.OnWaveStart -= UpdateWave;
            }
            EnemyBoss.OnBossSpawn -= OnBossSpawn;
        }

        private void Update()
        {
            if (_boss != null && bossHpBar != null)
            {
                var bossHealth = _boss.GetComponent<EnemyHealth>();
                if (bossHealth != null)
                {
                    bossHpBar.value = (float)bossHealth.CurrentHealth / bossHealth.maxHealth;
                }
            }
            else if (_boss == null && bossHpBar != null && bossHpBar.gameObject.activeSelf)
            {
                bossHpBar.gameObject.SetActive(false);
            }
        }

        private void RefreshAll()
        {
            if (playerHealth != null)
            {
                UpdateHealth(playerHealth.CurrentHealth);
                UpdateShield(playerHealth.ShieldHP);
            }
            if (ScoreManager.Instance != null)
            {
                UpdateScore(ScoreManager.Instance.GetScore());
                UpdateMultiplier(ScoreManager.Instance.Multiplier);
            }
            if (bossHpBar != null) bossHpBar.gameObject.SetActive(false);
        }

        private void UpdateHealth(int current)
        {
            if (healthHearts == null) return;
            for (int i = 0; i < healthHearts.Length; i++)
            {
                if (healthHearts[i] != null) healthHearts[i].enabled = i < current;
            }
        }

        private void UpdateShield(int shieldHp)
        {
            if (shieldBar == null) return;
            shieldBar.gameObject.SetActive(shieldHp > 0);
            shieldBar.maxValue = Mathf.Max(shieldBar.maxValue, shieldHp);
            shieldBar.value = shieldHp;
        }

        private void UpdateScore(int score)
        {
            if (scoreText != null) scoreText.text = $"SCORE: {score}";
        }

        private void UpdateMultiplier(int multiplier)
        {
            if (multiplierText != null) multiplierText.text = multiplier > 1 ? $"x{multiplier}" : string.Empty;
        }

        private void UpdateWave(int wave)
        {
            if (waveText != null) waveText.text = $"WAVE {wave}";
        }

        private void OnBossSpawn(EnemyBoss boss)
        {
            _boss = boss;
            if (bossHpBar != null)
            {
                bossHpBar.gameObject.SetActive(true);
                bossHpBar.minValue = 0f;
                bossHpBar.maxValue = 1f;
                bossHpBar.value = 1f;
            }
        }
    }
}

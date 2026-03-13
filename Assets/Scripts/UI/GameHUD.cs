using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// In-game heads-up display showing score, lives, health bar, wave number, combo.
/// Subscribes to GameManager events for real-time updates.
/// </summary>
public class GameHUD : MonoBehaviour
{
    [Header("Score")]
    public Text scoreText;
    public Text highScoreText;
    public Text comboText;

    [Header("Lives")]
    public Text livesText;
    public Image[] lifeIcons;

    [Header("Health Bar")]
    public Slider healthBar;
    public Image healthBarFill;

    [Header("Wave")]
    public Text waveText;
    public Text waveAnnouncementText;

    [Header("Boss")]
    public Slider bossHealthBar;
    public Text bossNameText;
    public GameObject bossHealthPanel;

    private float waveAnnouncementTimer = 0f;

    private void Start()
    {
        // Subscribe to GameManager events
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnScoreChanged += UpdateScore;
            GameManager.Instance.OnLivesChanged += UpdateLives;
            GameManager.Instance.OnComboChanged += UpdateCombo;
        }

        // Subscribe to player health
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            player.OnHealthChanged += UpdateHealthBar;
        }

        // Subscribe to wave events
        WaveManager waveManager = FindObjectOfType<WaveManager>();
        if (waveManager != null)
        {
            waveManager.OnWaveStart += ShowWaveAnnouncement;
            waveManager.OnBossSpawned += ShowBossHealth;
        }

        if (bossHealthPanel != null)
            bossHealthPanel.SetActive(false);

        if (waveAnnouncementText != null)
            waveAnnouncementText.gameObject.SetActive(false);

        // Initialize display
        UpdateScore(0);
        UpdateLives(GameManager.Instance != null ? GameManager.Instance.CurrentLives : 3);
        UpdateCombo(1);

        if (highScoreText != null)
            highScoreText.text = "HI: " + (GameManager.Instance != null ? GameManager.Instance.HighScore : 0).ToString("N0");
    }

    private void Update()
    {
        // Fade out wave announcement
        if (waveAnnouncementTimer > 0f)
        {
            waveAnnouncementTimer -= Time.deltaTime;
            if (waveAnnouncementTimer <= 0f && waveAnnouncementText != null)
            {
                waveAnnouncementText.gameObject.SetActive(false);
            }
        }

        // Check for boss health bar
        if (bossHealthPanel != null && bossHealthPanel.activeSelf)
        {
            EnemyBoss boss = FindObjectOfType<EnemyBoss>();
            if (boss == null)
            {
                bossHealthPanel.SetActive(false);
            }
        }
    }

    private void UpdateScore(int score)
    {
        if (scoreText != null)
            scoreText.text = "SCORE: " + score.ToString("N0");
    }

    private void UpdateLives(int lives)
    {
        if (livesText != null)
            livesText.text = "LIVES: " + lives;

        // Update life icons
        if (lifeIcons != null)
        {
            for (int i = 0; i < lifeIcons.Length; i++)
            {
                if (lifeIcons[i] != null)
                    lifeIcons[i].enabled = i < lives;
            }
        }
    }

    private void UpdateCombo(int multiplier)
    {
        if (comboText != null)
        {
            if (multiplier > 1)
            {
                comboText.gameObject.SetActive(true);
                comboText.text = "x" + multiplier + " COMBO!";
                comboText.color = multiplier >= 4 ? Color.red :
                                  multiplier >= 3 ? new Color(1f, 0.5f, 0f) :
                                  Color.yellow;
            }
            else
            {
                comboText.gameObject.SetActive(false);
            }
        }
    }

    public void UpdateHealthBar(int current, int max)
    {
        if (healthBar != null)
        {
            healthBar.maxValue = max;
            healthBar.value = current;
        }

        if (healthBarFill != null)
        {
            float percent = (float)current / max;
            if (percent > 0.6f)
                healthBarFill.color = Color.green;
            else if (percent > 0.3f)
                healthBarFill.color = Color.yellow;
            else
                healthBarFill.color = Color.red;
        }
    }

    private void ShowWaveAnnouncement(int wave)
    {
        if (waveText != null)
            waveText.text = "WAVE " + wave;

        if (waveAnnouncementText != null)
        {
            bool isBoss = wave % 5 == 0;
            waveAnnouncementText.text = isBoss ? "!!! BOSS WAVE !!!" : "WAVE " + wave;
            waveAnnouncementText.color = isBoss ? Color.red : Color.white;
            waveAnnouncementText.gameObject.SetActive(true);
            waveAnnouncementTimer = 3f;
        }
    }

    private void ShowBossHealth()
    {
        if (bossHealthPanel != null)
        {
            bossHealthPanel.SetActive(true);
        }

        if (bossNameText != null)
        {
            int bossNum = GameManager.Instance != null ?
                FindObjectOfType<WaveManager>()?.CurrentWave / 5 ?? 1 : 1;
            bossNameText.text = "BOSS " + bossNum;
        }

        // Subscribe to boss health
        EnemyBoss boss = FindObjectOfType<EnemyBoss>();
        if (boss != null && bossHealthBar != null)
        {
            bossHealthBar.maxValue = 1f;
            bossHealthBar.value = 1f;
            boss.OnBossHealthChanged += UpdateBossHealthBar;
        }
    }

    private void UpdateBossHealthBar(float healthPercent)
    {
        if (bossHealthBar != null)
        {
            bossHealthBar.value = healthPercent;
        }
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnScoreChanged -= UpdateScore;
            GameManager.Instance.OnLivesChanged -= UpdateLives;
            GameManager.Instance.OnComboChanged -= UpdateCombo;
        }
    }
}

using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Manages all in-game HUD elements: health bar, score, wave indicator,
/// power-up notifications, and announcements.
/// Attach to a Canvas GameObject in the Game scene.
/// </summary>
public class UIManager : MonoBehaviour
{
    [Header("HUD Elements")]
    [SerializeField] private Text scoreText;
    [SerializeField] private Text highScoreText;
    [SerializeField] private Text waveText;
    [SerializeField] private Slider healthBar;
    [SerializeField] private Image healthBarFill;

    [Header("Announcements")]
    [SerializeField] private Text announcementText;
    [SerializeField] private Text powerUpText;
    [SerializeField] private Text messageText;

    [Header("Colors")]
    [SerializeField] private Color healthHighColor = Color.green;
    [SerializeField] private Color healthMidColor = Color.yellow;
    [SerializeField] private Color healthLowColor = Color.red;

    private Coroutine announcementCoroutine;
    private Coroutine powerUpCoroutine;
    private Coroutine messageCoroutine;

    private void Start()
    {
        // Hide announcement texts
        if (announcementText != null) announcementText.gameObject.SetActive(false);
        if (powerUpText != null) powerUpText.gameObject.SetActive(false);
        if (messageText != null) messageText.gameObject.SetActive(false);

        // Connect to player health
        StartCoroutine(ConnectToPlayerHealth());
    }

    private IEnumerator ConnectToPlayerHealth()
    {
        // Wait a frame for player to initialize
        yield return null;

        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player != null)
        {
            HealthSystem hs = player.GetHealthSystem();
            if (hs != null)
            {
                hs.OnHealthChanged += UpdateHealthBar;
                UpdateHealthBar(hs.CurrentHealth, hs.MaxHealth);
            }
        }
    }

    /// <summary>
    /// Update the score display.
    /// </summary>
    public void UpdateScore(int score)
    {
        if (scoreText != null)
        {
            scoreText.text = "SCORE: " + score.ToString("N0");
        }

        if (highScoreText != null)
        {
            int highScore = GameManager.Instance != null ? GameManager.Instance.HighScore : 0;
            highScoreText.text = "HIGH: " + Mathf.Max(score, highScore).ToString("N0");
        }
    }

    /// <summary>
    /// Update the wave display.
    /// </summary>
    public void UpdateWave(int wave)
    {
        if (waveText != null)
        {
            waveText.text = "WAVE " + wave;
        }
    }

    /// <summary>
    /// Update health bar display.
    /// </summary>
    public void UpdateHealthBar(int currentHealth, int maxHealth)
    {
        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = currentHealth;
        }

        if (healthBarFill != null)
        {
            float ratio = (float)currentHealth / maxHealth;
            if (ratio > 0.6f)
                healthBarFill.color = healthHighColor;
            else if (ratio > 0.3f)
                healthBarFill.color = healthMidColor;
            else
                healthBarFill.color = healthLowColor;
        }
    }

    /// <summary>
    /// Show a wave start announcement.
    /// </summary>
    public void ShowWaveAnnouncement(int wave)
    {
        if (announcementCoroutine != null) StopCoroutine(announcementCoroutine);
        announcementCoroutine = StartCoroutine(ShowAnnouncementRoutine("WAVE " + wave, 2f));
    }

    /// <summary>
    /// Show a power-up collection notification.
    /// </summary>
    public void ShowPowerUpText(string powerUpName)
    {
        if (powerUpCoroutine != null) StopCoroutine(powerUpCoroutine);
        powerUpCoroutine = StartCoroutine(ShowPowerUpRoutine(powerUpName, 1.5f));
    }

    /// <summary>
    /// Show a general message for a duration.
    /// </summary>
    public void ShowMessage(string message, float duration)
    {
        if (messageCoroutine != null) StopCoroutine(messageCoroutine);
        messageCoroutine = StartCoroutine(ShowMessageRoutine(message, duration));
    }

    private IEnumerator ShowAnnouncementRoutine(string text, float duration)
    {
        if (announcementText == null) yield break;

        announcementText.text = text;
        announcementText.gameObject.SetActive(true);

        // Scale-in animation
        float elapsed = 0f;
        float animDuration = 0.3f;
        while (elapsed < animDuration)
        {
            elapsed += Time.deltaTime;
            float scale = Mathf.Lerp(2f, 1f, elapsed / animDuration);
            announcementText.transform.localScale = Vector3.one * scale;
            yield return null;
        }

        yield return new WaitForSeconds(duration);

        // Fade out
        elapsed = 0f;
        Color orig = announcementText.color;
        while (elapsed < 0.5f)
        {
            elapsed += Time.deltaTime;
            Color c = orig;
            c.a = Mathf.Lerp(1f, 0f, elapsed / 0.5f);
            announcementText.color = c;
            yield return null;
        }

        announcementText.color = orig;
        announcementText.gameObject.SetActive(false);
    }

    private IEnumerator ShowPowerUpRoutine(string powerUpName, float duration)
    {
        if (powerUpText == null) yield break;

        powerUpText.text = "+ " + powerUpName.ToUpper();
        powerUpText.gameObject.SetActive(true);

        yield return new WaitForSeconds(duration);

        powerUpText.gameObject.SetActive(false);
    }

    private IEnumerator ShowMessageRoutine(string message, float duration)
    {
        if (messageText == null) yield break;

        messageText.text = message;
        messageText.gameObject.SetActive(true);

        yield return new WaitForSeconds(duration);

        messageText.gameObject.SetActive(false);
    }
}

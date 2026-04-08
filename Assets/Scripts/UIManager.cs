using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Manages all in-game UI: HUD, score, health bar, wave text, and pause menu.
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("HUD Elements")]
    public Text scoreText;
    public Text highScoreText;
    public Text waveText;
    public Image healthBarFill;
    public Text healthText;

    [Header("Wave Announcement")]
    public Text waveAnnouncementText;
    public float waveAnnouncementDuration = 2f;

    [Header("Pause Menu")]
    public GameObject pauseMenuPanel;

    [Header("Power-Up Indicators")]
    public Text powerUpStatusText;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);

        if (waveAnnouncementText != null)
            waveAnnouncementText.gameObject.SetActive(false);

        UpdateScore(0);

        if (highScoreText != null && GameManager.Instance != null)
        {
            highScoreText.text = "HIGH: " + GameManager.Instance.highScore.ToString();
        }
    }

    /// <summary>
    /// Updates the score display.
    /// </summary>
    public void UpdateScore(int score)
    {
        if (scoreText != null)
        {
            scoreText.text = "SCORE: " + score.ToString();
        }
    }

    /// <summary>
    /// Updates the health bar and text.
    /// </summary>
    public void UpdateHealth(int current, int max)
    {
        if (healthBarFill != null)
        {
            float fillAmount = (float)current / max;
            healthBarFill.fillAmount = fillAmount;

            // Color gradient: green -> yellow -> red
            if (fillAmount > 0.6f)
                healthBarFill.color = Color.green;
            else if (fillAmount > 0.3f)
                healthBarFill.color = Color.yellow;
            else
                healthBarFill.color = Color.red;
        }

        if (healthText != null)
        {
            healthText.text = current + " / " + max;
        }
    }

    /// <summary>
    /// Shows a wave start announcement.
    /// </summary>
    public void ShowWaveText(int waveNumber)
    {
        if (waveText != null)
        {
            waveText.text = "WAVE " + waveNumber.ToString();
        }

        if (waveAnnouncementText != null)
        {
            waveAnnouncementText.text = "WAVE " + waveNumber;
            StartCoroutine(ShowAnnouncementCoroutine());
        }
    }

    /// <summary>
    /// Shows a wave complete message with bonus points.
    /// </summary>
    public void ShowWaveCompleteText(int waveNumber, int bonus)
    {
        if (waveAnnouncementText != null)
        {
            waveAnnouncementText.text = "WAVE " + waveNumber + " COMPLETE!\n+" + bonus + " BONUS";
            StartCoroutine(ShowAnnouncementCoroutine());
        }
    }

    /// <summary>
    /// Coroutine to show and hide the announcement text.
    /// </summary>
    private IEnumerator ShowAnnouncementCoroutine()
    {
        if (waveAnnouncementText == null) yield break;

        waveAnnouncementText.gameObject.SetActive(true);

        // Scale-in animation
        float elapsed = 0f;
        float animDuration = 0.3f;
        while (elapsed < animDuration)
        {
            elapsed += Time.deltaTime;
            float scale = Mathf.Lerp(0f, 1f, elapsed / animDuration);
            waveAnnouncementText.transform.localScale = Vector3.one * scale;
            yield return null;
        }
        waveAnnouncementText.transform.localScale = Vector3.one;

        yield return new WaitForSeconds(waveAnnouncementDuration);

        // Fade out
        elapsed = 0f;
        Color originalColor = waveAnnouncementText.color;
        while (elapsed < 0.5f)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / 0.5f);
            waveAnnouncementText.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            yield return null;
        }

        waveAnnouncementText.gameObject.SetActive(false);
        waveAnnouncementText.color = originalColor;
    }

    /// <summary>
    /// Shows or hides the pause menu.
    /// </summary>
    public void ShowPauseMenu(bool show)
    {
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(show);
        }
    }

    /// <summary>
    /// Resume button callback.
    /// </summary>
    public void OnResumeClicked()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.TogglePause();
        }
    }

    /// <summary>
    /// Quit to menu button callback.
    /// </summary>
    public void OnQuitToMenuClicked()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ReturnToMenu();
        }
    }
}

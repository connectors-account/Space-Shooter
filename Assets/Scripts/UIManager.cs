// =============================================================================
// UIManager.cs
// Manages all in-game UI elements: HUD (score, health, wave), pause menu,
// and wave announcements. This is a singleton present in the GamePlay scene.
// Create a Canvas with child elements and attach this to the Canvas or an
// empty "UIManager" GameObject.
// =============================================================================
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIManager : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Singleton
    // -------------------------------------------------------------------------
    public static UIManager Instance { get; private set; }

    // -------------------------------------------------------------------------
    // HUD References
    // -------------------------------------------------------------------------
    [Header("HUD Elements")]
    [Tooltip("Text element displaying the current score.")]
    public Text scoreText;

    [Tooltip("Text element displaying the current wave number.")]
    public Text waveText;

    [Tooltip("Text element displaying the player's health.")]
    public Text healthText;

    [Tooltip("Image used as a health bar fill (Image type = Filled).")]
    public Image healthBarFill;

    // -------------------------------------------------------------------------
    // Wave Announcement
    // -------------------------------------------------------------------------
    [Header("Wave Announcement")]
    [Tooltip("Large text shown briefly when a new wave starts.")]
    public Text waveAnnouncementText;

    [Tooltip("How long the wave announcement stays visible.")]
    public float announcementDuration = 2f;

    // -------------------------------------------------------------------------
    // Pause Menu
    // -------------------------------------------------------------------------
    [Header("Pause Menu")]
    [Tooltip("The pause menu panel (toggled on/off).")]
    public GameObject pauseMenuPanel;

    // -------------------------------------------------------------------------
    // Unity Lifecycle
    // -------------------------------------------------------------------------

    /// <summary>
    /// Enforce singleton pattern (non-persistent — lives only in GamePlay scene).
    /// </summary>
    void Awake()
    {
        // UIManager is scene-local; if one already exists, destroy the duplicate
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    /// <summary>
    /// Initialize UI elements to default values.
    /// </summary>
    void Start()
    {
        // Hide wave announcement at start
        if (waveAnnouncementText != null)
        {
            waveAnnouncementText.gameObject.SetActive(false);
        }

        // Hide pause menu at start
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }

        // Set initial display values
        UpdateScore(0);
        UpdateWave(0);
    }

    /// <summary>
    /// Clean up singleton reference when this object is destroyed.
    /// </summary>
    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    // -------------------------------------------------------------------------
    // HUD Updates
    // -------------------------------------------------------------------------

    /// <summary>
    /// Updates the score display text.
    /// </summary>
    /// <param name="score">Current score value.</param>
    public void UpdateScore(int score)
    {
        if (scoreText != null)
        {
            scoreText.text = "SCORE: " + score.ToString("N0");
        }
    }

    /// <summary>
    /// Updates the wave number display text.
    /// </summary>
    /// <param name="wave">Current wave number.</param>
    public void UpdateWave(int wave)
    {
        if (waveText != null)
        {
            waveText.text = "WAVE: " + wave.ToString();
        }
    }

    /// <summary>
    /// Updates the health display (both text and health bar fill).
    /// </summary>
    /// <param name="currentHealth">Player's current health.</param>
    /// <param name="maxHealth">Player's maximum health.</param>
    public void UpdateHealth(int currentHealth, int maxHealth)
    {
        if (healthText != null)
        {
            healthText.text = "HP: " + currentHealth + " / " + maxHealth;
        }

        if (healthBarFill != null)
        {
            float fillAmount = (float)currentHealth / (float)maxHealth;
            healthBarFill.fillAmount = fillAmount;

            // Change color based on health percentage
            if (fillAmount > 0.6f)
                healthBarFill.color = Color.green;
            else if (fillAmount > 0.3f)
                healthBarFill.color = Color.yellow;
            else
                healthBarFill.color = Color.red;
        }
    }

    // -------------------------------------------------------------------------
    // Wave Announcement
    // -------------------------------------------------------------------------

    /// <summary>
    /// Shows a large "WAVE X" announcement that fades out after a duration.
    /// </summary>
    /// <param name="waveNumber">The wave number to announce.</param>
    public void ShowWaveAnnouncement(int waveNumber)
    {
        if (waveAnnouncementText != null)
        {
            StartCoroutine(WaveAnnouncementCoroutine(waveNumber));
        }
    }

    /// <summary>
    /// Coroutine that displays the wave announcement and hides it after a delay.
    /// </summary>
    private IEnumerator WaveAnnouncementCoroutine(int waveNumber)
    {
        waveAnnouncementText.text = "WAVE " + waveNumber;
        waveAnnouncementText.gameObject.SetActive(true);

        // Fade effect: start fully visible, fade out
        Color startColor = waveAnnouncementText.color;
        startColor.a = 1f;
        waveAnnouncementText.color = startColor;

        float elapsed = 0f;
        while (elapsed < announcementDuration)
        {
            elapsed += Time.deltaTime;

            // Fade out during the last half of the duration
            if (elapsed > announcementDuration * 0.5f)
            {
                float fadeProgress = (elapsed - announcementDuration * 0.5f) / (announcementDuration * 0.5f);
                Color c = waveAnnouncementText.color;
                c.a = 1f - fadeProgress;
                waveAnnouncementText.color = c;
            }

            yield return null;
        }

        waveAnnouncementText.gameObject.SetActive(false);
    }

    // -------------------------------------------------------------------------
    // Pause Menu
    // -------------------------------------------------------------------------

    /// <summary>
    /// Shows or hides the pause menu panel.
    /// </summary>
    /// <param name="show">True to show, false to hide.</param>
    public void ShowPauseMenu(bool show)
    {
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(show);
        }
    }

    // -------------------------------------------------------------------------
    // Button Callbacks (wire these up in the Unity Inspector)
    // -------------------------------------------------------------------------

    /// <summary>
    /// Called when the Resume button is pressed in the pause menu.
    /// </summary>
    public void OnResumeButton()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResumeGame();
        }
    }

    /// <summary>
    /// Called when the Main Menu button is pressed in the pause menu.
    /// </summary>
    public void OnMainMenuButton()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.GoToMainMenu();
        }
    }

    /// <summary>
    /// Called when the Quit button is pressed.
    /// </summary>
    public void OnQuitButton()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.QuitGame();
        }
    }
}

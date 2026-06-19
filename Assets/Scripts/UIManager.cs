using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages the in-game HUD: score text, health bar/text, transient banners
/// (wave / power-up) and the game-over panel. Singleton for easy access from
/// gameplay scripts.
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("HUD")]
    public Text scoreText;
    public Text healthText;
    [Tooltip("Optional fill image (Image type = Filled) for a health bar.")]
    public Image healthBarFill;

    [Header("Banners")]
    [Tooltip("Text used for temporary wave / power-up messages.")]
    public Text bannerText;
    [Tooltip("How long banners stay on screen.")]
    public float bannerDuration = 1.5f;

    [Header("Game Over")]
    public GameObject gameOverPanel;
    public Text finalScoreText;

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

    private void Start()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (bannerText != null) bannerText.gameObject.SetActive(false);
    }

    public void UpdateScore(int score)
    {
        if (scoreText != null) scoreText.text = "Score: " + score;
    }

    public void UpdateHealth(int current, int max)
    {
        if (healthText != null) healthText.text = "HP: " + current;
        if (healthBarFill != null)
            healthBarFill.fillAmount = max > 0 ? (float)current / max : 0f;
    }

    /// <summary>Show a short-lived banner announcing a new wave.</summary>
    public void ShowWaveBanner(int wave)
    {
        ShowBanner("Wave " + wave);
    }

    /// <summary>Show a short-lived banner when a power-up is collected.</summary>
    public void ShowPowerUpBanner(string powerUpName)
    {
        ShowBanner(powerUpName + "!");
    }

    private void ShowBanner(string message)
    {
        if (bannerText == null) return;

        if (bannerRoutine != null) StopCoroutine(bannerRoutine);
        bannerRoutine = StartCoroutine(BannerRoutine(message));
    }

    private IEnumerator BannerRoutine(string message)
    {
        bannerText.text = message;
        bannerText.gameObject.SetActive(true);
        // Use unscaled time so banners still fade if timeScale changes.
        yield return new WaitForSecondsRealtime(bannerDuration);
        bannerText.gameObject.SetActive(false);
    }

    public void ShowGameOver(int finalScore)
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        if (finalScoreText != null) finalScoreText.text = "Final Score: " + finalScore;
    }

    public void HideGameOver()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
    }

    // --- Button hooks (wire these in the Inspector) ---

    /// <summary>Restart button on the game-over panel.</summary>
    public void OnRestartButton()
    {
        GameManager.Instance?.RestartGame();
    }

    /// <summary>Main-menu button on the game-over panel.</summary>
    public void OnMainMenuButton()
    {
        GameManager.Instance?.GoToMainMenu();
    }
}

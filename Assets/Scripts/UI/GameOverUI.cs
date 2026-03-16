using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// GameOverUI handles the game over screen interactions and display.
/// </summary>
public class GameOverUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI gameOverText;
    [SerializeField] private TextMeshProUGUI finalScoreText;
    [SerializeField] private TextMeshProUGUI highScoreText;
    [SerializeField] private TextMeshProUGUI waveReachedText;
    [SerializeField] private TextMeshProUGUI newHighScoreText;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button mainMenuButton;

    [Header("Animation")]
    [SerializeField] private float fadeInDuration = 0.5f;
    [SerializeField] private float scoreCountUpSpeed = 50f;

    private CanvasGroup canvasGroup;
    private int displayedScore;
    private int targetScore;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    private void OnEnable()
    {
        // Start fade in animation
        StartCoroutine(FadeIn());

        // Setup display
        SetupGameOverDisplay();

        // Focus on restart button
        if (restartButton != null)
        {
            restartButton.Select();
        }
    }

    private void Start()
    {
        // Setup button listeners
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(OnRestartClicked);
        }

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(OnMainMenuClicked);
        }
    }

    private void Update()
    {
        // Animate score count up
        if (displayedScore < targetScore)
        {
            displayedScore = Mathf.Min(
                displayedScore + Mathf.CeilToInt(scoreCountUpSpeed * Time.unscaledDeltaTime * 100),
                targetScore
            );
            UpdateScoreText(displayedScore);
        }
    }

    /// <summary>
    /// Setup the game over display with final stats
    /// </summary>
    private void SetupGameOverDisplay()
    {
        displayedScore = 0;

        if (ScoreManager.Instance != null)
        {
            targetScore = ScoreManager.Instance.GetFinalScore();
            
            // High score
            if (highScoreText != null)
            {
                highScoreText.text = $"HIGH SCORE: {ScoreManager.Instance.HighScore:N0}";
            }

            // New high score notification
            if (newHighScoreText != null)
            {
                newHighScoreText.gameObject.SetActive(ScoreManager.Instance.IsNewHighScore());
            }
        }

        // Wave reached
        if (WaveManager.Instance != null && waveReachedText != null)
        {
            waveReachedText.text = $"WAVE REACHED: {WaveManager.Instance.CurrentWave}";
        }

        // Play game over sound
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySound("GameOver");
            SoundManager.Instance.StopMusic();
        }
    }

    /// <summary>
    /// Update score text with current displayed value
    /// </summary>
    private void UpdateScoreText(int score)
    {
        if (finalScoreText != null)
        {
            finalScoreText.text = $"SCORE: {score:N0}";
        }
    }

    /// <summary>
    /// Fade in the game over panel
    /// </summary>
    private System.Collections.IEnumerator FadeIn()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            float elapsed = 0f;

            while (elapsed < fadeInDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeInDuration);
                yield return null;
            }

            canvasGroup.alpha = 1f;
        }
    }

    /// <summary>
    /// Called when Restart button is clicked
    /// </summary>
    private void OnRestartClicked()
    {
        PlayButtonSound();
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RestartGame();
        }
    }

    /// <summary>
    /// Called when Main Menu button is clicked
    /// </summary>
    private void OnMainMenuClicked()
    {
        PlayButtonSound();
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ReturnToMainMenu();
        }
    }

    /// <summary>
    /// Play button click sound
    /// </summary>
    private void PlayButtonSound()
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySound("ButtonClick");
        }
    }
}

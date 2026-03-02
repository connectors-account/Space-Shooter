using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages all UI elements including score, health, wave display, and game over screen
/// </summary>
public class UIManager : MonoBehaviour
{
    [Header("HUD Elements")]
    [SerializeField] private Text scoreText;
    [SerializeField] private Text highScoreText;
    [SerializeField] private Text healthText;
    [SerializeField] private Text waveText;

    [Header("Health Display (Alternative - Image based)")]
    [SerializeField] private Image[] healthIcons;
    [SerializeField] private Sprite fullHeartSprite;
    [SerializeField] private Sprite emptyHeartSprite;

    [Header("Game Over Panel")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private Text finalScoreText;
    [SerializeField] private Text gameOverHighScoreText;
    [SerializeField] private Text restartInstructionText;

    [Header("Pause Menu")]
    [SerializeField] private GameObject pauseMenuPanel;

    [Header("Wave Notification")]
    [SerializeField] private GameObject waveNotificationPanel;
    [SerializeField] private Text waveNotificationText;
    [SerializeField] private float waveNotificationDuration = 2f;

    private void Start()
    {
        // Hide panels at start
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
        
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);
        
        if (waveNotificationPanel != null)
            waveNotificationPanel.SetActive(false);

        // Initialize with default values if no GameManager yet
        UpdateScore(0, PlayerPrefs.GetInt("HighScore", 0));
        UpdateHealth(3, 5);
        UpdateWave(1);
    }

    public void UpdateScore(int currentScore, int highScore)
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {currentScore:N0}";
        }

        if (highScoreText != null)
        {
            highScoreText.text = $"High Score: {highScore:N0}";
        }
    }

    public void UpdateHealth(int currentHealth, int maxHealth)
    {
        // Update text-based health display
        if (healthText != null)
        {
            healthText.text = $"Health: {currentHealth}/{maxHealth}";
        }

        // Update icon-based health display
        if (healthIcons != null && healthIcons.Length > 0)
        {
            for (int i = 0; i < healthIcons.Length; i++)
            {
                if (healthIcons[i] != null)
                {
                    if (i < currentHealth)
                    {
                        healthIcons[i].sprite = fullHeartSprite;
                        healthIcons[i].color = Color.red;
                    }
                    else
                    {
                        healthIcons[i].sprite = emptyHeartSprite;
                        healthIcons[i].color = Color.gray;
                    }
                }
            }
        }
    }

    public void UpdateWave(int waveNumber)
    {
        if (waveText != null)
        {
            waveText.text = $"Wave: {waveNumber}";
        }
    }

    public void ShowWaveNotification(int waveNumber)
    {
        if (waveNotificationPanel != null && waveNotificationText != null)
        {
            waveNotificationText.text = $"WAVE {waveNumber}";
            waveNotificationPanel.SetActive(true);
            StartCoroutine(HideWaveNotification());
        }
    }

    private System.Collections.IEnumerator HideWaveNotification()
    {
        yield return new WaitForSeconds(waveNotificationDuration);
        if (waveNotificationPanel != null)
        {
            waveNotificationPanel.SetActive(false);
        }
    }

    public void ShowGameOver(int finalScore, int highScore)
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        if (finalScoreText != null)
        {
            finalScoreText.text = $"Final Score: {finalScore:N0}";
        }

        if (gameOverHighScoreText != null)
        {
            gameOverHighScoreText.text = $"High Score: {highScore:N0}";
            
            // Highlight if new high score
            if (finalScore >= highScore)
            {
                gameOverHighScoreText.text = $"NEW HIGH SCORE: {highScore:N0}!";
                gameOverHighScoreText.color = Color.yellow;
            }
        }

        if (restartInstructionText != null)
        {
            restartInstructionText.text = "Press 'R' to Restart";
        }
    }

    public void HideGameOver()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
    }

    public void ShowPauseMenu(bool show)
    {
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(show);
        }
    }

    // Button callbacks (can be connected in Unity editor)
    public void OnRestartButtonClicked()
    {
        GameManager gm = FindObjectOfType<GameManager>();
        if (gm != null)
        {
            gm.RestartGame();
        }
    }

    public void OnResumeButtonClicked()
    {
        GameManager gm = FindObjectOfType<GameManager>();
        if (gm != null)
        {
            gm.TogglePause();
        }
    }

    public void OnQuitButtonClicked()
    {
        GameManager gm = FindObjectOfType<GameManager>();
        if (gm != null)
        {
            gm.QuitGame();
        }
    }

    // Utility method to create UI elements programmatically if needed
    public void CreateDefaultUI()
    {
        // This method can be called to set up basic UI if not configured in editor
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }

        // Create score text if not assigned
        if (scoreText == null)
        {
            scoreText = CreateTextElement(canvas.transform, "ScoreText", "Score: 0", 
                new Vector2(10, -10), TextAnchor.UpperLeft);
        }

        // Create health text if not assigned
        if (healthText == null)
        {
            healthText = CreateTextElement(canvas.transform, "HealthText", "Health: 3/5", 
                new Vector2(-10, -10), TextAnchor.UpperRight);
        }

        // Create wave text if not assigned
        if (waveText == null)
        {
            waveText = CreateTextElement(canvas.transform, "WaveText", "Wave: 1", 
                new Vector2(0, -10), TextAnchor.UpperCenter);
        }
    }

    private Text CreateTextElement(Transform parent, string name, string content, 
        Vector2 position, TextAnchor anchor)
    {
        GameObject textObj = new GameObject(name);
        textObj.transform.SetParent(parent, false);

        RectTransform rectTransform = textObj.AddComponent<RectTransform>();
        rectTransform.anchoredPosition = position;
        rectTransform.sizeDelta = new Vector2(200, 50);

        // Set anchors based on alignment
        switch (anchor)
        {
            case TextAnchor.UpperLeft:
                rectTransform.anchorMin = new Vector2(0, 1);
                rectTransform.anchorMax = new Vector2(0, 1);
                rectTransform.pivot = new Vector2(0, 1);
                break;
            case TextAnchor.UpperRight:
                rectTransform.anchorMin = new Vector2(1, 1);
                rectTransform.anchorMax = new Vector2(1, 1);
                rectTransform.pivot = new Vector2(1, 1);
                break;
            case TextAnchor.UpperCenter:
                rectTransform.anchorMin = new Vector2(0.5f, 1);
                rectTransform.anchorMax = new Vector2(0.5f, 1);
                rectTransform.pivot = new Vector2(0.5f, 1);
                break;
        }

        Text text = textObj.AddComponent<Text>();
        text.text = content;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 24;
        text.color = Color.white;
        text.alignment = anchor;

        return text;
    }
}

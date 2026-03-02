using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// UIManager controls all UI elements in the game.
/// Handles menus, HUD, and transitions between UI states.
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("UI Panels")]
    [Tooltip("Main menu panel")]
    public GameObject mainMenuPanel;
    
    [Tooltip("Game HUD panel (score, health, etc.)")]
    public GameObject gameUIPanel;
    
    [Tooltip("Pause menu panel")]
    public GameObject pauseMenuPanel;
    
    [Tooltip("Game over panel")]
    public GameObject gameOverPanel;

    [Header("HUD Elements")]
    [Tooltip("Score text display")]
    public Text scoreText;
    
    [Tooltip("High score text display")]
    public Text highScoreText;
    
    [Tooltip("Health bar slider")]
    public Slider healthBar;
    
    [Tooltip("Health text display")]
    public Text healthText;
    
    [Tooltip("Wave number text")]
    public Text waveText;

    [Header("Game Over Elements")]
    [Tooltip("Final score text on game over screen")]
    public Text finalScoreText;
    
    [Tooltip("High score text on game over screen")]
    public Text gameOverHighScoreText;

    [Header("Wave Announcement")]
    [Tooltip("Wave announcement text (appears briefly)")]
    public Text waveAnnouncementText;
    
    [Tooltip("Duration to show wave announcement")]
    public float waveAnnouncementDuration = 2f;

    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Subscribe to score changes
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnScoreChanged.AddListener(UpdateScoreUI);
            ScoreManager.Instance.OnHighScoreChanged.AddListener(UpdateHighScoreUI);
        }
        
        // Initialize wave announcement as hidden
        if (waveAnnouncementText != null)
        {
            waveAnnouncementText.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Show main menu, hide other panels
    /// </summary>
    public void ShowMainMenu()
    {
        SetPanelActive(mainMenuPanel, true);
        SetPanelActive(gameUIPanel, false);
        SetPanelActive(pauseMenuPanel, false);
        SetPanelActive(gameOverPanel, false);
    }

    /// <summary>
    /// Show game UI (HUD), hide menus
    /// </summary>
    public void ShowGameUI()
    {
        SetPanelActive(mainMenuPanel, false);
        SetPanelActive(gameUIPanel, true);
        SetPanelActive(pauseMenuPanel, false);
        SetPanelActive(gameOverPanel, false);
    }

    /// <summary>
    /// Show pause menu overlay
    /// </summary>
    public void ShowPauseMenu()
    {
        SetPanelActive(pauseMenuPanel, true);
    }

    /// <summary>
    /// Hide pause menu
    /// </summary>
    public void HidePauseMenu()
    {
        SetPanelActive(pauseMenuPanel, false);
    }

    /// <summary>
    /// Show game over screen
    /// </summary>
    public void ShowGameOver()
    {
        SetPanelActive(gameUIPanel, false);
        SetPanelActive(gameOverPanel, true);
        
        // Update final scores
        if (ScoreManager.Instance != null)
        {
            if (finalScoreText != null)
            {
                finalScoreText.text = "Score: " + ScoreManager.Instance.GetScore().ToString();
            }
            if (gameOverHighScoreText != null)
            {
                gameOverHighScoreText.text = "High Score: " + ScoreManager.Instance.GetHighScore().ToString();
            }
        }
    }

    /// <summary>
    /// Helper to safely set panel active state
    /// </summary>
    void SetPanelActive(GameObject panel, bool active)
    {
        if (panel != null)
        {
            panel.SetActive(active);
        }
    }

    /// <summary>
    /// Update score display
    /// </summary>
    public void UpdateScoreUI(int score)
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + score.ToString();
        }
    }

    /// <summary>
    /// Update high score display
    /// </summary>
    public void UpdateHighScoreUI(int highScore)
    {
        if (highScoreText != null)
        {
            highScoreText.text = "High Score: " + highScore.ToString();
        }
    }

    /// <summary>
    /// Update health bar and text
    /// </summary>
    public void UpdateHealthUI(int currentHealth, int maxHealth)
    {
        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = currentHealth;
        }
        
        if (healthText != null)
        {
            healthText.text = currentHealth.ToString() + " / " + maxHealth.ToString();
        }
    }

    /// <summary>
    /// Show wave announcement
    /// </summary>
    public void ShowWaveText(int waveNumber)
    {
        // Update wave counter in HUD
        if (waveText != null)
        {
            waveText.text = "Wave: " + waveNumber.ToString();
        }
        
        // Show wave announcement
        if (waveAnnouncementText != null)
        {
            StartCoroutine(ShowWaveAnnouncement(waveNumber));
        }
    }

    /// <summary>
    /// Coroutine to show and hide wave announcement
    /// </summary>
    IEnumerator ShowWaveAnnouncement(int waveNumber)
    {
        waveAnnouncementText.text = "WAVE " + waveNumber.ToString();
        waveAnnouncementText.gameObject.SetActive(true);
        
        yield return new WaitForSeconds(waveAnnouncementDuration);
        
        waveAnnouncementText.gameObject.SetActive(false);
    }

    // Button callbacks for UI buttons

    /// <summary>
    /// Called when Play button is pressed
    /// </summary>
    public void OnPlayButtonPressed()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartGame();
        }
    }

    /// <summary>
    /// Called when Resume button is pressed
    /// </summary>
    public void OnResumeButtonPressed()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResumeGame();
        }
    }

    /// <summary>
    /// Called when Restart button is pressed
    /// </summary>
    public void OnRestartButtonPressed()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RestartGame();
        }
    }

    /// <summary>
    /// Called when Main Menu button is pressed
    /// </summary>
    public void OnMainMenuButtonPressed()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.GoToMainMenu();
        }
    }

    /// <summary>
    /// Called when Quit button is pressed
    /// </summary>
    public void OnQuitButtonPressed()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.QuitGame();
        }
    }
}

using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// HUDManager – Controls all in-game UI: score, health, and Game Over panel.
/// Attach to the Canvas GameObject in the GamePlay scene.
/// </summary>
public class HUDManager : MonoBehaviour
{
    // ── Singleton ──
    public static HUDManager Instance { get; private set; }

    [Header("HUD Elements")]
    public Text scoreText;
    public Text healthText;

    [Header("Game Over Panel")]
    public GameObject gameOverPanel;
    public Text finalScoreText;
    public Button restartButton;
    public Button menuButton;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        // Ensure game-over panel is hidden at start
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        // Wire buttons
        if (restartButton != null)
            restartButton.onClick.AddListener(OnRestartClicked);
        if (menuButton != null)
            menuButton.onClick.AddListener(OnMenuClicked);

        // Initial display
        UpdateScore(0);
        UpdateHealth(3);
    }

    // ── Public API ──

    public void UpdateScore(int score)
    {
        if (scoreText != null)
            scoreText.text = "SCORE: " + score;
    }

    public void UpdateHealth(int health)
    {
        if (healthText != null)
        {
            // Show hearts for visual flair
            string hearts = "";
            for (int i = 0; i < health; i++) hearts += "\u2665 "; // ♥
            healthText.text = "LIVES: " + hearts;
        }
    }

    public void ShowGameOver(int finalScore)
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
        if (finalScoreText != null)
            finalScoreText.text = "FINAL SCORE: " + finalScore;
    }

    // ── Button Handlers ──

    void OnRestartClicked()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.RestartGame();
    }

    void OnMenuClicked()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.GoToMainMenu();
    }
}

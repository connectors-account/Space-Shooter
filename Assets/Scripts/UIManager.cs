using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages all on-screen UI: the score label, the health bar/label, and the
/// game-over panel. References are assigned in the inspector. The class works
/// with either legacy UI Text or TextMeshPro (see notes in the README); this
/// implementation uses the built-in UnityEngine.UI.Text for zero extra setup.
/// </summary>
public class UIManager : MonoBehaviour
{
    [Header("HUD")]
    [Tooltip("Text element that shows the current score.")]
    [SerializeField] private Text scoreText;

    [Tooltip("Text element that shows the current health value.")]
    [SerializeField] private Text healthText;

    [Tooltip("Optional Image used as a health bar fill (Image type = Filled).")]
    [SerializeField] private Image healthBarFill;

    [Header("Game Over")]
    [Tooltip("Root panel shown when the player loses.")]
    [SerializeField] private GameObject gameOverPanel;

    [Tooltip("Text element on the game-over panel for the final score.")]
    [SerializeField] private Text finalScoreText;

    [Tooltip("Optional restart button. If wired, it calls GameManager.RestartGame.")]
    [SerializeField] private Button restartButton;

    private void Start()
    {
        // Hide the game-over screen at launch.
        HideGameOver();

        // Hook up the restart button if present.
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(() =>
            {
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.RestartGame();
                }
            });
        }
    }

    /// <summary>Refresh the score label.</summary>
    public void UpdateScore(int score)
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + score;
        }
    }

    /// <summary>Refresh the health label and optional health bar.</summary>
    public void UpdateHealth(int current, int max)
    {
        if (healthText != null)
        {
            healthText.text = "Health: " + current;
        }

        if (healthBarFill != null && max > 0)
        {
            healthBarFill.fillAmount = Mathf.Clamp01((float)current / max);
        }
    }

    /// <summary>Show the game-over panel with the final score.</summary>
    public void ShowGameOver(int finalScore)
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        if (finalScoreText != null)
        {
            finalScoreText.text = "Final Score: " + finalScore + "\nPress R to Restart";
        }
    }

    /// <summary>Hide the game-over panel.</summary>
    public void HideGameOver()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
    }
}

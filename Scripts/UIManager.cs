using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages gameplay and menu UI updates.
/// Attach this to a UIManager GameObject in the Canvas.
/// </summary>
public class UIManager : MonoBehaviour
{
    [Header("HUD")]
    [SerializeField] private GameObject hudPanel;
    [SerializeField] private Text scoreText;
    [SerializeField] private Text healthText;
    [SerializeField] private Text waveText;

    [Header("Menus")]
    [SerializeField] private GameObject startMenuPanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private Text finalScoreText;

    public void UpdateScore(int score)
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {score}";
        }
    }

    public void UpdateHealth(int currentHealth, int maxHealth)
    {
        if (healthText != null)
        {
            healthText.text = $"Health: {currentHealth}/{maxHealth}";
        }
    }

    public void UpdateWave(int wave)
    {
        if (waveText != null)
        {
            waveText.text = $"Wave: {wave}";
        }
    }

    public void ShowHUD(bool show)
    {
        if (hudPanel != null)
        {
            hudPanel.SetActive(show);
        }
    }

    public void ShowStartMenu(bool show)
    {
        if (startMenuPanel != null)
        {
            startMenuPanel.SetActive(show);
        }
    }

    public void ShowGameOver(bool show, int finalScore)
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(show);
        }

        if (show && finalScoreText != null)
        {
            finalScoreText.text = $"Final Score: {finalScore}";
        }
    }
}

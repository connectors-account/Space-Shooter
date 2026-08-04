using UnityEngine;
using TMPro;

/// <summary>
/// Wires GameManager events to the in-game HUD.
/// Attach to a Canvas GameObject in the Game scene.
/// Requires TextMeshPro (install via Package Manager → "TextMeshPro").
/// </summary>
public class UIManager : MonoBehaviour
{
    // ── Inspector ──────────────────────────────────────────────────────────────
    [Header("HUD Elements")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI healthText;

    [Header("Game-Over Overlay (child panel, starts disabled)")]
    public GameObject gameOverPanel;

    // ── Unity ──────────────────────────────────────────────────────────────────
    void OnEnable()
    {
        if (GameManager.Instance == null) return;
        GameManager.Instance.OnScoreChanged  += RefreshScore;
        GameManager.Instance.OnHealthChanged += RefreshHealth;
        GameManager.Instance.OnGameOver      += ShowGameOver;
    }

    void OnDisable()
    {
        if (GameManager.Instance == null) return;
        GameManager.Instance.OnScoreChanged  -= RefreshScore;
        GameManager.Instance.OnHealthChanged -= RefreshHealth;
        GameManager.Instance.OnGameOver      -= ShowGameOver;
    }

    void Start()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(false);

        if (GameManager.Instance != null)
        {
            RefreshScore(GameManager.Instance.Score);
            RefreshHealth(GameManager.Instance.CurrentHealth);
        }
    }

    // ── Callbacks ──────────────────────────────────────────────────────────────
    void RefreshScore(int score)
    {
        if (scoreText != null)
            scoreText.text = "SCORE  " + score.ToString("D7");
    }

    void RefreshHealth(int health)
    {
        if (healthText == null) return;

        // Draw filled / empty hearts based on current vs max health
        int max  = GameManager.Instance != null ? GameManager.Instance.MaxHealth : 3;
        string s = "";
        for (int i = 0; i < max; i++)
            s += i < health ? "♥ " : "♡ ";

        healthText.text = s.TrimEnd();
    }

    void ShowGameOver()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
    }
}

using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Standalone Game Over screen (if using a separate scene instead of overlay panel).
/// </summary>
public class GameOverUI : MonoBehaviour
{
    [Header("UI Elements")]
    public Text gameOverTitle;
    public Text finalScoreText;
    public Text highScoreText;
    public Button restartButton;
    public Button mainMenuButton;

    private void Start()
    {
        if (gameOverTitle != null)
            gameOverTitle.text = "GAME OVER";

        if (finalScoreText != null && GameManager.Instance != null)
            finalScoreText.text = $"SCORE: {GameManager.Instance.Score:N0}";

        if (highScoreText != null && GameManager.Instance != null)
            highScoreText.text = $"HIGH SCORE: {GameManager.Instance.HighScore:N0}";

        if (restartButton != null)
            restartButton.onClick.AddListener(() => GameManager.Instance?.RestartGame());

        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(() => GameManager.Instance?.GoToMainMenu());
    }
}

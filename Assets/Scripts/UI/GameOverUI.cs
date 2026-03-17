using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Game Over screen showing final score and restart/menu options.
/// Attach to a panel inside the Game scene Canvas.
/// </summary>
public class GameOverUI : MonoBehaviour
{
    public static GameOverUI Instance { get; private set; }

    [Header("UI References")]
    public Text gameOverText;
    public Text finalScoreText;
    public Text finalWaveText;
    public Button restartButton;
    public Button mainMenuButton;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        gameObject.SetActive(false);
    }

    public void Show(int score, int wave)
    {
        gameObject.SetActive(true);

        if (gameOverText != null)
            gameOverText.text = "GAME OVER";

        if (finalScoreText != null)
            finalScoreText.text = $"Final Score: {score}";

        if (finalWaveText != null)
            finalWaveText.text = $"Wave Reached: {wave}";

        if (restartButton != null)
        {
            restartButton.onClick.RemoveAllListeners();
            restartButton.onClick.AddListener(OnRestartClicked);
        }

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.RemoveAllListeners();
            mainMenuButton.onClick.AddListener(OnMainMenuClicked);
        }
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    void OnRestartClicked()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("GameScene");
    }

    void OnMainMenuClicked()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}

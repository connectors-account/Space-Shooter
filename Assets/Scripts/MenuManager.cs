using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject victoryPanel;

    [Header("Labels")]
    [SerializeField] private Text gameOverScoreText;
    [SerializeField] private Text gameOverWaveText;
    [SerializeField] private Text victoryScoreText;

    [Header("Scene Names")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private string gameSceneName = "Game";

    public void HideAllMenus()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (victoryPanel != null) victoryPanel.SetActive(false);
    }

    public void ShowMainMenu()
    {
        Time.timeScale = 1f;
        HideAllMenus();
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
    }

    public void ShowPauseMenu()
    {
        if (pausePanel != null) pausePanel.SetActive(true);
    }

    public void HidePauseMenu()
    {
        if (pausePanel != null) pausePanel.SetActive(false);
    }

    public void ShowGameOver(int score, int wave)
    {
        HideAllMenus();
        if (gameOverPanel != null) gameOverPanel.SetActive(true);

        if (gameOverScoreText != null) gameOverScoreText.text = $"Score: {score}";
        if (gameOverWaveText != null) gameOverWaveText.text = $"Reached Wave: {wave}";
    }

    public void ShowVictory(int score)
    {
        HideAllMenus();
        if (victoryPanel != null) victoryPanel.SetActive(true);
        if (victoryScoreText != null) victoryScoreText.text = $"Final Score: {score}";
    }

    // Button callbacks
    public void OnStartClicked()
    {
        AudioManager.Instance?.PlayButtonClick();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartNewGame();
            return;
        }

        if (!string.IsNullOrWhiteSpace(gameSceneName))
        {
            SceneManager.LoadScene(gameSceneName);
        }
    }

    public void OnResumeClicked()
    {
        AudioManager.Instance?.PlayButtonClick();
        GameManager.Instance?.ResumeGame();
    }

    public void OnRestartClicked()
    {
        AudioManager.Instance?.PlayButtonClick();
        GameManager.Instance?.RestartGame();
    }

    public void OnMainMenuClicked()
    {
        AudioManager.Instance?.PlayButtonClick();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.ReturnToMainMenu();
        }

        if (!string.IsNullOrWhiteSpace(mainMenuSceneName) && SceneManager.GetActiveScene().name != mainMenuSceneName)
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(mainMenuSceneName);
        }
    }

    public void OnQuitClicked()
    {
        AudioManager.Instance?.PlayButtonClick();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}

using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Main Menu")]
    public GameObject mainMenuPanel;
    public Text titleText;
    public Text startPromptText;
    public Text highScoreMenuText;

    [Header("Game HUD")]
    public GameObject hudPanel;
    public Text scoreText;
    public Text healthText;
    public Text waveText;

    [Header("Pause Menu")]
    public GameObject pausePanel;
    public Text pauseText;

    [Header("Game Over Screen")]
    public GameObject gameOverPanel;
    public Text gameOverText;
    public Text finalScoreText;
    public Text gameOverHighScoreText;
    public Text restartPromptText;

    [Header("Victory Screen")]
    public GameObject victoryPanel;
    public Text victoryText;
    public Text victoryScoreText;
    public Text victoryHighScoreText;

    [Header("Wave Announcement")]
    public GameObject waveAnnouncementPanel;
    public Text waveAnnouncementText;

    private GameManager gameManager;

    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        CreateUIIfNeeded();
        HideAllPanels();
    }

    void CreateUIIfNeeded()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }

        if (mainMenuPanel == null)
            mainMenuPanel = CreatePanel(canvas.transform, "MainMenuPanel");

        if (hudPanel == null)
            hudPanel = CreatePanel(canvas.transform, "HUDPanel");

        if (pausePanel == null)
            pausePanel = CreatePanel(canvas.transform, "PausePanel");

        if (gameOverPanel == null)
            gameOverPanel = CreatePanel(canvas.transform, "GameOverPanel");

        if (victoryPanel == null)
            victoryPanel = CreatePanel(canvas.transform, "VictoryPanel");

        if (waveAnnouncementPanel == null)
            waveAnnouncementPanel = CreatePanel(canvas.transform, "WaveAnnouncementPanel");

        CreateMainMenuUI();
        CreateHUDUI();
        CreatePauseUI();
        CreateGameOverUI();
        CreateVictoryUI();
        CreateWaveAnnouncementUI();
    }

    GameObject CreatePanel(Transform parent, string name)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent, false);
        RectTransform rect = panel.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        return panel;
    }

    Text CreateText(Transform parent, string name, string content, int fontSize, Vector2 position, TextAnchor anchor = TextAnchor.MiddleCenter)
    {
        GameObject textObj = new GameObject(name);
        textObj.transform.SetParent(parent, false);

        RectTransform rect = textObj.AddComponent<RectTransform>();
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(600, 100);

        Text text = textObj.AddComponent<Text>();
        text.text = content;
        text.fontSize = fontSize;
        text.alignment = anchor;
        text.color = Color.white;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        return text;
    }

    void CreateMainMenuUI()
    {
        if (titleText == null)
            titleText = CreateText(mainMenuPanel.transform, "TitleText", "SPACE SHOOTER", 60, new Vector2(0, 100));

        if (startPromptText == null)
            startPromptText = CreateText(mainMenuPanel.transform, "StartPromptText", "Press ENTER to Start", 30, new Vector2(0, -50));

        if (highScoreMenuText == null)
            highScoreMenuText = CreateText(mainMenuPanel.transform, "HighScoreText", "High Score: 0", 24, new Vector2(0, -120));
    }

    void CreateHUDUI()
    {
        if (scoreText == null)
            scoreText = CreateText(hudPanel.transform, "ScoreText", "Score: 0", 28, new Vector2(-350, 250), TextAnchor.MiddleLeft);

        if (healthText == null)
            healthText = CreateText(hudPanel.transform, "HealthText", "Health: 3", 28, new Vector2(350, 250), TextAnchor.MiddleRight);

        if (waveText == null)
            waveText = CreateText(hudPanel.transform, "WaveText", "Wave: 1", 24, new Vector2(0, 250));
    }

    void CreatePauseUI()
    {
        Image bg = pausePanel.AddComponent<Image>();
        bg.color = new Color(0, 0, 0, 0.7f);

        if (pauseText == null)
            pauseText = CreateText(pausePanel.transform, "PauseText", "PAUSED\n\nPress ESC to Resume", 40, Vector2.zero);
    }

    void CreateGameOverUI()
    {
        Image bg = gameOverPanel.AddComponent<Image>();
        bg.color = new Color(0.2f, 0, 0, 0.8f);

        if (gameOverText == null)
            gameOverText = CreateText(gameOverPanel.transform, "GameOverText", "GAME OVER", 60, new Vector2(0, 100));

        if (finalScoreText == null)
            finalScoreText = CreateText(gameOverPanel.transform, "FinalScoreText", "Score: 0", 36, new Vector2(0, 0));

        if (gameOverHighScoreText == null)
            gameOverHighScoreText = CreateText(gameOverPanel.transform, "HighScoreText", "High Score: 0", 28, new Vector2(0, -60));

        if (restartPromptText == null)
            restartPromptText = CreateText(gameOverPanel.transform, "RestartPromptText", "Press R to Restart", 24, new Vector2(0, -130));
    }

    void CreateVictoryUI()
    {
        Image bg = victoryPanel.AddComponent<Image>();
        bg.color = new Color(0, 0.2f, 0, 0.8f);

        if (victoryText == null)
            victoryText = CreateText(victoryPanel.transform, "VictoryText", "VICTORY!", 60, new Vector2(0, 100));

        if (victoryScoreText == null)
            victoryScoreText = CreateText(victoryPanel.transform, "VictoryScoreText", "Final Score: 0", 36, new Vector2(0, 0));

        if (victoryHighScoreText == null)
            victoryHighScoreText = CreateText(victoryPanel.transform, "VictoryHighScoreText", "High Score: 0", 28, new Vector2(0, -60));

        CreateText(victoryPanel.transform, "VictoryRestartText", "Press R to Play Again", 24, new Vector2(0, -130));
    }

    void CreateWaveAnnouncementUI()
    {
        if (waveAnnouncementText == null)
            waveAnnouncementText = CreateText(waveAnnouncementPanel.transform, "WaveAnnouncementText", "", 48, Vector2.zero);
    }

    void HideAllPanels()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (hudPanel != null) hudPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (victoryPanel != null) victoryPanel.SetActive(false);
        if (waveAnnouncementPanel != null) waveAnnouncementPanel.SetActive(false);
    }

    public void ShowMainMenu()
    {
        HideAllPanels();
        mainMenuPanel.SetActive(true);

        if (gameManager != null && highScoreMenuText != null)
            highScoreMenuText.text = "High Score: " + gameManager.GetHighScore();
    }

    public void ShowGameHUD()
    {
        HideAllPanels();
        hudPanel.SetActive(true);
    }

    public void ShowPauseMenu()
    {
        pausePanel.SetActive(true);
    }

    public void ShowGameOver(int score, int highScore)
    {
        HideAllPanels();
        gameOverPanel.SetActive(true);

        if (finalScoreText != null)
            finalScoreText.text = "Score: " + score;

        if (gameOverHighScoreText != null)
            gameOverHighScoreText.text = "High Score: " + highScore;
    }

    public void ShowVictory(int score, int highScore)
    {
        HideAllPanels();
        victoryPanel.SetActive(true);

        if (victoryScoreText != null)
            victoryScoreText.text = "Final Score: " + score;

        if (victoryHighScoreText != null)
            victoryHighScoreText.text = "High Score: " + highScore;
    }

    public void ShowWaveText(string waveName)
    {
        StartCoroutine(ShowWaveAnnouncement(waveName));
    }

    System.Collections.IEnumerator ShowWaveAnnouncement(string waveName)
    {
        if (waveAnnouncementPanel != null && waveAnnouncementText != null)
        {
            waveAnnouncementText.text = waveName;
            waveAnnouncementPanel.SetActive(true);
            yield return new WaitForSeconds(1.5f);
            waveAnnouncementPanel.SetActive(false);
        }
    }

    public void UpdateScore(int score)
    {
        if (scoreText != null)
            scoreText.text = "Score: " + score;
    }

    public void UpdateHealth(int health)
    {
        if (healthText != null)
            healthText.text = "Health: " + health;
    }

    public void UpdateWave(int wave)
    {
        if (waveText != null)
            waveText.text = "Wave: " + wave;
    }
}

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("HUD Elements")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI highScoreText;
    public TextMeshProUGUI waveText;
    public TextMeshProUGUI livesText;
    public Slider healthBar;
    public TextMeshProUGUI multiplierText;
    public TextMeshProUGUI comboText;

    [Header("Panels")]
    public GameObject hudPanel;
    public GameObject mainMenuPanel;
    public GameObject pausePanel;
    public GameObject gameOverPanel;
    public GameObject victoryPanel;

    [Header("Game Over Elements")]
    public TextMeshProUGUI finalScoreText;
    public TextMeshProUGUI finalWaveText;
    public TextMeshProUGUI gameOverHighScoreText;

    [Header("Victory Elements")]
    public TextMeshProUGUI victoryScoreText;

    [Header("Animation Settings")]
    public float scorePunchScale = 1.2f;
    public float punchDuration = 0.1f;

    private Coroutine scoreAnimationCoroutine;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // Subscribe to events
        ScoreManager.OnScoreChanged += UpdateScore;
        ScoreManager.OnHighScoreChanged += UpdateHighScore;
        ScoreManager.OnMultiplierChanged += UpdateMultiplier;
        ScoreManager.OnComboChanged += UpdateCombo;
        WaveSpawner.OnWaveStarted += UpdateWave;
        PlayerController.OnLivesChanged += UpdateLives;
        GameManager.OnGameStateChanged += OnGameStateChanged;

        // Initialize UI
        InitializeUI();
    }

    private void InitializeUI()
    {
        // Check what scene we're in
        if (GameManager.Instance != null)
        {
            OnGameStateChanged(GameManager.Instance.CurrentState);
        }
        else
        {
            ShowPanel(mainMenuPanel);
        }

        // Initialize score display
        if (ScoreManager.Instance != null)
        {
            UpdateScore(ScoreManager.Instance.CurrentScore);
            UpdateHighScore(ScoreManager.Instance.HighScore);
        }
    }

    private void OnGameStateChanged(GameManager.GameState state)
    {
        HideAllPanels();

        switch (state)
        {
            case GameManager.GameState.MainMenu:
                ShowPanel(mainMenuPanel);
                break;
            case GameManager.GameState.Playing:
                ShowPanel(hudPanel);
                break;
            case GameManager.GameState.Paused:
                ShowPanel(hudPanel);
                ShowPanel(pausePanel);
                break;
            case GameManager.GameState.GameOver:
                ShowPanel(gameOverPanel);
                UpdateGameOverUI();
                break;
            case GameManager.GameState.Victory:
                ShowPanel(victoryPanel);
                UpdateVictoryUI();
                break;
        }
    }

    private void HideAllPanels()
    {
        if (hudPanel != null) hudPanel.SetActive(false);
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (victoryPanel != null) victoryPanel.SetActive(false);
    }

    private void ShowPanel(GameObject panel)
    {
        if (panel != null) panel.SetActive(true);
    }

    public void UpdateScore(int score)
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {score:N0}";

            // Animate score text
            if (scoreAnimationCoroutine != null)
            {
                StopCoroutine(scoreAnimationCoroutine);
            }
            scoreAnimationCoroutine = StartCoroutine(AnimateScoreText());
        }
    }

    private IEnumerator AnimateScoreText()
    {
        if (scoreText == null) yield break;

        Vector3 originalScale = Vector3.one;
        scoreText.transform.localScale = originalScale * scorePunchScale;

        float elapsed = 0f;
        while (elapsed < punchDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / punchDuration;
            scoreText.transform.localScale = Vector3.Lerp(originalScale * scorePunchScale, originalScale, t);
            yield return null;
        }

        scoreText.transform.localScale = originalScale;
    }

    public void UpdateHighScore(int highScore)
    {
        if (highScoreText != null)
        {
            highScoreText.text = $"High Score: {highScore:N0}";
        }
    }

    public void UpdateWave(int wave)
    {
        if (waveText != null)
        {
            waveText.text = $"Wave {wave}";
            StartCoroutine(FlashWaveText());
        }
    }

    private IEnumerator FlashWaveText()
    {
        if (waveText == null) yield break;

        Color originalColor = waveText.color;
        waveText.color = Color.yellow;
        waveText.transform.localScale = Vector3.one * 1.5f;

        yield return new WaitForSeconds(0.5f);

        float duration = 0.5f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            waveText.color = Color.Lerp(Color.yellow, originalColor, t);
            waveText.transform.localScale = Vector3.Lerp(Vector3.one * 1.5f, Vector3.one, t);
            yield return null;
        }

        waveText.color = originalColor;
        waveText.transform.localScale = Vector3.one;
    }

    public void UpdateLives(int lives)
    {
        if (livesText != null)
        {
            livesText.text = $"Lives: {lives}";
        }
    }

    public void UpdateHealth(int current, int max)
    {
        if (healthBar != null)
        {
            healthBar.maxValue = max;
            healthBar.value = current;
        }
    }

    public void UpdateMultiplier(int multiplier)
    {
        if (multiplierText != null)
        {
            if (multiplier > 1)
            {
                multiplierText.gameObject.SetActive(true);
                multiplierText.text = $"x{multiplier}";
            }
            else
            {
                multiplierText.gameObject.SetActive(false);
            }
        }
    }

    public void UpdateCombo(int combo)
    {
        if (comboText != null)
        {
            if (combo > 5)
            {
                comboText.gameObject.SetActive(true);
                comboText.text = $"{combo} Combo!";
            }
            else
            {
                comboText.gameObject.SetActive(false);
            }
        }
    }

    private void UpdateGameOverUI()
    {
        if (finalScoreText != null && ScoreManager.Instance != null)
        {
            finalScoreText.text = $"Final Score: {ScoreManager.Instance.CurrentScore:N0}";
        }

        if (finalWaveText != null && WaveSpawner.Instance != null)
        {
            finalWaveText.text = $"Reached Wave: {WaveSpawner.Instance.CurrentWave}";
        }

        if (gameOverHighScoreText != null && ScoreManager.Instance != null)
        {
            gameOverHighScoreText.text = $"High Score: {ScoreManager.Instance.HighScore:N0}";
        }
    }

    private void UpdateVictoryUI()
    {
        if (victoryScoreText != null && ScoreManager.Instance != null)
        {
            victoryScoreText.text = $"Final Score: {ScoreManager.Instance.CurrentScore:N0}";
        }
    }

    // Button callbacks
    public void OnPlayButton()
    {
        GameManager.Instance?.LoadGameScene();
    }

    public void OnResumeButton()
    {
        GameManager.Instance?.ResumeGame();
    }

    public void OnRestartButton()
    {
        GameManager.Instance?.RestartGame();
    }

    public void OnMainMenuButton()
    {
        GameManager.Instance?.LoadMainMenu();
    }

    public void OnQuitButton()
    {
        GameManager.Instance?.QuitGame();
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        ScoreManager.OnScoreChanged -= UpdateScore;
        ScoreManager.OnHighScoreChanged -= UpdateHighScore;
        ScoreManager.OnMultiplierChanged -= UpdateMultiplier;
        ScoreManager.OnComboChanged -= UpdateCombo;
        WaveSpawner.OnWaveStarted -= UpdateWave;
        PlayerController.OnLivesChanged -= UpdateLives;
        GameManager.OnGameStateChanged -= OnGameStateChanged;
    }
}

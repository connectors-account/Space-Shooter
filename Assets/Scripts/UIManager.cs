using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject hudPanel;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject gameOverPanel;

    [Header("HUD")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Text healthText;
    [SerializeField] private Text scoreText;
    [SerializeField] private Text waveText;
    [SerializeField] private GameObject shieldIndicator;
    [SerializeField] private GameObject rapidFireIndicator;

    [Header("Game Over")]
    [SerializeField] private Text finalScoreText;

    private Coroutine rapidFireIndicatorRoutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void ShowMainMenu()
    {
        mainMenuPanel?.SetActive(true);
        hudPanel?.SetActive(false);
        pausePanel?.SetActive(false);
        gameOverPanel?.SetActive(false);

        SetShieldIndicator(false);
        if (rapidFireIndicator != null)
        {
            rapidFireIndicator.SetActive(false);
        }
    }

    public void ShowHUD()
    {
        mainMenuPanel?.SetActive(false);
        hudPanel?.SetActive(true);
        pausePanel?.SetActive(false);
        gameOverPanel?.SetActive(false);

        SetShieldIndicator(false);
        if (rapidFireIndicator != null)
        {
            rapidFireIndicator.SetActive(false);
        }
    }

    public void ShowPauseMenu()
    {
        pausePanel?.SetActive(true);
    }

    public void HidePauseMenu()
    {
        pausePanel?.SetActive(false);
    }

    public void ShowGameOver(int finalScore)
    {
        gameOverPanel?.SetActive(true);
        hudPanel?.SetActive(false);
        pausePanel?.SetActive(false);

        if (finalScoreText != null)
        {
            finalScoreText.text = $"Final Score: {finalScore}";
        }
    }

    public void UpdateHealth(int current, int max)
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = max;
            healthSlider.value = current;
        }

        if (healthText != null)
        {
            healthText.text = $"HP: {current}/{max}";
        }
    }

    public void UpdateScore(int score)
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {score}";
        }
    }

    public void UpdateWave(int wave)
    {
        if (waveText != null)
        {
            waveText.text = $"Wave: {wave}";
        }
    }

    public void SetShieldIndicator(bool active)
    {
        if (shieldIndicator != null)
        {
            shieldIndicator.SetActive(active);
        }
    }

    public void SetRapidFireIndicator(bool active, float duration = 0f)
    {
        if (rapidFireIndicator == null)
        {
            return;
        }

        rapidFireIndicator.SetActive(active);

        if (rapidFireIndicatorRoutine != null)
        {
            StopCoroutine(rapidFireIndicatorRoutine);
            rapidFireIndicatorRoutine = null;
        }

        if (active && duration > 0f)
        {
            rapidFireIndicatorRoutine = StartCoroutine(HideRapidFireIndicatorAfter(duration));
        }
    }

    private IEnumerator HideRapidFireIndicatorAfter(float duration)
    {
        yield return new WaitForSeconds(duration);
        if (rapidFireIndicator != null)
        {
            rapidFireIndicator.SetActive(false);
        }

        rapidFireIndicatorRoutine = null;
    }

    // ----- UI Button Hooks -----
    public void OnClickStartGame()
    {
        GameManager.Instance?.StartGame();
    }

    public void OnClickResumeGame()
    {
        GameManager.Instance?.ResumeGame();
    }

    public void OnClickRestartToMainMenu()
    {
        GameManager.Instance?.BackToMainMenu();
    }

    public void OnClickQuitGame()
    {
        GameManager.Instance?.QuitGame();
    }
}

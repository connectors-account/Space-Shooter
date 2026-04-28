using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("HUD")]
    [SerializeField] private GameObject hudRoot;
    [SerializeField] private Text scoreText;
    [SerializeField] private Text healthText;
    [SerializeField] private Text waveText;
    [SerializeField] private Text shieldStatusText;
    [SerializeField] private Text rapidFireStatusText;

    [Header("Menus")]
    [SerializeField] private GameObject mainMenuRoot;
    [SerializeField] private GameObject pauseMenuRoot;
    [SerializeField] private GameObject endPanelRoot;
    [SerializeField] private Text endTitleText;
    [SerializeField] private Text endScoreText;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void SetScore(int score)
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {score}";
        }
    }

    public void SetHealth(int currentHealth, int maxHealth)
    {
        if (healthText != null)
        {
            healthText.text = $"Health: {currentHealth}/{maxHealth}";
        }
    }

    public void SetWave(int currentWave, int totalWaves)
    {
        if (waveText != null)
        {
            waveText.text = $"Wave: {currentWave}/{totalWaves}";
        }
    }

    public void SetShield(bool active)
    {
        if (shieldStatusText != null)
        {
            shieldStatusText.text = active ? "Shield: ACTIVE" : "Shield: OFF";
        }
    }

    public void SetRapidFire(bool active)
    {
        if (rapidFireStatusText != null)
        {
            rapidFireStatusText.text = active ? "Rapid Fire: ACTIVE" : "Rapid Fire: OFF";
        }
    }

    public void ShowGameplayHud(bool show)
    {
        if (hudRoot != null)
        {
            hudRoot.SetActive(show);
        }
    }

    public void ShowMainMenu(bool show)
    {
        if (mainMenuRoot != null)
        {
            mainMenuRoot.SetActive(show);
        }
    }

    public void ShowPauseMenu(bool show)
    {
        if (pauseMenuRoot != null)
        {
            pauseMenuRoot.SetActive(show);
        }
    }

    public void ShowEndPanel(bool show, bool isWin, int finalScore)
    {
        if (endPanelRoot != null)
        {
            endPanelRoot.SetActive(show);
        }

        if (!show)
        {
            return;
        }

        if (endTitleText != null)
        {
            endTitleText.text = isWin ? "YOU WIN" : "GAME OVER";
            endTitleText.color = isWin ? Color.cyan : new Color(1f, 0.35f, 0.35f);
        }

        if (endScoreText != null)
        {
            endScoreText.text = $"Final Score: {finalScore}";
        }
    }
}

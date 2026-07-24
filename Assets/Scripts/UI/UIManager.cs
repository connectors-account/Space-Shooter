// ============================================================
//  UIManager.cs  –  All in-game HUD + overlays
//
//  Expected Canvas hierarchy:
//   Canvas
//   ├─ HUD
//   │   ├─ ScoreText          (TMP_Text)
//   │   ├─ MultiplierText     (TMP_Text)
//   │   ├─ WaveText           (TMP_Text)
//   │   ├─ LivesPanel         (parent of life icons)
//   │   └─ HealthBar
//   │       ├─ Background     (Image)
//   │       └─ Fill           (Image, type=Filled)
//   ├─ WaveBanner             (TMP_Text – centred, shown briefly)
//   ├─ PowerUpText            (TMP_Text – shown briefly)
//   ├─ BossHPBar (root)
//   │   └─ BossFill           (Image, type=Filled)
//   ├─ PausePanel             (Panel)
//   └─ GameOverPanel          (Panel)
//       ├─ FinalScoreText     (TMP_Text)
//       └─ HighScoreText      (TMP_Text)
// ============================================================
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("HUD – Score")]
    public TMP_Text scoreText;
    public TMP_Text multiplierText;
    public TMP_Text waveText;

    [Header("HUD – Health")]
    public Image    healthBarFill;
    public GameObject livesIconPrefab;
    public Transform  livesPanel;

    [Header("Wave Banner")]
    public TMP_Text waveBannerText;
    public float    bannerDuration = 2.5f;

    [Header("Power-up Text")]
    public TMP_Text powerUpText;
    public float    powerUpTextDuration = 2f;

    [Header("Boss HP Bar")]
    public GameObject bossHPRoot;
    public Image      bossHPFill;

    [Header("Panels")]
    public GameObject pausePanel;
    public GameObject gameOverPanel;
    public TMP_Text   finalScoreText;
    public TMP_Text   highScoreText;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        pausePanel?.SetActive(false);
        gameOverPanel?.SetActive(false);
        if (waveBannerText) waveBannerText.gameObject.SetActive(false);
        if (powerUpText)    powerUpText.gameObject.SetActive(false);
        ShowBossHP(false, 1);
    }

    // ── Score ─────────────────────────────────────────────────

    public void RefreshScore(int score, int mult)
    {
        if (scoreText)      scoreText.text      = $"SCORE  {score:000000}";
        if (multiplierText)
        {
            multiplierText.text    = mult > 1 ? $"x{mult}" : "";
            multiplierText.color   = mult >= 4 ? Color.red
                                   : mult >= 3 ? Color.yellow
                                   : Color.white;
        }
    }

    // ── Health ────────────────────────────────────────────────

    public void RefreshHP(int current, int max)
    {
        if (healthBarFill)
            healthBarFill.fillAmount = (float)current / max;
    }

    public void RefreshLives(int lives)
    {
        if (livesPanel == null) return;
        foreach (Transform t in livesPanel)
            Destroy(t.gameObject);

        for (int i = 0; i < lives; i++)
        {
            if (livesIconPrefab)
                Instantiate(livesIconPrefab, livesPanel);
        }
    }

    // ── Wave ─────────────────────────────────────────────────

    public void ShowWaveBanner(string text)
    {
        if (waveBannerText == null) return;
        StopCoroutine(nameof(HideBanner));
        waveBannerText.text = text;
        waveBannerText.gameObject.SetActive(true);
        StartCoroutine(nameof(HideBanner));
    }

    IEnumerator HideBanner()
    {
        yield return new WaitForSeconds(bannerDuration);
        if (waveBannerText) waveBannerText.gameObject.SetActive(false);
    }

    // ── Power-up toast ────────────────────────────────────────

    public void ShowPowerUpText(string label)
    {
        if (powerUpText == null) return;
        StopCoroutine(nameof(HidePowerUpText));
        powerUpText.text = label.ToUpper() + "!";
        powerUpText.gameObject.SetActive(true);
        StartCoroutine(nameof(HidePowerUpText));
    }

    IEnumerator HidePowerUpText()
    {
        yield return new WaitForSeconds(powerUpTextDuration);
        if (powerUpText) powerUpText.gameObject.SetActive(false);
    }

    // ── Boss HP ───────────────────────────────────────────────

    public void ShowBossHP(bool show, int maxHP)
    {
        if (bossHPRoot) bossHPRoot.SetActive(show);
        if (bossHPFill) bossHPFill.fillAmount = 1f;
    }

    public void RefreshBossHP(int current, int max)
    {
        if (bossHPFill)
            bossHPFill.fillAmount = Mathf.Clamp01((float)current / max);
    }

    // ── Pause ─────────────────────────────────────────────────

    public void ShowPausePanel(bool show)
    {
        if (pausePanel) pausePanel.SetActive(show);
    }

    // ── Game Over ─────────────────────────────────────────────

    public void ShowGameOverPanel()
    {
        if (gameOverPanel) gameOverPanel.SetActive(true);

        if (finalScoreText && ScoreManager.Instance != null)
            finalScoreText.text = $"SCORE\n{ScoreManager.Instance.Score:000000}";

        if (highScoreText && ScoreManager.Instance != null)
            highScoreText.text = $"BEST\n{ScoreManager.Instance.HighScore:000000}";
    }

    // ── Button callbacks (wired in Inspector) ─────────────────

    public void OnResumeClicked()    => GameManager.Instance?.ResumeGame();
    public void OnRestartClicked()   => GameManager.Instance?.StartGame();
    public void OnMenuClicked()      => GameManager.Instance?.ReturnToMenu();
    public void OnQuitClicked()      => GameManager.Instance?.QuitApplication();
}

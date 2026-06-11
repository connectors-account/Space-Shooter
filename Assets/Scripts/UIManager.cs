using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Drives all on-screen UI: score, wave, health bar, and the menu / game-over
/// panels. Subscribes to GameManager and the player's Health events so it never
/// has to poll. Wire the public fields up in the Inspector.
/// </summary>
public class UIManager : MonoBehaviour
{
    [Header("HUD")]
    [Tooltip("Text element that shows the current score.")]
    public Text scoreText;

    [Tooltip("Text element that shows the current wave.")]
    public Text waveText;

    [Tooltip("Slider used as the player health bar (min 0, max 1).")]
    public Slider healthSlider;

    [Tooltip("Optional text showing numeric health, e.g. '80 / 100'.")]
    public Text healthText;

    [Header("Panels")]
    [Tooltip("Main menu / start panel.")]
    public GameObject menuPanel;

    [Tooltip("HUD container shown during gameplay.")]
    public GameObject hudPanel;

    [Tooltip("Game over panel.")]
    public GameObject gameOverPanel;

    [Header("Game Over Texts")]
    [Tooltip("Text showing the final score on the game over screen.")]
    public Text finalScoreText;

    [Tooltip("Text showing the high score on the game over screen.")]
    public Text highScoreText;

    [Header("References")]
    [Tooltip("The player's Health component. Auto-found by tag if left empty.")]
    public Health playerHealth;

    private void Start()
    {
        // Subscribe to game state and score/wave events.
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnScoreChanged += UpdateScore;
            GameManager.Instance.OnWaveChanged += UpdateWave;
            GameManager.Instance.OnStateChanged += HandleStateChanged;

            UpdateScore(GameManager.Instance.Score);
            UpdateWave(GameManager.Instance.Wave);
            HandleStateChanged(GameManager.Instance.State);
        }

        // Find the player health if it was not assigned.
        if (playerHealth == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerHealth = player.GetComponent<Health>();
            }
        }

        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged += UpdateHealth;
            UpdateHealth(playerHealth.CurrentHealth, playerHealth.maxHealth);
        }
    }

    private void OnDestroy()
    {
        // Clean up subscriptions to avoid leaks on scene reload.
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnScoreChanged -= UpdateScore;
            GameManager.Instance.OnWaveChanged -= UpdateWave;
            GameManager.Instance.OnStateChanged -= HandleStateChanged;
        }

        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged -= UpdateHealth;
        }
    }

    private void UpdateScore(int score)
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + score;
        }
    }

    private void UpdateWave(int wave)
    {
        if (waveText != null)
        {
            waveText.text = "Wave: " + wave;
        }
    }

    private void UpdateHealth(int current, int max)
    {
        if (healthSlider != null)
        {
            healthSlider.value = max > 0 ? (float)current / max : 0f;
        }

        if (healthText != null)
        {
            healthText.text = current + " / " + max;
        }
    }

    private void HandleStateChanged(GameManager.GameState state)
    {
        // Toggle panels based on the current game state.
        if (menuPanel != null)
        {
            menuPanel.SetActive(state == GameManager.GameState.Menu);
        }

        if (hudPanel != null)
        {
            hudPanel.SetActive(state == GameManager.GameState.Playing);
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(state == GameManager.GameState.GameOver);
        }

        if (state == GameManager.GameState.GameOver)
        {
            ShowGameOverStats();
        }
    }

    private void ShowGameOverStats()
    {
        if (GameManager.Instance == null)
        {
            return;
        }

        if (finalScoreText != null)
        {
            finalScoreText.text = "Final Score: " + GameManager.Instance.Score;
        }

        if (highScoreText != null)
        {
            highScoreText.text = "High Score: " + GameManager.Instance.HighScore;
        }
    }

    // ---- Button hooks (wire these to UI Buttons in the Inspector) ----

    /// <summary>
    /// Hook for the Start / Play button.
    /// </summary>
    public void OnStartButton()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartGame();
        }
    }

    /// <summary>
    /// Hook for the Restart button on the game over screen.
    /// </summary>
    public void OnRestartButton()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RestartGame();
        }
    }

    /// <summary>
    /// Hook for a Quit button. Works in standalone builds.
    /// </summary>
    public void OnQuitButton()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}

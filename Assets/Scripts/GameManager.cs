using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// GameManager is the central game-state controller.
/// Tracks score, wave number, game state, and orchestrates
/// spawners, UI, and scene transitions.
/// Persists across scenes via DontDestroyOnLoad.
/// </summary>
public class GameManager : MonoBehaviour
{
    // ── Singleton ────────────────────────────────────────────
    public static GameManager Instance { get; private set; }

    // ── Game State ───────────────────────────────────────────
    public enum GameState { MainMenu, Playing, Paused, GameOver }
    private GameState currentState = GameState.MainMenu;

    // ── Score ────────────────────────────────────────────────
    private int score = 0;
    private int highScore = 0;

    // ── Wave ─────────────────────────────────────────────────
    private int currentWave = 0;
    private float waveMultiplier = 1f;

    // ── Public Properties ────────────────────────────────────
    public GameState CurrentState => currentState;
    public int Score => score;
    public int HighScore => highScore;
    public int CurrentWave => currentWave;

    // ──────────────────────────────────────────────────────────
    // Unity Lifecycle
    // ──────────────────────────────────────────────────────────

    private void Awake()
    {
        // Singleton pattern – persist across scenes
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Load persisted high score
        highScore = PlayerPrefs.GetInt("HighScore", 0);
    }

    private void Update()
    {
        // Pause / unpause with Escape key during gameplay
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (currentState == GameState.Playing)
                PauseGame();
            else if (currentState == GameState.Paused)
                ResumeGame();
        }
    }

    // ──────────────────────────────────────────────────────────
    // Game Flow
    // ──────────────────────────────────────────────────────────

    /// <summary>
    /// Transition from Main Menu to the gameplay scene.
    /// </summary>
    public void StartGame()
    {
        score = 0;
        currentWave = 0;
        waveMultiplier = 1f;
        currentState = GameState.Playing;
        Time.timeScale = 1f;
        SceneManager.LoadScene("GamePlay");
    }

    /// <summary>
    /// Called after GamePlay scene loads; kick off spawners.
    /// Hook this to SceneManager.sceneLoaded or call from a scene bootstrap.
    /// </summary>
    public void BeginGameplay()
    {
        currentState = GameState.Playing;

        if (EnemySpawner.Instance != null)
            EnemySpawner.Instance.StartSpawning();

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateScoreDisplay(score);
            UIManager.Instance.UpdateWaveDisplay(currentWave);
        }
    }

    public void PauseGame()
    {
        if (currentState != GameState.Playing) return;
        currentState = GameState.Paused;
        Time.timeScale = 0f;

        if (MenuManager.Instance != null)
            MenuManager.Instance.ShowPauseMenu();
    }

    public void ResumeGame()
    {
        if (currentState != GameState.Paused) return;
        currentState = GameState.Playing;
        Time.timeScale = 1f;

        if (MenuManager.Instance != null)
            MenuManager.Instance.HidePauseMenu();
    }

    /// <summary>
    /// Called when the player ship is destroyed.
    /// </summary>
    public void OnPlayerDeath()
    {
        currentState = GameState.GameOver;

        // Save high score
        if (score > highScore)
        {
            highScore = score;
            PlayerPrefs.SetInt("HighScore", highScore);
            PlayerPrefs.Save();
        }

        // Stop spawning
        if (EnemySpawner.Instance != null)
            EnemySpawner.Instance.StopSpawning();

        // Show game over screen after a short delay
        StartCoroutine(ShowGameOverDelayed(1.5f));
    }

    private System.Collections.IEnumerator ShowGameOverDelayed(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (MenuManager.Instance != null)
            MenuManager.Instance.ShowGameOverScreen(score, highScore);
    }

    /// <summary>
    /// Return to main menu scene.
    /// </summary>
    public void ReturnToMainMenu()
    {
        currentState = GameState.MainMenu;
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    /// <summary>
    /// Restart the gameplay scene.
    /// </summary>
    public void RestartGame()
    {
        StartGame();
    }

    // ──────────────────────────────────────────────────────────
    // Score
    // ──────────────────────────────────────────────────────────

    /// <summary>
    /// Add score with the current wave multiplier applied.
    /// </summary>
    public void AddScore(int baseScore)
    {
        int earned = Mathf.RoundToInt(baseScore * waveMultiplier);
        score += earned;

        if (UIManager.Instance != null)
            UIManager.Instance.UpdateScoreDisplay(score);
    }

    // ──────────────────────────────────────────────────────────
    // Wave Progression
    // ──────────────────────────────────────────────────────────

    /// <summary>
    /// Called by EnemySpawner at the start of each new wave.
    /// Increases the score multiplier so later waves reward more.
    /// </summary>
    public void OnNewWave(int waveNumber)
    {
        currentWave = waveNumber;
        waveMultiplier = 1f + (waveNumber - 1) * 0.25f; // +25% per wave

        if (UIManager.Instance != null)
            UIManager.Instance.UpdateWaveDisplay(currentWave);
    }

    // ──────────────────────────────────────────────────────────
    // Scene Management Helpers
    // ──────────────────────────────────────────────────────────

    /// <summary>
    /// Quit the application.
    /// </summary>
    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}

using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Central game manager singleton. Handles game state, scoring,
/// scene transitions, and coordinates between subsystems.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Settings")]
    [SerializeField] private float gameOverDelay = 2f;

    private int currentScore;
    private bool isGameOver;
    private bool isGamePaused;

    // UI References (set by UIManager on scene load)
    private UIManager uiManager;

    public int CurrentScore => currentScore;
    public bool IsGameOver => isGameOver;
    public bool IsGamePaused => isGamePaused;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        uiManager = FindFirstObjectByType<UIManager>();
        StartGame();
    }

    private void Update()
    {
        // Pause toggle with Escape key
        if (Input.GetKeyDown(KeyCode.Escape) && !isGameOver)
        {
            if (isGamePaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    /// <summary>
    /// Initializes and starts a new game session.
    /// </summary>
    public void StartGame()
    {
        currentScore = 0;
        isGameOver = false;
        isGamePaused = false;
        Time.timeScale = 1f;

        if (uiManager != null)
        {
            uiManager.ShowHUD();
            uiManager.UpdateScore(0);
            uiManager.UpdateWave(0);
        }

        if (EnemySpawner.Instance != null)
            EnemySpawner.Instance.StartSpawning();
    }

    /// <summary>
    /// Adds to the current score and updates the UI.
    /// </summary>
    public void AddScore(int points)
    {
        if (isGameOver) return;

        currentScore += points;

        if (uiManager != null)
            uiManager.UpdateScore(currentScore);
    }

    /// <summary>
    /// Updates the health UI display.
    /// </summary>
    public void UpdateHealthUI(int current, int max)
    {
        if (uiManager != null)
            uiManager.UpdateHealth(current, max);
    }

    /// <summary>
    /// Updates the wave number UI display.
    /// </summary>
    public void UpdateWaveUI(int wave)
    {
        if (uiManager != null)
            uiManager.UpdateWave(wave);
    }

    /// <summary>
    /// Called when the player dies. Triggers game over sequence.
    /// </summary>
    public void OnPlayerDeath()
    {
        if (isGameOver) return;

        isGameOver = true;

        if (EnemySpawner.Instance != null)
            EnemySpawner.Instance.StopSpawning();

        Invoke(nameof(ShowGameOver), gameOverDelay);
    }

    /// <summary>
    /// Shows the game over screen with final score.
    /// </summary>
    private void ShowGameOver()
    {
        if (uiManager != null)
            uiManager.ShowGameOver(currentScore);
    }

    /// <summary>
    /// Pauses the game.
    /// </summary>
    public void PauseGame()
    {
        isGamePaused = true;
        Time.timeScale = 0f;

        if (uiManager != null)
            uiManager.ShowPauseMenu();
    }

    /// <summary>
    /// Resumes the game from pause.
    /// </summary>
    public void ResumeGame()
    {
        isGamePaused = false;
        Time.timeScale = 1f;

        if (uiManager != null)
            uiManager.HidePauseMenu();
    }

    /// <summary>
    /// Restarts the gameplay scene.
    /// </summary>
    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("GameScene");
    }

    /// <summary>
    /// Returns to the main menu scene.
    /// </summary>
    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    /// <summary>
    /// Quits the application.
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

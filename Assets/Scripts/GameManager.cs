using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Central game state manager — handles scoring, health, game flow, and scene transitions.
/// Singleton pattern — persists across the game session.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game State")]
    private int score;
    private int highScore;
    private int currentWave;
    private int playerHealth;
    private int playerMaxHealth;
    private bool isGameOver;
    private bool isGameActive;

    // References (found at runtime)
    private UIManager uiManager;
    private EnemySpawner enemySpawner;
    private PlayerController playerController;

    private const string HighScoreKey = "HighScore";

    private void Awake()
    {
        // Singleton setup
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        highScore = PlayerPrefs.GetInt(HighScoreKey, 0);
        FindReferences();
        ShowMainMenu();
    }

    /// <summary>
    /// Locates key components in the scene.
    /// </summary>
    private void FindReferences()
    {
        uiManager = FindObjectOfType<UIManager>();
        enemySpawner = FindObjectOfType<EnemySpawner>();
        playerController = FindObjectOfType<PlayerController>();
    }

    /// <summary>
    /// Displays the main menu and pauses gameplay.
    /// </summary>
    public void ShowMainMenu()
    {
        isGameActive = false;
        isGameOver = false;
        Time.timeScale = 1f;

        if (playerController != null)
            playerController.gameObject.SetActive(false);

        if (uiManager != null)
        {
            uiManager.ShowMainMenu(highScore);
        }
    }

    /// <summary>
    /// Starts a new game. Called from the main menu.
    /// </summary>
    public void StartGame()
    {
        FindReferences();

        score = 0;
        currentWave = 0;
        isGameOver = false;
        isGameActive = true;
        Time.timeScale = 1f;

        // Reset player
        if (playerController != null)
        {
            playerController.gameObject.SetActive(true);
            playerController.transform.position = new Vector3(0, -3.5f, 0);
        }

        // Clear existing enemies and bullets
        ClearAllEntities();

        if (uiManager != null)
        {
            uiManager.ShowGameHUD();
            uiManager.UpdateScore(score);
            uiManager.UpdateWave(currentWave);
        }

        if (enemySpawner != null)
            enemySpawner.StartSpawning();
    }

    /// <summary>
    /// Adds points to the score and updates UI.
    /// </summary>
    public void AddScore(int points)
    {
        if (!isGameActive) return;

        score += points;

        if (uiManager != null)
            uiManager.UpdateScore(score);
    }

    /// <summary>
    /// Updates the displayed wave number.
    /// </summary>
    public void UpdateWave(int wave)
    {
        currentWave = wave;
        if (uiManager != null)
            uiManager.UpdateWave(wave);
    }

    /// <summary>
    /// Sets the player's max health for UI display.
    /// </summary>
    public void SetPlayerMaxHealth(int max)
    {
        playerMaxHealth = max;
        playerHealth = max;
        if (uiManager != null)
            uiManager.UpdateHealth(playerHealth, playerMaxHealth);
    }

    /// <summary>
    /// Updates current player health in the UI.
    /// </summary>
    public void UpdatePlayerHealth(int health)
    {
        playerHealth = health;
        if (uiManager != null)
            uiManager.UpdateHealth(playerHealth, playerMaxHealth);
    }

    /// <summary>
    /// Displays a temporary power-up pickup message.
    /// </summary>
    public void ShowPowerUpText(string text)
    {
        if (uiManager != null)
            uiManager.ShowPowerUpText(text);
    }

    /// <summary>
    /// Called when the player dies. Triggers game over sequence.
    /// </summary>
    public void OnPlayerDeath()
    {
        if (isGameOver) return;
        isGameOver = true;
        isGameActive = false;

        // Update high score
        if (score > highScore)
        {
            highScore = score;
            PlayerPrefs.SetInt(HighScoreKey, highScore);
            PlayerPrefs.Save();
        }

        if (enemySpawner != null)
            enemySpawner.StopSpawning();

        if (uiManager != null)
            uiManager.ShowGameOver(score, highScore);
    }

    /// <summary>
    /// Restarts the game (called from Game Over screen).
    /// </summary>
    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    /// <summary>
    /// Returns to the main menu (called from Game Over screen).
    /// </summary>
    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
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

    /// <summary>
    /// Destroys all active enemies, bullets, and power-ups.
    /// </summary>
    private void ClearAllEntities()
    {
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("Enemy"))
            Destroy(go);
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("PlayerBullet"))
            Destroy(go);
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("EnemyBullet"))
            Destroy(go);
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("PowerUp"))
            Destroy(go);

        // Also destroy any explosions
        foreach (ExplosionEffect e in FindObjectsOfType<ExplosionEffect>())
            Destroy(e.gameObject);
    }

    // Public getters
    public int GetScore() => score;
    public int GetHighScore() => highScore;
    public int GetCurrentWave() => currentWave;
    public bool IsGameActive() => isGameActive;
    public bool IsGameOver() => isGameOver;
}

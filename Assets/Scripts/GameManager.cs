using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// GameManager is the central controller for the game.
/// It is implemented as a singleton so any script can easily access it.
/// Responsibilities:
///   - Track the player's score and health.
///   - Track the current game state (Playing / GameOver / Win).
///   - Notify the UIManager whenever score or health changes.
///   - Handle win / lose conditions and scene reloading.
/// </summary>
public class GameManager : MonoBehaviour
{
    // Singleton instance so other scripts can call GameManager.Instance.
    public static GameManager Instance { get; private set; }

    [Header("Player Settings")]
    [Tooltip("Maximum (and starting) health of the player.")]
    public int maxPlayerHealth = 100;

    [Header("Win Condition")]
    [Tooltip("Score required to win the game. Set to 0 to disable win-by-score.")]
    public int scoreToWin = 0;

    // Current runtime values.
    private int currentScore;
    private int currentHealth;

    // Possible states the game can be in.
    public enum GameState { Playing, GameOver, Win }
    public GameState State { get; private set; }

    /// <summary>
    /// Awake runs before Start. We set up the singleton here.
    /// </summary>
    private void Awake()
    {
        // Enforce a single instance of GameManager.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    /// <summary>
    /// Start initializes the game values when the scene loads.
    /// </summary>
    private void Start()
    {
        // Make sure time is running (it may have been paused on a previous game over).
        Time.timeScale = 1f;

        currentScore = 0;
        currentHealth = maxPlayerHealth;
        State = GameState.Playing;

        // Update the UI with the starting values (if a UIManager exists).
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateScore(currentScore);
            UIManager.Instance.UpdateHealth(currentHealth, maxPlayerHealth);
            UIManager.Instance.HideGameOver();
        }
    }

    /// <summary>
    /// Adds points to the player's score and updates the UI.
    /// Called by enemies when they are destroyed.
    /// </summary>
    /// <param name="points">Number of points to add.</param>
    public void AddScore(int points)
    {
        if (State != GameState.Playing) return;

        currentScore += points;

        if (UIManager.Instance != null)
            UIManager.Instance.UpdateScore(currentScore);

        // Check the optional win-by-score condition.
        if (scoreToWin > 0 && currentScore >= scoreToWin)
            WinGame();
    }

    /// <summary>
    /// Reduces the player's health by the given damage amount.
    /// Triggers game over when health reaches zero.
    /// </summary>
    /// <param name="damage">Amount of damage taken.</param>
    public void DamagePlayer(int damage)
    {
        if (State != GameState.Playing) return;

        currentHealth -= damage;
        if (currentHealth < 0) currentHealth = 0;

        if (UIManager.Instance != null)
            UIManager.Instance.UpdateHealth(currentHealth, maxPlayerHealth);

        if (currentHealth <= 0)
            GameOver();
    }

    /// <summary>
    /// Heals the player without exceeding the maximum health.
    /// </summary>
    /// <param name="amount">Amount of health to restore.</param>
    public void HealPlayer(int amount)
    {
        if (State != GameState.Playing) return;

        currentHealth += amount;
        if (currentHealth > maxPlayerHealth) currentHealth = maxPlayerHealth;

        if (UIManager.Instance != null)
            UIManager.Instance.UpdateHealth(currentHealth, maxPlayerHealth);
    }

    /// <summary>
    /// Ends the game with a loss. Shows the game over UI and freezes the game.
    /// </summary>
    public void GameOver()
    {
        if (State != GameState.Playing) return;

        State = GameState.GameOver;

        if (UIManager.Instance != null)
            UIManager.Instance.ShowGameOver(currentScore, false);

        // Freeze gameplay. UI buttons still work because they ignore timeScale.
        Time.timeScale = 0f;
    }

    /// <summary>
    /// Ends the game with a win. Shows the win UI and freezes the game.
    /// </summary>
    public void WinGame()
    {
        if (State != GameState.Playing) return;

        State = GameState.Win;

        if (UIManager.Instance != null)
            UIManager.Instance.ShowGameOver(currentScore, true);

        Time.timeScale = 0f;
    }

    /// <summary>
    /// Reloads the current gameplay scene to restart the game.
    /// Hooked up to the "Restart" button in the UI.
    /// </summary>
    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    /// <summary>
    /// Loads the main menu scene. Hooked up to the "Main Menu" button.
    /// </summary>
    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    // Public getters in case other scripts need to read these values.
    public int GetScore() => currentScore;
    public int GetHealth() => currentHealth;
}

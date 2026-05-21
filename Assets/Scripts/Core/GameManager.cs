using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// GameManager - Singleton that manages game state, scoring, waves, and transitions.
/// Attach to an empty GameObject named "GameManager" in the Game scene.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game State")]
    public bool isGameActive = false;
    public bool isGameOver = false;

    [Header("Score")]
    public int score = 0;
    public int highScore = 0;

    [Header("Wave Settings")]
    public int currentWave = 0;
    public int enemiesPerWave = 4;
    public int enemiesRemainingInWave = 0;
    public float waveDifficultyMultiplier = 1.2f;

    [Header("Player Reference")]
    public PlayerController player;

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
        highScore = PlayerPrefs.GetInt("HighScore", 0);
    }

    /// <summary>
    /// Called to begin the game (from menu or restart).
    /// </summary>
    public void StartGame()
    {
        isGameActive = true;
        isGameOver = false;
        score = 0;
        currentWave = 0;

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateScore(score);
            UIManager.Instance.HideGameOver();
        }

        StartNextWave();
    }

    /// <summary>
    /// Advance to the next wave. Increases enemy count with each wave.
    /// </summary>
    public void StartNextWave()
    {
        if (!isGameActive || isGameOver) return;

        currentWave++;
        int enemyCount = Mathf.RoundToInt(enemiesPerWave * Mathf.Pow(waveDifficultyMultiplier, currentWave - 1));
        enemiesRemainingInWave = enemyCount;

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowWaveBanner(currentWave);
        }

        if (SpawnManager.Instance != null)
        {
            SpawnManager.Instance.StartWave(enemyCount, currentWave);
        }

        Debug.Log($"Wave {currentWave} started with {enemyCount} enemies.");
    }

    /// <summary>
    /// Called when an enemy is destroyed. Awards points and checks wave completion.
    /// </summary>
    public void OnEnemyDestroyed(int points)
    {
        if (!isGameActive) return;

        score += points;
        enemiesRemainingInWave--;

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateScore(score);
        }

        if (enemiesRemainingInWave <= 0)
        {
            StartNextWave();
        }
    }

    /// <summary>
    /// Called when the player dies. Ends the game.
    /// </summary>
    public void OnPlayerDeath()
    {
        isGameActive = false;
        isGameOver = true;

        if (SpawnManager.Instance != null)
        {
            SpawnManager.Instance.StopSpawning();
        }

        if (score > highScore)
        {
            highScore = score;
            PlayerPrefs.SetInt("HighScore", highScore);
            PlayerPrefs.Save();
        }

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowGameOver(score, highScore);
        }

        Debug.Log($"Game Over! Score: {score} | High Score: {highScore}");
    }

    /// <summary>
    /// Restart the current game scene.
    /// </summary>
    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    /// <summary>
    /// Return to the main menu scene.
    /// </summary>
    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}

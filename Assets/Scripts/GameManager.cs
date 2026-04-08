using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Central game manager handling game state, scoring, wave management, and game flow.
/// Singleton pattern ensures only one instance exists across scenes.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game State")]
    public bool isGameActive = false;
    public bool isPaused = false;

    [Header("Score")]
    public int score = 0;
    public int highScore = 0;

    [Header("Wave Settings")]
    public int currentWave = 0;
    public int enemiesRemainingInWave = 0;
    public int enemiesKilledInWave = 0;
    public float waveCooldown = 3f;

    [Header("Difficulty Scaling")]
    public float difficultyMultiplier = 1f;
    public float difficultyIncreasePerWave = 0.15f;

    private bool waveInProgress = false;
    private float waveTimer = 0f;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        highScore = PlayerPrefs.GetInt("HighScore", 0);
    }

    void Update()
    {
        if (!isGameActive) return;

        // Handle pause toggle
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }

        if (isPaused) return;

        // Wave progression logic
        if (!waveInProgress)
        {
            waveTimer += Time.deltaTime;
            if (waveTimer >= waveCooldown)
            {
                StartNextWave();
            }
        }
        else if (enemiesRemainingInWave <= 0 && enemiesKilledInWave > 0)
        {
            EndWave();
        }
    }

    /// <summary>
    /// Starts a new game session, resetting all values.
    /// </summary>
    public void StartGame()
    {
        score = 0;
        currentWave = 0;
        difficultyMultiplier = 1f;
        waveInProgress = false;
        waveTimer = 0f;
        isGameActive = true;
        isPaused = false;
        Time.timeScale = 1f;

        SceneManager.LoadScene("GamePlay");
    }

    /// <summary>
    /// Starts the next wave of enemies.
    /// </summary>
    public void StartNextWave()
    {
        currentWave++;
        difficultyMultiplier = 1f + (currentWave - 1) * difficultyIncreasePerWave;

        int enemyCount = 3 + (currentWave * 2);
        enemiesRemainingInWave = enemyCount;
        enemiesKilledInWave = 0;
        waveInProgress = true;
        waveTimer = 0f;

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowWaveText(currentWave);
        }

        if (EnemySpawner.Instance != null)
        {
            EnemySpawner.Instance.StartWave(currentWave, enemyCount, difficultyMultiplier);
        }

        Debug.Log($"Wave {currentWave} started! Enemies: {enemyCount}, Difficulty: {difficultyMultiplier:F2}");
    }

    /// <summary>
    /// Called when an enemy is destroyed.
    /// </summary>
    public void OnEnemyKilled(int points)
    {
        score += Mathf.RoundToInt(points * difficultyMultiplier);
        enemiesKilledInWave++;
        enemiesRemainingInWave--;

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateScore(score);
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySound("EnemyDeath");
        }
    }

    /// <summary>
    /// Ends the current wave and prepares for the next one.
    /// </summary>
    private void EndWave()
    {
        waveInProgress = false;
        waveTimer = 0f;

        // Bonus points for completing a wave
        int waveBonus = currentWave * 50;
        score += waveBonus;

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateScore(score);
            UIManager.Instance.ShowWaveCompleteText(currentWave, waveBonus);
        }

        Debug.Log($"Wave {currentWave} complete! Bonus: {waveBonus}");
    }

    /// <summary>
    /// Toggles the pause state.
    /// </summary>
    public void TogglePause()
    {
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0f : 1f;

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowPauseMenu(isPaused);
        }
    }

    /// <summary>
    /// Called when the player dies. Triggers Game Over.
    /// </summary>
    public void GameOver()
    {
        isGameActive = false;
        waveInProgress = false;

        if (score > highScore)
        {
            highScore = score;
            PlayerPrefs.SetInt("HighScore", highScore);
            PlayerPrefs.Save();
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySound("GameOver");
        }

        Time.timeScale = 1f;
        SceneManager.LoadScene("GameOver");
    }

    /// <summary>
    /// Returns to the main menu.
    /// </summary>
    public void ReturnToMenu()
    {
        isGameActive = false;
        isPaused = false;
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    /// <summary>
    /// Quits the application.
    /// </summary>
    public void QuitGame()
    {
        PlayerPrefs.Save();
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}

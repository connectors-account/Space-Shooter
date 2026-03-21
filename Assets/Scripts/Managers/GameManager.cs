using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Central game state manager. Singleton that persists across scenes.
/// Controls game flow: menu, playing, paused, game over.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameState { MainMenu, Playing, Paused, GameOver }
    public GameState CurrentState { get; private set; } = GameState.MainMenu;

    [Header("Game Settings")]
    public int startingLives = 3;
    public float respawnDelay = 2f;

    // Runtime state
    private int currentScore;
    private int highScore;
    private int currentLives;
    private int currentWave;
    private bool isInitialized;

    public int Score => currentScore;
    public int HighScore => highScore;
    public int Lives => currentLives;
    public int Wave => currentWave;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        highScore = PlayerPrefs.GetInt("HighScore", 0);
    }

    /// <summary>Start a new game session.</summary>
    public void StartGame()
    {
        currentScore = 0;
        currentLives = startingLives;
        currentWave = 0;
        CurrentState = GameState.Playing;
        Time.timeScale = 1f;
        SceneManager.LoadScene("GameScene");
    }

    /// <summary>Return to main menu.</summary>
    public void GoToMainMenu()
    {
        CurrentState = GameState.MainMenu;
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    /// <summary>Toggle pause state.</summary>
    public void TogglePause()
    {
        if (CurrentState == GameState.Playing)
        {
            CurrentState = GameState.Paused;
            Time.timeScale = 0f;
        }
        else if (CurrentState == GameState.Paused)
        {
            CurrentState = GameState.Playing;
            Time.timeScale = 1f;
        }
    }

    /// <summary>Add score points.</summary>
    public void AddScore(int points)
    {
        if (CurrentState != GameState.Playing) return;
        currentScore += points;
        if (currentScore > highScore)
        {
            highScore = currentScore;
            PlayerPrefs.SetInt("HighScore", highScore);
            PlayerPrefs.Save();
        }
        UIManager.Instance?.UpdateScore(currentScore);
    }

    /// <summary>Set current wave number.</summary>
    public void SetWave(int wave)
    {
        currentWave = wave;
        UIManager.Instance?.UpdateWave(currentWave);
    }

    /// <summary>Player lost a life. Returns true if game over.</summary>
    public bool LoseLife()
    {
        currentLives--;
        UIManager.Instance?.UpdateLives(currentLives);

        if (currentLives <= 0)
        {
            TriggerGameOver();
            return true;
        }
        return false;
    }

    /// <summary>Add an extra life.</summary>
    public void AddLife()
    {
        currentLives++;
        UIManager.Instance?.UpdateLives(currentLives);
    }

    /// <summary>End the game.</summary>
    public void TriggerGameOver()
    {
        CurrentState = GameState.GameOver;
        Time.timeScale = 0f;
        UIManager.Instance?.ShowGameOver(currentScore, highScore);
    }

    /// <summary>Restart the current game.</summary>
    public void RestartGame()
    {
        StartGame();
    }

    /// <summary>Quit the application.</summary>
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

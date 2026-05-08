using UnityEngine;
using UnityEngine.SceneManagement;
using System;

/// <summary>
/// Central game manager singleton. Manages game state, score, and coordinates all subsystems.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    // --- Events ---
    public event Action<GameState> OnGameStateChanged;
    public event Action<int> OnScoreChanged;
    public event Action<int> OnWaveChanged;
    public event Action<int, int> OnPlayerHealthChanged; // current, max

    // --- Game State ---
    public GameState CurrentState { get; private set; } = GameState.MainMenu;
    public int Score { get; private set; }
    public int CurrentWave { get; private set; }
    public int HighScore { get; private set; }

    // --- Settings ---
    [Header("Game Settings")]
    public float gameBoundsX = 8f;
    public float gameBoundsY = 5f;
    public int startingPlayerHealth = 5;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        HighScore = PlayerPrefs.GetInt("HighScore", 0);
    }

    private void Start()
    {
        // If we're in the Game scene already, start playing
        if (SceneManager.GetActiveScene().name == "GameScene")
        {
            StartGame();
        }
    }

    public void ChangeState(GameState newState)
    {
        GameState oldState = CurrentState;
        CurrentState = newState;

        switch (newState)
        {
            case GameState.Playing:
                Time.timeScale = 1f;
                break;
            case GameState.Paused:
                Time.timeScale = 0f;
                break;
            case GameState.GameOver:
                Time.timeScale = 1f;
                SaveHighScore();
                break;
            case GameState.MainMenu:
                Time.timeScale = 1f;
                break;
        }

        OnGameStateChanged?.Invoke(newState);
    }

    public void StartGame()
    {
        Score = 0;
        CurrentWave = 0;
        OnScoreChanged?.Invoke(Score);
        ChangeState(GameState.Playing);
    }

    public void AddScore(int points)
    {
        if (CurrentState != GameState.Playing) return;
        Score += points;
        OnScoreChanged?.Invoke(Score);
    }

    public void SetWave(int wave)
    {
        CurrentWave = wave;
        OnWaveChanged?.Invoke(wave);
    }

    public void NotifyPlayerHealthChanged(int current, int max)
    {
        OnPlayerHealthChanged?.Invoke(current, max);
    }

    public void PlayerDied()
    {
        ChangeState(GameState.GameOver);
    }

    public void PauseGame()
    {
        if (CurrentState == GameState.Playing)
            ChangeState(GameState.Paused);
    }

    public void ResumeGame()
    {
        if (CurrentState == GameState.Paused)
            ChangeState(GameState.Playing);
    }

    public void TogglePause()
    {
        if (CurrentState == GameState.Playing)
            PauseGame();
        else if (CurrentState == GameState.Paused)
            ResumeGame();
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("GameScene");
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        ChangeState(GameState.MainMenu);
        SceneManager.LoadScene("MainMenuScene");
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void SaveHighScore()
    {
        if (Score > HighScore)
        {
            HighScore = Score;
            PlayerPrefs.SetInt("HighScore", HighScore);
            PlayerPrefs.Save();
        }
    }

    /// <summary>
    /// Check if a position is within game bounds (with optional padding).
    /// </summary>
    public bool IsInBounds(Vector2 position, float padding = 1f)
    {
        return Mathf.Abs(position.x) <= gameBoundsX + padding &&
               Mathf.Abs(position.y) <= gameBoundsY + padding;
    }

    /// <summary>
    /// Get a random spawn position along the top edge.
    /// </summary>
    public Vector2 GetRandomTopSpawnPosition()
    {
        float x = UnityEngine.Random.Range(-gameBoundsX + 1f, gameBoundsX - 1f);
        return new Vector2(x, gameBoundsY + 1f);
    }
}

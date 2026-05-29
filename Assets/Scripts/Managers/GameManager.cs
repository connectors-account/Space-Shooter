using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Central game state manager: scoring, wave tracking, game over, restart.
/// Singleton — persists as the authority for the Game scene.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Settings")]
    public int totalWaves = 10;

    // State
    public int Score { get; private set; }
    public int HighScore { get; private set; }
    public int CurrentWave { get; private set; }
    public bool IsGameOver { get; private set; }
    public bool IsPaused { get; private set; }

    private const string HighScoreKey = "HighScore";

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        HighScore = PlayerPrefs.GetInt(HighScoreKey, 0);
    }

    void Start()
    {
        StartGame();
    }

    public void StartGame()
    {
        Score = 0;
        CurrentWave = 0;
        IsGameOver = false;
        IsPaused = false;
        Time.timeScale = 1f;

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateScore(Score);
            UIManager.Instance.UpdateWave(CurrentWave);
            UIManager.Instance.HideGameOver();
        }
    }

    public void AddScore(int points)
    {
        if (IsGameOver) return;

        Score += points;

        if (Score > HighScore)
        {
            HighScore = Score;
            PlayerPrefs.SetInt(HighScoreKey, HighScore);
            PlayerPrefs.Save();
        }

        UIManager.Instance?.UpdateScore(Score);
    }

    public void SetWave(int wave)
    {
        CurrentWave = wave;
        UIManager.Instance?.UpdateWave(CurrentWave);
    }

    public void PlayerDied()
    {
        IsGameOver = true;
        Time.timeScale = 0f;

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowGameOver(Score, HighScore);
        }
    }

    public void AllWavesComplete()
    {
        IsGameOver = true;
        Time.timeScale = 0f;

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowVictory(Score, HighScore);
        }
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Game");
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public void TogglePause()
    {
        if (IsGameOver) return;

        IsPaused = !IsPaused;
        Time.timeScale = IsPaused ? 0f : 1f;

        UIManager.Instance?.ShowPauseMenu(IsPaused);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}

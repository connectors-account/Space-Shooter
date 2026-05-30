using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Central game state manager — scoring, waves, lives, game flow.
/// Attach to an empty GameObject named "GameManager" in the scene.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Settings")]
    public int startingLives = 3;
    public float respawnDelay = 2f;

    // ── Runtime state ──
    public int Score { get; private set; }
    public int Lives { get; private set; }
    public int CurrentWave { get; private set; }
    public bool IsGameOver { get; private set; }
    public bool IsPaused { get; private set; }
    public bool IsPlaying { get; private set; }

    // Events other scripts can subscribe to
    public System.Action<int> OnScoreChanged;
    public System.Action<int> OnLivesChanged;
    public System.Action<int> OnWaveChanged;
    public System.Action OnGameOver;
    public System.Action OnGameStarted;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    // Called from UIManager "Play" button
    public void StartGame()
    {
        Score = 0;
        Lives = startingLives;
        CurrentWave = 0;
        IsGameOver = false;
        IsPaused = false;
        IsPlaying = true;
        Time.timeScale = 1f;

        OnScoreChanged?.Invoke(Score);
        OnLivesChanged?.Invoke(Lives);
        OnGameStarted?.Invoke();
    }

    public void AddScore(int points)
    {
        if (IsGameOver) return;
        Score += points;
        OnScoreChanged?.Invoke(Score);
    }

    public void LoseLife()
    {
        if (IsGameOver) return;
        Lives--;
        OnLivesChanged?.Invoke(Lives);
        if (Lives <= 0)
        {
            IsGameOver = true;
            IsPlaying = false;
            OnGameOver?.Invoke();
        }
    }

    public void SetWave(int wave)
    {
        CurrentWave = wave;
        OnWaveChanged?.Invoke(wave);
    }

    public void TogglePause()
    {
        if (IsGameOver || !IsPlaying) return;
        IsPaused = !IsPaused;
        Time.timeScale = IsPaused ? 0f : 1f;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}

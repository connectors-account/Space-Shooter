using System;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Central game state manager.
/// Tracks score, player health state updates, and game-over flow.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Scene Names")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private string gameplaySceneName = "GameScene";

    public int Score { get; private set; }
    public bool IsGameOver { get; private set; }

    public event Action<int> OnScoreChanged;
    public event Action<int, int> OnPlayerHealthChanged;
    public event Action OnGameOver;

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
        Time.timeScale = 1f;
        Score = 0;
        IsGameOver = false;
        OnScoreChanged?.Invoke(Score);
    }

    public void AddScore(int amount)
    {
        if (IsGameOver)
        {
            return;
        }

        Score += Mathf.Max(0, amount);
        OnScoreChanged?.Invoke(Score);
    }

    public void ReportPlayerHealth(int currentHealth, int maxHealth)
    {
        OnPlayerHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void PlayerDied()
    {
        if (IsGameOver)
        {
            return;
        }

        IsGameOver = true;
        OnGameOver?.Invoke();
        Time.timeScale = 0f;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void StartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(gameplaySceneName);
    }
}

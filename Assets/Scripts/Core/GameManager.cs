using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameState { MainMenu, Playing, Paused, GameOver, Victory }
    public GameState CurrentState { get; private set; } = GameState.MainMenu;

    // Game settings
    [Header("Game Settings")]
    public int startingLives = 3;
    public float invincibilityDuration = 2f;

    // Events
    public static event Action<GameState> OnGameStateChanged;
    public static event Action OnGameStart;
    public static event Action OnGameOver;
    public static event Action OnVictory;
    public static event Action OnGamePaused;
    public static event Action OnGameResumed;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Check if we're in the game scene
        if (SceneManager.GetActiveScene().name == "GameScene")
        {
            StartGame();
        }
    }

    private void Update()
    {
        // Handle pause input
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (CurrentState == GameState.Playing)
            {
                PauseGame();
            }
            else if (CurrentState == GameState.Paused)
            {
                ResumeGame();
            }
        }
    }

    public void SetState(GameState newState)
    {
        CurrentState = newState;
        OnGameStateChanged?.Invoke(newState);

        switch (newState)
        {
            case GameState.Playing:
                Time.timeScale = 1f;
                break;
            case GameState.Paused:
                Time.timeScale = 0f;
                break;
            case GameState.GameOver:
            case GameState.Victory:
                Time.timeScale = 0f;
                break;
        }
    }

    public void StartGame()
    {
        SetState(GameState.Playing);
        OnGameStart?.Invoke();
        ScoreManager.Instance?.ResetScore();
    }

    public void PauseGame()
    {
        SetState(GameState.Paused);
        OnGamePaused?.Invoke();
    }

    public void ResumeGame()
    {
        SetState(GameState.Playing);
        OnGameResumed?.Invoke();
    }

    public void GameOver()
    {
        SetState(GameState.GameOver);
        OnGameOver?.Invoke();
    }

    public void Victory()
    {
        SetState(GameState.Victory);
        OnVictory?.Invoke();
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        SetState(GameState.MainMenu);
        SceneManager.LoadScene("MainMenu");
    }

    public void LoadGameScene()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("GameScene");
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("GameScene");
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

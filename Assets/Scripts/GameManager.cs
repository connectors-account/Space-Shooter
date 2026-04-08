using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Central game state controller. Singleton that persists across scenes.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameState { MainMenu, Playing, Paused, GameOver }
    public GameState CurrentState { get; private set; } = GameState.MainMenu;

    [Header("References (set in GamePlay scene)")]
    public EnemySpawner enemySpawner;
    public PlayerController player;

    private int score;
    private int highScore;
    private const string HIGH_SCORE_KEY = "HighScore";

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        highScore = PlayerPrefs.GetInt(HIGH_SCORE_KEY, 0);
    }

    /// <summary>Call from the Main Menu "Play" button.</summary>
    public void StartGame()
    {
        score = 0;
        CurrentState = GameState.Playing;
        SceneManager.LoadScene("GamePlay");
    }

    /// <summary>Called after the GamePlay scene loads to wire references.</summary>
    public void OnGamePlaySceneLoaded(EnemySpawner spawner, PlayerController playerCtrl)
    {
        enemySpawner = spawner;
        player = playerCtrl;

        CurrentState = GameState.Playing;
        Time.timeScale = 1f;

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateScore(score);
            UIManager.Instance.UpdateHealth(playerCtrl.maxHealth, playerCtrl.maxHealth);
        }

        if (enemySpawner != null)
            enemySpawner.StartSpawning();
    }

    public void AddScore(int amount)
    {
        score += amount;
        if (UIManager.Instance != null)
            UIManager.Instance.UpdateScore(score);
    }

    public int GetScore() => score;
    public int GetHighScore() => highScore;

    public void TogglePause()
    {
        if (CurrentState == GameState.Playing)
        {
            CurrentState = GameState.Paused;
            Time.timeScale = 0f;
            if (UIManager.Instance != null)
                UIManager.Instance.ShowPauseMenu(true);
        }
        else if (CurrentState == GameState.Paused)
        {
            CurrentState = GameState.Playing;
            Time.timeScale = 1f;
            if (UIManager.Instance != null)
                UIManager.Instance.ShowPauseMenu(false);
        }
    }

    public void GameOver()
    {
        CurrentState = GameState.GameOver;

        if (enemySpawner != null)
            enemySpawner.StopSpawning();

        // Update high score
        if (score > highScore)
        {
            highScore = score;
            PlayerPrefs.SetInt(HIGH_SCORE_KEY, highScore);
            PlayerPrefs.Save();
        }

        // Load GameOver scene after short delay
        Invoke(nameof(LoadGameOverScene), 1.5f);
    }

    void LoadGameOverScene()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("GameOver");
    }

    public void ReturnToMainMenu()
    {
        CurrentState = GameState.MainMenu;
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
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

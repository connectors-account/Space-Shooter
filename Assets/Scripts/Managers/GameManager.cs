using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Central game manager: handles game state, scoring, wave progression, and lives.
/// Singleton pattern - persists across scenes.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameState { MainMenu, Playing, Paused, GameOver }

    [Header("Game Settings")]
    [SerializeField] private int startingLives = 3;
    [SerializeField] private float difficultyScaleRate = 0.1f;

    // Game state
    private GameState currentState = GameState.MainMenu;
    private int score;
    private int highScore;
    private int currentWave;
    private int lives;
    private float difficultyMultiplier = 1f;
    private float gameTime;

    // Properties
    public GameState CurrentState => currentState;
    public bool IsPlaying => currentState == GameState.Playing;
    public int Score => score;
    public int HighScore => highScore;
    public int CurrentWave => currentWave;
    public int Lives => lives;
    public float DifficultyMultiplier => difficultyMultiplier;
    public float GameTime => gameTime;

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

    private void Update()
    {
        if (currentState == GameState.Playing)
        {
            gameTime += Time.deltaTime;
            difficultyMultiplier = 1f + (gameTime * difficultyScaleRate / 60f);
        }

        // Pause toggle
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (currentState == GameState.Playing)
                PauseGame();
            else if (currentState == GameState.Paused)
                ResumeGame();
        }
    }

    public void StartGame()
    {
        score = 0;
        currentWave = 0;
        lives = startingLives;
        gameTime = 0f;
        difficultyMultiplier = 1f;
        currentState = GameState.Playing;
        Time.timeScale = 1f;

        SceneManager.LoadScene("GameScene");
    }

    public void PauseGame()
    {
        if (currentState != GameState.Playing) return;
        currentState = GameState.Paused;
        Time.timeScale = 0f;
        UIManager.Instance?.ShowPauseMenu(true);
    }

    public void ResumeGame()
    {
        if (currentState != GameState.Paused) return;
        currentState = GameState.Playing;
        Time.timeScale = 1f;
        UIManager.Instance?.ShowPauseMenu(false);
    }

    public void GameOver()
    {
        currentState = GameState.GameOver;
        Time.timeScale = 1f;

        if (score > highScore)
        {
            highScore = score;
            PlayerPrefs.SetInt("HighScore", highScore);
            PlayerPrefs.Save();
        }

        UIManager.Instance?.ShowGameOver(score, highScore);
    }

    public void ReturnToMainMenu()
    {
        currentState = GameState.MainMenu;
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public void RestartGame()
    {
        StartGame();
    }

    public void AddScore(int points)
    {
        score += Mathf.RoundToInt(points * difficultyMultiplier);
        UIManager.Instance?.UpdateScore(score);
    }

    public void OnWaveStarted(int waveNumber)
    {
        currentWave = waveNumber;
        UIManager.Instance?.UpdateWave(waveNumber);
        UIManager.Instance?.ShowWaveAnnouncement(waveNumber);
    }

    public void OnPlayerHealthChanged(int current, int max)
    {
        UIManager.Instance?.UpdateHealth(current, max);
    }

    public void OnPlayerDeath()
    {
        lives--;
        if (lives <= 0)
        {
            GameOver();
        }
        else
        {
            UIManager.Instance?.UpdateLives(lives);
            // Respawn player after delay
            StartCoroutine(RespawnPlayer());
        }
    }

    private System.Collections.IEnumerator RespawnPlayer()
    {
        yield return new WaitForSeconds(2f);
        if (currentState == GameState.Playing)
        {
            PlayerController player = FindAnyObjectByType<PlayerController>();
            if (player != null)
            {
                player.ResetPlayer();
            }
        }
    }

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

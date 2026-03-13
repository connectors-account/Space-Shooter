using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Central game manager - handles game state, scoring, lives, and overall game flow.
/// Singleton pattern ensures only one instance exists.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Settings")]
    public int startingLives = 3;
    public float respawnDelay = 2f;
    public float invincibilityDuration = 3f;

    [Header("Game State")]
    public bool isGameActive = false;
    public bool isPaused = false;

    // Score
    private int currentScore = 0;
    private int highScore = 0;
    private int comboCount = 0;
    private float comboTimer = 0f;
    private float comboTimeWindow = 2f;
    private int comboMultiplier = 1;

    // Lives
    private int currentLives;

    // Events
    public System.Action<int> OnScoreChanged;
    public System.Action<int> OnLivesChanged;
    public System.Action<int> OnComboChanged;
    public System.Action OnGameOver;
    public System.Action OnGameStart;
    public System.Action<bool> OnGamePaused;

    public int CurrentScore => currentScore;
    public int HighScore => highScore;
    public int CurrentLives => currentLives;
    public int ComboMultiplier => comboMultiplier;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadHighScore();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (!isGameActive) return;

        // Handle pause input
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }

        // Update combo timer
        if (comboTimer > 0)
        {
            comboTimer -= Time.unscaledDeltaTime;
            if (comboTimer <= 0)
            {
                ResetCombo();
            }
        }
    }

    public void StartGame()
    {
        currentScore = 0;
        currentLives = startingLives;
        comboCount = 0;
        comboMultiplier = 1;
        comboTimer = 0f;
        isGameActive = true;
        isPaused = false;
        Time.timeScale = 1f;

        OnScoreChanged?.Invoke(currentScore);
        OnLivesChanged?.Invoke(currentLives);
        OnComboChanged?.Invoke(comboMultiplier);
        OnGameStart?.Invoke();

        SceneManager.LoadScene("GameScene");
    }

    public void AddScore(int basePoints)
    {
        if (!isGameActive) return;

        // Apply combo multiplier
        int points = basePoints * comboMultiplier;
        currentScore += points;

        // Update combo
        comboCount++;
        comboTimer = comboTimeWindow;
        if (comboCount >= 10)
            comboMultiplier = 4;
        else if (comboCount >= 5)
            comboMultiplier = 3;
        else if (comboCount >= 3)
            comboMultiplier = 2;
        else
            comboMultiplier = 1;

        OnScoreChanged?.Invoke(currentScore);
        OnComboChanged?.Invoke(comboMultiplier);

        // Update high score
        if (currentScore > highScore)
        {
            highScore = currentScore;
            SaveHighScore();
        }
    }

    private void ResetCombo()
    {
        comboCount = 0;
        comboMultiplier = 1;
        OnComboChanged?.Invoke(comboMultiplier);
    }

    public void PlayerDied()
    {
        currentLives--;
        OnLivesChanged?.Invoke(currentLives);

        if (currentLives <= 0)
        {
            GameOver();
        }
        else
        {
            // Respawn handled by PlayerController
        }
    }

    public void AddLife()
    {
        currentLives++;
        OnLivesChanged?.Invoke(currentLives);
    }

    private void GameOver()
    {
        isGameActive = false;
        SaveHighScore();
        OnGameOver?.Invoke();
    }

    public void TogglePause()
    {
        if (!isGameActive) return;

        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0f : 1f;
        OnGamePaused?.Invoke(isPaused);
    }

    public void ReturnToMainMenu()
    {
        isGameActive = false;
        isPaused = false;
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public void RestartGame()
    {
        StartGame();
    }

    private void SaveHighScore()
    {
        PlayerPrefs.SetInt("HighScore", highScore);
        PlayerPrefs.Save();
    }

    private void LoadHighScore()
    {
        highScore = PlayerPrefs.GetInt("HighScore", 0);
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

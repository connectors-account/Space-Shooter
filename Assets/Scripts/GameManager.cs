using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages overall game state, score, and game flow.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Settings")]
    [SerializeField] private int startingScore = 0;

    private int currentScore;
    private bool isGameOver = false;
    private bool isPaused = false;

    public int CurrentScore => currentScore;
    public bool IsGameOver => isGameOver;
    public bool IsPaused => isPaused;

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        StartGame();
    }

    private void Update()
    {
        HandlePauseInput();
        HandleRestartInput();
    }

    private void HandlePauseInput()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !isGameOver)
        {
            TogglePause();
        }
    }

    private void HandleRestartInput()
    {
        if (isGameOver && Input.GetKeyDown(KeyCode.R))
        {
            RestartGame();
        }
    }

    public void StartGame()
    {
        currentScore = startingScore;
        isGameOver = false;
        isPaused = false;
        Time.timeScale = 1f;

        UpdateScoreUI();

        if (UIManager.Instance != null)
        {
            UIManager.Instance.HideGameOver();
            UIManager.Instance.HidePauseMenu();
        }
    }

    public void AddScore(int points)
    {
        if (isGameOver)
            return;

        currentScore += points;
        UpdateScoreUI();
    }

    private void UpdateScoreUI()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateScore(currentScore);
        }
    }

    public void GameOver()
    {
        if (isGameOver)
            return;

        isGameOver = true;
        Time.timeScale = 0f;

        // Save high score
        int highScore = PlayerPrefs.GetInt("HighScore", 0);
        if (currentScore > highScore)
        {
            PlayerPrefs.SetInt("HighScore", currentScore);
            PlayerPrefs.Save();
        }

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowGameOver(currentScore, PlayerPrefs.GetInt("HighScore", 0));
        }

        Debug.Log($"Game Over! Final Score: {currentScore}");
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void TogglePause()
    {
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0f : 1f;

        if (UIManager.Instance != null)
        {
            if (isPaused)
            {
                UIManager.Instance.ShowPauseMenu();
            }
            else
            {
                UIManager.Instance.HidePauseMenu();
            }
        }
    }

    public void QuitGame()
    {
        Debug.Log("Quitting game...");
        
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}

using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game State")]
    public bool isGameOver = false;
    public bool isPaused = false;

    [Header("References")]
    public GameObject playerPrefab;
    public Transform playerSpawnPoint;

    private GameObject currentPlayer;

    void Awake()
    {
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

    void Start()
    {
        StartGame();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isGameOver)
            {
                LoadMainMenu();
            }
            else
            {
                TogglePause();
            }
        }

        // Quick restart with R key
        if (Input.GetKeyDown(KeyCode.R) && isGameOver)
        {
            RestartGame();
        }
    }

    public void StartGame()
    {
        isGameOver = false;
        isPaused = false;
        Time.timeScale = 1f;

        // Spawn player if prefab is assigned
        if (playerPrefab != null && playerSpawnPoint != null)
        {
            currentPlayer = Instantiate(playerPrefab, playerSpawnPoint.position, Quaternion.identity);
        }
        else if (playerPrefab != null)
        {
            currentPlayer = Instantiate(playerPrefab, new Vector3(0f, -3f, 0f), Quaternion.identity);
        }

        // Start wave system
        WaveManager.Instance?.StartWaves();

        // Reset score
        ScoreManager.Instance?.ResetScore();
    }

    public void GameOver()
    {
        if (isGameOver) return;

        isGameOver = true;
        Time.timeScale = 0f;

        // Stop spawning
        EnemySpawner.Instance?.StopSpawning();

        // Show game over UI
        UIManager.Instance?.ShowGameOverScreen();
    }

    public void TogglePause()
    {
        if (isGameOver) return;

        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0f : 1f;

        UIManager.Instance?.ShowPauseMenu(isPaused);
    }

    public void PauseGame()
    {
        if (isGameOver) return;
        
        isPaused = true;
        Time.timeScale = 0f;
        UIManager.Instance?.ShowPauseMenu(true);
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        UIManager.Instance?.ShowPauseMenu(false);
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public void LoadGameScene()
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

    public bool IsGamePaused()
    {
        return isPaused || isGameOver;
    }

    public bool IsGameOver()
    {
        return isGameOver;
    }

    public GameObject GetPlayer()
    {
        return currentPlayer;
    }
}

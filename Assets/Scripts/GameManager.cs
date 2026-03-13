using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState
{
    Menu,
    Playing,
    Paused,
    GameOver,
    Victory
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Settings")]
    public int startingLives = 3;

    private int score = 0;
    private int highScore = 0;
    private int currentWave = 1;
    private int playerHealth;
    private GameState gameState = GameState.Menu;

    private UIManager uiManager;
    private SpawnManager spawnManager;
    private AudioManager audioManager;
    private PlayerController player;

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

        highScore = PlayerPrefs.GetInt("HighScore", 0);
    }

    void Start()
    {
        uiManager = FindObjectOfType<UIManager>();
        spawnManager = FindObjectOfType<SpawnManager>();
        audioManager = FindObjectOfType<AudioManager>();
        player = FindObjectOfType<PlayerController>();

        if (player != null)
        {
            playerHealth = player.maxHealth;
            player.gameObject.SetActive(false);
        }

        ShowMainMenu();
    }

    void Update()
    {
        if (gameState == GameState.Playing && Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }

        if ((gameState == GameState.GameOver || gameState == GameState.Victory) && Input.GetKeyDown(KeyCode.R))
        {
            RestartGame();
        }

        if (gameState == GameState.Menu && Input.GetKeyDown(KeyCode.Return))
        {
            StartGame();
        }
    }

    public void ShowMainMenu()
    {
        gameState = GameState.Menu;
        if (uiManager != null)
            uiManager.ShowMainMenu();
    }

    public void StartGame()
    {
        gameState = GameState.Playing;
        score = 0;
        currentWave = 1;

        if (player != null)
        {
            player.gameObject.SetActive(true);
            player.transform.position = new Vector3(0, -3f, 0);
            playerHealth = player.maxHealth;
        }

        if (uiManager != null)
        {
            uiManager.ShowGameHUD();
            uiManager.UpdateScore(score);
            uiManager.UpdateHealth(playerHealth);
            uiManager.UpdateWave(currentWave);
        }

        if (spawnManager != null)
            spawnManager.StartSpawning();

        if (audioManager != null)
            audioManager.PlayBackgroundMusic();
    }

    public void TogglePause()
    {
        if (gameState == GameState.Playing)
        {
            gameState = GameState.Paused;
            Time.timeScale = 0f;
            if (uiManager != null)
                uiManager.ShowPauseMenu();
        }
        else if (gameState == GameState.Paused)
        {
            ResumeGame();
        }
    }

    public void ResumeGame()
    {
        gameState = GameState.Playing;
        Time.timeScale = 1f;
        if (uiManager != null)
            uiManager.ShowGameHUD();
    }

    public void GameOver()
    {
        gameState = GameState.GameOver;
        Time.timeScale = 1f;

        if (spawnManager != null)
            spawnManager.StopSpawning();

        if (score > highScore)
        {
            highScore = score;
            PlayerPrefs.SetInt("HighScore", highScore);
            PlayerPrefs.Save();
        }

        if (uiManager != null)
            uiManager.ShowGameOver(score, highScore);

        if (audioManager != null)
            audioManager.StopBackgroundMusic();
    }

    public void Victory()
    {
        gameState = GameState.Victory;
        Time.timeScale = 1f;

        if (spawnManager != null)
            spawnManager.StopSpawning();

        if (score > highScore)
        {
            highScore = score;
            PlayerPrefs.SetInt("HighScore", highScore);
            PlayerPrefs.Save();
        }

        if (uiManager != null)
            uiManager.ShowVictory(score, highScore);

        if (audioManager != null)
            audioManager.PlayVictorySound();
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        ClearAllEnemiesAndBullets();
        
        PlayerController playerController = FindObjectOfType<PlayerController>(true);
        if (playerController != null)
        {
            playerController.gameObject.SetActive(true);
        }
        
        player = playerController;
        StartGame();
    }

    void ClearAllEnemiesAndBullets()
    {
        foreach (GameObject enemy in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            Destroy(enemy);
        }
        foreach (GameObject bullet in GameObject.FindGameObjectsWithTag("Bullet"))
        {
            Destroy(bullet);
        }
        foreach (GameObject powerUp in GameObject.FindGameObjectsWithTag("PowerUp"))
        {
            Destroy(powerUp);
        }
    }

    public void QuitGame()
    {
        PlayerPrefs.Save();
        Application.Quit();

        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    public void AddScore(int points)
    {
        score += points;
        if (uiManager != null)
            uiManager.UpdateScore(score);
    }

    public void UpdatePlayerHealth(int health)
    {
        playerHealth = health;
        if (uiManager != null)
            uiManager.UpdateHealth(playerHealth);
    }

    public void SetCurrentWave(int wave)
    {
        currentWave = wave;
        if (uiManager != null)
            uiManager.UpdateWave(currentWave);
    }

    public bool IsGameActive()
    {
        return gameState == GameState.Playing;
    }

    public GameState GetGameState()
    {
        return gameState;
    }

    public int GetScore()
    {
        return score;
    }

    public int GetHighScore()
    {
        return highScore;
    }
}

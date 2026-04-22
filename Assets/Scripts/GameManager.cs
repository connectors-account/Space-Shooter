using UnityEngine;

public class GameManager : MonoBehaviour
{
    public enum GameState
    {
        MainMenu,
        Playing,
        Paused,
        GameOver,
        Victory
    }

    public static GameManager Instance { get; private set; }

    [Header("Wave Setup")]
    [SerializeField] private int totalWaves = 5;

    [Header("Start Behavior")]
    [SerializeField] private bool autoStartGameOnSceneLoad = true;

    [Header("Scene References")]
    [SerializeField] private SpawnManager spawnManager;
    [SerializeField] private UIManager uiManager;
    [SerializeField] private MenuManager menuManager;
    [SerializeField] private PlayerController player;

    private GameState currentState = GameState.MainMenu;
    private int score;
    private int currentWave;

    public int Score => score;
    public int CurrentWave => currentWave;
    public int TotalWaves => totalWaves;
    public GameState CurrentState => currentState;

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
        uiManager?.UpdateScore(0);
        uiManager?.UpdateWave(0, totalWaves);

        if (autoStartGameOnSceneLoad)
        {
            StartNewGame();
            return;
        }

        SetState(GameState.MainMenu);
        menuManager?.ShowMainMenu();
        uiManager?.SetHudVisible(false);
    }

    private void Update()
    {
        if (currentState == GameState.Playing && Input.GetKeyDown(KeyCode.Escape))
        {
            PauseGame();
        }
        else if (currentState == GameState.Paused && Input.GetKeyDown(KeyCode.Escape))
        {
            ResumeGame();
        }
    }

    public void StartNewGame()
    {
        score = 0;
        currentWave = 0;

        spawnManager?.ClearAllEnemies();
        player?.ResetPlayer();

        uiManager?.SetHudVisible(true);
        uiManager?.UpdateScore(score);
        uiManager?.UpdateWave(0, totalWaves);

        SetState(GameState.Playing);
        menuManager?.HideAllMenus();

        StartNextWave();
    }

    public void StartNextWave()
    {
        if (currentState != GameState.Playing)
        {
            return;
        }

        currentWave++;

        if (currentWave > totalWaves)
        {
            HandleVictory();
            return;
        }

        uiManager?.UpdateWave(currentWave, totalWaves);
        uiManager?.ShowMessage($"Wave {currentWave}", 1.25f);
        AudioManager.Instance?.PlayWaveStart();

        spawnManager?.BeginWave(currentWave);
    }

    public void AddScore(int points)
    {
        if (currentState != GameState.Playing)
        {
            return;
        }

        score += points;
        uiManager?.UpdateScore(score);
    }

    public void OnWaveCleared()
    {
        if (currentState != GameState.Playing)
        {
            return;
        }

        StartNextWave();
    }

    public void OnPlayerDeath()
    {
        if (currentState != GameState.Playing)
        {
            return;
        }

        SetState(GameState.GameOver);
        spawnManager?.StopSpawning();
        menuManager?.ShowGameOver(score, currentWave);
        uiManager?.SetHudVisible(false);
        AudioManager.Instance?.PlayGameOver();
    }

    public void PauseGame()
    {
        if (currentState != GameState.Playing)
        {
            return;
        }

        SetState(GameState.Paused);
        Time.timeScale = 0f;
        menuManager?.ShowPauseMenu();
    }

    public void ResumeGame()
    {
        if (currentState != GameState.Paused)
        {
            return;
        }

        Time.timeScale = 1f;
        SetState(GameState.Playing);
        menuManager?.HidePauseMenu();
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        spawnManager?.ClearAllEnemies();
        uiManager?.SetHudVisible(false);
        menuManager?.ShowMainMenu();
        SetState(GameState.MainMenu);
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        StartNewGame();
    }

    private void HandleVictory()
    {
        SetState(GameState.Victory);
        spawnManager?.StopSpawning();
        menuManager?.ShowVictory(score);
        uiManager?.SetHudVisible(false);
    }

    private void SetState(GameState state)
    {
        currentState = state;
    }
}

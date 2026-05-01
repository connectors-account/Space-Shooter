using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public enum GameState
    {
        MainMenu,
        Playing,
        Paused,
        GameOver
    }

    public static GameManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private SpawnManager spawnManager;
    [SerializeField] private UIManager uiManager;
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform playerSpawnPoint;

    [Header("Wave")]
    [SerializeField] private float nextWaveDelay = 2f;

    public GameState CurrentState { get; private set; } = GameState.MainMenu;

    private int score;
    private int currentWave;
    private int enemiesAlive;
    private bool waveSpawnComplete;
    private PlayerController playerInstance;

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
        CurrentState = GameState.MainMenu;
        uiManager?.ShowMainMenu();
        Time.timeScale = 1f;
    }

    private void Update()
    {
        if (CurrentState == GameState.Playing && Input.GetKeyDown(KeyCode.Escape))
        {
            PauseGame();
        }
        else if (CurrentState == GameState.Paused && Input.GetKeyDown(KeyCode.Escape))
        {
            ResumeGame();
        }
    }

    public void StartGame()
    {
        score = 0;
        currentWave = 0;
        enemiesAlive = 0;
        waveSpawnComplete = false;

        if (playerInstance != null)
        {
            Destroy(playerInstance.gameObject);
        }

        SpawnPlayer();

        CurrentState = GameState.Playing;
        uiManager?.ShowHUD();
        uiManager?.UpdateScore(score);

        StartNextWave();
    }

    private void SpawnPlayer()
    {
        if (playerPrefab == null || playerSpawnPoint == null)
        {
            Debug.LogError("Player prefab or spawn point is not assigned in GameManager.");
            return;
        }

        GameObject playerObj = Instantiate(playerPrefab, playerSpawnPoint.position, Quaternion.identity);
        playerInstance = playerObj.GetComponent<PlayerController>();
    }

    public void AddScore(int amount)
    {
        score += Mathf.Max(0, amount);
        uiManager?.UpdateScore(score);
    }

    public void NotifyEnemySpawned()
    {
        enemiesAlive++;
    }

    public void NotifyEnemyDestroyed(bool killedByPlayer)
    {
        enemiesAlive = Mathf.Max(0, enemiesAlive - 1);

        if (CurrentState == GameState.Playing && waveSpawnComplete && enemiesAlive == 0)
        {
            StartCoroutine(StartNextWaveAfterDelay());
        }
    }

    public void NotifyWaveSpawnComplete()
    {
        waveSpawnComplete = true;

        if (enemiesAlive == 0 && CurrentState == GameState.Playing)
        {
            StartCoroutine(StartNextWaveAfterDelay());
        }
    }

    private IEnumerator StartNextWaveAfterDelay()
    {
        waveSpawnComplete = false;
        yield return new WaitForSeconds(nextWaveDelay);

        if (CurrentState == GameState.Playing)
        {
            StartNextWave();
        }
    }

    private void StartNextWave()
    {
        currentWave++;
        uiManager?.UpdateWave(currentWave);
        spawnManager?.SpawnWave(currentWave);
    }

    public void PauseGame()
    {
        if (CurrentState != GameState.Playing)
        {
            return;
        }

        CurrentState = GameState.Paused;
        Time.timeScale = 0f;
        uiManager?.ShowPauseMenu();
    }

    public void ResumeGame()
    {
        if (CurrentState != GameState.Paused)
        {
            return;
        }

        CurrentState = GameState.Playing;
        Time.timeScale = 1f;
        uiManager?.HidePauseMenu();
    }

    public void GameOver()
    {
        if (CurrentState == GameState.GameOver)
        {
            return;
        }

        CurrentState = GameState.GameOver;
        Time.timeScale = 1f;
        spawnManager?.StopAllCoroutines();
        uiManager?.ShowGameOver(score);
        AudioManager.Instance?.PlayGameOver(); // Add game-over SFX clip in AudioManager inspector
    }

    public void BackToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}

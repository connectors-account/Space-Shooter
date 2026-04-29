using UnityEngine;

/// <summary>
/// Central game state controller: score, lives, pause, win/lose conditions.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Settings")]
    [SerializeField] private int startingLives = 3;

    [Header("Scene References")]
    [SerializeField] private SpawnManager spawnManager;
    [SerializeField] private UIManager uiManager;

    private int score;
    private int lives;
    private bool isGameOver;
    private bool isPaused;

    public int Score => score;
    public int Lives => lives;
    public bool IsGameOver => isGameOver;
    public bool IsPaused => isPaused;

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
        ShowMainMenu();
    }

    private void Update()
    {
        if (isGameOver)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    public void StartNewRun()
    {
        score = 0;
        lives = Mathf.Max(1, startingLives);
        isGameOver = false;
        SetPause(false);

        uiManager?.ShowGameplayHUD();
        uiManager?.UpdateScore(score);
        uiManager?.UpdateLives(lives);

        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            player.ResetForNewRun();
        }

        if (spawnManager != null)
        {
            spawnManager.BeginSpawning();
        }
    }

    public void AddScore(int amount)
    {
        if (isGameOver)
        {
            return;
        }

        score += Mathf.Max(0, amount);
        uiManager?.UpdateScore(score);
    }

    public void PlayerLostLife()
    {
        if (isGameOver)
        {
            return;
        }

        lives--;
        uiManager?.UpdateLives(lives);

        if (lives <= 0)
        {
            TriggerGameOver(false);
        }
    }

    public void OnAllWavesCompleted()
    {
        if (isGameOver)
        {
            return;
        }

        TriggerGameOver(true);
    }

    public void TriggerGameOver(bool playerWon)
    {
        isGameOver = true;
        SetPause(false);
        uiManager?.ShowGameOver(playerWon, score);
    }

    public void TogglePause()
    {
        SetPause(!isPaused);
    }

    public void SetPause(bool pause)
    {
        if (isGameOver)
        {
            pause = false;
        }

        isPaused = pause;
        Time.timeScale = isPaused ? 0f : 1f;
        uiManager?.ShowPause(isPaused);
    }

    public void ShowMainMenu()
    {
        isGameOver = false;
        SetPause(false);
        uiManager?.ShowMainMenu();
    }
}

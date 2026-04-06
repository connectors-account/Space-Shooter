using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Central game manager singleton. Controls game state, scoring, and wave progression.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Settings")]
    public float enemySpawnRate = 2f;
    public int enemiesPerWave = 5;
    public float waveCooldown = 3f;

    // Game state
    public GameState CurrentState { get; private set; } = GameState.MainMenu;
    public int Score { get; private set; }
    public int HighScore { get; private set; }
    public int CurrentWave { get; private set; }
    public int EnemiesRemainingInWave { get; private set; }
    public int EnemiesAlive { get; private set; }

    // Events
    public System.Action<GameState> OnGameStateChanged;
    public System.Action<int> OnScoreChanged;
    public System.Action<int> OnWaveChanged;
    public System.Action OnGameOver;

    private float waveTimer;
    private bool waveInProgress;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        HighScore = PlayerPrefs.GetInt("HighScore", 0);
    }

    private void Update()
    {
        if (CurrentState != GameState.Playing) return;

        if (!waveInProgress && EnemiesAlive <= 0)
        {
            waveTimer -= Time.deltaTime;
            if (waveTimer <= 0f)
            {
                StartNextWave();
            }
        }
    }

    public void StartGame()
    {
        Score = 0;
        CurrentWave = 0;
        EnemiesAlive = 0;
        EnemiesRemainingInWave = 0;
        waveInProgress = false;
        waveTimer = 1f;

        OnScoreChanged?.Invoke(Score);
        ChangeState(GameState.Playing);
    }

    public void PauseGame()
    {
        if (CurrentState == GameState.Playing)
        {
            Time.timeScale = 0f;
            ChangeState(GameState.Paused);
        }
    }

    public void ResumeGame()
    {
        if (CurrentState == GameState.Paused)
        {
            Time.timeScale = 1f;
            ChangeState(GameState.Playing);
        }
    }

    public void GameOver()
    {
        if (Score > HighScore)
        {
            HighScore = Score;
            PlayerPrefs.SetInt("HighScore", HighScore);
            PlayerPrefs.Save();
        }
        ChangeState(GameState.GameOver);
        OnGameOver?.Invoke();
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        ChangeState(GameState.MainMenu);
    }

    public void AddScore(int points)
    {
        Score += points;
        OnScoreChanged?.Invoke(Score);
    }

    public void EnemyDestroyed()
    {
        EnemiesAlive = Mathf.Max(0, EnemiesAlive - 1);
        if (EnemiesAlive <= 0 && EnemiesRemainingInWave <= 0)
        {
            waveInProgress = false;
            waveTimer = waveCooldown;
        }
    }

    public void EnemySpawned()
    {
        EnemiesAlive++;
        EnemiesRemainingInWave--;
        if (EnemiesRemainingInWave <= 0)
        {
            waveInProgress = false;
        }
    }

    private void StartNextWave()
    {
        CurrentWave++;
        int totalEnemies = enemiesPerWave + (CurrentWave - 1) * 2;
        EnemiesRemainingInWave = totalEnemies;
        waveInProgress = true;
        OnWaveChanged?.Invoke(CurrentWave);

        EnemySpawner spawner = FindObjectOfType<EnemySpawner>();
        if (spawner != null)
        {
            spawner.BeginWave(CurrentWave, totalEnemies);
        }
    }

    private void ChangeState(GameState newState)
    {
        CurrentState = newState;
        OnGameStateChanged?.Invoke(newState);
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

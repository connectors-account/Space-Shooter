using UnityEngine;

public enum GameState
{
    MainMenu,
    Playing,
    Paused,
    GameOver
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Progression")]
    [SerializeField] private int baseEnemiesPerWave = 6;
    [SerializeField] private float enemyHealthScalePerWave = 0.15f;
    [SerializeField] private float enemySpeedScalePerWave = 0.1f;

    [Header("Runtime")]
    [SerializeField] private int currentWave = 1;
    [SerializeField] private int score = 0;

    private GameState currentState = GameState.MainMenu;

    public int CurrentWave => currentWave;
    public int Score => score;
    public GameState CurrentState => currentState;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        Time.timeScale = 1f;
    }

    private void Start()
    {
        UIManager.Instance?.ShowGameplayHUD(true);
        UIManager.Instance?.HideGameOver();
        UIManager.Instance?.UpdateScore(score);
        UIManager.Instance?.UpdateWave(currentWave);
        SetGameState(GameState.Playing);
        SpawnManager.Instance?.StartWave(currentWave);
    }

    public void SetGameState(GameState newState)
    {
        currentState = newState;

        switch (newState)
        {
            case GameState.Playing:
                Time.timeScale = 1f;
                UIManager.Instance?.ShowPauseMenu(false);
                break;

            case GameState.Paused:
                Time.timeScale = 0f;
                UIManager.Instance?.ShowPauseMenu(true);
                break;

            case GameState.GameOver:
                Time.timeScale = 0f;
                UIManager.Instance?.ShowGameOver(score, currentWave);
                break;
        }
    }

    public void AddScore(int points)
    {
        if (currentState != GameState.Playing)
        {
            return;
        }

        score += points;
        UIManager.Instance?.UpdateScore(score);
    }

    public void OnWaveCleared()
    {
        if (currentState != GameState.Playing)
        {
            return;
        }

        currentWave++;
        UIManager.Instance?.UpdateWave(currentWave);
        SpawnManager.Instance?.StartWave(currentWave);
    }

    public int GetEnemyCountForWave(int wave)
    {
        return baseEnemiesPerWave + (wave - 1) * 2;
    }

    public float GetEnemyHealthMultiplier()
    {
        return 1f + (currentWave - 1) * enemyHealthScalePerWave;
    }

    public float GetEnemySpeedMultiplier()
    {
        return 1f + (currentWave - 1) * enemySpeedScalePerWave;
    }

    public void TriggerGameOver()
    {
        if (currentState == GameState.GameOver)
        {
            return;
        }

        SetGameState(GameState.GameOver);
    }
}

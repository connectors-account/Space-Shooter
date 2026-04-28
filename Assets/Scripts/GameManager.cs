using UnityEngine;

public enum GameFlowState
{
    MainMenu,
    Playing,
    Paused,
    GameOver,
    Victory
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private EnemyManager enemyManager;

    public int Score { get; private set; }
    public GameFlowState CurrentState { get; private set; } = GameFlowState.MainMenu;
    public bool IsGameplayActive => CurrentState == GameFlowState.Playing;

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
        Time.timeScale = 1f;
        UIManager.Instance?.ShowMainMenu(true);
        UIManager.Instance?.ShowGameplayHud(false);
        UIManager.Instance?.ShowPauseMenu(false);
        UIManager.Instance?.ShowEndPanel(false, false, 0);
    }

    private void Update()
    {
        if (CurrentState == GameFlowState.Playing && Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
        else if (CurrentState == GameFlowState.Paused && Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    public void StartGame()
    {
        Score = 0;
        CurrentState = GameFlowState.Playing;
        Time.timeScale = 1f;

        UIManager.Instance?.SetScore(Score);
        UIManager.Instance?.SetWave(1, enemyManager != null ? enemyManager.TotalWaves : 5);
        UIManager.Instance?.ShowMainMenu(false);
        UIManager.Instance?.ShowPauseMenu(false);
        UIManager.Instance?.ShowGameplayHud(true);
        UIManager.Instance?.ShowEndPanel(false, false, 0);

        if (playerController != null)
        {
            playerController.gameObject.SetActive(true);
        }

        enemyManager?.BeginWaves();
        AudioManager.Instance?.PlayMusic();
    }

    public void TogglePause()
    {
        if (CurrentState == GameFlowState.Playing)
        {
            CurrentState = GameFlowState.Paused;
            Time.timeScale = 0f;
            UIManager.Instance?.ShowPauseMenu(true);
        }
        else if (CurrentState == GameFlowState.Paused)
        {
            CurrentState = GameFlowState.Playing;
            Time.timeScale = 1f;
            UIManager.Instance?.ShowPauseMenu(false);
        }
    }

    public void AddScore(int points)
    {
        Score += Mathf.Max(0, points);
        UIManager.Instance?.SetScore(Score);
    }

    public void OnWaveStarted(int waveNumber)
    {
        UIManager.Instance?.SetWave(waveNumber, enemyManager != null ? enemyManager.TotalWaves : 5);
        AudioManager.Instance?.PlaySfx(AudioSfx.WaveStart);
    }

    public void OnWaveCompleted(int waveNumber)
    {
        AddScore(250 + (waveNumber * 50));
    }

    public void OnAllWavesCompleted()
    {
        if (CurrentState != GameFlowState.Playing)
        {
            return;
        }

        CurrentState = GameFlowState.Victory;
        Time.timeScale = 0f;
        UIManager.Instance?.ShowEndPanel(true, true, Score);
        AudioManager.Instance?.PlaySfx(AudioSfx.Win);
    }

    public void OnPlayerDied()
    {
        if (CurrentState != GameFlowState.Playing)
        {
            return;
        }

        CurrentState = GameFlowState.GameOver;
        Time.timeScale = 0f;
        enemyManager?.StopWaves();
        UIManager.Instance?.ShowEndPanel(true, false, Score);
        AudioManager.Instance?.PlaySfx(AudioSfx.GameOver);
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

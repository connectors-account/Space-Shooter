using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Central game state manager. Handles scoring, lives, wave progression,
/// pause/resume and game-over flow.  Singleton – persists across scenes.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    // ── Serialised tunables ──────────────────────────────────────────
    [Header("Wave Settings")]
    [SerializeField] private int   startingWave        = 1;
    [SerializeField] private float waveCooldown        = 3f;

    [Header("Player Settings")]
    [SerializeField] private int   playerMaxHealth     = 5;
    [SerializeField] private float respawnDelay        = 2f;

    // ── Runtime state ────────────────────────────────────────────────
    public int   Score          { get; private set; }
    public int   CurrentWave    { get; private set; }
    public int   PlayerHealth   { get; private set; }
    public bool  IsGameOver     { get; private set; }
    public bool  IsPaused       { get; private set; }
    public int   PlayerMaxHealth => playerMaxHealth;

    // Events other systems can subscribe to
    public System.Action<int> OnScoreChanged;
    public System.Action<int> OnHealthChanged;
    public System.Action<int> OnWaveChanged;
    public System.Action      OnGameOver;
    public System.Action      OnGamePaused;
    public System.Action      OnGameResumed;

    // ── Unity lifecycle ──────────────────────────────────────────────
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        // Don't persist across scenes so reloading resets state cleanly
    }

    private void Start()
    {
        InitGame();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (IsGameOver) return;
            if (IsPaused) ResumeGame(); else PauseGame();
        }
    }

    // ── Public API ───────────────────────────────────────────────────
    public void InitGame()
    {
        Score        = 0;
        CurrentWave  = startingWave;
        PlayerHealth = playerMaxHealth;
        IsGameOver   = false;
        IsPaused     = false;
        Time.timeScale = 1f;

        OnScoreChanged?.Invoke(Score);
        OnHealthChanged?.Invoke(PlayerHealth);
        OnWaveChanged?.Invoke(CurrentWave);
    }

    public void AddScore(int points)
    {
        if (IsGameOver) return;
        Score += points;
        OnScoreChanged?.Invoke(Score);
    }

    public void TakeDamage(int amount)
    {
        if (IsGameOver) return;
        PlayerHealth = Mathf.Max(0, PlayerHealth - amount);
        OnHealthChanged?.Invoke(PlayerHealth);
        if (PlayerHealth <= 0) TriggerGameOver();
    }

    public void Heal(int amount)
    {
        PlayerHealth = Mathf.Min(playerMaxHealth, PlayerHealth + amount);
        OnHealthChanged?.Invoke(PlayerHealth);
    }

    public void AdvanceWave()
    {
        CurrentWave++;
        OnWaveChanged?.Invoke(CurrentWave);
    }

    public void TriggerGameOver()
    {
        if (IsGameOver) return;
        IsGameOver = true;
        OnGameOver?.Invoke();
    }

    public void PauseGame()
    {
        IsPaused = true;
        Time.timeScale = 0f;
        OnGamePaused?.Invoke();
    }

    public void ResumeGame()
    {
        IsPaused = false;
        Time.timeScale = 1f;
        OnGameResumed?.Invoke();
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("GameScene");
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
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

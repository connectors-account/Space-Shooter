using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Singleton that owns all global game state: score, health, scene transitions.
/// Persists across scenes via DontDestroyOnLoad.
/// </summary>
public class GameManager : MonoBehaviour
{
    // ── Singleton ─────────────────────────────────────────────────────────────
    public static GameManager Instance { get; private set; }

    // ── Inspector ──────────────────────────────────────────────────────────────
    [Header("Player Settings")]
    public int maxHealth = 3;

    // ── State ──────────────────────────────────────────────────────────────────
    private int  currentHealth;
    private int  score;
    private bool isGameOver;

    // ── Public Accessors ───────────────────────────────────────────────────────
    public bool IsGameOver    => isGameOver;
    public int  Score         => score;
    public int  CurrentHealth => currentHealth;
    public int  MaxHealth     => maxHealth;

    // ── Events (UI listens to these) ───────────────────────────────────────────
    public event System.Action<int> OnScoreChanged;
    public event System.Action<int> OnHealthChanged;
    public event System.Action      OnGameOver;

    // ── Unity ──────────────────────────────────────────────────────────────────
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start() => ResetGame();

    // ── Public API ─────────────────────────────────────────────────────────────
    public void ResetGame()
    {
        currentHealth = maxHealth;
        score         = 0;
        isGameOver    = false;
        OnScoreChanged?.Invoke(score);
        OnHealthChanged?.Invoke(currentHealth);
    }

    public void AddScore(int points)
    {
        if (isGameOver) return;
        score += points;
        OnScoreChanged?.Invoke(score);
    }

    public void TakeDamage(int amount)
    {
        if (isGameOver) return;
        currentHealth = Mathf.Max(0, currentHealth - amount);
        OnHealthChanged?.Invoke(currentHealth);
        if (currentHealth <= 0) TriggerGameOver();
    }

    public void LoadGame()
    {
        ResetGame();
        SceneManager.LoadScene("Game");
    }

    public void LoadMainMenu()
    {
        ResetGame();
        SceneManager.LoadScene("MainMenu");
    }

    public void QuitGame() => Application.Quit();

    // ── Private ────────────────────────────────────────────────────────────────
    void TriggerGameOver()
    {
        isGameOver = true;
        OnGameOver?.Invoke();
        Invoke(nameof(GoToGameOverScene), 2f);
    }

    void GoToGameOverScene() => SceneManager.LoadScene("GameOver");
}

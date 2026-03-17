using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Central game state manager. Handles score, game flow, and state transitions.
/// Attach to a persistent GameObject in every scene.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game State")]
    public bool IsGameOver { get; private set; }
    public bool IsPaused { get; set; }

    private int currentScore;
    private PlayerController player;

    public int CurrentScore => currentScore;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        StartGame();
    }

    public void StartGame()
    {
        IsGameOver = false;
        IsPaused = false;
        currentScore = 0;
        Time.timeScale = 1f;

        // Find player
        player = FindObjectOfType<PlayerController>();

        // Initialize HUD
        if (HUDManager.Instance != null)
        {
            HUDManager.Instance.Show();
            HUDManager.Instance.UpdateScore(0);
            if (player != null)
                HUDManager.Instance.UpdateHealth(player.CurrentHealth, player.MaxHealth);
            HUDManager.Instance.UpdateWave(1);
        }

        // Hide game over screen
        if (GameOverUI.Instance != null)
            GameOverUI.Instance.Hide();

        // Start enemy spawning
        if (EnemySpawner.Instance != null)
            EnemySpawner.Instance.StartSpawning();
    }

    public void AddScore(int points)
    {
        if (IsGameOver) return;

        currentScore += points;
        HUDManager.Instance?.UpdateScore(currentScore);
    }

    public void GameOver()
    {
        if (IsGameOver) return;

        IsGameOver = true;

        // Stop spawning
        EnemySpawner.Instance?.StopSpawning();

        // Show game over UI
        int wave = EnemySpawner.Instance != null ? EnemySpawner.Instance.CurrentWave : 1;
        GameOverUI.Instance?.Show(currentScore, wave);

        // Hide HUD
        // HUDManager.Instance?.Hide(); // optional: keep visible to show final stats
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}

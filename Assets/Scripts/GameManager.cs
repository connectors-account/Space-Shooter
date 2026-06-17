using UnityEngine;

/// <summary>
/// Central game state controller. Tracks the player's score and health,
/// handles game-over logic, and exposes simple methods that other scripts
/// (player, enemies, bullets) call when relevant events occur.
///
/// Implemented as a lightweight singleton so any script can reach it via
/// <see cref="GameManager.Instance"/> without needing inspector references.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Player Settings")]
    [Tooltip("Health the player starts each game with.")]
    [SerializeField] private int startingHealth = 100;

    [Header("References")]
    [Tooltip("UI manager used to refresh on-screen values. Optional but recommended.")]
    [SerializeField] private UIManager uiManager;

    public int Score { get; private set; }
    public int Health { get; private set; }
    public bool IsGameOver { get; private set; }

    private void Awake()
    {
        // Enforce a single instance.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        // Find the UIManager automatically if it was not wired up in the inspector.
        if (uiManager == null)
        {
            uiManager = FindObjectOfType<UIManager>();
        }

        ResetGame();
    }

    private void Update()
    {
        // Allow a quick restart after the player loses.
        if (IsGameOver && Input.GetKeyDown(KeyCode.R))
        {
            RestartGame();
        }
    }

    /// <summary>Initialise / reset all gameplay values to their defaults.</summary>
    public void ResetGame()
    {
        Score = 0;
        Health = startingHealth;
        IsGameOver = false;
        Time.timeScale = 1f;

        if (uiManager != null)
        {
            uiManager.UpdateScore(Score);
            uiManager.UpdateHealth(Health, startingHealth);
            uiManager.HideGameOver();
        }
    }

    /// <summary>Add points to the score (called when an enemy is destroyed).</summary>
    public void AddScore(int amount)
    {
        if (IsGameOver) return;

        Score += amount;
        if (uiManager != null)
        {
            uiManager.UpdateScore(Score);
        }
    }

    /// <summary>Apply damage to the player. Triggers game over when health is depleted.</summary>
    public void DamagePlayer(int amount)
    {
        if (IsGameOver) return;

        Health -= amount;
        if (Health < 0) Health = 0;

        if (uiManager != null)
        {
            uiManager.UpdateHealth(Health, startingHealth);
        }

        if (Health <= 0)
        {
            TriggerGameOver();
        }
    }

    /// <summary>End the game: freeze time and show the game-over screen.</summary>
    private void TriggerGameOver()
    {
        IsGameOver = true;
        Time.timeScale = 0f; // Pause all time-based movement.

        if (uiManager != null)
        {
            uiManager.ShowGameOver(Score);
        }
    }

    /// <summary>Reload the active scene to start a fresh game.</summary>
    public void RestartGame()
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }
}

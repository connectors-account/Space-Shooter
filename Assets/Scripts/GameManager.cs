using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Central game state controller. Tracks score, player health and the
/// running/game-over state. Implemented as a lightweight singleton so any
/// script can reach it through GameManager.Instance.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Player Settings")]
    [Tooltip("Health the player starts each run with.")]
    public int startingHealth = 100;

    // Runtime state
    public int Score { get; private set; }
    public int Health { get; private set; }
    public bool IsGameOver { get; private set; }

    private void Awake()
    {
        // Enforce a single instance. If a duplicate exists (e.g. when reloading
        // the scene) destroy the new one so references stay valid.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        ResetGame();
    }

    /// <summary>Reset all runtime values to start a fresh run.</summary>
    public void ResetGame()
    {
        Score = 0;
        Health = startingHealth;
        IsGameOver = false;
        Time.timeScale = 1f; // make sure the game is un-paused

        // Push initial values to the HUD if one is present.
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateScore(Score);
            UIManager.Instance.UpdateHealth(Health, startingHealth);
            UIManager.Instance.HideGameOver();
        }
    }

    /// <summary>Add points to the score and refresh the HUD.</summary>
    public void AddScore(int amount)
    {
        if (IsGameOver) return;
        Score += amount;
        if (UIManager.Instance != null)
            UIManager.Instance.UpdateScore(Score);
    }

    /// <summary>Apply damage to the player. Triggers game over at zero.</summary>
    public void DamagePlayer(int amount)
    {
        if (IsGameOver) return;

        Health -= amount;
        if (Health < 0) Health = 0;

        if (UIManager.Instance != null)
            UIManager.Instance.UpdateHealth(Health, startingHealth);

        if (Health <= 0)
            GameOver();
    }

    /// <summary>Heal the player without exceeding the starting maximum.</summary>
    public void HealPlayer(int amount)
    {
        if (IsGameOver) return;

        Health += amount;
        if (Health > startingHealth) Health = startingHealth;

        if (UIManager.Instance != null)
            UIManager.Instance.UpdateHealth(Health, startingHealth);
    }

    /// <summary>End the current run and show the game-over screen.</summary>
    public void GameOver()
    {
        if (IsGameOver) return;
        IsGameOver = true;
        Time.timeScale = 0f; // freeze the action

        if (UIManager.Instance != null)
            UIManager.Instance.ShowGameOver(Score);
    }

    /// <summary>Reload the gameplay scene to restart.</summary>
    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    /// <summary>Return to the main menu scene (build index 0).</summary>
    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
    }
}

using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// GameManager is a singleton that tracks score, player health, and game state.
/// Attach this script to an empty "GameManager" GameObject in the scene.
/// </summary>
public class GameManager : MonoBehaviour
{
    // ---- Singleton ----
    public static GameManager Instance { get; private set; }

    [Header("Player Stats")]
    [Tooltip("Starting health of the player")]
    public int maxHealth = 5;

    // Current runtime values
    [HideInInspector] public int currentHealth;
    [HideInInspector] public int score;
    [HideInInspector] public bool isGameOver;

    // Event delegates so the UI can react to changes
    public System.Action<int> OnScoreChanged;
    public System.Action<int> OnHealthChanged;
    public System.Action OnGameOver;

    void Awake()
    {
        // Simple singleton pattern (no DontDestroyOnLoad needed for single-scene game)
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Initialize state
        currentHealth = maxHealth;
        score = 0;
        isGameOver = false;
    }

    /// <summary>
    /// Call this whenever an enemy is destroyed to award points.
    /// </summary>
    public void AddScore(int points)
    {
        if (isGameOver) return;

        score += points;
        OnScoreChanged?.Invoke(score);
    }

    /// <summary>
    /// Call this when the player is hit by an enemy.
    /// </summary>
    public void PlayerTakeDamage(int damage)
    {
        if (isGameOver) return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);
        OnHealthChanged?.Invoke(currentHealth);

        if (currentHealth <= 0)
        {
            TriggerGameOver();
        }
    }

    /// <summary>
    /// Ends the game: disables player, notifies UI.
    /// </summary>
    private void TriggerGameOver()
    {
        isGameOver = true;
        OnGameOver?.Invoke();

        // Optionally deactivate the player ship
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            player.SetActive(false);
        }
    }

    /// <summary>
    /// Reloads the current scene to restart the game.
    /// Called from the Restart button in the UI.
    /// </summary>
    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}

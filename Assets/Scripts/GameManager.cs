using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Central game state manager. Tracks score, game-over state, and
/// handles restarting. Uses a simple singleton pattern.
/// Attach to an empty "GameManager" GameObject in the scene.
/// </summary>
public class GameManager : MonoBehaviour
{
    // --- Singleton ---
    public static GameManager Instance { get; private set; }

    // --- State ---
    public int Score { get; private set; }
    public bool IsGameOver { get; private set; }

    void Awake()
    {
        // Simple singleton (no DontDestroyOnLoad needed for single-scene game)
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        Score = 0;
        IsGameOver = false;
    }

    void Update()
    {
        // Allow restart after game over by pressing R
        if (IsGameOver && Input.GetKeyDown(KeyCode.R))
        {
            RestartGame();
        }
    }

    /// <summary>
    /// Add points to the score and update the UI.
    /// </summary>
    public void AddScore(int points)
    {
        if (IsGameOver) return;

        Score += points;

        if (UIManager.Instance != null)
            UIManager.Instance.UpdateScore(Score);
    }

    /// <summary>
    /// Trigger game over state.
    /// </summary>
    public void GameOver()
    {
        if (IsGameOver) return;

        IsGameOver = true;

        if (UIManager.Instance != null)
            UIManager.Instance.ShowGameOver(Score);

        Debug.Log("Game Over! Final Score: " + Score);
    }

    /// <summary>
    /// Reload the scene to restart the game.
    /// </summary>
    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}

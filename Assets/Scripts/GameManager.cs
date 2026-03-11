using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Central game manager: tracks score, game state, and Game Over logic.
/// Uses a singleton pattern so other scripts can access it via GameManager.Instance.
/// Attach this to an empty "GameManager" GameObject in the scene.
/// </summary>
public class GameManager : MonoBehaviour
{
    // ---- Singleton ----
    public static GameManager Instance { get; private set; }

    // ---- State ----
    private int score = 0;
    private bool isGameOver = false;

    // ---- Properties ----
    public int Score => score;
    public bool IsGameOver => isGameOver;

    // =========================================================================
    // Unity Lifecycle
    // =========================================================================

    private void Awake()
    {
        // Singleton enforcement
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Update()
    {
        // Allow restarting the game after Game Over by pressing R
        if (isGameOver && Input.GetKeyDown(KeyCode.R))
        {
            RestartGame();
        }

        // Allow quitting with Escape
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            QuitGame();
        }
    }

    // =========================================================================
    // Score
    // =========================================================================

    /// <summary>
    /// Adds points to the score and updates the UI.
    /// </summary>
    public void AddScore(int points)
    {
        if (isGameOver) return;

        score += points;

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateScore(score);
        }
    }

    // =========================================================================
    // Game Over
    // =========================================================================

    /// <summary>
    /// Called when the player dies. Stops the game and shows the Game Over screen.
    /// </summary>
    public void GameOver()
    {
        if (isGameOver) return;

        isGameOver = true;

        // Stop enemy spawning
        EnemySpawner spawner = FindObjectOfType<EnemySpawner>();
        if (spawner != null)
        {
            spawner.StopSpawning();
        }

        // Destroy all remaining enemies and bullets
        foreach (GameObject enemy in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            Destroy(enemy);
        }
        foreach (GameObject bullet in GameObject.FindGameObjectsWithTag("PlayerBullet"))
        {
            Destroy(bullet);
        }

        // Show Game Over UI
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowGameOver(score);
        }
    }

    // =========================================================================
    // Restart / Quit
    // =========================================================================

    /// <summary>
    /// Reloads the current scene to restart the game.
    /// </summary>
    public void RestartGame()
    {
        // Reset the singleton so the new scene creates a fresh one
        Instance = null;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    /// <summary>
    /// Quits the application (only works in a built executable, not in the editor).
    /// </summary>
    public void QuitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}

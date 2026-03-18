using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Central game manager: handles score, game state, and scene transitions.
/// Singleton pattern — persists across the game lifetime within a scene.
/// Attach this to an empty "GameManager" GameObject.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game State")]
    public bool isGameOver = false;
    public int score = 0;

    [Header("Power-Up Prefabs")]
    [Tooltip("Assign your power-up prefabs here so enemies can drop them.")]
    public GameObject[] powerUpPrefabs;

    void Awake()
    {
        // Singleton setup
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    /// <summary>
    /// Adds score and updates the UI.
    /// </summary>
    public void AddScore(int amount)
    {
        if (isGameOver) return;

        score += amount;
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateScore(score);
        }
    }

    /// <summary>
    /// Called when the player dies. Triggers the Game Over screen.
    /// </summary>
    public void GameOver()
    {
        isGameOver = true;
        Debug.Log("GAME OVER! Final score: " + score);

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowGameOverScreen(score);
        }
    }

    /// <summary>
    /// Restarts the game by reloading the Game scene.
    /// </summary>
    public void RestartGame()
    {
        isGameOver = false;
        score = 0;
        SceneManager.LoadScene("GameScene");
    }

    /// <summary>
    /// Loads the main game scene from the main menu.
    /// </summary>
    public void StartGame()
    {
        SceneManager.LoadScene("GameScene");
    }

    /// <summary>
    /// Returns to the main menu.
    /// </summary>
    public void GoToMainMenu()
    {
        isGameOver = false;
        score = 0;
        SceneManager.LoadScene("MainMenu");
    }

    /// <summary>
    /// Quits the application.
    /// </summary>
    public void QuitGame()
    {
        Debug.Log("Quitting game...");
        Application.Quit();
    }
}

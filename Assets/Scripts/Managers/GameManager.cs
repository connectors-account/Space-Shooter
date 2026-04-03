using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Central game state manager. Controls game flow: menu, playing, paused, game over.
/// Singleton pattern for global access.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameState { Menu, Playing, Paused, GameOver }
    public GameState currentState = GameState.Menu;

    [Header("References")]
    public EnemySpawner enemySpawner;
    public GameObject playerObject;

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
        currentState = GameState.Menu;
        Time.timeScale = 1f;

        if (playerObject != null)
            playerObject.SetActive(false);

        if (UIManager.Instance != null)
            UIManager.Instance.ShowMainMenu();
    }

    void Update()
    {
        if (currentState == GameState.Playing && Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    public void StartGame()
    {
        currentState = GameState.Playing;
        Time.timeScale = 1f;

        // Reset score
        if (ScoreManager.Instance != null)
            ScoreManager.Instance.ResetScore();

        // Activate player
        if (playerObject != null)
        {
            playerObject.SetActive(true);
            playerObject.transform.position = new Vector3(0, -3.5f, 0);
            PlayerHealth health = playerObject.GetComponent<PlayerHealth>();
            if (health != null)
            {
                health.currentHealth = health.maxHealth;
            }
            PlayerController controller = playerObject.GetComponent<PlayerController>();
            if (controller != null)
            {
                controller.hasRapidFire = false;
                controller.hasSpreadShot = false;
                controller.hasShield = false;
            }
        }

        // Clear existing enemies and bullets
        ClearGameObjects();

        // Start spawning
        if (enemySpawner != null)
        {
            enemySpawner.currentWave = 0;
            enemySpawner.StartSpawning();
        }

        if (UIManager.Instance != null)
            UIManager.Instance.ShowGameHUD();
    }

    public void TogglePause()
    {
        if (currentState == GameState.Playing)
        {
            currentState = GameState.Paused;
            Time.timeScale = 0f;
            if (UIManager.Instance != null)
                UIManager.Instance.ShowPauseMenu();
        }
        else if (currentState == GameState.Paused)
        {
            ResumeGame();
        }
    }

    public void ResumeGame()
    {
        currentState = GameState.Playing;
        Time.timeScale = 1f;
        if (UIManager.Instance != null)
            UIManager.Instance.ShowGameHUD();
    }

    public void GameOver()
    {
        currentState = GameState.GameOver;

        if (enemySpawner != null)
            enemySpawner.StopSpawning();

        if (UIManager.Instance != null)
        {
            int finalScore = ScoreManager.Instance != null ? ScoreManager.Instance.GetScore() : 0;
            int wave = enemySpawner != null ? enemySpawner.GetCurrentWave() : 0;
            UIManager.Instance.ShowGameOver(finalScore, wave);
        }
    }

    public void ReturnToMenu()
    {
        currentState = GameState.Menu;
        Time.timeScale = 1f;

        ClearGameObjects();

        if (playerObject != null)
            playerObject.SetActive(false);

        if (enemySpawner != null)
            enemySpawner.StopSpawning();

        if (UIManager.Instance != null)
            UIManager.Instance.ShowMainMenu();
    }

    public void QuitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    public bool IsGameActive()
    {
        return currentState == GameState.Playing;
    }

    void ClearGameObjects()
    {
        // Destroy all enemies
        foreach (GameObject enemy in GameObject.FindGameObjectsWithTag("Enemy"))
            Destroy(enemy);

        // Destroy all bullets
        foreach (GameObject bullet in GameObject.FindGameObjectsWithTag("Bullet"))
            Destroy(bullet);

        // Destroy all power-ups
        foreach (GameObject powerUp in GameObject.FindGameObjectsWithTag("PowerUp"))
            Destroy(powerUp);
    }
}

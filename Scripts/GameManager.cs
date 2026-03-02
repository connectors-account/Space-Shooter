using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages game state, score, player health, and game over logic
/// </summary>
public class GameManager : MonoBehaviour
{
    [Header("Game Settings")]
    [SerializeField] private int startingHealth = 3;
    [SerializeField] private int maxHealth = 5;
    [SerializeField] private float invincibilityDuration = 2f;

    [Header("References")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform playerSpawnPoint;

    // Game state
    private int currentScore = 0;
    private int highScore = 0;
    private int currentHealth;
    private int currentWave = 1;
    private bool isGameOver = false;
    private bool isInvincible = false;

    // References
    private UIManager uiManager;
    private EnemySpawner enemySpawner;
    private GameObject playerInstance;

    // Singleton pattern for easy access
    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Load high score from PlayerPrefs
        highScore = PlayerPrefs.GetInt("HighScore", 0);
    }

    private void Start()
    {
        // Find references
        uiManager = FindObjectOfType<UIManager>();
        enemySpawner = FindObjectOfType<EnemySpawner>();

        // Start the game
        StartGame();
    }

    private void Update()
    {
        // Check for restart input when game is over
        if (isGameOver && Input.GetKeyDown(KeyCode.R))
        {
            RestartGame();
        }

        // Pause functionality
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
        {
            TogglePause();
        }
    }

    public void StartGame()
    {
        currentHealth = startingHealth;
        currentScore = 0;
        currentWave = 1;
        isGameOver = false;
        Time.timeScale = 1f;

        // Update UI
        UpdateUI();

        Debug.Log("Game Started!");
    }

    public void AddScore(int points)
    {
        if (isGameOver)
            return;

        currentScore += points;

        // Check for new high score
        if (currentScore > highScore)
        {
            highScore = currentScore;
            PlayerPrefs.SetInt("HighScore", highScore);
            PlayerPrefs.Save();
        }

        // Update UI
        if (uiManager != null)
        {
            uiManager.UpdateScore(currentScore, highScore);
        }
    }

    public void PlayerTakeDamage(int damage)
    {
        if (isGameOver || isInvincible)
            return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);

        // Update UI
        if (uiManager != null)
        {
            uiManager.UpdateHealth(currentHealth, maxHealth);
        }

        // Start invincibility period
        StartCoroutine(InvincibilityCoroutine());

        // Check for game over
        if (currentHealth <= 0)
        {
            GameOver();
        }
    }

    private System.Collections.IEnumerator InvincibilityCoroutine()
    {
        isInvincible = true;

        // Flash player to indicate invincibility
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            SpriteRenderer sr = player.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                float elapsed = 0f;
                while (elapsed < invincibilityDuration)
                {
                    sr.enabled = !sr.enabled;
                    yield return new WaitForSeconds(0.1f);
                    elapsed += 0.1f;
                }
                sr.enabled = true;
            }
        }

        isInvincible = false;
    }

    public void AddHealth(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        
        if (uiManager != null)
        {
            uiManager.UpdateHealth(currentHealth, maxHealth);
        }
    }

    public void OnNewWave(int waveNumber)
    {
        currentWave = waveNumber;
        
        if (uiManager != null)
        {
            uiManager.UpdateWave(currentWave);
            uiManager.ShowWaveNotification(currentWave);
        }

        Debug.Log($"Wave {currentWave} started!");
    }

    private void GameOver()
    {
        isGameOver = true;

        // Show game over UI
        if (uiManager != null)
        {
            uiManager.ShowGameOver(currentScore, highScore);
        }

        // Optional: Slow down time for dramatic effect
        StartCoroutine(SlowMotionGameOver());

        Debug.Log($"Game Over! Final Score: {currentScore}");
    }

    private System.Collections.IEnumerator SlowMotionGameOver()
    {
        Time.timeScale = 0.5f;
        yield return new WaitForSecondsRealtime(1f);
        Time.timeScale = 0f;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void TogglePause()
    {
        if (isGameOver)
            return;

        if (Time.timeScale > 0)
        {
            Time.timeScale = 0f;
            if (uiManager != null)
            {
                uiManager.ShowPauseMenu(true);
            }
        }
        else
        {
            Time.timeScale = 1f;
            if (uiManager != null)
            {
                uiManager.ShowPauseMenu(false);
            }
        }
    }

    public void QuitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    private void UpdateUI()
    {
        if (uiManager != null)
        {
            uiManager.UpdateScore(currentScore, highScore);
            uiManager.UpdateHealth(currentHealth, maxHealth);
            uiManager.UpdateWave(currentWave);
        }
    }

    // Public getters
    public bool IsGameOver() => isGameOver;
    public int GetCurrentScore() => currentScore;
    public int GetHighScore() => highScore;
    public int GetCurrentHealth() => currentHealth;
    public int GetCurrentWave() => currentWave;
    public bool IsInvincible() => isInvincible;
}

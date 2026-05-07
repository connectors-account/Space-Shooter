using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Central game state manager handling score, wave progression, and game lifecycle.
/// Attach this to a dedicated GameManager GameObject.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public bool IsGameActive => currentState == GameState.Playing;

    [Header("Scene References")]
    [SerializeField] private EnemySpawner enemySpawner;
    [SerializeField] private UIManager uiManager;
    [SerializeField] private PlayerController player;

    private int score;
    private int wave;
    private GameState currentState = GameState.StartMenu;

    private enum GameState
    {
        StartMenu,
        Playing,
        GameOver
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        InitializeStartMenu();
    }

    public void StartGame()
    {
        score = 0;
        wave = 1;
        currentState = GameState.Playing;

        if (player != null)
        {
            player.ResetPlayerState();
        }

        uiManager?.ShowStartMenu(false);
        uiManager?.ShowGameOver(false, score);
        uiManager?.ShowHUD(true);
        uiManager?.UpdateScore(score);
        uiManager?.UpdateWave(wave);

        if (player != null)
        {
            HealthSystem health = player.GetComponent<HealthSystem>();
            if (health != null)
            {
                uiManager?.UpdateHealth(health.CurrentHealth, health.MaxHealth);
            }
        }

        enemySpawner?.BeginSpawning();
    }

    public void GameOver()
    {
        if (currentState != GameState.Playing)
        {
            return;
        }

        currentState = GameState.GameOver;
        enemySpawner?.StopSpawning();

        uiManager?.ShowHUD(false);
        uiManager?.ShowGameOver(true, score);
    }

    public void RestartGame()
    {
        Scene activeScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(activeScene.buildIndex);
    }

    public void AddScore(int points)
    {
        if (currentState != GameState.Playing)
        {
            return;
        }

        score += Mathf.Max(0, points);
        uiManager?.UpdateScore(score);
    }

    public void SetWave(int currentWave)
    {
        if (currentState != GameState.Playing)
        {
            return;
        }

        wave = Mathf.Max(1, currentWave);
        uiManager?.UpdateWave(wave);
    }

    public void OnPlayerHealthChanged(int currentHealth, int maxHealth)
    {
        uiManager?.UpdateHealth(currentHealth, maxHealth);
    }

    private void InitializeStartMenu()
    {
        currentState = GameState.StartMenu;
        score = 0;
        wave = 1;

        uiManager?.ShowHUD(false);
        uiManager?.ShowGameOver(false, score);
        uiManager?.ShowStartMenu(true);
        uiManager?.UpdateScore(score);
        uiManager?.UpdateWave(wave);
    }
}

using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public event Action<int> OnScoreChanged;
    public event Action OnGameOver;

    [SerializeField] private HealthSystem playerHealthSystem;

    public bool IsGameOver { get; private set; }
    public int Score { get; private set; }

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
        if (playerHealthSystem == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerHealthSystem = player.GetComponent<HealthSystem>();
            }
        }

        if (playerHealthSystem != null)
        {
            playerHealthSystem.OnDeath += HandlePlayerDeath;
        }
        else
        {
            Debug.LogWarning("GameManager: Player HealthSystem not set and could not be auto-found.");
        }

        Score = 0;
        IsGameOver = false;
        OnScoreChanged?.Invoke(Score);
    }

    private void OnDestroy()
    {
        if (playerHealthSystem != null)
        {
            playerHealthSystem.OnDeath -= HandlePlayerDeath;
        }

        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void AddScore(int value)
    {
        if (IsGameOver || value <= 0)
        {
            return;
        }

        Score += value;
        OnScoreChanged?.Invoke(Score);
    }

    public void OnEnemyPassed()
    {
        if (IsGameOver)
        {
            return;
        }

        if (playerHealthSystem != null)
        {
            playerHealthSystem.TakeDamage(1);
        }
    }

    public void TriggerGameOver()
    {
        if (IsGameOver)
        {
            return;
        }

        IsGameOver = true;
        OnGameOver?.Invoke();

        EnemySpawner[] spawners = FindObjectsOfType<EnemySpawner>();
        foreach (EnemySpawner spawner in spawners)
        {
            spawner.enabled = false;
        }

        Enemy[] enemies = FindObjectsOfType<Enemy>();
        foreach (Enemy enemy in enemies)
        {
            Destroy(enemy.gameObject);
        }
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void HandlePlayerDeath()
    {
        TriggerGameOver();
    }
}

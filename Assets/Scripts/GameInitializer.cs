using UnityEngine;

/// <summary>
/// Scene bootstrap script. Place on a root GameObject in the scene.
/// Wires up the GameManager ↔ UIManager ↔ Player ↔ Spawner connections
/// that require runtime references, and kicks off the game loop.
/// </summary>
public class GameInitializer : MonoBehaviour
{
    [Header("Scene References")]
    [SerializeField] private GameManager gameManager;
    [SerializeField] private UIManager uiManager;
    [SerializeField] private EnemySpawner enemySpawner;
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform playerSpawnPoint;

    private GameObject _playerInstance;

    private void Start()
    {
        // Subscribe to state changes to manage player spawning
        if (gameManager != null)
            gameManager.OnGameStateChanged += HandleGameStateChanged;
    }

    private void OnDestroy()
    {
        if (gameManager != null)
            gameManager.OnGameStateChanged -= HandleGameStateChanged;
    }

    private void HandleGameStateChanged(GameManager.GameState state)
    {
        switch (state)
        {
            case GameManager.GameState.Playing:
                SpawnPlayer();
                if (enemySpawner != null)
                    enemySpawner.BeginSpawning();
                break;

            case GameManager.GameState.GameOver:
                if (enemySpawner != null)
                    enemySpawner.StopSpawning();
                break;

            case GameManager.GameState.MainMenu:
                CleanupGame();
                break;
        }
    }

    private void SpawnPlayer()
    {
        if (_playerInstance != null) return;
        if (playerPrefab == null) return;

        Vector3 spawnPos = playerSpawnPoint != null
            ? playerSpawnPoint.position
            : new Vector3(0f, -4f, 0f);

        _playerInstance = Instantiate(playerPrefab, spawnPos, Quaternion.identity);

        // Wire player health to UI
        HealthSystem playerHealth = _playerInstance.GetComponent<HealthSystem>();
        if (playerHealth != null && uiManager != null)
        {
            playerHealth.OnHealthChanged += uiManager.UpdateHealth;
            uiManager.UpdateHealth(playerHealth.CurrentHealth, playerHealth.MaxHealth);
        }
    }

    private void CleanupGame()
    {
        if (_playerInstance != null)
            Destroy(_playerInstance);

        if (enemySpawner != null)
        {
            enemySpawner.StopSpawning();
            enemySpawner.ClearAllEnemies();
        }

        // Destroy all remaining bullets
        foreach (var bullet in FindObjectsByType<BulletController>(FindObjectsSortMode.None))
            Destroy(bullet.gameObject);

        // Destroy all remaining power-ups
        foreach (var pu in FindObjectsByType<PowerUpController>(FindObjectsSortMode.None))
            Destroy(pu.gameObject);
    }
}

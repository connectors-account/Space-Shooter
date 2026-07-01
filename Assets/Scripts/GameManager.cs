using System.Collections;
using UnityEngine;

/// <summary>
/// Central game controller. Handles game state, scoring, wave progression,
/// and enemy spawning. Implemented as a singleton so other scripts can
/// reach it easily via GameManager.Instance.
/// </summary>
public class GameManager : MonoBehaviour
{
    public enum GameState { MainMenu, Playing, GameOver }

    public static GameManager Instance { get; private set; }

    [Header("Prefabs")]
    [Tooltip("Enemy prefab spawned during waves.")]
    public GameObject enemyPrefab;
    [Tooltip("Player prefab spawned when a game starts.")]
    public GameObject playerPrefab;

    [Header("Spawn Settings")]
    [Tooltip("Horizontal half-range (world units) enemies can spawn across.")]
    public float spawnHalfWidth = 7.5f;
    [Tooltip("World Y position where enemies appear (above the screen).")]
    public float spawnY = 6f;
    [Tooltip("Base number of enemies in wave 1.")]
    public int baseEnemiesPerWave = 5;
    [Tooltip("Extra enemies added each subsequent wave.")]
    public int enemiesAddedPerWave = 2;
    [Tooltip("Seconds between individual enemy spawns.")]
    public float spawnInterval = 0.8f;
    [Tooltip("Seconds of pause between waves.")]
    public float timeBetweenWaves = 3f;

    // Runtime state
    public GameState State { get; private set; } = GameState.MainMenu;
    public int Score { get; private set; }
    public int Wave { get; private set; }

    private int enemiesAlive;
    private PlayerController player;

    private void Awake()
    {
        // Enforce singleton.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        // Begin at the main menu.
        SetState(GameState.MainMenu);
    }

    /// <summary>Called by the UI Start button.</summary>
    public void StartGame()
    {
        Score = 0;
        Wave = 0;
        enemiesAlive = 0;

        SpawnPlayer();
        SetState(GameState.Playing);

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateScore(Score);
            UIManager.Instance.UpdateWave(Wave);
        }

        StopAllCoroutines();
        StartCoroutine(WaveRoutine());
    }

    /// <summary>Called by the UI Restart button on the game over screen.</summary>
    public void RestartGame()
    {
        // Clean up any leftover enemies / bullets from the previous run.
        DestroyAllWithTag("Enemy");
        DestroyAllWithTag("EnemyBullet");
        DestroyAllWithTag("PlayerBullet");
        StartGame();
    }

    private void SpawnPlayer()
    {
        if (player != null)
        {
            Destroy(player.gameObject);
        }

        if (playerPrefab != null)
        {
            GameObject go = Instantiate(playerPrefab, new Vector3(0f, -4f, 0f), Quaternion.identity);
            go.SetActive(true); // template prefabs may be inactive
            player = go.GetComponent<PlayerController>();
        }
    }

    private IEnumerator WaveRoutine()
    {
        while (State == GameState.Playing)
        {
            Wave++;
            if (UIManager.Instance != null)
            {
                UIManager.Instance.UpdateWave(Wave);
                UIManager.Instance.ShowWaveBanner(Wave);
            }

            int enemyCount = baseEnemiesPerWave + (Wave - 1) * enemiesAddedPerWave;

            // Spawn all enemies for this wave.
            for (int i = 0; i < enemyCount; i++)
            {
                if (State != GameState.Playing) yield break;
                SpawnEnemy();
                yield return new WaitForSeconds(spawnInterval);
            }

            // Wait until every enemy of the wave is cleared.
            while (enemiesAlive > 0 && State == GameState.Playing)
            {
                yield return null;
            }

            // Short breather before next wave.
            yield return new WaitForSeconds(timeBetweenWaves);
        }
    }

    private void SpawnEnemy()
    {
        if (enemyPrefab == null) return;

        float x = Random.Range(-spawnHalfWidth, spawnHalfWidth);
        Vector3 pos = new Vector3(x, spawnY, 0f);
        GameObject enemy = Instantiate(enemyPrefab, pos, Quaternion.identity);
        enemy.SetActive(true); // template prefabs may be inactive
        enemiesAlive++;
    }

    /// <summary>Called by an EnemyController when it dies (destroyed by player).</summary>
    public void OnEnemyKilled(int points)
    {
        enemiesAlive = Mathf.Max(0, enemiesAlive - 1);
        AddScore(points);
    }

    /// <summary>Called when an enemy leaves the screen without being killed.</summary>
    public void OnEnemyDespawned()
    {
        enemiesAlive = Mathf.Max(0, enemiesAlive - 1);
    }

    public void AddScore(int points)
    {
        Score += points;
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateScore(Score);
        }
    }

    /// <summary>Called by the player when health reaches zero.</summary>
    public void OnPlayerDied()
    {
        SetState(GameState.GameOver);
        StopAllCoroutines();

        DestroyAllWithTag("Enemy");
        DestroyAllWithTag("EnemyBullet");
        DestroyAllWithTag("PlayerBullet");

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowGameOver(Score, Wave);
        }
    }

    private void SetState(GameState newState)
    {
        State = newState;

        if (UIManager.Instance != null)
        {
            UIManager.Instance.OnGameStateChanged(newState);
        }
    }

    private void DestroyAllWithTag(string tag)
    {
        GameObject[] objects = GameObject.FindGameObjectsWithTag(tag);
        foreach (GameObject obj in objects)
        {
            Destroy(obj);
        }
    }
}

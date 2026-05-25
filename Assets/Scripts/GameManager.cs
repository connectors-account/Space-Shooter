using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Central game manager handling spawning, waves, scoring, and game state.
/// Singleton - persists as the main orchestrator of gameplay.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game State")]
    public bool isGameActive = false;
    public int score = 0;
    public int currentWave = 0;
    public int highScore = 0;

    [Header("Enemy Prefabs")]
    public GameObject[] enemyPrefabs; // Assign in Inspector: BasicEnemy, ZigzagEnemy, ShooterEnemy, DiverEnemy

    [Header("Power-Up Prefabs")]
    public GameObject healthPowerUpPrefab;
    public GameObject weaponPowerUpPrefab;

    [Header("Wave Settings")]
    public float timeBetweenWaves = 3f;
    public int baseEnemiesPerWave = 5;
    public float spawnInterval = 1f;

    [Header("Spawn Bounds")]
    public float spawnMinX = -5f;
    public float spawnMaxX = 5f;
    public float spawnY = 7f;

    private int enemiesAliveCount = 0;
    private int enemiesSpawnedThisWave = 0;
    private int enemiesToSpawnThisWave = 0;
    private bool isSpawning = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        highScore = PlayerPrefs.GetInt("HighScore", 0);
    }

    /// <summary>
    /// Called by MenuManager or UIManager to begin gameplay.
    /// </summary>
    public void StartGame()
    {
        score = 0;
        currentWave = 0;
        isGameActive = true;
        enemiesAliveCount = 0;

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateScore(score);
            UIManager.Instance.UpdateWave(currentWave);
            UIManager.Instance.ShowGameUI();
        }

        StartCoroutine(StartNextWave());
    }

    IEnumerator StartNextWave()
    {
        currentWave++;
        enemiesToSpawnThisWave = baseEnemiesPerWave + (currentWave - 1) * 2;
        enemiesSpawnedThisWave = 0;

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateWave(currentWave);
            UIManager.Instance.ShowWaveAnnouncement(currentWave);
        }

        yield return new WaitForSeconds(timeBetweenWaves);

        if (!isGameActive) yield break;

        isSpawning = true;
        StartCoroutine(SpawnWaveEnemies());
    }

    IEnumerator SpawnWaveEnemies()
    {
        while (enemiesSpawnedThisWave < enemiesToSpawnThisWave && isGameActive)
        {
            SpawnEnemy();
            enemiesSpawnedThisWave++;

            float interval = Mathf.Max(0.3f, spawnInterval - currentWave * 0.05f);
            yield return new WaitForSeconds(interval);
        }
        isSpawning = false;
    }

    void SpawnEnemy()
    {
        if (enemyPrefabs == null || enemyPrefabs.Length == 0) return;

        // Choose enemy type based on wave difficulty
        int prefabIndex = ChooseEnemyType();
        GameObject prefab = enemyPrefabs[Mathf.Clamp(prefabIndex, 0, enemyPrefabs.Length - 1)];

        float xPos = Random.Range(spawnMinX, spawnMaxX);
        Vector3 spawnPos = new Vector3(xPos, spawnY, 0f);

        GameObject enemy = Instantiate(prefab, spawnPos, Quaternion.identity);
        EnemyController ec = enemy.GetComponent<EnemyController>();
        if (ec != null)
        {
            // Scale difficulty with waves
            ec.maxHealth = Mathf.Min(2 + (currentWave / 3), 6);
            ec.currentHealth = ec.maxHealth;
            ec.moveSpeed += currentWave * 0.15f;
            ec.scoreValue = 100 + (currentWave - 1) * 20;
        }

        enemiesAliveCount++;
    }

    int ChooseEnemyType()
    {
        if (enemyPrefabs.Length == 1) return 0;

        // Progressively unlock harder enemy types
        int maxType = Mathf.Min(enemyPrefabs.Length - 1, (currentWave - 1) / 2);
        return Random.Range(0, maxType + 1);
    }

    public void EnemyDestroyed()
    {
        enemiesAliveCount = Mathf.Max(0, enemiesAliveCount - 1);

        // Check if wave is complete
        if (enemiesAliveCount <= 0 && !isSpawning && isGameActive)
        {
            StartCoroutine(StartNextWave());
        }
    }

    public void AddScore(int points)
    {
        if (!isGameActive) return;
        score += points;
        if (UIManager.Instance != null)
            UIManager.Instance.UpdateScore(score);
    }

    public void SpawnPowerUp(Vector3 position)
    {
        GameObject prefab = Random.value > 0.5f ? healthPowerUpPrefab : weaponPowerUpPrefab;
        if (prefab != null)
        {
            Instantiate(prefab, position, Quaternion.identity);
        }
    }

    public void GameOver()
    {
        isGameActive = false;
        isSpawning = false;
        StopAllCoroutines();

        // Save high score
        if (score > highScore)
        {
            highScore = score;
            PlayerPrefs.SetInt("HighScore", highScore);
            PlayerPrefs.Save();
        }

        // Clean up remaining enemies and bullets
        CleanUpGameObjects();

        if (UIManager.Instance != null)
            UIManager.Instance.ShowGameOver(score, highScore);
    }

    void CleanUpGameObjects()
    {
        foreach (GameObject enemy in GameObject.FindGameObjectsWithTag("Enemy"))
            Destroy(enemy);
        foreach (GameObject bullet in GameObject.FindGameObjectsWithTag("Bullet"))
            Destroy(bullet);
        foreach (GameObject powerUp in GameObject.FindGameObjectsWithTag("PowerUp"))
            Destroy(powerUp);
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitGame()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}

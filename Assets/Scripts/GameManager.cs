using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private PlayerController player;
    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] private WaveManager waveManager;
    [SerializeField] private GameOverScreen gameOverScreen;
    [SerializeField] private PauseMenu pauseMenu;

    [Header("Pools")]
    [SerializeField] private ObjectPool playerBulletPool;
    [SerializeField] private ObjectPool enemyBulletPool;
    [SerializeField] private ObjectPool basicEnemyPool;
    [SerializeField] private ObjectPool fastEnemyPool;
    [SerializeField] private ObjectPool tankEnemyPool;
    [SerializeField] private ObjectPool powerUpPool;

    [Header("Effects")]
    [SerializeField] private ParticleSystem explosionPrefab;

    public bool IsGameOver { get; private set; }
    public bool IsPaused => Time.timeScale <= 0.001f;

    private const string HighScoreKey = "HIGH_SCORE";

    public int HighScore => PlayerPrefs.GetInt(HighScoreKey, 0);
    public ScoreManager ScoreManager => scoreManager;
    public WaveManager WaveManager => waveManager;
    public PlayerController Player => player;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        Time.timeScale = 1f;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !IsGameOver)
        {
            TogglePause();
        }
    }

    public ObjectPool GetEnemyPool(EnemyType enemyType)
    {
        return enemyType switch
        {
            EnemyType.Basic => basicEnemyPool,
            EnemyType.Fast => fastEnemyPool,
            EnemyType.Tank => tankEnemyPool,
            _ => basicEnemyPool
        };
    }

    public ObjectPool GetPlayerBulletPool() => playerBulletPool;
    public ObjectPool GetEnemyBulletPool() => enemyBulletPool;
    public ObjectPool GetPowerUpPool() => powerUpPool;

    public void SpawnExplosion(Vector3 position)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySfx(AudioCue.Explosion);
        }

        if (explosionPrefab == null)
        {
            return;
        }

        ParticleSystem explosion = Instantiate(explosionPrefab, position, Quaternion.identity);
        explosion.Play();
        Destroy(explosion.gameObject, explosion.main.duration + explosion.main.startLifetime.constantMax + 0.1f);
    }

    public void OnPlayerDied()
    {
        if (IsGameOver)
        {
            return;
        }

        IsGameOver = true;
        Time.timeScale = 0f;

        int score = scoreManager != null ? scoreManager.Score : 0;
        if (score > HighScore)
        {
            PlayerPrefs.SetInt(HighScoreKey, score);
            PlayerPrefs.Save();
        }

        if (gameOverScreen != null)
        {
            gameOverScreen.Show(score, HighScore);
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySfx(AudioCue.GameOver);
        }
    }

    public void TogglePause()
    {
        bool shouldPause = !IsPaused;
        Time.timeScale = shouldPause ? 0f : 1f;

        if (pauseMenu != null)
        {
            pauseMenu.SetVisible(shouldPause);
        }
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;
        if (pauseMenu != null)
        {
            pauseMenu.SetVisible(false);
        }
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}

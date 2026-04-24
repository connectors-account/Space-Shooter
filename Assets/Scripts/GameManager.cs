using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Central game state manager: score, player health, and game-over flow.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Player Health")]
    [SerializeField] private int maxHealth = 5;

    private int currentHealth;
    private int currentScore;
    private bool isGameOver;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public int CurrentScore => currentScore;
    public bool IsGameOver => isGameOver;

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
        StartGame();
    }

    private void Update()
    {
        if (isGameOver && Input.GetKeyDown(KeyCode.R))
        {
            RestartGame();
        }
    }

    public void StartGame()
    {
        currentScore = 0;
        currentHealth = maxHealth;
        isGameOver = false;
        Time.timeScale = 1f;

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateScore(currentScore);
            UIManager.Instance.UpdateHealth(currentHealth, maxHealth);
            UIManager.Instance.HideGameOver();
        }
    }

    public void AddScore(int value)
    {
        if (isGameOver)
            return;

        currentScore += value;
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateScore(currentScore);
        }
    }

    public void DamagePlayer(int damage)
    {
        if (isGameOver)
            return;

        currentHealth -= damage;
        if (currentHealth < 0)
            currentHealth = 0;

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateHealth(currentHealth, maxHealth);
        }

        if (currentHealth <= 0)
        {
            GameOver();
        }
    }

    public void GameOver()
    {
        if (isGameOver)
            return;

        isGameOver = true;

        int highScore = PlayerPrefs.GetInt("HighScore", 0);
        if (currentScore > highScore)
        {
            highScore = currentScore;
            PlayerPrefs.SetInt("HighScore", highScore);
            PlayerPrefs.Save();
        }

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowGameOver(currentScore, highScore);
        }

        Time.timeScale = 0f;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}

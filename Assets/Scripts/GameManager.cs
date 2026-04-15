using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Central manager for score, game state, and scene-level flow.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game State")]
    [SerializeField] private bool gameOver;
    [SerializeField] private int score;

    private UIManager uiManager;

    public bool IsGameOver => gameOver;
    public int Score => score;

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
        uiManager = FindObjectOfType<UIManager>();

        score = 0;
        gameOver = false;

        if (uiManager != null)
        {
            uiManager.UpdateScore(score);
            uiManager.HideGameOver();
        }
    }

    private void Update()
    {
        // Keyboard restart shortcut after game over.
        if (gameOver && Input.GetKeyDown(KeyCode.R))
        {
            RestartGame();
        }
    }

    public void AddScore(int value)
    {
        if (gameOver)
        {
            return;
        }

        score += Mathf.Max(0, value);
        if (uiManager != null)
        {
            uiManager.UpdateScore(score);
        }
    }

    public void PlayerDied()
    {
        if (gameOver)
        {
            return;
        }

        gameOver = true;

        if (uiManager != null)
        {
            uiManager.ShowGameOver();
        }
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void LoadMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}

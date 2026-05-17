using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Central game manager. Manages game state, pause, and scene transitions.
/// Persists across scenes via DontDestroyOnLoad.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public bool IsGameActive { get; private set; }
    public bool IsPaused { get; private set; }

    /// <summary>Called by bootstrap to set game active state directly.</summary>
    public void IsGameActive_Set(bool value) { IsGameActive = value; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (SceneManager.GetActiveScene().name == "GameScene")
            {
                if (IsGameActive)
                    TogglePause();
            }
        }
    }

    public void StartGame()
    {
        IsGameActive = true;
        IsPaused = false;
        Time.timeScale = 1f;
        SceneManager.LoadScene("GameScene");
    }

    public void GameOver()
    {
        IsGameActive = false;
        Time.timeScale = 1f;

        if (UIManager.Instance != null)
            UIManager.Instance.ShowGameOver();

        if (WaveSpawner.Instance != null)
            WaveSpawner.Instance.StopSpawning();
    }

    public void TogglePause()
    {
        IsPaused = !IsPaused;
        Time.timeScale = IsPaused ? 0f : 1f;

        if (UIManager.Instance != null)
            UIManager.Instance.ShowPauseMenu(IsPaused);
    }

    public void ResumeGame()
    {
        IsPaused = false;
        Time.timeScale = 1f;

        if (UIManager.Instance != null)
            UIManager.Instance.ShowPauseMenu(false);
    }

    public void ReturnToMainMenu()
    {
        IsGameActive = false;
        IsPaused = false;
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenuScene");
    }

    public void RestartGame()
    {
        IsGameActive = true;
        IsPaused = false;
        Time.timeScale = 1f;
        SceneManager.LoadScene("GameScene");
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

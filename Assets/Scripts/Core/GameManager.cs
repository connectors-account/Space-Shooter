// ============================================================
//  GameManager.cs  –  Core singleton: state machine & lives
// ============================================================
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState { Menu, Playing, Paused, GameOver }

public class GameManager : MonoBehaviour
{
    // ── Singleton ────────────────────────────────────────────
    public static GameManager Instance { get; private set; }

    // ── Public state ─────────────────────────────────────────
    public GameState State { get; private set; } = GameState.Menu;

    [Header("Player")]
    public GameObject playerPrefab;
    public Vector3    playerSpawnPos = new Vector3(0f, -3.5f, 0f);
    public int        startLives     = 3;
    public float      respawnDelay   = 2f;

    public int Lives { get; private set; }

    // ── Unity lifecycle ──────────────────────────────────────
    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // ── Public API ───────────────────────────────────────────

    /// <summary>Called from the Main Menu "Play" button.</summary>
    public void StartGame()
    {
        Lives = startLives;
        ScoreManager.Instance?.Reset();
        WaveManager.Instance?.Reset();
        Time.timeScale = 1f;
        State = GameState.Playing;
        SceneManager.LoadScene("Game");
    }

    public void PauseGame()
    {
        if (State != GameState.Playing) return;
        State = GameState.Paused;
        Time.timeScale = 0f;
        UIManager.Instance?.ShowPausePanel(true);
    }

    public void ResumeGame()
    {
        if (State != GameState.Paused) return;
        State = GameState.Playing;
        Time.timeScale = 1f;
        UIManager.Instance?.ShowPausePanel(false);
    }

    /// <summary>Called by PlayerHealth when the player dies.</summary>
    public void OnPlayerDied()
    {
        Lives--;
        UIManager.Instance?.RefreshLives(Lives);

        if (Lives <= 0)
            StartCoroutine(GameOverSequence());
        else
            StartCoroutine(RespawnSequence());
    }

    public void ReturnToMenu()
    {
        Time.timeScale = 1f;
        State = GameState.Menu;
        SceneManager.LoadScene("MainMenu");
    }

    public void QuitApplication()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // ── Private helpers ──────────────────────────────────────

    IEnumerator RespawnSequence()
    {
        yield return new WaitForSeconds(respawnDelay);
        if (playerPrefab != null)
            Instantiate(playerPrefab, playerSpawnPos, Quaternion.identity);
    }

    IEnumerator GameOverSequence()
    {
        yield return new WaitForSeconds(1.5f);
        State = GameState.GameOver;
        ScoreManager.Instance?.SaveHighScore();
        Time.timeScale = 0f;
        UIManager.Instance?.ShowGameOverPanel();
    }

    // ── Input (Escape = pause/unpause) ───────────────────────
    void Update()
    {
        if (!Input.GetKeyDown(KeyCode.Escape)) return;
        if (State == GameState.Playing)  PauseGame();
        else if (State == GameState.Paused) ResumeGame();
    }
}

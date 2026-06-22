using System;
using UnityEngine;

/// <summary>
/// Central game flow controller. Owns the high-level game state machine
/// (Menu -> Playing -> GameOver) and exposes events that other systems
/// (UI, spawner, player) subscribe to.
///
/// This is implemented as a lightweight singleton so any script can reach
/// it via GameManager.Instance without needing inspector references.
/// </summary>
public class GameManager : MonoBehaviour
{
    /// <summary>All possible high-level states the game can be in.</summary>
    public enum GameState
    {
        Menu,
        Playing,
        GameOver
    }

    // -------- Singleton --------
    public static GameManager Instance { get; private set; }

    // -------- Events --------
    // UIManager and other systems listen to these to react to state changes.
    public event Action<GameState> OnStateChanged;
    public event Action OnGameStarted;
    public event Action OnGameOver;

    [Header("Scene References (assign in Inspector)")]
    [Tooltip("Player GameObject in the scene. Disabled until the game starts.")]
    public GameObject player;

    [Tooltip("Enemy spawner. Enabled only while playing.")]
    public EnemySpawner enemySpawner;

    /// <summary>Current state of the game. Read-only from outside.</summary>
    public GameState CurrentState { get; private set; } = GameState.Menu;

    private void Awake()
    {
        // Standard singleton guard: keep the first instance, destroy duplicates.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        // Begin on the menu so the player must press Play first.
        SetState(GameState.Menu);
    }

    /// <summary>
    /// Called by the UI "Play" button (or pressing Enter on the menu).
    /// Resets score/health and switches into the Playing state.
    /// </summary>
    public void StartGame()
    {
        // Reset gameplay subsystems to a clean slate.
        if (ScoreManager.Instance != null)
            ScoreManager.Instance.ResetScore();

        // Activate and reset the player.
        if (player != null)
        {
            player.SetActive(true);
            // Reposition to a sensible starting spot near the bottom-center.
            player.transform.position = new Vector3(0f, -4f, 0f);

            var health = player.GetComponent<HealthSystem>();
            if (health != null)
                health.ResetHealth();

            var controller = player.GetComponent<PlayerController>();
            if (controller != null)
                controller.ResetState();
        }

        // Turn the spawner on and reset its wave progression.
        if (enemySpawner != null)
        {
            enemySpawner.ResetSpawner();
            enemySpawner.enabled = true;
        }

        // Ensure time is running (it may have been paused on game over).
        Time.timeScale = 1f;

        SetState(GameState.Playing);
        OnGameStarted?.Invoke();
    }

    /// <summary>
    /// Called when the player's health reaches zero. Freezes gameplay and
    /// shows the game-over screen via the UI event.
    /// </summary>
    public void EndGame()
    {
        if (CurrentState == GameState.GameOver)
            return;

        if (enemySpawner != null)
            enemySpawner.enabled = false;

        if (player != null)
            player.SetActive(false);

        // Record the high score if the new score beats it.
        if (ScoreManager.Instance != null)
            ScoreManager.Instance.SaveHighScore();

        SetState(GameState.GameOver);
        OnGameOver?.Invoke();
    }

    /// <summary>
    /// Return to the main menu (called by the "Menu" button on game over).
    /// </summary>
    public void ReturnToMenu()
    {
        Time.timeScale = 1f;
        SetState(GameState.Menu);
    }

    /// <summary>Quit the application (works in a built .exe, no-op in editor play).</summary>
    public void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    private void Update()
    {
        // Convenience hotkeys so the game is playable without UI buttons too.
        if (CurrentState == GameState.Menu && (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
        {
            StartGame();
        }
        else if (CurrentState == GameState.GameOver && Input.GetKeyDown(KeyCode.R))
        {
            StartGame();
        }
    }

    /// <summary>Internal helper to change state and notify listeners.</summary>
    private void SetState(GameState newState)
    {
        CurrentState = newState;
        OnStateChanged?.Invoke(newState);
    }
}

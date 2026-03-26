using System;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Singleton that owns all core game state: score, health, wave, and game-flow state machine.
/// Raises C# events so UI / audio / spawners can react without tight coupling.
/// </summary>
public class GameManager : MonoBehaviour
{
    // ── Singleton ────────────────────────────────────────────────────────
    public static GameManager Instance { get; private set; }

    // ── Game state enum ──────────────────────────────────────────────────
    public enum State { Menu, Playing, Paused, GameOver }
    public State CurrentState { get; private set; } = State.Menu;

    // ── Events ───────────────────────────────────────────────────────────
    public event Action<State> OnStateChanged;
    public event Action<int>   OnScoreChanged;
    public event Action<int>   OnHealthChanged;
    public event Action<int>   OnWaveChanged;

    // ── Runtime data ─────────────────────────────────────────────────────
    public int Score       { get; private set; }
    public int PlayerHealth{ get; private set; }
    public int MaxHealth   { get; private set; } = 3;
    public int WaveNumber  { get; private set; }

    // ── Lifecycle ─────────────────────────────────────────────────────────
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // ── Public API ────────────────────────────────────────────────────────
    public void StartGame()
    {
        Score        = 0;
        PlayerHealth = MaxHealth;
        WaveNumber   = 0;
        Time.timeScale = 1f;
        SetState(State.Playing);
        OnScoreChanged?.Invoke(Score);
        OnHealthChanged?.Invoke(PlayerHealth);
    }

    public void PauseGame()
    {
        if (CurrentState != State.Playing) return;
        Time.timeScale = 0f;
        SetState(State.Paused);
    }

    public void ResumeGame()
    {
        if (CurrentState != State.Paused) return;
        Time.timeScale = 1f;
        SetState(State.Playing);
    }

    public void GameOver()
    {
        Time.timeScale = 0f;
        SetState(State.GameOver);
    }

    public void ReturnToMenu()
    {
        Time.timeScale = 1f;
        SetState(State.Menu);
        SceneManager.LoadScene("MenuScene");
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("GameScene");
        // StartGame will be called by GameScene bootstrap
    }

    // ── Score ─────────────────────────────────────────────────────────────
    public void AddScore(int points)
    {
        Score += points;
        OnScoreChanged?.Invoke(Score);
    }

    // ── Health ────────────────────────────────────────────────────────────
    public void TakeDamage(int amount)
    {
        if (CurrentState != State.Playing) return;
        PlayerHealth = Mathf.Max(0, PlayerHealth - amount);
        OnHealthChanged?.Invoke(PlayerHealth);
        if (PlayerHealth <= 0) GameOver();
    }

    public void RestoreHealth(int amount)
    {
        PlayerHealth = Mathf.Min(MaxHealth, PlayerHealth + amount);
        OnHealthChanged?.Invoke(PlayerHealth);
    }

    // ── Waves ─────────────────────────────────────────────────────────────
    public void AdvanceWave()
    {
        WaveNumber++;
        OnWaveChanged?.Invoke(WaveNumber);
    }

    // ── Internal ──────────────────────────────────────────────────────────
    private void SetState(State newState)
    {
        CurrentState = newState;
        OnStateChanged?.Invoke(newState);
    }
}

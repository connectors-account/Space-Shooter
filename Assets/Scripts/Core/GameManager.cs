using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SpaceShooter.Core
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public int Score { get; private set; }
        public int PlayerHealth { get; private set; } = 5;
        public int MaxHealth { get; private set; } = 5;
        public int Wave { get; private set; } = 1;
        public GameState State { get; private set; } = GameState.MainMenu;

        public event Action<int> OnScoreChanged;
        public event Action<int, int> OnHealthChanged;
        public event Action<int> OnWaveChanged;
        public event Action<GameState> OnStateChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void Update()
        {
            if (SceneManager.GetActiveScene().name != "GamePlay")
            {
                return;
            }

            if (Input.GetKeyDown(KeyCode.Escape) && (State == GameState.Playing || State == GameState.Paused))
            {
                TogglePause();
            }
        }

        public void StartGame()
        {
            Score = 0;
            PlayerHealth = MaxHealth;
            Wave = 1;
            SetState(GameState.Playing);
            Time.timeScale = 1f;

            OnScoreChanged?.Invoke(Score);
            OnHealthChanged?.Invoke(PlayerHealth, MaxHealth);
            OnWaveChanged?.Invoke(Wave);
        }

        public void SetWave(int wave)
        {
            Wave = Mathf.Max(1, wave);
            OnWaveChanged?.Invoke(Wave);
        }

        public void AddScore(int amount)
        {
            if (State != GameState.Playing)
            {
                return;
            }

            Score += Mathf.Max(0, amount);
            OnScoreChanged?.Invoke(Score);
        }

        public void DamagePlayer(int amount)
        {
            if (State != GameState.Playing)
            {
                return;
            }

            PlayerHealth -= Mathf.Max(0, amount);
            PlayerHealth = Mathf.Clamp(PlayerHealth, 0, MaxHealth);
            OnHealthChanged?.Invoke(PlayerHealth, MaxHealth);

            if (PlayerHealth <= 0)
            {
                TriggerGameOver();
            }
        }

        public void HealPlayer(int amount)
        {
            PlayerHealth += Mathf.Max(0, amount);
            PlayerHealth = Mathf.Clamp(PlayerHealth, 0, MaxHealth);
            OnHealthChanged?.Invoke(PlayerHealth, MaxHealth);
        }

        public void TogglePause()
        {
            if (State == GameState.Playing)
            {
                SetState(GameState.Paused);
                Time.timeScale = 0f;
            }
            else if (State == GameState.Paused)
            {
                SetState(GameState.Playing);
                Time.timeScale = 1f;
            }
        }

        public void TriggerGameOver()
        {
            SetState(GameState.GameOver);
            Time.timeScale = 0f;
        }

        public void RestartGame()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene("GamePlay");
        }

        public void GoToMainMenu()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene("MainMenu");
        }

        private void SetState(GameState state)
        {
            State = state;
            OnStateChanged?.Invoke(state);
        }
    }
}

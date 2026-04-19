using System;
using System.Collections;
using SpaceShooter.Gameplay;
using SpaceShooter.UI;
using UnityEngine;

namespace SpaceShooter.Core
{
    public class GameManager : MonoBehaviour
    {
        public enum GameState
        {
            MainMenu,
            Playing,
            Paused,
            GameOver
        }

        public static GameManager Instance { get; private set; }

        [Header("Wave Settings")]
        [SerializeField] private int baseEnemiesPerWave = 5;
        [SerializeField] private int enemiesPerWaveGrowth = 3;
        [SerializeField] private float waveStartDelay = 2f;

        [Header("Player Settings")]
        [SerializeField] private int playerMaxHealth = 100;

        private SpawnManager spawnManager;
        private UIManager uiManager;
        private AudioManager audioManager;
        private PlayerController player;

        public int Score { get; private set; }
        public int Wave { get; private set; }
        public int HighScore { get; private set; }
        public GameState State { get; private set; } = GameState.MainMenu;

        public event Action<GameState> OnStateChanged;
        public event Action<int> OnScoreChanged;
        public event Action<int> OnWaveChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            HighScore = PlayerPrefs.GetInt("high_score", 0);
        }

        public void Initialize(SpawnManager spawn, UIManager ui, AudioManager audio)
        {
            spawnManager = spawn;
            uiManager = ui;
            audioManager = audio;

            uiManager.Bind(this);
            SetState(GameState.MainMenu);
            uiManager.RefreshMenu(HighScore);
        }

        public void StartNewGame()
        {
            CleanupRoundEntities();

            Score = 0;
            Wave = 0;
            OnScoreChanged?.Invoke(Score);

            EnsurePlayer();
            player.ResetForNewRun();

            SetState(GameState.Playing);
            StartCoroutine(BeginNextWaveRoutine());
        }

        public void RestartFromGameOver()
        {
            Time.timeScale = 1f;
            StartNewGame();
        }

        public void ReturnToMenu()
        {
            Time.timeScale = 1f;
            CleanupRoundEntities();
            if (player != null)
            {
                player.gameObject.SetActive(false);
            }

            SetState(GameState.MainMenu);
            uiManager.RefreshMenu(HighScore);
        }

        public void TogglePause()
        {
            if (State == GameState.Playing)
            {
                Time.timeScale = 0f;
                SetState(GameState.Paused);
            }
            else if (State == GameState.Paused)
            {
                Time.timeScale = 1f;
                SetState(GameState.Playing);
            }
        }

        public void AddScore(int points)
        {
            if (State != GameState.Playing)
            {
                return;
            }

            Score += Mathf.Max(0, points);
            OnScoreChanged?.Invoke(Score);
        }

        public void ReportEnemyDestroyed(int scoreAward)
        {
            if (State != GameState.Playing)
            {
                return;
            }

            AddScore(scoreAward);
            spawnManager.NotifyEnemyDestroyed();

            if (spawnManager.IsWaveClear)
            {
                StartCoroutine(BeginNextWaveRoutine());
            }
        }

        public void OnPlayerDeath()
        {
            if (State != GameState.Playing)
            {
                return;
            }

            Time.timeScale = 1f;
            SetState(GameState.GameOver);
            audioManager.PlayGameOver();

            if (Score > HighScore)
            {
                HighScore = Score;
                PlayerPrefs.SetInt("high_score", HighScore);
                PlayerPrefs.Save();
            }

            uiManager.ShowGameOver(Score, HighScore, Wave);
        }

        private IEnumerator BeginNextWaveRoutine()
        {
            if (State != GameState.Playing)
            {
                yield break;
            }

            yield return new WaitForSeconds(waveStartDelay);

            if (State != GameState.Playing)
            {
                yield break;
            }

            Wave += 1;
            OnWaveChanged?.Invoke(Wave);
            audioManager.PlayWaveStart();

            int enemies = baseEnemiesPerWave + (Wave - 1) * enemiesPerWaveGrowth;
            spawnManager.BeginWave(Wave, enemies);
        }

        private void EnsurePlayer()
        {
            if (player != null)
            {
                player.gameObject.SetActive(true);
                return;
            }

            player = EntityFactory.CreatePlayer(playerMaxHealth);
            player.OnPlayerDied += OnPlayerDeath;
            player.OnHealthChanged += uiManager.UpdateHealth;
            uiManager.UpdateHealth(player.CurrentHealth, player.MaxHealth);
        }

        private void CleanupRoundEntities()
        {
            spawnManager.ClearAllDynamicEntities();
        }

        private void SetState(GameState newState)
        {
            State = newState;
            OnStateChanged?.Invoke(State);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape) && (State == GameState.Playing || State == GameState.Paused))
            {
                TogglePause();
            }
        }
    }
}

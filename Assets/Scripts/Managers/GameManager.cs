using System.Collections;
using UnityEngine;

namespace SpaceShooter.Managers
{
    /// <summary>
    /// Central game state authority for score, combo, waves, and high score persistence.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public enum GameState
        {
            MainMenu,
            Playing,
            Paused,
            GameOver
        }

        [Header("Wave Settings")]
        [SerializeField] private int startingWave = 1;
        [SerializeField] private int enemiesPerWaveBase = 5;
        [SerializeField] private int enemiesPerWaveIncrement = 3;
        [SerializeField] private float waveCooldown = 2.5f;

        [Header("Scoring")]
        [SerializeField] private float comboResetDelay = 2.5f;
        [SerializeField] private int maxComboMultiplier = 8;

        private GameState currentState = GameState.MainMenu;
        private int score;
        private int highScore;
        private int currentWave;
        private int enemiesRemainingInWave;

        private int comboCount;
        private int comboMultiplier = 1;
        private float lastKillTime = -999f;

        private Coroutine nextWaveCoroutine;

        public GameState CurrentState => currentState;
        public int Score => score;
        public int HighScore => highScore;
        public int CurrentWave => currentWave;
        public int EnemiesRemainingInWave => enemiesRemainingInWave;
        public int ComboCount => comboCount;
        public int ComboMultiplier => comboMultiplier;

        public event System.Action<int> OnScoreChanged;
        public event System.Action<int> OnWaveChanged;
        public event System.Action<GameState> OnGameStateChanged;
        public event System.Action<int, int> OnComboChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            highScore = PlayerPrefs.GetInt("HighScore", 0);
        }

        private void Update()
        {
            if ((currentState == GameState.Playing || currentState == GameState.Paused) && Input.GetKeyDown(KeyCode.Escape))
            {
                TogglePause();
            }

            if (currentState == GameState.Playing && comboMultiplier > 1 && Time.time - lastKillTime > comboResetDelay)
            {
                ResetCombo();
            }
        }

        public void StartGame()
        {
            Time.timeScale = 1f;
            score = 0;
            currentWave = Mathf.Max(1, startingWave - 1);
            ResetCombo();

            SetState(GameState.Playing);
            OnScoreChanged?.Invoke(score);
            StartNextWave();
        }

        public void RestartGame()
        {
            StartGame();
        }

        public void ReturnToMenu()
        {
            Time.timeScale = 1f;
            StopNextWaveRoutine();
            SetState(GameState.MainMenu);
            score = 0;
            currentWave = 0;
            enemiesRemainingInWave = 0;
            ResetCombo();
            OnScoreChanged?.Invoke(score);
            OnWaveChanged?.Invoke(currentWave);
        }

        public void TogglePause()
        {
            if (currentState == GameState.Playing)
            {
                Time.timeScale = 0f;
                SetState(GameState.Paused);
            }
            else if (currentState == GameState.Paused)
            {
                Time.timeScale = 1f;
                SetState(GameState.Playing);
            }
        }

        public void GameOver()
        {
            if (currentState == GameState.GameOver)
            {
                return;
            }

            Time.timeScale = 1f;
            StopNextWaveRoutine();
            SetState(GameState.GameOver);

            if (score > highScore)
            {
                highScore = score;
                PlayerPrefs.SetInt("HighScore", highScore);
                PlayerPrefs.Save();
            }
        }

        public void HandleEnemyDestroyed(int basePoints)
        {
            if (currentState != GameState.Playing)
            {
                return;
            }

            UpdateCombo();
            int gained = Mathf.Max(1, basePoints) * comboMultiplier;
            score += gained;
            OnScoreChanged?.Invoke(score);
        }

        public void OnEnemyKilledInWave()
        {
            if (currentState != GameState.Playing)
            {
                return;
            }

            enemiesRemainingInWave = Mathf.Max(0, enemiesRemainingInWave - 1);
            if (enemiesRemainingInWave == 0)
            {
                StopNextWaveRoutine();
                nextWaveCoroutine = StartCoroutine(StartNextWaveAfterDelay());
            }
        }

        public int GetEnemiesForWave(int wave)
        {
            return Mathf.Max(1, enemiesPerWaveBase + (wave - 1) * enemiesPerWaveIncrement);
        }

        private void StartNextWave()
        {
            currentWave++;
            enemiesRemainingInWave = GetEnemiesForWave(currentWave);
            OnWaveChanged?.Invoke(currentWave);

            SpawnManager spawnManager = FindObjectOfType<SpawnManager>();
            if (spawnManager != null)
            {
                spawnManager.StartWave(currentWave, enemiesRemainingInWave);
            }
        }

        private IEnumerator StartNextWaveAfterDelay()
        {
            yield return new WaitForSeconds(waveCooldown);

            if (currentState == GameState.Playing)
            {
                StartNextWave();
            }
        }

        private void UpdateCombo()
        {
            if (Time.time - lastKillTime <= comboResetDelay)
            {
                comboCount++;
            }
            else
            {
                comboCount = 1;
            }

            comboMultiplier = Mathf.Clamp(1 + comboCount / 3, 1, maxComboMultiplier);
            lastKillTime = Time.time;
            OnComboChanged?.Invoke(comboCount, comboMultiplier);
        }

        private void ResetCombo()
        {
            comboCount = 0;
            comboMultiplier = 1;
            OnComboChanged?.Invoke(comboCount, comboMultiplier);
        }

        private void SetState(GameState next)
        {
            currentState = next;
            OnGameStateChanged?.Invoke(currentState);
        }

        private void StopNextWaveRoutine()
        {
            if (nextWaveCoroutine != null)
            {
                StopCoroutine(nextWaveCoroutine);
                nextWaveCoroutine = null;
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}

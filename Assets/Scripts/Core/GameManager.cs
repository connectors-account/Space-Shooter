using System;
using System.Collections;
using UnityEngine;
using SpaceShooter.Environment;

namespace SpaceShooter.Core
{
    /// <summary>
    /// Game state machine and top-level orchestrator. Persistent singleton.
    /// Spawns the player, drives wave flow, controls pause/timescale and high score.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        #region Singleton
        public static GameManager Instance { get; private set; }
        #endregion

        #region State
        public enum GameState { MainMenu, Playing, Paused, GameOver, BossIntro }

        [SerializeField] private GameState _state = GameState.MainMenu;
        public GameState State => _state;
        #endregion

        #region Events
        public static event Action OnGameStart;
        public static event Action OnGameOver;
        public static event Action OnPause;
        public static event Action OnResume;
        public static event Action<GameState> OnStateChanged;
        #endregion

        #region Inspector Fields
        [Header("Prefabs")]
        [SerializeField] private GameObject _playerPrefab;

        [Header("References")]
        [SerializeField] private WaveManager _waveManager;

        [Header("Runtime")]
        [SerializeField] private Vector3 _playerSpawnPosition = new Vector3(0f, GameConstants.PLAYER_START_Y, 0f);
        #endregion

        #region Private
        private GameObject _playerInstance;
        private int _highScore;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadHighScore();
        }

        private void OnEnable()
        {
            PlayerHealthDeathBridge();
        }

        private void Update()
        {
            if (_state == GameState.Playing || _state == GameState.Paused)
            {
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    if (_state == GameState.Playing) PauseGame();
                    else if (_state == GameState.Paused) ResumeGame();
                }
            }
        }
        #endregion

        #region State Machine
        /// <summary>Changes the current state and broadcasts the change.</summary>
        private void SetState(GameState newState)
        {
            if (_state == newState) return;
            _state = newState;
            OnStateChanged?.Invoke(newState);
        }

        /// <summary>Begins (or restarts) a new game run.</summary>
        public void StartGame()
        {
            Time.timeScale = 1f;
            if (ScoreManager.Instance != null) ScoreManager.Instance.ResetScore();

            SpawnPlayer();
            SetState(GameState.Playing);
            OnGameStart?.Invoke();

            if (_waveManager == null) _waveManager = FindObjectOfType<WaveManager>();
            if (_waveManager != null) _waveManager.BeginWaves();
        }

        /// <summary>Pauses gameplay and freezes time.</summary>
        public void PauseGame()
        {
            if (_state != GameState.Playing) return;
            Time.timeScale = 0f;
            SetState(GameState.Paused);
            OnPause?.Invoke();
        }

        /// <summary>Resumes gameplay from pause.</summary>
        public void ResumeGame()
        {
            if (_state != GameState.Paused) return;
            Time.timeScale = 1f;
            SetState(GameState.Playing);
            OnResume?.Invoke();
        }

        /// <summary>Enters the boss intro state (used to gate spawning/UI).</summary>
        public void EnterBossIntro()
        {
            SetState(GameState.BossIntro);
        }

        /// <summary>Returns to the boss fight playing state after intro.</summary>
        public void ExitBossIntro()
        {
            SetState(GameState.Playing);
        }

        /// <summary>Triggers the game over sequence.</summary>
        public void TriggerGameOver()
        {
            if (_state == GameState.GameOver) return;
            SetState(GameState.GameOver);
            SaveHighScoreIfNeeded();

            if (CameraShake.Instance != null)
                CameraShake.Instance.Shake(0.6f, 0.4f);

            OnGameOver?.Invoke();
            StartCoroutine(FreezeAfterDeath());
        }

        private IEnumerator FreezeAfterDeath()
        {
            yield return new WaitForSecondsRealtime(1.2f);
            Time.timeScale = 0f;
        }

        /// <summary>Returns to the main menu and clears the active run.</summary>
        public void GoToMainMenu()
        {
            Time.timeScale = 1f;
            if (_playerInstance != null)
            {
                Destroy(_playerInstance);
                _playerInstance = null;
            }
            if (_waveManager != null) _waveManager.StopWaves();
            SetState(GameState.MainMenu);
        }
        #endregion

        #region Player
        /// <summary>Spawns (or respawns) the player prefab at the spawn position.</summary>
        private void SpawnPlayer()
        {
            if (_playerInstance != null)
            {
                Destroy(_playerInstance);
                _playerInstance = null;
            }

            if (_playerPrefab != null)
            {
                _playerInstance = Instantiate(_playerPrefab, _playerSpawnPosition, Quaternion.identity);
            }
            else
            {
                Debug.LogWarning("[GameManager] Player prefab not assigned; spawning empty ship object.");
                _playerInstance = new GameObject("Player");
                _playerInstance.transform.position = _playerSpawnPosition;
            }
        }

        /// <summary>Returns the current player instance (may be null).</summary>
        public GameObject GetPlayer() => _playerInstance;

        /// <summary>Convenience accessor for the player's world position.</summary>
        public Vector3 GetPlayerPosition()
        {
            return _playerInstance != null ? _playerInstance.transform.position : Vector3.zero;
        }

        private void PlayerHealthDeathBridge()
        {
            // Reserved hook point. PlayerHealth calls TriggerGameOver() directly on death.
        }
        #endregion

        #region High Score
        private void LoadHighScore()
        {
            _highScore = PlayerPrefs.GetInt(GameConstants.PREF_HIGH_SCORE, 0);
        }

        private void SaveHighScoreIfNeeded()
        {
            int current = ScoreManager.Instance != null ? ScoreManager.Instance.GetScore() : 0;
            if (current > _highScore)
            {
                _highScore = current;
                PlayerPrefs.SetInt(GameConstants.PREF_HIGH_SCORE, _highScore);
                PlayerPrefs.Save();
            }
        }

        /// <summary>Returns the persisted high score.</summary>
        public int GetHighScore() => _highScore;
        #endregion
    }
}

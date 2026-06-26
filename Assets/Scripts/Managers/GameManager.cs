using System;
using System.Collections;
using SpaceShooter.Core;
using SpaceShooter.Player;
using SpaceShooter.Weapons;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SpaceShooter.Managers
{
    /// <summary>
    /// The central game-flow controller. Owns the <see cref="GameState"/>, the score, wave
    /// progression and the win/lose conditions, and coordinates the other managers and the player.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        /// <summary>Global access point.</summary>
        public static GameManager Instance { get; private set; }

        private GameConfig _config;
        private PlayerController _player;
        private SpawnManager _spawnManager;

        private GameState _state = GameState.MainMenu;
        private int _score;
        private int _currentWave;
        private Coroutine _nextWaveRoutine;

        /// <summary>Name of the gameplay scene (must be in Build Settings).</summary>
        public const string GamePlaySceneName = "GamePlay";

        /// <summary>Name of the main menu scene (must be in Build Settings).</summary>
        public const string MainMenuSceneName = "MainMenu";

        /// <summary>Raised when the score changes.</summary>
        public event Action<int> ScoreChanged;

        /// <summary>Raised when the wave changes. Args: (waveNumber, isBossWave).</summary>
        public event Action<int, bool> WaveChanged;

        /// <summary>Raised when the overall game state changes.</summary>
        public event Action<GameState> StateChanged;

        /// <summary>The current game state.</summary>
        public GameState State => _state;

        /// <summary>Current score.</summary>
        public int Score => _score;

        /// <summary>Current wave number (1-based).</summary>
        public int CurrentWave => _currentWave;

        /// <summary>Shared configuration.</summary>
        public GameConfig Config => _config;

        /// <summary>
        /// Wires the game manager to the scene's player and spawn manager. Called by the bootstrap.
        /// </summary>
        public void Initialize(GameConfig config, PlayerController player, SpawnManager spawnManager)
        {
            Instance = this;
            _config = config;
            _player = player;
            _spawnManager = spawnManager;

            _player.PlayerDied += OnPlayerDied;
            _spawnManager.WaveCleared += OnWaveCleared;
        }

        /// <summary>
        /// Starts a brand-new game session from wave one.
        /// </summary>
        public void StartGame()
        {
            _score = 0;
            _currentWave = 0;
            ScoreChanged?.Invoke(_score);
            Time.timeScale = 1f;

            SetState(GameState.Playing);
            _player.SetControllable(true);
            AudioManager.Instance?.StartMusic();
            AdvanceToNextWave();
        }

        private void AdvanceToNextWave()
        {
            _currentWave++;
            if (_currentWave > _config.TotalWaves)
            {
                TriggerVictory();
                return;
            }

            bool isBoss = _currentWave % _config.BossEveryNWaves == 0;
            WaveChanged?.Invoke(_currentWave, isBoss);
            AudioManager.Instance?.PlayWaveStart();
            _spawnManager.BeginWave(_currentWave);
        }

        private void OnWaveCleared()
        {
            if (_state != GameState.Playing)
            {
                return;
            }

            if (_currentWave >= _config.TotalWaves)
            {
                TriggerVictory();
                return;
            }

            if (_nextWaveRoutine != null)
            {
                StopCoroutine(_nextWaveRoutine);
            }
            _nextWaveRoutine = StartCoroutine(NextWaveAfterDelay());
        }

        private IEnumerator NextWaveAfterDelay()
        {
            yield return new WaitForSeconds(_config.TimeBetweenWaves);
            if (_state == GameState.Playing)
            {
                AdvanceToNextWave();
            }
            _nextWaveRoutine = null;
        }

        /// <summary>
        /// Adds score, applying the player's current score multiplier.
        /// </summary>
        /// <param name="basePoints">Base points before any multiplier.</param>
        public void AddScore(int basePoints)
        {
            int multiplier = _player != null ? _player.ScoreMultiplier : 1;
            _score += basePoints * multiplier;
            ScoreChanged?.Invoke(_score);
        }

        private void OnPlayerDied()
        {
            TriggerGameOver();
        }

        private void TriggerGameOver()
        {
            if (_state == GameState.GameOver)
            {
                return;
            }
            SetState(GameState.GameOver);
            _player.SetControllable(false);
            AudioManager.Instance?.StopMusic();
        }

        private void TriggerVictory()
        {
            if (_state == GameState.Victory)
            {
                return;
            }
            SetState(GameState.Victory);
            _player.SetControllable(false);
            AudioManager.Instance?.StopMusic();
            ClearField();
        }

        /// <summary>
        /// Toggles between playing and paused.
        /// </summary>
        public void TogglePause()
        {
            if (_state == GameState.Playing)
            {
                PauseGame();
            }
            else if (_state == GameState.Paused)
            {
                ResumeGame();
            }
        }

        /// <summary>Pauses gameplay (sets time scale to zero).</summary>
        public void PauseGame()
        {
            if (_state != GameState.Playing)
            {
                return;
            }
            SetState(GameState.Paused);
            Time.timeScale = 0f;
            _player.SetControllable(false);
        }

        /// <summary>Resumes gameplay from a paused state.</summary>
        public void ResumeGame()
        {
            if (_state != GameState.Paused)
            {
                return;
            }
            SetState(GameState.Playing);
            Time.timeScale = 1f;
            _player.SetControllable(true);
        }

        /// <summary>Restarts the gameplay scene from scratch.</summary>
        public void RestartGame()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(GamePlaySceneName);
        }

        /// <summary>Returns to the main menu scene.</summary>
        public void GoToMainMenu()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(MainMenuSceneName);
        }

        /// <summary>Quits the application (or stops play mode in the editor).</summary>
        public void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void ClearField()
        {
            BulletManager.Instance?.ReleaseAll();
            _spawnManager?.ReleaseAll();
        }

        private void SetState(GameState newState)
        {
            _state = newState;
            StateChanged?.Invoke(_state);
        }

        private void OnDestroy()
        {
            if (_player != null)
            {
                _player.PlayerDied -= OnPlayerDied;
            }
            if (_spawnManager != null)
            {
                _spawnManager.WaveCleared -= OnWaveCleared;
            }
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}

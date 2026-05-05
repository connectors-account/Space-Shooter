using SpaceShooter.Enemies;
using SpaceShooter.PowerUps;
using SpaceShooter.UI;
using UnityEngine;

namespace SpaceShooter.Core
{
    public enum GameState
    {
        MainMenu,
        Playing,
        Paused,
        GameOver
    }

    public class GameManager : MonoBehaviour
    {
        public GameState CurrentState { get; private set; } = GameState.MainMenu;
        public int CurrentWave { get; private set; }
        public int Score { get; private set; }
        public float DifficultyScale => 1f + (CurrentWave - 1) * _config.DifficultyRampPerWave;
        public UIManager UiManager { get; private set; }

        private GameConfig _config;
        private ObjectPoolManager _pool;
        private WaveManager _waveManager;
        private Player.PlayerController _player;

        public void Initialize(GameConfig config, ObjectPoolManager pool, WaveManager waveManager, UIManager uiManager)
        {
            _config = config;
            _pool = pool;
            _waveManager = waveManager;
            UiManager = uiManager;
            CurrentState = GameState.MainMenu;
            Time.timeScale = 1f;
            UiManager.ShowMainMenu();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape) && (CurrentState == GameState.Playing || CurrentState == GameState.Paused))
            {
                TogglePause();
            }
        }

        public void RegisterPlayer(Player.PlayerController player)
        {
            _player = player;
        }

        public void StartGame()
        {
            Score = 0;
            CurrentWave = 0;
            CurrentState = GameState.Playing;
            Time.timeScale = 1f;
            UiManager.ShowHud();
            UiManager.RefreshScore(Score);
            UiManager.RefreshWave(0);
            Sound.SoundManager.Instance?.PlayMusic();
        }

        public void SetWave(int wave)
        {
            CurrentWave = wave;
            UiManager.RefreshWave(CurrentWave);
            Sound.SoundManager.Instance?.PlaySfx("wave");
        }

        public void AddScore(int amount)
        {
            Score += amount;
            UiManager.RefreshScore(Score);
        }

        public void TrySpawnPowerUp(Vector3 position)
        {
            if (Random.value > _config.PowerUpDropChance) return;
            var type = (PowerUpType)Random.Range(0, 3);
            var key = $"power_{type.ToString().ToLower()}";
            var obj = _pool.Get(key, position, Quaternion.identity);
            if (obj == null) return;
            obj.GetComponent<PowerUp.PowerUp>().Initialize(_pool, this, _config, type);
        }

        public void NotifyEnemyDespawned(EnemyController _)
        {
            _waveManager.OnEnemyDespawned();
        }

        public void TogglePause()
        {
            if (CurrentState == GameState.GameOver || CurrentState == GameState.MainMenu) return;

            if (CurrentState == GameState.Paused)
            {
                CurrentState = GameState.Playing;
                Time.timeScale = 1f;
                UiManager.HidePauseMenu();
            }
            else
            {
                CurrentState = GameState.Paused;
                Time.timeScale = 0f;
                UiManager.ShowPauseMenu();
            }
        }

        public void GameOver()
        {
            CurrentState = GameState.GameOver;
            Time.timeScale = 0f;
            UiManager.ShowGameOver(Score, CurrentWave);
            Sound.SoundManager.Instance?.PlaySfx("game_over");
        }

        public void RestartGame()
        {
            Time.timeScale = 1f;
            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
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
}

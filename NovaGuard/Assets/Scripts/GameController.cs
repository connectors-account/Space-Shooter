using System.Collections.Generic;
using UnityEngine;

namespace NovaGuard
{
    public enum GamePhase { Menu, Playing, Paused, GameOver }
    public enum PickupKind { Repair, Rapid, Shield }

    /// <summary>Owns run state. Every combat object is a child of worldRoot for clean restarts.</summary>
    public sealed class GameController : MonoBehaviour
    {
        public static GameController Instance { get; private set; }
        public PlayerShip playerPrefab;
        public EnemyShip[] enemyPrefabs;
        public Bullet playerBulletPrefab;
        public Bullet enemyBulletPrefab;
        public PowerUp[] powerUpPrefabs;
        public Transform worldRoot;
        public WaveDirector waves;
        public GameUI ui;
        public SoundBank sound;

        public GamePhase Phase { get; private set; } = GamePhase.Menu;
        public PlayerShip Player { get; private set; }
        public int Score { get; private set; }
        public int BestScore { get; private set; }
        public int Wave { get; private set; }
        public bool IsPlaying => Phase == GamePhase.Playing;
        public int EnemyCount => enemies.Count;
        public const float Left = -14f, Right = 14f, Bottom = -7.2f, Top = 6.7f;
        private readonly HashSet<EnemyShip> enemies = new HashSet<EnemyShip>();

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            Time.timeScale = 1f;
            BestScore = Mathf.Max(0, PlayerPrefs.GetInt("NovaGuard.Best", 0));
        }

        private void Start()
        {
            ui.Bind(this);
            ui.Show(Phase);
        }

        private void Update()
        {
            if (KeyboardInput.PausePressed)
            {
                if (IsPlaying) Pause();
                else if (Phase == GamePhase.Paused) Resume();
            }
            if (KeyboardInput.RestartPressed && Phase == GamePhase.GameOver) StartRun();
            if (KeyboardInput.MutePressed) sound.ToggleMute();
        }

        public void StartRun()
        {
            ClearRun();
            Score = 0;
            Wave = 0;
            Phase = GamePhase.Playing;
            Player = Instantiate(playerPrefab, new Vector3(0, -5.7f, 0), Quaternion.identity, worldRoot);
            ui.Show(Phase);
            waves.Begin();
        }

        public void Pause()
        {
            if (!IsPlaying) return;
            Phase = GamePhase.Paused;
            Time.timeScale = 0;
            ui.Show(Phase);
        }

        public void Resume()
        {
            if (Phase != GamePhase.Paused) return;
            Time.timeScale = 1;
            Phase = GamePhase.Playing;
            ui.Show(Phase);
        }

        public void MainMenu()
        {
            ClearRun();
            Phase = GamePhase.Menu;
            ui.Show(Phase);
        }

        public void EndRun()
        {
            if (!IsPlaying) return;
            Phase = GamePhase.GameOver;
            waves.Stop();
            SaveBest();
            sound.Play(SoundCue.GameOver);
            Time.timeScale = 0;
            ui.Show(Phase);
        }

        public void SetWave(int number)
        {
            Wave = number;
            ui.Announce("WAVE " + number, 2f);
            sound.Play(SoundCue.Wave);
        }

        public void AddScore(int amount)
        {
            if (!IsPlaying) return;
            Score += amount;
            if (Score > BestScore) BestScore = Score;
        }

        public void Register(EnemyShip enemy) => enemies.Add(enemy);
        public void Unregister(EnemyShip enemy) => enemies.Remove(enemy);

        public void SpawnEnemy(int type, Vector2 position)
        {
            EnemyShip enemy = Instantiate(enemyPrefabs[type], position, Quaternion.identity, worldRoot);
            enemy.Configure(Wave);
        }

        public void Fire(Vector2 position, Vector2 direction, bool friendly, float speed)
        {
            if (!IsPlaying) return;
            Bullet prefab = friendly ? playerBulletPrefab : enemyBulletPrefab;
            Bullet bullet = Instantiate(prefab, position, Quaternion.identity, worldRoot);
            bullet.Launch(direction, speed, friendly);
        }

        public void DropPowerUp(Vector3 position)
        {
            if (Random.value > 0.22f) return;
            Instantiate(powerUpPrefabs[Random.Range(0, powerUpPrefabs.Length)], position,
                Quaternion.identity, worldRoot);
        }

        public void SpawnEffect(Vector3 position, Color color, int count = 16)
        {
            HitBurst.Create(position, color, count, worldRoot);
        }

        private void SaveBest()
        {
            PlayerPrefs.SetInt("NovaGuard.Best", BestScore);
            PlayerPrefs.Save();
        }

        private void ClearRun()
        {
            waves.Stop();
            SaveBest();
            Time.timeScale = 1;
            // Disable immediately: Destroy is deferred until the end of the frame.
            foreach (Transform child in worldRoot)
            {
                child.gameObject.SetActive(false);
                Destroy(child.gameObject);
            }
            enemies.Clear();
            Player = null;
        }

        private void OnApplicationFocus(bool focused) { if (!focused) Pause(); }
        private void OnApplicationPause(bool paused) { if (paused) Pause(); }
        private void OnApplicationQuit() { SaveBest(); Time.timeScale = 1; }
        private void OnDestroy()
        {
            if (Instance != this) return;
            Instance = null;
            Time.timeScale = 1;
        }

        public void Quit()
        {
            SaveBest();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}

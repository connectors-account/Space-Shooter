using System.Collections.Generic;
using UnityEngine;

namespace OrbitBreaker
{
    public enum GamePhase { Title, Playing, Paused, GameOver }

    public sealed class GameController : MonoBehaviour
    {
        public GameAssets assets;
        public GameUI ui;
        public WaveDirector director;
        public AudioSource audioSource;
        public const float HalfWidth = 8.35f;
        public const float Bottom = -4.55f;
        public const float Top = 3.95f;
        public GamePhase Phase { get; private set; } = GamePhase.Title;
        public PlayerController Player { get; private set; }
        public Transform World { get; private set; }
        public int Score { get; private set; }
        public int Wave { get; set; }
        public int Best { get; private set; }
        public bool Muted { get; private set; }
        public int EnemyCount => enemies.Count;
        private readonly HashSet<Enemy> enemies = new HashSet<Enemy>();
        private int kills;
        private float nextEnemySound;

        private void Awake()
        {
            Time.timeScale = 1f;
            AudioListener.pause = false;
            Application.targetFrameRate = 60;
            Best = PlayerPrefs.GetInt("OrbitBreaker.Best", 0);
            Muted = PlayerPrefs.GetInt("OrbitBreaker.Muted", 0) == 1;
            audioSource.mute = Muted;
            ConfigureCollisionLayers();
        }

        private void Start() { ui.Refresh(); }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.M)) ToggleMute();
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
            {
                if (Phase == GamePhase.Playing) SetPaused(true);
                else if (Phase == GamePhase.Paused) SetPaused(false);
            }
            if (Input.GetKeyDown(KeyCode.Return) && (Phase == GamePhase.Title || Phase == GamePhase.GameOver)) BeginGame();
            else if (Input.GetKeyDown(KeyCode.R) && (Phase == GamePhase.GameOver || Phase == GamePhase.Paused)) BeginGame();
        }

        private void OnApplicationFocus(bool focused)
        {
            if (!focused && Phase == GamePhase.Playing) SetPaused(true);
        }

        public void BeginGame()
        {
            director.StopAllCoroutines();
            Time.timeScale = 1f;
            AudioListener.pause = false;
            audioSource.Stop();
            ClearWorld();
            World = new GameObject("Active Run").transform;
            Score = 0; Wave = 0; kills = 0; nextEnemySound = 0;
            Phase = GamePhase.Playing;
            Player = Instantiate(assets.player, new Vector3(0f, -3.6f, 0f), Quaternion.identity, World);
            Player.Initialize(this);
            ui.Refresh();
            Play(assets.ui);
            director.Begin(this);
        }

        public void SetPaused(bool paused)
        {
            if (Phase != GamePhase.Playing && Phase != GamePhase.Paused) return;
            Phase = paused ? GamePhase.Paused : GamePhase.Playing;
            if (Player != null) Player.ClearInput();
            Time.timeScale = paused ? 0f : 1f;
            AudioListener.pause = paused;
            ui.Refresh();
        }

        public void ReturnToTitle()
        {
            director.StopAllCoroutines();
            Time.timeScale = 1f;
            AudioListener.pause = false;
            audioSource.Stop();
            ClearWorld();
            Phase = GamePhase.Title;
            ui.Refresh();
        }

        private void ClearWorld()
        {
            if (World != null)
            {
                // Deactivate before deferred Destroy: old colliders/coroutines cannot hit the new run.
                World.gameObject.SetActive(false);
                Destroy(World.gameObject);
            }
            World = null; Player = null; enemies.Clear();
        }

        public void EndGame()
        {
            if (Phase != GamePhase.Playing) return;
            Phase = GamePhase.GameOver;
            director.StopAllCoroutines();
            if (Player != null) Player.ClearInput();
            Best = Mathf.Max(Best, Score);
            PlayerPrefs.SetInt("OrbitBreaker.Best", Best);
            PlayerPrefs.Save();
            Play(assets.gameOver);
            Time.timeScale = 0f;
            ui.Refresh();
        }

        public void Register(Enemy enemy) { enemies.Add(enemy); }
        public void Unregister(Enemy enemy) { enemies.Remove(enemy); }

        public void EnemyKilled(Enemy enemy)
        {
            if (Phase != GamePhase.Playing) return;
            Score += enemy.Points;
            kills++;
            Burst(enemy.transform.position, new Color(1f, .5f, .23f));
            Play(assets.explode, .65f);
            if (kills % 3 == 0)
            {
                int index = (kills / 3 - 1) % assets.powerups.Length;
                Powerup pickup = Instantiate(assets.powerups[index], enemy.transform.position, Quaternion.identity, World);
                pickup.Initialize(this);
            }
        }

        public void Fire(Vector2 origin, Vector2 direction, bool friendly, float speed)
        {
            if (Phase != GamePhase.Playing) return;
            Bullet bullet = Instantiate(friendly ? assets.playerBullet : assets.enemyBullet, origin,
                Quaternion.identity, World);
            bullet.Initialize(this, direction, speed, friendly);
        }

        public void EnemyFireSound()
        {
            // One shared rate limiter avoids multiplying loudness during radial patterns.
            if (Time.time < nextEnemySound) return;
            nextEnemySound = Time.time + .12f;
            Play(assets.enemyShot, .26f);
        }

        public void Burst(Vector3 position, Color color)
        {
            var effect = new GameObject("Explosion").AddComponent<BurstEffect>();
            effect.transform.SetParent(World, false);
            effect.transform.position = position;
            effect.Initialize(assets.spark, color);
        }

        public void Play(AudioClip clip, float volume = .65f)
        {
            if (clip != null) audioSource.PlayOneShot(clip, volume);
        }

        public void ToggleMute()
        {
            Muted = !Muted;
            audioSource.mute = Muted;
            PlayerPrefs.SetInt("OrbitBreaker.Muted", Muted ? 1 : 0);
            PlayerPrefs.Save();
            ui.Refresh();
        }

        public void Quit()
        {
            PlayerPrefs.Save();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void OnDestroy()
        {
            Time.timeScale = 1f;
            AudioListener.pause = false;
        }

        public static void ConfigureCollisionLayers()
        {
            // 8 Player, 9 Enemy, 10 PlayerBullet, 11 EnemyBullet, 12 Powerup.
            // Every actor has a dynamic, gravity-free Rigidbody2D and a trigger collider.
            for (int a = 8; a <= 12; a++)
                for (int b = a; b <= 12; b++)
                {
                    bool collide = (a == 8 && (b == 9 || b == 11 || b == 12)) || (a == 9 && b == 10);
                    Physics2D.IgnoreLayerCollision(a, b, !collide);
                }
        }
    }
}

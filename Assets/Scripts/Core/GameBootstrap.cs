using UnityEngine;
using SpaceShooter.Utilities;
using SpaceShooter.Player;
using SpaceShooter.Enemy;
using SpaceShooter.Weapons;
using SpaceShooter.PowerUps;
using SpaceShooter.Background;
using SpaceShooter.UI;
using SpaceShooter.Scoring;
using SpaceShooter.Audio;

namespace SpaceShooter.Core
{
    /// <summary>
    /// Builds the entire playable Game scene at runtime: singletons, camera,
    /// object pools (with procedurally-generated prefab templates), the player,
    /// the parallax background, the wave manager / spawner and the HUD, pause
    /// and game-over UIs.
    ///
    /// Drop a single empty GameObject with this component into the Game scene
    /// (see README) and everything is created — no manual prefab or canvas
    /// wiring required. All settings have sensible defaults.
    /// </summary>
    public class GameBootstrap : MonoBehaviour
    {
        [Header("Camera")]
        [SerializeField] private float orthographicSize = 6f;
        [SerializeField] private Color backgroundColour = new Color(0.02f, 0.02f, 0.06f);

        [Header("Pool sizes")]
        [SerializeField] private int playerBulletPool = 48;
        [SerializeField] private int enemyBulletPool = 120;
        [SerializeField] private int dronePool = 24;
        [SerializeField] private int fighterPool = 16;
        [SerializeField] private int bomberPool = 8;
        [SerializeField] private int bossPool = 1;
        [SerializeField] private int explosionPool = 24;
        [SerializeField] private int powerUpPool = 6;

        private void Awake()
        {
            EnsureSingletons();
            SetupCamera();
        }

        private void Start()
        {
            SetupPools();
            var player = SetupPlayer();
            SetupBackground();
            SetupWaves(player);
            SetupUI();

            if (ScoreManager.Instance != null && ScoreManager.Instance.Score == 0)
                ScoreManager.Instance.ResetForNewGame();

            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayMusic(Constants.MusicGame);

            if (GameManager.Instance != null)
                GameManager.Instance.EnterPlayingState();
        }

        // -----------------------------------------------------------------
        // Singletons
        // -----------------------------------------------------------------
        private void EnsureSingletons()
        {
            if (GameManager.Instance == null) new GameObject("GameManager").AddComponent<GameManager>();
            if (SceneLoader.Instance == null) new GameObject("SceneLoader").AddComponent<SceneLoader>();
            if (AudioManager.Instance == null) new GameObject("AudioManager").AddComponent<AudioManager>();
            if (ScoreManager.Instance == null) new GameObject("ScoreManager").AddComponent<ScoreManager>();
            if (ObjectPool.Instance == null) new GameObject("ObjectPool").AddComponent<ObjectPool>();
        }

        // -----------------------------------------------------------------
        // Camera
        // -----------------------------------------------------------------
        private void SetupCamera()
        {
            var cam = Camera.main;
            if (cam == null)
            {
                var go = new GameObject("Main Camera");
                go.tag = "MainCamera";
                cam = go.AddComponent<Camera>();
            }
            cam.orthographic = true;
            cam.orthographicSize = orthographicSize;
            cam.backgroundColor = backgroundColour;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.transform.position = new Vector3(0f, 0f, -10f);

            if (cam.GetComponent<CameraShake>() == null)
                cam.gameObject.AddComponent<CameraShake>();

            if (cam.GetComponent<AudioListener>() == null)
                cam.gameObject.AddComponent<AudioListener>();
        }

        // -----------------------------------------------------------------
        // Pools & prefab templates
        // -----------------------------------------------------------------
        private void SetupPools()
        {
            var pool = ObjectPool.Instance;
            if (pool == null) return;

            pool.EnsurePool(Constants.PoolPlayerBullet, BuildBulletTemplate(Constants.TagPlayerBullet, new Color(0.4f, 0.9f, 1f)), playerBulletPool);
            pool.EnsurePool(Constants.PoolEnemyBullet, BuildBulletTemplate(Constants.TagEnemyBullet, new Color(1f, 0.5f, 0.35f)), enemyBulletPool);

            pool.EnsurePool(Constants.PoolEnemyDrone, BuildEnemyTemplate<EnemyDrone>("DroneTemplate"), dronePool);
            pool.EnsurePool(Constants.PoolEnemyFighter, BuildEnemyTemplate<EnemyFighter>("FighterTemplate"), fighterPool);
            pool.EnsurePool(Constants.PoolEnemyBomber, BuildEnemyTemplate<EnemyBomber>("BomberTemplate"), bomberPool);
            pool.EnsurePool(Constants.PoolEnemyBoss, BuildEnemyTemplate<EnemyBoss>("BossTemplate"), bossPool);

            pool.EnsurePool(Constants.PoolExplosion, BuildExplosionTemplate(), explosionPool);

            pool.EnsurePool(Constants.PoolPowerUpShield, BuildPowerUpTemplate<PowerUpShield>("ShieldTemplate"), powerUpPool);
            pool.EnsurePool(Constants.PoolPowerUpRapidFire, BuildPowerUpTemplate<PowerUpRapidFire>("RapidTemplate"), powerUpPool);
            pool.EnsurePool(Constants.PoolPowerUpTripleShot, BuildPowerUpTemplate<PowerUpTripleShot>("TripleTemplate"), powerUpPool);
            pool.EnsurePool(Constants.PoolPowerUpBomb, BuildPowerUpTemplate<PowerUpBomb>("BombTemplate"), powerUpPool);
            pool.EnsurePool(Constants.PoolPowerUpSpeed, BuildPowerUpTemplate<PowerUpSpeed>("SpeedTemplate"), powerUpPool);
        }

        private GameObject BuildBulletTemplate(string tag, Color colour)
        {
            var go = new GameObject(tag + "Template");
            SafeSetTag(go, tag);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteGenerator.CreateBulletSprite(colour);
            var col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.12f;
            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.bodyType = RigidbodyType2D.Kinematic;
            go.AddComponent<Bullet>();
            go.SetActive(false);
            HideTemplate(go);
            return go;
        }

        private GameObject BuildEnemyTemplate<T>(string name) where T : EnemyBase
        {
            var go = new GameObject(name);
            var sr = go.AddComponent<SpriteRenderer>();
            var col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.45f;
            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.bodyType = RigidbodyType2D.Kinematic;
            go.AddComponent<T>();
            SafeSetTag(go, typeof(T) == typeof(EnemyBoss) ? Constants.TagBoss : Constants.TagEnemy);
            go.SetActive(false);
            HideTemplate(go);
            return go;
        }

        private GameObject BuildExplosionTemplate()
        {
            var go = new GameObject("ExplosionTemplate");
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteGenerator.CreateExplosionSprite();
            go.AddComponent<ExplosionVFX>();
            go.SetActive(false);
            HideTemplate(go);
            return go;
        }

        private GameObject BuildPowerUpTemplate<T>(string name) where T : PowerUpBase
        {
            var go = new GameObject(name);
            var sr = go.AddComponent<SpriteRenderer>();
            var col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.4f;
            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.bodyType = RigidbodyType2D.Kinematic;
            go.AddComponent<T>();
            SafeSetTag(go, Constants.TagPowerUp);
            go.SetActive(false);
            HideTemplate(go);
            return go;
        }

        private void HideTemplate(GameObject go)
        {
            // Park templates under this bootstrap so they are tidy in the hierarchy.
            go.transform.SetParent(transform, false);
        }

        // -----------------------------------------------------------------
        // Player
        // -----------------------------------------------------------------
        private PlayerController SetupPlayer()
        {
            var go = new GameObject("Player");
            SafeSetTag(go, Constants.TagPlayer);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteGenerator.CreatePlayerSprite();
            sr.sortingOrder = 2;

            var col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.35f;

            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.bodyType = RigidbodyType2D.Kinematic;

            go.AddComponent<PlayerInputHandler>();
            var controller = go.AddComponent<PlayerController>();
            go.AddComponent<PlayerHealth>();
            go.AddComponent<PlayerShooter>();
            go.AddComponent<Bomb>();

            var cam = Camera.main;
            float startY = cam != null ? cam.transform.position.y - orthographicSize * 0.6f : -3.5f;
            go.transform.position = new Vector3(0f, startY, 0f);

            return controller;
        }

        // -----------------------------------------------------------------
        // Background
        // -----------------------------------------------------------------
        private void SetupBackground()
        {
            var go = new GameObject("ParallaxBackground");
            go.transform.position = Vector3.zero;
            go.AddComponent<ParallaxBackground>();
        }

        // -----------------------------------------------------------------
        // Waves
        // -----------------------------------------------------------------
        private void SetupWaves(PlayerController player)
        {
            var spawnerGo = new GameObject("EnemySpawner");
            var spawner = spawnerGo.AddComponent<EnemySpawner>();
            if (player != null) spawner.SetPlayer(player.transform);

            var waveGo = new GameObject("WaveManager");
            var wave = waveGo.AddComponent<WaveManager>();
            // WaveManager finds the spawner in Start().
        }

        // -----------------------------------------------------------------
        // UI
        // -----------------------------------------------------------------
        private void SetupUI()
        {
            new GameObject("HUD").AddComponent<HUDController>();
            new GameObject("PauseMenu").AddComponent<PauseMenuController>();
            new GameObject("GameOverMenu").AddComponent<GameOverController>();
        }

        // -----------------------------------------------------------------
        // Helpers
        // -----------------------------------------------------------------
        private void SafeSetTag(GameObject go, string tag)
        {
            try
            {
                go.tag = tag;
            }
            catch (UnityEngine.UnityException)
            {
                // Tag not defined in the Tag Manager – fall back to Untagged.
                // See README for the required tags. CompareTag checks still work
                // once the tags are added in Project Settings.
                Debug.LogWarning($"[GameBootstrap] Tag '{tag}' is not defined. Add it under " +
                                 "Edit ▸ Project Settings ▸ Tags and Layers (see README).");
            }
        }
    }
}

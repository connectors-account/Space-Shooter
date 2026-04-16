using SpaceShooter.Core;
using SpaceShooter.Managers;
using SpaceShooter.UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SpaceShooter.Gameplay
{
    public class GameBootstrap : MonoBehaviour
    {
        public const string PlayerBulletPool = "player_bullet";
        public const string EnemyBulletPool = "enemy_bullet";
        public const string EnemyBasicPool = "enemy_basic";
        public const string EnemyZigzagPool = "enemy_zigzag";
        public const string EnemyTankPool = "enemy_tank";
        public const string EnemySpinnerPool = "enemy_spinner";
        public const string RapidPool = "power_rapid";
        public const string ShieldPool = "power_shield";
        public const string HealthPool = "power_health";
        public const string SpreadPool = "power_spread";

        public static string GetEnemyPool(EnemyType type)
        {
            switch (type)
            {
                case EnemyType.Basic:
                    return EnemyBasicPool;
                case EnemyType.Zigzag:
                    return EnemyZigzagPool;
                case EnemyType.Tank:
                    return EnemyTankPool;
                default:
                    return EnemySpinnerPool;
            }
        }

        public static string GetPowerUpPool(PowerUpType type)
        {
            switch (type)
            {
                case PowerUpType.RapidFire:
                    return RapidPool;
                case PowerUpType.Shield:
                    return ShieldPool;
                case PowerUpType.HealthRestore:
                    return HealthPool;
                default:
                    return SpreadPool;
            }
        }

        public static Color GetPowerUpColor(PowerUpType type)
        {
            switch (type)
            {
                case PowerUpType.RapidFire:
                    return new Color(1f, 0.6f, 0.2f);
                case PowerUpType.Shield:
                    return new Color(0.3f, 0.8f, 1f);
                case PowerUpType.HealthRestore:
                    return new Color(0.35f, 1f, 0.45f);
                default:
                    return new Color(0.9f, 0.55f, 1f);
            }
        }

        private void Awake()
        {
            Time.timeScale = 1f;
            EnsureCamera();
            EnsureSessionAndAudio();
            EnsureEventSystem();

            var spriteFactory = new SpriteFactory();

            var background = new GameObject("ParallaxBackground").AddComponent<ParallaxScroller>();
            background.Setup(spriteFactory.CreateRectSprite(16, 16, Color.white), spriteFactory.CreateRectSprite(16, 16, Color.gray));

            var pool = new GameObject("PoolManager").AddComponent<PoolManager>();
            RegisterPools(pool, spriteFactory);

            var player = CreatePlayer(spriteFactory);
            var spawner = new GameObject("EnemySpawner").AddComponent<EnemySpawner>();
            spawner.Setup(pool, GameSession.Instance, player.transform);

            player.Setup(pool, spawner);

            var ui = new GameObject("GameUI").AddComponent<GameUIController>();
            ui.Setup(GameSession.Instance, player, spawner);

            if (!GameSession.Instance.IsRunActive)
            {
                GameSession.Instance.StartNewRun();
            }
        }

        private static void EnsureCamera()
        {
            if (Camera.main != null)
            {
                Camera.main.orthographic = true;
                Camera.main.orthographicSize = 5.6f;
                Camera.main.backgroundColor = new Color(0.03f, 0.03f, 0.08f);
                return;
            }

            var cam = new GameObject("Main Camera").AddComponent<Camera>();
            cam.tag = "MainCamera";
            cam.orthographic = true;
            cam.orthographicSize = 5.6f;
            cam.backgroundColor = new Color(0.03f, 0.03f, 0.08f);
            cam.transform.position = new Vector3(0f, 0f, -10f);
        }

        private static void EnsureSessionAndAudio()
        {
            if (GameSession.Instance == null)
            {
                new GameObject("GameSession").AddComponent<GameSession>();
            }

            if (AudioManager.Instance == null)
            {
                new GameObject("AudioManager").AddComponent<AudioManager>();
            }
        }

        private static void EnsureEventSystem()
        {
            if (Object.FindObjectOfType<EventSystem>() != null)
            {
                return;
            }

            var eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();
        }

        private static void RegisterPools(PoolManager pool, SpriteFactory sprites)
        {
            pool.RegisterPool(PlayerBulletPool, CreateProjectilePrefab("PlayerBullet", sprites.CreateRectSprite(6, 12, new Color(0.9f, 1f, 0.3f))), 48);
            pool.RegisterPool(EnemyBulletPool, CreateProjectilePrefab("EnemyBullet", sprites.CreateRectSprite(6, 12, new Color(1f, 0.45f, 0.45f))), 80);

            pool.RegisterPool(EnemyBasicPool, CreateEnemyPrefab("EnemyBasic", EnemyType.Basic, sprites.CreateTriangleSprite(30, 24, new Color(0.95f, 0.35f, 0.35f))), 18);
            pool.RegisterPool(EnemyZigzagPool, CreateEnemyPrefab("EnemyZigzag", EnemyType.Zigzag, sprites.CreateTriangleSprite(28, 24, new Color(0.95f, 0.75f, 0.35f))), 16);
            pool.RegisterPool(EnemyTankPool, CreateEnemyPrefab("EnemyTank", EnemyType.Tank, sprites.CreateRectSprite(34, 24, new Color(0.55f, 0.85f, 1f))), 14);
            pool.RegisterPool(EnemySpinnerPool, CreateEnemyPrefab("EnemySpinner", EnemyType.Spinner, sprites.CreateDiamondSprite(28, 28, new Color(0.85f, 0.55f, 1f))), 14);

            pool.RegisterPool(RapidPool, CreatePowerUpPrefab("PowerRapid", PowerUpType.RapidFire, sprites.CreateDiamondSprite(20, 20, GetPowerUpColor(PowerUpType.RapidFire))), 6);
            pool.RegisterPool(ShieldPool, CreatePowerUpPrefab("PowerShield", PowerUpType.Shield, sprites.CreateDiamondSprite(20, 20, GetPowerUpColor(PowerUpType.Shield))), 6);
            pool.RegisterPool(HealthPool, CreatePowerUpPrefab("PowerHealth", PowerUpType.HealthRestore, sprites.CreateDiamondSprite(20, 20, GetPowerUpColor(PowerUpType.HealthRestore))), 6);
            pool.RegisterPool(SpreadPool, CreatePowerUpPrefab("PowerSpread", PowerUpType.SpreadShot, sprites.CreateDiamondSprite(20, 20, GetPowerUpColor(PowerUpType.SpreadShot))), 6);
        }

        private static PlayerController CreatePlayer(SpriteFactory sprites)
        {
            var playerGo = new GameObject("Player");
            playerGo.transform.position = new Vector3(0f, -4f, 0f);

            var spriteRenderer = playerGo.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = sprites.CreateTriangleSprite(36, 30, new Color(0.4f, 0.95f, 1f));
            spriteRenderer.sortingOrder = 5;

            var collider = playerGo.AddComponent<CircleCollider2D>();
            collider.radius = 0.35f;
            collider.isTrigger = true;

            return playerGo.AddComponent<PlayerController>();
        }

        private static GameObject CreateProjectilePrefab(string name, Sprite sprite)
        {
            var go = new GameObject(name);
            go.SetActive(false);
            go.AddComponent<PooledIdentity>();
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = 8;
            go.AddComponent<BoxCollider2D>().isTrigger = true;
            go.AddComponent<Projectile>();
            return go;
        }

        private static GameObject CreateEnemyPrefab(string name, EnemyType type, Sprite sprite)
        {
            var go = new GameObject(name);
            go.SetActive(false);
            go.AddComponent<PooledIdentity>();
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = 6;
            go.AddComponent<CircleCollider2D>().isTrigger = true;
            go.AddComponent<EnemyController>();
            return go;
        }

        private static GameObject CreatePowerUpPrefab(string name, PowerUpType type, Sprite sprite)
        {
            var go = new GameObject(name);
            go.SetActive(false);
            go.AddComponent<PooledIdentity>();
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = 7;
            go.AddComponent<CircleCollider2D>().isTrigger = true;
            var pickup = go.AddComponent<PowerUpPickup>();
            pickup.Configure(type, GetPowerUpColor(type));
            return go;
        }

        private class SpriteFactory
        {
            public Sprite CreateRectSprite(int width, int height, Color color)
            {
                var tex = CreateTexture(width, height, (x, y) => color);
                return ToSprite(tex);
            }

            public Sprite CreateTriangleSprite(int width, int height, Color color)
            {
                var tex = CreateTexture(width, height, (x, y) =>
                {
                    var center = width / 2f;
                    var t = y / (float)height;
                    var halfSpan = Mathf.Lerp(1f, width * 0.48f, t);
                    return Mathf.Abs(x - center) <= halfSpan ? color : Color.clear;
                });
                return ToSprite(tex);
            }

            public Sprite CreateDiamondSprite(int width, int height, Color color)
            {
                var tex = CreateTexture(width, height, (x, y) =>
                {
                    var nx = Mathf.Abs((x - width / 2f) / (width / 2f));
                    var ny = Mathf.Abs((y - height / 2f) / (height / 2f));
                    return nx + ny <= 1f ? color : Color.clear;
                });
                return ToSprite(tex);
            }

            private static Texture2D CreateTexture(int width, int height, System.Func<int, int, Color> pixelGenerator)
            {
                var tex = new Texture2D(width, height, TextureFormat.RGBA32, false)
                {
                    filterMode = FilterMode.Point,
                    wrapMode = TextureWrapMode.Clamp
                };

                for (var y = 0; y < height; y++)
                {
                    for (var x = 0; x < width; x++)
                    {
                        tex.SetPixel(x, y, pixelGenerator(x, y));
                    }
                }

                tex.Apply();
                return tex;
            }

            private static Sprite ToSprite(Texture2D texture)
            {
                return Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 24f);
            }
        }
    }
}

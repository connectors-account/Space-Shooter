using SpaceShooter.Enemies;
using SpaceShooter.Player;
using SpaceShooter.PowerUps;
using SpaceShooter.Projectiles;
using SpaceShooter.UI;
using SpaceShooter.Visual;
using UnityEngine;

namespace SpaceShooter.Core
{
    /// <summary>
    /// Creates a complete playable scene at runtime using simple geometric visuals.
    /// This keeps the project portable and avoids hidden scene dependencies.
    /// </summary>
    public class GameBootstrapper : MonoBehaviour
    {
        private GameConfig _config;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void AutoCreate()
        {
            if (FindObjectOfType<GameBootstrapper>() != null) return;
            var bootstrapObject = new GameObject("GameBootstrapper");
            bootstrapObject.AddComponent<GameBootstrapper>();
        }

        private void Awake()
        {
            _config = new GameConfig();

            SetupCamera();
            var pool = new GameObject("ObjectPoolManager").AddComponent<ObjectPoolManager>();
            var gameManager = new GameObject("GameManager").AddComponent<GameManager>();
            var waveManager = new GameObject("WaveManager").AddComponent<WaveManager>();
            var uiManager = new GameObject("UIManager").AddComponent<UIManager>();
            var effectManager = new GameObject("EffectManager").AddComponent<EffectManager>();
            new GameObject("SoundManager").AddComponent<Sound.SoundManager>();

            uiManager.Initialize(gameManager);
            waveManager.Initialize(gameManager, pool, _config);
            gameManager.Initialize(_config, pool, waveManager, uiManager);
            effectManager.Initialize(pool);

            BuildParallaxBackground();
            RegisterPools(pool, gameManager);
            SpawnPlayer(gameManager, pool);
        }

        private static void SetupCamera()
        {
            var cam = Camera.main;
            if (cam == null)
            {
                cam = new GameObject("Main Camera").AddComponent<Camera>();
                cam.tag = "MainCamera";
            }

            cam.transform.position = new Vector3(0f, 0f, -10f);
            cam.orthographic = true;
            cam.orthographicSize = 5.4f;
            cam.backgroundColor = new Color(0.03f, 0.04f, 0.11f);
        }

        private void RegisterPools(ObjectPoolManager pool, GameManager gameManager)
        {
            var rectSprite = RuntimeShapeFactory.CreateRectangleSprite(24, 24);
            var shipSprite = RuntimeShapeFactory.CreateDiamondSprite(48);
            var circleSprite = RuntimeShapeFactory.CreateCircleSprite(32);

            var playerBulletPrefab = CreateProjectilePrefab("PlayerBulletPrefab", rectSprite, new Color(0.3f, 1f, 1f), new Vector2(0.18f, 0.5f));
            var enemyBulletPrefab = CreateProjectilePrefab("EnemyBulletPrefab", rectSprite, new Color(1f, 0.4f, 0.4f), new Vector2(0.22f, 0.45f));
            pool.RegisterPool("bullet_player", playerBulletPrefab, 64);
            pool.RegisterPool("bullet_enemy", enemyBulletPrefab, 96);

            var gruntPrefab = CreateEnemyPrefab("EnemyGruntPrefab", shipSprite, new Color(1f, 0.45f, 0.45f), new Vector2(1.0f, 1.0f));
            var sinePrefab = CreateEnemyPrefab("EnemySinePrefab", shipSprite, new Color(1f, 0.75f, 0.3f), new Vector2(0.9f, 0.9f));
            var shooterPrefab = CreateEnemyPrefab("EnemyShooterPrefab", shipSprite, new Color(1f, 0.2f, 0.7f), new Vector2(1.15f, 1.15f));
            pool.RegisterPool("enemy_grunt", gruntPrefab, 18);
            pool.RegisterPool("enemy_sine", sinePrefab, 14);
            pool.RegisterPool("enemy_shooter", shooterPrefab, 10);

            var weaponPower = CreatePowerUpPrefab("PowerWeaponPrefab", circleSprite, new Color(0.35f, 1f, 0.55f));
            var healthPower = CreatePowerUpPrefab("PowerHealthPrefab", circleSprite, new Color(0.35f, 0.8f, 1f));
            var shieldPower = CreatePowerUpPrefab("PowerShieldPrefab", circleSprite, new Color(0.95f, 0.95f, 0.35f));
            pool.RegisterPool("power_weaponupgrade", weaponPower, 6);
            pool.RegisterPool("power_health", healthPower, 6);
            pool.RegisterPool("power_shield", shieldPower, 6);

            var hitFx = CreateEffectPrefab("HitFxPrefab", circleSprite, new Color(1f, 0.9f, 0.5f));
            var explosionFx = CreateEffectPrefab("ExplosionFxPrefab", circleSprite, new Color(1f, 0.45f, 0.25f));
            pool.RegisterPool("fx_hit", hitFx, 24);
            pool.RegisterPool("fx_explosion", explosionFx, 24);
        }

        private void SpawnPlayer(GameManager gameManager, ObjectPoolManager pool)
        {
            var playerObj = new GameObject("Player");
            playerObj.transform.position = new Vector3(0f, -3.5f, 0f);

            var renderer = playerObj.AddComponent<SpriteRenderer>();
            renderer.sprite = RuntimeShapeFactory.CreateDiamondSprite(52);
            renderer.color = new Color(0.45f, 0.95f, 1f);
            renderer.sortingOrder = 12;

            var collider = playerObj.AddComponent<CircleCollider2D>();
            collider.isTrigger = true;
            collider.radius = 0.42f;

            var rb = playerObj.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.isKinematic = true;

            var health = playerObj.AddComponent<PlayerHealth>();
            var controller = playerObj.AddComponent<PlayerController>();
            controller.Initialize(gameManager, pool, _config);

            gameManager.RegisterPlayer(controller);
        }

        private void BuildParallaxBackground()
        {
            var scroller = new GameObject("ParallaxScroller").AddComponent<ParallaxScroller>();
            var layer1 = CreateBackgroundStrip("BG_Layer_1", new Color(0.04f, 0.08f, 0.18f), -1f, 26f);
            var layer2 = CreateBackgroundStrip("BG_Layer_2", new Color(0.07f, 0.13f, 0.24f), -0.5f, 24f);
            var layer3 = CreateBackgroundStrip("BG_Layer_3", new Color(0.09f, 0.18f, 0.3f), 0f, 20f);

            scroller.AddLayer(layer1.transform, _config.BackgroundScrollSpeed, 11f, -11f);
            scroller.AddLayer(layer2.transform, _config.MidgroundScrollSpeed, 11f, -11f);
            scroller.AddLayer(layer3.transform, _config.ForegroundScrollSpeed, 11f, -11f);
        }

        private static GameObject CreateBackgroundStrip(string name, Color color, float z, float height)
        {
            var obj = new GameObject(name);
            obj.transform.position = new Vector3(0f, 0f, z);
            var sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = RuntimeShapeFactory.CreateRectangleSprite(64, 64);
            sr.drawMode = SpriteDrawMode.Tiled;
            sr.size = new Vector2(22f, height);
            sr.color = color;
            sr.sortingOrder = -10;
            return obj;
        }

        private static GameObject CreateProjectilePrefab(string name, Sprite sprite, Color color, Vector2 scale)
        {
            var prefab = new GameObject(name);
            prefab.SetActive(false);

            var sr = prefab.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color = color;
            sr.sortingOrder = 20;

            var col = prefab.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = Vector2.one;

            prefab.transform.localScale = scale;
            prefab.AddComponent<Projectile>();
            return prefab;
        }

        private static GameObject CreateEnemyPrefab(string name, Sprite sprite, Color color, Vector2 scale)
        {
            var prefab = new GameObject(name);
            prefab.SetActive(false);

            var sr = prefab.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color = color;
            sr.sortingOrder = 15;

            prefab.transform.localScale = new Vector3(scale.x, scale.y, 1f);

            var col = prefab.AddComponent<CircleCollider2D>();
            col.isTrigger = true;

            var rb = prefab.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.isKinematic = true;

            prefab.AddComponent<EnemyController>();
            return prefab;
        }

        private static GameObject CreatePowerUpPrefab(string name, Sprite sprite, Color color)
        {
            var prefab = new GameObject(name);
            prefab.SetActive(false);

            var sr = prefab.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color = color;
            sr.sortingOrder = 16;

            prefab.transform.localScale = Vector3.one * 0.55f;

            var col = prefab.AddComponent<CircleCollider2D>();
            col.isTrigger = true;

            var rb = prefab.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.isKinematic = true;

            prefab.AddComponent<PowerUp.PowerUp>();
            return prefab;
        }

        private static GameObject CreateEffectPrefab(string name, Sprite sprite, Color color)
        {
            var prefab = new GameObject(name);
            prefab.SetActive(false);
            var sr = prefab.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color = color;
            sr.sortingOrder = 30;
            prefab.AddComponent<EffectPulse>();
            return prefab;
        }
    }
}

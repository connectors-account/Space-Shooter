#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using SpaceShooter.Utils;

namespace SpaceShooter.Editor
{
    public class GameSetupEditor : EditorWindow
    {
        [MenuItem("Space Shooter/Setup Game")]
        public static void ShowWindow()
        {
            GetWindow<GameSetupEditor>("Space Shooter Setup");
        }

        [MenuItem("Space Shooter/Create Sprites")]
        public static void CreateSprites()
        {
            CreateSpriteAssets();
        }

        [MenuItem("Space Shooter/Create Prefabs")]
        public static void CreatePrefabs()
        {
            CreateAllPrefabs();
        }

        [MenuItem("Space Shooter/Setup Scenes")]
        public static void SetupScenes()
        {
            CreateScenes();
        }

        private void OnGUI()
        {
            GUILayout.Label("Space Shooter Game Setup", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            if (GUILayout.Button("1. Create Sprite Assets", GUILayout.Height(30)))
            {
                CreateSpriteAssets();
            }

            EditorGUILayout.Space();

            if (GUILayout.Button("2. Create All Prefabs", GUILayout.Height(30)))
            {
                CreateAllPrefabs();
            }

            EditorGUILayout.Space();

            if (GUILayout.Button("3. Setup Scenes", GUILayout.Height(30)))
            {
                CreateScenes();
            }

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("Click buttons in order to set up the game.\n\n1. Create sprites first\n2. Then create prefabs\n3. Finally setup scenes\n\nAfter setup, open MainMenu scene to play!", MessageType.Info);
        }

        private static void CreateSpriteAssets()
        {
            string spritePath = "Assets/Sprites";
            
            if (!AssetDatabase.IsValidFolder(spritePath))
            {
                AssetDatabase.CreateFolder("Assets", "Sprites");
            }

            // Create player sprite
            Sprite playerSprite = SpriteGenerator.CreatePlayerShipSprite();
            SaveSpriteAsset(playerSprite, $"{spritePath}/PlayerShip.png");

            // Create enemy sprites
            Sprite basicEnemy = SpriteGenerator.CreateEnemyShipSprite(new Color(1f, 0.3f, 0.3f));
            SaveSpriteAsset(basicEnemy, $"{spritePath}/EnemyBasic.png");

            Sprite fastEnemy = SpriteGenerator.CreateEnemyShipSprite(new Color(0.3f, 1f, 0.3f));
            SaveSpriteAsset(fastEnemy, $"{spritePath}/EnemyFast.png");

            Sprite tankEnemy = SpriteGenerator.CreateEnemyShipSprite(new Color(0.6f, 0.3f, 0.8f));
            SaveSpriteAsset(tankEnemy, $"{spritePath}/EnemyTank.png");

            Sprite shooterEnemy = SpriteGenerator.CreateEnemyShipSprite(new Color(1f, 0.6f, 0.2f));
            SaveSpriteAsset(shooterEnemy, $"{spritePath}/EnemyShooter.png");

            // Create bullet sprites
            Sprite playerBullet = SpriteGenerator.CreateBulletSprite(new Color(0.2f, 0.8f, 1f));
            SaveSpriteAsset(playerBullet, $"{spritePath}/PlayerBullet.png");

            Sprite enemyBullet = SpriteGenerator.CreateBulletSprite(new Color(1f, 0.3f, 0.3f));
            SaveSpriteAsset(enemyBullet, $"{spritePath}/EnemyBullet.png");

            // Create power-up sprites
            Sprite weaponPowerUp = SpriteGenerator.CreatePowerUpSprite(new Color(1f, 0.8f, 0.2f));
            SaveSpriteAsset(weaponPowerUp, $"{spritePath}/PowerUpWeapon.png");

            Sprite healthPowerUp = SpriteGenerator.CreatePowerUpSprite(new Color(0.2f, 1f, 0.3f));
            SaveSpriteAsset(healthPowerUp, $"{spritePath}/PowerUpHealth.png");

            Sprite shieldPowerUp = SpriteGenerator.CreatePowerUpSprite(new Color(0.3f, 0.6f, 1f));
            SaveSpriteAsset(shieldPowerUp, $"{spritePath}/PowerUpShield.png");

            Sprite speedPowerUp = SpriteGenerator.CreatePowerUpSprite(new Color(1f, 0.5f, 1f));
            SaveSpriteAsset(speedPowerUp, $"{spritePath}/PowerUpSpeed.png");

            // Create explosion sprite
            Sprite explosion = SpriteGenerator.CreateCircleSprite(64, new Color(1f, 0.6f, 0.2f));
            SaveSpriteAsset(explosion, $"{spritePath}/Explosion.png");

            // Create star sprite for background
            Sprite star = SpriteGenerator.CreateCircleSprite(8, Color.white);
            SaveSpriteAsset(star, $"{spritePath}/Star.png");

            AssetDatabase.Refresh();
            Debug.Log("Sprite assets created successfully!");
        }

        private static void SaveSpriteAsset(Sprite sprite, string path)
        {
            if (sprite == null || sprite.texture == null) return;

            byte[] pngData = sprite.texture.EncodeToPNG();
            System.IO.File.WriteAllBytes(path, pngData);
        }

        private static void CreateAllPrefabs()
        {
            string prefabPath = "Assets/Prefabs";
            
            if (!AssetDatabase.IsValidFolder(prefabPath))
            {
                AssetDatabase.CreateFolder("Assets", "Prefabs");
            }

            CreatePlayerPrefab(prefabPath);
            CreateBulletPrefabs(prefabPath);
            CreateEnemyPrefabs(prefabPath);
            CreatePowerUpPrefabs(prefabPath);
            CreateEffectPrefabs(prefabPath);

            AssetDatabase.Refresh();
            Debug.Log("All prefabs created successfully!");
        }

        private static void CreatePlayerPrefab(string prefabPath)
        {
            GameObject player = new GameObject("Player");
            player.tag = "Player";
            player.layer = LayerMask.NameToLayer("Player");

            SpriteRenderer sr = player.AddComponent<SpriteRenderer>();
            sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/PlayerShip.png");
            sr.sortingLayerName = "Default";
            sr.sortingOrder = 10;

            BoxCollider2D col = player.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(0.8f, 1f);

            Rigidbody2D rb = player.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;

            player.AddComponent<Player.PlayerController>();

            // Create fire point
            GameObject firePoint = new GameObject("FirePoint");
            firePoint.transform.SetParent(player.transform);
            firePoint.transform.localPosition = new Vector3(0, 0.6f, 0);

            // Create shield visual
            GameObject shield = new GameObject("ShieldVisual");
            shield.transform.SetParent(player.transform);
            shield.transform.localPosition = Vector3.zero;
            SpriteRenderer shieldSr = shield.AddComponent<SpriteRenderer>();
            shieldSr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/PowerUpShield.png");
            shieldSr.color = new Color(0.3f, 0.6f, 1f, 0.5f);
            shield.transform.localScale = Vector3.one * 2f;
            shield.SetActive(false);

            PrefabUtility.SaveAsPrefabAsset(player, $"{prefabPath}/Player.prefab");
            DestroyImmediate(player);
        }

        private static void CreateBulletPrefabs(string prefabPath)
        {
            // Player bullet
            CreateBulletPrefab("PlayerBullet", "Assets/Sprites/PlayerBullet.png", "PlayerBullet", prefabPath, true);
            
            // Enemy bullet
            CreateBulletPrefab("EnemyBullet", "Assets/Sprites/EnemyBullet.png", "EnemyBullet", prefabPath, false);
        }

        private static void CreateBulletPrefab(string name, string spritePath, string tag, string prefabPath, bool isPlayer)
        {
            GameObject bullet = new GameObject(name);
            bullet.tag = tag;
            bullet.layer = LayerMask.NameToLayer(tag);

            SpriteRenderer sr = bullet.AddComponent<SpriteRenderer>();
            sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
            sr.sortingLayerName = "Default";
            sr.sortingOrder = 5;

            BoxCollider2D col = bullet.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(0.2f, 0.4f);

            Rigidbody2D rb = bullet.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0;

            Combat.Bullet bulletScript = bullet.AddComponent<Combat.Bullet>();

            PrefabUtility.SaveAsPrefabAsset(bullet, $"{prefabPath}/{name}.prefab");
            DestroyImmediate(bullet);
        }

        private static void CreateEnemyPrefabs(string prefabPath)
        {
            CreateEnemyPrefab("EnemyBasic", "Assets/Sprites/EnemyBasic.png", typeof(Enemy.EnemyBase), prefabPath);
            CreateEnemyPrefab("EnemyFast", "Assets/Sprites/EnemyFast.png", typeof(Enemy.EnemyFast), prefabPath);
            CreateEnemyPrefab("EnemyTank", "Assets/Sprites/EnemyTank.png", typeof(Enemy.EnemyTank), prefabPath);
            CreateEnemyPrefab("EnemyShooter", "Assets/Sprites/EnemyShooter.png", typeof(Enemy.EnemyShooter), prefabPath);
            CreateBossPrefab(prefabPath);
        }

        private static void CreateEnemyPrefab(string name, string spritePath, System.Type enemyType, string prefabPath)
        {
            GameObject enemy = new GameObject(name);
            enemy.tag = "Enemy";
            enemy.layer = LayerMask.NameToLayer("Enemy");

            SpriteRenderer sr = enemy.AddComponent<SpriteRenderer>();
            sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
            sr.sortingLayerName = "Default";
            sr.sortingOrder = 8;
            sr.flipY = true;

            BoxCollider2D col = enemy.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(0.8f, 0.8f);

            Rigidbody2D rb = enemy.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0;

            enemy.AddComponent(enemyType);

            // Create fire point
            GameObject firePoint = new GameObject("FirePoint");
            firePoint.transform.SetParent(enemy.transform);
            firePoint.transform.localPosition = new Vector3(0, -0.5f, 0);

            PrefabUtility.SaveAsPrefabAsset(enemy, $"{prefabPath}/{name}.prefab");
            DestroyImmediate(enemy);
        }

        private static void CreateBossPrefab(string prefabPath)
        {
            GameObject boss = new GameObject("Boss");
            boss.tag = "Enemy";
            boss.layer = LayerMask.NameToLayer("Enemy");

            SpriteRenderer sr = boss.AddComponent<SpriteRenderer>();
            sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/EnemyTank.png");
            sr.sortingLayerName = "Default";
            sr.sortingOrder = 9;
            sr.flipY = true;
            boss.transform.localScale = Vector3.one * 3f;

            BoxCollider2D col = boss.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(1.5f, 1.5f);

            Rigidbody2D rb = boss.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0;

            boss.AddComponent<Enemy.BossEnemy>();

            PrefabUtility.SaveAsPrefabAsset(boss, $"{prefabPath}/Boss.prefab");
            DestroyImmediate(boss);
        }

        private static void CreatePowerUpPrefabs(string prefabPath)
        {
            CreatePowerUpPrefab("PowerUpWeapon", "Assets/Sprites/PowerUpWeapon.png", typeof(PowerUps.WeaponUpgrade), prefabPath);
            CreatePowerUpPrefab("PowerUpHealth", "Assets/Sprites/PowerUpHealth.png", typeof(PowerUps.HealthPack), prefabPath);
            CreatePowerUpPrefab("PowerUpShield", "Assets/Sprites/PowerUpShield.png", typeof(PowerUps.ShieldPowerUp), prefabPath);
            CreatePowerUpPrefab("PowerUpSpeed", "Assets/Sprites/PowerUpSpeed.png", typeof(PowerUps.SpeedBoost), prefabPath);
        }

        private static void CreatePowerUpPrefab(string name, string spritePath, System.Type powerUpType, string prefabPath)
        {
            GameObject powerUp = new GameObject(name);
            powerUp.tag = "PowerUp";
            powerUp.layer = LayerMask.NameToLayer("PowerUp");

            SpriteRenderer sr = powerUp.AddComponent<SpriteRenderer>();
            sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
            sr.sortingLayerName = "Default";
            sr.sortingOrder = 6;

            CircleCollider2D col = powerUp.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.4f;

            powerUp.AddComponent(powerUpType);

            PrefabUtility.SaveAsPrefabAsset(powerUp, $"{prefabPath}/{name}.prefab");
            DestroyImmediate(powerUp);
        }

        private static void CreateEffectPrefabs(string prefabPath)
        {
            // Explosion prefab
            GameObject explosion = new GameObject("Explosion");
            
            SpriteRenderer sr = explosion.AddComponent<SpriteRenderer>();
            sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Explosion.png");
            sr.sortingLayerName = "Foreground";
            sr.sortingOrder = 100;

            explosion.AddComponent<Effects.Explosion>();

            PrefabUtility.SaveAsPrefabAsset(explosion, $"{prefabPath}/Explosion.prefab");
            DestroyImmediate(explosion);
        }

        private static void CreateScenes()
        {
            string scenePath = "Assets/Scenes";
            
            if (!AssetDatabase.IsValidFolder(scenePath))
            {
                AssetDatabase.CreateFolder("Assets", "Scenes");
            }

            CreateMainMenuScene(scenePath);
            CreateGameScene(scenePath);

            // Add scenes to build settings
            EditorBuildSettingsScene[] scenes = new EditorBuildSettingsScene[]
            {
                new EditorBuildSettingsScene($"{scenePath}/MainMenu.unity", true),
                new EditorBuildSettingsScene($"{scenePath}/GameScene.unity", true)
            };
            EditorBuildSettings.scenes = scenes;

            AssetDatabase.Refresh();
            Debug.Log("Scenes created and added to build settings!");
        }

        private static void CreateMainMenuScene(string scenePath)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            
            // Setup camera
            Camera.main.orthographic = true;
            Camera.main.orthographicSize = 5f;
            Camera.main.backgroundColor = new Color(0.05f, 0.05f, 0.15f);

            // Create GameManager
            GameObject gameManager = new GameObject("GameManager");
            gameManager.AddComponent<Managers.GameManager>();
            gameManager.AddComponent<Managers.AudioManager>();

            // Create Canvas
            GameObject canvas = new GameObject("Canvas");
            Canvas canvasComponent = canvas.AddComponent<Canvas>();
            canvasComponent.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvas.AddComponent<UnityEngine.UI.GraphicRaycaster>();

            // Create EventSystem
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

            // Create Main Menu UI
            GameObject mainMenuUI = new GameObject("MainMenuUI");
            mainMenuUI.transform.SetParent(canvas.transform);
            mainMenuUI.AddComponent<UI.MainMenuUI>();

            // Create star field
            GameObject starField = new GameObject("StarField");
            starField.AddComponent<Effects.StarField>();

            EditorSceneManager.SaveScene(scene, $"{scenePath}/MainMenu.unity");
        }

        private static void CreateGameScene(string scenePath)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            
            // Setup camera
            Camera.main.orthographic = true;
            Camera.main.orthographicSize = 5f;
            Camera.main.backgroundColor = new Color(0.02f, 0.02f, 0.08f);

            // Create Managers
            GameObject managers = new GameObject("--- MANAGERS ---");

            GameObject gameManager = new GameObject("GameManager");
            gameManager.AddComponent<Managers.GameManager>();

            GameObject audioManager = new GameObject("AudioManager");
            audioManager.AddComponent<Managers.AudioManager>();

            GameObject waveManager = new GameObject("WaveManager");
            waveManager.AddComponent<Managers.WaveManager>();

            GameObject effectsManager = new GameObject("EffectsManager");
            effectsManager.AddComponent<Managers.EffectsManager>();

            GameObject gameInitializer = new GameObject("GameInitializer");
            gameInitializer.AddComponent<Managers.GameInitializer>();

            GameObject screenBounds = new GameObject("ScreenBounds");
            screenBounds.AddComponent<Utils.ScreenBounds>();

            GameObject collisionHandler = new GameObject("CollisionHandler");
            collisionHandler.AddComponent<Combat.CollisionHandler>();

            // Create spawn point
            GameObject spawnPoint = new GameObject("PlayerSpawnPoint");
            spawnPoint.transform.position = new Vector3(0, -3.5f, 0);

            // Create Canvas
            GameObject canvas = new GameObject("Canvas");
            Canvas canvasComponent = canvas.AddComponent<Canvas>();
            canvasComponent.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvas.AddComponent<UnityEngine.UI.GraphicRaycaster>();

            // Create EventSystem
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

            // Create UI elements
            GameObject gameHUD = new GameObject("GameHUD");
            gameHUD.transform.SetParent(canvas.transform);
            gameHUD.AddComponent<UI.GameHUD>();

            GameObject pauseMenu = new GameObject("PauseMenu");
            pauseMenu.transform.SetParent(canvas.transform);
            pauseMenu.AddComponent<UI.PauseMenuUI>();

            GameObject gameOverUI = new GameObject("GameOverUI");
            gameOverUI.transform.SetParent(canvas.transform);
            gameOverUI.AddComponent<UI.GameOverUI>();

            // Create star field
            GameObject starField = new GameObject("StarField");
            starField.AddComponent<Effects.StarField>();

            EditorSceneManager.SaveScene(scene, $"{scenePath}/GameScene.unity");
        }
    }
}
#endif

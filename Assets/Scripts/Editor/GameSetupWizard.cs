// =============================================================================
// GameSetupWizard.cs — One-click editor tool to set up all scenes and prefabs
// =============================================================================
// Access via Unity menu: Tools > Space Shooter > Full Game Setup
// This creates all prefabs, configures scenes, and sets up the entire game.
// =============================================================================
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;

namespace SpaceShooter.Editor
{
    public class GameSetupWizard : EditorWindow
    {
        [MenuItem("Tools/Space Shooter/Full Game Setup")]
        public static void RunFullSetup()
        {
            if (!EditorUtility.DisplayDialog("Space Shooter Setup",
                "This will generate sprites, create prefabs, and set up all scenes.\n\n" +
                "Make sure you have imported all scripts first.\n\nContinue?",
                "Yes, Set Up Everything", "Cancel"))
                return;

            // Step 1: Generate sprites
            ProceduralSpriteGenerator.GenerateAllSprites();
            AssetDatabase.Refresh();

            // Step 2: Create prefabs
            CreateAllPrefabs();
            AssetDatabase.Refresh();

            // Step 3: Create scenes
            CreateMainMenuScene();
            CreateGamePlayScene();
            CreateGameOverScene();

            // Step 4: Configure build settings
            ConfigureBuildSettings();

            // Step 5: Configure tags and layers
            ConfigureTagsAndLayers();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Setup Complete!",
                "Space Shooter game has been set up!\n\n" +
                "1. Open MainMenu scene to start\n" +
                "2. Press Play to test\n" +
                "3. Use File > Build Settings to build",
                "OK");
        }

        // =====================================================================
        // PREFAB CREATION
        // =====================================================================
        private static void CreateAllPrefabs()
        {
            CreatePlayerPrefab();
            CreateBulletPrefabs();
            CreateEnemyPrefabs();
            CreatePowerUpPrefabs();
            CreateExplosionPrefab();
            CreateShieldPrefab();
            Debug.Log("[GameSetup] All prefabs created.");
        }

        private static void CreatePlayerPrefab()
        {
            GameObject player = new GameObject("Player");
            player.tag = "Player";
            player.layer = LayerMask.NameToLayer("Default");

            // Sprite
            SpriteRenderer sr = player.AddComponent<SpriteRenderer>();
            sr.sprite = LoadSprite("Assets/Sprites/Player/player_ship.png");
            sr.sortingOrder = 10;

            // Collider
            BoxCollider2D col = player.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(0.5f, 0.6f);

            // Rigidbody
            Rigidbody2D rb = player.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;

            // Scripts
            player.AddComponent<Player.HealthSystem>();
            Player.PlayerController pc = player.AddComponent<Player.PlayerController>();

            // Fire point
            GameObject firePoint = new GameObject("FirePoint");
            firePoint.transform.SetParent(player.transform);
            firePoint.transform.localPosition = new Vector3(0f, 0.5f, 0f);

            SavePrefab(player, "Assets/Prefabs/Player/Player.prefab");
            Object.DestroyImmediate(player);
        }

        private static void CreateBulletPrefabs()
        {
            // Player bullet
            CreateBulletPrefab("PlayerBullet", "PlayerBullet",
                "Assets/Sprites/Bullets/player_bullet.png",
                "Assets/Prefabs/Bullets/PlayerBullet.prefab",
                new Vector2(0.1f, 0.3f), 12f, 1, true);

            // Enemy bullet
            CreateBulletPrefab("EnemyBullet", "EnemyBullet",
                "Assets/Sprites/Bullets/enemy_bullet.png",
                "Assets/Prefabs/Bullets/EnemyBullet.prefab",
                new Vector2(0.2f, 0.2f), 6f, 1, false);

            // Boss bullet
            CreateBulletPrefab("BossBullet", "EnemyBullet",
                "Assets/Sprites/Bullets/boss_bullet.png",
                "Assets/Prefabs/Bullets/BossBullet.prefab",
                new Vector2(0.3f, 0.3f), 5f, 2, false);
        }

        private static void CreateBulletPrefab(string name, string tag, string spritePath,
            string prefabPath, Vector2 colliderSize, float speed, int damage, bool isPlayer)
        {
            GameObject bullet = new GameObject(name);
            bullet.tag = tag;

            SpriteRenderer sr = bullet.AddComponent<SpriteRenderer>();
            sr.sprite = LoadSprite(spritePath);
            sr.sortingOrder = 5;

            BoxCollider2D col = bullet.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = colliderSize;

            Rigidbody2D rb = bullet.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;

            Weapons.Bullet b = bullet.AddComponent<Weapons.Bullet>();

            SavePrefab(bullet, prefabPath);
            Object.DestroyImmediate(bullet);
        }

        private static void CreateEnemyPrefabs()
        {
            // Basic Enemy
            CreateEnemyPrefab<Enemy.BasicEnemy>("BasicEnemy",
                "Assets/Sprites/Enemies/basic_enemy.png",
                "Assets/Prefabs/Enemies/BasicEnemy.prefab",
                new Vector2(0.5f, 0.5f), 3, 100, 3f, 1.5f);

            // Fast Enemy
            CreateEnemyPrefab<Enemy.FastEnemy>("FastEnemy",
                "Assets/Sprites/Enemies/fast_enemy.png",
                "Assets/Prefabs/Enemies/FastEnemy.prefab",
                new Vector2(0.4f, 0.4f), 2, 150, 5f, 2f);

            // Tank Enemy
            CreateEnemyPrefab<Enemy.TankEnemy>("TankEnemy",
                "Assets/Sprites/Enemies/tank_enemy.png",
                "Assets/Prefabs/Enemies/TankEnemy.prefab",
                new Vector2(0.8f, 0.8f), 8, 300, 1.5f, 1f);

            // Boss Enemy
            CreateBossPrefab();
        }

        private static void CreateEnemyPrefab<T>(string name, string spritePath, string prefabPath,
            Vector2 colliderSize, int health, int score, float speed, float fireRate) where T : Enemy.EnemyBase
        {
            GameObject enemy = new GameObject(name);
            enemy.tag = "Enemy";

            SpriteRenderer sr = enemy.AddComponent<SpriteRenderer>();
            sr.sprite = LoadSprite(spritePath);
            sr.sortingOrder = 8;

            BoxCollider2D col = enemy.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = colliderSize;

            Rigidbody2D rb = enemy.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;

            enemy.AddComponent<T>();

            // Add bullet pattern
            Weapons.BulletPattern bp = enemy.AddComponent<Weapons.BulletPattern>();

            SavePrefab(enemy, prefabPath);
            Object.DestroyImmediate(enemy);
        }

        private static void CreateBossPrefab()
        {
            GameObject boss = new GameObject("BossEnemy");
            boss.tag = "Enemy";

            SpriteRenderer sr = boss.AddComponent<SpriteRenderer>();
            sr.sprite = LoadSprite("Assets/Sprites/Enemies/boss_enemy.png");
            sr.sortingOrder = 9;

            BoxCollider2D col = boss.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(1.5f, 1f);

            Rigidbody2D rb = boss.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;

            boss.AddComponent<Enemy.BossEnemy>();

            // Main pattern
            Weapons.BulletPattern bp1 = boss.AddComponent<Weapons.BulletPattern>();

            // Add child objects for additional patterns
            GameObject phase2Obj = new GameObject("Phase2Pattern");
            phase2Obj.transform.SetParent(boss.transform);
            Weapons.BulletPattern bp2 = phase2Obj.AddComponent<Weapons.BulletPattern>();

            GameObject phase3Obj = new GameObject("Phase3Pattern");
            phase3Obj.transform.SetParent(boss.transform);
            Weapons.BulletPattern bp3 = phase3Obj.AddComponent<Weapons.BulletPattern>();

            SavePrefab(boss, "Assets/Prefabs/Enemies/BossEnemy.prefab");
            Object.DestroyImmediate(boss);
        }

        private static void CreatePowerUpPrefabs()
        {
            CreatePowerUpPrefab("HealthPowerUp", PowerUps.PowerUpType.Health,
                "Assets/Sprites/PowerUps/powerup_health.png", "Assets/Prefabs/PowerUps/HealthPowerUp.prefab");
            CreatePowerUpPrefab("ShieldPowerUp", PowerUps.PowerUpType.Shield,
                "Assets/Sprites/PowerUps/powerup_shield.png", "Assets/Prefabs/PowerUps/ShieldPowerUp.prefab");
            CreatePowerUpPrefab("RapidFirePowerUp", PowerUps.PowerUpType.RapidFire,
                "Assets/Sprites/PowerUps/powerup_rapid.png", "Assets/Prefabs/PowerUps/RapidFirePowerUp.prefab");
            CreatePowerUpPrefab("SpreadShotPowerUp", PowerUps.PowerUpType.SpreadShot,
                "Assets/Sprites/PowerUps/powerup_spread.png", "Assets/Prefabs/PowerUps/SpreadShotPowerUp.prefab");
            CreatePowerUpPrefab("ExtraLifePowerUp", PowerUps.PowerUpType.ExtraLife,
                "Assets/Sprites/PowerUps/powerup_life.png", "Assets/Prefabs/PowerUps/ExtraLifePowerUp.prefab");
            CreatePowerUpPrefab("ScoreBonusPowerUp", PowerUps.PowerUpType.ScoreBonus,
                "Assets/Sprites/PowerUps/powerup_score.png", "Assets/Prefabs/PowerUps/ScoreBonusPowerUp.prefab");
        }

        private static void CreatePowerUpPrefab(string name, PowerUps.PowerUpType type,
            string spritePath, string prefabPath)
        {
            GameObject pu = new GameObject(name);
            pu.tag = "PowerUp";

            SpriteRenderer sr = pu.AddComponent<SpriteRenderer>();
            sr.sprite = LoadSprite(spritePath);
            sr.sortingOrder = 6;

            CircleCollider2D col = pu.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.3f;

            Rigidbody2D rb = pu.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;

            pu.AddComponent<PowerUps.PowerUp>();

            SavePrefab(pu, prefabPath);
            Object.DestroyImmediate(pu);
        }

        private static void CreateExplosionPrefab()
        {
            GameObject explosion = new GameObject("Explosion");

            SpriteRenderer sr = explosion.AddComponent<SpriteRenderer>();
            sr.sprite = LoadSprite("Assets/Sprites/Effects/explosion.png");
            sr.sortingOrder = 15;

            explosion.AddComponent<Effects.ExplosionEffect>();
            explosion.AddComponent<Utils.AutoDestroy>();

            SavePrefab(explosion, "Assets/Prefabs/Effects/Explosion.prefab");
            Object.DestroyImmediate(explosion);
        }

        private static void CreateShieldPrefab()
        {
            GameObject shield = new GameObject("ShieldVisual");

            SpriteRenderer sr = shield.AddComponent<SpriteRenderer>();
            sr.sprite = LoadSprite("Assets/Sprites/Effects/shield.png");
            sr.sortingOrder = 11;

            SavePrefab(shield, "Assets/Prefabs/Effects/ShieldVisual.prefab");
            Object.DestroyImmediate(shield);
        }

        // =====================================================================
        // SCENE CREATION
        // =====================================================================
        private static void CreateMainMenuScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            // Camera setup
            Camera cam = Camera.main;
            cam.backgroundColor = new Color(0.02f, 0.02f, 0.08f);
            cam.orthographic = true;
            cam.orthographicSize = 5f;

            // Starfield background
            GameObject starfield = new GameObject("Starfield");
            starfield.AddComponent<Effects.StarfieldGenerator>();

            // Canvas
            GameObject canvas = CreateUICanvas();

            // Title
            GameObject titleObj = CreateUIText(canvas.transform, "TitleText",
                "SPACE SHOOTER", 48, TextAnchor.MiddleCenter,
                new Vector2(0, 120), new Vector2(600, 80));
            Text titleText = titleObj.GetComponent<Text>();
            titleText.color = new Color(0.3f, 0.8f, 1f);

            // High Score
            CreateUIText(canvas.transform, "HighScoreText",
                "HIGH SCORE: 0", 20, TextAnchor.MiddleCenter,
                new Vector2(0, 50), new Vector2(400, 40));

            // Start Button
            GameObject startBtn = CreateUIButton(canvas.transform, "StartButton",
                "START GAME", new Vector2(0, -30), new Vector2(250, 50));

            // Quit Button
            GameObject quitBtn = CreateUIButton(canvas.transform, "QuitButton",
                "QUIT", new Vector2(0, -100), new Vector2(250, 50));

            // Volume Sliders
            CreateUIText(canvas.transform, "MusicLabel",
                "Music", 16, TextAnchor.MiddleRight,
                new Vector2(-80, -170), new Vector2(100, 30));
            GameObject musicSlider = CreateUISlider(canvas.transform, "MusicSlider",
                new Vector2(60, -170), new Vector2(200, 20));

            CreateUIText(canvas.transform, "SFXLabel",
                "SFX", 16, TextAnchor.MiddleRight,
                new Vector2(-80, -210), new Vector2(100, 30));
            GameObject sfxSlider = CreateUISlider(canvas.transform, "SFXSlider",
                new Vector2(60, -210), new Vector2(200, 20));

            // MainMenu Controller
            GameObject menuController = new GameObject("MainMenuController");
            UI.MainMenuController mmc = menuController.AddComponent<UI.MainMenuController>();

            // GameManager (if not already in scene)
            CreateManagerObjects();

            EditorSceneManager.SaveScene(scene, "Assets/Scenes/MainMenu.unity");
            Debug.Log("[GameSetup] MainMenu scene created.");
        }

        private static void CreateGamePlayScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            // Camera
            Camera cam = Camera.main;
            cam.backgroundColor = new Color(0.01f, 0.01f, 0.05f);
            cam.orthographic = true;
            cam.orthographicSize = 5f;

            // Background layers
            CreateBackgroundLayer("BG_Far", "Assets/Sprites/Backgrounds/bg_far.png", 0.3f, -5f);
            CreateBackgroundLayer("BG_Mid", "Assets/Sprites/Backgrounds/bg_mid.png", 0.7f, -3f);
            CreateBackgroundLayer("BG_Near", "Assets/Sprites/Backgrounds/bg_near.png", 1.5f, -1f);

            // Starfield
            GameObject starfield = new GameObject("Starfield");
            starfield.AddComponent<Effects.StarfieldGenerator>();

            // Player
            GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player/Player.prefab");
            if (playerPrefab != null)
            {
                GameObject player = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab);
                player.transform.position = new Vector3(0f, -3.5f, 0f);
            }

            // Enemy Spawner
            GameObject spawnerObj = new GameObject("EnemySpawner");
            spawnerObj.AddComponent<Enemy.EnemySpawner>();

            // HUD Canvas
            GameObject canvas = CreateUICanvas();

            // Score
            CreateUIText(canvas.transform, "ScoreText",
                "SCORE: 0", 24, TextAnchor.UpperLeft,
                new Vector2(-300, 220), new Vector2(300, 40));

            // High Score
            CreateUIText(canvas.transform, "HighScoreText",
                "HI: 0", 18, TextAnchor.UpperRight,
                new Vector2(280, 220), new Vector2(200, 30));

            // Lives
            CreateUIText(canvas.transform, "LivesText",
                "x3", 20, TextAnchor.UpperLeft,
                new Vector2(-300, 185), new Vector2(100, 30));

            // Health Bar
            GameObject healthSlider = CreateUISlider(canvas.transform, "HealthBar",
                new Vector2(-200, 185), new Vector2(150, 15));

            // Wave Announcement (centered)
            GameObject waveGroup = new GameObject("WaveGroup");
            waveGroup.transform.SetParent(canvas.transform, false);
            CanvasGroup cg = waveGroup.AddComponent<CanvasGroup>();
            cg.alpha = 0f;
            RectTransform wgrt = waveGroup.AddComponent<RectTransform>();
            wgrt.anchoredPosition = Vector2.zero;
            wgrt.sizeDelta = new Vector2(400, 60);

            CreateUIText(waveGroup.transform, "WaveText",
                "WAVE 1", 36, TextAnchor.MiddleCenter,
                Vector2.zero, new Vector2(400, 60));

            // Pause Panel
            GameObject pausePanel = new GameObject("PausePanel");
            pausePanel.transform.SetParent(canvas.transform, false);
            RectTransform pprt = pausePanel.AddComponent<RectTransform>();
            pprt.anchoredPosition = Vector2.zero;
            pprt.sizeDelta = new Vector2(400, 400);
            Image ppbg = pausePanel.AddComponent<Image>();
            ppbg.color = new Color(0, 0, 0, 0.7f);

            CreateUIText(pausePanel.transform, "PauseTitle",
                "PAUSED", 36, TextAnchor.MiddleCenter,
                new Vector2(0, 100), new Vector2(300, 50));

            CreateUIButton(pausePanel.transform, "ResumeButton",
                "RESUME", new Vector2(0, 20), new Vector2(200, 45));
            CreateUIButton(pausePanel.transform, "MainMenuButton",
                "MAIN MENU", new Vector2(0, -40), new Vector2(200, 45));
            CreateUIButton(pausePanel.transform, "QuitButton",
                "QUIT", new Vector2(0, -100), new Vector2(200, 45));

            pausePanel.SetActive(false);

            // HUD Controller
            GameObject hudObj = new GameObject("HUDController");
            hudObj.AddComponent<UI.HUDController>();

            EditorSceneManager.SaveScene(scene, "Assets/Scenes/GamePlay.unity");
            Debug.Log("[GameSetup] GamePlay scene created.");
        }

        private static void CreateGameOverScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            Camera cam = Camera.main;
            cam.backgroundColor = new Color(0.05f, 0f, 0f);
            cam.orthographic = true;
            cam.orthographicSize = 5f;

            // Starfield
            GameObject starfield = new GameObject("Starfield");
            starfield.AddComponent<Effects.StarfieldGenerator>();

            // Canvas
            GameObject canvas = CreateUICanvas();

            // Game Over Text
            GameObject goText = CreateUIText(canvas.transform, "GameOverText",
                "GAME OVER", 48, TextAnchor.MiddleCenter,
                new Vector2(0, 140), new Vector2(500, 70));
            goText.GetComponent<Text>().color = Color.red;

            // Final Score
            CreateUIText(canvas.transform, "FinalScoreText",
                "FINAL SCORE: 0", 28, TextAnchor.MiddleCenter,
                new Vector2(0, 70), new Vector2(400, 40));

            // High Score
            CreateUIText(canvas.transform, "HighScoreText",
                "HIGH SCORE: 0", 22, TextAnchor.MiddleCenter,
                new Vector2(0, 30), new Vector2(400, 35));

            // New High Score
            GameObject newHS = CreateUIText(canvas.transform, "NewHighScoreText",
                "NEW HIGH SCORE!", 20, TextAnchor.MiddleCenter,
                new Vector2(0, -10), new Vector2(300, 30));
            newHS.GetComponent<Text>().color = Color.yellow;
            newHS.SetActive(false);

            // Buttons
            CreateUIButton(canvas.transform, "RestartButton",
                "PLAY AGAIN", new Vector2(0, -70), new Vector2(250, 50));
            CreateUIButton(canvas.transform, "MainMenuButton",
                "MAIN MENU", new Vector2(0, -135), new Vector2(250, 50));
            CreateUIButton(canvas.transform, "QuitButton",
                "QUIT", new Vector2(0, -200), new Vector2(250, 50));

            // Controller
            GameObject controller = new GameObject("GameOverController");
            controller.AddComponent<UI.GameOverController>();

            EditorSceneManager.SaveScene(scene, "Assets/Scenes/GameOver.unity");
            Debug.Log("[GameSetup] GameOver scene created.");
        }

        // =====================================================================
        // HELPER METHODS
        // =====================================================================
        private static void CreateManagerObjects()
        {
            // These are created once in MainMenu and persist via DontDestroyOnLoad
            GameObject gmObj = new GameObject("GameManager");
            gmObj.AddComponent<Managers.GameManager>();

            GameObject smObj = new GameObject("SoundManager");
            smObj.AddComponent<Managers.SoundManager>();

            GameObject imObj = new GameObject("InputHandler");
            imObj.AddComponent<Managers.InputHandler>();
        }

        private static void CreateBackgroundLayer(string name, string spritePath, float speed, float z)
        {
            GameObject bg = new GameObject(name);
            SpriteRenderer sr = bg.AddComponent<SpriteRenderer>();
            sr.sprite = LoadSprite(spritePath);
            sr.sortingOrder = -10;
            sr.drawMode = SpriteDrawMode.Tiled;
            bg.transform.position = new Vector3(0, 0, z);

            Effects.ParallaxBackground pb = bg.AddComponent<Effects.ParallaxBackground>();
        }

        private static GameObject CreateUICanvas()
        {
            GameObject canvas = new GameObject("Canvas");
            Canvas c = canvas.AddComponent<Canvas>();
            c.renderMode = RenderMode.ScreenSpaceOverlay;
            c.sortingOrder = 100;

            CanvasScaler scaler = canvas.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(800, 600);

            canvas.AddComponent<GraphicRaycaster>();

            // EventSystem
            if (Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                GameObject es = new GameObject("EventSystem");
                es.AddComponent<UnityEngine.EventSystems.EventSystem>();
                es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }

            return canvas;
        }

        private static GameObject CreateUIText(Transform parent, string name, string text,
            int fontSize, TextAnchor alignment, Vector2 position, Vector2 size)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);

            RectTransform rt = obj.AddComponent<RectTransform>();
            rt.anchoredPosition = position;
            rt.sizeDelta = size;

            Text t = obj.AddComponent<Text>();
            t.text = text;
            t.fontSize = fontSize;
            t.alignment = alignment;
            t.color = Color.white;
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (t.font == null)
                t.font = Resources.GetBuiltinResource<Font>("Arial.ttf");

            // Add outline for readability
            Outline outline = obj.AddComponent<Outline>();
            outline.effectColor = Color.black;
            outline.effectDistance = new Vector2(1, -1);

            return obj;
        }

        private static GameObject CreateUIButton(Transform parent, string name, string label,
            Vector2 position, Vector2 size)
        {
            GameObject btnObj = new GameObject(name);
            btnObj.transform.SetParent(parent, false);

            RectTransform rt = btnObj.AddComponent<RectTransform>();
            rt.anchoredPosition = position;
            rt.sizeDelta = size;

            Image img = btnObj.AddComponent<Image>();
            img.color = new Color(0.2f, 0.3f, 0.5f, 0.8f);

            Button btn = btnObj.AddComponent<Button>();
            ColorBlock colors = btn.colors;
            colors.normalColor = new Color(0.2f, 0.3f, 0.5f, 0.8f);
            colors.highlightedColor = new Color(0.3f, 0.5f, 0.8f, 0.9f);
            colors.pressedColor = new Color(0.15f, 0.2f, 0.4f, 1f);
            btn.colors = colors;

            // Button text
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform, false);

            RectTransform trt = textObj.AddComponent<RectTransform>();
            trt.anchorMin = Vector2.zero;
            trt.anchorMax = Vector2.one;
            trt.sizeDelta = Vector2.zero;

            Text t = textObj.AddComponent<Text>();
            t.text = label;
            t.fontSize = 22;
            t.alignment = TextAnchor.MiddleCenter;
            t.color = Color.white;
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (t.font == null)
                t.font = Resources.GetBuiltinResource<Font>("Arial.ttf");

            return btnObj;
        }

        private static GameObject CreateUISlider(Transform parent, string name,
            Vector2 position, Vector2 size)
        {
            // Create a simplified slider
            GameObject sliderObj = new GameObject(name);
            sliderObj.transform.SetParent(parent, false);

            RectTransform rt = sliderObj.AddComponent<RectTransform>();
            rt.anchoredPosition = position;
            rt.sizeDelta = size;

            Slider slider = sliderObj.AddComponent<Slider>();
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = 0.7f;

            // Background
            GameObject bg = new GameObject("Background");
            bg.transform.SetParent(sliderObj.transform, false);
            RectTransform bgrt = bg.AddComponent<RectTransform>();
            bgrt.anchorMin = Vector2.zero;
            bgrt.anchorMax = Vector2.one;
            bgrt.sizeDelta = Vector2.zero;
            Image bgImg = bg.AddComponent<Image>();
            bgImg.color = new Color(0.15f, 0.15f, 0.15f, 0.8f);

            // Fill area
            GameObject fillArea = new GameObject("Fill Area");
            fillArea.transform.SetParent(sliderObj.transform, false);
            RectTransform fart = fillArea.AddComponent<RectTransform>();
            fart.anchorMin = Vector2.zero;
            fart.anchorMax = Vector2.one;
            fart.sizeDelta = Vector2.zero;

            GameObject fill = new GameObject("Fill");
            fill.transform.SetParent(fillArea.transform, false);
            RectTransform frt = fill.AddComponent<RectTransform>();
            frt.anchorMin = Vector2.zero;
            frt.anchorMax = Vector2.one;
            frt.sizeDelta = Vector2.zero;
            Image fillImg = fill.AddComponent<Image>();
            fillImg.color = new Color(0.3f, 0.7f, 1f, 0.8f);

            slider.fillRect = frt;

            return sliderObj;
        }

        private static void ConfigureBuildSettings()
        {
            EditorBuildSettingsScene[] scenes = new EditorBuildSettingsScene[]
            {
                new EditorBuildSettingsScene("Assets/Scenes/MainMenu.unity", true),
                new EditorBuildSettingsScene("Assets/Scenes/GamePlay.unity", true),
                new EditorBuildSettingsScene("Assets/Scenes/GameOver.unity", true),
            };
            EditorBuildSettings.scenes = scenes;
            Debug.Log("[GameSetup] Build settings configured with 3 scenes.");
        }

        private static void ConfigureTagsAndLayers()
        {
            // Tags are configured through the TagManager asset.
            // For a programmatic approach, we just log what needs manual setup.
            Debug.Log("[GameSetup] Please verify the following tags exist in Project Settings > Tags and Layers:");
            Debug.Log("  - Player, Enemy, PlayerBullet, EnemyBullet, PowerUp");
            Debug.Log("  You can add them via Edit > Project Settings > Tags and Layers");
        }

        private static Sprite LoadSprite(string path)
        {
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite == null)
            {
                // Try to load texture and create sprite
                Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                if (tex != null)
                {
                    // Configure texture import settings
                    TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
                    if (importer != null)
                    {
                        importer.textureType = TextureImporterType.Sprite;
                        importer.spritePixelsPerUnit = 64;
                        importer.filterMode = FilterMode.Point;
                        importer.textureCompression = TextureImporterCompression.Uncompressed;
                        importer.SaveAndReimport();
                    }
                    sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                }
            }
            return sprite;
        }

        private static void SavePrefab(GameObject go, string path)
        {
            string dir = Path.GetDirectoryName(path);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            PrefabUtility.SaveAsPrefabAsset(go, path);
        }
    }
}
#endif

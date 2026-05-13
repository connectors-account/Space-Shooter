// ============================================================================
// SceneSetupWizard.cs — Editor wizard that builds the entire game automatically
// Run from Unity menu: Tools > Space Shooter > Setup Complete Game
// Creates all GameObjects, prefabs, layers, tags, scenes, and wiring
// so the game is playable with a single click.
// ============================================================================
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.IO;
using TMPro;

namespace SpaceShooter.EditorTools
{
    public static class SceneSetupWizard
    {
        private const string PrefabPath = "Assets/Prefabs";
        private const string ScenePath  = "Assets/Scenes";
        private const string SpritePath = "Assets/Sprites/Generated";

        // ====================================================================
        [MenuItem("Tools/Space Shooter/Setup Complete Game")]
        public static void SetupAll()
        {
            // Step 0: Generate sprites first
            SpriteGenerator.GenerateAll();
            AssetDatabase.Refresh();

            // Step 1: Ensure tags and layers
            EnsureTag("Player");
            EnsureTag("Enemy");
            EnsureTag("PlayerBullet");
            EnsureTag("EnemyBullet");
            EnsureTag("PowerUp");

            // Sorting layers
            EnsureSortingLayer("Background");
            EnsureSortingLayer("Gameplay");
            EnsureSortingLayer("Projectiles");
            EnsureSortingLayer("UI");

            // Step 2: Create prefabs
            Directory.CreateDirectory(PrefabPath);
            CreatePlayerBulletPrefab();
            CreateEnemyBulletPrefab();
            CreateEnemyPrefab("EnemyStraight", "EnemyStraight", typeof(Enemies.EnemyStraight), 1, 100, 3f);
            CreateEnemyPrefab("EnemyZigzag", "EnemyZigzag", typeof(Enemies.EnemyZigzag), 2, 150, 2.5f);
            CreateEnemyPrefab("EnemyTracker", "EnemyTracker", typeof(Enemies.EnemyTracker), 2, 200, 2f);
            CreateEnemyPrefab("EnemyTank", "EnemyTank", typeof(Enemies.EnemyTank), 5, 500, 1.5f, canShoot: true);
            CreatePowerUpPrefabs();

            // Step 3: Build scenes
            Directory.CreateDirectory(ScenePath);
            BuildMainMenuScene();
            BuildGameplayScene();

            // Step 4: Configure Build Settings
            var scenes = new EditorBuildSettingsScene[]
            {
                new EditorBuildSettingsScene($"{ScenePath}/MainMenu.unity", true),
                new EditorBuildSettingsScene($"{ScenePath}/Gameplay.unity", true),
            };
            EditorBuildSettings.scenes = scenes;

            // Step 5: Set Player Settings
            PlayerSettings.companyName = "IndieStudio";
            PlayerSettings.productName = "Star Blaster";
            PlayerSettings.defaultScreenWidth = 1920;
            PlayerSettings.defaultScreenHeight = 1080;
            PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
            PlayerSettings.resizableWindow = true;

            // Step 6: Configure Physics2D layers
            // Player (layer 6), Enemy (layer 7), PlayerBullet (layer 8), EnemyBullet (layer 9)
            // We set layer collision ignoring to prevent bullets hitting same-team objects
            SetLayerName(6, "Player");
            SetLayerName(7, "Enemy");
            SetLayerName(8, "PlayerBullet");
            SetLayerName(9, "EnemyBullet");
            SetLayerName(10, "PowerUp");

            // Player bullets should not hit player; enemy bullets should not hit enemies
            Physics2D.IgnoreLayerCollision(6, 8, true);   // Player <-> PlayerBullet
            Physics2D.IgnoreLayerCollision(7, 9, true);   // Enemy  <-> EnemyBullet
            Physics2D.IgnoreLayerCollision(8, 9, true);   // PlayerBullet <-> EnemyBullet
            Physics2D.IgnoreLayerCollision(8, 8, true);   // PlayerBullet <-> PlayerBullet
            Physics2D.IgnoreLayerCollision(9, 9, true);   // EnemyBullet <-> EnemyBullet

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[SceneSetupWizard] ✅ Complete game setup finished! Open MainMenu scene and press Play.");
        }

        // ====================================================================
        // Prefab Builders
        // ====================================================================
        private static void CreatePlayerBulletPrefab()
        {
            var go = new GameObject("PlayerBullet");
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = LoadSprite("PlayerBullet");
            sr.sortingLayerName = "Projectiles";

            var col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(0.25f, 0.75f);

            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.isKinematic = true;

            var bullet = go.AddComponent<Combat.Bullet>();
            // Set via SerializedObject to access private serialized fields
            SetSerializedField(bullet, "speed", 14f);
            SetSerializedField(bullet, "damage", 1);
            SetSerializedField(bullet, "isPlayerBullet", true);
            SetSerializedField(bullet, "poolTag", "PlayerBullet");

            go.layer = 8; // PlayerBullet
            go.tag = "PlayerBullet";

            SavePrefab(go, "PlayerBullet");
            Object.DestroyImmediate(go);
        }

        private static void CreateEnemyBulletPrefab()
        {
            var go = new GameObject("EnemyBullet");
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = LoadSprite("EnemyBullet");
            sr.sortingLayerName = "Projectiles";
            sr.flipY = true;

            var col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(0.25f, 0.75f);

            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.isKinematic = true;

            var bullet = go.AddComponent<Combat.Bullet>();
            SetSerializedField(bullet, "speed", -8f);  // negative = downward
            SetSerializedField(bullet, "damage", 1);
            SetSerializedField(bullet, "isPlayerBullet", false);
            SetSerializedField(bullet, "poolTag", "EnemyBullet");

            go.layer = 9; // EnemyBullet
            go.tag = "EnemyBullet";

            SavePrefab(go, "EnemyBullet");
            Object.DestroyImmediate(go);
        }

        private static void CreateEnemyPrefab(string name, string spriteName,
            System.Type enemyScript, int hp, int score, float speed, bool canShoot = false)
        {
            var go = new GameObject(name);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = LoadSprite(spriteName);
            sr.sortingLayerName = "Gameplay";

            var col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = true;

            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.isKinematic = true;

            var enemy = go.AddComponent(enemyScript);
            SetSerializedField(enemy as Enemies.EnemyBase, "maxHealth", hp);
            SetSerializedField(enemy as Enemies.EnemyBase, "scoreValue", score);
            SetSerializedField(enemy as Enemies.EnemyBase, "moveSpeed", speed);
            SetSerializedField(enemy as Enemies.EnemyBase, "poolTag", name);
            SetSerializedField(enemy as Enemies.EnemyBase, "canShoot", canShoot);
            if (canShoot)
                SetSerializedField(enemy as Enemies.EnemyBase, "bulletPoolTag", "EnemyBullet");

            go.AddComponent<Combat.ContactDamage>();
            go.layer = 7;  // Enemy
            go.tag = "Enemy";

            SavePrefab(go, name);
            Object.DestroyImmediate(go);
        }

        private static void CreatePowerUpPrefabs()
        {
            string[] names = { "PowerUpHealth", "PowerUpShield", "PowerUpRapidFire", "PowerUpSpreadShot" };
            PowerUps.PowerUpType[] types = {
                PowerUps.PowerUpType.Health,
                PowerUps.PowerUpType.Shield,
                PowerUps.PowerUpType.RapidFire,
                PowerUps.PowerUpType.SpreadShot
            };

            for (int i = 0; i < names.Length; i++)
            {
                var go = new GameObject(names[i]);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = LoadSprite(names[i]);
                sr.sortingLayerName = "Gameplay";

                var col = go.AddComponent<CircleCollider2D>();
                col.isTrigger = true;
                col.radius = 0.5f;

                var rb = go.AddComponent<Rigidbody2D>();
                rb.gravityScale = 0f;
                rb.isKinematic = true;

                var pu = go.AddComponent<PowerUps.PowerUp>();
                SetSerializedField(pu, "type", (int)types[i]);

                go.layer = 10; // PowerUp
                go.tag = "PowerUp";

                SavePrefab(go, names[i]);
                Object.DestroyImmediate(go);
            }
        }

        // ====================================================================
        // Scene Builders
        // ====================================================================
        private static void BuildMainMenuScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            // Camera
            var cam = Camera.main;
            cam.orthographic = true;
            cam.orthographicSize = 5.4f;
            cam.backgroundColor = new Color(0.02f, 0.02f, 0.08f);

            // GameManager (persistent)
            var gm = new GameObject("GameManager");
            gm.AddComponent<Core.GameManager>();

            // AudioManager (persistent)
            var am = new GameObject("AudioManager");
            am.AddComponent<Audio.AudioManager>();

            // Star field background
            var stars = new GameObject("StarField");
            stars.AddComponent<ParticleSystem>();
            stars.AddComponent<Background.StarFieldGenerator>();

            // ---- UI Canvas ----
            var canvas = CreateCanvas("MainMenuCanvas");

            // Title
            var title = CreateTMPText(canvas.transform, "TitleText", "STAR BLASTER",
                new Vector2(0, 150), 72, TextAlignmentOptions.Center, Color.cyan);

            // High Score
            var hs = CreateTMPText(canvas.transform, "HighScoreText", "HIGH SCORE: 0",
                new Vector2(0, 60), 28, TextAlignmentOptions.Center, Color.white);

            // Start Button
            var startBtn = CreateButton(canvas.transform, "StartButton", "START GAME",
                new Vector2(0, -40), new Vector2(300, 60));

            // Quit Button
            var quitBtn = CreateButton(canvas.transform, "QuitButton", "QUIT",
                new Vector2(0, -120), new Vector2(300, 60));

            // MainMenuUI script
            var menuUI = canvas.AddComponent<UI.MainMenuUI>();
            SetSerializedField(menuUI, "startButton", startBtn.GetComponent<Button>());
            SetSerializedField(menuUI, "quitButton", quitBtn.GetComponent<Button>());
            SetSerializedField(menuUI, "titleText", title.GetComponent<TextMeshProUGUI>());
            SetSerializedField(menuUI, "highScoreText", hs.GetComponent<TextMeshProUGUI>());

            EditorSceneManager.SaveScene(scene, $"{ScenePath}/MainMenu.unity");
        }

        private static void BuildGameplayScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            // Camera
            var cam = Camera.main;
            cam.orthographic = true;
            cam.orthographicSize = 5.4f;
            cam.backgroundColor = new Color(0.02f, 0.02f, 0.08f);

            // GameBounds
            var bounds = new GameObject("GameBounds");
            bounds.AddComponent<Core.GameBounds>();

            // Star field
            var stars = new GameObject("StarField");
            stars.AddComponent<ParticleSystem>();
            stars.AddComponent<Background.StarFieldGenerator>();

            // Parallax background layers
            var bgParent = new GameObject("ParallaxBG");
            var parallax = bgParent.AddComponent<Background.ParallaxBackground>();
            var bgSprite = LoadSprite("BackgroundTile");

            var layer1 = new GameObject("BG_Layer1");
            layer1.transform.SetParent(bgParent.transform);
            var sr1 = layer1.AddComponent<SpriteRenderer>();
            sr1.sprite = bgSprite;
            sr1.sortingLayerName = "Background";
            sr1.sortingOrder = -10;
            layer1.transform.localScale = new Vector3(2f, 2f, 1f);

            var layer2 = new GameObject("BG_Layer2");
            layer2.transform.SetParent(bgParent.transform);
            var sr2 = layer2.AddComponent<SpriteRenderer>();
            sr2.sprite = bgSprite;
            sr2.sortingLayerName = "Background";
            sr2.sortingOrder = -10;
            layer2.transform.localScale = new Vector3(2f, 2f, 1f);

            SetSerializedField(parallax, "layers", new Transform[] { layer1.transform, layer2.transform });

            // ---- Player ----
            var player = new GameObject("Player");
            player.tag = "Player";
            player.layer = 6;
            var psr = player.AddComponent<SpriteRenderer>();
            psr.sprite = LoadSprite("PlayerShip");
            psr.sortingLayerName = "Gameplay";
            psr.sortingOrder = 5;
            player.transform.position = new Vector3(0, -3.5f, 0);

            var pcol = player.AddComponent<BoxCollider2D>();
            pcol.isTrigger = true;
            pcol.size = new Vector2(1.5f, 1.5f);

            player.AddComponent<Rigidbody2D>();
            player.AddComponent<Player.PlayerController>();
            player.AddComponent<Player.PlayerHealth>();

            // Shield visual (child)
            var shield = new GameObject("ShieldVisual");
            shield.transform.SetParent(player.transform);
            shield.transform.localPosition = Vector3.zero;
            var shieldSR = shield.AddComponent<SpriteRenderer>();
            shieldSR.sprite = LoadSprite("ShieldOverlay");
            shieldSR.sortingLayerName = "Gameplay";
            shieldSR.sortingOrder = 6;
            shield.SetActive(false);

            // Wire shield visual
            var ph = player.GetComponent<Player.PlayerHealth>();
            SetSerializedField(ph, "shieldVisual", shield);

            // Fire point
            var fp = new GameObject("FirePoint");
            fp.transform.SetParent(player.transform);
            fp.transform.localPosition = new Vector3(0, 1f, 0);

            var shooting = player.AddComponent<Player.PlayerShooting>();
            SetSerializedField(shooting, "firePoint", fp.transform);

            // ---- Object Pool ----
            var poolGO = new GameObject("ObjectPool");
            var pool = poolGO.AddComponent<Core.ObjectPool>();

            // We need to set the pools list via SerializedObject
            var poolSO = new SerializedObject(pool);
            var poolsProp = poolSO.FindProperty("pools");
            poolsProp.ClearArray();

            AddPoolEntry(poolsProp, "PlayerBullet", $"{PrefabPath}/PlayerBullet.prefab", 30);
            AddPoolEntry(poolsProp, "EnemyBullet", $"{PrefabPath}/EnemyBullet.prefab", 30);
            AddPoolEntry(poolsProp, "EnemyStraight", $"{PrefabPath}/EnemyStraight.prefab", 15);
            AddPoolEntry(poolsProp, "EnemyZigzag", $"{PrefabPath}/EnemyZigzag.prefab", 10);
            AddPoolEntry(poolsProp, "EnemyTracker", $"{PrefabPath}/EnemyTracker.prefab", 10);
            AddPoolEntry(poolsProp, "EnemyTank", $"{PrefabPath}/EnemyTank.prefab", 5);

            poolSO.ApplyModifiedProperties();

            // ---- Enemy Spawner ----
            var spawnerGO = new GameObject("EnemySpawner");
            var spawner = spawnerGO.AddComponent<Enemies.EnemySpawner>();

            var spawnerSO = new SerializedObject(spawner);
            var typesProp = spawnerSO.FindProperty("enemyTypes");
            typesProp.ClearArray();

            AddEnemyWeight(typesProp, "EnemyStraight", 1, 0.35f);
            AddEnemyWeight(typesProp, "EnemyZigzag", 2, 0.25f);
            AddEnemyWeight(typesProp, "EnemyTracker", 3, 0.25f);
            AddEnemyWeight(typesProp, "EnemyTank", 4, 0.15f);

            spawnerSO.ApplyModifiedProperties();

            // ---- Power-Up Spawner ----
            var puSpawnerGO = new GameObject("PowerUpSpawner");
            var puSpawner = puSpawnerGO.AddComponent<Enemies.PowerUpSpawner>();

            var puSO = new SerializedObject(puSpawner);
            var puPrefabsProp = puSO.FindProperty("powerUpPrefabs");
            puPrefabsProp.ClearArray();

            string[] puNames = { "PowerUpHealth", "PowerUpShield", "PowerUpRapidFire", "PowerUpSpreadShot" };
            for (int i = 0; i < puNames.Length; i++)
            {
                puPrefabsProp.InsertArrayElementAtIndex(i);
                var elem = puPrefabsProp.GetArrayElementAtIndex(i);
                elem.objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabPath}/{puNames[i]}.prefab");
            }
            puSO.ApplyModifiedProperties();

            // ---- HUD Canvas ----
            var hudCanvas = CreateCanvas("HUDCanvas");

            // Health bar (top left)
            var healthBarBG = new GameObject("HealthBarBG");
            healthBarBG.transform.SetParent(hudCanvas.transform, false);
            var hbRect = healthBarBG.AddComponent<RectTransform>();
            hbRect.anchorMin = new Vector2(0, 1);
            hbRect.anchorMax = new Vector2(0, 1);
            hbRect.pivot = new Vector2(0, 1);
            hbRect.anchoredPosition = new Vector2(20, -20);
            hbRect.sizeDelta = new Vector2(300, 30);

            var slider = healthBarBG.AddComponent<Slider>();
            slider.minValue = 0;
            slider.maxValue = 100;
            slider.value = 100;
            slider.interactable = false;

            // Background image for slider
            var sliderBGImg = healthBarBG.AddComponent<Image>();
            sliderBGImg.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

            // Fill area
            var fillArea = new GameObject("Fill Area");
            fillArea.transform.SetParent(healthBarBG.transform, false);
            var faRect = fillArea.AddComponent<RectTransform>();
            faRect.anchorMin = Vector2.zero;
            faRect.anchorMax = Vector2.one;
            faRect.offsetMin = new Vector2(5, 5);
            faRect.offsetMax = new Vector2(-5, -5);

            var fill = new GameObject("Fill");
            fill.transform.SetParent(fillArea.transform, false);
            var fillRect = fill.AddComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.sizeDelta = Vector2.zero;
            var fillImg = fill.AddComponent<Image>();
            fillImg.color = Color.green;

            slider.fillRect = fillRect;

            // Score text (top right)
            var scoreText = CreateTMPText(hudCanvas.transform, "ScoreText", "SCORE: 0",
                new Vector2(-20, -20), 32, TextAlignmentOptions.TopRight, Color.white);
            var scoreRect = scoreText.GetComponent<RectTransform>();
            scoreRect.anchorMin = new Vector2(1, 1);
            scoreRect.anchorMax = new Vector2(1, 1);
            scoreRect.pivot = new Vector2(1, 1);

            // Wave text (top center)
            var waveText = CreateTMPText(hudCanvas.transform, "WaveText", "WAVE 1",
                new Vector2(0, -20), 28, TextAlignmentOptions.Top, Color.yellow);
            var waveRect = waveText.GetComponent<RectTransform>();
            waveRect.anchorMin = new Vector2(0.5f, 1);
            waveRect.anchorMax = new Vector2(0.5f, 1);
            waveRect.pivot = new Vector2(0.5f, 1);

            // Combo text (below wave)
            var comboText = CreateTMPText(hudCanvas.transform, "ComboText", "",
                new Vector2(0, -55), 24, TextAlignmentOptions.Top, Color.white);
            var comboRect = comboText.GetComponent<RectTransform>();
            comboRect.anchorMin = new Vector2(0.5f, 1);
            comboRect.anchorMax = new Vector2(0.5f, 1);
            comboRect.pivot = new Vector2(0.5f, 1);

            // Lives text (below health bar)
            var livesText = CreateTMPText(hudCanvas.transform, "LivesText", "LIVES: 3",
                new Vector2(20, -60), 24, TextAlignmentOptions.TopLeft, Color.white);
            var livesRect = livesText.GetComponent<RectTransform>();
            livesRect.anchorMin = new Vector2(0, 1);
            livesRect.anchorMax = new Vector2(0, 1);
            livesRect.pivot = new Vector2(0, 1);

            // HUDManager
            var hud = hudCanvas.AddComponent<UI.HUDManager>();
            SetSerializedField(hud, "healthBar", slider);
            SetSerializedField(hud, "healthFill", fillImg);
            SetSerializedField(hud, "scoreText", scoreText.GetComponent<TextMeshProUGUI>());
            SetSerializedField(hud, "waveText", waveText.GetComponent<TextMeshProUGUI>());
            SetSerializedField(hud, "comboText", comboText.GetComponent<TextMeshProUGUI>());
            SetSerializedField(hud, "livesText", livesText.GetComponent<TextMeshProUGUI>());

            // ---- Pause Menu ----
            var pauseCanvas = CreateCanvas("PauseCanvas");
            pauseCanvas.GetComponent<Canvas>().sortingOrder = 10;

            var pausePanel = new GameObject("PausePanel");
            pausePanel.transform.SetParent(pauseCanvas.transform, false);
            var ppRect = pausePanel.AddComponent<RectTransform>();
            ppRect.anchorMin = Vector2.zero;
            ppRect.anchorMax = Vector2.one;
            ppRect.sizeDelta = Vector2.zero;
            var ppImg = pausePanel.AddComponent<Image>();
            ppImg.color = new Color(0, 0, 0, 0.7f);

            CreateTMPText(pausePanel.transform, "PausedTitle", "PAUSED",
                new Vector2(0, 100), 56, TextAlignmentOptions.Center, Color.white);

            var resumeBtn = CreateButton(pausePanel.transform, "ResumeButton", "RESUME",
                new Vector2(0, 0), new Vector2(250, 50));
            var menuBtn = CreateButton(pausePanel.transform, "MainMenuButton", "MAIN MENU",
                new Vector2(0, -70), new Vector2(250, 50));
            var quitBtn2 = CreateButton(pausePanel.transform, "QuitButton", "QUIT",
                new Vector2(0, -140), new Vector2(250, 50));

            var pauseUI = pauseCanvas.AddComponent<UI.PauseMenuUI>();
            SetSerializedField(pauseUI, "pausePanel", pausePanel);
            SetSerializedField(pauseUI, "resumeButton", resumeBtn.GetComponent<Button>());
            SetSerializedField(pauseUI, "mainMenuButton", menuBtn.GetComponent<Button>());
            SetSerializedField(pauseUI, "quitButton", quitBtn2.GetComponent<Button>());

            // ---- Game Over UI ----
            var goCanvas = CreateCanvas("GameOverCanvas");
            goCanvas.GetComponent<Canvas>().sortingOrder = 20;

            var goPanel = new GameObject("GameOverPanel");
            goPanel.transform.SetParent(goCanvas.transform, false);
            var goRect = goPanel.AddComponent<RectTransform>();
            goRect.anchorMin = Vector2.zero;
            goRect.anchorMax = Vector2.one;
            goRect.sizeDelta = Vector2.zero;
            var goImg = goPanel.AddComponent<Image>();
            goImg.color = new Color(0.1f, 0, 0, 0.8f);

            CreateTMPText(goPanel.transform, "GameOverTitle", "GAME OVER",
                new Vector2(0, 120), 64, TextAlignmentOptions.Center, Color.red);

            var finalScore = CreateTMPText(goPanel.transform, "FinalScoreText", "SCORE: 0",
                new Vector2(0, 50), 36, TextAlignmentOptions.Center, Color.white);
            var highScore = CreateTMPText(goPanel.transform, "HighScoreText", "HIGH SCORE: 0",
                new Vector2(0, 0), 28, TextAlignmentOptions.Center, Color.yellow);
            var newHS = CreateTMPText(goPanel.transform, "NewHighScoreLabel", "★ NEW HIGH SCORE! ★",
                new Vector2(0, -35), 24, TextAlignmentOptions.Center, Color.yellow);

            var restartBtn = CreateButton(goPanel.transform, "RestartButton", "PLAY AGAIN",
                new Vector2(0, -90), new Vector2(250, 50));
            var menuBtn2 = CreateButton(goPanel.transform, "MainMenuButton2", "MAIN MENU",
                new Vector2(0, -160), new Vector2(250, 50));

            var goUI = goCanvas.AddComponent<UI.GameOverUI>();
            SetSerializedField(goUI, "gameOverPanel", goPanel);
            SetSerializedField(goUI, "finalScoreText", finalScore.GetComponent<TextMeshProUGUI>());
            SetSerializedField(goUI, "highScoreText", highScore.GetComponent<TextMeshProUGUI>());
            SetSerializedField(goUI, "newHighScoreLabel", newHS.GetComponent<TextMeshProUGUI>());
            SetSerializedField(goUI, "restartButton", restartBtn.GetComponent<Button>());
            SetSerializedField(goUI, "mainMenuButton", menuBtn2.GetComponent<Button>());

            EditorSceneManager.SaveScene(scene, $"{ScenePath}/Gameplay.unity");
        }

        // ====================================================================
        // UI Helpers
        // ====================================================================
        private static GameObject CreateCanvas(string name)
        {
            var canvasGO = new GameObject(name);
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasGO.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1920, 1080);
            canvasGO.AddComponent<GraphicRaycaster>();

            // EventSystem (only if one doesn't exist)
            if (Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                var es = new GameObject("EventSystem");
                es.AddComponent<UnityEngine.EventSystems.EventSystem>();
                es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }

            return canvasGO;
        }

        private static GameObject CreateTMPText(Transform parent, string name, string text,
            Vector2 pos, int fontSize, TextAlignmentOptions alignment, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchoredPosition = pos;
            rect.sizeDelta = new Vector2(600, 80);

            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.alignment = alignment;
            tmp.color = color;

            return go;
        }

        private static GameObject CreateButton(Transform parent, string name, string label,
            Vector2 pos, Vector2 size)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchoredPosition = pos;
            rect.sizeDelta = size;

            var img = go.AddComponent<Image>();
            img.color = new Color(0.15f, 0.15f, 0.3f, 0.9f);

            go.AddComponent<Button>();

            // Label child
            var labelGO = new GameObject("Label");
            labelGO.transform.SetParent(go.transform, false);
            var labelRect = labelGO.AddComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.sizeDelta = Vector2.zero;

            var tmp = labelGO.AddComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = 28;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;

            return go;
        }

        // ====================================================================
        // Serialization Helpers
        // ====================================================================
        private static void SetSerializedField(Object target, string fieldName, object value)
        {
            if (target == null) return;
            var so = new SerializedObject(target);
            var prop = so.FindProperty(fieldName);
            if (prop == null)
            {
                Debug.LogWarning($"Could not find serialized field '{fieldName}' on {target.GetType().Name}");
                return;
            }

            switch (prop.propertyType)
            {
                case SerializedPropertyType.Integer:
                    prop.intValue = (int)value;
                    break;
                case SerializedPropertyType.Float:
                    prop.floatValue = System.Convert.ToSingle(value);
                    break;
                case SerializedPropertyType.Boolean:
                    prop.boolValue = (bool)value;
                    break;
                case SerializedPropertyType.String:
                    prop.stringValue = (string)value;
                    break;
                case SerializedPropertyType.ObjectReference:
                    prop.objectReferenceValue = value as Object;
                    break;
                case SerializedPropertyType.Enum:
                    prop.enumValueIndex = (int)value;
                    break;
            }

            so.ApplyModifiedProperties();
        }

        private static void AddPoolEntry(SerializedProperty arrayProp, string tag, string prefabPath, int size)
        {
            int idx = arrayProp.arraySize;
            arrayProp.InsertArrayElementAtIndex(idx);
            var elem = arrayProp.GetArrayElementAtIndex(idx);
            elem.FindPropertyRelative("tag").stringValue = tag;
            elem.FindPropertyRelative("prefab").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            elem.FindPropertyRelative("initialSize").intValue = size;
        }

        private static void AddEnemyWeight(SerializedProperty arrayProp, string poolTag, int minWave, float weight)
        {
            int idx = arrayProp.arraySize;
            arrayProp.InsertArrayElementAtIndex(idx);
            var elem = arrayProp.GetArrayElementAtIndex(idx);
            elem.FindPropertyRelative("poolTag").stringValue = poolTag;
            elem.FindPropertyRelative("minWave").intValue = minWave;
            elem.FindPropertyRelative("weight").floatValue = weight;
        }

        private static Sprite LoadSprite(string name)
        {
            return AssetDatabase.LoadAssetAtPath<Sprite>($"{SpritePath}/{name}.png");
        }

        private static void SavePrefab(GameObject go, string name)
        {
            string path = $"{PrefabPath}/{name}.prefab";
            PrefabUtility.SaveAsPrefabAsset(go, path);
        }

        // ====================================================================
        // Tag / Layer helpers
        // ====================================================================
        private static void EnsureTag(string tag)
        {
            var tagManager = new SerializedObject(
                AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            var tagsProp = tagManager.FindProperty("tags");

            for (int i = 0; i < tagsProp.arraySize; i++)
            {
                if (tagsProp.GetArrayElementAtIndex(i).stringValue == tag) return;
            }

            tagsProp.InsertArrayElementAtIndex(tagsProp.arraySize);
            tagsProp.GetArrayElementAtIndex(tagsProp.arraySize - 1).stringValue = tag;
            tagManager.ApplyModifiedProperties();
        }

        private static void EnsureSortingLayer(string layerName)
        {
            var tagManager = new SerializedObject(
                AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            var sortingLayers = tagManager.FindProperty("m_SortingLayers");

            for (int i = 0; i < sortingLayers.arraySize; i++)
            {
                if (sortingLayers.GetArrayElementAtIndex(i).FindPropertyRelative("name").stringValue == layerName)
                    return;
            }

            sortingLayers.InsertArrayElementAtIndex(sortingLayers.arraySize);
            var newLayer = sortingLayers.GetArrayElementAtIndex(sortingLayers.arraySize - 1);
            newLayer.FindPropertyRelative("name").stringValue = layerName;
            newLayer.FindPropertyRelative("uniqueID").intValue = layerName.GetHashCode();
            tagManager.ApplyModifiedProperties();
        }

        private static void SetLayerName(int layerIndex, string name)
        {
            var tagManager = new SerializedObject(
                AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            var prop = tagManager.FindProperty($"layers.Array.data[{layerIndex}]");
            if (prop != null)
            {
                prop.stringValue = name;
                tagManager.ApplyModifiedProperties();
            }
        }
    }
}
#endif

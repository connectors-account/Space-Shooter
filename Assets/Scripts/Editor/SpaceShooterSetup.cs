#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

using SpaceShooter.Core;
using SpaceShooter.Player;
using SpaceShooter.Enemy;
using SpaceShooter.Projectiles;
using SpaceShooter.PowerUps;
using SpaceShooter.UI;
using SpaceShooter.Background;
using SpaceShooter.Utilities;
using SpaceShooter.Resources;

namespace SpaceShooter.EditorTools
{
    /// <summary>
    /// One-click project generator. Creates tags, layers, all prefabs, and both scenes
    /// (MainMenu + GameScene) fully wired up. Run via the "Space Shooter/Setup Game" menu.
    ///
    /// Sprites are generated at runtime by the gameplay scripts, so no art assets are needed;
    /// prefabs only carry the required components and collider sizes.
    /// </summary>
    public static class SpaceShooterSetup
    {
        private const string PrefabDir = "Assets/Prefabs";
        private const string SceneDir = "Assets/Scenes";

        // Holds references to the prefabs created so the scene builder can wire them.
        private class Prefabs
        {
            public GameObject player;
            public GameObject enemyA;
            public GameObject enemyB;
            public GameObject boss;
            public GameObject playerBullet;
            public GameObject enemyBullet;
            public GameObject bossBullet;
            public GameObject shield;
            public GameObject rapidFire;
            public GameObject tripleShot;
        }

        [MenuItem("Space Shooter/Setup Game")]
        public static void SetupGame()
        {
            try
            {
                EditorUtility.DisplayProgressBar("Space Shooter Setup", "Creating tags & layers...", 0.05f);
                CreateTagsAndLayers();

                EnsureFolder(PrefabDir);
                EnsureFolder(SceneDir);

                EditorUtility.DisplayProgressBar("Space Shooter Setup", "Creating prefabs...", 0.25f);
                var prefabs = CreatePrefabs();

                EditorUtility.DisplayProgressBar("Space Shooter Setup", "Building GameScene...", 0.6f);
                CreateGameScene(prefabs);

                EditorUtility.DisplayProgressBar("Space Shooter Setup", "Building MainMenu...", 0.85f);
                CreateMainMenuScene();

                EditorUtility.DisplayProgressBar("Space Shooter Setup", "Configuring build settings...", 0.95f);
                AddScenesToBuildSettings();
                ConfigurePlayerSettings();

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            EditorUtility.DisplayDialog("Space Shooter",
                "Setup complete!\n\n" +
                "- Tags & layers created\n" +
                "- Prefabs created in Assets/Prefabs\n" +
                "- Scenes created in Assets/Scenes (MainMenu, GameScene)\n" +
                "- Scenes added to Build Settings\n\n" +
                "Open Assets/Scenes/MainMenu and press Play to test.",
                "Great!");
        }

        // ------------------------------------------------------------------
        //  Tags & Layers
        // ------------------------------------------------------------------

        private static void CreateTagsAndLayers()
        {
            string[] tags = { "Player", "Enemy", "PlayerBullet", "EnemyBullet", "PowerUp" };
            string[] layers = { "Player", "Enemy", "PlayerBullet", "EnemyBullet", "PowerUp" };

            var tagManager = new SerializedObject(
                AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);

            foreach (var t in tags) AddTag(tagManager, t);
            foreach (var l in layers) AddLayer(tagManager, l);

            tagManager.ApplyModifiedProperties();
        }

        private static void AddTag(SerializedObject tagManager, string tag)
        {
            SerializedProperty tagsProp = tagManager.FindProperty("tags");
            for (int i = 0; i < tagsProp.arraySize; i++)
                if (tagsProp.GetArrayElementAtIndex(i).stringValue == tag) return;

            tagsProp.InsertArrayElementAtIndex(tagsProp.arraySize);
            tagsProp.GetArrayElementAtIndex(tagsProp.arraySize - 1).stringValue = tag;
        }

        private static void AddLayer(SerializedObject tagManager, string layer)
        {
            SerializedProperty layersProp = tagManager.FindProperty("layers");
            // User layers are indices 8..31.
            for (int i = 8; i < layersProp.arraySize; i++)
                if (layersProp.GetArrayElementAtIndex(i).stringValue == layer) return;

            for (int i = 8; i < layersProp.arraySize; i++)
            {
                var el = layersProp.GetArrayElementAtIndex(i);
                if (string.IsNullOrEmpty(el.stringValue))
                {
                    el.stringValue = layer;
                    return;
                }
            }
            Debug.LogWarning($"[Setup] No free layer slot for '{layer}'.");
        }

        // ------------------------------------------------------------------
        //  Prefabs
        // ------------------------------------------------------------------

        private static Prefabs CreatePrefabs()
        {
            var p = new Prefabs();

            p.playerBullet = BuildBulletPrefab("PlayerBullet", "PlayerBullet");
            p.enemyBullet = BuildBulletPrefab("EnemyBullet", "EnemyBullet");
            p.bossBullet = BuildBulletPrefab("BossBullet", "EnemyBullet");

            p.player = BuildPlayerPrefab(p.playerBullet);
            p.enemyA = BuildEnemyPrefab<EnemyTypeA>("EnemyTypeA", p.enemyBullet, new Vector2(0.3f, 0.3f));
            p.enemyB = BuildEnemyPrefab<EnemyTypeB>("EnemyTypeB", p.enemyBullet, new Vector2(0.3f, 0.3f));
            p.boss = BuildEnemyPrefab<BossEnemy>("BossEnemy", p.bossBullet, new Vector2(0.9f, 0.6f));

            p.shield = BuildPowerUpPrefab<ShieldPowerUp>("ShieldPowerUp");
            p.rapidFire = BuildPowerUpPrefab<RapidFirePowerUp>("RapidFirePowerUp");
            p.tripleShot = BuildPowerUpPrefab<TripleShotPowerUp>("TripleShotPowerUp");

            return p;
        }

        private static GameObject BuildBulletPrefab(string name, string tag)
        {
            var go = new GameObject(name);
            go.tag = tag;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 3;

            var rb = go.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0f;

            var col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(0.06f, 0.14f);

            go.AddComponent<Bullet>();

            return SaveAndDestroy(go, name);
        }

        private static GameObject BuildPlayerPrefab(GameObject bulletPrefab)
        {
            var go = new GameObject("PlayerShip");
            go.tag = "Player";

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 4;

            var rb = go.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0f;

            var col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(0.28f, 0.3f);

            go.AddComponent<PlayerController>();
            go.AddComponent<PlayerHealth>();
            var shooter = go.AddComponent<PlayerShooter>();
            shooter.bulletPrefab = bulletPrefab;

            return SaveAndDestroy(go, "PlayerShip");
        }

        private static GameObject BuildEnemyPrefab<T>(string name, GameObject bulletPrefab, Vector2 colSize)
            where T : EnemyBase
        {
            var go = new GameObject(name);
            go.tag = "Enemy";

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 3;

            var rb = go.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0f;

            var col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = colSize;

            var enemy = go.AddComponent<T>();
            enemy.enemyBulletPrefab = bulletPrefab;

            return SaveAndDestroy(go, name);
        }

        private static GameObject BuildPowerUpPrefab<T>(string name) where T : PowerUpBase
        {
            var go = new GameObject(name);
            go.tag = "PowerUp";

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 4;

            var col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.16f;

            go.AddComponent<T>();

            return SaveAndDestroy(go, name);
        }

        private static GameObject SaveAndDestroy(GameObject go, string name)
        {
            string path = $"{PrefabDir}/{name}.prefab";
            var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return prefab;
        }

        // ------------------------------------------------------------------
        //  Game Scene
        // ------------------------------------------------------------------

        private static void CreateGameScene(Prefabs prefabs)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Camera
            var camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            var cam = camGo.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 5f;
            cam.backgroundColor = new Color(0.02f, 0.02f, 0.06f);
            cam.clearFlags = CameraClearFlags.SolidColor;
            camGo.transform.position = new Vector3(0, 0, -10);
            camGo.AddComponent<AudioListener>();
            camGo.AddComponent<ScreenBounds>();

            // Persistent managers
            CreateManagers();

            // Background
            var bg = new GameObject("ParallaxBackground");
            bg.AddComponent<ParallaxBackground>();

            // Bullet pools
            var playerPool = new GameObject("PlayerBulletPool").AddComponent<ObjectPool>();
            playerPool.prefab = prefabs.playerBullet;
            playerPool.prewarmCount = 40;
            playerPool.role = PoolRole.PlayerBullets;

            var enemyPool = new GameObject("EnemyBulletPool").AddComponent<ObjectPool>();
            enemyPool.prefab = prefabs.enemyBullet;
            enemyPool.prewarmCount = 120;
            enemyPool.role = PoolRole.EnemyBullets;

            // Player
            var player = (GameObject)PrefabUtility.InstantiatePrefab(prefabs.player);
            player.transform.position = new Vector3(0, -3.5f, 0);

            // Spawner + WaveManager
            var spawnerGo = new GameObject("EnemySpawner");
            var spawner = spawnerGo.AddComponent<EnemySpawner>();
            spawner.enemyTypeAPrefab = prefabs.enemyA;
            spawner.enemyTypeBPrefab = prefabs.enemyB;
            spawner.bossPrefab = prefabs.boss;
            spawner.enemyBulletPrefab = prefabs.enemyBullet;
            spawner.powerUpPrefabs = new[] { prefabs.shield, prefabs.rapidFire, prefabs.tripleShot };

            var waveGo = new GameObject("WaveManager");
            var wave = waveGo.AddComponent<WaveManager>();
            wave.spawner = spawner;

            // UI
            BuildGameUI();

            string path = $"{SceneDir}/GameScene.unity";
            EditorSceneManager.SaveScene(scene, path);
        }

        private static void CreateManagers()
        {
            var gm = new GameObject("GameManager");
            gm.AddComponent<GameManager>();

            var am = new GameObject("AudioManager");
            am.AddComponent<AudioManager>();
            am.AddComponent<AudioClipPlaceholder>();

            var sl = new GameObject("SceneLoader");
            sl.AddComponent<SceneLoader>();

            var sm = new GameObject("ScoreManager");
            sm.AddComponent<ScoreManager>();
        }

        private static void BuildGameUI()
        {
            var canvasGo = CreateCanvas("HUDCanvas");
            var canvas = canvasGo.transform;

            var uiManager = canvasGo.AddComponent<UIManager>();
            var hud = canvasGo.AddComponent<HUDController>();

            // --- HUD panel ---
            var hudPanel = CreatePanel(canvas, "HUDPanel", new Color(0, 0, 0, 0));

            hud.scoreText = CreateText(hudPanel.transform, "ScoreText", "SCORE\n0",
                new Vector2(1, 1), new Vector2(1, 1), new Vector2(-20, -20), new Vector2(260, 70),
                26, TextAnchor.UpperRight, Color.white);

            hud.multiplierText = CreateText(hudPanel.transform, "MultiplierText", "",
                new Vector2(1, 1), new Vector2(1, 1), new Vector2(-20, -95), new Vector2(260, 40),
                24, TextAnchor.UpperRight, new Color(1f, 0.9f, 0.3f));

            hud.waveText = CreateText(hudPanel.transform, "WaveText", "WAVE 1 / 10",
                new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -20), new Vector2(400, 50),
                28, TextAnchor.UpperCenter, Color.white);

            hud.countdownText = CreateText(hudPanel.transform, "CountdownText", "",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 120), new Vector2(600, 60),
                34, TextAnchor.MiddleCenter, new Color(0.6f, 1f, 1f));

            // Health bar (top left)
            var healthBg = CreateImage(hudPanel.transform, "HealthBarBG",
                new Vector2(0, 1), new Vector2(0, 1), new Vector2(20, -20), new Vector2(300, 26),
                new Color(0.15f, 0.15f, 0.15f, 0.85f));
            hud.healthFill = CreateFillImage(healthBg.transform, "HealthFill", new Color(0.9f, 0.2f, 0.2f));

            hud.livesText = CreateText(hudPanel.transform, "LivesText", "Lives: ***",
                new Vector2(0, 1), new Vector2(0, 1), new Vector2(20, -52), new Vector2(300, 34),
                22, TextAnchor.UpperLeft, new Color(0.4f, 1f, 0.5f));

            // Power-up timer bar (top left, below lives)
            hud.powerUpBarRoot = CreateImage(hudPanel.transform, "PowerUpBarBG",
                new Vector2(0, 1), new Vector2(0, 1), new Vector2(20, -92), new Vector2(220, 16),
                new Color(0.15f, 0.15f, 0.15f, 0.85f));
            hud.powerUpFill = CreateFillImage(hud.powerUpBarRoot.transform, "PowerUpFill",
                new Color(0.3f, 0.8f, 1f));

            // Boss bar (bottom center)
            hud.bossBarRoot = CreateImage(hudPanel.transform, "BossBarBG",
                new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 40), new Vector2(700, 24),
                new Color(0.15f, 0.05f, 0.05f, 0.9f));
            hud.bossHealthFill = CreateFillImage(hud.bossBarRoot.transform, "BossFill",
                new Color(1f, 0.25f, 0.25f));
            hud.bossBarRoot.SetActive(false);

            // --- Pause panel ---
            var pausePanel = CreatePanel(canvas, "PausePanel", new Color(0, 0, 0, 0.6f));
            var pauseCtrl = canvasGo.AddComponent<PauseMenuController>();
            pauseCtrl.panelRoot = pausePanel;
            CreateText(pausePanel.transform, "PausedTitle", "PAUSED",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 200), new Vector2(500, 80),
                48, TextAnchor.MiddleCenter, Color.white);
            pauseCtrl.resumeButton = CreateButton(pausePanel.transform, "ResumeButton", "RESUME", new Vector2(0, 90));
            pauseCtrl.restartButton = CreateButton(pausePanel.transform, "RestartButton", "RESTART", new Vector2(0, 20));
            pauseCtrl.mainMenuButton = CreateButton(pausePanel.transform, "MenuButton", "MAIN MENU", new Vector2(0, -50));
            pauseCtrl.quitButton = CreateButton(pausePanel.transform, "QuitButton", "QUIT", new Vector2(0, -120));
            pauseCtrl.sfxSlider = CreateSlider(pausePanel.transform, "SFXSlider", "SFX", new Vector2(0, -200));
            pauseCtrl.musicSlider = CreateSlider(pausePanel.transform, "MusicSlider", "Music", new Vector2(0, -260));
            pausePanel.SetActive(false);

            // --- Game over panel ---
            var overPanel = CreatePanel(canvas, "GameOverPanel", new Color(0, 0, 0, 0.75f));
            var overCtrl = canvasGo.AddComponent<GameOverController>();
            overCtrl.panelRoot = overPanel;
            overCtrl.titleText = CreateText(overPanel.transform, "GOTitle", "GAME OVER",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 180), new Vector2(700, 90),
                56, TextAnchor.MiddleCenter, new Color(1f, 0.35f, 0.35f));
            overCtrl.finalScoreText = CreateText(overPanel.transform, "FinalScore", "FINAL SCORE: 0",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 90), new Vector2(600, 50),
                30, TextAnchor.MiddleCenter, Color.white);
            overCtrl.highScoreText = CreateText(overPanel.transform, "HighScore", "HIGH SCORE: 0",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 40), new Vector2(600, 50),
                26, TextAnchor.MiddleCenter, new Color(0.9f, 0.9f, 0.5f));
            overCtrl.newRecordText = CreateText(overPanel.transform, "NewRecord", "",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 0), new Vector2(600, 40),
                24, TextAnchor.MiddleCenter, new Color(0.4f, 1f, 0.5f));
            overCtrl.restartButton = CreateButton(overPanel.transform, "GORestart", "RESTART", new Vector2(0, -80));
            overCtrl.mainMenuButton = CreateButton(overPanel.transform, "GOMenu", "MAIN MENU", new Vector2(0, -150));
            overPanel.SetActive(false);

            // Register panels with UIManager
            uiManager.panels = new List<UIManager.NamedPanel>
            {
                new UIManager.NamedPanel { name = "HUD", panel = hudPanel },
                new UIManager.NamedPanel { name = "Pause", panel = pausePanel },
                new UIManager.NamedPanel { name = "GameOver", panel = overPanel },
            };

            CreateEventSystem();
        }

        // ------------------------------------------------------------------
        //  Main Menu Scene
        // ------------------------------------------------------------------

        private static void CreateMainMenuScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            var cam = camGo.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 5f;
            cam.backgroundColor = new Color(0.02f, 0.02f, 0.06f);
            cam.clearFlags = CameraClearFlags.SolidColor;
            camGo.transform.position = new Vector3(0, 0, -10);
            camGo.AddComponent<AudioListener>();

            CreateManagers();

            var canvasGo = CreateCanvas("MenuCanvas");
            var canvas = canvasGo.transform;

            var menuPanel = CreatePanel(canvas, "MainMenuPanel", new Color(0, 0, 0, 0));
            var menu = canvasGo.AddComponent<MainMenuController>();

            // Starfield background (RawImage filling the screen)
            var starGo = CreateImageRaw(menuPanel.transform, "Starfield",
                new Vector2(0, 0), new Vector2(1, 1), Vector2.zero, Vector2.zero);
            menu.starfield = starGo.GetComponent<RawImage>();

            menu.titleText = CreateText(menuPanel.transform, "Title", "SPACE SHOOTER",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 180), new Vector2(900, 120),
                72, TextAnchor.MiddleCenter, new Color(0.3f, 0.9f, 1f));

            menu.highScoreText = CreateText(menuPanel.transform, "HighScore", "HIGH SCORE: 0",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 60), new Vector2(600, 50),
                28, TextAnchor.MiddleCenter, new Color(0.9f, 0.9f, 0.5f));

            menu.playButton = CreateButton(menuPanel.transform, "PlayButton", "PLAY", new Vector2(0, -30));
            menu.quitButton = CreateButton(menuPanel.transform, "QuitButton", "QUIT", new Vector2(0, -110));

            menu.versionText = CreateText(menuPanel.transform, "Version", "v1.0.0",
                new Vector2(1, 0), new Vector2(1, 0), new Vector2(-20, 20), new Vector2(200, 30),
                18, TextAnchor.LowerRight, new Color(0.6f, 0.6f, 0.6f));

            CreateEventSystem();

            string path = $"{SceneDir}/MainMenu.unity";
            EditorSceneManager.SaveScene(scene, path);
        }

        // ------------------------------------------------------------------
        //  UI helpers
        // ------------------------------------------------------------------

        private static Font GetFont()
        {
            Font f = null;
            try { f = UnityEngine.Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); } catch { }
            if (f == null)
            {
                try { f = UnityEngine.Resources.GetBuiltinResource<Font>("Arial.ttf"); } catch { }
            }
            return f;
        }

        private static GameObject CreateCanvas(string name)
        {
            var go = new GameObject(name);
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            go.AddComponent<GraphicRaycaster>();
            return go;
        }

        private static void CreateEventSystem()
        {
            if (Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() != null) return;
            var es = new GameObject("EventSystem");
            es.AddComponent<UnityEngine.EventSystems.EventSystem>();
            es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        private static GameObject CreatePanel(Transform parent, string name, Color bg)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            var img = go.AddComponent<Image>();
            img.color = bg;
            img.raycastTarget = bg.a > 0.01f;
            return go;
        }

        private static Text CreateText(Transform parent, string name, string content,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 size,
            int fontSize, TextAnchor align, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;

            var text = go.AddComponent<Text>();
            text.text = content;
            text.font = GetFont();
            text.fontSize = fontSize;
            text.alignment = align;
            text.color = color;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            return text;
        }

        private static GameObject CreateImage(Transform parent, string name,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 size, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;
            var img = go.AddComponent<Image>();
            img.color = color;
            return go;
        }

        private static GameObject CreateImageRaw(Transform parent, string name,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 offMin, Vector2 offMax)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = offMin;
            rt.offsetMax = offMax;
            var img = go.AddComponent<RawImage>();
            img.color = new Color(1, 1, 1, 0.5f);
            return go;
        }

        private static Image CreateFillImage(Transform parent, string name, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(2, 2);
            rt.offsetMax = new Vector2(-2, -2);
            var img = go.AddComponent<Image>();
            img.color = color;
            img.type = Image.Type.Filled;
            img.fillMethod = Image.FillMethod.Horizontal;
            img.fillOrigin = (int)Image.OriginHorizontal.Left;
            img.fillAmount = 1f;
            // Needs a sprite for filled type to render; use the built-in UI sprite.
            img.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            return img;
        }

        private static Button CreateButton(Transform parent, string name, string label, Vector2 anchoredPos)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = new Vector2(280, 56);

            var img = go.AddComponent<Image>();
            img.color = new Color(0.2f, 0.35f, 0.55f, 0.95f);
            img.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            img.type = Image.Type.Sliced;

            var btn = go.AddComponent<Button>();
            btn.targetGraphic = img;
            var colors = btn.colors;
            colors.highlightedColor = new Color(0.35f, 0.55f, 0.8f);
            colors.pressedColor = new Color(0.15f, 0.25f, 0.4f);
            btn.colors = colors;

            CreateText(go.transform, "Label", label,
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero,
                26, TextAnchor.MiddleCenter, Color.white);

            return btn;
        }

        private static Slider CreateSlider(Transform parent, string name, string label, Vector2 anchoredPos)
        {
            var container = new GameObject(name, typeof(RectTransform));
            container.transform.SetParent(parent, false);
            var crt = container.GetComponent<RectTransform>();
            crt.anchorMin = new Vector2(0.5f, 0.5f);
            crt.anchorMax = new Vector2(0.5f, 0.5f);
            crt.pivot = new Vector2(0.5f, 0.5f);
            crt.anchoredPosition = anchoredPos;
            crt.sizeDelta = new Vector2(360, 40);

            CreateText(container.transform, "Label", label,
                new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(-110, 0), new Vector2(120, 40),
                20, TextAnchor.MiddleLeft, Color.white);

            var sliderGo = new GameObject("Slider", typeof(RectTransform));
            sliderGo.transform.SetParent(container.transform, false);
            var srt = sliderGo.GetComponent<RectTransform>();
            srt.anchorMin = new Vector2(0.5f, 0.5f);
            srt.anchorMax = new Vector2(0.5f, 0.5f);
            srt.pivot = new Vector2(0.5f, 0.5f);
            srt.anchoredPosition = new Vector2(60, 0);
            srt.sizeDelta = new Vector2(220, 20);

            var bgImg = sliderGo.AddComponent<Image>();
            bgImg.color = new Color(0.2f, 0.2f, 0.2f, 0.9f);
            bgImg.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
            bgImg.type = Image.Type.Sliced;

            var slider = sliderGo.AddComponent<Slider>();

            // Fill area
            var fillArea = new GameObject("Fill Area", typeof(RectTransform));
            fillArea.transform.SetParent(sliderGo.transform, false);
            var fart = fillArea.GetComponent<RectTransform>();
            fart.anchorMin = new Vector2(0, 0.25f);
            fart.anchorMax = new Vector2(1, 0.75f);
            fart.offsetMin = new Vector2(5, 0);
            fart.offsetMax = new Vector2(-5, 0);

            var fill = new GameObject("Fill", typeof(RectTransform));
            fill.transform.SetParent(fillArea.transform, false);
            var frt = fill.GetComponent<RectTransform>();
            frt.anchorMin = Vector2.zero;
            frt.anchorMax = new Vector2(1, 1);
            frt.sizeDelta = Vector2.zero;
            var fillImg = fill.AddComponent<Image>();
            fillImg.color = new Color(0.3f, 0.7f, 1f);
            fillImg.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            fillImg.type = Image.Type.Sliced;

            // Handle
            var handleArea = new GameObject("Handle Slide Area", typeof(RectTransform));
            handleArea.transform.SetParent(sliderGo.transform, false);
            var hart = handleArea.GetComponent<RectTransform>();
            hart.anchorMin = Vector2.zero;
            hart.anchorMax = Vector2.one;
            hart.offsetMin = new Vector2(10, 0);
            hart.offsetMax = new Vector2(-10, 0);

            var handle = new GameObject("Handle", typeof(RectTransform));
            handle.transform.SetParent(handleArea.transform, false);
            var hrt = handle.GetComponent<RectTransform>();
            hrt.sizeDelta = new Vector2(16, 24);
            var handleImg = handle.AddComponent<Image>();
            handleImg.color = Color.white;
            handleImg.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");

            slider.fillRect = frt;
            slider.handleRect = hrt;
            slider.targetGraphic = handleImg;
            slider.direction = Slider.Direction.LeftToRight;
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = 0.6f;

            return slider;
        }

        // ------------------------------------------------------------------
        //  Build settings & project config
        // ------------------------------------------------------------------

        private static void AddScenesToBuildSettings()
        {
            var scenes = new List<EditorBuildSettingsScene>
            {
                new EditorBuildSettingsScene($"{SceneDir}/MainMenu.unity", true),
                new EditorBuildSettingsScene($"{SceneDir}/GameScene.unity", true),
            };
            EditorBuildSettings.scenes = scenes.ToArray();
        }

        private static void ConfigurePlayerSettings()
        {
            PlayerSettings.productName = "Space Shooter";
            PlayerSettings.companyName = "IndieDev";
            PlayerSettings.defaultScreenWidth = 1920;
            PlayerSettings.defaultScreenHeight = 1080;
            PlayerSettings.defaultIsNativeResolution = true;
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = System.IO.Path.GetDirectoryName(path).Replace('\\', '/');
            string leaf = System.IO.Path.GetFileName(path);
            if (!AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, leaf);
        }
    }
}
#endif

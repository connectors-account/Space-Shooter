#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

namespace SpaceShooter.EditorTools
{
    /// <summary>
    /// One-click builder that generates sprites, prefabs and a fully wired
    /// playable scene for the Space Shooter game.
    ///
    /// Usage in Unity:  menu  ->  "Space Shooter/Build Complete Game"
    /// Then press Play, or build for Windows via "Space Shooter/Build Windows Executable".
    /// </summary>
    public static class SceneBuilder
    {
        private const string PrefabFolder = "Assets/Prefabs";
        private const string SceneFolder = "Assets/Scenes";
        private const string ScenePath = "Assets/Scenes/Game.unity";

        // Cached prefab references built during generation.
        private static GameObject playerBulletPrefab;
        private static GameObject enemyBulletPrefab;
        private static GameObject explosionPrefab;
        private static GameObject hitEffectPrefab;
        private static GameObject[] powerUpPrefabs;
        private static GameObject[] enemyPrefabs;
        private static GameObject playerPrefab;

        [MenuItem("Space Shooter/Build Complete Game")]
        public static void BuildGame()
        {
            Directory.CreateDirectory(PrefabFolder);
            Directory.CreateDirectory(SceneFolder);

            EnsureTags();

            BuildEffectPrefabs();
            BuildBulletPrefabs();
            BuildPowerUpPrefabs();
            BuildEnemyPrefabs();
            BuildPlayerPrefab();

            BuildScene();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("<color=lime>[SpaceShooter] Game built successfully! Press Play to start.</color>");
            EditorUtility.DisplayDialog("Space Shooter",
                "Game built successfully!\n\nThe scene 'Game' is now open. Press Play to test.\n\nTo build a Windows .exe use menu: Space Shooter > Build Windows Executable.",
                "OK");
        }

        // ---------------- Reflection helper to set [SerializeField] private fields ----------------

        private static void SetField(Object target, string fieldName, object value)
        {
            SerializedObject so = new SerializedObject(target);
            SerializedProperty prop = so.FindProperty(fieldName);
            if (prop == null)
            {
                Debug.LogWarning($"[SceneBuilder] Field '{fieldName}' not found on {target.GetType().Name}");
                return;
            }
            AssignProperty(prop, value);
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void AssignProperty(SerializedProperty prop, object value)
        {
            switch (prop.propertyType)
            {
                case SerializedPropertyType.ObjectReference:
                    prop.objectReferenceValue = value as Object;
                    break;
                case SerializedPropertyType.Float:
                    prop.floatValue = System.Convert.ToSingle(value);
                    break;
                case SerializedPropertyType.Integer:
                    prop.intValue = System.Convert.ToInt32(value);
                    break;
                case SerializedPropertyType.Boolean:
                    prop.boolValue = System.Convert.ToBoolean(value);
                    break;
                case SerializedPropertyType.Enum:
                    prop.enumValueIndex = System.Convert.ToInt32(value);
                    break;
                case SerializedPropertyType.Color:
                    prop.colorValue = (Color)value;
                    break;
                default:
                    Debug.LogWarning($"[SceneBuilder] Unsupported property type {prop.propertyType} for {prop.name}");
                    break;
            }
        }

        private static void SetObjectArray(Object target, string fieldName, Object[] values)
        {
            SerializedObject so = new SerializedObject(target);
            SerializedProperty prop = so.FindProperty(fieldName);
            if (prop == null) { Debug.LogWarning($"[SceneBuilder] Array '{fieldName}' not found"); return; }
            prop.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++)
            {
                prop.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            }
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        // ---------------- Tags ----------------

        private static void EnsureTags()
        {
            string[] tags = { "Player", "Enemy", "Bullet", "PowerUp" };
            SerializedObject tagManager = new SerializedObject(
                AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty tagsProp = tagManager.FindProperty("tags");

            foreach (string tag in tags)
            {
                bool found = false;
                for (int i = 0; i < tagsProp.arraySize; i++)
                {
                    if (tagsProp.GetArrayElementAtIndex(i).stringValue == tag) { found = true; break; }
                }
                if (!found)
                {
                    tagsProp.InsertArrayElementAtIndex(tagsProp.arraySize);
                    tagsProp.GetArrayElementAtIndex(tagsProp.arraySize - 1).stringValue = tag;
                }
            }
            tagManager.ApplyModifiedProperties();
        }

        // ---------------- Effect prefabs ----------------

        private static void BuildEffectPrefabs()
        {
            explosionPrefab = BuildParticleEffect("Explosion", new Color(1f, 0.6f, 0.1f), 0.6f, 40, 4f);
            hitEffectPrefab = BuildParticleEffect("HitEffect", new Color(1f, 1f, 0.4f), 0.3f, 12, 2.5f);
        }

        private static GameObject BuildParticleEffect(string name, Color color, float duration, int count, float speed)
        {
            GameObject go = new GameObject(name);
            ParticleSystem ps = go.AddComponent<ParticleSystem>();

            var main = ps.main;
            main.duration = duration;
            main.loop = false;
            main.startLifetime = duration;
            main.startSpeed = speed;
            main.startSize = 0.2f;
            main.startColor = color;
            main.maxParticles = count;

            var emission = ps.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, (short)count) });

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.1f;

            ParticleSystemRenderer renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.material = new Material(Shader.Find("Sprites/Default"));
            renderer.sortingOrder = 50;

            go.AddComponent<AutoDestroy>();

            GameObject prefab = SavePrefab(go, name);
            Object.DestroyImmediate(go);
            return prefab;
        }

        // ---------------- Bullet prefabs ----------------

        private static void BuildBulletPrefabs()
        {
            Sprite playerBulletSprite = SpriteFactory.CreateOrLoad("bullet_player", 8, 24,
                SpriteFactory.Rect(new Color(0.4f, 1f, 1f)));
            Sprite enemyBulletSprite = SpriteFactory.CreateOrLoad("bullet_enemy", 10, 10,
                SpriteFactory.Circle(10, 10, new Color(1f, 0.4f, 0.3f)));

            playerBulletPrefab = BuildBullet("PlayerBullet", playerBulletSprite, BulletOwner.Player, 14f, 25f, hitEffectPrefab);
            enemyBulletPrefab = BuildBullet("EnemyBullet", enemyBulletSprite, BulletOwner.Enemy, 7f, 15f, hitEffectPrefab);
        }

        private static GameObject BuildBullet(string name, Sprite sprite, BulletOwner owner, float speed, float damage, GameObject hitFx)
        {
            GameObject go = new GameObject(name);
            go.tag = "Bullet";

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = 5;

            Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0f;

            BoxCollider2D col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = true;

            BulletController bc = go.AddComponent<BulletController>();
            SetField(bc, "owner", (int)owner);
            SetField(bc, "speed", speed);
            SetField(bc, "damage", damage);
            SetField(bc, "lifeTime", 4f);
            if (hitFx != null) SetField(bc, "hitEffect", hitFx);

            GameObject prefab = SavePrefab(go, name);
            Object.DestroyImmediate(go);
            return prefab;
        }

        // ---------------- Power-up prefabs ----------------

        private static void BuildPowerUpPrefabs()
        {
            Sprite healthSprite = SpriteFactory.CreateOrLoad("powerup_health", 28, 28,
                SpriteFactory.RoundedRect(28, 28, new Color(0.2f, 1f, 0.3f)));
            Sprite weaponSprite = SpriteFactory.CreateOrLoad("powerup_weapon", 28, 28,
                SpriteFactory.Diamond(28, 28, new Color(1f, 0.85f, 0.2f)));
            Sprite shieldSprite = SpriteFactory.CreateOrLoad("powerup_shield", 28, 28,
                SpriteFactory.Circle(28, 28, new Color(0.3f, 0.6f, 1f)));

            GameObject health = BuildPowerUp("PowerUp_Health", healthSprite, PowerUpType.Health);
            GameObject weapon = BuildPowerUp("PowerUp_Weapon", weaponSprite, PowerUpType.WeaponUpgrade);
            GameObject shield = BuildPowerUp("PowerUp_Shield", shieldSprite, PowerUpType.Shield);
            powerUpPrefabs = new[] { health, weapon, shield };
        }

        private static GameObject BuildPowerUp(string name, Sprite sprite, PowerUpType type)
        {
            GameObject go = new GameObject(name);
            go.tag = "PowerUp";

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = 4;

            CircleCollider2D col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;

            PowerUpController pc = go.AddComponent<PowerUpController>();
            SetField(pc, "type", (int)type);

            GameObject prefab = SavePrefab(go, name);
            Object.DestroyImmediate(go);
            return prefab;
        }

        // ---------------- Enemy prefabs ----------------

        private static void BuildEnemyPrefabs()
        {
            Sprite straightSprite = SpriteFactory.CreateOrLoad("enemy_straight", 40, 40,
                SpriteFactory.InvertedTriangle(40, 40, new Color(1f, 0.35f, 0.35f)));
            Sprite zigzagSprite = SpriteFactory.CreateOrLoad("enemy_zigzag", 40, 40,
                SpriteFactory.Diamond(40, 40, new Color(1f, 0.6f, 0.2f)));
            Sprite chaserSprite = SpriteFactory.CreateOrLoad("enemy_chaser", 40, 40,
                SpriteFactory.Circle(40, 40, new Color(0.8f, 0.3f, 1f)));
            Sprite shooterSprite = SpriteFactory.CreateOrLoad("enemy_shooter", 44, 44,
                SpriteFactory.RoundedRect(44, 44, new Color(1f, 0.25f, 0.5f)));

            GameObject straight = BuildEnemy("Enemy_Straight", straightSprite, EnemyType.Straight, 50f, 100, 3f, false);
            GameObject zigzag = BuildEnemy("Enemy_Zigzag", zigzagSprite, EnemyType.Zigzag, 60f, 150, 3f, false);
            GameObject chaser = BuildEnemy("Enemy_Chaser", chaserSprite, EnemyType.Chaser, 70f, 200, 2.5f, false);
            GameObject shooter = BuildEnemy("Enemy_Shooter", shooterSprite, EnemyType.Shooter, 90f, 250, 2f, true);

            // Order must match EnemySpawner index mapping: 0=Straight,1=Zigzag,2=Chaser,3=Shooter
            enemyPrefabs = new[] { straight, zigzag, chaser, shooter };
        }

        private static GameObject BuildEnemy(string name, Sprite sprite, EnemyType type, float health, int score, float speed, bool shoots)
        {
            GameObject go = new GameObject(name);
            go.tag = "Enemy";

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = 3;

            Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0f;

            BoxCollider2D col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = true;

            HealthSystem hs = go.AddComponent<HealthSystem>();
            SetField(hs, "maxHealth", health);

            EnemyController ec = go.AddComponent<EnemyController>();
            SetField(ec, "type", (int)type);
            SetField(ec, "moveSpeed", speed);
            SetField(ec, "scoreValue", score);
            SetField(ec, "explosionEffect", explosionPrefab);
            SetObjectArray(ec, "powerUpPrefabs", powerUpPrefabs);

            if (shoots)
            {
                GameObject firePoint = new GameObject("FirePoint");
                firePoint.transform.SetParent(go.transform);
                firePoint.transform.localPosition = new Vector3(0f, -0.3f, 0f);
                SetField(ec, "bulletPrefab", enemyBulletPrefab);
                SetField(ec, "firePoint", firePoint.transform);
            }

            GameObject prefab = SavePrefab(go, name);
            Object.DestroyImmediate(go);
            return prefab;
        }

        // ---------------- Player prefab ----------------

        private static void BuildPlayerPrefab()
        {
            Sprite shipSprite = SpriteFactory.CreateOrLoad("player_ship", 48, 48,
                SpriteFactory.Triangle(48, 48, new Color(0.3f, 0.8f, 1f)));
            Sprite shieldSprite = SpriteFactory.CreateOrLoad("shield_ring", 72, 72,
                SpriteFactory.Circle(72, 72, new Color(0.4f, 0.7f, 1f, 0.35f)));

            GameObject go = new GameObject("Player");
            go.tag = "Player";

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = shipSprite;
            sr.sortingOrder = 10;

            Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0f;

            BoxCollider2D col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = true;

            HealthSystem hs = go.AddComponent<HealthSystem>();
            SetField(hs, "maxHealth", 100f);

            // Fire point
            GameObject firePoint = new GameObject("FirePoint");
            firePoint.transform.SetParent(go.transform);
            firePoint.transform.localPosition = new Vector3(0f, 0.5f, 0f);

            // Shield visual (child sprite, disabled by default)
            GameObject shieldVisual = new GameObject("ShieldVisual");
            shieldVisual.transform.SetParent(go.transform);
            shieldVisual.transform.localPosition = Vector3.zero;
            SpriteRenderer shieldSr = shieldVisual.AddComponent<SpriteRenderer>();
            shieldSr.sprite = shieldSprite;
            shieldSr.sortingOrder = 11;
            shieldVisual.SetActive(false);

            PlayerController pc = go.AddComponent<PlayerController>();
            SetField(pc, "bulletPrefab", playerBulletPrefab);
            SetObjectArray(pc, "firePoints", new Object[] { firePoint.transform });
            SetField(pc, "shieldVisual", shieldVisual);

            playerPrefab = SavePrefab(go, "Player");
            Object.DestroyImmediate(go);
        }

        // ---------------- Scene ----------------

        private static void BuildScene()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Camera
            GameObject camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            Camera cam = camGo.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 6f;
            cam.backgroundColor = new Color(0.02f, 0.02f, 0.08f);
            cam.clearFlags = CameraClearFlags.SolidColor;
            camGo.transform.position = new Vector3(0f, 0f, -10f);
            camGo.AddComponent<AudioListener>();

            // Parallax background
            GameObject bg = BuildBackground();

            // Player instance
            GameObject player = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab);
            player.transform.position = new Vector3(0f, -4f, 0f);
            PlayerController playerCtrl = player.GetComponent<PlayerController>();
            HealthSystem playerHealth = player.GetComponent<HealthSystem>();

            // Spawn point
            GameObject spawnPoint = new GameObject("PlayerSpawnPoint");
            spawnPoint.transform.position = new Vector3(0f, -4f, 0f);

            // Managers
            GameObject managers = new GameObject("Managers");
            ScoreManager scoreMgr = managers.AddComponent<ScoreManager>();
            AudioManager audioMgr = managers.AddComponent<AudioManager>();
            GameManager gameMgr = managers.AddComponent<GameManager>();

            // Spawner
            GameObject spawnerGo = new GameObject("EnemySpawner");
            EnemySpawner spawner = spawnerGo.AddComponent<EnemySpawner>();
            SetObjectArray(spawner, "enemyPrefabs", enemyPrefabs);

            // Wire GameManager
            SetField(gameMgr, "player", playerCtrl);
            SetField(gameMgr, "spawner", spawner);
            SetField(gameMgr, "playerSpawnPoint", spawnPoint.transform);

            // UI
            UIManager uiManager = BuildUI(playerHealth);

            // EventSystem
            BuildEventSystem();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);

            AddSceneToBuildSettings(ScenePath);
        }

        private static GameObject BuildBackground()
        {
            Sprite farSprite = SpriteFactory.CreateOrLoad("bg_far", 256, 256,
                SpriteFactory.Starfield(256, 256, new Color(0.02f, 0.02f, 0.08f), new Color(0.5f, 0.5f, 0.7f), 0.01f, 1));
            Sprite nearSprite = SpriteFactory.CreateOrLoad("bg_near", 256, 256,
                SpriteFactory.Starfield(256, 256, Color.clear, Color.white, 0.006f, 2));

            GameObject root = new GameObject("ParallaxBackground");
            ParallaxBackground parallax = root.AddComponent<ParallaxBackground>();

            // Two layers, each with two stacked tiles scaled to cover the camera.
            Transform farLayer = BuildParallaxLayer(root.transform, "FarLayer", farSprite, 0, 1f);
            Transform nearLayer = BuildParallaxLayer(root.transform, "NearLayer", nearSprite, 1, 2.2f);

            // Configure the ParallaxBackground.layers array (list of serializable structs).
            SerializedObject so = new SerializedObject(parallax);
            SerializedProperty layers = so.FindProperty("layers");
            layers.arraySize = 2;

            SerializedProperty far = layers.GetArrayElementAtIndex(0);
            far.FindPropertyRelative("layerRoot").objectReferenceValue = farLayer;
            far.FindPropertyRelative("scrollSpeed").floatValue = 1f;

            SerializedProperty near = layers.GetArrayElementAtIndex(1);
            near.FindPropertyRelative("layerRoot").objectReferenceValue = nearLayer;
            near.FindPropertyRelative("scrollSpeed").floatValue = 2.2f;

            so.ApplyModifiedPropertiesWithoutUndo();
            return root;
        }

        private static Transform BuildParallaxLayer(Transform parent, string name, Sprite sprite, int order, float speed)
        {
            GameObject layer = new GameObject(name);
            layer.transform.SetParent(parent);

            for (int i = 0; i < 2; i++)
            {
                GameObject tile = new GameObject($"Tile{i}");
                tile.transform.SetParent(layer.transform);
                SpriteRenderer sr = tile.AddComponent<SpriteRenderer>();
                sr.sprite = sprite;
                sr.sortingOrder = -10 + order;
                // Scale the 256px sprite (2.56 world units) to cover a ~16x14 camera view.
                tile.transform.localScale = new Vector3(7f, 7f, 1f);
                tile.transform.position = new Vector3(0f, i * (2.56f * 7f), 0f);
            }
            return layer.transform;
        }

        // ---------------- UI ----------------

        private static UIManager BuildUI(HealthSystem playerHealth)
        {
            GameObject canvasGo = new GameObject("Canvas");
            Canvas canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            canvasGo.AddComponent<GraphicRaycaster>();

            UIManager ui = canvasGo.AddComponent<UIManager>();

            // ----- Main Menu Panel -----
            GameObject mainMenu = CreatePanel(canvasGo.transform, "MainMenuPanel", new Color(0.02f, 0.02f, 0.1f, 0.95f));
            CreateText(mainMenu.transform, "Title", "SPACE SHOOTER", 90, new Vector2(0, 250), new Vector2(1200, 160), new Color(0.4f, 0.9f, 1f));
            TMP_Text menuHigh = CreateText(mainMenu.transform, "HighScore", "HIGH SCORE: 0", 40, new Vector2(0, 120), new Vector2(800, 70), Color.yellow);
            Button startBtn = CreateButton(mainMenu.transform, "StartButton", "START", new Vector2(0, -20), new Vector2(360, 90));
            Button quitBtn = CreateButton(mainMenu.transform, "QuitButton", "QUIT", new Vector2(0, -140), new Vector2(360, 90));
            CreateText(mainMenu.transform, "Controls", "MOVE: WASD / ARROWS    SHOOT: SPACE    PAUSE: ESC", 28, new Vector2(0, -280), new Vector2(1400, 60), Color.gray);

            // ----- HUD Panel -----
            GameObject hud = CreatePanel(canvasGo.transform, "HUDPanel", Color.clear);
            TMP_Text scoreText = CreateText(hud.transform, "ScoreText", "SCORE: 0", 40, Vector2.zero, new Vector2(500, 60), Color.white);
            AnchorTopLeft(scoreText.rectTransform, new Vector2(30, -30));
            scoreText.alignment = TextAlignmentOptions.TopLeft;

            TMP_Text waveText = CreateText(hud.transform, "WaveText", "WAVE: 1", 40, Vector2.zero, new Vector2(500, 60), Color.white);
            AnchorTopRight(waveText.rectTransform, new Vector2(-30, -30));
            waveText.alignment = TextAlignmentOptions.TopRight;

            TMP_Text livesText = CreateText(hud.transform, "LivesText", "LIVES: 3", 36, Vector2.zero, new Vector2(400, 50), Color.white);
            AnchorTopLeft(livesText.rectTransform, new Vector2(30, -90));
            livesText.alignment = TextAlignmentOptions.TopLeft;

            // Health bar (bottom-left)
            Slider healthBar;
            Image healthFill;
            CreateHealthBar(hud.transform, out healthBar, out healthFill);

            TMP_Text waveBanner = CreateText(hud.transform, "WaveBanner", "WAVE 1", 100, Vector2.zero, new Vector2(900, 160), new Color(0.4f, 0.9f, 1f));
            waveBanner.gameObject.SetActive(false);
            TMP_Text powerUpText = CreateText(hud.transform, "PowerUpText", "", 50, new Vector2(0, -150), new Vector2(800, 80), Color.yellow);
            powerUpText.gameObject.SetActive(false);

            // ----- Pause Panel -----
            GameObject pause = CreatePanel(canvasGo.transform, "PausePanel", new Color(0f, 0f, 0f, 0.8f));
            CreateText(pause.transform, "PauseTitle", "PAUSED", 80, new Vector2(0, 180), new Vector2(800, 120), Color.white);
            Button resumeBtn = CreateButton(pause.transform, "ResumeButton", "RESUME", new Vector2(0, 30), new Vector2(360, 90));
            Button pauseMenuBtn = CreateButton(pause.transform, "MenuButton", "MAIN MENU", new Vector2(0, -90), new Vector2(360, 90));

            // ----- Game Over Panel -----
            GameObject gameOver = CreatePanel(canvasGo.transform, "GameOverPanel", new Color(0.1f, 0f, 0f, 0.9f));
            CreateText(gameOver.transform, "GameOverTitle", "GAME OVER", 90, new Vector2(0, 220), new Vector2(1000, 150), new Color(1f, 0.3f, 0.3f));
            TMP_Text finalScore = CreateText(gameOver.transform, "FinalScore", "SCORE: 0", 50, new Vector2(0, 90), new Vector2(800, 80), Color.white);
            TMP_Text goHigh = CreateText(gameOver.transform, "GOHighScore", "HIGH SCORE: 0", 40, new Vector2(0, 20), new Vector2(800, 70), Color.yellow);
            Button restartBtn = CreateButton(gameOver.transform, "RestartButton", "RESTART", new Vector2(0, -90), new Vector2(360, 90));
            Button goMenuBtn = CreateButton(gameOver.transform, "GOMenuButton", "MAIN MENU", new Vector2(0, -210), new Vector2(360, 90));

            // ----- Wire UIManager fields -----
            SetField(ui, "mainMenuPanel", mainMenu);
            SetField(ui, "hudPanel", hud);
            SetField(ui, "pausePanel", pause);
            SetField(ui, "gameOverPanel", gameOver);
            SetField(ui, "scoreText", scoreText);
            SetField(ui, "waveText", waveText);
            SetField(ui, "livesText", livesText);
            SetField(ui, "healthBar", healthBar);
            SetField(ui, "healthFill", healthFill);
            SetField(ui, "waveBannerText", waveBanner);
            SetField(ui, "powerUpText", powerUpText);
            SetField(ui, "menuHighScoreText", menuHigh);
            SetField(ui, "finalScoreText", finalScore);
            SetField(ui, "gameOverHighScoreText", goHigh);
            SetField(ui, "playerHealth", playerHealth);

            // ----- Wire button onClick -----
            AddPersistentClick(startBtn, ui, "OnStartButton");
            AddPersistentClick(quitBtn, ui, "OnQuitButton");
            AddPersistentClick(resumeBtn, ui, "OnResumeButton");
            AddPersistentClick(pauseMenuBtn, ui, "OnMainMenuButton");
            AddPersistentClick(restartBtn, ui, "OnRestartButton");
            AddPersistentClick(goMenuBtn, ui, "OnMainMenuButton");

            return ui;
        }

        private static void CreateHealthBar(Transform parent, out Slider slider, out Image fill)
        {
            GameObject sliderGo = new GameObject("HealthBar");
            sliderGo.transform.SetParent(parent, false);
            RectTransform rt = sliderGo.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 0f);
            rt.anchorMax = new Vector2(0f, 0f);
            rt.pivot = new Vector2(0f, 0f);
            rt.anchoredPosition = new Vector2(30, 30);
            rt.sizeDelta = new Vector2(400, 36);

            slider = sliderGo.AddComponent<Slider>();
            slider.minValue = 0;
            slider.maxValue = 100;
            slider.value = 100;
            slider.transition = Selectable.Transition.None;

            // Background
            GameObject bg = new GameObject("Background");
            bg.transform.SetParent(sliderGo.transform, false);
            Image bgImg = bg.AddComponent<Image>();
            bgImg.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            StretchFull(bg.GetComponent<RectTransform>());

            // Fill area
            GameObject fillArea = new GameObject("Fill Area");
            fillArea.transform.SetParent(sliderGo.transform, false);
            RectTransform fillAreaRt = fillArea.AddComponent<RectTransform>();
            StretchFull(fillAreaRt);

            GameObject fillGo = new GameObject("Fill");
            fillGo.transform.SetParent(fillArea.transform, false);
            fill = fillGo.AddComponent<Image>();
            fill.color = Color.green;
            RectTransform fillRt = fillGo.GetComponent<RectTransform>();
            StretchFull(fillRt);

            slider.fillRect = fillRt;
            slider.targetGraphic = fill;

            // Label
            TMP_Text label = CreateText(sliderGo.transform, "Label", "HEALTH", 22, Vector2.zero, new Vector2(400, 30), Color.white);
            RectTransform labelRt = label.rectTransform;
            labelRt.anchorMin = new Vector2(0f, 1f);
            labelRt.anchorMax = new Vector2(0f, 1f);
            labelRt.pivot = new Vector2(0f, 0f);
            labelRt.anchoredPosition = new Vector2(0, 4);
            label.alignment = TextAlignmentOptions.Left;
        }

        // ---------------- UI primitive helpers ----------------

        private static GameObject CreatePanel(Transform parent, string name, Color color)
        {
            GameObject panel = new GameObject(name);
            panel.transform.SetParent(parent, false);
            RectTransform rt = panel.AddComponent<RectTransform>();
            StretchFull(rt);
            Image img = panel.AddComponent<Image>();
            img.color = color;
            if (color.a == 0f) img.raycastTarget = false;
            return panel;
        }

        private static TMP_Text CreateText(Transform parent, string name, string content, float size, Vector2 pos, Vector2 sizeDelta, Color color)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            RectTransform rt = go.AddComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = sizeDelta;
            TextMeshProUGUI text = go.AddComponent<TextMeshProUGUI>();
            text.text = content;
            text.fontSize = size;
            text.color = color;
            text.alignment = TextAlignmentOptions.Center;
            text.raycastTarget = false;
            return text;
        }

        private static Button CreateButton(Transform parent, string name, string label, Vector2 pos, Vector2 size)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            RectTransform rt = go.AddComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            Image img = go.AddComponent<Image>();
            img.color = new Color(0.15f, 0.35f, 0.6f, 1f);

            Button btn = go.AddComponent<Button>();
            btn.targetGraphic = img;
            ColorBlock cb = btn.colors;
            cb.normalColor = new Color(0.15f, 0.35f, 0.6f, 1f);
            cb.highlightedColor = new Color(0.25f, 0.5f, 0.8f, 1f);
            cb.pressedColor = new Color(0.1f, 0.25f, 0.45f, 1f);
            btn.colors = cb;

            TMP_Text text = CreateText(go.transform, "Text", label, 40, Vector2.zero, size, Color.white);
            StretchFull(text.rectTransform);
            text.alignment = TextAlignmentOptions.Center;

            return btn;
        }

        private static void AddPersistentClick(Button btn, Object target, string methodName)
        {
            var method = (UnityEngine.Events.UnityAction)System.Delegate.CreateDelegate(
                typeof(UnityEngine.Events.UnityAction), target, methodName);
            UnityEditor.Events.UnityEventTools.AddPersistentListener(btn.onClick, method);
        }

        private static void StretchFull(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        private static void AnchorTopLeft(RectTransform rt, Vector2 offset)
        {
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = offset;
        }

        private static void AnchorTopRight(RectTransform rt, Vector2 offset)
        {
            rt.anchorMin = new Vector2(1f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(1f, 1f);
            rt.anchoredPosition = offset;
        }

        private static void BuildEventSystem()
        {
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<UnityEngine.EventSystems.EventSystem>();
            es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        // ---------------- Prefab / build helpers ----------------

        private static GameObject SavePrefab(GameObject go, string name)
        {
            string path = $"{PrefabFolder}/{name}.prefab";
            return PrefabUtility.SaveAsPrefabAsset(go, path);
        }

        private static void AddSceneToBuildSettings(string scenePath)
        {
            List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            bool exists = scenes.Exists(s => s.path == scenePath);
            if (!exists)
            {
                scenes.Insert(0, new EditorBuildSettingsScene(scenePath, true));
                EditorBuildSettings.scenes = scenes.ToArray();
            }
        }

        // ---------------- Windows build ----------------

        [MenuItem("Space Shooter/Build Windows Executable")]
        public static void BuildWindows()
        {
            if (!File.Exists(ScenePath))
            {
                EditorUtility.DisplayDialog("Space Shooter", "Build the game first: Space Shooter > Build Complete Game.", "OK");
                return;
            }

            string buildPath = EditorUtility.SaveFolderPanel("Choose Windows Build Output Folder", "", "Build");
            if (string.IsNullOrEmpty(buildPath)) return;

            BuildPlayerOptions options = new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                locationPathName = Path.Combine(buildPath, "SpaceShooter.exe"),
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            var report = BuildPipeline.BuildPlayer(options);
            Debug.Log($"[SpaceShooter] Build result: {report.summary.result}, output: {options.locationPathName}");
            EditorUtility.DisplayDialog("Space Shooter",
                $"Windows build finished: {report.summary.result}\n\nOutput:\n{options.locationPathName}", "OK");
        }
    }
}
#endif

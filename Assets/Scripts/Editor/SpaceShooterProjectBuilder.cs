using System.Collections.Generic;
using SpaceShooter.Enemy;
using SpaceShooter.Powerups;
using SpaceShooter.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SpaceShooter.Editor
{
    public static class SpaceShooterProjectBuilder
    {
        private const string PrefabsPath = "Assets/Prefabs";
        private const string ScenesPath = "Assets/Scenes";

        [MenuItem("Tools/Space Shooter/Generate Complete Project")]
        public static void GenerateCompleteProject()
        {
            EnsureRequiredLayers();
            ConfigurePhysicsCollisionMatrix();

            GameObject playerBullet = CreateBulletPrefab("PlayerBullet.prefab", "Assets/Sprites/Bullets/player_bullet.png");
            GameObject enemyBullet = CreateBulletPrefab("EnemyBullet.prefab", "Assets/Sprites/Bullets/enemy_bullet.png");

            GameObject player = CreatePlayerPrefab(playerBullet);
            GameObject enemyBasic = CreateEnemyPrefab("Enemy_Basic.prefab", EnemyType.Basic, "Assets/Sprites/Enemies/enemy_basic.png", enemyBullet);
            GameObject enemyFast = CreateEnemyPrefab("Enemy_Fast.prefab", EnemyType.Fast, "Assets/Sprites/Enemies/enemy_fast.png", enemyBullet);
            GameObject enemyTank = CreateEnemyPrefab("Enemy_Tank.prefab", EnemyType.Tank, "Assets/Sprites/Enemies/enemy_tank.png", enemyBullet);

            GameObject puRapid = CreatePowerUpPrefab("PowerUp_Rapid.prefab", PowerUpType.RapidFire, "Assets/Sprites/Powerups/powerup_rapid.png");
            GameObject puShield = CreatePowerUpPrefab("PowerUp_Shield.prefab", PowerUpType.Shield, "Assets/Sprites/Powerups/powerup_shield.png");
            GameObject puHealth = CreatePowerUpPrefab("PowerUp_Health.prefab", PowerUpType.HealthRestore, "Assets/Sprites/Powerups/powerup_health.png");

            GameObject gmPrefab = CreateGameManagerPrefab();
            GameObject smPrefab = CreateSoundManagerPrefab();

            CreateMainMenuScene(gmPrefab, smPrefab);
            CreateGameplayScene(player, enemyBasic, enemyFast, enemyTank, puRapid, puShield, puHealth, gmPrefab, smPrefab);
            ConfigureBuildSettings();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Space Shooter project content generated successfully.");
        }

        private static void EnsureRequiredLayers()
        {
            SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty layers = tagManager.FindProperty("layers");

            SetLayer(layers, 8, "Player");
            SetLayer(layers, 9, "Enemy");
            SetLayer(layers, 10, "PlayerBullet");
            SetLayer(layers, 11, "EnemyBullet");
            SetLayer(layers, 12, "PowerUp");

            tagManager.ApplyModifiedProperties();
        }

        private static void SetLayer(SerializedProperty layers, int index, string layerName)
        {
            if (index < 0 || index >= layers.arraySize) return;
            SerializedProperty layerProperty = layers.GetArrayElementAtIndex(index);
            if (string.IsNullOrEmpty(layerProperty.stringValue) || layerProperty.stringValue == layerName)
            {
                layerProperty.stringValue = layerName;
            }
        }

        private static void ConfigurePhysicsCollisionMatrix()
        {
            int player = LayerMask.NameToLayer("Player");
            int enemy = LayerMask.NameToLayer("Enemy");
            int playerBullet = LayerMask.NameToLayer("PlayerBullet");
            int enemyBullet = LayerMask.NameToLayer("EnemyBullet");
            int powerUp = LayerMask.NameToLayer("PowerUp");

            if (player < 0 || enemy < 0 || playerBullet < 0 || enemyBullet < 0 || powerUp < 0)
            {
                return;
            }

            Physics2D.IgnoreLayerCollision(playerBullet, player, true);
            Physics2D.IgnoreLayerCollision(playerBullet, playerBullet, true);
            Physics2D.IgnoreLayerCollision(playerBullet, powerUp, true);
            Physics2D.IgnoreLayerCollision(playerBullet, enemyBullet, true);
            Physics2D.IgnoreLayerCollision(enemyBullet, enemy, true);
            Physics2D.IgnoreLayerCollision(enemyBullet, enemyBullet, true);
            Physics2D.IgnoreLayerCollision(enemyBullet, powerUp, true);
            Physics2D.IgnoreLayerCollision(powerUp, enemy, true);
            Physics2D.IgnoreLayerCollision(powerUp, playerBullet, true);
            Physics2D.IgnoreLayerCollision(powerUp, enemyBullet, true);
        }

        private static GameObject CreateBulletPrefab(string prefabName, string spritePath)
        {
            GameObject bullet = new GameObject(prefabName.Replace(".prefab", ""));
            SpriteRenderer renderer = bullet.AddComponent<SpriteRenderer>();
            renderer.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
            BoxCollider2D collider = bullet.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            Rigidbody2D rb = bullet.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.bodyType = RigidbodyType2D.Kinematic;
            bullet.AddComponent<Combat.Bullet>();

            string path = $"{PrefabsPath}/{prefabName}";
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(bullet, path);
            Object.DestroyImmediate(bullet);
            return prefab;
        }

        private static GameObject CreatePlayerPrefab(GameObject bulletPrefab)
        {
            GameObject player = new GameObject("Player");
            SpriteRenderer renderer = player.AddComponent<SpriteRenderer>();
            renderer.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Player/player_ship.png");
            BoxCollider2D collider = player.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            Rigidbody2D rb = player.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;

            GameObject firePoint = new GameObject("FirePoint");
            firePoint.transform.SetParent(player.transform);
            firePoint.transform.localPosition = new Vector3(0f, 0.45f, 0f);

            Player.PlayerController controller = player.AddComponent<Player.PlayerController>();
            SerializedObject so = new SerializedObject(controller);
            so.FindProperty("bulletPrefab").objectReferenceValue = bulletPrefab;
            so.FindProperty("firePoint").objectReferenceValue = firePoint.transform;
            so.FindProperty("spriteRenderer").objectReferenceValue = renderer;
            so.ApplyModifiedPropertiesWithoutUndo();

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(player, $"{PrefabsPath}/Player.prefab");
            Object.DestroyImmediate(player);
            return prefab;
        }

        private static GameObject CreateEnemyPrefab(string prefabName, EnemyType type, string spritePath, GameObject enemyBulletPrefab)
        {
            GameObject enemy = new GameObject(prefabName.Replace(".prefab", ""));
            SpriteRenderer renderer = enemy.AddComponent<SpriteRenderer>();
            renderer.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
            BoxCollider2D collider = enemy.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            Rigidbody2D rb = enemy.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.bodyType = RigidbodyType2D.Kinematic;

            GameObject firePoint = new GameObject("FirePoint");
            firePoint.transform.SetParent(enemy.transform);
            firePoint.transform.localPosition = new Vector3(0f, -0.45f, 0f);

            EnemyController enemyController = enemy.AddComponent<EnemyController>();
            SerializedObject so = new SerializedObject(enemyController);
            so.FindProperty("enemyType").enumValueIndex = (int)type;
            so.FindProperty("enemyBulletPrefab").objectReferenceValue = enemyBulletPrefab;
            so.FindProperty("firePoint").objectReferenceValue = firePoint.transform;
            so.ApplyModifiedPropertiesWithoutUndo();

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(enemy, $"{PrefabsPath}/{prefabName}");
            Object.DestroyImmediate(enemy);
            return prefab;
        }

        private static GameObject CreatePowerUpPrefab(string prefabName, PowerUpType type, string spritePath)
        {
            GameObject obj = new GameObject(prefabName.Replace(".prefab", ""));
            SpriteRenderer renderer = obj.AddComponent<SpriteRenderer>();
            renderer.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
            CircleCollider2D collider = obj.AddComponent<CircleCollider2D>();
            collider.isTrigger = true;
            Rigidbody2D rb = obj.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.bodyType = RigidbodyType2D.Kinematic;
            PowerUp pu = obj.AddComponent<PowerUp>();

            SerializedObject so = new SerializedObject(pu);
            so.FindProperty("powerUpType").enumValueIndex = (int)type;
            so.ApplyModifiedPropertiesWithoutUndo();

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(obj, $"{PrefabsPath}/{prefabName}");
            Object.DestroyImmediate(obj);
            return prefab;
        }

        private static GameObject CreateGameManagerPrefab()
        {
            GameObject gm = new GameObject("GameManager");
            gm.AddComponent<Core.GameManager>();
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(gm, $"{PrefabsPath}/GameManager.prefab");
            Object.DestroyImmediate(gm);
            return prefab;
        }

        private static GameObject CreateSoundManagerPrefab()
        {
            GameObject sm = new GameObject("SoundManager");
            AudioSource source = sm.AddComponent<AudioSource>();
            source.playOnAwake = false;
            Audio.SoundManager soundManager = sm.AddComponent<Audio.SoundManager>();

            SerializedObject so = new SerializedObject(soundManager);
            so.FindProperty("shootClip").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Sounds/shoot.wav");
            so.FindProperty("explosionClip").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Sounds/explosion.wav");
            so.FindProperty("powerUpClip").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Sounds/powerup.wav");
            so.FindProperty("uiClickClip").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Sounds/ui_click.wav");
            so.ApplyModifiedPropertiesWithoutUndo();

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(sm, $"{PrefabsPath}/SoundManager.prefab");
            Object.DestroyImmediate(sm);
            return prefab;
        }

        private static void CreateMainMenuScene(GameObject gameManagerPrefab, GameObject soundManagerPrefab)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            CreateCamera();
            CreateEventSystem();

            GameObject bootstrap = new GameObject("PersistentBootstrap");
            Core.PersistentBootstrap persistentBootstrap = bootstrap.AddComponent<Core.PersistentBootstrap>();
            SerializedObject bootSO = new SerializedObject(persistentBootstrap);
            bootSO.FindProperty("gameManagerPrefab").objectReferenceValue = gameManagerPrefab;
            bootSO.FindProperty("soundManagerPrefab").objectReferenceValue = soundManagerPrefab;
            bootSO.ApplyModifiedPropertiesWithoutUndo();

            GameObject canvas = CreateCanvas("MenuCanvas");
            UIManager uiManager = canvas.AddComponent<UIManager>();
            UIButtonSfx buttonSfx = canvas.AddComponent<UIButtonSfx>();

            GameObject mainPanel = CreatePanel("MainMenuPanel", canvas.transform, new Vector2(0, 0), new Vector2(700, 500));
            CreateText("Title", mainPanel.transform, "RETRO SPACE SHOOTER", 48, new Vector2(0, 150), TextAnchor.MiddleCenter);
            CreateText("Subtitle", mainPanel.transform, "WASD/Arrows Move   Space Shoot", 24, new Vector2(0, 85), TextAnchor.MiddleCenter);

            Button start = CreateButton("StartButton", mainPanel.transform, "START GAME", new Vector2(0, 0), new Vector2(260, 70));
            start.onClick.AddListener(buttonSfx.PlayClick);
            start.onClick.AddListener(uiManager.OnStartGamePressed);

            Button quit = CreateButton("QuitButton", mainPanel.transform, "QUIT", new Vector2(0, -95), new Vector2(260, 70));
            quit.onClick.AddListener(buttonSfx.PlayClick);
            quit.onClick.AddListener(uiManager.OnQuitPressed);

            SerializedObject uiSO = new SerializedObject(uiManager);
            uiSO.FindProperty("mainMenuPanel").objectReferenceValue = mainPanel;
            uiSO.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.SaveScene(scene, $"{ScenesPath}/MainMenu.unity");
        }

        private static void CreateGameplayScene(GameObject playerPrefab, GameObject enemyBasicPrefab, GameObject enemyFastPrefab, GameObject enemyTankPrefab,
            GameObject rapidPrefab, GameObject shieldPrefab, GameObject healthPrefab, GameObject gameManagerPrefab, GameObject soundManagerPrefab)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            Camera camera = CreateCamera();
            camera.backgroundColor = new Color(0.02f, 0.02f, 0.08f, 1f);
            CreateEventSystem();

            GameObject bootstrap = new GameObject("PersistentBootstrap");
            Core.PersistentBootstrap persistentBootstrap = bootstrap.AddComponent<Core.PersistentBootstrap>();
            SerializedObject bootSO = new SerializedObject(persistentBootstrap);
            bootSO.FindProperty("gameManagerPrefab").objectReferenceValue = gameManagerPrefab;
            bootSO.FindProperty("soundManagerPrefab").objectReferenceValue = soundManagerPrefab;
            bootSO.ApplyModifiedPropertiesWithoutUndo();

            CreateBackground();

            GameObject player = PrefabUtility.InstantiatePrefab(playerPrefab) as GameObject;
            if (player != null) player.transform.position = new Vector3(0f, -3.9f, 0f);

            GameObject managers = new GameObject("GameplaySystems");
            Transform enemyParent = new GameObject("EnemyContainer").transform;
            enemyParent.SetParent(managers.transform);
            Transform powerupParent = new GameObject("PowerUpContainer").transform;
            powerupParent.SetParent(managers.transform);

            EnemySpawner spawner = managers.AddComponent<EnemySpawner>();
            SerializedObject spawnerSO = new SerializedObject(spawner);
            var enemyMaps = spawnerSO.FindProperty("enemyPrefabs");
            enemyMaps.arraySize = 3;
            enemyMaps.GetArrayElementAtIndex(0).FindPropertyRelative("type").enumValueIndex = (int)EnemyType.Basic;
            enemyMaps.GetArrayElementAtIndex(0).FindPropertyRelative("prefab").objectReferenceValue = enemyBasicPrefab;
            enemyMaps.GetArrayElementAtIndex(1).FindPropertyRelative("type").enumValueIndex = (int)EnemyType.Fast;
            enemyMaps.GetArrayElementAtIndex(1).FindPropertyRelative("prefab").objectReferenceValue = enemyFastPrefab;
            enemyMaps.GetArrayElementAtIndex(2).FindPropertyRelative("type").enumValueIndex = (int)EnemyType.Tank;
            enemyMaps.GetArrayElementAtIndex(2).FindPropertyRelative("prefab").objectReferenceValue = enemyTankPrefab;
            spawnerSO.FindProperty("enemyParent").objectReferenceValue = enemyParent;
            spawnerSO.ApplyModifiedPropertiesWithoutUndo();

            PowerUpSpawner powerUpSpawner = managers.AddComponent<PowerUpSpawner>();
            SerializedObject puSO = new SerializedObject(powerUpSpawner);
            puSO.FindProperty("powerUpParent").objectReferenceValue = powerupParent;
            var puMaps = puSO.FindProperty("powerUpPrefabs");
            puMaps.arraySize = 3;
            puMaps.GetArrayElementAtIndex(0).FindPropertyRelative("type").enumValueIndex = (int)PowerUpType.RapidFire;
            puMaps.GetArrayElementAtIndex(0).FindPropertyRelative("prefab").objectReferenceValue = rapidPrefab;
            puMaps.GetArrayElementAtIndex(1).FindPropertyRelative("type").enumValueIndex = (int)PowerUpType.Shield;
            puMaps.GetArrayElementAtIndex(1).FindPropertyRelative("prefab").objectReferenceValue = shieldPrefab;
            puMaps.GetArrayElementAtIndex(2).FindPropertyRelative("type").enumValueIndex = (int)PowerUpType.HealthRestore;
            puMaps.GetArrayElementAtIndex(2).FindPropertyRelative("prefab").objectReferenceValue = healthPrefab;
            puSO.ApplyModifiedPropertiesWithoutUndo();

            Core.GameplayDirector director = managers.AddComponent<Core.GameplayDirector>();
            SerializedObject directorSO = new SerializedObject(director);
            directorSO.FindProperty("enemySpawner").objectReferenceValue = spawner;
            directorSO.ApplyModifiedPropertiesWithoutUndo();

            CreateGameplayUI();

            EditorSceneManager.SaveScene(scene, $"{ScenesPath}/Gameplay.unity");
        }

        private static void CreateGameplayUI()
        {
            GameObject canvas = CreateCanvas("GameplayCanvas");
            UIManager uiManager = canvas.AddComponent<UIManager>();
            UIButtonSfx buttonSfx = canvas.AddComponent<UIButtonSfx>();

            GameObject hud = CreatePanel("HUD", canvas.transform, new Vector2(0f, 0f), new Vector2(0f, 0f));
            var hudRT = hud.GetComponent<RectTransform>();
            hudRT.anchorMin = Vector2.zero;
            hudRT.anchorMax = Vector2.one;
            hudRT.offsetMin = Vector2.zero;
            hudRT.offsetMax = Vector2.zero;

            Text score = CreateText("ScoreText", hud.transform, "Score: 0", 24, new Vector2(90, -24), TextAnchor.MiddleLeft, upperLeft: true);
            Text high = CreateText("HighScoreText", hud.transform, "High Score: 0", 24, new Vector2(220, -24), TextAnchor.MiddleLeft, upperLeft: true);
            Text wave = CreateText("WaveText", hud.transform, "Wave: 1 / 5", 24, new Vector2(-95, -24), TextAnchor.MiddleRight, upperRight: true);
            Slider hpSlider = CreateSlider("HealthSlider", hud.transform, new Vector2(180, 36));
            Text hpText = CreateText("HealthText", hud.transform, "HP: 100/100", 20, new Vector2(0, -48), TextAnchor.MiddleLeft, upperLeft: true);
            Text status = CreateText("StatusText", hud.transform, "", 20, new Vector2(0, -78), TextAnchor.MiddleLeft, upperLeft: true);

            GameObject pause = CreatePanel("PausePanel", canvas.transform, Vector2.zero, new Vector2(500, 330));
            pause.SetActive(false);
            CreateText("PauseTitle", pause.transform, "PAUSED", 42, new Vector2(0, 96), TextAnchor.MiddleCenter);
            Button resume = CreateButton("ResumeButton", pause.transform, "RESUME", new Vector2(0, 8), new Vector2(240, 64));
            resume.onClick.AddListener(buttonSfx.PlayClick);
            resume.onClick.AddListener(uiManager.OnResumePressed);
            Button pauseMenu = CreateButton("PauseToMenuButton", pause.transform, "MAIN MENU", new Vector2(0, -78), new Vector2(240, 64));
            pauseMenu.onClick.AddListener(buttonSfx.PlayClick);
            pauseMenu.onClick.AddListener(uiManager.OnMainMenuPressed);

            GameObject gameOver = CreatePanel("GameOverPanel", canvas.transform, Vector2.zero, new Vector2(560, 370));
            gameOver.SetActive(false);
            CreateText("GameOverTitle", gameOver.transform, "GAME OVER", 46, new Vector2(0, 122), TextAnchor.MiddleCenter);
            Text overScore = CreateText("FinalScoreText", gameOver.transform, "Score: 0", 28, new Vector2(0, 50), TextAnchor.MiddleCenter);
            Text overHigh = CreateText("FinalHighScoreText", gameOver.transform, "High Score: 0", 28, new Vector2(0, 15), TextAnchor.MiddleCenter);
            Button restart = CreateButton("RestartButton", gameOver.transform, "RESTART", new Vector2(0, -60), new Vector2(250, 64));
            restart.onClick.AddListener(buttonSfx.PlayClick);
            restart.onClick.AddListener(uiManager.OnRestartPressed);
            Button toMenu = CreateButton("GameOverMainMenuButton", gameOver.transform, "MAIN MENU", new Vector2(0, -140), new Vector2(250, 64));
            toMenu.onClick.AddListener(buttonSfx.PlayClick);
            toMenu.onClick.AddListener(uiManager.OnMainMenuPressed);

            SerializedObject uiSO = new SerializedObject(uiManager);
            uiSO.FindProperty("hudPanel").objectReferenceValue = hud;
            uiSO.FindProperty("pausePanel").objectReferenceValue = pause;
            uiSO.FindProperty("gameOverPanel").objectReferenceValue = gameOver;
            uiSO.FindProperty("scoreText").objectReferenceValue = score;
            uiSO.FindProperty("highScoreText").objectReferenceValue = high;
            uiSO.FindProperty("waveText").objectReferenceValue = wave;
            uiSO.FindProperty("healthSlider").objectReferenceValue = hpSlider;
            uiSO.FindProperty("healthText").objectReferenceValue = hpText;
            uiSO.FindProperty("statusEffectsText").objectReferenceValue = status;
            uiSO.FindProperty("gameOverScoreText").objectReferenceValue = overScore;
            uiSO.FindProperty("gameOverHighScoreText").objectReferenceValue = overHigh;
            uiSO.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void CreateBackground()
        {
            GameObject bgRoot = new GameObject("Background");
            Background.ParallaxScroller scroller = bgRoot.AddComponent<Background.ParallaxScroller>();
            var so = new SerializedObject(scroller);
            SerializedProperty layers = so.FindProperty("layers");
            layers.arraySize = 6;

            string[] spritePaths =
            {
                "Assets/Sprites/Background/bg_far.png",
                "Assets/Sprites/Background/bg_far.png",
                "Assets/Sprites/Background/bg_mid.png",
                "Assets/Sprites/Background/bg_mid.png",
                "Assets/Sprites/Background/bg_near.png",
                "Assets/Sprites/Background/bg_near.png"
            };
            float[] speeds = { 0.18f, 0.18f, 0.36f, 0.36f, 0.58f, 0.58f };
            float[] yPos = { 0f, 20f, 0f, 20f, 0f, 20f };

            for (int i = 0; i < 6; i++)
            {
                GameObject layer = new GameObject($"Layer_{i}");
                layer.transform.SetParent(bgRoot.transform);
                layer.transform.position = new Vector3(0f, yPos[i], 10f);
                SpriteRenderer renderer = layer.AddComponent<SpriteRenderer>();
                renderer.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePaths[i]);
                renderer.sortingOrder = -10 + i;

                SerializedProperty element = layers.GetArrayElementAtIndex(i);
                element.FindPropertyRelative("layerTransform").objectReferenceValue = layer.transform;
                element.FindPropertyRelative("speedMultiplier").floatValue = speeds[i];
                element.FindPropertyRelative("loopHeight").floatValue = 20f;
            }

            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static Camera CreateCamera()
        {
            GameObject camObj = new GameObject("Main Camera");
            Camera cam = camObj.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 5f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            camObj.tag = "MainCamera";
            return cam;
        }

        private static void CreateEventSystem()
        {
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }

        private static GameObject CreateCanvas(string name)
        {
            GameObject canvasGO = new GameObject(name);
            Canvas canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasGO.AddComponent<GraphicRaycaster>();
            return canvasGO;
        }

        private static GameObject CreatePanel(string name, Transform parent, Vector2 anchoredPos, Vector2 size)
        {
            GameObject panel = new GameObject(name, typeof(Image));
            panel.transform.SetParent(parent, false);
            RectTransform rt = panel.GetComponent<RectTransform>();
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;
            Image image = panel.GetComponent<Image>();
            image.color = new Color(0f, 0f, 0f, 0.5f);
            return panel;
        }

        private static Text CreateText(string name, Transform parent, string text, int fontSize, Vector2 anchoredPos, TextAnchor alignment, bool upperLeft = false, bool upperRight = false)
        {
            GameObject go = new GameObject(name, typeof(Text));
            go.transform.SetParent(parent, false);
            Text t = go.GetComponent<Text>();
            t.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            t.text = text;
            t.fontSize = fontSize;
            t.color = Color.white;
            t.alignment = alignment;

            RectTransform rt = t.rectTransform;
            rt.sizeDelta = new Vector2(440f, 40f);
            if (upperLeft)
            {
                rt.anchorMin = new Vector2(0f, 1f);
                rt.anchorMax = new Vector2(0f, 1f);
                rt.pivot = new Vector2(0f, 1f);
            }
            else if (upperRight)
            {
                rt.anchorMin = new Vector2(1f, 1f);
                rt.anchorMax = new Vector2(1f, 1f);
                rt.pivot = new Vector2(1f, 1f);
            }
            rt.anchoredPosition = anchoredPos;
            return t;
        }

        private static Button CreateButton(string name, Transform parent, string label, Vector2 anchoredPos, Vector2 size)
        {
            GameObject buttonObj = new GameObject(name, typeof(Image), typeof(Button));
            buttonObj.transform.SetParent(parent, false);
            RectTransform rt = buttonObj.GetComponent<RectTransform>();
            rt.sizeDelta = size;
            rt.anchoredPosition = anchoredPos;

            Image image = buttonObj.GetComponent<Image>();
            image.color = new Color(0.2f, 0.2f, 0.4f, 0.95f);

            Button button = buttonObj.GetComponent<Button>();
            ColorBlock block = button.colors;
            block.normalColor = image.color;
            block.highlightedColor = new Color(0.3f, 0.3f, 0.55f, 1f);
            block.pressedColor = new Color(0.15f, 0.15f, 0.28f, 1f);
            button.colors = block;

            Text buttonText = CreateText("Label", buttonObj.transform, label, 26, Vector2.zero, TextAnchor.MiddleCenter);
            buttonText.rectTransform.sizeDelta = size;
            return button;
        }

        private static Slider CreateSlider(string name, Transform parent, Vector2 anchoredPos)
        {
            GameObject root = new GameObject(name, typeof(RectTransform));
            root.transform.SetParent(parent, false);
            RectTransform rootRT = root.GetComponent<RectTransform>();
            rootRT.anchorMin = new Vector2(0f, 1f);
            rootRT.anchorMax = new Vector2(0f, 1f);
            rootRT.pivot = new Vector2(0f, 1f);
            rootRT.anchoredPosition = anchoredPos;
            rootRT.sizeDelta = new Vector2(240f, 24f);

            Slider slider = root.AddComponent<Slider>();
            slider.minValue = 0f;
            slider.maxValue = 100f;
            slider.value = 100f;

            GameObject bg = new GameObject("Background", typeof(Image));
            bg.transform.SetParent(root.transform, false);
            Image bgImage = bg.GetComponent<Image>();
            bgImage.color = new Color(0f, 0f, 0f, 0.7f);
            RectTransform bgRT = bg.GetComponent<RectTransform>();
            bgRT.anchorMin = Vector2.zero;
            bgRT.anchorMax = Vector2.one;
            bgRT.offsetMin = Vector2.zero;
            bgRT.offsetMax = Vector2.zero;

            GameObject fillArea = new GameObject("Fill Area", typeof(RectTransform));
            fillArea.transform.SetParent(root.transform, false);
            RectTransform fillAreaRT = fillArea.GetComponent<RectTransform>();
            fillAreaRT.anchorMin = new Vector2(0f, 0f);
            fillAreaRT.anchorMax = new Vector2(1f, 1f);
            fillAreaRT.offsetMin = new Vector2(5f, 5f);
            fillAreaRT.offsetMax = new Vector2(-5f, -5f);

            GameObject fill = new GameObject("Fill", typeof(Image));
            fill.transform.SetParent(fillArea.transform, false);
            Image fillImage = fill.GetComponent<Image>();
            fillImage.color = new Color(0.18f, 0.88f, 0.26f, 1f);
            RectTransform fillRT = fill.GetComponent<RectTransform>();
            fillRT.anchorMin = Vector2.zero;
            fillRT.anchorMax = Vector2.one;
            fillRT.offsetMin = Vector2.zero;
            fillRT.offsetMax = Vector2.zero;

            slider.fillRect = fillRT;
            slider.targetGraphic = fillImage;
            slider.direction = Slider.Direction.LeftToRight;
            return slider;
        }

        private static void ConfigureBuildSettings()
        {
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene("Assets/Scenes/MainMenu.unity", true),
                new EditorBuildSettingsScene("Assets/Scenes/Gameplay.unity", true)
            };
        }
    }
}

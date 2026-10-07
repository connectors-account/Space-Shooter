using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace StellarShooter.Editor
{
    // All generated data stays under Generated. Re-running replaces only this project's assets.
    public static class ProjectBuilder
    {
        public const string Root = "Assets/StellarShooter";
        public const string Generated = Root + "/Generated";
        public const string ScenePath = Generated + "/Scenes/StellarShooter.unity";
        private static readonly Color Cyan = new Color(.30f, .88f, 1f);
        private static readonly Color Pale = new Color(.81f, .88f, .96f);
        private static Font font;

        [MenuItem("Stellar Shooter/1 - Generate Project")]
        public static void GenerateMenu()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Exit Play mode before generating the project.");
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            if (File.Exists(ScenePath) && !EditorUtility.DisplayDialog("Regenerate Stellar Shooter?",
                "This replaces the generated scene and prefabs. Custom edits to generated assets will be overwritten. Source scripts, art and audio are kept.", "Regenerate", "Cancel")) return;
            Generate();
        }

        // Non-interactive batch entry point: -executeMethod StellarShooter.Editor.ProjectBuilder.Generate
        public static void Generate()
        {
            Debug.Log("Stellar Shooter: generating serialized prefabs and scene.");
            Directory.CreateDirectory(Generated + "/Prefabs");
            Directory.CreateDirectory(Generated + "/Scenes");
            AssetDatabase.Refresh();
            ConfigureSettings();
            ImportArtwork();
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null) throw new InvalidOperationException("Unity built-in LegacyRuntime.ttf font is unavailable.");

            Projectile playerShot = MakeProjectile("PlayerShot", "player-shot", 10);
            Projectile enemyShot = MakeProjectile("EnemyShot", "enemy-shot", 11);
            PlayerController player = MakePlayer(playerShot);
            Enemy[] enemies = { MakeEnemy("Scout", "scout", 0, enemyShot), MakeEnemy("Gunner", "gunner", 1, enemyShot), MakeEnemy("Bomber", "bomber", 2, enemyShot) };
            Pickup[] pickups = { MakePickup("Spread", "spread", PickupKind.Spread), MakePickup("Repair", "repair", PickupKind.Repair), MakePickup("Shield", "shield", PickupKind.Shield) };
            ImpactEffect impact = MakeImpact();

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var cameraObject = new GameObject("Game Camera", typeof(Camera), typeof(AudioListener), typeof(FixedAspectCamera));
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(0, 0, -10);
            Camera camera = cameraObject.GetComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = GameRules.HalfHeight;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(.008f, .015f, .04f);
            camera.nearClipPlane = .3f;
            camera.farClipPlane = 100;
            MakeBackground("Distant Nebula", "space", .16f, -30, Color.white);
            MakeBackground("Near Stars", "stars", .53f, -29, new Color(.65f, .85f, 1, .65f));

            var sessionObject = new GameObject("Game Session");
            GameSession game = sessionObject.AddComponent<GameSession>();
            game.actors = new GameObject("Runtime Actors").transform;
            game.playerPrefab = player;
            game.impactPrefab = impact;
            game.pickupPrefabs = pickups;
            game.waves = sessionObject.AddComponent<WaveDirector>();
            game.waves.enemyPrefabs = enemies;
            game.sound = sessionObject.AddComponent<SoundBank>();
            AudioSource source = sessionObject.GetComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 0;
            source.volume = .8f;
            string[] sounds = { "shoot", "enemy-fire", "hit", "explosion", "pickup", "wave", "game-over", "click" };
            game.sound.clips = sounds.Select(name => Load<AudioClip>(Root + "/Audio/" + name + ".wav")).ToArray();
            new GameObject("Event System", typeof(EventSystem), typeof(StandaloneInputModule));
            game.ui = MakeUI(camera, game);

            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene, ScenePath)) throw new IOException("Could not save generated scene.");
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            AssetDatabase.SaveAssets();
            ValidateLoadedScene();
            Selection.activeGameObject = sessionObject;
            Debug.Log("Stellar Shooter: generation and serialized-reference validation complete. Press Play.");
        }

        private static void ConfigureSettings()
        {
            var settings = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset")[0]);
            SerializedProperty input = settings.FindProperty("activeInputHandler");
            if (input == null) throw new InvalidOperationException("Expected Unity 6 activeInputHandler setting was not found.");
            input.intValue = 0; // Input Manager (Old), used by Input.GetKey and StandaloneInputModule.
            settings.ApplyModifiedPropertiesWithoutUndo();
            var tags = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            string[] layers = { "Player", "Enemy", "PlayerShot", "EnemyShot", "Pickup" };
            for (int i = 0; i < layers.Length; i++) tags.FindProperty("layers").GetArrayElementAtIndex(8+i).stringValue = layers[i];
            tags.ApplyModifiedPropertiesWithoutUndo();
            PlayerSettings.companyName = "Independent Arcade";
            PlayerSettings.productName = "Stellar Shooter";
            PlayerSettings.bundleVersion = "1.0.0";
            PlayerSettings.defaultScreenWidth = 1280;
            PlayerSettings.defaultScreenHeight = 720;
            PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
            PlayerSettings.resizableWindow = true;
            PlayerSettings.runInBackground = false;
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Standalone, ScriptingImplementation.Mono2x);
            ConfigureWindowsTarget();
            PlayerSettings.colorSpace = ColorSpace.Gamma;
            QualitySettings.vSyncCount = 1;
            Time.fixedDeltaTime = .02f;
        }

        private static void ConfigureWindowsTarget()
        {
            EditorUserBuildSettings.selectedStandaloneTarget = BuildTarget.StandaloneWindows64;
            EditorUserBuildSettings.standaloneBuildSubtarget = StandaloneBuildSubtarget.Player;
            // Unity 6 uses a platform setting for Windows architecture, not PlayerSettings.SetArchitecture.
            EditorUserBuildSettings.SetPlatformSettings(BuildPipeline.GetBuildTargetName(BuildTarget.StandaloneWindows64), "Architecture", "x64");
        }

        private static void ImportArtwork()
        {
            foreach (string path in Directory.GetFiles(Root + "/Art", "*.png"))
            {
                var importer = (TextureImporter)AssetImporter.GetAtPath(path.Replace('\\', '/'));
                if (importer == null) throw new InvalidOperationException("Missing texture importer: " + path);
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spritePixelsPerUnit = 256;
                var spriteSettings = new TextureImporterSettings();
                importer.ReadTextureSettings(spriteSettings);
                spriteSettings.spriteMeshType = SpriteMeshType.FullRect;
                importer.SetTextureSettings(spriteSettings);
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false;
                importer.filterMode = FilterMode.Bilinear;
                importer.wrapMode = TextureWrapMode.Clamp;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.maxTextureSize = 1024;
                importer.SaveAndReimport();
            }
        }

        private static T Load<T>(string path) where T : UnityEngine.Object
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset == null) throw new InvalidOperationException("Required asset is missing or not imported: " + path);
            return asset;
        }
        private static Sprite Sprite(string name) => Load<Sprite>(Root + "/Art/" + name + ".png");
        private static GameObject Visual(string name, string art, int order)
        {
            var go = new GameObject(name, typeof(SpriteRenderer));
            SpriteRenderer renderer = go.GetComponent<SpriteRenderer>();
            renderer.sprite = Sprite(art);
            renderer.sortingOrder = order;
            return go;
        }
        private static void PhysicsBody(GameObject go, int layer, Vector2 size)
        {
            go.layer = layer;
            var body = go.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Dynamic;
            body.gravityScale = 0;
            body.freezeRotation = true;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            body.sleepMode = RigidbodySleepMode2D.NeverSleep;
            BoxCollider2D collider = go.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            collider.size = size;
        }
        private static T SavePrefab<T>(GameObject go) where T : Component
        {
            string path = Generated + "/Prefabs/" + go.name + ".prefab";
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            UnityEngine.Object.DestroyImmediate(go);
            if (prefab == null) throw new IOException("Prefab save failed: " + path);
            return prefab.GetComponent<T>();
        }
        private static Projectile MakeProjectile(string name, string art, int layer)
        {
            GameObject go = Visual(name, art, 5);
            go.transform.localScale = Vector3.one * .7f;
            PhysicsBody(go, layer, new Vector2(.15f, .68f));
            go.AddComponent<Projectile>();
            return SavePrefab<Projectile>(go);
        }
        private static PlayerController MakePlayer(Projectile shot)
        {
            GameObject go = Visual("Player", "player", 10);
            go.transform.localScale = Vector3.one * 1.15f;
            PhysicsBody(go, 8, new Vector2(.50f, .61f));
            PlayerController player = go.AddComponent<PlayerController>();
            player.projectilePrefab = shot;
            GameObject shield = Visual("Shield Glow", "shield", 11);
            shield.transform.SetParent(go.transform, false);
            shield.transform.localScale = Vector3.one * 1.5f;
            player.shieldVisual = shield.GetComponent<SpriteRenderer>();
            player.shieldVisual.color = new Color(.3f, .8f, 1, .28f);
            player.shieldVisual.enabled = false;
            return SavePrefab<PlayerController>(go);
        }
        private static Enemy MakeEnemy(string name, string art, int kind, Projectile shot)
        {
            GameObject go = Visual(name, art, 8);
            go.transform.localScale = Vector3.one * (kind == 2 ? 1.35f : 1.1f);
            PhysicsBody(go, 9, new Vector2(.66f, .66f));
            Enemy enemy = go.AddComponent<Enemy>();
            enemy.kind = kind;
            enemy.projectilePrefab = shot;
            return SavePrefab<Enemy>(go);
        }
        private static Pickup MakePickup(string name, string art, PickupKind kind)
        {
            GameObject go = Visual(name, art, 9);
            go.transform.localScale = Vector3.one * .68f;
            PhysicsBody(go, 12, new Vector2(1, 1));
            go.AddComponent<Pickup>().kind = kind;
            return SavePrefab<Pickup>(go);
        }
        private static ImpactEffect MakeImpact()
        {
            var go = new GameObject("Impact");
            ImpactEffect effect = go.AddComponent<ImpactEffect>();
            effect.sparks = new SpriteRenderer[8];
            for (int i = 0; i < effect.sparks.Length; i++)
            {
                GameObject spark = Visual("Spark " + i, "player-shot", 15);
                spark.transform.SetParent(go.transform, false);
                spark.transform.localRotation = Quaternion.Euler(0, 0, i*45);
                spark.transform.localScale = Vector3.one * .15f;
                effect.sparks[i] = spark.GetComponent<SpriteRenderer>();
            }
            return SavePrefab<ImpactEffect>(go);
        }
        private static void MakeBackground(string name, string art, float speed, int order, Color tint)
        {
            var go = new GameObject(name);
            ParallaxLayer layer = go.AddComponent<ParallaxLayer>();
            layer.speed = speed;
            layer.tileHeight = 18;
            Transform[] tiles = new Transform[2];
            for (int i = 0; i < 2; i++)
            {
                GameObject tile = Visual("Tile " + i, art, order);
                tile.transform.SetParent(go.transform, false);
                tile.transform.localPosition = new Vector3(0, i * 18, 0);
                tile.transform.localScale = Vector3.one * 4.5f; // 1024 px / 256 PPU * 4.5 = 18 m.
                tile.GetComponent<SpriteRenderer>().color = tint;
                tiles[i] = tile.transform;
            }
            layer.first = tiles[0];
            layer.second = tiles[1];
        }

        private static GameUI MakeUI(Camera camera, GameSession game)
        {
            var canvasObject = new GameObject("Interface", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            // Sprite sorting orders reach 15. Keep menus/HUD above every world sprite.
            canvas.sortingOrder = 100;
            canvas.worldCamera = camera;
            canvas.planeDistance = 5;
            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 720);
            scaler.matchWidthOrHeight = .5f;
            GameUI ui = canvasObject.AddComponent<GameUI>();
            ui.game = game;

            ui.hud = Panel(canvasObject.transform, "HUD", Color.clear);
            ui.stats = Label(ui.hud.transform, "Stats", "", 25, new Vector2(0, 313), new Vector2(1170, 48), Cyan);
            ui.powers = Label(ui.hud.transform, "Power Timers", "", 17, new Vector2(0, -325), new Vector2(1180, 32), Pale);
            ui.announcement = Label(ui.hud.transform, "Wave Announcement", "", 42, new Vector2(0, 205), new Vector2(1000, 70), Cyan);

            ui.mainPanel = Panel(canvasObject.transform, "Main Menu", new Color(.015f, .03f, .075f, .85f));
            Label(ui.mainPanel.transform, "Eyebrow", "INDEPENDENT ARCADE  /  01", 17, new Vector2(0, 265), new Vector2(900, 36), Cyan);
            Label(ui.mainPanel.transform, "Title", "STELLAR SHOOTER", 62, new Vector2(0, 184), new Vector2(1180, 100), Color.white);
            Label(ui.mainPanel.transform, "Subtitle", "Hold the line. Break the wave. Beat your best.", 23, new Vector2(0, 111), new Vector2(1100, 50), Pale);
            ui.best = Label(ui.mainPanel.transform, "Best Score", "", 20, new Vector2(0, 51), new Vector2(900, 38), Cyan);
            ui.play = Button(ui.mainPanel.transform, "START FLIGHT", new Vector2(0, -31));
            ui.mainQuit = Button(ui.mainPanel.transform, "QUIT", new Vector2(0, -99));
            Label(ui.mainPanel.transform, "Controls", "WASD / ARROWS  Move       SPACE  Fire       ESC / P  Pause\nArrow keys + ENTER select menus. Mouse also supported.", 19, new Vector2(0, -198), new Vector2(1150, 65), Pale);
            Label(ui.mainPanel.transform, "Briefing", "Clear endless waves. Escaping enemies cost hull.\nEvery fourth kill drops a power-up: triple shot, repair, then shield.", 18, new Vector2(0, -276), new Vector2(1120, 65), Pale);

            ui.pausePanel = Panel(canvasObject.transform, "Pause Menu", new Color(.015f, .025f, .06f, .95f));
            Label(ui.pausePanel.transform, "Title", "FLIGHT PAUSED", 49, new Vector2(0, 230), new Vector2(1100, 90), Color.white);
            Label(ui.pausePanel.transform, "Hint", "Take a breath. The sector can wait.", 22, new Vector2(0, 157), new Vector2(1000, 45), Pale);
            ui.resume = Button(ui.pausePanel.transform, "RESUME", new Vector2(0, 55));
            ui.pauseRestart = Button(ui.pausePanel.transform, "RESTART", new Vector2(0, -13));
            ui.pauseMenu = Button(ui.pausePanel.transform, "MAIN MENU", new Vector2(0, -81));
            ui.pauseQuit = Button(ui.pausePanel.transform, "QUIT", new Vector2(0, -149));

            ui.overPanel = Panel(canvasObject.transform, "Game Over", new Color(.03f, .02f, .065f, .95f));
            Label(ui.overPanel.transform, "Title", "SIGNAL LOST", 56, new Vector2(0, 223), new Vector2(1100, 95), new Color(1, .48f, .40f));
            ui.result = Label(ui.overPanel.transform, "Results", "", 27, new Vector2(0, 118), new Vector2(1100, 95), Pale);
            ui.retry = Button(ui.overPanel.transform, "FLY AGAIN", new Vector2(0, 0));
            ui.overMenu = Button(ui.overPanel.transform, "MAIN MENU", new Vector2(0, -68));
            ui.overQuit = Button(ui.overPanel.transform, "QUIT", new Vector2(0, -136));
            Label(ui.overPanel.transform, "Restart Hint", "Press R to restart. Each flight begins with a fresh hull.", 19, new Vector2(0, -228), new Vector2(1120, 50), Pale);

            ui.mute = Button(canvasObject.transform, "M  /  SOUND ON", new Vector2(488, 270), new Vector2(220, 36));
            ui.soundLabel = ui.mute.GetComponentInChildren<Text>();
            ui.soundLabel.fontSize = 16;
            // Keep global mute out of the menu's arrow-key navigation chain.
            Navigation navigation = ui.mute.navigation;
            navigation.mode = Navigation.Mode.None;
            ui.mute.navigation = navigation;
            ui.mainPanel.SetActive(true);
            ui.pausePanel.SetActive(false);
            ui.overPanel.SetActive(false);
            ui.hud.SetActive(false);
            return ui;
        }

        private static GameObject Panel(Transform parent, string name, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            RectTransform rect = (RectTransform)go.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            Image image = go.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = color.a > 0;
            return go;
        }
        private static RectTransform Rect(Transform parent, string name, Vector2 position, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            RectTransform rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(.5f, .5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = position;
            return rect;
        }
        private static Text Label(Transform parent, string name, string value, int size, Vector2 position, Vector2 dimensions, Color color)
        {
            RectTransform rect = Rect(parent, name, position, dimensions);
            Text text = rect.gameObject.AddComponent<Text>();
            text.font = font;
            text.text = value;
            text.fontSize = size;
            text.fontStyle = FontStyle.Bold;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = color;
            text.raycastTarget = false;
            Shadow shadow = rect.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0, 0, 0, .8f);
            shadow.effectDistance = new Vector2(1, -2);
            return text;
        }
        private static Button Button(Transform parent, string title, Vector2 position, Vector2? size = null)
        {
            Vector2 dimensions = size ?? new Vector2(320, 54);
            RectTransform rect = Rect(parent, title, position, dimensions);
            Image image = rect.gameObject.AddComponent<Image>();
            image.color = new Color(.08f, .20f, .29f, 1);
            Button button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(.65f, .93f, 1);
            colors.selectedColor = new Color(.60f, .88f, 1);
            colors.pressedColor = new Color(.3f, .65f, .85f);
            button.colors = colors;
            Label(rect, "Label", title, 21, Vector2.zero, dimensions, Color.white);
            return button;
        }

        [MenuItem("Stellar Shooter/2 - Validate Generated Project")]
        public static void ValidateProject()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Exit Play mode before validating the generated scene.");
            if (!File.Exists(ScenePath)) throw new InvalidOperationException("Generate the project first.");
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            ValidateLoadedScene();
        }
        private static void Require(bool valid, string description)
        {
            if (!valid) throw new InvalidOperationException("Project validation failed: " + description);
        }
        private static void ValidateLoadedScene()
        {
            GameSession game = UnityEngine.Object.FindFirstObjectByType<GameSession>();
            Require(game != null, "GameSession");
            Require(game.playerPrefab != null && game.playerPrefab.projectilePrefab != null && game.playerPrefab.shieldVisual != null, "player references");
            Require(game.actors != null && game.ui != null && game.sound != null && game.waves != null, "scene wiring");
            Require(game.impactPrefab != null && game.impactPrefab.sparks.Length == 8 && game.impactPrefab.sparks.All(s => s != null), "impact references");
            Require(game.waves.enemyPrefabs.Length == 3 && game.waves.enemyPrefabs.All(e => e != null && e.projectilePrefab != null), "enemy prefabs");
            Require(game.pickupPrefabs.Length == 3 && game.pickupPrefabs.All(p => p != null), "pickup prefabs");
            Require(game.sound.clips.Length == 8 && game.sound.clips.All(c => c != null && c.length > 0), "audio clips");
            Require(UnityEngine.Object.FindFirstObjectByType<EventSystem>() != null, "EventSystem");
            Canvas canvas = game.ui.GetComponent<Canvas>();
            Require(canvas != null && canvas.worldCamera != null && canvas.sortingOrder == 100, "UI camera and foreground sorting");
            // Catch missing scripts and unassigned serialized object fields, including inactive menus.
            foreach (GameObject root in SceneManager.GetActiveScene().GetRootGameObjects())
                foreach (Transform node in root.GetComponentsInChildren<Transform>(true))
                {
                    Require(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(node.gameObject) == 0, "missing script on " + node.name);
                    foreach (MonoBehaviour component in node.GetComponents<MonoBehaviour>())
                    {
                        if (component == null || component.GetType().Namespace != "StellarShooter") continue;
                        SerializedProperty property = new SerializedObject(component).GetIterator();
                        while (property.NextVisible(true))
                            if (property.propertyType == SerializedPropertyType.ObjectReference && property.name != "m_Script")
                                Require(property.objectReferenceValue != null, component.name + "." + property.propertyPath);
                    }
                }
            foreach (string path in AssetDatabase.FindAssets("t:Prefab", new[] { Generated }))
            {
                GameObject prefab = Load<GameObject>(AssetDatabase.GUIDToAssetPath(path));
                Require(prefab != null, "prefab load");
                foreach (Transform node in prefab.GetComponentsInChildren<Transform>(true))
                    Require(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(node.gameObject) == 0, "missing prefab script on " + prefab.name + "/" + node.name);
                if (prefab.GetComponent<ImpactEffect>() == null)
                {
                    Rigidbody2D body = prefab.GetComponent<Rigidbody2D>();
                    Collider2D collider = prefab.GetComponent<Collider2D>();
                    Require(body != null && body.gravityScale == 0 && body.bodyType == RigidbodyType2D.Dynamic, "zero-gravity body on " + prefab.name);
                    Require(collider != null && collider.isTrigger, "trigger collider on " + prefab.name);
                    int layer = prefab.GetComponent<PlayerController>() != null ? 8
                        : prefab.GetComponent<Enemy>() != null ? 9
                        : prefab.GetComponent<Pickup>() != null ? 12
                        : prefab.name == "PlayerShot" ? 10 : 11;
                    Require(prefab.layer == layer, "collision layer on " + prefab.name);
                }
                foreach (SpriteRenderer renderer in prefab.GetComponentsInChildren<SpriteRenderer>(true))
                    Require(renderer.sprite != null, "sprite on " + prefab.name);
            }
            Require(EditorBuildSettings.scenes.Any(s => s.enabled && s.path == ScenePath), "scene in build list");
            Debug.Log("Stellar Shooter validation passed: required scene, prefab, UI and audio references are assigned.");
        }

        [MenuItem("Stellar Shooter/3 - Build Windows x64")]
        public static void BuildWindows()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Exit Play mode before building.");
            if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64))
                throw new BuildFailedException("Windows build support is missing. Install the Windows target module through Unity Hub for this editor.");
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            if (!File.Exists(ScenePath)) Generate();
            else { EditorSceneManager.OpenScene(ScenePath); ValidateLoadedScene(); }
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Standalone, ScriptingImplementation.Mono2x);
            ConfigureWindowsTarget();
            Directory.CreateDirectory("Builds/Windows");
            BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                locationPathName = "Builds/Windows/StellarShooter.exe",
                target = BuildTarget.StandaloneWindows64,
                subtarget = (int)StandaloneBuildSubtarget.Player,
                options = BuildOptions.None
            });
            if (report == null) throw new BuildFailedException("Unity returned no build report. Read the first Console error.");
            if (report.summary.result != BuildResult.Succeeded)
                throw new BuildFailedException("Windows build failed: " + report.summary.result + ". Read the first Console error.");
            Debug.Log("Windows x64 build succeeded. Distribute the entire Builds/Windows folder, not only the executable.");
        }
    }
}
